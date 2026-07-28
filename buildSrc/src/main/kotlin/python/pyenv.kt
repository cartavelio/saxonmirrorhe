package com.saxonica.build.python

import com.saxonica.build.TargetHost
import com.saxonica.build.AsInput
import org.gradle.api.DefaultTask
import org.gradle.api.GradleException
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.FileSystemOperations
import org.gradle.api.file.ProjectLayout
import org.gradle.api.file.RegularFile
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.provider.ProviderFactory
import org.gradle.api.tasks.*
import org.gradle.process.ExecOperations
import org.gradle.process.ExecSpec
import java.io.ByteArrayOutputStream
import javax.inject.Inject


/**
 * Only needed for macOS / Linux - Windows uses the system-installed Python Install Manager pymanager
 */
abstract class PyenvExe @Inject constructor(private val ex: ExecOperations, private val layout: ProjectLayout, private val providers: ProviderFactory) : DefaultTask() {
    @get:Optional
    @get:Input
    abstract val pyenvExePath: Property<String>

    @OutputFile
    val pyenv: Provider<RegularFile> = pyenvExePath.orElse(findPyenvExePath()).map {
        layout.projectDirectory.file(it)
    }

    fun execString(exe: String , arguments: List<String>, env: Map<String, String> = mapOf()): String {
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

    fun findPyenvExePath(): Provider<String> {
        return providers.provider {
            execString("which", listOf("pyenv")).lines().last().trim()
        }
    }
}

abstract class PythonVersionTask: DefaultTask() {
    @get:Inject
    abstract val ex: ExecOperations

    @get:Inject
    abstract val layout: ProjectLayout

    @get:Internal
    abstract val targetHost: Property<TargetHost>

    @get:Input
    abstract val pythonVersion: Property<AsInput<PythonVersion>>

    @get:Internal
    val _pythonVersion = pythonVersion.map { it.get<PythonVersion>() }

    @get:Internal
    abstract val pythonVersionsRoot: DirectoryProperty

    @get:Input
    val _pythonVersionsRoot = pythonVersionsRoot.map { it.asFile.toString() }

    @get:Optional
    @get:Input
    abstract val debugVariant: Property<String>

    @get:Internal
    abstract val pythonExePathFile: RegularFileProperty
}

abstract class PyenvPythonVersionTask: PythonVersionTask() {
    @get:InputFile
    abstract val pyenvExe: RegularFileProperty

    @get:InputDirectory
    abstract val pythonBuildDefinitionsDir: DirectoryProperty

    @Internal
    val pyenvVersionName: Provider<String> = _pythonVersion.map {
        "${it.buildName}${if (debugVariant.isPresent()) "-${debugVariant.get()}" else "" }"
    }

    fun execToString(action: ExecSpec.() -> Unit): Triple<Boolean, String, String> {
        ByteArrayOutputStream().use { out ->
            ByteArrayOutputStream().use { err ->
                val result = ex.exec {
                    action(this)
                    standardOutput = out
                    errorOutput = err
                    setIgnoreExitValue(true)
                }

                return Triple(result.exitValue == 0, out.toString().trim(), err.toString().trim())
            }
        }
    }

    fun execWithPyenv(action: ExecSpec.() -> Unit): Triple<Boolean, String, String> {
        return execToString {
            environment("PYENV_VERSION", _pythonVersion.get().version)
            environment("PYENV_ROOT", _pythonVersionsRoot.get())
            action(this)
        }

    }

    fun checkPyenvLatest(pyenv: RegularFile, pythonBuildVersionName: String): Triple<Boolean, String, String> {
        return execWithPyenv {
            executable(pyenv)
            args("latest", pythonBuildVersionName)
        }
    }

    protected fun pythonExePath(pyenv: RegularFile): Triple<Boolean, String, String> {
        return execWithPyenv {
            executable(pyenv)
            args("which", "python")
        }
    }

