import org.gradle.api.DefaultTask
import org.gradle.api.Plugin
import org.gradle.api.Project
import org.gradle.api.file.Directory
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.FileSystemOperations
import org.gradle.api.file.RegularFile
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.provider.Property
import org.gradle.api.provider.ListProperty
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.Input
import org.gradle.api.tasks.InputDirectory
import org.gradle.api.tasks.InputFile
import org.gradle.api.tasks.Internal
import org.gradle.api.tasks.OutputDirectory
import org.gradle.api.tasks.OutputFile
import org.gradle.api.tasks.TaskAction
import org.gradle.internal.os.OperatingSystem

import javax.inject.Inject

abstract class ManagePythonEnvExtension {
    abstract Property<String> getSystemPythonBinary()
    abstract Property<String> getVenvPath()
    abstract Property<String> getVenvPipPath()
    abstract Property<String> getVenvPythonPath()
    abstract Property<String> getVenvBinPath()
    abstract Property<String> getVenvBinarySuffix()
    abstract DirectoryProperty getVenvBinDir()

    Provider<String> venvBinHostedExe(String exe) {
        venvBinDir.map { it.file("${exe}${venvBinarySuffix.get()}") }
    }
}

class ManagePythonEnvPlugin implements Plugin<Project> {
    void apply(Project project) {
        def extension = project.extensions.create('pythonEnv', ManagePythonEnvExtension)
        OperatingSystem systemOs = OperatingSystem.current()

        def sysVenvBinarySuffix = ".exe"
        def sysPythonBinary = "python"
        def pythonBinFolder = "Scripts"
        if(systemOs.isMacOsX() || systemOs.isLinux()) {
            sysVenvBinarySuffix = ""
            sysPythonBinary = "python3"
            pythonBinFolder = "bin"
        }

        def venvDir = project.layout.buildDirectory.dir(project.branches.current.map { "py_venvs/${it}" })
        def venvBinDir = venvDir.map { it.dir(pythonBinFolder) }

        extension.systemPythonBinary.convention(sysPythonBinary)
        extension.venvPath.convention(venvDir.map { it.asFile.toString() })
        extension.venvBinPath.convention(venvBinDir.map { it.asFile.toString() })
        extension.venvBinDir.convention(venvBinDir)
        extension.venvBinarySuffix.convention(sysVenvBinarySuffix)
        extension.venvPipPath.convention(venvBinDir.map { it.file("pip3${sysVenvBinarySuffix}").asFile.toString() })
        extension.venvPythonPath.convention(venvBinDir.map { it.file("python${sysVenvBinarySuffix}").asFile.toString() })
    }
}

abstract class PythonVenv extends DefaultTask {
    @Input
    abstract Property<String> getBranchesExtension()

    @Input
    abstract ListProperty<String> getBuildRequirements()

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
        buildRequirements.convention([])
    }

    @TaskAction
    void execute() {
        logger.info("Creating Python Venv for ${branch.get()}")

        project.exec {
            executable project.pythonEnv.systemPythonBinary.get()
            args "-m", "venv", project.pythonEnv.venvPath.get()
        }

        project.exec {
            executable project.pythonEnv.venvPipPath.get()
            args("install", "--require-virtualenv", "build", "wheel", "repairwheel", *buildRequirements.get())
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

/**
 * Task for building a wheel and fixing it up with repairwheel
 */
abstract class BuildWheel extends DefaultTask {
    private final FileSystemOperations fileSystemOperations

    @Inject
    public BuildWheel(FileSystemOperations fileSystemOperations) {
        this.fileSystemOperations = fileSystemOperations
    }


    @InputFile
    abstract RegularFileProperty getPythonExe()

    @Input
    abstract Property<String> getProductEdition()

    @Input
    abstract Property<String> getProductVersion()

    @InputDirectory
    abstract DirectoryProperty getLibraryDir()

    @InputDirectory
    abstract DirectoryProperty getSourceDir()

    @Internal
    abstract DirectoryProperty getOutputDir()

    @OutputDirectory
    final Provider<Directory> buildDir = outputDir.map {it.dir("build") }

    @Internal
    Provider<String> wheelFilePlatformSuffix = pythonExe.map {python ->
        (new ByteArrayOutputStream()).withStream { out ->
            project.exec {
                workingDir sourceDir.get()
                executable python
                args "-c", "from wheel.bdist_wheel import get_platform; from wheel.vendored.packaging.tags import Tag, sys_tags; t = next(sys_tags()); print(Tag(t.interpreter, t.abi, get_platform(\".\")))"
                standardOutput out
            }
            out.toString().trim()
        }
    }

    @Internal
    Provider<String> wheelFilename = productEdition.map { prod ->
        "saxonc${prod}-${productVersion.get()}-${wheelFilePlatformSuffix.get()}.whl"
    }

    @OutputFile
    Provider<RegularFile> wheel = productEdition.flatMap { prod ->
        outputDir.map { it.file(wheelFilename.get()) }
    }

    @TaskAction
    void build() {
        def tmpWheelDir = temporaryDir
        def edition = productEdition.get()

        fileSystemOperations.copy {
            from(sourceDir) {
                exclude "__pycache__/**"
                exclude ".pytest_cache/**"
            }
            into buildDir
        }

        project.exec {
            workingDir buildDir.get()
            executable pythonExe.get()
            args "-m", "build", "--wheel"
        }

        def pythonEnv = project.extensions.getByType(ManagePythonEnvExtension)
        project.exec {
            executable pythonEnv.venvBinHostedExe("repairwheel").get()
            args "--output-dir", outputDir.get(),
                    "--lib-dir", libraryDir.get(),
                    buildDir.map {it.file("dist/${wheelFilename.get()}") }.get()
        }
    }
}

/**
 * Task to install a wheel from a local file
 */
abstract class InstallLocalWheel extends DefaultTask {
    @InputFile
    abstract RegularFileProperty getPipExe()

    @InputFile
    abstract RegularFileProperty getWheel()

    @TaskAction
    void install() {
        project.exec {
            executable pipExe.get()
            args "install", "--force-reinstall", wheel.map { it.asFile }.get()
        }
    }
}