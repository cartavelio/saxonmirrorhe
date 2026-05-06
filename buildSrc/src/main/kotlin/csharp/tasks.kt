package com.saxonica.build.csharp

import org.gradle.api.Action
import org.gradle.api.DefaultTask
import org.gradle.api.file.ConfigurableFileCollection
import org.gradle.api.provider.Property
import org.gradle.api.tasks.Input
import org.gradle.api.tasks.InputFile
import org.gradle.api.tasks.InputFiles
import org.gradle.api.tasks.Internal
import org.gradle.api.tasks.OutputFile
import org.gradle.api.tasks.TaskAction
import org.gradle.process.ExecOperations
import java.io.File
import java.util.UUID
import javax.inject.Inject
import kotlin.text.uppercase
import org.gradle.kotlin.dsl.assign
import org.gradle.process.ExecSpec


abstract class DotNetImplementation: DefaultTask() {
  @get:Inject
  protected abstract val ex: ExecOperations

  @get:Input
  abstract val configuration: Property<String>

  @get:Input
  abstract val edition: Property<String>

  @get:Input
  abstract val version: Property<String>

  @get:Input
  abstract val target: Property<String>

  @get:Input
  abstract val dotnet: Property<String>

  @get:Input
  abstract val arch: Property<String>

  @get:Input
  abstract val platform: Property<String>

  @get:Internal
  val cwd = arch.map { arch -> project.layout.buildDirectory.dir("${arch}/${target.get()}").get().asFile }

  public fun dotnetExec(command: String, execBlock: Action<ExecSpec>) {
    ex.exec {
      workingDir = cwd.get()
      executable = dotnet.get()
      args(command)
      execBlock.execute(this)
    }
  }

  init {
    configuration.convention("${project.findProperty("csharpBuild") ?: "Debug"}")
  }
}

abstract class DotNetConfiguration @Inject constructor(private val ex: ExecOperations): DefaultTask() {
  @get:InputFile
  val generateCsProjFiles = project.layout.projectDirectory.file("../tools/generateCSProjFiles.xsl")

  @get:OutputFile
  val appSlnFile = this.arch.map { arch -> project.layout.buildDirectory.file("${arch}/SaxonCS/SaxonCS.sln").get() }

  @get:OutputFile
  val testSlnFile = this.arch.map { arch -> project.layout.buildDirectory.file("${arch}/TestDrivers/TestDrivers.sln").get() }

  @get:OutputFile
  val nunitSlnFile = this.arch.map { arch -> project.layout.buildDirectory.file("${arch}/NUnitTests/NUnitTests.sln").get() }

  @get:Input
  abstract val edition: Property<String>

  @get:Input
  abstract val version: Property<String>

  @get:Input
  abstract val arch: Property<String>

  @get:Input
  abstract val platform: Property<String>

  @get:Input
  abstract val release: Property<String>

  @get:Input
  abstract val dotnetversion: Property<String>

  @get:InputFiles
  abstract val classpath: ConfigurableFileCollection

  private
  fun genArgs(outputFile: File, releaseBuild: String): Array<String> {
    return arrayOf(
      "-it",
      "-xsl:${generateCsProjFiles}",
      "-o:${outputFile}",
      "release=${release.get()}",
      "releaseBuild=${releaseBuild}",
      "platform=${platform.get()}",
      "arch=${arch.get()}",
      "edition=${edition.get()}",
      "framework=net${dotnetversion.get()}",
      "saxonVersion=${version.get()}",
      "uuid1=${UUID.randomUUID()}",
      "uuid2=${UUID.randomUUID()}"
    )
  }


  @TaskAction
  fun makeCSProj() {
    val cp = classpath
    ex.javaexec {
      classpath = cp
      mainClass = "net.sf.saxon.Transform"
      args(*genArgs(appSlnFile.get().asFile, "SaxonCS"))
    }

    ex.javaexec {
      classpath = cp
      mainClass = "net.sf.saxon.Transform"
      args(*genArgs(testSlnFile.get().asFile, "TestDrivers"))
    }

    ex.javaexec {
      classpath = cp
      mainClass = "net.sf.saxon.Transform"
      args(*genArgs(nunitSlnFile.get().asFile, "NUnitTests"))
    }
  }
}

abstract class DotNetBuild : DotNetImplementation() {
  @TaskAction
  fun build() {
    val solution = "${target.get()}.sln"

    println("-p:DefineConstants=${edition.get().uppercase()}")
    dotnetExec("restore") {
      args(solution, "--verbosity", "Normal",
        "-p:Configuration=${configuration.get()}", "-p:Platform=Any CPU")
    }

    dotnetExec("build") {
      args(solution, "--verbosity", "Normal",
        "/nodereuse:false", "--no-restore",
        "-p:Version=${version.get()}",
        "-p:PackageVersion=${version.get()}",
        "-p:Configuration=${configuration.get()}", "-p:Platform=Any CPU",
        "-p:DefineConstants=${edition.get().uppercase()}")
    }
  }

  init {
    target.convention("SaxonCS") // by default
  }
}

abstract class DotNetPublish : DotNetImplementation() {
  @TaskAction
  fun build() {
    val csrid = "${platform.get()}-${arch.get()}"
    val solution = "${target.get()}.sln"

    dotnetExec("publish") {
      args(solution, "-r", csrid,
        "--self-contained", "true",
        "-p:PublishSingleFile=true",
        "-p:Version=${version.get()}", "-p:PackageVersion=${version.get()}",
        "-p:Configuration=${configuration.get()}", "-p:Platform=Any CPU")
    }
  }

  init {
    target.convention("SaxonCS") // by default
  }
}

abstract class DotNetMakeApp : DotNetImplementation() {
  @get:Internal
  val execwd = this.arch.map { project.layout.buildDirectory.file("${it}/SaxonCS").get().asFile }

  @TaskAction
  fun build() {
    val configuration = this.configuration.get()

    dotnetExec("msbuild") {
      args("-t:BundleApp",
        "-p:Configuration=${configuration}",
        "SaxonCS.csproj")
    }
  }
}
