package com.saxonica.build.python

import com.saxonica.build.PyprojectToml
import org.gradle.api.DefaultTask
import org.gradle.api.GradleException
import org.gradle.api.file.*
import org.gradle.api.provider.ListProperty
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.*
import org.gradle.internal.os.OperatingSystem
import org.gradle.process.ExecResult
import java.io.ByteArrayOutputStream
import javax.inject.Inject


/**
 * Task for providing the Wheel platform tag, which we need in a couple of places
 */
abstract class WheelCompatibilityTags : DefaultTask() {
    @get:InputFile
    abstract val pyenvExe: RegularFileProperty

    @get:InputFile
    abstract val pythonExe: RegularFileProperty

    @get:Input
    abstract val pythonVersion: Property<String>

    @get:Input
    abstract val manylinuxTarget: Property<String>

    @Internal
    val systemOs = OperatingSystem.current()

    @Internal
    val pythonVersionPath: Provider<RegularFile> = pyenvExe.flatMap { pyenv ->
        val versionPath = project.objects.fileProperty()
        versionPath.set(project.file(execString(pyenv, listOf("which", "pip"), mapOf("PYENV_VERSION" to pythonVersion.get()))).parentFile.parentFile)
        versionPath
    }

    @Internal
    val macosxDeploymentTarget: Provider<String> = pythonExe.map { python ->
        if (systemOs.isMacOsX()) {
            execString(python, listOf("-c", "import sysconfig; base_version = tuple(int(x) for x in sysconfig.get_platform().split('-')[1].split('.'))[:2]; base_version = (base_version[0], 0) if base_version[0] > 10 else base_version; print('.'.join([str(x) for x in base_version]))"))
        } else {
            ""
        }
    }

    @Internal
    val platform: Provider<String> = pythonVersionPath.map { path ->
        execString(pythonExe.get(),
            listOf("-c", "import os; from setuptools.command.bdist_wheel import get_platform; print(get_platform(os.environ['PYTHON_VERSION_PREFIX']))"),
            mapOf("PYTHON_VERSION_PREFIX" to path.asFile.toString()))
    }

    @Internal
    val wheelSuffix: Provider<String> = platform.map { generateWheelSuffix(it) }

    @Internal
    val repairedWheelSuffix: Provider<String> = manylinuxTarget.map { manylinux ->
        val unrepairedPlatform = platform.get()
        val repairedPlatform = if (systemOs.isLinux()) "${manylinux}_${unrepairedPlatform.replace("linux_", "")}" else unrepairedPlatform
        generateWheelSuffix(repairedPlatform)
    }

    fun generateWheelSuffix(platform: String): String {
        return execString(pythonExe.get(),
            listOf("-c", "import os; from setuptools.command.bdist_wheel import get_platform; from wheel.vendored.packaging.tags import Tag, sys_tags; t = next(sys_tags()); print(Tag(t.interpreter, t.abi, os.environ['PLATFORM_TAG']))"),
            mapOf("PLATFORM_TAG" to platform))
    }

    fun execString(exe: RegularFile, arguments: List<String>, env: Map<String, String> = mapOf()): String {
        return (ByteArrayOutputStream()).use { out ->
            project.exec {
                environment(env)
                executable(exe)
                args(arguments)
                standardOutput = out
            }
            out.toString().trim()
        }
    }
}

/**
 * Task for building a wheel and fixing it up with repairwheel
 */
abstract class BuildWheel : DefaultTask() {
    @get:InputFile
    abstract val pythonExe: RegularFileProperty

    @get:Input
    abstract val productEdition: Property<String>

    @get:Input
    abstract val productVersion: Property<String>

    @get:Input
    @get:Optional
    abstract val debug: Property<Boolean>

    @get:Internal
    abstract val sourceDir: DirectoryProperty

    @get:InputFiles
    val sources = sourceDir.map {
        it.asFileTree.matching {
            exclude(".pytest_cache")
            exclude("**/__pycache__")
            exclude("test")
        }
    }

    @get:InputDirectory
    abstract val saxoncCoreDir: DirectoryProperty

    @get:Internal
    abstract val outputDir: DirectoryProperty

    @Internal
    val systemOs = OperatingSystem.current()

    @OutputDirectory
    val buildDir: Provider<Directory> = outputDir.map { it }

    @get:Input
    abstract val wheelSuffix: Property<String>

    @get:Input
    abstract val macosxDeploymentTarget: Property<String>

    @Internal
    val wheelFilename: Provider<String> = productEdition.map { prod ->
        "saxonc${prod}-${productVersion.get()}-${wheelSuffix.get()}.whl"
    }

    @OutputFile
    val wheel: Provider<RegularFile> = productEdition.flatMap { buildDir.map { it.file(wheelFilename.get()) } }

