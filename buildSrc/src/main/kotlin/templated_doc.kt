package com.saxonica.build

import com.saxonica.build.doc.Model
import io.pebbletemplates.pebble.PebbleEngine
import io.pebbletemplates.pebble.loader.DelegatingLoader
import io.pebbletemplates.pebble.loader.FileLoader
import org.gradle.api.DefaultTask
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.provider.Property
import org.gradle.api.tasks.Input
import org.gradle.api.tasks.InputDirectory
import org.gradle.api.tasks.Internal
import org.gradle.api.tasks.Optional
import org.gradle.api.tasks.OutputFile
import org.gradle.api.tasks.TaskAction
import java.io.FileWriter

abstract class TemplatedDocTask : DefaultTask() {
  @get:InputDirectory
  abstract val templateRoot: DirectoryProperty

  @get:Optional
  @get:InputDirectory
  abstract val editionTemplateRoot: DirectoryProperty

  @get:OutputFile
  abstract val renderedDoc: RegularFileProperty

  @get:Internal
  abstract val model: Property<Model>

  @get:Input
  abstract val template: Property<String>

  private val fileLoaders: List<FileLoader> get() = listOf(editionTemplateRoot, templateRoot).filter { it.isPresent }.map { FileLoader(it.get().asFile.toString()) }
  @TaskAction
  fun generate() {
    val loader = DelegatingLoader(fileLoaders)

    val engine = PebbleEngine.Builder().loader(loader).build()

    val compiledTemplate = engine.getTemplate(template.get());
    val writer = FileWriter(renderedDoc.get().asFile)


    compiledTemplate.evaluate(writer, mapOf("model" to model.get()));
  }
}