package com.saxonica.build.php

import org.gradle.api.GradleException
import org.gradle.api.provider.Provider
import org.gradle.api.provider.ProviderFactory
import java.io.File
import java.nio.file.Paths
import kotlin.io.path.exists
import kotlin.io.path.isDirectory

class PhpFinder(val providers: ProviderFactory) {
    private val phpEnv = providers.environmentVariable("SAXON_PHP")
    private val phpizeEnv = providers.environmentVariable("SAXON_PHPIZE")
    private val makeEnv = providers.environmentVariable("SAXON_PHP_MAKE")

    private fun envVarPresent(envVarProvider: Provider<String>): Boolean {
        return envVarProvider.isPresent() && envVarProvider.get() != ""
    }

    val php: Provider<File> get() {
        return providers.provider<File> { if (envVarPresent(phpEnv)) checkExe(phpEnv.get())!! else findExe("php")!! }
    }

    val phpize: Provider<File> get() {
        return providers.provider<File> {
            if (envVarPresent(phpizeEnv)) {
                checkExe(phpizeEnv.get())!!
            } else {
                if (envVarPresent(phpEnv)) checkExe(phpEnv.get(), "phpize")!! else findExe("phpize")!!
            }
        }
    }

    val make: Provider<File> get() {
        return providers.provider<File> { if (envVarPresent(makeEnv)) checkExe(makeEnv.get())!! else findExe("make")!! }
    }

    private fun findExe(exeName: String) : File {
        listOf("/opt/homebrew/bin", "/usr/local/bin", "/usr/bin", "/bin").forEach { root ->
            val rdir = File(root).toPath()
            if (rdir.exists() && rdir.isDirectory()) {
                val exe = rdir.resolve(exeName)
                if (exe.exists()) return exe.toFile()
            }
        }
        throw GradleException("Cannot build PHP ext, cannot find <${exeName}> executable")
    }

    private fun checkExe(exe: String) : File {
        val exePath = Paths.get(exe)
        if (exePath.exists()) return exePath.toFile()
        throw GradleException("Cannot find PHP-related executable <${exe}>")
    }

    private fun checkExe(exe: String, exeName: String) : File {
        val exePath = Paths.get(exe)
        if (exePath.exists()) return checkExe(exePath.parent.resolve(exeName).toString())
        throw GradleException("Cannot find PHP-related executable <${exeName}> in <${exePath.parent}>")
    }

}