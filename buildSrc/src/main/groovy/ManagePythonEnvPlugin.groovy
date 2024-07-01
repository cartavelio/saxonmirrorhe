import org.gradle.api.DefaultTask
import org.gradle.api.Plugin
import org.gradle.api.Project
import org.gradle.api.file.ConfigurableFileCollection
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
import org.gradle.api.tasks.InputFiles
import org.gradle.api.tasks.Internal
import org.gradle.api.tasks.Optional
import org.gradle.api.tasks.OutputDirectory
import org.gradle.api.tasks.OutputFile
import org.gradle.api.tasks.TaskAction
import org.gradle.internal.os.OperatingSystem

import javax.inject.Inject

abstract class ManagePythonEnvExtension {
    abstract Property<String> getVenvBasePath()
    abstract DirectoryProperty getVenvBaseDir()
    abstract Property<String> getPythonBinDirName()
    abstract Property<String> getVenvBinarySuffix()
}

class ManagePythonEnvPlugin implements Plugin<Project> {
    void apply(Project project) {
        def extension = project.extensions.create('pythonEnv', ManagePythonEnvExtension)
        OperatingSystem systemOs = OperatingSystem.current()

        def sysVenvBinarySuffix = ".exe"
        def pythonBinFolder = "Scripts"
        if(systemOs.isMacOsX() || systemOs.isLinux()) {
            sysVenvBinarySuffix = ""
            pythonBinFolder = "bin"
        }

        def venvBaseDir = project.layout.buildDirectory.dir("pyvenv")

        extension.pythonBinDirName.convention(pythonBinFolder)
        extension.venvBinarySuffix.convention(sysVenvBinarySuffix)
        extension.venvBaseDir.convention(venvBaseDir)
        extension.venvBasePath.convention(venvBaseDir.map { it.asFile.toString() })
    }
}

abstract class PyenvExe extends DefaultTask {
    @Optional
    @Input
    abstract Property<String> getPyenvExePath()

    @OutputFile
    final Provider<RegularFile> pyenv = pyenvExePath.orElse(findPyenvExePath()).flatMap {
        def fileProp = project.objects.fileProperty()
        fileProp.set(project.file(it))
        fileProp
    }

    // Needs something done to it for Windows
    private Provider<String> findPyenvExePath() {
        project.providers.provider {
            OperatingSystem systemOs = OperatingSystem.current()
            def whichIsh = systemOs.isWindows() ? "where.exe" : "which"
            (new ByteArrayOutputStream()).withStream { out ->
                project.exec {
                    executable whichIsh
                    args "pyenv"
                    standardOutput out
                }
                def res = out.toString()
                res.split(System.getProperty("line.separator"))[-1].trim()
            }
        }
    }
}

abstract class PythonPyenv extends DefaultTask {
    @Input
    abstract Property<String> getPythonVersion()

    @InputFile
    abstract RegularFileProperty getPyenvExe()

    @InputDirectory
    abstract DirectoryProperty getPythonBuildDefinitionsDir()

    @Optional
    @Input
    abstract Property<String> getDebugVariant()

    @Internal
    final Provider<String> pyenvVersionName = pythonVersion.map {
        "${it}${debugVariant.isPresent() ? "-${debugVariant.get()}" : "" }"
    }

    @TaskAction
    void execute() {
        // version
        // if not in pyenv versions
        //   if version in python/
        //      pyenv install /path/to/python/version
        //   else
        //      pyenv install version
        def pyenv = pyenvExe.get()
        def pyenvVersions = (new ByteArrayOutputStream()).withStream { out ->
            project.exec {
                executable pyenv
                args "versions", "--bare"
                standardOutput out
            }
            out.toString().trim()
        }.split("\\R")

        def pythonBuildVersionName = pyenvVersionName.get()
        if (!pyenvVersions.contains(pythonBuildVersionName)) {
            def definitionFile = pythonBuildDefinitionsDir.get().file(pythonBuildVersionName)
            if (definitionFile.asFile.exists()) {
                pythonBuildVersionName = definitionFile.asFile.toString()
            }
            project.exec {
                executable pyenv
                args "install", pythonBuildVersionName
            }
        }
    }
}

