import com.saxonica.build.cpp.*
import com.saxonica.build.java.ManifestComparer
import com.saxonica.build.java.PreprocessSourcesTask
import org.gradle.api.*
import org.gradle.api.tasks.*
import org.gradle.api.tasks.bundling.Jar
import org.gradle.kotlin.dsl.*

plugins {
    id("java-library")
}

val extension = project.extensions.create<SaxonCCoreExtension>("saxonc")

repositories {
    mavenLocal()
    mavenCentral()
    maven { url = uri("https://maven.saxonica.com/maven") }
}

val saxonBuild = the<SaxonBuildExtension>()

val graalRelease: String by project
extension.project.set(project)
extension.graal {
    release.convention(graalRelease)
    val homeDir = GraalHomeFinder(release)
    home.convention(homeDir.home())
    nativeImage.convention(homeDir.nativeImage())
}

val javaDependencies by configurations.dependencyScope("javaDependencies")
val javaSourcesBucket by configurations.dependencyScope("javaSourcesBucket")
val javaRuntime by configurations.dependencyScope("javaRuntime")
val cMakePackageDir by configurations.consumable("cMakePackageDir")
val debugCMakePackageDir by configurations.consumable("debugCMakePackageDir")

val filteredSourcesBucket by configurations.dependencyScope("filteredSourcesBucket")

val filteredSourcesDir by configurations.resolvable("filteredSourcesDir") {
    extendsFrom(filteredSourcesBucket)
}
val javaSources by configurations.resolvable("javaSources") {
    extendsFrom(javaSourcesBucket)
}
val preprocessor by configurations.creating {
    extendsFrom(configurations["runtimeOnly"])
}
val javaResources by configurations.resolvable("javaResources")

fun javaEditionProject(edition: String): String {
    return if (edition == "ee") ":java" else ":java:${edition}"
}

dependencies {
    javaRuntime(extension.edition.map { edition ->
        project(path = javaEditionProject(edition), configuration = "runtimeElements")
    })
    filteredSourcesBucket(extension.edition.map { edition ->
        project(path = javaEditionProject(edition), configuration = "filteredSourcesDir")
    })
    javaSources(extension.edition.map { edition ->
        project(path = javaEditionProject(edition), configuration = "unprocessedJavaSources")
    })
    javaResources(extension.edition.map { edition ->
        project(path = javaEditionProject(edition), configuration = "resources")
    })

    javaDependencies

    preprocessor(extension.edition.map { edition ->
        project(path = javaEditionProject(edition), configuration = "preprocessor")
    })
    compileOnly(extension.edition.map { edition ->
        project(path = javaEditionProject(edition), configuration = "compileOnlyDependencies")
    })
    implementation(extension.edition.map { edition ->
        project(path = javaEditionProject(edition), configuration = "implementationDependencies")
    })

    compileOnly("org.graalvm.sdk:graal-sdk:${findProperty("graalSdkVersion")}")
    compileOnly("org.graalvm.nativeimage:svm:${findProperty("graalSvmVersion")}")
    compileOnly("com.ibm.icu:icu4j:${project.findProperty("icu4jVersion")}")
}

java {
    sourceCompatibility = JavaVersion.VERSION_11
    targetCompatibility = JavaVersion.VERSION_11
}

val saxonCFilteredSources = tasks.register<Sync>("filteredSources") {
    into(layout.buildDirectory.dir("tmp/filtered-sources"))
    duplicatesStrategy = DuplicatesStrategy.WARN
}

val bundledFilteredSources = tasks.register<Sync>("bundleFilteredSources") {
    into(layout.buildDirectory.dir("tmp/bundled-filtered-sources"))
    from(filteredSourcesDir)
    from(saxonCFilteredSources)
    duplicatesStrategy = DuplicatesStrategy.WARN
}

val preprocessJava = tasks.register<PreprocessSourcesTask>("preprocessJava") {
    dependsOn(bundledFilteredSources)
    classpath.from(configurations.named("preprocessor"))
    sources = bundledFilteredSources.map { it.destinationDir }

    opts {
        off("EE", "PE", "STREAM", "SCHEMA", "OPT", "BYTECODE", "CSHARP")
        on("SAXONC")
    }

    output = layout.buildDirectory.dir("src/main/java")
}

sourceSets {
    main {
        java {
            srcDir(preprocessJava.map { it.output })
        }
        resources {
            srcDir(javaResources)
        }
    }
}

val bundleDependencies = tasks.register<UnjarBundleDependencies>("bundleJarDependencies") {
    classpath = extension.classpath
    outputDir = project.layout.buildDirectory.dir("unjar")

    bundle {
        jar(project.layout.projectDirectory.file("../lib/xercesImpl.jar")) {
            include("**/*.class")
            include("**/*.properties")
            exclude("org.w3c.dom.html.HTMLDOMImplementation.class")
            exclude("META-INF/MANIFEST.MF")
        }
        jar("icu4j-${project.findProperty("icu4jVersion")}.jar")
        jar("jline-${project.findProperty("jlineVersion")}.jar")
        jar("xmlresolver-${project.findProperty("xmlresolverVersion")}.jar")
        jar("xmlresolver-${project.findProperty("xmlresolverVersion")}-data.jar")
    }
}

val testJarManifest = tasks.register<ManifestComparer>("testJarManifest") {
    jar = tasks.jar.map { it.archiveFile.get() }
    goldJarManifest = extension.edition.map { edition -> layout.projectDirectory.file("../tools/java-manifests/${edition}.list") }
    releaseBuild = saxonBuild.releaseBuild
    expectSignedJar = false
    newFilesLog = layout.buildDirectory.file("new-files.log")
    missingFilesLog = layout.buildDirectory.file("missing-files.log")
}

