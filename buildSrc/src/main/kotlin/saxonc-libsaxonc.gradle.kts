import com.saxonica.build.LifecycleTasks
import com.saxonica.build.TemplatedDocTask
import com.saxonica.build.cpp.CMakeBuildTask
import com.saxonica.build.cpp.CMakeGeneratorTask
import com.saxonica.build.cpp.CMakeInstallTask
import com.saxonica.build.cpp.DoxygenTask
import com.saxonica.build.doc.SaxonCReadme
import org.gradle.api.tasks.Exec
import org.gradle.kotlin.dsl.*
import java.nio.file.Files
import org.apache.tools.ant.filters.ReplaceTokens
import java.io.IOException
import java.nio.file.LinkOption
import java.nio.file.StandardCopyOption
import kotlin.io.path.createParentDirectories
import kotlin.io.path.isSymbolicLink

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
extension.binaries.addAll("Transform", "Query")
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
val sourceIncludeDir by configurations.consumable("sourceIncludeDir")
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

private fun setupConfigTasks(config: String, stagingPath: Provider<Directory>, coreCMakePackageDir: ResolvableConfiguration): Triple<TaskProvider<CMakeGeneratorTask>, TaskProvider<CMakeBuildTask>, TaskProvider<CMakeInstallTask>> {
    val configTaskName = config.lowercase()

    val cmakeBuildDir = layout.buildDirectory.dir(config)
    val saxonCCoreCMakePackageDir = coreCMakePackageDir.elements.map { it.first().asFile }
    val generator = tasks.register<CMakeGeneratorTask>("cmake_generate_${configTaskName}") {
        dependsOn(coreCMakePackageDir)
        cMakeInputFiles.from(layout.projectDirectory.dir("../cmake").asFileTree)
        version = saxonBuild.version
        platformIsWindows = saxonBuild.targetHost.map { it.isWindows }
        macosMinVersion = saxonBuild.macosx_min_version
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

    val build = tasks.register<CMakeBuildTask>("build_${configTaskName}") {
        group = LifecycleBasePlugin.BUILD_GROUP
        inputs.files(generator)
        inputs.files(layout.projectDirectory.dir("../src"))
        inputs.files(layout.projectDirectory.dir("src"))
        inputs.files(layout.projectDirectory.dir("../samples/cppTests"))
        inputs.files(layout.projectDirectory.dir("../samples/cTests"))

        buildPath = cmakeBuildDir
        cMakeInputFiles.from(generator)
        configuration = config
    }

    val stage = tasks.register<CMakeInstallTask>("install_staging_${configTaskName}") {
        buildDir = cmakeBuildDir
        buildFiles.from(build)
        stagingDir = stagingPath
        configuration = config
    }

    return Triple(generator, build, stage)
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

tasks.build.configure {
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

val exportHeaderTask = tasks.register("combined_export_header") {
    val cmakeExportHeader = build.map { layout.buildDirectory.file("Release/saxonc_export.h").get() }
    val combinedHeaderTemplate = layout.projectDirectory.file("../saxonc_export_header/saxonc_export.h.in")
    val clangHeader = layout.projectDirectory.file("../saxonc_export_header/clang/saxonc_export.h")
    val msvcHeader = layout.projectDirectory.file("../saxonc_export_header/msvc/saxonc_export.h")
    inputs.files(cmakeExportHeader, combinedHeaderTemplate, clangHeader, msvcHeader)
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

val packagedDir = layout.buildDirectory.dir("packaged")

fun CopySpec.fromPreservingSymlinks(sourceDir: Provider<File>, dest: java.nio.file.Path, copyBlock: Action<CopySpec>) {
    this.from(sourceDir) {
        copyBlock.execute(this)
        eachFile {
            val p = this.file.toPath()
            if (p.isSymbolicLink()) {
                this.exclude()
                val sourcePath = sourceDir.get().toPath().resolve(this.sourcePath)
                val targetPath = dest.resolve(this.path)
                targetPath.createParentDirectories()
                Files.copy(sourcePath, targetPath, StandardCopyOption.REPLACE_EXISTING, LinkOption.NOFOLLOW_LINKS)
            }
        }
    }
}

fun replaceTokens(): Map<String, String> {
    return mapOf(
        "edition" to extension.edition.get(),
        "EDITION" to extension.edition.map { it.uppercase() }.get(),
        "version" to saxonBuild.version.get(),
        "platform" to saxonBuild.targetHost.get().platform,
        "architecture" to saxonBuild.targetHost.get().architecture,
    )
}

val readmeDir = layout.buildDirectory.dir("release-readme")

val editionReadmeTemplateDir = layout.projectDirectory.dir("README.in")
val binDirName = saxonBuild.targetHost.map {
    val (os, arch) = it
    "${os}-${arch}"
}
val readmeTask = tasks.register<TemplatedDocTask>("readme") {
    group = LifecycleTasks.DOCUMENTATION_GROUP
    templateRoot = layout.projectDirectory.dir("../README.in")
    if (editionReadmeTemplateDir.asFile.exists()) {
        editionTemplateRoot = editionReadmeTemplateDir
    }
    template = "README.md.peb"
    renderedDoc = readmeDir.map { it.file("README.md") }
    model = SaxonCReadme(
        productVersion = saxonBuild.version.get(),
        edition = extension.edition.get(),
        host = saxonBuild.targetHost.get(),
        releaseDirName = extension.package_file_name.get(),
        binaries = extension.binaries.get().toTypedArray(),
    )
}

val relaseFolderTmpDir = packagedDir.map { it.dir("tmp") }
val releaseFolderTask = tasks.register<Copy>("build_release_zip_contents_folder") {
    val dest = relaseFolderTmpDir.map { it.dir(extension.package_file_name.get()) }.get().asFile.toPath()

    // set root destination dir
    into(dest)

    // Saxon C Source
    into(extension.edition.map { "src" }) {
        from(layout.projectDirectory.dir("../source-cmake-distro")) {
            include("CMakeLists.txt.in")
            rename { "CMakeLists.txt" }
            filter<ReplaceTokens>("tokens" to replaceTokens())
        }
        from(layout.projectDirectory.dir("../cmake")) {
            into("cmake")
            include("libsaxonc.cmake")
        }
        from(layout.projectDirectory.dir("../saxonc_export_header")) {
            into("cmake")
            include("export-header.cmake")
        }

        into("lib") {
            from(layout.projectDirectory.dir("../src/lib")) {
                exclude("*.c.in")
            }
        }

        from(layout.projectDirectory.dir("../src/include")) {
            into("include")
        }
        from(layout.projectDirectory.dir("src/include")) {
            into("include")
        }
        from(exportHeaderTask.map { it.outputs.files }) {
            into("include")
        }

        from(readmeTask.map { it.outputs.files })

        into("lib") {
            from(generator.map { it.outputFiles.get() }) {
                include("saxonc_version.c")
            }
        }
    }

    // notices
    from(notices) {
        into("notices")
    }

    // Binary artifacts
    val saxonCCoreCMakePackageDir = saxonCCoreReleaseCMakePackageDir.elements.map { it.first() }
    val saxonCCoreDestDirName = extension.edition.map { "SaxonCCore${extension.edition.get().uppercase()}" }
    val saxonCDestDirName = extension.package_name

    // libsaxonc-core
    fromPreservingSymlinks(saxonCCoreCMakePackageDir.map { it.asFile }, dest) {
        into(saxonCCoreDestDirName)
    }
    // libsaxonc
    fromPreservingSymlinks(stage.map { it.outputs.files.singleFile }, dest) {
        into(saxonCDestDirName)
    }

    // php
    into("php") {
        from(phpReleaseFolder.elements.map { it.first() })
    }

    // samples
    into("samples") {
        from(layout.projectDirectory.dir("../samples")) {
            exclude("CMakeLists.txt.in")
        }
        from(layout.projectDirectory.dir("../samples")) {
            include("CMakeLists.txt.in")
            rename { _ -> "CMakeLists.txt" }
            filter<ReplaceTokens>("tokens" to replaceTokens())
        }
    }

    // python
    into("python") {
        from(pythonSamples.elements.map { it.first() })
    }

    from(readmeTask.map { it.outputs.files })
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

val release = tasks.named(LifecycleTasks.RELEASE_TASK_NAME) {
    dependsOn(if (saxonBuild.targetHost.get().isWindows) releaseZipWindowsTask else releaseZipUnicesTask)
}

sourceIncludeDir.outgoing.artifact(layout.projectDirectory.dir("../src/include"))
libDir.outgoing.artifact(stagingPath.map { it.dir("lib") }) { builtBy(stage) }
includeDir.outgoing.artifact(stagingPath.map { it.dir("include") }) { builtBy(stage) }
releaseArtifactsDir.outgoing.artifact(stagingPath) { builtBy(stage) }
releaseZips.outgoing.artifact(packagedZip) { builtBy(release) }
debugLibDir.outgoing.artifact(debugStagingPath.map { it.dir("lib") }) { builtBy(debugStage) }
debugIncludeDir.outgoing.artifact(debugStagingPath.map { it.dir("include") }) { builtBy(debugStage) }
debugArtifactsDir.outgoing.artifact(debugStagingPath) { builtBy(debugStage) }
