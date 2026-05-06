package com.saxonica.build.csharp

import com.saxonica.build.Architectures
import com.saxonica.build.Platforms
import kotlinx.serialization.Serializable
import org.gradle.api.Action
import org.gradle.api.DefaultTask
import org.gradle.api.file.ConfigurableFileCollection
import org.gradle.api.file.Directory
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.FileCollection
import org.gradle.api.file.FileSystemOperations
import org.gradle.api.file.ProjectLayout
import org.gradle.api.file.RegularFile
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.model.ObjectFactory
import org.gradle.api.provider.ListProperty
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.Input
import org.gradle.api.tasks.InputFile
import org.gradle.api.tasks.InputFiles
import org.gradle.api.tasks.Internal
import org.gradle.api.tasks.OutputDirectory
import org.gradle.api.tasks.OutputFile
import org.gradle.api.tasks.OutputFiles
import org.gradle.api.tasks.TaskAction
import org.gradle.process.ExecOperations
import java.io.File
import java.util.UUID
import javax.inject.Inject
import kotlin.text.uppercase
import org.gradle.kotlin.dsl.assign
import org.gradle.process.ExecSpec

@Serializable
enum class CSReleaseConfiguration(val value: String) {
  Release("Release"),
  Debug("Debug");

  override fun toString(): String = value
}

abstract class MSBuildConventions : DefaultTask() {
  @get:Input
  abstract val arch: Property<Architectures>

  @get:Internal
  val msbuildArch = arch.map { it.msbuild }

  @get:Input
  abstract val platform: Property<Platforms>

  @get:Internal
  val msbuildPlatform = platform.map { it.msbuild }
}

abstract class DotNetRunner: DefaultTask() {
  @get:Inject
  protected abstract val ex: ExecOperations

  @get:Input
  abstract val dotnet: Property<String>

  @get:Input
  abstract val command: Property<String>

  @get:Input
  abstract val _args: ListProperty<String>

  fun args(vararg args: String) {
    _args.addAll(args.toList())
  }

  fun args(vararg args: Provider<String>) {
    args.forEach { arg -> _args.add(arg) }
  }

  @TaskAction
  fun execute() {
    ex.exec {
      executable = dotnet.get()
      this.args(command.get())
      this.args(_args.get())
    }
  }
}

abstract class DotNetImplementation: MSBuildConventions() {
  @get:Inject
  protected abstract val ex: ExecOperations

  @get:Inject
  protected abstract val projectLayout: ProjectLayout

  @get:Input
  abstract val configuration: Property<CSReleaseConfiguration>

  @get:Input
  abstract val edition: Property<String>

  @get:Input
  abstract val version: Property<String>

  @get:Input
  abstract val target: Property<String>

  @get:Input
  abstract val dotnet: Property<String>

  @get:Input
  abstract val dotnetversion: Property<String>

  @get:Internal
  val csrid = msbuildPlatform.map { "${it}-${msbuildArch.get()}" }

  @get:Internal
  val cwd = msbuildArch.map { project.layout.buildDirectory.dir("${it}/${target.get()}").get().asFile }

  @get:Internal
  val buildDir: Provider<Directory> = msbuildArch.map { it -> projectLayout.buildDirectory.dir("${it}/${target.get()}/bin/${configuration.get()}").get() }

  @get:Internal
  val buildProductsDir: Provider<Directory> = buildDir.map { it.dir("net${dotnetversion.get()}/${csrid.get()}") }

  public fun dotnetExec(command: String, execBlock: Action<ExecSpec>) {
    ex.exec {
      workingDir = cwd.get()
      executable = dotnet.get()
      args(command)
      execBlock.execute(this)
    }
  }

  init {
    configuration.convention(CSReleaseConfiguration.Debug)
  }
}

abstract class DotNetConfiguration @Inject constructor(private val ex: ExecOperations, private val fs: FileSystemOperations, private val obj: ObjectFactory): MSBuildConventions() {
  @get:InputFile
  abstract val csProjXsl: RegularFileProperty

  @get:Internal
  val generatedBuildRoot = this.msbuildArch.map { msbuildArch -> project.layout.buildDirectory.dir(msbuildArch).get() }

  @get:OutputFile
  val appSlnFile = generatedBuildRoot.map { it.file("SaxonCS/SaxonCS.sln") }

  @get:OutputFile
  val testSlnFile = generatedBuildRoot.map { it.file("TestDrivers/TestDrivers.sln") }

  @get:OutputFile
  val nunitSlnFile = generatedBuildRoot.map { it.file("NUnitTests/NUnitTests.sln") }

  @get:InputFiles
  abstract val resources: ConfigurableFileCollection

  @get:OutputFiles
  val copiedResources: FileCollection get() = obj.fileCollection().from(resources.elements.map { files ->
    val br = generatedBuildRoot.get()
    files.map { f -> br.file(f.asFile.name) }
  })

