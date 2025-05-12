package com.saxonica.build.java

import org.gradle.api.DefaultTask
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.provider.MapProperty
import org.gradle.api.tasks.Input
import org.gradle.api.tasks.OutputDirectory
import org.gradle.api.tasks.TaskAction

abstract class JarManifestExtrasTask : DefaultTask() {
  @get:OutputDirectory
  abstract val outputDir: DirectoryProperty

  @get:Input
  abstract val serviceEntries: MapProperty<String, String>

  @TaskAction
  fun action() {
    val servicesDir = outputDir.get().dir("META-INF/services")
    val services = serviceEntries.get()
    if (!services.isEmpty()) {
      servicesDir.asFile.mkdirs()
      services.forEach() { entry ->
        servicesDir.file(entry.key).asFile.writeText(entry.value)
      }
    }
  }
}