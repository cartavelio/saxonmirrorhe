package com.saxonica.build.cpp

import org.gradle.api.DefaultTask
import org.gradle.api.file.ConfigurableFileCollection
import org.gradle.api.file.Directory
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.*
import org.gradle.internal.os.OperatingSystem

abstract class CMakePackageBuilder : DefaultTask() {
  private val systemOs: OperatingSystem = OperatingSystem.current()

  @get:Internal
  abstract val packageDir: DirectoryProperty

  @get:Input
  abstract val libName: Property<String>

  @get:Input
  abstract val version: Property<String>

  @get:Input
  abstract val majorVersion: Property<String>

  @get:InputFiles
  abstract val sharedLibFiles: ConfigurableFileCollection

  @get:InputFiles
  abstract val importLibFiles: ConfigurableFileCollection

  @get:InputFiles
  abstract val headerFiles: ConfigurableFileCollection

  @get:InputFiles
  abstract val packageCMakeFiles: ConfigurableFileCollection

  @get:Internal
  val sharedLibDir: Provider<Directory> = packageDir.map { it.dir(if (systemOs.isWindows) "bin" else "lib") }

  @get:Internal
  val libFilenameWithoutExtension: Provider<String> = libName.map { "lib${it}" }

  @get:Internal
  val importLibDir = packageDir.map { it.dir("lib") }

  @get:Internal
  val includeDir = packageDir.map { it.dir("include") }

  @get:Internal
  val cmakeDir = packageDir.map { it.dir("lib/cmake") }

  @get:OutputDirectory
  val outputDir: Provider<Directory> = packageDir

  @TaskAction
  fun build() {
    project.delete(packageDir)
    buildSharedLibDir()
    buildImportLibDir()
    buildIncludeDir()
    buildCMakeDir()
  }

  private fun platformSharedLibFilename(base: String, libversion: String): (String) -> String  {
    return { filename ->
      if (systemOs.isMacOsX) {
        "${base}.${libversion}.dylib"
      } else if (systemOs.isLinux) {
        "${base}.so.${libversion}"
      } else {
        filename
      }
    }
  }

  private fun platformLinkerNameFilename(base: String): (String) -> String {
    return { _ ->
      val ext = if (systemOs.isMacOsX) "dylib" else "so"
      "${base}.${ext}"
    }
  }

  private fun symlinkLibrary(source: (String) -> String, target: (String) -> String) {
    project.exec {
      workingDir = sharedLibDir.get().asFile
      executable = "ln"
      args("-nsf", source.invoke(""), target.invoke(""))
    }
  }

  private fun buildSharedLibDir() {
    val baseLibFilename = libFilenameWithoutExtension.get()
    val realName = platformSharedLibFilename(baseLibFilename, version.get())
    if (!systemOs.isWindows) {
      // project.sync gets itself in a mess because of symlinks pointed at symlinks in the lib dir.
      // This is a bug in Gradle, hence this nasty workaround. It's mac/linux only because there
      // are no symlinks involved on Windows.
      project.exec {
        executable = "rm"
        args("-rf", sharedLibDir.get().asFile)
      }
    }
    project.copy {
      from(sharedLibFiles)
      into(sharedLibDir)
      rename(realName)
    }
    if (!systemOs.isWindows) {
      val soName = platformSharedLibFilename(baseLibFilename, majorVersion.get())
      val linkName = platformLinkerNameFilename(baseLibFilename)
      symlinkLibrary(realName, soName)
      symlinkLibrary(soName, linkName)
    }
  }

  private fun buildImportLibDir() {
    // Cannot use project.sync here because it just nukes the lib dir on Unices
    // and then causes broken symlinks which causes the sync task to explode in
    // a little fireball
    project.copy {
      from(importLibFiles)
      into(importLibDir)
    }
  }

  private fun buildIncludeDir() {
    project.copy {
      from(headerFiles)
      into(includeDir)
    }
  }
  private fun buildCMakeDir() {
    project.copy {
      from(packageCMakeFiles)
      into(cmakeDir)
    }
  }
}