package com.saxonica.build

import org.gradle.api.DefaultTask
import org.gradle.api.file.ConfigurableFileCollection
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.FileSystemOperations
import org.gradle.api.tasks.InputFiles
import org.gradle.api.tasks.Internal
import org.gradle.api.tasks.TaskAction
import org.gradle.api.tasks.options.Option
import javax.inject.Inject

abstract class GatherReleaseArtifacts @Inject constructor(private val fs: FileSystemOperations) : DefaultTask() {
  @get:InputFiles
  val artifacts: ConfigurableFileCollection = project.objects.fileCollection()

  @get:Internal
  abstract val outputDir: DirectoryProperty

  @Option(option = "to", description = "set the test specifiers passed to pytest")
  fun setOutput(path: String): GatherReleaseArtifacts {
    outputDir.set(project.file(path))
    return this;
  }

  @TaskAction
  fun gather() {
    fs.copy {
      from(artifacts)
      into(outputDir)
    }
  }
}