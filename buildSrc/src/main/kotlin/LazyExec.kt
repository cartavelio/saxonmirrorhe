package com.saxonica.build

import org.gradle.api.DefaultTask
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.RegularFile
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.provider.ListProperty
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.Input
import org.gradle.api.tasks.InputDirectory
import org.gradle.api.tasks.Internal
import org.gradle.api.tasks.Optional
import org.gradle.api.tasks.TaskAction
import org.gradle.process.ExecOperations
import java.io.File
import javax.inject.Inject

abstract class LazyExec: DefaultTask() {
  @get:Inject
  abstract val ex: ExecOperations

  @get:Input
  abstract val executable: Property<String>

  fun executable(exe: Any) {
    if (exe is Provider<*>) {
      executable.set(exe.map { mapExecutable(it) })
    } else {
      executable.set(mapExecutable(exe))
    }
  }

  fun mapExecutable(exe: Any): String {
    return if (exe is String) {
      exe
    } else if (exe is RegularFile) {
      exe.asFile.toString()
    } else if (exe is File) {
      exe.toString()
    } else {
      throw IllegalArgumentException("Unsupported executable type")
    }
  }

  @get:Internal
  abstract val workingDir: DirectoryProperty

  @get:Optional
  @get:Input
  val _workingDir: Provider<File> = workingDir.map { it.asFile }

  @get:Input
  abstract val _args: ListProperty<String>

  fun args(vararg args: String) {
    args.forEach { arg ->
      _args.add(arg)
    }
  }

  fun args(vararg args: Provider<String>) {
    args.forEach { arg ->
      _args.add(arg)
    }
  }

  @TaskAction
  fun exec() {
    val wd = workingDir
    val exe = executable.get()

    ex.exec {
      if (wd.isPresent) {
        workingDir = wd.get().asFile
      }

      executable = exe
      args(_args.get())
    }
  }

}