import org.gradle.api.Plugin
import org.gradle.api.Project
import org.gradle.api.provider.Property
import org.gradle.internal.os.OperatingSystem

interface ManagePythonEnvExtension {
    Property<String> getVenvDirPath()
    Property<String> getSystemPythonBinary()
    Property<String> getVenvPip()
    Property<String> getVenvPython()
}

class ManagePythonEnvPlugin implements Plugin<Project> {
    void apply(Project project) {
        def extension = project.extensions.create('pythonEnv', ManagePythonEnvExtension)
        OperatingSystem systemOs = OperatingSystem.current()

        def sysPythonBinary = "python"
        if(systemOs.isMacOsX() || systemOs.isLinux()) {
            sysPythonBinary = "python3"
        }
        def pythonBinFolder = "Scripts"
        if(systemOs.isMacOsX() || systemOs.isLinux()) {
            pythonBinFolder = "bin"
        }

        def venvDir = project.branches.current.map { "py_venvs/${it}" }
        extension.venvDirPath.convention(project.layout.buildDirectory.file(venvDir).map { it.toString() })

        extension.systemPythonBinary.convention(sysPythonBinary)
        extension.venvPip.convention(project.layout.buildDirectory.dir(venvDir).map { it.dir(pythonBinFolder) }.map { it.file("pip") }.map { it.toString() })
        extension.venvPython.convention(project.layout.buildDirectory.dir(venvDir).map { it.dir(pythonBinFolder) }.map { it.file("python") }.map { it.toString() })
    }
}