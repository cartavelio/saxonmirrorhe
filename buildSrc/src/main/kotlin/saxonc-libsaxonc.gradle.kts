import com.saxonica.build.LifecycleTasks
import com.saxonica.build.cpp.DoxygenTask
import org.gradle.api.tasks.Copy
import org.gradle.api.tasks.Exec
import org.gradle.kotlin.dsl.*

plugins {
    base
}

val extension = project.extensions.create<SaxonCExtension>("libsaxonc")
val saxonBuild = the<SaxonBuildExtension>()

extension.package_name.convention(extension.edition.map { "SaxonC${it.uppercase()}" })
extension.package_file_name.convention(extension.package_name.map { name ->
    val (os, arch) = saxonBuild.targetHost.get()
    "${name}-${os}-${arch}-${saxonBuild.version.get()}"
})
val saxonCCoreCMakePackageBucket by configurations.dependencyScope("saxonCCoreCMakePackageBucket")
val saxonCCoreCMakePackageDir by configurations.resolvable("saxonCCoreCMakePackageDir") {
    extendsFrom(saxonCCoreCMakePackageBucket)
}

val phpReleaseFolderBucket by configurations.dependencyScope("phpReleaseFolderBucket")
val phpReleaseFolder by configurations.resolvable("phpReleaseFolder") {
    extendsFrom(phpReleaseFolderBucket)
}

project.layout.projectDirectory.dir("src/include")
val includeDir by configurations.consumable("includeDir")
val libDir by configurations.consumable("libDir")
val releaseArtifactsDir by configurations.consumable("releaseArtifactsDir")
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

val build_package = tasks.register<Exec>("cpack_package") {
    dependsOn(build)

    outputs.file(layout.buildDirectory.file(extension.package_file_name.map { "${it}.zip" }))

    group = LifecycleTasks.RELEASE_GROUP
    workingDir = layout.buildDirectory.get().asFile
    executable = "cpack"
    args("-C", "Release")
}

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
val releaseZipTask = tasks.register<Zip>("package") {
    group = LifecycleTasks.RELEASE_GROUP
    description = "generate the libsaxon download .zip package"

    inputs.files(build_package.map { it.outputs.files })
    inputs.files(notices)
    archiveBaseName =  extension.package_file_name
    destinationDirectory =  layout.buildDirectory.dir("out")

    from(zipTree(build_package.map { it.outputs.files.singleFile })) {
        into(extension.package_name)
        eachFile {
            // We want to extract the files from the cpack-created .zip and drop the top-level folder.
            // For infuriating reasons, the destination folder gets put on the front of the relative path
            // and we have to drop the second segment to achieve what we want.
            val rejiggeredPath = relativePath.segments.take(1) + relativePath.segments.drop(2)
            relativePath = RelativePath(true, *rejiggeredPath.toTypedArray())
        }
        includeEmptyDirs = false
    }
    into("notices") {
        from(notices)
    }
    into("php") {
        from(phpReleaseFolder.elements.map { it.first() })
    }
    into("samples") {
        from(layout.projectDirectory.dir("../samples"))
    }
}

tasks.named(LifecycleTasks.RELEASE_TASK_NAME) {
    dependsOn(releaseZipTask)
}

libDir.outgoing.artifact(releaseStagingPath.map { it.dir("lib") }) { builtBy(stage) }
includeDir.outgoing.artifact(releaseStagingPath.map { it.dir("include") }) { builtBy(stage) }
releaseArtifactsDir.outgoing.artifact(releaseStagingPath) { builtBy(stage) }
debugLibDir.outgoing.artifact(debugStagingPath.map { it.dir("lib") }) { builtBy(debugStage) }
debugIncludeDir.outgoing.artifact(debugStagingPath.map { it.dir("include") }) { builtBy(debugStage) }
debugArtifactsDir.outgoing.artifact(debugStagingPath) { builtBy(debugStage) }
