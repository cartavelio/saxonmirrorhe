package com.saxonica.build.python

import SaxonBuildExtension
import SaxonCPythonExtension
import com.saxonica.build.PyprojectToml
import org.apache.tools.ant.filters.ReplaceTokens
import org.gradle.api.DefaultTask
import org.gradle.api.GradleException
import org.gradle.api.Project
import org.gradle.api.file.*
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.*
import org.gradle.internal.os.OperatingSystem
import org.gradle.kotlin.dsl.assign
import org.gradle.kotlin.dsl.filter
import org.gradle.kotlin.dsl.register
import org.gradle.language.base.plugins.LifecycleBasePlugin
import org.gradle.process.ExecOperations
import org.gradle.process.ExecResult
import java.io.ByteArrayOutputStream
import javax.inject.Inject

/**
 * Task for providing the Wheel platform tag, which we need in a couple of places
 */
abstract class WheelCompatibilityTags @Inject constructor(private val ex: ExecOperations) : DefaultTask() {
    @get:InputFile
    abstract val pythonExe: RegularFileProperty

    @get:Input
    abstract val manylinuxTarget: Property<String>

    @get:Input
    @get:Optional
    abstract val macosxDeploymentTarget: Property<String>

    @Internal
    val systemOs = OperatingSystem.current()

    @Internal
    val platform: Provider<String> = pythonExe.map { pyExe ->
        execString(pyExe,
            listOf("-c", "import sys; from setuptools.command.bdist_wheel import get_platform; print(get_platform(sys.base_prefix))"))
    }

    @Internal
    val wheelSuffix: Provider<String> = platform.map {
        generateWheelSuffix(it)
    }

    @Internal
    val repairedWheelSuffix: Provider<String> = manylinuxTarget.map { manylinux ->
        val unrepairedPlatform = platform.get()
        val repairedPlatform = if (systemOs.isLinux()) "${manylinux}_${unrepairedPlatform.replace("linux_", "")}" else unrepairedPlatform
        generateWheelSuffix(repairedPlatform)
    }

    fun generateWheelSuffix(platform: String): String {
        return execString(pythonExe.get(),
            listOf("-c", "import os; from packaging.tags import Tag, sys_tags; t = next(sys_tags()); print(Tag(t.interpreter, t.abi, os.environ['PLATFORM_TAG']))"),
            mapOf("PLATFORM_TAG" to platform))
    }