abstract class PythonPyenvPythonExe extends DefaultTask {
    @Input
    abstract Property<String> getPyenvVersionName()

    @InputFile
    abstract RegularFileProperty getPyenvExe()

    @Internal
    final Provider<String> pythonExePath = pyenvExe.map { pyenvPath ->
        (new ByteArrayOutputStream()).withStream { out ->
            project.exec {
                environment "PYENV_VERSION", pyenvVersionName.get()
                executable pyenvPath.asFile
                args "which", "python"
                standardOutput out
            }
            out.toString().trim()
        }
    }

    @OutputFile
    Provider<RegularFile> pythonExe = pythonExePath.flatMap {
        def fileProp = project.objects.fileProperty()
        fileProp.set(project.file(it))
        fileProp
    }
}


abstract class PythonVenv extends DefaultTask {
    @Input
    abstract Property<String> getBranchesExtension()

    @Input
    abstract ListProperty<String> getBuildRequirements()

    @InputFile
    abstract RegularFileProperty getPyenvPythonExe()

    @Optional
    @Input
    abstract Property<String> getSuffix()

    @Internal
    final Provider<String> branch = branchesExtension.map { project.extensions.getByName(it).properties.get("current").get() }

    @Internal
    final Provider<String> venvName = branch.map {
        def finalSuffix = suffix.isPresent() ? "-${suffix.get()}" : ""
        "${it}${finalSuffix}"
    }

    @Internal
    final Provider<Directory> venvPath = project.pythonEnv.venvBaseDir.map { it.dir(venvName.get()) }

    @Internal
    final Provider<Directory> venvBinPath = venvPath.map { it.dir(project.pythonEnv.pythonBinDirName.get()) }

    @OutputFile
    final Provider<RegularFile> pythonExe = venvPath.flatMap { venvBinHostedExe("python") }

    @OutputFile
    final Provider<RegularFile> pipExe = venvPath.flatMap { venvBinHostedExe("pip3") }// envBinPath.map { it.file("pip3${sysVenvBinarySuffix}") }

    Provider<RegularFile> venvBinHostedExe(String exe) {
        venvBinPath.map {
            it.file("${exe}${project.pythonEnv.venvBinarySuffix.get()}")
        }
    }

    PythonVenv() {
        branchesExtension.convention("branches")
        buildRequirements.convention([])
    }

    @TaskAction
    void execute() {
        logger.info("Creating Python Venv for ${branch.get()}: ${venvName.get()}")

        project.exec {
            executable pyenvPythonExe.get()
            args "-m", "venv", venvPath.get()
        }

        project.exec {
            executable pipExe.get().asFile.toString()
            args("install", "--require-virtualenv", "build", "wheel", "repairwheel", "setuptools", *buildRequirements.get())
        }
    }
}


abstract class PythonSetup extends DefaultTask {
    @Input
    abstract Property<String> getEdition()

    @Input
    abstract Property<String> getLibpath()

    @Input
    abstract Property<String> getVersion3()

    @InputFiles
    abstract ConfigurableFileCollection getSaxoncHomeFiles()

    @InputFiles
    abstract ConfigurableFileCollection getGraalImageHeaderFiles()

    @InputFiles
    abstract DirectoryProperty getPypiDir()

    @InputFiles
    abstract DirectoryProperty getSaxonCApiDir()

    @Internal
    abstract DirectoryProperty getOutputDir()

    @OutputDirectory
    final Provider<Directory> packageDir = outputDir.map { it }

    @OutputFile
    final Provider<RegularFile> requirementsTxtFile = outputDir.map { it.file("requirements.txt") }