    protected fun pythonExecutesNonZero(pythonExe: String): Boolean {
        val (ok, _, err) = execWithPyenv {
            executable(pythonExe)
            args("--version")
        }
        if (!ok) {
            logger.warn("Python version $pythonExe found, but exited non-zero:\n$err")
        }
        return ok
    }

    protected fun checkInstalled(pyenv: RegularFile, pythonBuildVersionName: String): Pair<Boolean, String> {
        val (claimedInstalled, latestOut, latestErr) = checkPyenvLatest(pyenv, pythonBuildVersionName)
        if (!claimedInstalled) {
            logger.info("$pythonBuildVersionName is not installed")
            logger.debug(latestOut)
            logger.debug(latestErr)
            return Pair(false, "")
        }
        val (ok, exePath, err) = pythonExePath(pyenv)
        if (!ok) {
            logger.info("Could not find python version $pythonBuildVersionName:\n$err")
        }
        return Pair(ok && pythonExecutesNonZero(exePath), exePath)
    }
}

abstract class InstallPyenvPythonVersionTask : PyenvPythonVersionTask() {
    @get:OutputFile
    val _pythonExePathFile = pythonExePathFile.map { it }

    @TaskAction
    fun execute() {
        // version
        //   if version in python/
        //      pyenv install /path/to/python/version
        //   else
        //      pyenv install version
        val _targetHost = targetHost.get()
        if (_targetHost.isWindows) {
            throw GradleException("pyenv tasks cannot run on Windows")
        }
        val pyenv = pyenvExe.get()
        var pythonBuildVersionName = pyenvVersionName.get()
        val (alreadyInstalled, existingPythonExePath) = checkInstalled(pyenv, pythonBuildVersionName)
        var pythonExePath = existingPythonExePath

        if (!alreadyInstalled) {
            val definitionFile = pythonBuildDefinitionsDir.get().file(pythonBuildVersionName)
            if (definitionFile.asFile.exists()) {
                pythonBuildVersionName = definitionFile.asFile.toString()
            }
            ex.exec {
                environment("PYENV_ROOT", _pythonVersionsRoot.get())
                executable(pyenv)
                if (_targetHost.isMacOS) {
                    environment("MACOSX_DEPLOYMENT_TARGET", _pythonVersion.get().macOsMinVersion(_targetHost))
                }
                args("install")
                if (debugVariant.isPresent() && !definitionFile.asFile.exists()) {
                    args("--debug")
                }
                args(pythonBuildVersionName)
            }

            val (installed, installedPythonExePath) = checkInstalled(pyenv, pythonBuildVersionName)
            if (!installed) {
                throw GradleException("Python version $pythonBuildVersionName installation failed")
            }
            pythonExePath = installedPythonExePath
        }

        _pythonExePathFile.get().asFile.writeText(pythonExePath)
    }
}

abstract class PyenvUninstallPythonTask : PyenvPythonVersionTask() {
    @get:Inject
    abstract val fs: FileSystemOperations

    @TaskAction
    fun uninstall() {
        if (pythonExePathFile.isPresent && pythonExePathFile.get().asFile.exists()) {
            fs.delete {
                delete(pythonExePathFile)
            }
        }

        val pythonBuildVersionName = pyenvVersionName.get()
        val (claimedInstalled, latestOut, latestErr) = checkPyenvLatest(pyenvExe.get(), pythonBuildVersionName)

        if (!claimedInstalled) {
            logger.info("${pythonBuildVersionName} is not installed")
            return
        }

        val (ok, out, err) = execWithPyenv {
            executable(pyenvExe.get())
            args("uninstall", "-f", latestOut)
        }
        logger.info(out)

        if (!ok) {
            throw GradleException("Unable to remove Python ${pythonBuildVersionName}:\n$out\n$err")
        } else {
            logger.info("pyenv uninstall successful:\n$out")
        }
    }
}
