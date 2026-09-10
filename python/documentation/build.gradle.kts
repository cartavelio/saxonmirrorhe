import com.saxonica.build.python.*

plugins {
    id("saxon-build")
    id("com.saxonica.build.managebranches")
    id("saxonc-python")
}

val saxonBuild = the<SaxonBuildExtension>()

val doxygenArtifact by configurations.consumable("doxygenArtifact")

saxoncPython {
    edition = "ee"
    version = "3.9.19"
}

val venv = tasks.named<PythonVenv>("venv")
val installWheelTask = tasks.named<InstallLocalWheel>("install_wheel")

val pydocTask = tasks.register("pydoc") {
    dependsOn(installWheelTask)
    inputs.file(venv.map { it.pythonExe.get() })
    inputs.dir(layout.projectDirectory.dir("../saxonc"))
    outputs.file(layout.buildDirectory.file("saxoncee.html"))

    doLast {
        exec {
            workingDir(layout.buildDirectory.get())
            executable(venv.flatMap { it.pythonExe }.get())
            args("-m", "pydoc", "-w", "saxoncee")
        }
    }
}

doxygenArtifact.outgoing.artifact(pydocTask)
