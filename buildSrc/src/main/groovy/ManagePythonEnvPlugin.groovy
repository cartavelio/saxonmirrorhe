import org.gradle.api.Plugin
import org.gradle.api.Project
import org.gradle.api.file.Directory
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.internal.os.OperatingSystem

interface ManagePythonEnvExtension {
    Property<String> getVenvDirPath()
    Property<String> getSystemPythonBinary()
    Property<String> getVenvPip()
    Property<String> getVenvPython()
    Property<String> getVenvBinPath()
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
        def venvBinDir = venvDir.map { "${it}/${pythonBinFolder}" }
        def venvBinDirectoryProvider = project.layout.buildDirectory.dir(venvBinDir)

        extension.systemPythonBinary.convention(sysPythonBinary)
        extension.venvDirPath.convention(project.layout.buildDirectory.file(venvDir).map { it.toString() })
        extension.venvBinPath.convention(venvBinDirectoryProvider.map { it.asFile.toString() })
        extension.venvPip.convention(venvBinDirectoryProvider.map { it.file("pip") }.map { it.toString() })
        extension.venvPython.convention(venvBinDirectoryProvider.map { it.file("python") }.map { it.toString() })
    }
}