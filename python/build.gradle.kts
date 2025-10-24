import com.saxonica.build.AwsReleaseStagingResources
import com.saxonica.build.CheckStagedReleaseArtifactsPresent
import com.saxonica.build.TargetHost
import com.saxonica.build.python.PyenvExe
import com.saxonica.build.python.PyenvPythonVersion
import com.saxonica.build.python.PythonVersions
import com.saxonica.build.python.PythonVersion
import com.saxonica.build.AsInput
import com.saxonica.build.GatherReleaseArtifacts
import com.saxonica.build.UploadReleaseArtifactsToStagingBucket
import com.saxonica.build.python.PythonReleaseWheels
import org.gradle.kotlin.dsl.register
import kotlin.collections.joinToString

plugins {
  base
  id("saxon-build")
}

val _targetHost = TargetHost.current()
val pythonVersion: String by project
val saxonBuild = the<SaxonBuildExtension>()
val extension = project.extensions.create<PyenvExtension>("pyenv")
val pythonVersions = PythonVersions(layout.projectDirectory.file("python-versions.json").asFile)

extension.devVersion = AsInput.from<PythonVersion>(pythonVersions.findSupported(pythonVersion))
extension.pythonVersions = pythonVersions
extension.debugVariant = "valgrind"
extension.definitionsDir = layout.projectDirectory.dir("pyenv")
extension.pyenvRoot = layout.projectDirectory.dir(".pyenv")

val pyenvExeTask = tasks.register<PyenvExe>("pyenv_exe")

val pyenvDevValgrindPythonTask = tasks.register<PyenvPythonVersion>("pyenv_dev_valgrind_python") {
  targetHost = _targetHost
  pyenvExe = pyenvExeTask.map { it.pyenv.get() }
  pyenvRoot = extension.pyenvRoot.map { it.asFile.toString() }
  pythonVersion = extension.devVersion
  pythonBuildDefinitionsDir = extension.definitionsDir
  debugVariant = extension.debugVariant
}

val devDebugVersionConfig by configurations.consumable("debugPython")
devDebugVersionConfig.outgoing.artifact(pyenvDevValgrindPythonTask) { builtBy(pyenvDevValgrindPythonTask) }

extension.pythonVersions.get().supported.forEach { python ->
  val v = python.version
  val versionConfig = configurations.consumable("python${v}").get()

  val versionTask = tasks.register<PyenvPythonVersion>("pyenv_python_${v}") {
    targetHost = _targetHost
    pyenvExe = pyenvExeTask.map { it.pyenv.get() }
    pyenvRoot = extension.pyenvRoot.map { it.asFile.toString() }
    pythonVersion = AsInput.from<PythonVersion>(python)
    pythonBuildDefinitionsDir = extension.definitionsDir
  }

  versionConfig.outgoing.artifact(versionTask) { builtBy(versionTask) }
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