    fun execString(exe: RegularFile, arguments: List<String>, env: Map<String, String> = mapOf()): String {
        return (ByteArrayOutputStream()).use { out ->
            ex.exec {
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
abstract class BuildWheel @Inject constructor(private val ex: ExecOperations): DefaultTask() {
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
    abstract val libsaxoncDir: DirectoryProperty

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

            exclude("pyproject.toml", "README.md", "src/saxonc/saxonc.pyx")
        }

        project.copy {
            into(srcCopyDir)

            from(sourceDir) {
                include("README.md", "src/saxonc/saxonc.pyx")
                filter<ReplaceTokens>("tokens" to mapOf(
                    "EDITION" to productEdition.get().uppercase(),
                    "edition" to productEdition.get(),
                    "VERSION" to  productVersion.get()
                ))
            }
        }

        val pyproject = PyprojectToml(sourceDir, productVersion, productEdition)
        pyproject.process(srcCopyDir)

        val buildResult = ex.exec {
            workingDir = srcCopyDir.get().asFile
            setIgnoreExitValue(true)
            executable(pythonExe.get())
            environment("BUILD_DEBUG", debug.getOrElse(false).toString())
            if (macosxDeploymentTarget.isPresent) {
                environment("MACOSX_DEPLOYMENT_TARGET", macosxDeploymentTarget.get())
            }

            environment("LIBSAXONC_PATH", libsaxoncDir.get())
            environment("SAXONC_BUILD_EDITION", productEdition.get())
            environment("SAXONC_BUILD_VERSION", productVersion.get())
            args("-m", "build", "--wheel", "--outdir", outputDir.get().asFile.toString())
        }

        if (buildResult.exitValue == 0) project.delete(srcCopyDir)
        buildResult.assertNormalExitValue()
    }
}

abstract class RepairLocalWheel  @Inject constructor(private val ex: ExecOperations): DefaultTask() {
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
        return ex.exec {
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
        return ex.exec {
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
        return ex.exec {
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
abstract class InstallLocalWheel  @Inject constructor(private val ex: ExecOperations): DefaultTask() {
    @get:InputFile
    abstract val pipExe: RegularFileProperty

    @get:InputFile
    abstract val wheel: RegularFileProperty

    @TaskAction
    fun install() {
        ex.exec {
            executable(pipExe.get())
            args("install", "--force-reinstall", wheel.map { it.asFile }.get())
        }
    }
}

class WheelTasksBuilder(val extension: SaxonCPythonExtension, val saxonBuild: SaxonBuildExtension, val project: Project, ) {
    companion object {
        fun create(
            project: Project,
            extension: SaxonCPythonExtension,
            saxonBuild: SaxonBuildExtension
        ): (WheelTasksBuilder.() -> Unit) -> TaskProvider<RepairLocalWheel> {
            return { configuration ->
                val builder = WheelTasksBuilder(extension, saxonBuild, project)
                builder.configuration()
                builder.invoke()
            }
        }
    }

    val sourceDir = project.objects.directoryProperty()
    val buildDir = project.objects.directoryProperty()

    private var venvVal: TaskProvider<PythonVenv>? = null
    var venvTask: TaskProvider<PythonVenv>
        get() = venvVal!!
        set(value) {
            venvVal = value
        }

    private var platformTagTaskVal: TaskProvider<WheelCompatibilityTags>?  = null
    var platformTagTask: TaskProvider<WheelCompatibilityTags>
        get() = platformTagTaskVal!!
        set(value) {
            platformTagTaskVal = value
        }
    private var libsaxoncDirVal: Provider<Set<FileSystemLocation>>?  = null
    var libsaxoncDir: Provider<Set<FileSystemLocation>>
        get() = libsaxoncDirVal!!
        set(value) {
            libsaxoncDirVal = value
        }

    var prefix: String? = null
    protected fun invoke():  TaskProvider<RepairLocalWheel> {
        val builder = this
        val name_suffix = prefix?.let { "${it}_wheel" } ?: "wheel"
        val buildWheel = project.tasks.register<BuildWheel>("build_${name_suffix}") {
            group = LifecycleBasePlugin.BUILD_GROUP
            description = "Build the release wheel"
            pythonExe = venvTask.map { it.pythonExe.get() }
            productEdition = extension.edition
            productVersion = saxonBuild.version
            wheelSuffix = platformTagTask.flatMap { it.wheelSuffix }
            macosxDeploymentTarget = platformTagTask.flatMap { it.macosxDeploymentTarget }
            sourceDir = builder.sourceDir
            libsaxoncDir = project.layout.dir(builder.libsaxoncDir.map { it.first().asFile })
            outputDir = builder.buildDir.dir("wheel")
        }

        val libLocations: Provider<List<String>> = saxonBuild.systemOsName.map { os -> if (os == "windows") listOf("lib", "bin") else listOf("lib") }
        val libDirs = libsaxoncDir.map { dirs ->
            val dirPath = dirs.first().asFile.toString()
            libLocations.map { dirnames ->
                dirnames.map { dirname ->
                    project.layout.projectDirectory.dir(dirPath).dir(dirname)
                }
            }.get()
        }

        val repairWheel = project.tasks.register<RepairLocalWheel>("repair_${name_suffix}") {
            description = "Fix up the shared libs within the release wheel"
            outputDir = buildDir.dir("repaired-wheel")
            wheel = buildWheel.map { it.wheel.get() }
            manylinuxTarget = platformTagTask.flatMap { it.manylinuxTarget }
            pythonExe = venvTask.map { it.pythonExe.get() }
            pythonVenvTask = venvTask
            productEdition = extension.edition
            systemOsArch = saxonBuild.systemOsArch
            repairedWheelFilePlatformSuffix = platformTagTask.flatMap { it.repairedWheelSuffix }
            libraryDirs.from(libDirs)
        }

        return repairWheel
    }
}