  @get:Input
  abstract val edition: Property<String>

  @get:Input
  abstract val version: Property<String>

  @get:Input
  abstract val release: Property<CSReleaseConfiguration>

  @get:Input
  abstract val dotnetversion: Property<String>

  @get:InputFiles
  abstract val classpath: ConfigurableFileCollection

  private
  fun genArgs(outputFile: File, releaseBuild: String): Array<String> {
    return arrayOf(
      "-it",
      "-xsl:${csProjXsl.get()}",
      "-o:${outputFile}",
      "release=${release.get()}",
      "releaseBuild=${releaseBuild}",
      "platform=${msbuildPlatform.get()}",
      "arch=${msbuildArch.get()}",
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

    fs.copy {
      into(generatedBuildRoot)
      from(resources)
    }
  }
}

abstract class DotNetBuild : DotNetImplementation() {
  @get:InputFile
  abstract val slnFile: RegularFileProperty
  private val solution = slnFile.map { it.asFile.name }

  @get:InputFiles
  abstract val inputSources: ConfigurableFileCollection

  @get:OutputFile
  val apiDocumentationFile: Provider<RegularFile> = buildProductsDir.map { it.file("${target.get()}.xml") }

  @get:OutputDirectory
  val outputDir = buildProductsDir

  @TaskAction
  fun build() {
    dotnetExec("restore") {
      args(solution.get(), "--verbosity", "Normal",
        "-p:Configuration=${configuration.get()}", "-p:Platform=Any CPU")
    }

    dotnetExec("build") {
      args(solution.get(), "--verbosity", "Normal",
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
  @get:InputFile
  abstract val slnFile: RegularFileProperty
  private val solution = slnFile.map { it.asFile.name }

  @get:InputFiles
  abstract val inputSources: ConfigurableFileCollection

  @get:OutputDirectory
  val outputDir: Provider<Directory> = dotnetversion.map { v -> projectLayout.buildDirectory.dir("net${v}/${configuration.get()}/${csrid.get()}").get() }


  @TaskAction
  fun build() {

    dotnetExec("publish") {
      args(solution.get(), "-r", csrid.get(),
        "--self-contained", "true",
        "--configuration", configuration.get(),
        "--output", outputDir.get().asFile,
        "-p:PublishSingleFile=true",
        "-p:Version=${version.get()}", "-p:PackageVersion=${version.get()}",
        "-p:Platform=Any CPU")
    }
  }

  init {
    target.convention("SaxonCS") // by default
  }
}

abstract class DotNetCreateNupkg : DotNetImplementation() {
  @get:InputFile
  abstract val slnFile: RegularFileProperty
  private val solution = slnFile.map { it.asFile.name }

  @get:InputFiles
  abstract val inputSources: ConfigurableFileCollection

  @get:Internal
  val nupkgOutputDir: Provider<Directory> = dotnetversion.map { v -> projectLayout.buildDirectory.dir("net${v}/${configuration.get()}/nupkg").get() }

  @get:OutputFile
  val nupkgFile: Provider<RegularFile> = nupkgOutputDir.map { it.file("${target.get()}.${version.get()}.nupkg") }

  @TaskAction
  fun build() {
    dotnetExec("pack") {
      args(solution.get(), "--runtime", csrid.get(),
        "--configuration", configuration.get(),
        "--output", nupkgOutputDir.get().asFile,
        "-p:Version=${version.get()}", "-p:PackageVersion=${version.get()}",
        "-p:Platform=Any CPU")
    }
  }

  init {
    target.convention("SaxonCS") // by default
  }
}

abstract class DotNetMakeApp : DotNetImplementation() {
  @get:OutputDirectory
  val outputDir = buildProductsDir.map { it.dir("publish/SaxonCS.app") }

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

abstract class DotNetHtmlDoc @Inject constructor(private val ex: ExecOperations, private val fs: FileSystemOperations): DefaultTask() {
  @get:InputFile
  abstract val merged: RegularFileProperty

  @get:InputFiles
  abstract val cssFiles: ConfigurableFileCollection

  @get:InputFile
  abstract val xslt: RegularFileProperty

  @get:OutputDirectory
  abstract val outputDir: DirectoryProperty

  @get:InputFiles
  abstract val classpath: ConfigurableFileCollection

  @TaskAction
  fun generate() {
    fs.delete {
      delete(outputDir.get().asFileTree.files)
    }

    val clspth = classpath
    ex.javaexec {
      this.classpath = clspth
      mainClass = "net.sf.saxon.Transform"
      args("-s:${merged.get().asFile}",
        "-xsl:${xslt.get().asFile}",
        "-o:${outputDir.get().file("index.html").asFile}")
    }

    fs.copy {
      into(outputDir)
      from(cssFiles)
    }
  }
}
