package com.saxonica.build.php

import org.gradle.api.DefaultTask
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.provider.ListProperty
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.Input
import org.gradle.api.tasks.InputDirectory
import org.gradle.api.tasks.InputFile
import org.gradle.api.tasks.Internal
import org.gradle.api.tasks.Optional
import org.gradle.api.tasks.TaskAction
import org.gradle.process.ExecOperations
import javax.inject.Inject

abstract class PhpExec: DefaultTask() {
  @get:Inject
  abstract val ex: ExecOperations

  @get:Optional
  @get:InputFile
  abstract val saxonPhpExt: RegularFileProperty

  @get:Optional
  @get:InputDirectory
  abstract val libsaxoncCoreLibPath: DirectoryProperty

  @get:Optional
  @get:InputDirectory
  abstract val libsaxoncLibPath: DirectoryProperty



  @get:InputFile
  abstract val phpExe: RegularFileProperty

  @get:Internal
  abstract val workingDir: DirectoryProperty

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
    val wd = workingDir.get().asFile

    ex.exec {
      workingDir = wd

      executable = phpExe.get().asFile.absolutePath

      if (libsaxoncLibPath.isPresent) {
        environment("DYLD_LIBRARY_PATH", "${libsaxoncLibPath.get().asFile}:${libsaxoncCoreLibPath.get().asFile}")
      }
      if (saxonPhpExt.isPresent) {
        args("-dextension=${saxonPhpExt.get().asFile.absolutePath}")
      }
      args(_args.get())
    }
  }
}