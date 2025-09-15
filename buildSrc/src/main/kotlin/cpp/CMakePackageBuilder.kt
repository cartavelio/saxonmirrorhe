package com.saxonica.build.cpp

import com.saxonica.build.TargetHost
import org.gradle.api.DefaultTask
import org.gradle.api.file.ConfigurableFileCollection
import org.gradle.api.file.Directory
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.*
import org.gradle.process.ExecOperations
import javax.inject.Inject

abstract class CMakePackageBuilder @Inject constructor(private val ex: ExecOperations) : DefaultTask() {
  private val targetHost = TargetHost.current()

  @get:Internal
  abstract val packageDir: DirectoryProperty

  @get:Input
  abstract val libName: Property<String>

  @get:Input
  abstract val version: Property<String>

  @get:Input
  abstract val cmakePackageName: Property<String>

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

  @get:Input
  abstract val debugConfig: Property<Boolean>

  @get:Internal
  val sharedLibDir: Provider<Directory> = packageDir.map { it.dir(if (targetHost.isWindows) "bin" else "lib") }

  @get:Internal
  val libFilenameWithoutExtension: Provider<String> = libName.map { "${if (targetHost.isWindows) "" else "lib"}${it}" }

  @get:Internal
  val importLibDir = packageDir.map { it.dir("lib") }

  @get:Internal
  val includeDir = packageDir.map { it.dir("include") }

  @get:Internal
  val cmakeDir = packageDir.map { packageDir -> cmakePackageName.map { name -> packageDir.dir("lib/cmake/${name}") }.get() }

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

  private fun debugPostfix(): String {
    if (!debugConfig.get()) {
      return ""
    }
    if (targetHost.isMacOS) {
      return "_debug"
    } else {
      return "d"
    }
  }

  private fun platformSharedLibFilename(base: String, libversion: String): (String) -> String  {
    return { _ ->
      if (targetHost.isMacOS) {
        "${base}${debugPostfix()}.${libversion}.dylib"
      } else if (targetHost.isLinux) {
        "${base}${debugPostfix()}.so.${libversion}"
      } else {
        "${base}${debugPostfix()}.dll"
      }
    }
  }

  private fun importLibFilename(base: String, libversion: String): (String) -> String  {
    return { filename ->
      if (targetHost.isWindows) {
        "${base}${debugPostfix()}.lib"
      } else {
        filename
      }
    }
  }

  private fun platformLinkerNameFilename(base: String): (String) -> String {
    return { _ ->
      val ext = if (targetHost.isMacOS) "dylib" else "so"
      "${base}${debugPostfix()}.${ext}"
    }
  }

  private fun symlinkLibrary(source: (String) -> String, target: (String) -> String) {
    ex.exec {
      workingDir = sharedLibDir.get().asFile
      executable = "ln"
      args("-nsf", source.invoke(""), target.invoke(""))
    }
  }

  private fun buildSharedLibDir() {
    val baseLibFilename = libFilenameWithoutExtension.get()
    val realName = platformSharedLibFilename(baseLibFilename, version.get())
    if (!targetHost.isWindows) {
      // project.sync gets itself in a mess because of symlinks pointed at symlinks in the lib dir.
      // This is a bug in Gradle, hence this nasty workaround. It's mac/linux only because there
      // are no symlinks involved on Windows.
      ex.exec {
        executable = "rm"
        args("-rf", sharedLibDir.get().asFile)
      }
    }
    project.copy {
      from(sharedLibFiles)
      into(sharedLibDir)
      rename(realName)
    }
    if (!targetHost.isWindows) {
      val soName = platformSharedLibFilename(baseLibFilename, majorVersion.get())
      val linkName = platformLinkerNameFilename(baseLibFilename)
      symlinkLibrary(realName, soName)
      symlinkLibrary(soName, linkName)
    }
  }

  private fun buildImportLibDir() {
    val importLibRenamer = importLibFilename(libFilenameWithoutExtension.get(), version.get())
    // Cannot use project.sync here because it just nukes the lib dir on Unices
    // and then causes broken symlinks which causes the sync task to explode in
    // a little fireball
    project.copy {
      from(importLibFiles)
      into(importLibDir)
      rename(importLibRenamer)
    }
  }

  private fun buildIncludeDir() {
    project.copy {
      from(headerFiles)
      into(includeDir)
    }
  }
  private fun buildCMakeDir() {
    // We need the task to remove release files when debug is present and vice versa
    project.copy {
      from(packageCMakeFiles)
      into(cmakeDir)
    }
  }
}