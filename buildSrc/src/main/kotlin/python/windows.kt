package com.saxonica.build.python

import org.gradle.api.file.FileSystemOperations
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.tasks.OutputFile
import org.gradle.api.tasks.TaskAction
import java.io.ByteArrayOutputStream
import javax.inject.Inject

abstract class WinPythonVersionInstallTask : PythonVersionTask() {
    @get:OutputFile
    val _pythonExePathFile = pythonExePathFile.map { it }

    @TaskAction
    fun execute() {
        val v = _pythonVersion.get().version
        ex.exec {
            executable = "pymanager"
            args("install", v)
        }


        val exePath: String = ByteArrayOutputStream().use { outputStream ->
            ex.exec {
                executable = "pymanager"
                args("list", "--format=exe", "--one", v)
                standardOutput = outputStream
            }
            outputStream.toString()
        }

        _pythonExePathFile.get().asFile.writeText(exePath)
    }
}

abstract class WinPythonVersionUninstallTask : PythonVersionTask() {
    @get:Inject
    abstract val fs: FileSystemOperations

    @TaskAction
    fun uninstall() {
        if (pythonExePathFile.isPresent && pythonExePathFile.get().asFile.exists()) {
            fs.delete {
                delete(pythonExePathFile)
            }
        }

        val v = _pythonVersion.get().version
        ex.exec {
            executable = "pymanager"
            args("uninstall", "--yes", v)
        }
    }
}
