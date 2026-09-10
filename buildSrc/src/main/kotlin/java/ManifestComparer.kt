package com.saxonica.build.java

import org.gradle.api.DefaultTask
import org.gradle.api.GradleException
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.provider.Property
import org.gradle.api.tasks.InputFile
import org.gradle.api.tasks.Internal
import org.gradle.api.tasks.OutputFile
import org.gradle.api.tasks.TaskAction
import java.io.File
import java.io.FileInputStream
import java.util.zip.ZipInputStream

abstract class ManifestComparer : DefaultTask() {
    @get:InputFile
    abstract val jar: RegularFileProperty

    @get:InputFile
    abstract val goldJarManifest: RegularFileProperty

    @get:Internal
    abstract val releaseBuild: Property<Boolean>

    @get:Internal
    abstract val expectSignedJar: Property<Boolean>

    @get:OutputFile
    abstract val newFilesLog: RegularFileProperty

    @get:OutputFile
    abstract val missingFilesLog: RegularFileProperty

    class GeneratedManifest(val manifest: Set<String>, val signature: Boolean)

    private fun generateManifest(jar: File): GeneratedManifest {
        val jarManifest = mutableSetOf<String>()
        var signature = false

        FileInputStream(jar).use { bufferedInputStream ->
            ZipInputStream(bufferedInputStream).use { zipInputStream ->
                var zipEntry = zipInputStream.nextEntry
                while (zipEntry != null) {
                    signature = signature || zipEntry.name.contains("META-INF/TE-")
                    if (!zipEntry.name.contains("$") && !zipEntry.name.endsWith("/")) {
                        jarManifest.add(zipEntry.name)
                    }
                    zipEntry = zipInputStream.nextEntry
                }
            }
        }

        return GeneratedManifest(jarManifest, signature)
    }

    @TaskAction
    fun compareManifests() {
        val jarManifest = generateManifest(jar.get().asFile)

        val goldManifest = goldJarManifest.get().asFile.readLines().filter { fn ->
            !fn.endsWith("/") && (jarManifest.signature || !fn.contains("META-INF/TE-"))
        }.toSet()
        val newFiles = jarManifest.manifest - goldManifest
        val missingFiles = goldManifest - jarManifest.manifest

        if (newFiles.isEmpty() && missingFiles.isEmpty()) {
            newFilesLog.get().asFile.writeText("")
            missingFilesLog.get().asFile.writeText("")
            return
        }
        newFilesLog.get().asFile.writer().use { log ->
            newFiles.forEach { log.appendLine(it) }
        }
        missingFilesLog.get().asFile.writer().use { log ->
            missingFiles.forEach { log.appendLine(it) }
        }

        println("COMPARE: ${jar.get().asFile}")
        println("       : ${goldJarManifest.get().asFile}")
        newFiles.forEach { fn ->
            println("New file: ${fn}")
        }
        missingFiles.forEach { fn ->
            println("Missing file: ${fn}")
        }

        if (expectSignedJar.get() && !jarManifest.signature) {
            println("WARNING: did not find expected jar signature")
        }

        if (releaseBuild.get() && !missingFiles.isEmpty()) {
            throw GradleException("ERROR: Jar file appears to be incomplete")
        }
    }
}