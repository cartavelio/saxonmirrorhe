package com.saxonica.build.python

import org.gradle.api.DefaultTask
import org.gradle.api.GradleException
import org.gradle.api.file.Directory
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.provider.MapProperty
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.Input
import org.gradle.api.tasks.InputDirectory
import org.gradle.api.tasks.Internal
import org.gradle.api.tasks.OutputDirectory
import org.gradle.api.tasks.TaskAction
import org.gradle.api.tasks.TaskProvider
import org.gradle.process.ExecOperations
import javax.inject.Inject

abstract class SphinxDoc @Inject constructor(private val ex: ExecOperations): DefaultTask() {
  @get:Internal
  abstract val venv: Property<PythonVenv>

  @get:InputDirectory
  abstract val docSrcDir: DirectoryProperty

  @get:Input
  abstract val overrides: MapProperty<String, String>

  @get:Internal
  abstract val docBuildDir: DirectoryProperty

  @get:OutputDirectory
  val outputDir: Provider<Directory> = docBuildDir

  private var needsInstallerDependency: Boolean = true

  fun wheelInstaller(installTask: TaskProvider<InstallLocalWheel>) {
    this.dependsOn(installTask)
    needsInstallerDependency = false
  }

  @TaskAction
  fun build() {
    if (needsInstallerDependency) throw GradleException("wheel installer dependency is not set. Specify with wheelInstaller().")

    ex.exec {
      executable(venv.get().venvBinHostedExe("sphinx-build").get())
      args("-M", "html")
      overrides.get().forEach { (k, v) -> args("--define", "${k}=${v}") }
      args(docSrcDir.get().asFile, docBuildDir.get().asFile)
    }
  }
}
