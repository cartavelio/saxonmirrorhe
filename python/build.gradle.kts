import com.saxonica.build.AwsReleaseStagingResources
import com.saxonica.build.AwsSigningResources
import com.saxonica.build.CheckStagedReleaseArtifactsPresent
import com.saxonica.build.python.PyenvExe
import com.saxonica.build.python.PyenvPythonVersion
import org.gradle.internal.os.OperatingSystem
import com.saxonica.build.GatherReleaseArtifacts
import com.saxonica.build.UnsignedArtifactUploadTask
import com.saxonica.build.UploadReleaseArtifactsToStagingBucket
import com.saxonica.build.python.PythonReleaseWheels
import org.gradle.kotlin.dsl.assign
import org.gradle.kotlin.dsl.register
import kotlin.collections.joinToString

plugins {
  base
  id("saxon-build")
}

val systemOs = OperatingSystem.current()
val pythonVersion: String by project
val pythonReleaseVersions: String by project
val saxonBuild = the<SaxonBuildExtension>()
val extension = project.extensions.create<PyenvExtension>("pyenv")

extension.devVersion = pythonVersion
extension.releaseVersions = pythonReleaseVersions.split(",")
extension.debugVariant = "valgrind"
extension.definitionsDir = layout.projectDirectory.dir("pyenv")
extension.pyenvRoot = layout.projectDirectory.dir(".pyenv")

val pyenvExeTask = tasks.register<PyenvExe>("pyenv_exe")

val pyenvDevPythonTask = tasks.register<PyenvPythonVersion>("pyenv_dev_python") {
  os = systemOs
  pyenvExe = pyenvExeTask.map { it.pyenv.get() }
  pyenvRoot = extension.pyenvRoot.map { it.asFile.toString() }
  pythonVersion = extension.devVersion
  pythonBuildDefinitionsDir = extension.definitionsDir
  macosxDeploymentTarget = saxonBuild.macosx_min_version
}

val pyenvDevValgrindPythonTask = tasks.register<PyenvPythonVersion>("pyenv_dev_valgrind_python") {
  os = systemOs
  pyenvExe = pyenvExeTask.map { it.pyenv.get() }
  pyenvRoot = extension.pyenvRoot.map { it.asFile.toString() }
  pythonVersion = extension.devVersion
  pythonBuildDefinitionsDir = extension.definitionsDir
  debugVariant = extension.debugVariant
  macosxDeploymentTarget = saxonBuild.macosx_min_version
}

val devVersionConfig by configurations.consumable("python")
devVersionConfig.outgoing.artifact(pyenvDevPythonTask) { builtBy(pyenvDevPythonTask) }
val devDebugVersionConfig by configurations.consumable("debugPython")
devDebugVersionConfig.outgoing.artifact(pyenvDevValgrindPythonTask) { builtBy(pyenvDevValgrindPythonTask) }

extension.releaseVersions.get().forEach { releaseVersion ->
  val versionConfig = configurations.consumable("python${releaseVersion}").get()

  val versionTask = tasks.register<PyenvPythonVersion>("pyenv_python_${releaseVersion}") {
    os = systemOs
    pyenvExe = pyenvExeTask.map { it.pyenv.get() }
    pyenvRoot = extension.pyenvRoot.map { it.asFile.toString() }
    pythonVersion = releaseVersion
    pythonBuildDefinitionsDir = extension.definitionsDir
    macosxDeploymentTarget = saxonBuild.macosx_min_version
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

val macosx_min_version_aarch64: String by project
val macosx_min_version_x86_64: String by project
val manylinuxTarget: String by project

val expectedReleaseArtifacts = tasks.register("expected_release_artifacts") {
  doLast {
    val wheels = PythonReleaseWheels(saxonBuild.version.get(), manylinuxTarget, macosx_min_version_aarch64, macosx_min_version_x86_64, extension.releaseVersions.get())
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

  expectedArtifacts = saxonBuild.version.map { version ->
    PythonReleaseWheels(version, manylinuxTarget, macosx_min_version_aarch64, macosx_min_version_x86_64, extension.releaseVersions.get()).artifacts()
  }
}