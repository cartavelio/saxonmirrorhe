package com.saxonica.build.cpp

import org.gradle.api.DefaultTask
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.provider.ListProperty
import org.gradle.api.provider.Property
import org.gradle.process.ExecOperations
import javax.inject.Inject
import org.gradle.api.tasks.*

abstract class ErsatzCMakeGenerator @Inject constructor(private val ex: ExecOperations) : DefaultTask() {
  @get:Internal
  abstract val buildPath: DirectoryProperty

  @get:Input
  abstract val cmakeFilenames: ListProperty<String>

  @get:Input
  abstract val platformIsWindows: Property<Boolean>

  @get:Input
  abstract val version: Property<String>

  @InputFiles
  val inputFiles = project.files(project.layout.projectDirectory.dir("../cmake"), project.layout.projectDirectory.file("CMakeLists.txt"))

  @OutputFiles
  val outputFiles = buildPath.map {
    it.files("CMakeCache.txt", "cmake_install.cmake")
    it.files(cmakeFilenames)
  }

  @TaskAction
  fun generate() {
    val generatorName = platformIsWindows.map { isWindows -> if (isWindows) "Visual Studio 17 2022" else "Ninja Multi-Config" }
    ex.exec {
      workingDir = project.layout.projectDirectory.asFile
      executable = "cmake"
      args("-G", generatorName.get(), "-B", buildPath.map { it.asFile.toString() }.get(), "-D", version.map { "SAXON_VERSION:STRING=${it}" }.get())
    }
  }
}
