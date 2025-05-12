package com.saxonica.build

import java.io.File

class ExecutableFinder(private val pathEnvEntrySeparator: String, private val fileSeparator: String) {
    fun find(executable: String, extraPaths: List<String> = emptyList()): String? {
        val spath = extraPaths + System.getenv("PATH").split(pathEnvEntrySeparator)
        var found: String? = null
        spath.forEach { path ->
            if (found == null) {
                val fn = File(path + fileSeparator + executable)
                if (fn.exists() && fn.canExecute()) {
                    found = fn.toString()
                } else {
                    val wfn = File(path + fileSeparator + executable + ".exe")
                    if (wfn.exists() && wfn.canExecute()) {
                        found = wfn.toString()
                    }
                }
            }
        }

        return found
    }
}