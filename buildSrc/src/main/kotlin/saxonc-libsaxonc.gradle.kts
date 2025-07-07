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
val saxonCCoreCMakePackageBucket by configurations.dependencyScope("saxonCCoreCMakePackageBucket")

val saxonCCoreCMakePackageDir by configurations.resolvable("saxonCCoreCMakePackageDir") {
    extendsFrom(saxonCCoreCMakePackageBucket)
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
}

val generator = tasks.register("cmake_generate") {
    group = LifecycleBasePlugin.BUILD_GROUP
    inputs.file(layout.projectDirectory.file("CMakeLists.txt"))
    inputs.files(layout.projectDirectory.dir("../cmake"))
    inputs.dir(saxonCCoreCMakePackageDir.elements.map { it.first().asFile })
    outputs.file(layout.buildDirectory.file("CMakeCache.txt"))

    doLast {
        println("saxonCCoreCMakePackageDir: ${saxonCCoreCMakePackageDir.elements.get()}")
        val generatorName = if (saxonBuild.platform.get() == "windows") "Visual Studio 17 2022" else "Ninja Multi-Config"
        project.exec {
            workingDir = layout.projectDirectory.asFile
            executable = "cmake"
            args("-G", generatorName, "-B", layout.buildDirectory.get().asFile.toString(),
                "-D", saxonBuild.version.map { "SAXON_VERSION:STRING=${it}" }.get(),
                "-D", saxonCCoreCMakePackageDir.elements.map { "SaxonCCore_PackageDir:PATH=${it.first().asFile}" }.get()
            )
        }
    }
}

val copyIncludes = tasks.register<Copy>("cpp_includes") {
    into(layout.buildDirectory.dir("include"))
    from(layout.projectDirectory.dir("../src/include"))
    from(layout.projectDirectory.dir("src/include"))
}

val build = tasks.register<Exec>("build_release") {
    group = LifecycleBasePlugin.BUILD_GROUP
    inputs.files(generator)
    inputs.files(layout.projectDirectory.dir("../src"))
    inputs.files(layout.projectDirectory.dir("src"))
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
    inputs.files(build)
    outputs.dir(releaseStagingPath)

    workingDir = layout.projectDirectory.asFile
    executable = "cmake"
    args("--install", layout.buildDirectory.get().asFile.toString(), "--config", "Release", "--prefix", releaseStagingPath.get().asFile.toString())
}

val debugStagingPath = layout.buildDirectory.dir("staging/Debug")
val debugStage = tasks.register<Exec>("debug_staging_install") {
    inputs.files(buildDebug)
    outputs.dir(debugStagingPath)

    workingDir = layout.projectDirectory.asFile
    executable = "cmake"
    args("--install", layout.buildDirectory.get().asFile.toString(), "--config", "Debug", "--prefix", debugStagingPath.get().asFile.toString())
}

val test = tasks.register<Exec>("test") {
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

val build_package = tasks.register<Exec>("package") {
    dependsOn(build)
    group = LifecycleTasks.RELEASE_GROUP
    workingDir = layout.buildDirectory.get().asFile
    executable = "cpack"
    args("-C", "Release")
}

tasks.named(LifecycleTasks.RELEASE_TASK_NAME) {
    dependsOn(build_package)
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
    logo = layout.projectDirectory.file("../../docs/src/doc/img/saxonica_logo.gif")
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

libDir.outgoing.artifact(releaseStagingPath.map { it.dir("lib") }) { builtBy(stage) }
includeDir.outgoing.artifact(releaseStagingPath.map { it.dir("include") }) { builtBy(stage) }
releaseArtifactsDir.outgoing.artifact(releaseStagingPath) { builtBy(stage) }
debugLibDir.outgoing.artifact(debugStagingPath.map { it.dir("lib") }) { builtBy(debugStage) }
debugIncludeDir.outgoing.artifact(debugStagingPath.map { it.dir("include") }) { builtBy(debugStage) }
debugArtifactsDir.outgoing.artifact(debugStagingPath) { builtBy(debugStage) }
