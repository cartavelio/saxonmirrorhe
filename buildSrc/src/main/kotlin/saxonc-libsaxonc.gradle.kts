import com.saxonica.build.LifecycleTasks
import com.saxonica.build.cpp.CMakeCompatibleSourceFolder
import com.saxonica.build.cpp.CMakeGeneratorTask
import com.saxonica.build.cpp.DoxygenTask
import org.gradle.api.tasks.Copy
import org.gradle.api.tasks.Exec
import org.gradle.kotlin.dsl.*
import java.nio.file.Files
import org.apache.tools.ant.filters.ReplaceTokens
import packagedDir
import java.io.IOException

plugins {
    base
}

val extension = project.extensions.create<SaxonCExtension>("libsaxonc")
val saxonBuild = the<SaxonBuildExtension>()

extension.package_name.convention(extension.edition.map { "SaxonC${it.uppercase()}" })
extension.package_file_name.convention(extension.package_name.map { name ->
    val (os, arch) = saxonBuild.targetHost.get()
    "${name}-${os}-${arch}-${saxonBuild.hyphenatedVersion.get()}"
})
val saxonCCoreReleaseCMakePackageBucket by configurations.dependencyScope("saxonCCoreReleaseCMakePackageBucket")
val saxonCCoreReleaseCMakePackageDir by configurations.resolvable("saxonCCoreReleaseCMakePackageDir") {
    extendsFrom(saxonCCoreReleaseCMakePackageBucket)
}
val saxonCCoreDebugCMakePackageBucket by configurations.dependencyScope("saxonCCoreDebugCMakePackageBucket")
val saxonCCoreDebugCMakePackageDir by configurations.resolvable("saxonCCoreDebugCMakePackageDir") {
    extendsFrom(saxonCCoreDebugCMakePackageBucket)
}

val phpReleaseFolderBucket by configurations.dependencyScope("phpReleaseFolderBucket")
val phpReleaseFolder by configurations.resolvable("phpReleaseFolder") {
    extendsFrom(phpReleaseFolderBucket)
}

val pythonSamplesBucket by configurations.dependencyScope("pythonSamplesBucket")
val pythonSamples by configurations.resolvable("pythonSamples") {
    extendsFrom(pythonSamplesBucket)
}

val pythonSourceBucket by configurations.dependencyScope("pythonSourceBucket")
val pythonSourceDir by configurations.resolvable("pythonSourceDir") {
    extendsFrom(pythonSourceBucket)
}

val saxonCCoreJavaSrcBucket by configurations.dependencyScope("saxonCCoreJavaSrc")
val saxonCCoreJavaSrcDir by configurations.resolvable("saxonCCoreJavaSrcDir") {
    extendsFrom(saxonCCoreJavaSrcBucket)
}

project.layout.projectDirectory.dir("src/include")
val includeDir by configurations.consumable("includeDir")
val libDir by configurations.consumable("libDir")
val releaseArtifactsDir by configurations.consumable("releaseArtifactsDir")
val releaseZips by configurations.consumable("releaseZips")
val debugIncludeDir by configurations.consumable("debugIncludeDir")
val debugLibDir by configurations.consumable("debugLibDir")
val debugArtifactsDir by configurations.consumable("debugArtifactsDir")
val sampleData by configurations.consumable("sampleData")
sampleData.outgoing.artifact(layout.projectDirectory.dir("../samples/data"))


dependencies {
    saxonCCoreReleaseCMakePackageBucket(extension.edition.map { edition ->
        project(path = ":libsaxonc-core:${edition}", configuration = "releaseCMakePackageDir")
    })
    saxonCCoreDebugCMakePackageBucket(extension.edition.map { edition ->
        project(path = ":libsaxonc-core:${edition}", configuration = "debugCMakePackageDir")
    })
    phpReleaseFolderBucket(extension.edition.map  { edition ->
        project(path = ":php:${edition}", configuration = "releaseFolder")
    })
    pythonSamplesBucket(extension.edition.map  { edition ->
        project(path = ":python:${edition}", configuration = "packagedSamples")
    })
    pythonSourceBucket(extension.edition.map  { edition ->
        project(path = ":python:${edition}", configuration = "sourceFiles")
    })
    saxonCCoreJavaSrcBucket(extension.edition.map { edition ->
        project(path = ":libsaxonc-core:${edition}", configuration = "javaSrcDir")
    })
}

