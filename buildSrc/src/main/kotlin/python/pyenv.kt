package com.saxonica.build.python

import org.gradle.api.DefaultTask
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.RegularFile
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.*
import org.gradle.internal.os.OperatingSystem
import java.io.ByteArrayOutputStream

abstract class PyenvExe : DefaultTask() {
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
            project.exec {
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

abstract class PythonPyenv: DefaultTask() {
    @get:Input
    abstract val pythonVersion: Property<String>

    @get:InputFile
    abstract val pyenvExe: RegularFileProperty

    @get:InputDirectory
    abstract val pythonBuildDefinitionsDir: DirectoryProperty

    @get:Optional
    @get:Input
    abstract val debugVariant: Property<String>

    @Internal
    val pyenvVersionName: Provider<String> = pythonVersion.map {
        "${it}${if (debugVariant.isPresent()) "-${debugVariant.get()}" else "" }"
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
        val pyenvVersions = ByteArrayOutputStream().use { out ->
            project.exec {
                executable(pyenv)
                args("versions", "--bare")
                standardOutput = out
            }
            out.toString().trim()
        }.lines()

        var pythonBuildVersionName = pyenvVersionName.get()
        if (!pyenvVersions.contains(pythonBuildVersionName)) {
            val definitionFile = pythonBuildDefinitionsDir.get().file(pythonBuildVersionName)
            if (definitionFile.asFile.exists()) {
                pythonBuildVersionName = definitionFile.asFile.toString()
            }
            project.exec {
                executable(pyenv)
                args("install", pythonBuildVersionName)
            }
        }
    }
}

abstract class PythonPyenvPythonExe: DefaultTask() {
    @get:Input
    abstract val pyenvVersionName: Property<String>

    @get:InputFile
    abstract val pyenvExe: RegularFileProperty

    @Internal
    val pythonExePath: Provider<String> = pyenvExe.map { pyenvPath ->
        ByteArrayOutputStream().use { out ->
            project.exec {
                environment("PYENV_VERSION", pyenvVersionName.get())
                executable(pyenvPath.asFile)
                args("which", "python")
                standardOutput = out
            }
            out.toString().trim()
        }
    }

    @OutputFile
    val pythonExe: Provider<RegularFile> = pythonExePath.flatMap {
        val fileProp = project.objects.fileProperty()
        fileProp.set(project.file(it))
        fileProp
    }
}
