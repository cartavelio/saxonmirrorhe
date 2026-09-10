package com.saxonica.build.python

import SaxonCPythonExtension
import com.saxonica.build.PyprojectToml
import org.gradle.api.Action
import org.gradle.api.DefaultTask
import org.gradle.api.GradleException
import org.gradle.api.file.Directory
import org.gradle.api.file.RegularFile
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.*
import org.gradle.internal.os.OperatingSystem
import org.gradle.process.ExecOperations
import org.gradle.process.ExecResult
import org.gradle.process.ExecSpec
import org.gradle.process.internal.ExecException
import java.io.ByteArrayOutputStream
import javax.inject.Inject

abstract class ErrorLoggingExecTask @Inject constructor(private val ex: ExecOperations): DefaultTask() {
    class ExecContext(val envLogWhitelist: List<String> = listOf("^SAXON.*")) {
        private var _executable: String = ""
        private var _args = listOf<String>()
        private var _env = mapOf<String, Any>()
        var executable: String
            get() = _executable
            set(value) { _executable = value }
        var args: List<String>
            get() = _args
            set(value) { _args = value }
        var env: Map<String, Any>
            get() = _env
            set(value) {
                _env = filterEnv(value)
            }

        private fun filterEnv(unfiltered: Map<String, Any>): Map<String, Any> {
            val filters = envLogWhitelist.map { Regex(it) }
            return unfiltered.filter { (k, _) ->
                filters.any { it.matches(k) }
            }
        }

        override fun toString(): String {
            val out = listOf(executable, args.map { "\"${it}\"" }.joinToString(" ")) + env.map { (k, v) -> "$k = $v" }
            return out.joinToString("\n")
        }
    }

    fun execWithLogging(execSpec: Action<ExecSpec>): ExecResult {
        val output = ByteArrayOutputStream()
        val context = ExecContext()
        val res = ex.exec {
            execSpec.execute(this)
            context.executable = executable
            context.args = args
            context.env = environment
            standardOutput = output
            errorOutput = output
            setIgnoreExitValue(true)
        }
        try {
            res.assertNormalExitValue()
        } catch (e: ExecException) {
            logger.error(e.message)
            logger.error("Exec context:")
            logger.error(context.toString())
            throw e
        }

        return res
    }
}

abstract class PythonVenv @Inject constructor(ex: ExecOperations) : ErrorLoggingExecTask(ex) {
    @get:Internal
    abstract val pythonEnv: Property<SaxonCPythonExtension>

    @get:InputFile
    abstract val pyenvPythonExe: RegularFileProperty

    @get:Optional
    @get:Input
    abstract val suffix: Property<String>

    @get:Input
    abstract val branch: Property<String>

    @Internal
    val venvBaseDir: Provider<Directory> = pythonEnv.map { it.venvBaseDir.get() }

    @Internal
    val venvName: Provider<String> = branch.map {
        val finalSuffix = if (suffix.isPresent()) "-${suffix.get()}" else ""
        "${it}${finalSuffix}"
    }

    @Internal
    val venvPath: Provider<Directory> = venvBaseDir.map { it.dir(venvName.get()) }

    @Internal
    val venvBinPath: Provider<Directory> = venvPath.map { it.dir(pythonEnv.get().pythonBinDirName.get()) }

    @get:InputFile
    abstract val buildSystemBootstrap: RegularFileProperty

    @Internal
    val systemOs = OperatingSystem.current()

    @OutputFile
    val pythonExe: Provider<RegularFile> = venvPath.flatMap { venvBinHostedExe("python") }

    @OutputFile
    val pipExe: Provider<RegularFile> = venvPath.flatMap { venvBinHostedExe("pip3") }// envBinPath.map { it.file("pip3${sysVenvBinarySuffix}") }

    fun venvBinHostedExe(exe: String): Provider<RegularFile> {
        return venvBinPath.map {
            it.file("${exe}${pythonEnv.get().venvBinarySuffix.get()}")
        }
    }

    fun platformWheelFixupRequirements(): Array<String> {
        return if (systemOs.isLinux()) {
            arrayOf("auditwheel")
        } else if (systemOs.isMacOsX()) {
            arrayOf("delocate")
        } else if (systemOs.isWindows()) {
            arrayOf("delvewheel")
        } else {
            throw GradleException("Unknown Operating System")
        }
    }

    fun buildSystemBootstrapRequirements(): Array<String> {
        return PyprojectToml.buildSystemRequires(buildSystemBootstrap).toTypedArray()
    }

    @TaskAction
    fun execute() {
        logger.info("Creating Python Venv for ${branch.get()}: ${venvName.get()}")

        execWithLogging {
            executable(pyenvPythonExe.get())
            args("-m", "venv", venvPath.get())
        }

        execWithLogging {
            executable(pipExe.get().asFile.toString())
            args("install", "--require-virtualenv", *platformWheelFixupRequirements(), *buildSystemBootstrapRequirements())
        }
    }
}

/**
 * Task for installing packages from a Pip requirements.txt.
 * Note that the lack of outputs for this task means it will always run, which it needs to in order
 * to verify the state of installed packages.
 */
abstract class PipInstall @Inject constructor(private val ex: ExecOperations): DefaultTask() {
    @get:InputFile
    abstract val pipExe: RegularFileProperty

    @get:InputFile
    abstract val requirements: RegularFileProperty

    @TaskAction
    fun install() {
        val requirementsFile = requirements.get()
        if (installedRequirements(requirementsFile).startsWith(requirementsFile.asFile.readText())) {
            logger.info("Packages already installed")
        } else {
            ex.exec {
                executable(pipExe.get())
                args("install", "--require-virtualenv", "--requirement", requirementsFile)
            }
        }
    }

    fun installedRequirements(requirementsFile: RegularFile): String  {
        return ByteArrayOutputStream().use { stdout ->
            ex.exec {
                executable(pipExe.get())
                args("freeze", "--require-virtualenv", "--local", "--requirement", requirementsFile)
            }
            stdout.toString()
        }
    }
}