interface InjectedExecOps {
    @get:Inject val execOps: ExecOperations
}

//abstract class CMakeGeneratorTask  @Inject constructor(private val ex: ExecOperations) : DefaultTask() {
//    @get:InputFiles
//    abstract val cMakeFiles: ConfigurableFileCollection
//
//    @get:InputDirectory
//    abstract val saxonCCoreCMakePackageDir: DirectoryProperty
//
//    @get:Input
//    abstract val extraCMakeArgs: ListProperty<String>
//
//    @get:Internal
//    abstract val buildDir: DirectoryProperty
//
//    @get:OutputFile
//    val cMakeCache = buildDir.map { it.file("CMakeCache.txt") }
//
//    @get:OutputFile
//    val saxonVersionCFile = buildDir.map { it.file("saxonc_version.c") }
//
//    @TaskAction
//    fun generate() {
//        val generatorName = if (saxonBuild.platform.get() == "windows") "Visual Studio 17 2022" else "Ninja Multi-Config"
//        ex.exec {
//            workingDir = layout.projectDirectory.asFile
//            executable = "cmake"
//            args("-G", generatorName, "-B", buildDir.get().asFile.toString(),
//            args(extraCMakeArgs)
//            if (saxonBuild.macosx_min_version.isPresent) {
//                args("-D", "CMAKE_OSX_DEPLOYMENT_TARGET=${saxonBuild.macosx_min_version.get()}")
//            }
//        }
//    }
//}

private fun setupConfigTasks(config: String, stagingPath: Provider<Directory>, coreCMakePackageDir: ResolvableConfiguration): Triple<TaskProvider<CMakeGeneratorTask>, TaskProvider<Exec>, TaskProvider<Exec>> {
    val configTaskName = config.lowercase()

    val cmakeBuildDir = layout.buildDirectory.dir(config)
    val saxonCCoreCMakePackageDir = coreCMakePackageDir.elements.map { it.first().asFile }
    val generator = tasks.register<CMakeGeneratorTask>("cmake_generate_${configTaskName}") {
        dependsOn(coreCMakePackageDir)
        cMakeInputFiles.from(layout.projectDirectory.dir("../cmake").asFileTree)
        version = saxonBuild.version
        platformIsWindows = saxonBuild.targetHost.map { it.isWindows }
        buildPath = cmakeBuildDir
        cmakeFilenames = listOf("saxonc_version.c")
        extraCMakeArgs = { ->
            listOf(
                "-D", extension.package_name.map { "SAXON_PACKAGE_NAME:STRING=${it}" }.get(),
                "-D", extension.package_file_name.map { "SAXON_PACKAGE_FILE_NAME:STRING=${it}" }.get(),
                "-D", saxonBuild.targetHost.map { "SAXON_PACKAGE_OS:STRING=${it.platform}" }.get(),
                "-D", saxonBuild.targetHost.map { "SAXON_PACKAGE_ARCH:STRING=${it.architecture}" }.get(),
                "-D", "SaxonCCore_PackageDir:PATH=${saxonCCoreCMakePackageDir.get()}"
            )
        }
    }

//    val generator2 = tasks.register("cmake_generate_${configTaskName}") {
//        group = LifecycleBasePlugin.BUILD_GROUP
//        inputs.file(layout.projectDirectory.file("CMakeLists.txt"))
//        inputs.dir(layout.projectDirectory.dir("../cmake"))
//        inputs.dir(coreCMakePackageDir.elements.map { it.first().asFile })
//        outputs.file(cmakeBuildDir.map { it.file("CMakeCache.txt") })
//
//        val injected = project.objects.newInstance<InjectedExecOps>()
//
//        doLast {
//            val generatorName = if (saxonBuild.platform.get() == "windows") "Visual Studio 17 2022" else "Ninja Multi-Config"
//            injected.execOps.exec {
//                workingDir = layout.projectDirectory.asFile
//                executable = "cmake"
//                args("-G", generatorName, "-B", cmakeBuildDir.get().asFile.toString(),
//                    "-D", saxonBuild.version.map { "SAXON_VERSION:STRING=${it}" }.get(),
//                    "-D", extension.package_name.map { "SAXON_PACKAGE_NAME:STRING=${it}" }.get(),
//                    "-D", extension.package_file_name.map { "SAXON_PACKAGE_FILE_NAME:STRING=${it}" }.get(),
//                    "-D", saxonBuild.targetHost.map { "SAXON_PACKAGE_OS:STRING=${it.platform}" }.get(),
//                    "-D", saxonBuild.targetHost.map { "SAXON_PACKAGE_ARCH:STRING=${it.architecture}" }.get(),
//                    "-D", coreCMakePackageDir.elements.map { "SaxonCCore_PackageDir:PATH=${it.first().asFile}" }.get()
//                )
//                if (saxonBuild.macosx_min_version.isPresent) {
//                    args("-D", "CMAKE_OSX_DEPLOYMENT_TARGET=${saxonBuild.macosx_min_version.get()}")
//                }
//            }
//        }
//    }

    val build = tasks.register<Exec>("build_${configTaskName}") {
        group = LifecycleBasePlugin.BUILD_GROUP
        inputs.files(generator)
        inputs.files(layout.projectDirectory.dir("../src"))
        inputs.files(layout.projectDirectory.dir("src"))
        inputs.files(layout.projectDirectory.dir("../samples/cppTests"))
        inputs.files(layout.projectDirectory.dir("../samples/cTests"))
        outputs.dir(cmakeBuildDir.map { it.dir(config) })

        workingDir = layout.projectDirectory.asFile
        executable = "cmake"
        args("--build", cmakeBuildDir.get().asFile.toString(), "--config", config)
    }

    val stage = tasks.register<Exec>("install_staging_${configTaskName}") {
        group = LifecycleBasePlugin.BUILD_GROUP
        inputs.files(build)
        outputs.dir(stagingPath)

        workingDir = layout.projectDirectory.asFile
        executable = "cmake"
        args("--install", cmakeBuildDir.get().asFile.toString(), "--config", config, "--prefix", stagingPath.get().asFile.toString())
    }

    return Triple(generator,build, stage)
}

