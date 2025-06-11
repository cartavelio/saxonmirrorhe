import com.saxonica.build.python.*
import org.gradle.internal.os.OperatingSystem
import org.gradle.kotlin.dsl.create

plugins {
    base
    id("saxon-build")
}


val extension = project.extensions.create<SaxonCPythonExtension>("saxoncPython")
val manageBranches = the<ManageBranchesExtension>()
val saxonBuild = the<SaxonBuildExtension>()

val systemOs = OperatingSystem.current()
val unixish = systemOs.isMacOsX() || systemOs.isLinux()
val venvBaseDir = project.layout.buildDirectory.dir("pyvenv")
val pythonVersion: String by project

extension.pythonBinDirName.convention(if (unixish) "bin" else "scripts")
extension.venvBinarySuffix.convention(if (unixish) "" else ".exe")
extension.venvBaseDir.convention(venvBaseDir)
extension.venvBasePath.convention(venvBaseDir.map { it.asFile.toString() })
extension.version.convention(pythonVersion)
extension.manylinuxTarget.convention("manylinux_2_24")

val saxoncCoreReleaseArtifactsBucket by configurations.dependencyScope("saxoncCoreReleaseArtifactsBucket")
val saxoncCoreReleaseArtifactsDir by configurations.resolvable("saxoncCoreReleaseArtifactsDir") {
    extendsFrom(saxoncCoreReleaseArtifactsBucket)
}
val libsaxoncDebugArtifactsBucket by configurations.dependencyScope("libsaxoncDebugArtifactsBucket")
val libsaxoncDebugArtifactsDir by configurations.resolvable("libsaxoncDebugArtifactsDir") {
    extendsFrom(libsaxoncDebugArtifactsBucket)
}
val saxonCSampleDataBucket by configurations.dependencyScope("saxonCSampleDataBucket")
val saxonCSampleData by configurations.resolvable("saxonCSampleData") {
    extendsFrom(saxonCSampleDataBucket)
}

dependencies {
    saxoncCoreReleaseArtifactsBucket(extension.edition.map { edition ->
        project(path = ":libsaxonc:${edition}", configuration = "releaseArtifactsDir")
    })
    libsaxoncDebugArtifactsBucket(extension.edition.map { edition ->
        project(path = ":libsaxonc:${edition}", configuration = "debugArtifactsDir")
    })
    saxonCSampleDataBucket(extension.edition.map { edition ->
        project(path = ":libsaxonc:${edition}", configuration = "sampleData")
    })
}

val pyenvExeTask = tasks.register<PyenvExe>("pyenv_exe")

val pyenvTask = tasks.register<PythonPyenv>("pyenv") {
    pyenvExe = pyenvExeTask.map { it.pyenv.get() }
    pythonVersion = extension.version
    pythonBuildDefinitionsDir = layout.projectDirectory.dir("../pyenv")
}

val pyenvValgrindTask = tasks.register<PythonPyenv>("pyenv_valgrind") {
    pyenvExe = pyenvExeTask.map { it.pyenv.get() }
    pythonVersion = extension.version
    pythonBuildDefinitionsDir = layout.projectDirectory.dir("../pyenv")
    debugVariant = "valgrind"
}

val pyenvPythonExeTask = tasks.register<PythonPyenvPythonExe>("pyenv_python_exe") {
    pyenvExe = pyenvExeTask.map { it.pyenv.get() }
    pyenvVersionName = pyenvTask.map { it.pyenvVersionName.get() }
}

val pyenvPythonValgrindExeTask = tasks.register<PythonPyenvPythonExe>("pyenv_valgrind_python_exe") {
    pyenvExe = pyenvExeTask.map { it.pyenv.get() }
    pyenvVersionName = pyenvValgrindTask.map { it.pyenvVersionName.get() }
}

val pythonSrcDir = layout.projectDirectory.dir(provider { "../saxonc" })

val venv = tasks.register<PythonVenv>("venv") {
    pythonEnv = extension
    branch = manageBranches.currentBranch
    pyenvPythonExe = pyenvPythonExeTask.map { it.pythonExe.get() }
    buildSystemBootstrap = pythonSrcDir.map { it.file("pyproject.toml") }
}

