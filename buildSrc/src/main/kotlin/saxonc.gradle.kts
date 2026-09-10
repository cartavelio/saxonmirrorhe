import com.saxonica.build.cpp.*
import com.saxonica.build.java.ManifestComparer
import com.saxonica.build.java.PreprocessSourcesTask
import org.gradle.api.*
import org.gradle.api.tasks.*
import org.gradle.api.tasks.bundling.Jar
import org.gradle.kotlin.dsl.*

plugins {
    id("java-library")
    id("saxon-build")
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
        jar("vavr-champ-${findProperty("vavrVersion")}.jar")
        jar("vavr-match-${findProperty("vavrVersion")}.jar")
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
    macosx_min_version = saxonBuild.macosx_min_version
    specifiedArgs = saxonBuild.targetHost.map {
        if (it.isWindows && it.isX86_64) listOf("-march=x86-64-1") else listOf()
    }
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
    macosx_min_version = saxonBuild.macosx_min_version
    specifiedArgs = saxonBuild.targetHost.map {
        if (it.isWindows && it.isX86_64) listOf("-march=x86-64-1") else listOf()
    }
    debug = true
}
val ersatzPath = layout.buildDirectory.dir("ersatz")
val EDITION = extension.edition.map { it.uppercase() }
fun cmakeFilenameMapper(suffixes: List<String>): (String) -> List<String> {
    return { ED ->
        suffixes.map { suffix -> "SaxonCCore${ED}${suffix}.cmake" }
    }
}
val universalCMakeFileSuffixes = listOf("Config", "ConfigVersion")
val universalCMakeFilenames = EDITION.map(cmakeFilenameMapper(universalCMakeFileSuffixes))

val generator = tasks.register<ErsatzCMakeGenerator>("cmake_generate") {
    group = LifecycleBasePlugin.BUILD_GROUP
    description = "Generate build system for the ersatz CMake-driven library"

    version = saxonBuild.version
    platformIsWindows = saxonBuild.targetHost.map { it.isWindows }
    buildPath = ersatzPath
    cmakeFilenames = universalCMakeFilenames
}

val packageOutputDir = layout.buildDirectory.dir("cmake-package")

fun setupConfigTasks(config: String, imageTask: TaskProvider<GraalNativeImageTask>, debug: Boolean, taskBlock: Action<Task>): TaskProvider<CMakePackageBuilder> {
    val configTaskName = config.lowercase()
    val configPackageOutputDir = layout.buildDirectory.dir(config)

    val build = tasks.register<Exec>("build_ersatz_${configTaskName}") {
        inputs.files(generator)
        inputs.files(layout.projectDirectory.dir("../cmake/ersatz"))
        outputs.dir(layout.buildDirectory.dir(config))

        workingDir = layout.projectDirectory.asFile
        executable = "cmake"
        args("--build", ersatzPath.get().asFile.toString(), "--config", config)
    }

    val ersatzConfigPath = ersatzPath.map { it.dir("staging") }
    val cmakeFilenames = EDITION.map(cmakeFilenameMapper(universalCMakeFileSuffixes.plus(listOf("", "-${config.lowercase()}"))))
    val generateCMakeConfigPackageFiles = tasks.register<Exec>("install_ersatz_${configTaskName}") {
        inputs.files(build)
        outputs.files(ersatzConfigPath.map { conf -> conf.dir("lib/cmake/SaxonCCore${EDITION.get()}").files(cmakeFilenames.get()) })

        workingDir = layout.projectDirectory.asFile
        executable = "cmake"
        args("--install", ersatzPath.get().asFile.toString(), "--config", config, "--prefix", ersatzConfigPath.get().asFile.toString())
    }

    val mainTask = tasks.register<CMakePackageBuilder>("${configTaskName}_cmake_package") {
        group = LifecycleBasePlugin.BUILD_GROUP
        libName = extension.edition.map { edition -> "saxonc-core-${edition}" }
        cmakePackageName = EDITION.map { "SaxonCCore${it}" }
        version =  saxonBuild.version
        majorVersion = saxonBuild.majorVersion
        sharedLibFiles.from(imageTask.map { it.sharedLibFiles })
        importLibFiles.from(imageTask.map { it.importLibFiles })
        headerFiles.from(imageTask.map { it.headerFiles })
        debugConfig = debug
        packageCMakeFiles.from(generateCMakeConfigPackageFiles)
        packageDir = configPackageOutputDir
    }

    mainTask.configure(taskBlock)
    return mainTask
}

val releaseCMakePackage = setupConfigTasks("Release", graalImage, false) {
    description = "Build and package up the native-image and CMake package config files"
}
val debugCMakePackage = setupConfigTasks("Debug", debugGraalImage, true) {
    description = "Build and package up the debug-enabled version of the native-image and CMake package config files"
}

tasks.named(LifecycleBasePlugin.ASSEMBLE_TASK_NAME) {
    dependsOn(releaseCMakePackage)
}

cMakePackageDir.outgoing.artifact(releaseCMakePackage.map { it.packageDir.asFile }) { builtBy(releaseCMakePackage) }
debugCMakePackageDir.outgoing.artifact(debugCMakePackage.map { it.packageDir.asFile }) { builtBy(debugCMakePackage) }