val stagingPath = layout.buildDirectory.dir("staging/Release")
val debugStagingPath = layout.buildDirectory.dir("staging/Debug")
val (generator, build, stage) = setupConfigTasks("Release", stagingPath, saxonCCoreReleaseCMakePackageDir)
val (debugGenerator, debugBuild, debugStage) = setupConfigTasks("Debug", debugStagingPath, saxonCCoreDebugCMakePackageDir)

tasks.named(LifecycleBasePlugin.ASSEMBLE_TASK_NAME) {
    dependsOn(build)
}

val cmakeBuildDir = layout.buildDirectory.dir("Release")
val ctestWorkingDir = cmakeBuildDir.map { it.dir("ctest-working-dir") }

val mkCtestWorkingDir = tasks.register("mk_ctest_working_dir") {
    group = LifecycleBasePlugin.VERIFICATION_GROUP
    outputs.dir(ctestWorkingDir)

    doLast {
        project.mkdir(ctestWorkingDir)
    }
}

val cleanCtestWorkingDir = tasks.register("ctest_working_dir") {
    group = LifecycleBasePlugin.VERIFICATION_GROUP
    inputs.dir(mkCtestWorkingDir.map { ctestWorkingDir.get() })

    doLast {
        project.delete(ctestWorkingDir)
        project.mkdir(ctestWorkingDir)
    }
}

val test = tasks.register<Exec>("test") {
    inputs.dir(cleanCtestWorkingDir.map { ctestWorkingDir.get() })
    inputs.files(layout.projectDirectory.dir("../samples/data"))
    group = LifecycleBasePlugin.VERIFICATION_GROUP
    description = "Run the tests defined in CMakeLists with CTest"
    dependsOn(build)
    workingDir = cmakeBuildDir.get().asFile
    executable = "ctest"
    args("-C", "Release")
}

