package com.saxonica.build.python

import org.gradle.api.tasks.TaskAction
import java.io.ByteArrayOutputStream

abstract class WinPythonVersionTask : PythonVersionTask() {
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

        pythonExe.get().asFile.writeText(exePath)
    }
}
