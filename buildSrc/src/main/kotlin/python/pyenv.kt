package com.saxonica.build.python

import com.saxonica.build.TargetHost
import com.saxonica.build.AsInput
import org.gradle.api.DefaultTask
import org.gradle.api.GradleException
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.ProjectLayout
import org.gradle.api.file.RegularFile
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.model.ObjectFactory
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.provider.ProviderFactory
import org.gradle.api.tasks.*
import org.gradle.internal.os.OperatingSystem
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

    @get:OutputFile
    val pythonExe = _pythonVersion.map { layout.buildDirectory.file("python-${it.version}").get() }

    @get:Internal
    abstract val pythonVersionsRoot: DirectoryProperty

    @get:Input
    val _pythonVersionsRoot = pythonVersionsRoot.map { it.asFile.toString() }

    @get:Optional
    @get:Input
    abstract val debugVariant: Property<String>
}

abstract class PyenvPythonVersionTask : PythonVersionTask() {
    @get:InputFile
    abstract val pyenvExe: RegularFileProperty

    @get:InputDirectory
    abstract val pythonBuildDefinitionsDir: DirectoryProperty

    @Internal
    val pyenvVersionName: Provider<String> = _pythonVersion.map {
        "${it.buildName}${if (debugVariant.isPresent()) "-${debugVariant.get()}" else "" }"
    }

    private fun execToString(action: ExecSpec.() -> Unit): String {
        ByteArrayOutputStream().use { out ->
            ex.exec {
                action(this)
                standardOutput = out
            }
            return out.toString().trim()
        }
    }

    private fun pythonExePath(pyenv: RegularFile): String {
        return execToString {
            environment("PYENV_VERSION", pyenvVersionName.get())
            environment("PYENV_ROOT", _pythonVersionsRoot.get())
            executable(pyenv)
            args("which", "python")
        }
    }

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
        val execResult = ex.exec {
            environment("PYENV_ROOT", _pythonVersionsRoot.get())
            executable(pyenv)
            args("latest", pythonBuildVersionName)
            setIgnoreExitValue(true)
        }
        val notInstalled = execResult.exitValue != 0

        if (notInstalled) {
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
                args("install", pythonBuildVersionName)
            }
        }

        pythonExe.get().asFile.writeText(pythonExePath(pyenv))
    }
}