tasks.jar {
    description = "Create the JAR used as input to native-image"
    finalizedBy(testJarManifest)
    archiveBaseName = extension.edition.map { edition ->
        val ver = findProperty("saxonVersion")
        "saxon-${edition}c-${ver}"
    }

    doFirst {
        // Make the META-INF/services for HE
        val extraDir = layout.buildDirectory.dir("jar-extra/META-INF/services").get()
        mkdir(extraDir)
        extraDir.file("javax.xml.transform.TransformerFactory").asFile.writeText("net.sf.saxon.TransformerFactoryImpl")
    }

    from(layout.buildDirectory.dir("jar-extra")) {
        include("META-INF/ **")
    }

    from(bundleDependencies.map { it.outputDir }) {
        include("**/*")
    }

    from(layout.projectDirectory.dir("../src/graalvm")) {
        include("META-INF/**")
    }

    manifest {
        attributes("Project-Name" to "Saxon-EE",
            "Main-Class" to "net.sf.saxon.Transform")
    }
}
val jar = project.tasks.named<Jar>("jar")

val graalImage = tasks.register<GraalNativeImageTask>("release_graal_image") {
    group = LifecycleBasePlugin.BUILD_GROUP
    description = "Build the GraalVM native-image shared library"
    sourceJar = jar.map { it.archiveFile.get() }
    outputDir = project.layout.buildDirectory.dir("c")
    graalNativeImage = extension.graal()!!.nativeImage
    libName = extension.edition.map { edition -> "saxonc-core-${edition}" }
    version = saxonBuild.version
    majorVersion = saxonBuild.majorVersion
}

val debugGraalImage = tasks.register<GraalNativeImageTask>("debug_graal_image") {
    group = LifecycleBasePlugin.BUILD_GROUP
    description = "Build a debug-enabled GraalVM native-image shared library"
    sourceJar = jar.map { it.archiveFile.get() }
    outputDir = project.layout.buildDirectory.dir("c")
    graalNativeImage = extension.graal()!!.nativeImage
    libName = extension.edition.map { edition -> "saxonc-core-${edition}" }
    version = saxonBuild.version
    majorVersion = saxonBuild.majorVersion
    debug = true
}
val ersatzPath = layout.buildDirectory.dir("ersatz")
val generator = tasks.register("cmake_generate") {
    group = LifecycleBasePlugin.BUILD_GROUP
    description = "Generate build system for the ersatz CMake-driven library"
    inputs.file(layout.projectDirectory.file("CMakeLists.txt"))
    inputs.files(layout.projectDirectory.dir("../cmake"))
    outputs.file(ersatzPath.map { it.file("CMakeCache.txt") })

    doLast {
        val generatorName = if (saxonBuild.platform.get() == "windows") "Visual Studio 17 2022" else "Ninja Multi-Config"
        project.exec {
            workingDir = layout.projectDirectory.asFile
            executable = "cmake"
            args("-G", generatorName, "-B", ersatzPath.get().asFile.toString(),
                "-D", saxonBuild.version.map { "SAXON_VERSION:STRING=${it}" }.get()
            )
        }
    }
}

fun setupConfigTasks(config: String, imageTask: TaskProvider<GraalNativeImageTask>, taskBlock: Action<CMakePackageBuilder>): TaskProvider<CMakePackageBuilder> {
    val config_task_name = config.lowercase()

    val configDir = layout.buildDirectory.dir(config)

    val build = tasks.register<Exec>("build_ersatz_${config_task_name}") {
        inputs.files(generator)
        inputs.files(layout.projectDirectory.dir("../cmake/ersatz"))
        outputs.dir(layout.buildDirectory.dir(config))

        workingDir = layout.projectDirectory.asFile
        executable = "cmake"
        args("--build", ersatzPath.get().asFile.toString(), "--config", config)
    }

    val ersatzConfigPath = ersatzPath.map { it.dir("staging/${config}") }
    val generateCMakeConfigPackageFiles = tasks.register<Exec>("install_ersatz_${config_task_name}") {
        inputs.files(build)
        outputs.dir(ersatzConfigPath.map { it.dir("lib/cmake") })

        workingDir = layout.projectDirectory.asFile
        executable = "cmake"
        args("--install", ersatzPath.get().asFile.toString(), "--config", "Release", "--prefix", ersatzConfigPath.get().asFile.toString())
    }

    val mainTask = tasks.register<CMakePackageBuilder>("${config_task_name}_cmake_package") {
        group = LifecycleBasePlugin.BUILD_GROUP
        libName = extension.edition.map { edition -> "saxonc-core-${edition}" }
        version =  saxonBuild.version
        majorVersion = saxonBuild.majorVersion
        sharedLibFiles.from(imageTask.map { it.sharedLibFiles })
        importLibFiles.from(imageTask.map { it.importLibFiles })
        headerFiles.from(imageTask.map { it.headerFiles })
        packageCMakeFiles.from(generateCMakeConfigPackageFiles)
        packageDir = configDir
    }
    mainTask.configure(taskBlock)
    return mainTask
}

val releaseCMakePackage = setupConfigTasks("Release", graalImage) {
    description = "Build and package up the native-image and CMake package config files"
}
val debugCMakePackage = setupConfigTasks("Debug", debugGraalImage) {
    description = "Build and package up the debug-enabled version of the native-image and CMake package config files"
}

tasks.named(LifecycleBasePlugin.ASSEMBLE_TASK_NAME) {
    dependsOn(releaseCMakePackage)
}

cMakePackageDir.outgoing.artifact(releaseCMakePackage.map { it.outputDir }) { builtBy(releaseCMakePackage) }
debugCMakePackageDir.outgoing.artifact(debugCMakePackage.map { it.outputDir }) { builtBy(debugCMakePackage) }