    @Internal
    def systemOs = OperatingSystem.current()

    @Internal
    def systemOsArch = System.getProperty("os.arch") == "amd64" ? "x86_64" : System.getProperty("os.arch")

    @TaskAction
    void execute() {
        def editionString = edition.get()

        project.copy {
            into packageDir
            from pypiDir
            filter textReplacementFilter(editionString, '', libpath.get())
        }
        def platformDir = (systemOs.isMacOsX() ? "darwin" : (systemOs.isLinux() ? "nix" : "win"))
        def SAXONC_HOME = packageDir.map { it.file("libs/${platformDir}") }

        if (SAXONC_HOME == null) {
            throw GradleException("Don't know how to create Python packages on this OS.");
        }
        project.copy {
            println("\"${editionString}c_build_graal_image\" files:")
            println(saxoncHomeFiles.files)
            into SAXONC_HOME
            from(saxoncHomeFiles)
        }

        // What does this do?
        project.copy {
            from(graalImageHeaderFiles)
            into packageDir.map { it.dir("Saxon.C.API/graalvm") }
        }
        project.copy {
            from project.layout.projectDirectory.dir("src/main/c/pypi/python_saxon")
            into packageDir.map { it.dir("python_saxon") }
            include "saxonc.pyx"
            include "saxoncClasses.pxd"
            include "readme.txt"
            filter textReplacementFilter(editionString)
        }
        project.copy {
            from project.layout.projectDirectory.dir("src/main/c/pypi/python_saxon")
            into packageDir
            include "test_saxonc.py"
            include "test_saxon_schema.py"
            filter textReplacementFilter(editionString)
        }
        project.copy {
            from project.layout.projectDirectory.dir("src/main/c/pypi/python_saxon")
            into packageDir.map { it.dir("samples") }
            include "saxon_example.py"
            include "saxon_example2.py"
            include "saxon_example3.py"
            filter textReplacementFilter(editionString)
        }
        project.copy {
            from project.layout.projectDirectory.dir("src/main/c/Saxon.C.API")
            into packageDir.map { it.dir("Saxon.C.API") }

            include "*.h"
            include "*.c"
            include "*.cpp"
            exclude "php*"

            filter textReplacementFilter(editionString)
        }
    }