    @TaskAction
    fun build() {
        val srcCopyDir = project.layout.buildDirectory.dir("wheelSrc")

        project.delete(srcCopyDir)

        project.copy {
            from(sourceDir)
            into(srcCopyDir)

            exclude("pyproject.toml")
        }

        val pyproject = PyprojectToml(sourceDir, productVersion, productEdition)
        pyproject.process(srcCopyDir)

        val buildResult = project.exec {
            workingDir = srcCopyDir.get().asFile
            setIgnoreExitValue(true)
            executable(pythonExe.get())
            environment("BUILD_DEBUG", debug.getOrElse(false).toString())
            if (systemOs.isMacOsX()) {
                environment("MACOSX_DEPLOYMENT_TARGET", macosxDeploymentTarget.get())
            }

            environment("SAXONC_CORE_PATH", saxoncCoreDir.get())
            environment("SAXONC_BUILD_EDITION", productEdition.get())
            environment("SAXONC_BUILD_VERSION", productVersion.get())
            args("-m", "build", "--wheel", "--outdir", outputDir.get().asFile.toString())
        }

        if (buildResult.exitValue == 0) project.delete(srcCopyDir)
        buildResult.assertNormalExitValue()
    }
}

abstract class RepairLocalWheel : DefaultTask() {
    @get:Input
    abstract val systemOsArch: Property<String>

    @get:InputFile
    abstract val wheel: RegularFileProperty

    @get:InputFiles
    abstract val libraryDirs: ConfigurableFileCollection

    @get:Input
    abstract val productEdition: Property<String>

    @get:InputFile
    abstract val pythonExe: RegularFileProperty

    @get:Internal
    abstract val pythonVenvTask: Property<PythonVenv>

    @get:Optional
    @get:Input
    abstract val manylinuxTarget: Property<String>

    @get:Internal
    abstract val outputDir: DirectoryProperty

    @Internal
    val systemOs = OperatingSystem.current()

    @get:Input
    abstract val repairedWheelFilePlatformSuffix: Property<String>

    @Internal
    val nonPlatformPartRegex = """^(saxonc[ehp]e-[0-9]+\.[0-9]+\.[0-9]+)-.+$""".toRegex()

    // Linux - auditwheel
    fun repairLinux(): ExecResult {
        return project.exec {
            workingDir(outputDir.get())
            executable(pythonVenvTask.flatMap { it.venvBinHostedExe("auditwheel") }.get())
            environment("LD_LIBRARY_PATH", libraryDirs.toList().map { it.toString() }.joinToString(separator = ":"))
            args("repair")
            if (manylinuxTarget.getOrElse("") != "") {
                args("--only-plat", "--plat", "${manylinuxTarget.get()}_${systemOsArch.get()}")
            }
            args("--wheel-dir", outputDir.get())
            args(wheel.get())
        }
    }

    // MacOS - delocate
    fun repairMacos(): ExecResult {
        return project.exec {
            workingDir(outputDir.get())
            environment("LD_LIBRARY_PATH", libraryDirs.toList().map { it.toString() }.joinToString(separator = ":"))
            environment("DYLD_LIBRARY_PATH", libraryDirs.toList().map { it.toString() }.joinToString(separator = ":"))
            executable(pythonVenvTask.flatMap { it.venvBinHostedExe("delocate-wheel") }.get())
            args("-w", outputDir.get())
            args(wheel.get())
        }
    }

    // Windows - delvewheel
    fun repairWindows(): ExecResult {
        return project.exec {
            workingDir(outputDir.get())
            executable(pythonExe.get())
            args("-m", "delvewheel")
            args("repair", wheel.get())
            args("--wheel-dir", outputDir.get())
            args("--add-path", libraryDirs.toList().map { it.toString() }.joinToString(separator = ";"))
        }
    }

    // regex the wheel filename to the output /(saxonc[ehp]e-[0-9]+\.[0-9]+\.[0-9]+)-.+$/
    @Internal
    val repairedWheelFilename: Provider<String> = wheel.map { wheel ->
        val nonPlatformPart = nonPlatformPartRegex.replace(wheel.asFile.name, "$1")
        "${nonPlatformPart}-${repairedWheelFilePlatformSuffix.get()}.whl"
    }

    @OutputFile
    val repairedWheel: Provider<RegularFile> = productEdition.flatMap { _ ->
        outputDir.map { it.file(repairedWheelFilename.get()) }
    }

    @TaskAction
    fun repairWheel() {
        if (systemOs.isLinux()) {
            repairLinux()
        } else if (systemOs.isMacOsX()) {
            repairMacos()
        } else if (systemOs.isWindows()) {
            repairWindows()
        } else {
            throw GradleException("Unknown Operating System")
        }
    }
}

/**
 * Task to install a wheel from a local file
 */
abstract class InstallLocalWheel : DefaultTask() {
    @get:InputFile
    abstract val pipExe: RegularFileProperty

    @get:InputFile
    abstract val wheel: RegularFileProperty

    @TaskAction
    fun install() {
        project.exec {
            executable(pipExe.get())
            args("install", "--force-reinstall", wheel.map { it.asFile }.get())
        }
    }
}
