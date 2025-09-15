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

class DebugPostfix(private val targetHost: TargetHost) {
  override fun toString(): String {
    return if (targetHost.isMacOS) "_debug" else "d"
  }
}

class LibFilenames(val libName: String, val targetHost: TargetHost) {
  val base: String get() = "${if (targetHost.isWindows) "" else "lib"}${libName}"

  fun platformSharedLibFilename(version: String): String  {
    return if (targetHost.isMacOS) {
      "${base}.${version}.dylib"
    } else if (targetHost.isLinux) {
      "${base}.so.${version}"
    } else {
      "${base}.dll"
    }
  }

  val importLibFilename: String get() = "${base}.lib"

  val platformLinkerNameFilename: String get() {
    val ext = if (targetHost.isMacOS) "dylib" else "so"
    return "${base}.${ext}"
  }
}

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
  val importLibDir = packageDir.map { it.dir("lib") }

  @get:Internal
  val includeDir = packageDir.map { it.dir("include") }

  @get:Internal
  val cmakeDir = packageDir.map { packageDir -> cmakePackageName.map { name -> packageDir.dir("lib/cmake/${name}") }.get() }

  @get:OutputDirectory
  val outputDir: Provider<Directory> = packageDir

  @get:Internal
  val libFilenames = libName.map { LibFilenames(it, targetHost) }

  @TaskAction
  fun build() {
    project.delete(packageDir)
    buildSharedLibDir()
    buildImportLibDir()
    buildIncludeDir()
    buildCMakeDir()
  }

  private fun symlinkLibrary(source: String, target: String) {
    ex.exec {
      workingDir = sharedLibDir.get().asFile
      executable = "ln"
      args("-nsf", source, target)
    }
  }

  private fun buildSharedLibDir() {
    val libFilenames = libFilenames.get()
    val realName = libFilenames.platformSharedLibFilename(version.get())
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
      rename({ _ -> realName })
    }
    if (!targetHost.isWindows) {
      val soName = libFilenames.platformSharedLibFilename(majorVersion.get())
      val linkName = libFilenames.platformLinkerNameFilename
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

      rename({ filename ->
        if (targetHost.isWindows) {
          libFilenames.get().importLibFilename
        } else {
          filename
        }
      })
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