    def textReplacementFilter(String edition, String location='../', String libpath=null) {
        def gccopts = System.getProperty('os.arch') == 'amd64' ? '-m64' : ''
        def saxonVersion3 = version3.get()

        def libdir = (systemOs.isMacOsX()
                ? 'libs/darwin'
                : (systemOs.isLinux()
                ? 'libs/nix'
                : 'libs/win'))

        if (libpath == null) {
            libpath = (systemOs.isLinux()
                    ? '../libs/nix/'
                    : (systemOs.isMacOsX()
                    ? '../libs/darwin/'
                    : '..\\\\libs\\\\win\\\\'))
        }

        def lastLinePatch = (systemOs.isLinux()
                ? ('[tool.cibuildwheel]\n'
                + 'build = "*"\n'
                + 'skip = ["cp36*", "cp37*", "pp*", "cp*musl*"]\n'
                + "archs = [\"${systemOsArch}\"]\n"
                + 'environment.LD_LIBRARY_PATH = "/project/libs/nix"\n')
                : (systemOs.isWindows()
                ? ('[tool.cibuildwheel]\n'
                + 'build = "*"\n'
                + 'archs = ["AMD64"]\n'
                + 'skip = ["*-win32", "cp36*", "cp37*", "pp*", "cp*musl*"]\n')
                : ('[tool.cibuildwheel]\n'
                + 'build = "*"\n'
                + 'skip = ["cp36*", "cp37*", "pp*", "cp*musl*"]\n')))

        return { line ->
            line.replace('@@VERSION@@', saxonVersion3)
                    .replace('@@VERSION.3@@', saxonVersion3)
                    .replace('@@edition@@', edition)
                    .replace('@@EDITION@@', edition.toUpperCase())
                    .replace('@@gccopts@@', gccopts)
                    .replace('@@libdir@@', libdir)
                    .replace('@@libpath@@', libpath)
                    .replace('@@location@@', location)
                    .replace('[LAST-LINE-BUILD-PATCH]', lastLinePatch)
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

    @InputFile
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

    @Input
    abstract Property<String> getManylinuxTarget()

    @InputFile
    abstract RegularFileProperty getPythonExe()

    @InputFile
    abstract RegularFileProperty getRepairwheelExe()

    @Input
    abstract Property<String> getProductEdition()

    @Input
    abstract Property<String> getProductVersion()

    @Input
    @Optional
    abstract Property<Boolean> getDebug()

    @InputDirectory
    abstract DirectoryProperty getLibraryDir()

    @InputDirectory
    abstract DirectoryProperty getSourceDir()

    @Internal
    abstract DirectoryProperty getOutputDir()

    @OutputDirectory
    final Provider<Directory> buildDir = outputDir.map {it.dir("build") }

    @Internal
    Provider<String> repairedWheelFilePlatformSuffix = pythonExe.map {python ->
        OperatingSystem systemOs = OperatingSystem.current()
        def platform = systemOs.isLinux() ? repairedLinuxPlatform() : unrepairedPlatform()
        (new ByteArrayOutputStream()).withStream { out ->
            project.exec {
                environment REPAIRED_PLATFORM: platform
                workingDir sourceDir.get()
                executable python
                args "-c", "from wheel.bdist_wheel import get_platform; from wheel.vendored.packaging.tags import Tag, sys_tags; import os; t = next(sys_tags()); print(Tag(t.interpreter, t.abi, os.environ['REPAIRED_PLATFORM']))"
                standardOutput out
            }
            out.toString().trim()
        }
    }

    String repairedLinuxPlatform() {
        def arch = unrepairedPlatform().replace("linux_", "")
        "${manylinuxTarget.get()}_${arch}"
    }

    String unrepairedPlatform() {
        (new ByteArrayOutputStream()).withStream { out ->
            project.exec {
                workingDir sourceDir.get()
                executable pythonExe.get()
                args "-c", "from wheel.bdist_wheel import get_platform; print(get_platform('.'))"
                standardOutput out
            }
            out.toString().trim()
        }
    }

    @Internal
    Provider<String> inputWheelFilePlatformSuffix = pythonExe.map {python ->
        (new ByteArrayOutputStream()).withStream { out ->
            project.exec {
                workingDir sourceDir.get()
                executable python
                args "-c", "from wheel.bdist_wheel import get_platform; from wheel.vendored.packaging.tags import Tag, sys_tags; t = next(sys_tags()); print(Tag(t.interpreter, t.abi, get_platform('.')))"
                standardOutput out
            }
            out.toString().trim()
        }
    }

    @Internal
    Provider<String> inputWheelFilename = productEdition.map { prod ->
        "saxonc${prod}-${productVersion.get()}-${inputWheelFilePlatformSuffix.get()}.whl"
    }

    @Internal
    Provider<String> repairedWheelFilename = productEdition.map { prod ->
        "saxonc${prod}-${productVersion.get()}-${repairedWheelFilePlatformSuffix.get()}.whl"
    }

    @OutputFile
    Provider<RegularFile> wheel = productEdition.flatMap { prod ->
        outputDir.map { it.file(repairedWheelFilename.get()) }
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
            environment "BUILD_DEBUG", debug.getOrElse(false).toString()
            args "-m", "build", "--wheel"
        }

        project.exec {
            executable repairwheelExe.get()
            args "--output-dir", outputDir.get(),
                    "--lib-dir", libraryDir.get(),
                    buildDir.map {it.file("dist/${inputWheelFilename.get()}") }.get()
            println(it.commandLine)
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