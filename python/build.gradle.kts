import com.saxonica.build.AwsReleaseStagingResources
import com.saxonica.build.CheckStagedReleaseArtifactsPresent
import com.saxonica.build.TargetHost
import com.saxonica.build.python.PyenvExe
import com.saxonica.build.python.InstallPyenvPythonVersionTask
import com.saxonica.build.python.PythonVersions
import com.saxonica.build.python.PythonVersion
import com.saxonica.build.AsInput
import com.saxonica.build.GatherReleaseArtifacts
import com.saxonica.build.UploadReleaseArtifactsToStagingBucket
import com.saxonica.build.python.PyenvPythonVersionTask
import com.saxonica.build.python.PyenvUninstallPythonTask
import com.saxonica.build.python.PythonReleaseWheels
import com.saxonica.build.python.PythonVersionTask
import com.saxonica.build.python.WinPythonVersionInstallTask
import org.gradle.kotlin.dsl.invoke
import org.gradle.kotlin.dsl.register
import kotlin.collections.joinToString

plugins {
  base
  id("saxon-build")
}

val _targetHost = TargetHost.current()
val pythonVersion: String by project
val saxonBuild = the<SaxonBuildExtension>()
val extension = project.extensions.create<PythonVersionsExtension>("pythonVersions")
val pythonVersions = PythonVersions(layout.projectDirectory.file("python-versions.json").asFile)
val devVersion = pythonVersions.findSupported(pythonVersion)

extension.devVersion = AsInput.from<PythonVersion>(devVersion)
extension.pythonVersions = pythonVersions
extension.debugVariant = "valgrind"
extension.definitionsDir = layout.projectDirectory.dir("pyenv")
extension.installRoot = layout.projectDirectory.dir(".python-versions")

inline fun <reified T : PythonVersionTask> createPythonVersionTask(name: String, py: PythonVersion, th: TargetHost, publish: ConsumableConfiguration): TaskProvider<T> {
  val installTask = tasks.register<T>(name) {
    targetHost = th
    pythonVersionsRoot = extension.installRoot
    this.pythonVersion = AsInput.from<PythonVersion>(py)
  }
  publish.outgoing.artifact(installTask) { builtBy(installTask) }

  return installTask
}

inline fun <reified T : PyenvPythonVersionTask> createUninstallTask(name: String, installTask: TaskProvider<T>, py: PythonVersion, action: Action<PyenvPythonVersionTask>) {
  tasks.register<PyenvUninstallPythonTask>("uninstall_${name}") {
    description = "Remove ${name}"
    pythonExePathFile = installTask.map { it.outputs.files.singleFile }
    pythonVersionsRoot = extension.installRoot
    pythonVersion = AsInput.from<PythonVersion>(py)
    action(this)
  }
}

inline fun <reified T : PyenvPythonVersionTask> createPyenvPythonVersionTask(name: String, py: PythonVersion, th: TargetHost, publish: ConsumableConfiguration, action: Action<PyenvPythonVersionTask>) {
  val installTask = createPythonVersionTask<T>(name, py, th, publish)

  val _pythonExePathFile = layout.buildDirectory.map { it.file("python-${py.version}") }
  installTask.configure {
    action(this)
    pythonExePathFile = _pythonExePathFile
  }

  tasks.register<PyenvUninstallPythonTask>("uninstall_${name}") {
    description = "Remove ${name}"
    pythonExePathFile = installTask.map { it.outputs.files.singleFile }
    pythonVersionsRoot = extension.installRoot
    pythonVersion = AsInput.from<PythonVersion>(py)
    pythonExePathFile = _pythonExePathFile
    action(this)
  }
}

inline fun createPythonVersionTasks(th: TargetHost, builder: (String, PythonVersion, TargetHost, ConsumableConfiguration) -> Unit) {
  extension.pythonVersions.get().supported.forEach { python ->
    val v = python.version
    val versionConfig = configurations.consumable("python${v}").get()

    builder("pyenv_python_${v}", python, th, versionConfig)
  }
}

