package com.saxonica.build.java

import org.gradle.api.GradleException
import org.gradle.api.artifacts.Configuration
import java.io.File

class JarFinder (private val configuration: Configuration) {
    fun find(jarname: String): File {
        val jar = configuration.getFiles()
            .filter { jar -> jar.toString().endsWith(jarname) }
            .elementAtOrNull(0)
        if (jar == null) {
            throw GradleException("Cannot find ${jarname} in configuration")
        }
        return jar
    }
}