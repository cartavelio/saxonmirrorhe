import org.gradle.api.DefaultTask
import org.gradle.api.Plugin
import org.gradle.api.Project
import org.gradle.api.file.ConfigurableFileCollection
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.RegularFile
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.provider.MapProperty
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.Input
import org.gradle.api.tasks.InputFile
import org.gradle.api.tasks.Internal
import org.gradle.api.tasks.OutputFile
import org.gradle.api.tasks.TaskAction
import org.gradle.internal.os.OperatingSystem

interface ManagePythonEnvExtension {
    Property<String> getSystemPythonBinary()
    Property<String> getVenvPath()
    Property<String> getVenvPipPath()
    Property<String> getVenvPythonPath()
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

        def venvDir = project.layout.buildDirectory.dir(project.branches.current.map { "py_venvs/${it}" })
        def venvBinDir = venvDir.map { it.dir(pythonBinFolder) }

        extension.systemPythonBinary.convention(sysPythonBinary)
        extension.venvPath.convention(venvDir.map { it.asFile.toString() })
        extension.venvBinPath.convention(venvBinDir.map { it.asFile.toString() })
        extension.venvPipPath.convention(venvBinDir.map { it.file("pip").asFile.toString() })
        extension.venvPythonPath.convention(venvBinDir.map { it.file("python").asFile.toString() })
    }
}

abstract class PythonVenv extends DefaultTask {
    @Input
    abstract Property<String> getBranchesExtension()

    @Internal
    final Provider<String> branch = branchesExtension.map { project.extensions.getByName(it).properties.get("current").get() }

    @OutputFile
    final Provider<RegularFile> pythonExe = project.layout.file(project.pythonEnv.venvPythonPath.map { project.file(it) })

    @OutputFile
    final Provider<RegularFile> pipExe = project.layout.file(project.pythonEnv.venvPipPath.map { project.file(it) })

    Provider<RegularFile> venvBinHostedExe(String exe) {
        project.layout.dir(project.pythonEnv.venvBinPath.map { project.file(it) }).map { it.file(exe) }
    }

    PythonVenv() {
        branchesExtension.convention("branches")
    }

    @TaskAction
    void execute() {
        logger.info("Creating Python Venv for ${branch.get()}")

        project.exec {
            executable project.pythonEnv.systemPythonBinary.get()
            args "-m", "venv", project.pythonEnv.venvPath.get()
        }
    }
}
/**
 * Task for installing packages from a Pip requirements.txt.
 * Note that the lack of outputs for this task means it will always run, which it needs to in order
 * to verify the state of installed packages.
 */
abstract class PipInstall extends DefaultTask {
    @InputFile
    abstract RegularFileProperty getPipExe()

    @Input
    abstract RegularFileProperty getRequirements()

    @TaskAction
    void install() {
        def requirementsFile = requirements.get()
        if (installedRequirements(requirementsFile).startsWith(requirementsFile.asFile.text)) {
            logger.info("Packages already installed")
        } else {
            project.exec {
                executable pipExe.get()
                args "install", "--require-virtualenv", "--requirement", requirementsFile
            }
        }

    }

    String installedRequirements(RegularFile requirementsFile) {
        def stdout = new ByteArrayOutputStream()
        project.exec {
            executable pipExe.get()
            args "freeze", "--require-virtualenv", "--local", "--requirement", requirementsFile
        }
        return stdout.toString()
    }
}