inline fun <reified T : PyenvPythonVersionTask> createPyenvPythonVersionTasks(th: TargetHost, action: Action<PyenvPythonVersionTask>) {
  createPythonVersionTasks(th) { name, python, th, publish ->
    createPyenvPythonVersionTask<T>(name, python, th, publish, action)
  }
}

val devDebugVersionConfig by configurations.consumable("debugPython")

if (!_targetHost.isWindows) {
  val pyenvExeTask = tasks.register<PyenvExe>("pyenv_exe")
  createPyenvPythonVersionTasks<InstallPyenvPythonVersionTask>(_targetHost) {
    pyenvExe = pyenvExeTask.map { it.pyenv.get() }
    pythonBuildDefinitionsDir = extension.definitionsDir
  }

  createPyenvPythonVersionTask<InstallPyenvPythonVersionTask>("pyenv_dev_valgrind_python", devVersion, _targetHost, devDebugVersionConfig) {
    pyenvExe = pyenvExeTask.map { it.pyenv.get() }
    pythonBuildDefinitionsDir = extension.definitionsDir
    debugVariant = extension.debugVariant
  }
} else {
  createPythonVersionTasks(_targetHost) { name, python, th, publish ->
    createPythonVersionTask<WinPythonVersionInstallTask>(name, python, _targetHost, publish)
  }

}

val heReleaseWheelsBucket by configurations.dependencyScope("heReleaseWheelsBucket")
val heReleaseWheels by configurations.resolvable("heReleaseWheels") {
  extendsFrom(heReleaseWheelsBucket)
}
val peReleaseWheelsBucket by configurations.dependencyScope("peReleaseWheelsBucket")
val peReleaseWheels by configurations.resolvable("peReleaseWheels") {
  extendsFrom(peReleaseWheelsBucket)
}
val eeReleaseWheelsBucket by configurations.dependencyScope("eeReleaseWheelsBucket")
val eeReleaseWheels by configurations.resolvable("eeReleaseWheels") {
  extendsFrom(eeReleaseWheelsBucket)
}

dependencies {
  heReleaseWheelsBucket(project(path = ":python:he", configuration = "releaseWhl"))
  peReleaseWheelsBucket(project(path = ":python:pe", configuration = "releaseWhl"))
  eeReleaseWheelsBucket(project(path = ":python:ee", configuration = "releaseWhl"))
}

val gather = tasks.register<GatherReleaseArtifacts>("gather_release_artifacts") {
  artifacts.from(heReleaseWheels)
  artifacts.from(peReleaseWheels)
  artifacts.from(eeReleaseWheels)
}

val manylinuxTarget: String by project

val expectedReleaseArtifacts = tasks.register("expected_release_artifacts") {
  doLast {
    val wheels = PythonReleaseWheels(saxonBuild.version.get(), manylinuxTarget, extension.pythonVersions.get().released)
    println(wheels.artifacts().joinToString("\n"))
  }
}

val stagingResources = tasks.register<AwsReleaseStagingResources>("staging_resources") {
  region = "eu-west-2"
  stackName = "SaxonicaCiCdStack"
}

val upload = tasks.register<UploadReleaseArtifactsToStagingBucket>("build_and_stage_release_artifacts") {
  bucket = stagingResources.map { it.releaseArtifactsStagingBucket.get() }
  region = stagingResources.map { it.region.get() }

  artifacts.from(heReleaseWheels)
  artifacts.from(peReleaseWheels)
  artifacts.from(eeReleaseWheels)
}

val checkReleaseArtifactsStaged = tasks.register<CheckStagedReleaseArtifactsPresent>("check_release_artifacts_staged") {
  bucket = stagingResources.map { it.releaseArtifactsStagingBucket.get() }
  region = stagingResources.map { it.region.get() }

  expectedArtifacts = saxonBuild.version.map { saxonVersion ->
    PythonReleaseWheels(saxonVersion, manylinuxTarget,
      extension.pythonVersions.map { it.released }.get()
    ).artifacts()
  }
}