tasks.named(LifecycleBasePlugin.CHECK_TASK_NAME) {
    dependsOn(test)
}

//val build_package = tasks.register<Exec>("cpack_package") {
//    dependsOn(build)
//
//    outputs.file(layout.buildDirectory.file(extension.package_file_name.map { "${it}.zip" }))
//
//    group = LifecycleTasks.RELEASE_GROUP
//    workingDir = layout.buildDirectory.get().asFile
//    executable = "cpack"
//    args("-C", "Release")
//}

tasks.build.configure {
//    dependsOn(build)
    dependsOn(stage)
}

val doxygenTask = tasks.register<DoxygenTask>("doxygen") {
    group = LifecycleTasks.DOCUMENTATION_GROUP
    doxyfile = layout.projectDirectory.file("../doc/Doxyfile")
    mainSrcDir = layout.projectDirectory.dir("../src")
    editionSrcDir = layout.projectDirectory.dir("src")
    docSrcDir = layout.projectDirectory.dir("../doc")
    logo = layout.projectDirectory.file("../doc/saxonica_logo.gif")
    version = saxonBuild.version
    outputDir = layout.buildDirectory.dir("docs")

    configValues {
        add("INPUT").value(files(mainSrcDir, editionSrcDir, docSrcDir))
        add("PROJECT_NUMBER").value(version)
        add("PROJECT_LOGO").value(logo)
        add("OUTPUT_DIRECTORY").value(outputDir)
    }
}

tasks.named(LifecycleTasks.DOCUMENTATION_TASK_NAME) {
    dependsOn(doxygenTask)
}

val noticeFilenames = listOf("JAMESCLARK.txt", "JLINE2.txt", "LICENSE.txt")
val notices = project.rootProject.layout.projectDirectory.dir("src/main/notices").files(noticeFilenames)

val packagedDir = layout.buildDirectory.dir("packaged")
val releaseFolderTask = tasks.register<Copy>("build_release_zip_contents_folder") {
    into(packagedDir.map { it.dir("tmp/${extension.package_file_name.get()}") })

    // this is the path releaseStagingPath, but we're triggering the build dependency
    from(stage.map { it.outputs.files.singleFile }) {
        val sourceRootPath = layout.buildDirectory.dir("staging/Release").get().asFile.toPath()
        val destinationRootPath = destinationDir.toPath()
        into(extension.package_name)

        if (!saxonBuild.targetHost.get().isWindows) {
            val injected = project.objects.newInstance<InjectedExecOps>()
            eachFile {
                val sourcePathObj = sourceRootPath.resolve(sourcePath)
                val destinationPath = destinationRootPath.resolve(path)
                if(Files.isSymbolicLink(sourcePathObj)) {
                    exclude()
                    destinationPath.parent.toFile().mkdirs()
                    val linkTarget = Files.readSymbolicLink(sourcePathObj)
                    injected.execOps.exec {
                        executable = "ln"
                        args("-nsf", linkTarget, destinationPath)
                    }
                }
            }
        }
    }
    into("notices") {
        from(notices)
    }
    into("php") {
        from(phpReleaseFolder.elements.map { it.first() })
    }
    into("samples") {
        from(layout.projectDirectory.dir("../samples")) {
            exclude("CMakeLists.txt.in")
        }
        from(layout.projectDirectory.dir("../samples")) {
            include("CMakeLists.txt.in")
            rename { _ -> "CMakeLists.txt" }
            filter<ReplaceTokens>("tokens" to mapOf(
                "version" to saxonBuild.version.get(),
                "EDITION" to extension.edition.get().uppercase(),
                "edition" to extension.edition.get(),
            ))
        }
    }
    into("python") {
        from(pythonSamples.elements.map { it.first() })
    }
    from(layout.projectDirectory.dir("../")) {
        include("README.md")
        filter<ReplaceTokens>("tokens" to mapOf(
            "EDITION" to extension.edition.map { it.uppercase() }.get(),
            "VERSION" to saxonBuild.version.get(),
        ))
    }
}