val debugVenv = tasks.register<PythonVenv>("debug_venv") {
    pyenvPythonExe = pyenvPythonValgrindExeTask.map { it.pythonExe.get() }
    pythonEnv = extension
    branch = manageBranches.currentBranch
    suffix = "debug"
}

val wheelPlatformTagTask = tasks.register<WheelCompatibilityTags>("wheel_platform_tag") {
    pyenvExe = pyenvExeTask.map { it.pyenv.get() }
    pythonVersion = pyenvTask.map { it.pythonVersion.get() }
    pythonExe = venv.map { it.pythonExe.get() }
    manylinuxTarget = extension.manylinuxTarget
}

val wheelDebugPlatformTagTask = tasks.register<WheelCompatibilityTags>("wheel_debug_platform_tag") {
    pyenvExe = pyenvExeTask.map { it.pyenv.get() }
    pythonVersion = pyenvTask.map { it.pythonVersion.get() }
    pythonExe = debugVenv.map { it.pythonExe.get() }
    manylinuxTarget = extension.manylinuxTarget
}

val pipInstall = project.tasks.register<PipInstall>("pip_install") {
    pipExe = venv.map { it.pipExe.get() }
    requirements = layout.projectDirectory.dir("../saxonc").file("requirements.txt")
}

val conf = this

val buildWheel = project.tasks.register<BuildWheel>("build_wheel") {
    group = LifecycleBasePlugin.BUILD_GROUP
    description = "Build the release wheel"
    pythonExe = venv.map { it.pythonExe.get() }
    productEdition = extension.edition
    productVersion = saxonBuild.version
    wheelSuffix = wheelPlatformTagTask.flatMap { it.wheelSuffix }
    macosxDeploymentTarget = wheelPlatformTagTask.flatMap { it.macosxDeploymentTarget }
    sourceDir = pythonSrcDir
    saxoncCoreDir = layout.dir(conf.saxoncCoreReleaseArtifactsDir.elements.map { it.first().asFile })
    outputDir = layout.buildDirectory.dir("wheel")
}

fun repairWheelTask(name: String, platformTagTask: TaskProvider<WheelCompatibilityTags>): TaskProvider<RepairLocalWheel> {
    val libLocations: List<String> = if (systemOs.isWindows) listOf("lib", "bin") else listOf("lib")
//    val libDirs = provider<List<File>> {
//        val saxoncCoreDir = layout.dir(conf.saxoncCoreReleaseArtifactsDir.elements.map { it.first().asFile }).get()
//        libLocations.map { dirname ->
//            saxoncCoreDir.dir(dirname).asFile
//        }
//    }
    val libDirs = conf.saxoncCoreReleaseArtifactsDir.elements.map { dirs ->
        project.files(libLocations.map { dirname ->
            layout.projectDirectory.dir(dirs.first().asFile.toString()).dir(dirname)
        })
    }

//        val saxoncCoreDir = layout.dir(conf.saxoncCoreReleaseArtifactsDir.elements.map { it.first().asFile }).get()
//        libLocations.map { dirname ->
//            saxoncCoreDir.dir(dirname).asFile
//        }
//    }


    return tasks.register<RepairLocalWheel>(name) {
        group = LifecycleBasePlugin.BUILD_GROUP
        manylinuxTarget = platformTagTask.flatMap { it.manylinuxTarget }
        pythonExe = venv.map { it.pythonExe.get() }
        pythonVenvTask = venv
        productEdition = extension.edition
        systemOsArch = saxonBuild.systemOsArch
        repairedWheelFilePlatformSuffix = platformTagTask.flatMap { it.repairedWheelSuffix }
        libraryDirs.from(libDirs)
    }
}

val repairWheel = repairWheelTask("repair_wheel", wheelPlatformTagTask)
repairWheel.configure {
    description = "Fix up the shared libs within the release wheel"
    outputDir = layout.buildDirectory.dir("repaired-wheel")
    wheel = buildWheel.map { it.wheel.get() }
}

