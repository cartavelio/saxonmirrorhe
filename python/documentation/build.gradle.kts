import com.saxonica.build.python.*
import com.saxonica.build.xslt.XsltOnHtmlTask

plugins {
    id("saxon-build")
    id("com.saxonica.build.managebranches")
    id("saxonc-python")
}

val saxonBuild = the<SaxonBuildExtension>()

val pydocArtifact by configurations.consumable("pydocArtifact")

saxoncPython {
    edition = "ee"
    version = "3.9"
}

val devPythonExe by configurations.named<ResolvableConfiguration>(saxoncPython.version.map { v -> "python${v}" }.get())

val venv = tasks.named<PythonVenv>("venv") {
    pyenvPythonExe = devPythonExe.elements.map { project.file(it.single().asFile.readText()) }
}
val installWheelTask = tasks.named<InstallLocalWheel>("install_wheel")

val pydocTask = tasks.register<Exec>("raw_pydoc") {
    dependsOn(installWheelTask)
    inputs.file(venv.map { it.pythonExe.get() })
    inputs.dir(layout.projectDirectory.dir("../saxonc"))
    outputs.file(layout.buildDirectory.file("saxoncee.html"))

    workingDir(layout.buildDirectory.get())
    executable(venv.flatMap { it.pythonExe }.get())
    args("-m", "pydoc", "-w", "saxoncee")
}

val cleanedPydocTask = tasks.register<XsltOnHtmlTask>("pydoc") {
    xslt = layout.projectDirectory.dir("xslt").file("clean-pydoc.xsl")

    input = pydocTask.map { it.outputs.files.singleFile }
    output = input.map { f -> layout.buildDirectory.file("cleaned-pydoc/${f.asFile.name}").get() }
}

pydocArtifact.outgoing.artifact(cleanedPydocTask) { builtBy(cleanedPydocTask) }
