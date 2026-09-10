package com.saxonica.build.cpp

import org.gradle.api.DefaultTask
import org.gradle.api.file.ConfigurableFileCollection
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.ProjectLayout
import org.gradle.api.provider.ListProperty
import org.gradle.api.provider.Property
import org.gradle.process.ExecOperations
import javax.inject.Inject
import org.gradle.api.tasks.*

abstract class CMakeGeneratorTask @Inject constructor(private val ex: ExecOperations, private val layout: ProjectLayout) : DefaultTask() {
  @get:Internal
  abstract val buildPath: DirectoryProperty

  @get:Input
  abstract val cmakeFilenames: ListProperty<String>

  @get:Internal
  abstract val extraCMakeArgs: Property<() -> List<String>>

  @get:Input
  abstract val platformIsWindows: Property<Boolean>

  @get:Input
  abstract val version: Property<String>

  @get:InputFiles
  val cMakeInputFiles: ConfigurableFileCollection = project.objects.fileCollection()

  @get:InputFile
  val cMakeListsFile = project.file(layout.projectDirectory.file("CMakeLists.txt"))

  @get:OutputFiles
  val outputFiles = buildPath.map {
    it.files("CMakeCache.txt", "cmake_install.cmake") + it.files(cmakeFilenames)
  }

  @TaskAction
  fun generate() {
    val generatorName = platformIsWindows.map { isWindows -> if (isWindows) "Visual Studio 17 2022" else "Ninja Multi-Config" }
    ex.exec {
      workingDir = layout.projectDirectory.asFile
      executable = "cmake"
      args("-G", generatorName.get(), "-B", buildPath.map { it.asFile.toString() }.get(), "-D", version.map { "SAXON_VERSION:STRING=${it}" }.get())
      if (extraCMakeArgs.isPresent) args(extraCMakeArgs.get().invoke())
    }
  }
}