val installWheel = tasks.register<InstallLocalWheel>("install_wheel") {
    group = LifecycleBasePlugin.BUILD_GROUP
    description = "Install the release wheel into the branch's VENV"
    pipExe = venv.map { it.pipExe.get() }
    wheel = repairWheel.map { it.repairedWheel.get() }
}

val buildDebugWheel = project.tasks.register<BuildWheel>("build_debug_wheel") {
    group = LifecycleBasePlugin.BUILD_GROUP
    description = "Build the debug wheel"
    pythonExe = venv.map { it.pythonExe.get() }
    productEdition = extension.edition
    productVersion = saxonBuild.version
    wheelSuffix = wheelPlatformTagTask.flatMap { it.wheelSuffix }
    macosxDeploymentTarget = wheelPlatformTagTask.flatMap { it.macosxDeploymentTarget }
    sourceDir = pythonSrcDir
    saxoncCoreDir = layout.dir(conf.libsaxoncDebugArtifactsDir.elements.map { it.first().asFile })
    outputDir = layout.buildDirectory.dir("wheel")
    debug = true
}

val repairDebugWheel = repairWheelTask("repair_debug_wheel", wheelPlatformTagTask)
repairDebugWheel.configure {
    description = "Fix up the shared libs within the debug wheel"
    outputDir = layout.buildDirectory.dir("repaired-debug-wheel")
    wheel = buildDebugWheel.map { it.wheel.get() }
}

val installDebugWheel = tasks.register<InstallLocalWheel>("install_debug_wheel") {
    group = LifecycleBasePlugin.BUILD_GROUP
    description = "Install the debug wheel into the branch's VENV"
    pipExe = venv.map { it.pipExe.get() }
    wheel = repairDebugWheel.map { it.repairedWheel.get() }
}

val pyTestSamples = tasks.register<Copy>("pytest_data") {
    from(saxonCSampleData)
    into(layout.buildDirectory.dir("samples/data"))
}
val pyTestData = pyTestSamples.map { it.destinationDir }

val testWheelPlatformTagTask = tasks.register<WheelCompatibilityTags>("test_wheel_platform_tag") {
    pyenvExe = pyenvExeTask.map { it.pyenv.get() }
    pythonVersion = pyenvTask.map { it.pythonVersion.get() }
    pythonExe = venv.map { it.pythonExe.get() }
    manylinuxTarget = "linux"
}

val repairTestWheel = repairWheelTask("repair_test_wheel", testWheelPlatformTagTask)
repairTestWheel.configure {
    group = LifecycleBasePlugin.VERIFICATION_GROUP
    description = "Repair the test wheel into the branch's VENV"
    outputDir = layout.buildDirectory.dir("repaired-test-wheel")
    wheel = buildWheel.map { it.wheel.get() }
}

val installTestWheel = tasks.register<InstallLocalWheel>("install_test_wheel") {
    group = LifecycleBasePlugin.VERIFICATION_GROUP
    pipExe = venv.map { it.pipExe.get() }
    wheel = repairTestWheel.map { it.repairedWheel.get() }
}

val pytestCopy = tasks.register<Copy>("pytest_copy") {
    from(layout.projectDirectory.dir("../saxonc/test"))
    into(layout.buildDirectory.dir("pytest"))
}
val pytestCopyDir = pytestCopy.map { it.destinationDir }

val pyTest = tasks.register<PytestRunner>("pytest") {
    group = LifecycleBasePlugin.VERIFICATION_GROUP
    description = "Run unit tests with pytest"
    inputs.files(pyTestSamples)
    inputs.dir(pytestCopyDir)
    dependsOn(installTestWheel)
    dependsOn(pipInstall)

    pytest = venv.map { it.venvBinHostedExe("pytest").get() }

    workingDir = layout.projectDirectory.dir("../saxonc")
    environment("SAXONC_HOME" to saxonBuild.saxonLicenseDir, "SAXONC_EDITION" to extension.edition)

    baseArgs("--import-mode=importlib", "--data-dir", pyTestData)
    tests("test")//, "tests")
}

tasks.named(LifecycleBasePlugin.CHECK_TASK_NAME) {
    dependsOn(pyTest)
}