import com.saxonica.build.TargetHost
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

val targetHost = TargetHost.current()
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
    sourceCompatibility = JavaVersion.VERSION_17
    targetCompatibility = JavaVersion.VERSION_17
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

    doLast {
      saxonBuild.patchReleaseInfo(
          layout.buildDirectory.file("tmp/bundled-filtered-sources/net/sf/saxon/Version.java").get().asFile,
          "${findProperty("saxonVersion")!!}",
          "${findProperty("saxonMajorReleaseDate")!!}")
      val verifier = layout.buildDirectory.file("tmp/bundled-filtered-sources/com/saxonica/config/JavaVerifier.java").get().asFile
      val dsaPublicKey = findProperty("dsaPublicKey")?.toString()
      val ed25519PublicKey = findProperty("ed25519PublicKey")?.toString()
      if (verifier.exists()) {
        saxonBuild.patchKey(verifier, "dsa", dsaPublicKey)
        saxonBuild.patchKey(verifier, "ed25519", ed25519PublicKey)
      }
    }
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

val th = targetHost

val ersatzPath = layout.buildDirectory.dir("ersatz")
val EDITION = extension.edition.map { it.uppercase() }
fun cmakePackageNameMapper(ED: String): String = "SaxonCCore${ED}"
fun cmakeFilenameMapper(suffixes: List<String>): (String) -> List<String> {
    return { ED ->
        suffixes.map { suffix -> "${cmakePackageNameMapper(ED)}${suffix}.cmake" }
    }
}
val universalCMakeFileSuffixes = listOf("Config", "ConfigVersion")
val universalCMakeFilenames = EDITION.map(cmakeFilenameMapper(universalCMakeFileSuffixes))

val configPackageOutputDir = layout.buildDirectory.dir("cmake-package")

val editionLibName = extension.edition.map { edition -> "saxonc-core-${edition}" }

fun setupConfigTasks(config: String, debug: Boolean, taskBlock: Action<Task>): TaskProvider<CMakePackageBuilder> {
    val configTaskName = config.lowercase()
    val cmakeBuildDir = ersatzPath.map { it.dir("build-${configTaskName}") }
    val ersatzPackageDir = cmakeBuildDir.map { it.dir("cmake-package") }
    val debugPostfix = DebugPostfix(targetHost)
    val releaseKindLibName = editionLibName.map { if (debug) "${it}${debugPostfix}" else it }
    val libFilenames = releaseKindLibName.map { LibFilenames(it, targetHost) }

    val imageTask = tasks.register<GraalNativeImageTask>("${configTaskName}_graal_image") {
        group = LifecycleBasePlugin.BUILD_GROUP
        description = "Build the GraalVM native-image shared library"
        targetHost = th
        sourceJar = jar.map { it.archiveFile.get() }
        outputDir = project.layout.buildDirectory.dir("native-image/${config}")
        graalNativeImage = extension.graal()!!.nativeImage
        libName = releaseKindLibName
        headerLibName = editionLibName
        version = saxonBuild.version
        majorVersion = saxonBuild.majorVersion
        macosx_min_version = saxonBuild.macosx_min_version
        this.debug = debug
        specifiedArgs = saxonBuild.targetHost.map {
            if (it.isWindows && it.isX86_64) listOf("-march=x86-64-v1") else listOf()
        }
    }

    val generator = tasks.register<CMakeGeneratorTask>("${configTaskName}_cmake_generate") {
        group = LifecycleBasePlugin.BUILD_GROUP
        description = "Generate build system for the ersatz CMake-driven library"

        version = saxonBuild.version
        platformIsWindows = saxonBuild.targetHost.map { it.isWindows }
        buildPath = cmakeBuildDir
        cmakeFilenames = universalCMakeFilenames
        cMakeInputFiles.from(layout.projectDirectory.dir("../cmake/ersatz").asFileTree)
    }

    val build = tasks.register<Exec>("${configTaskName}_cmake_build") {
        inputs.files(generator)
        outputs.files(cmakeBuildDir.map { it.dir(config).files(libFilenames.map { it.platformSharedLibFilename(saxonBuild.version.get()) })})

        workingDir = layout.projectDirectory.asFile
        executable = "cmake"
        args("--build", cmakeBuildDir.get().asFile.toString(), "--config", config)
    }

    val configCMakeFilenames = EDITION.map(cmakeFilenameMapper(listOf("", "-${configTaskName}")))
    val install = tasks.register<Exec>("${configTaskName}_cmake_install") {
        inputs.files(build)
        outputs.files(ersatzPackageDir.map {
            it.dir("lib/cmake/${cmakePackageNameMapper(EDITION.get())}")
        }.map {
            it.files(configCMakeFilenames) + it.files(universalCMakeFilenames)
        })

        workingDir = layout.projectDirectory.asFile
        executable = "cmake"
        args(
            "--install", cmakeBuildDir.get().asFile.toString(),
            "--config", config,
            "--prefix", ersatzPackageDir.get().asFile.toString()
        )
    }

    val mainTask = tasks.register<CMakePackageBuilder>("${configTaskName}_cmake_package") {
        group = LifecycleBasePlugin.BUILD_GROUP
        libName = releaseKindLibName
        cmakePackageName = EDITION.map { "SaxonCCore${it}" }
        version =  saxonBuild.version
        majorVersion = saxonBuild.majorVersion
        sharedLibFiles.from(imageTask.map { it.sharedLibFiles })
        importLibFiles.from(imageTask.map { it.importLibFiles })
        headerFiles.from(imageTask.map { it.headerFiles })
        debugConfig = debug
        packageCMakeFiles.from(install)
        packageDir = configPackageOutputDir.map { it.dir(config) }
    }

    mainTask.configure(taskBlock)
    return mainTask
}

val stopgapCOnlyJavaSrc = tasks.register<Sync>("stopgap_c_only_java_src") {
    into(layout.buildDirectory.dir("c-only-java-src"))
    from(preprocessJava.map { it.output }) {
        include("net/sf/saxon/option/cpp/**/*")
    }
}

val releaseCMakePackage = setupConfigTasks("Release", false) {
    description = "Build and package up the native-image and CMake package config files"
}
val debugCMakePackage = setupConfigTasks("Debug", true) {
    description = "Build and package up the debug-enabled version of the native-image and CMake package config files"
}

tasks.named(LifecycleBasePlugin.ASSEMBLE_TASK_NAME) {
    dependsOn(releaseCMakePackage)
}

val releaseCMakePackageDir by configurations.consumable("releaseCMakePackageDir")
val debugCMakePackageDir by configurations.consumable("debugCMakePackageDir")
val javaSrcDir by configurations.consumable("javaSrcDir")

releaseCMakePackageDir.outgoing.artifact(releaseCMakePackage.map { it.packageDir.asFile }) { builtBy(releaseCMakePackage) }
debugCMakePackageDir.outgoing.artifact(debugCMakePackage.map { it.packageDir.asFile }) { builtBy(debugCMakePackage) }
javaSrcDir.outgoing.artifact(stopgapCOnlyJavaSrc.map { it.destinationDir }) { builtBy(stopgapCOnlyJavaSrc) }
