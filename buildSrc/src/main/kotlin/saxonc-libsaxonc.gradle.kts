import com.saxonica.build.LifecycleTasks
import com.saxonica.build.GatherReleaseArtifacts
import com.saxonica.build.cpp.DoxygenTask
import org.gradle.api.tasks.Copy
import org.gradle.api.tasks.Exec
import org.gradle.kotlin.dsl.*
import java.nio.file.Files
import org.apache.tools.ant.filters.ReplaceTokens

plugins {
    base
}

val extension = project.extensions.create<SaxonCExtension>("libsaxonc")
val saxonBuild = the<SaxonBuildExtension>()

extension.package_name.convention(extension.edition.map { "SaxonC${it.uppercase()}" })
extension.package_file_name.convention(extension.package_name.map { name ->
    val (os, arch) = saxonBuild.targetHost.get()
    "${name}-${os}-${arch}-${saxonBuild.version.get().replace('.', '-')}"
})
val saxonCCoreCMakePackageBucket by configurations.dependencyScope("saxonCCoreCMakePackageBucket")
val saxonCCoreCMakePackageDir by configurations.resolvable("saxonCCoreCMakePackageDir") {
    extendsFrom(saxonCCoreCMakePackageBucket)
}

val phpReleaseFolderBucket by configurations.dependencyScope("phpReleaseFolderBucket")
val phpReleaseFolder by configurations.resolvable("phpReleaseFolder") {
    extendsFrom(phpReleaseFolderBucket)
}

val pythonSamplesBucket by configurations.dependencyScope("pythonSamplesBucket")
val pythonSamples by configurations.resolvable("pythonSamples") {
    extendsFrom(pythonSamplesBucket)
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
    saxonCCoreCMakePackageBucket(extension.edition.map { edition ->
        project(path = ":libsaxonc-core:${edition}", configuration = "cMakePackageDir")
    })
    phpReleaseFolderBucket(extension.edition.map  { edition ->
        project(path = ":php:${edition}", configuration = "releaseFolder")
    })
    pythonSamplesBucket(extension.edition.map  { edition ->
        project(path = ":python:${edition}", configuration = "packagedSamples")
    })
}

interface InjectedExecOps {
    @get:Inject val execOps: ExecOperations
}

val generator = tasks.register("cmake_generate") {
    group = LifecycleBasePlugin.BUILD_GROUP
    inputs.file(layout.projectDirectory.file("CMakeLists.txt"))
    inputs.files(layout.projectDirectory.dir("../cmake"))
    inputs.dir(saxonCCoreCMakePackageDir.elements.map { it.first().asFile })
    outputs.file(layout.buildDirectory.file("CMakeCache.txt"))

    val injected = project.objects.newInstance<InjectedExecOps>()

    doLast {
        val generatorName = if (saxonBuild.platform.get() == "windows") "Visual Studio 17 2022" else "Ninja Multi-Config"
        injected.execOps.exec {
            workingDir = layout.projectDirectory.asFile
            executable = "cmake"
            args("-G", generatorName, "-B", layout.buildDirectory.get().asFile.toString(),
                "-D", saxonBuild.version.map { "SAXON_VERSION:STRING=${it}" }.get(),
                "-D", extension.package_name.map { "SAXON_PACKAGE_NAME:STRING=${it}" }.get(),
                "-D", extension.package_file_name.map { "SAXON_PACKAGE_FILE_NAME:STRING=${it}" }.get(),
                "-D", saxonBuild.targetHost.map { "SAXON_PACKAGE_OS:STRING=${it.name}" }.get(),
                "-D", saxonBuild.targetHost.map { "SAXON_PACKAGE_ARCH:STRING=${it.architecture}" }.get(),
                "-D", saxonCCoreCMakePackageDir.elements.map { "SaxonCCore_PackageDir:PATH=${it.first().asFile}" }.get()
            )
            if (saxonBuild.macosx_min_version.isPresent) {
                args("-D", "CMAKE_OSX_DEPLOYMENT_TARGET=${saxonBuild.macosx_min_version.get()}")
            }
        }
    }
}