val packagedZip = packagedDir.map { it.file("${extension.package_file_name.get()}.zip") }
val releaseZipWindowsTask = tasks.register<Zip>("release_zip_windows") {
    from(releaseFolderTask.map { it.destinationDir })
    destinationDirectory = packagedDir
    archiveBaseName = extension.package_file_name
}

val releaseZipUnicesTask = tasks.register<Exec>("release_zip") {
    group = LifecycleTasks.RELEASE_GROUP
    description = "generate the libsaxon download .zip package"

    inputs.dir(releaseFolderTask.map { it.destinationDir })
    outputs.file(packagedZip)

    executable("zip")
    args("--recurse-paths", "--symlinks", packagedZip.get(), extension.package_file_name.get())
    workingDir = packagedDir.map { it.dir("tmp") }.get().asFile
}

val exportHeaderTask = tasks.register("combined_export_header") {
    val cmakeExportHeader = build.map { layout.buildDirectory.file("Release/saxonc_export.h").get() }
    val clangHeader = layout.projectDirectory.file("../saxonc_export_header/clang/saxonc_export.h")
    val msvcHeader = layout.projectDirectory.file("../saxonc_export_header/msvc/saxonc_export.h")
    inputs.file(cmakeExportHeader)
    inputs.file(clangHeader)
    inputs.file(msvcHeader)
    outputs.file(layout.buildDirectory.file("unified_export_header/saxonc_export.h"))

    doLast {
        val compilerDirName = if (saxonBuild.targetHost.get().isWindows) "msvc" else "clang"
        val comparisonHeaderFile = layout.projectDirectory.file("../saxonc_export_header/${compilerDirName}/saxonc_export.h").asFile.toPath()
        val cmakeChanged = Files.mismatch(cmakeExportHeader.get().asFile.toPath(), comparisonHeaderFile) > -1L
        if (cmakeChanged) IOException("CMake generated export header ${comparisonHeaderFile} has changed")

        val transformer = Transformer<String?, String> { line ->
            val skip = when (line) {
                "#ifndef SAXONC_EXPORT_H" -> true
                "#define SAXONC_EXPORT_H" -> true
                "#endif /* SAXONC_EXPORT_H */" -> true
                else -> false
            }
            if (skip) "" else line
        }

        copy {
            into(layout.buildDirectory.dir("unified_export_header"))
            from(clangHeader) {
                rename({_ -> "clang_export.h"})
                filter(transformer)
            }
            from(msvcHeader) {
                rename({_ -> "msvc_export.h"})
                filter(transformer)
            }
        }

        copy {
            into(layout.buildDirectory.dir("unified_export_header"))
            from(layout.projectDirectory.file("../saxonc_export_header/saxonc_export.h.in")) {
                rename({_ -> "saxonc_export.h"})
                filter<ReplaceTokens>("tokens" to mapOf(
                    "MSVC_HEADER" to layout.buildDirectory.file("unified_export_header/msvc_export.h").get().asFile.readText(),
                    "CLANG_HEADER" to layout.buildDirectory.file("unified_export_header/clang_export.h").get().asFile.readText()
                ))
            }
        }
    }
}

val sourceCMakeDistroFolderTask = tasks.register<CMakeCompatibleSourceFolder>("source_cmake_distro_folder") {
    dependsOn(generator, saxonCCoreReleaseCMakePackageDir)

    packageDirName = extension.package_file_name.map {
        "${it}-cmake"
    }
    packageTmpDir = packagedDir
    replaceTokens = mapOf(
        "edition" to extension.edition.get(),
        "EDITION" to extension.edition.map { it.uppercase() }.get(),
        "VERSION" to saxonBuild.version.get(),
    )

    copyBlock.set({
        into("lib") {
            from(generator.map { it.outputFiles.get() }) {
              include("saxonc_version.c")
            }
        }
        from(notices) {
            into("notices")
        }
        from(saxonCCoreReleaseCMakePackageDir.elements.map { it.first() }) {
            into(extension.edition.map { "SaxonCCore${it.uppercase()}" })
        }
    })
}

