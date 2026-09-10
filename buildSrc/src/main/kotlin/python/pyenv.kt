package com.saxonica.build.python

import org.gradle.api.Action
import org.gradle.api.DefaultTask
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.RegularFile
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.*
import org.gradle.internal.os.OperatingSystem
import org.gradle.process.ExecOperations
import org.gradle.process.ExecSpec
import java.io.ByteArrayOutputStream
import javax.inject.Inject

abstract class PyenvExe @Inject constructor(private val ex: ExecOperations) : DefaultTask() {
    @get:Optional
    @get:Input
    abstract val pyenvExePath: Property<String>

    @OutputFile
    val pyenv: Provider<RegularFile> = pyenvExePath.orElse(findPyenvExePath()).flatMap {
        val fileProp = project.objects.fileProperty()
        fileProp.set(project.file(it))
        fileProp
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

    // Needs something done to it for Windows
    fun findPyenvExePath(): Provider<String> {
        return project.providers.provider {
            val systemOs = OperatingSystem.current()
            val whichIsh = if (systemOs.isWindows()) "where.exe" else "which"
            execString(whichIsh, listOf("pyenv")).lines().last().trim()
        }
    }
}

abstract class PyenvPythonVersion @Inject constructor(private val ex: ExecOperations): DefaultTask() {
    @get:Internal
    abstract val os: Property<OperatingSystem>

    @get:Input
    abstract val pythonVersion: Property<String>

    @get:InputFile
    abstract val pyenvExe: RegularFileProperty

    @get:Input
    abstract val pyenvRoot: Property<String>

    @get:InputDirectory
    abstract val pythonBuildDefinitionsDir: DirectoryProperty

    @get:Optional
    @get:Input
    abstract val debugVariant: Property<String>

    @get:Optional
    @get:Input
    abstract val macosxDeploymentTarget: Property<String>

    @get:OutputFile
    val pythonExe = pythonVersion.map { project.layout.buildDirectory.file("python-${it}").get() }

    @Internal
    val pyenvVersionName: Provider<String> = pythonVersion.map {
        "${it}${if (debugVariant.isPresent()) "-${debugVariant.get()}" else "" }"
    }

    private fun pythonExePath(pyenv: RegularFile): String {
        return if (os.get().isWindows) {
            windowsExePath(pyenv)
        } else {
            nonWindowsExePath(pyenv)
        }
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

    private fun nonWindowsExePath(pyenv: RegularFile): String {
        return execToString {
            environment("PYENV_VERSION", pyenvVersionName.get())
            environment("PYENV_ROOT", pyenvRoot.get())
            executable(pyenv)
            args("which", "python")
        }
    }

    private fun windowsExePath(pyenv: RegularFile): String {
        val latestInstalledOfMajorVersion = execToString {
            executable(pyenv)
            args("latest", pyenvVersionName.get())
        }
        return execToString {
            executable("cmd.exe")
            args("/c")
            args(pyenv, "shell", latestInstalledOfMajorVersion, "&&", pyenv, "which", "python")
        }
    }

    @TaskAction
    fun execute() {
        // version
        // if not in pyenv versions
        //   if version in python/
        //      pyenv install /path/to/python/version
        //   else
        //      pyenv install version
        val pyenv = pyenvExe.get()
        var pythonBuildVersionName = pyenvVersionName.get()
        val execResult = ex.exec {
            if (!os.get().isWindows) {
                environment("PYENV_ROOT", pyenvRoot.get())
            }
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
                if (!os.get().isWindows) {
                    environment("PYENV_ROOT", pyenvRoot.get())
                }
                executable(pyenv)
                if (macosxDeploymentTarget.isPresent) {
                    environment("MACOSX_DEPLOYMENT_TARGET", macosxDeploymentTarget.get())
                }
                args("install", pythonBuildVersionName)
            }
        }

        pythonExe.get().asFile.writeText(pythonExePath(pyenv))
    }
}
