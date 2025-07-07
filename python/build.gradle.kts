import com.saxonica.build.python.PyenvExe
import com.saxonica.build.python.PyenvPythonVersion
import com.saxonica.build.python.PythonPyenvPythonExe

plugins {
  base
  id("saxon-build")
}

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
  pyenvExe = pyenvExeTask.map { it.pyenv.get() }
  pyenvRoot = extension.pyenvRoot.map { it.asFile.toString() }
  pythonVersion = extension.devVersion
  pythonBuildDefinitionsDir = extension.definitionsDir
  macosxDeploymentTarget = saxonBuild.macosx_min_version
}

val pyenvDevValgrindPythonTask = tasks.register<PyenvPythonVersion>("pyenv_dev_valgrind_python") {
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
    pyenvExe = pyenvExeTask.map { it.pyenv.get() }
    pyenvRoot = extension.pyenvRoot.map { it.asFile.toString() }
    pythonVersion = releaseVersion
    pythonBuildDefinitionsDir = extension.definitionsDir
    macosxDeploymentTarget = saxonBuild.macosx_min_version
  }

  versionConfig.outgoing.artifact(versionTask) { builtBy(versionTask) }
}