val sourceCMakeDistroZipTask = tasks.register<Zip>("source_cmake_distro_zip") {
    archiveBaseName = extension.package_name
    archiveAppendix = "cmake"
    archiveVersion = saxonBuild.hyphenatedVersion

    into(sourceCMakeDistroFolderTask.map { it.outputDir.get().asFile.name })
    from(sourceCMakeDistroFolderTask.map { it.outputDir.get() })
}
//val sourceCMakeDistroFolderTask = tasks.register<Copy>("source_cmake_distro_folder") {
//    fun tokens(): Map<String, String> {
//        return mapOf(
//            "edition" to extension.edition.get(),
//            "EDITION" to extension.edition.map { it.uppercase() }.get(),
//            "VERSION" to saxonBuild.version.get(),
//        )
//    }
//
//    into(packagedDir.map { it.dir("tmp/${outputDirName.get()}") })
//
//    from(layout.projectDirectory.dir("../source-cmake-distro")) {
//        include("CMakeLists.txt.in")
//        rename { "CMakeLists.txt" }
//        filter<ReplaceTokens>("tokens" to tokens())
//    }
//    from(layout.projectDirectory.dir("../cmake")) {
//        into("cmake")
//        include("libsaxonc.cmake")
//    }
//
//    into("lib") {
//        from(layout.projectDirectory.dir("../src/lib")) {
//            exclude("*.c.in")
//        }
//        from(generator.map { it.outputFiles.get() }) {
//            include("saxonc_version.c")
//        }
//    }
//
//    from(layout.projectDirectory.dir("../src/include")) {
//        into("include")
//    }
//
//    from(notices) {
//        into("notices")
//    }
//
//    from(layout.projectDirectory.dir("../")) {
//        include("README.md")
//        filter<ReplaceTokens>("tokens" to tokens())
//    }
//}

val sourceZipTask = tasks.register<Zip>("source_release_zip") {
    archiveBaseName = extension.package_name
    archiveAppendix = "source"
    archiveVersion = saxonBuild.hyphenatedVersion

    val topLevelDir = extension.package_name.map { name -> "${name}-source-${saxonBuild.hyphenatedVersion.get()}" }
    into(topLevelDir)

    // C/C++/Java Sources
    from(layout.projectDirectory.dir("../src")) {
        into("src/main/c")
        exclude("**/*.c.in", "**/Gizmo.c")
    }
    from(layout.projectDirectory.dir("src")) {
        into("src/main/c")
    }
    from(saxonCCoreJavaSrcDir.elements.map { it.first() }) {
        into("src/main/java")
    }
    from(exportHeaderTask) {
        into("src/main/c/include")
    }
    from(build.map { layout.buildDirectory.file("saxonc_version.c") }) {
        into("src/main/c/lib")
    }
    // Python sources
    from(pythonSourceDir.elements.map { it.first() }) {
        into("python")
    }
    // License
    from(layout.projectDirectory.dir("../../src/main/notices")) {
        include("LICENSE.txt")
        into("notices")
    }
    // Readme
    from(layout.projectDirectory.dir("../")) {
        include("README-source-zip.md")
        rename { _ -> "README.md" }
        filter<ReplaceTokens>("tokens" to mapOf(
            "EDITION" to extension.edition.map { it.uppercase() }.get(),
            "VERSION" to saxonBuild.version.get(),
        ))
    }
}

val release = tasks.named(LifecycleTasks.RELEASE_TASK_NAME) {
    dependsOn(if (saxonBuild.targetHost.get().isWindows) releaseZipWindowsTask else releaseZipUnicesTask)
    dependsOn(sourceZipTask)
}

libDir.outgoing.artifact(stagingPath.map { it.dir("lib") }) { builtBy(stage) }
includeDir.outgoing.artifact(stagingPath.map { it.dir("include") }) { builtBy(stage) }
releaseArtifactsDir.outgoing.artifact(stagingPath) { builtBy(stage) }
releaseZips.outgoing.artifact(packagedZip) { builtBy(release) }
debugLibDir.outgoing.artifact(debugStagingPath.map { it.dir("lib") }) { builtBy(debugStage) }
debugIncludeDir.outgoing.artifact(debugStagingPath.map { it.dir("include") }) { builtBy(debugStage) }
debugArtifactsDir.outgoing.artifact(debugStagingPath) { builtBy(debugStage) }
