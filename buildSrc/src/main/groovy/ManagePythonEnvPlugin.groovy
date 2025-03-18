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
import org.gradle.api.tasks.options.Option
import org.gradle.internal.os.OperatingSystem
import org.gradle.process.ExecResult

import javax.annotation.Nullable
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

    @Internal
    def systemOs = OperatingSystem.current()

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

    List<String> platformWheelFixupRequirements() {
        if (systemOs.isLinux()) {
            ["auditwheel"]
        } else if (systemOs.isMacOsX()) {
            ["delocate"]
        } else if (systemOs.isWindows()) {
            ["delvewheel"]
        } else {
            throw "Unknown Operating System"
        }
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
            args("install", "--require-virtualenv", "build", "wheel", "setuptools", *platformWheelFixupRequirements(), *buildRequirements.get())
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

    @Internal
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
 * Task for providing the Wheel platform tag, which we need in a couple of places
 */
abstract class WheelCompatibilityTags extends DefaultTask {
    @InputFile
    abstract RegularFileProperty getPyenvExe()

    @InputFile
    abstract RegularFileProperty getPythonExe()

    @Input
    abstract Property<String> getPythonVersion()

    @Input
    abstract Property<String> getManylinuxTarget()

    @Internal
    abstract DirectoryProperty getWheelMetaDir()

    @Internal
    OperatingSystem systemOs = OperatingSystem.current()

    @Internal
    Provider<RegularFile> pythonVersionPath = pyenvExe.flatMap { RegularFile pyenv ->
        def versionPath = project.objects.fileProperty()
        versionPath.set(project.file(execString(pyenv, ["which", "pip"], [PYENV_VERSION: pythonVersion.get()])).parentFile.parentFile)
        versionPath
    }

    @Internal
    Provider<String> macosxDeploymentTarget = pythonExe.map { python ->
        if (systemOs.isMacOsX()) {
            def pyScript = """
import sysconfig

base_version = tuple(int(x) for x in sysconfig.get_platform().split('-')[1].split('.'))[:2]
version = (base_version[0], 0) if base_version[0] > 10 else base_version
print('.'.join([str(x) for x in version]))
"""
            execPython(pyScript)
        } else {
            ""
        }
    }

    @OutputFile
    Provider<String> wheelSuffix = wheelMetaDir.map { it.file("wheel-suffix.txt") }

    @OutputFile
    Provider<RegularFile> repairedWheelSuffix = wheelMetaDir.map { it.file("repaired-wheel-suffix.txt") }

    @OutputFile
    Provider<RegularFile> platform = wheelMetaDir.map { it.file("platform.txt") }

    @TaskAction
    generate() {
        def platformString = generatePlatformString(pythonVersionPath.get())

        def repairedWheelSuffixString = manylinuxTarget.map { manylinux ->
            def unrepairedPlatform = platformString
            def repairedPlatform = systemOs.isLinux() ? "${manylinux}_${unrepairedPlatform.replace("linux_", "")}" : unrepairedPlatform
            generateWheelSuffix(repairedPlatform)
        }.get()

        platform.get().asFile.write(platformString)
        repairedWheelSuffix.get().asFile.write(repairedWheelSuffixString)
        wheelSuffix.get().asFile.write(generateWheelSuffix(platformString))
    }

    String generatePlatformString(RegularFile pythonVersionPath) {
        def pyScript = """
import os
from setuptools.command.bdist_wheel import get_platform 

print(get_platform(os.environ['PYTHON_VERSION_PREFIX']))
"""

        execPython(pyScript, [PYTHON_VERSION_PREFIX: pythonVersionPath.asFile.toString()])
    }

    String generateWheelSuffix(String platform) {
        def pyScript = """
import os
from setuptools.command.bdist_wheel import get_platform
from wheel.vendored.packaging.tags import Tag, sys_tags

t = next(sys_tags())
print(Tag(t.interpreter, t.abi, os.environ['PLATFORM_TAG']))
"""
        execPython(pyScript, [PLATFORM_TAG: platform])
    }

    String execPython(String script, Map<String, String> env = [:]) {
        InputStream stdin = new ByteArrayInputStream(script.getBytes())
        execString(pythonExe.get(),
                ["-"],
                env,
                stdin)
    }

    String execString(RegularFile exe, List<String> arguments, Map<String, String> env = [:], InputStream stdin = new ByteArrayInputStream(new byte[0])) {
        (new ByteArrayOutputStream()).withStream { out ->
            project.exec {
                environment env
                executable exe
                args(*arguments)
                standardOutput out
                standardInput stdin
            }
            out.toString().trim()
        }
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

    @Input
    @Optional
    abstract Property<Boolean> getDebug()

    @InputDirectory
    abstract DirectoryProperty getSourceDir()

    @Internal
    abstract DirectoryProperty getOutputDir()

    @Internal
    def systemOs = OperatingSystem.current()

    @OutputDirectory
    final Provider<Directory> buildDir = outputDir.map { it.dir("build") }

    @InputFile
    abstract RegularFileProperty getWheelSuffix()

    @Input
    abstract Property<String> getMacosxDeploymentTarget()

    @Internal
    Provider<String> wheelFilename = productEdition.map { prod ->
        "saxonc${prod}-${productVersion.get()}-${wheelSuffix.get().asFile.text.strip()}.whl"
    }

    @OutputFile
    Provider<RegularFile> wheel = buildDir.map { it.file("dist/${wheelFilename.get()}") }

    @TaskAction
    void build() {
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
            if (systemOs.isMacOsX()) {
                environment "MACOSX_DEPLOYMENT_TARGET", macosxDeploymentTarget.get()
            }
            args "-m", "build", "--wheel"
        }
    }
}

abstract class RepairLocalWheel extends DefaultTask {
    @InputFile
    abstract RegularFileProperty getWheel()

    @InputDirectory
    abstract DirectoryProperty getLibraryDir()

    @Input
    abstract Property<String> getProductEdition()

    @InputFile
    abstract RegularFileProperty getPythonExe()

    @Internal
    abstract Property<PythonVenv> getPythonVenvTask()

    @Optional
    @Input
    abstract Property<String> getManylinuxTarget()

    @Internal
    abstract DirectoryProperty getOutputDir()

    @Internal
    def systemOs = OperatingSystem.current()

    @Internal
    def systemOsArch = System.getProperty("os.arch") == "amd64" ? "x86_64" : System.getProperty("os.arch")

    @InputFile
    abstract RegularFileProperty getRepairedWheelFilePlatformSuffix()

    // Linux - auditwheel
    ExecResult repairLinux() {
        project.exec {
            workingDir outputDir.get()
            executable pythonVenvTask.flatMap { it.venvBinHostedExe("auditwheel") }.get()
            environment "LD_LIBRARY_PATH", libraryDir.get()
            args "repair"
            if (manylinuxTarget.getOrElse("") != "") {
                args "--only-plat", "--plat", "${manylinuxTarget.get()}_${systemOsArch}"
            }
            args "--wheel-dir", outputDir.get()
            args wheel.get()
        }
    }

    // MacOS - delocate
    ExecResult repairMacos() {
        project.exec {
            workingDir outputDir.get()
            executable pythonVenvTask.flatMap { it.venvBinHostedExe("delocate-wheel") }.get()
            args "-w", outputDir.get()
            args wheel.get()
        }
    }

    // Windows - delvewheel
    ExecResult repairWindows() {
        project.exec {
            workingDir outputDir.get()
            executable pythonExe.get()
            args "-m", "delvewheel"
            args "repair", wheel.get()
            args "--wheel-dir", outputDir.get()
            args "--add-path", libraryDir.get()
        }
    }

    // regex the wheel filename to the output /(saxonc[ehp]e-[0-9]+\.[0-9]+\.[0-9]+)-.+$/
    @Internal
    Provider<String> repairedWheelFilename = wheel.map { wheel ->
        def nonPlatformPart = wheel.asFile.name.replaceFirst(/^(saxonc[ehp]e-[0-9]+\.[0-9]+\.[0-9]+)-.+$/, '$1')
        "${nonPlatformPart}-${repairedWheelFilePlatformSuffix.get().asFile.text.strip()}.whl"
    }

    @OutputFile
    Provider<RegularFile> repairedWheel = productEdition.flatMap { prod ->
        outputDir.map { it.file(repairedWheelFilename.get()) }
    }

    @TaskAction
    void repairWheel() {
        if (systemOs.isLinux()) {
            repairLinux()
        } else if (systemOs.isMacOsX()) {
            repairMacos()
        } else if (systemOs.isWindows()) {
            repairWindows()
        } else {
            throw "Unknown Operating System"
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

abstract class PytestRunner extends DefaultTask {
    private final List<Object> baseArguments = new ArrayList<>();
    private final List<Object> testArguments = new ArrayList<>();
    private final List<Object> pytestTrailingArguments = new ArrayList<>();
    private Map<String, Object> environment = [:];
    private Object workingDir;

    @InputFile
    abstract RegularFileProperty getPytest()

    @Internal
    public Object getWorkingDir() {
        return workingDir;
    }

    void setWorkingDir(Object dir) {
        this.workingDir = dir;
    }

    PytestRunner workingDir(Object dir) {
        setWorkingDir(dir);
        return this;
    }

    boolean hasWorkingDir() {
        return this.workingDir != null
    }

    PytestRunner baseArgs(Object... args) {
        if (args == null) {
            throw new IllegalArgumentException("args == null!")
        }
        baseArgs(Arrays.asList(args))
        return this
    }

    PytestRunner baseArgs(Iterable<?> args) {
        baseArguments.addAll(args)
        return this;
    }

    PytestRunner setBaseArgs(List<String> args) {
        baseArguments.clear()
        baseArgs(args)
        return this;
    }

    PytestRunner setBaseArgs(Iterable<?> args) {
        baseArguments.clear()
        baseArgs(args)
        return this;
    }

    @Nullable
    @Optional
    @Input
    List<String> getBaseArgs() {
        List<String> args = new ArrayList<>();
        for (Object argument : baseArguments) {
            args.add(argument.toString());
        }
        return args;
    }

    PytestRunner tests(Object... args) {
        if (args == null) {
            throw new IllegalArgumentException("args == null!")
        }
        tests(Arrays.asList(args))
        return this
    }

    PytestRunner tests(Iterable<?> args) {
        testArguments.addAll(args)
        return this;
    }

    @Option(option = "test", description = "set the test specifiers passed to pytest")
    PytestRunner setTests(List<String> args) {
        testArguments.clear()
        tests(args)
        return this;
    }

    PytestRunner setTests(Iterable<?> args) {
        testArguments.clear()
        tests(args)
        return this;
    }

    @Nullable
    @Optional
    @Input
    List<String> getTests() {
        List<String> args = new ArrayList<>();
        for (Object argument : testArguments) {
            args.add(argument.toString());
        }
        return args;
    }

    @Nullable
    @Optional
    @Input
    List<String> getPytestTrailingArguments() {
        List<String> args = new ArrayList<>();
        for (Object argument : pytestTrailingArguments) {
            args.add(argument.toString());
        }
        return args;
    }

    @Option(option = "s", description = "disable pytest capturing")
    PytestRunner setS() {
        pytestTrailingArguments.add("-s")
        return this;
    }

    @Option(option = "tee-sys", description = "disable pytest capturing")
    PytestRunner setTeeS() {
        pytestTrailingArguments.add("--capture=tee-sys")
        return this;
    }

    @Option(option = "v", description = "set pytest -v")
    PytestRunner setV() {
        pytestTrailingArguments.add("-v")
        return this;
    }

    @Option(option = "debug", description = "set SAXONC_DEBUG_FLAG=true")
    PytestRunner setSaxonCDebug() {
        getEnvironment().put("SAXONC_DEBUG_FLAG", "true");
        return this;
    }

    @Internal
    public Map<String, Object> getEnvironment() {
        return this.environment
    }

    void setEnvironment(Map<String, ?> environmentVariables) {
        // shallow-copy the input map then replace the existing environment
        this.environment = environmentVariables + [:]
    }

    PytestRunner environment(String name, Object value) {
        getEnvironment().put(name, value);
        return this;
    }

    PytestRunner environment(Map<String, ?> environmentVariables) {
        getEnvironment().putAll(environmentVariables);
        return this;
    }

    boolean hasEnvironment() {
        return !this.environment.isEmpty()
    }

    @TaskAction
    void execute() {
        def runner = this
        project.exec {
            if (runner.hasWorkingDir()) {
                workingDir runner.getWorkingDir()
            }
            executable runner.pytest.get()
            args runner.getBaseArgs()
            args runner.getTests()
            args runner.getPytestTrailingArguments()

            if (runner.hasEnvironment()) {
                environment runner.getEnvironment()
            }

            println(commandLine.join(" "))
        }
    }
}