val build = tasks.register<Exec>("build_release") {
    group = LifecycleBasePlugin.BUILD_GROUP
    inputs.files(generator)
    inputs.files(layout.projectDirectory.dir("../src"))
    inputs.files(layout.projectDirectory.dir("src"))
    inputs.files(layout.projectDirectory.dir("../samples/cppTests"))
    inputs.files(layout.projectDirectory.dir("../samples/cTests"))
    outputs.dir(layout.buildDirectory.dir("Release"))

    workingDir = layout.projectDirectory.asFile
    executable = "cmake"
    args("--build", layout.buildDirectory.get().asFile.toString(), "--config", "Release")
}

tasks.named(LifecycleBasePlugin.ASSEMBLE_TASK_NAME) {
    dependsOn(build)
}
val buildDebug = tasks.register<Exec>("build_debug") {
    group = LifecycleBasePlugin.BUILD_GROUP
    inputs.files(generator)
    inputs.files(layout.projectDirectory.dir("../src"))
    inputs.files(layout.projectDirectory.dir("src"))
    outputs.dir(layout.buildDirectory.dir("Debug"))

    workingDir = layout.projectDirectory.asFile
    executable = "cmake"
    args("--build", layout.buildDirectory.get().asFile.toString(), "--config", "Debug")
}

val releaseStagingPath = layout.buildDirectory.dir("staging/Release")
val stage = tasks.register<Exec>("staging_install") {
    group = LifecycleBasePlugin.BUILD_GROUP
    inputs.files(build)
    outputs.dir(releaseStagingPath)

    workingDir = layout.projectDirectory.asFile
    executable = "cmake"
    args("--install", layout.buildDirectory.get().asFile.toString(), "--config", "Release", "--prefix", releaseStagingPath.get().asFile.toString())
}

val debugStagingPath = layout.buildDirectory.dir("staging/Debug")
val debugStage = tasks.register<Exec>("debug_staging_install") {
    group = LifecycleBasePlugin.BUILD_GROUP
    inputs.files(buildDebug)
    outputs.dir(debugStagingPath)

    workingDir = layout.projectDirectory.asFile
    executable = "cmake"
    args("--install", layout.buildDirectory.get().asFile.toString(), "--config", "Debug", "--prefix", debugStagingPath.get().asFile.toString())
}

val ctestWorkingDir = layout.buildDirectory.dir("ctest-working-dir")
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
    workingDir = layout.buildDirectory.get().asFile
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
    dependsOn(build)
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
        val sourceRootPath = releaseStagingPath.get().asFile.toPath()
        val destinationRootPath = destinationDir.toPath()
        into(extension.package_name)

        if (!saxonBuild.targetHost.get().isWindows) {
            eachFile {
                val sourcePathObj = sourceRootPath.resolve(sourcePath)
                val destinationPath = destinationRootPath.resolve(path)
                if(Files.isSymbolicLink(sourcePathObj)) {
                    exclude()
                    destinationPath.parent.toFile().mkdirs()
                    val linkTarget = Files.readSymbolicLink(sourcePathObj)
                    project.exec {
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

val sourceZipTask = tasks.register<Zip>("source_release_zip") {
    archiveBaseName = extension.package_name
    archiveAppendix = "source"
    archiveVersion = saxonBuild.version

    destinationDirectory = packagedDir

    val topLevelDir = extension.package_name.map { name -> "${name}-${saxonBuild.version.get()}" }
    into(topLevelDir)
    from(layout.projectDirectory.dir("../src"))

}

val release = tasks.named(LifecycleTasks.RELEASE_TASK_NAME) {
    dependsOn(if (saxonBuild.targetHost.get().isWindows) releaseZipWindowsTask else releaseZipUnicesTask)
}

libDir.outgoing.artifact(releaseStagingPath.map { it.dir("lib") }) { builtBy(stage) }
includeDir.outgoing.artifact(releaseStagingPath.map { it.dir("include") }) { builtBy(stage) }
releaseArtifactsDir.outgoing.artifact(releaseStagingPath) { builtBy(stage) }
releaseZips.outgoing.artifact(packagedZip) { builtBy(release) }
debugLibDir.outgoing.artifact(debugStagingPath.map { it.dir("lib") }) { builtBy(debugStage) }
debugIncludeDir.outgoing.artifact(debugStagingPath.map { it.dir("include") }) { builtBy(debugStage) }
debugArtifactsDir.outgoing.artifact(debugStagingPath) { builtBy(debugStage) }
