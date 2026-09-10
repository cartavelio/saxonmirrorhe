package com.saxonica.build

import SaxonBuildExtension
import java.io.File
import java.util.UUID
import org.gradle.api.DefaultTask
import org.gradle.api.GradleException
import org.gradle.api.Plugin
import org.gradle.api.Project
import org.gradle.api.artifacts.Configuration
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.Input
import org.gradle.api.tasks.TaskAction
import org.gradle.api.tasks.Internal
import org.gradle.internal.os.OperatingSystem;
import org.gradle.kotlin.dsl.create
import org.gradle.process.ExecOperations
import javax.inject.Inject

interface SaxonCSExtension {
  abstract val project: Property<Project>
  abstract val arch: Property<String>     // Normalized to x64 or arm64 for .NET
  abstract val platform: Property<String>
  abstract val edition: Property<String>
  abstract val release: Property<String>
  abstract val classpath: Property<Configuration>
}

class SaxonCSPlugin : Plugin<Project> {
  override fun apply(project: Project) {
    val extension = project.extensions.create<SaxonCSExtension>("saxoncs")
    extension.project.set(project)

    /* FIXME: do we need to support this?
    val release = project.findProperty("release") ?: "debug"
    if (release != "production" && release != "prod" && release != "debug") {
      throw GradleException("Unexpected release: ${release}")
    }
    extension.release.convention("${if (release == "prod") "production" else release}")
    */

    extension.release.convention("${project.findProperty("csharpBuild") ?: "Debug"}")
    extension.arch.convention("${project.findProperty("csharpArch") ?:
                                     (if (System.getProperty("os.arch") == "aarch64") "arm64" else "x64")}")
    extension.platform.convention(if (OperatingSystem.current().isMacOsX())
                                    "osx" else if (OperatingSystem.current().isWindows())
                                      "win" else "linux")
  }
}

abstract class DotNetImplementation: DefaultTask() {
  @get:Input
  abstract val platform: Property<String>

  @get:Input
  abstract val arch: Property<String>

  @get:Input
  abstract val configuration: Property<String>

  @get:Internal
  val saxonBuildExt = project.extensions.findByType(SaxonBuildExtension::class.java)

  @get:Internal
  val saxonCSExt = project.extensions.findByType(SaxonCSExtension::class.java)

  init {
    configuration.convention("${project.findProperty("csharpBuild") ?: "Debug"}")
    platform.convention(if (OperatingSystem.current().isMacOsX())
                        "osx" else if (OperatingSystem.current().isWindows())
                          "win" else "linux")
    arch.convention(if (System.getProperty("os.arch") == "aarch64") "arm64" else "x64")
  }
}

abstract class DotNetConfiguration @Inject constructor(private val ex: ExecOperations): DotNetImplementation() {
  @get:Internal
  val generateAppCsProj = project.layout.projectDirectory.file("../tools/generateCSProj.xsl")

  @get:Internal
  val appCsProj = this.arch.map { project.layout.buildDirectory.file("${it}/SaxonCS/core.csproj").get() }

  @get:Internal
  val generateTestCsProj = project.layout.projectDirectory.file("../tools/generateTestDriverCSProj.xsl")

  @get:Internal
  val generateNUnitCsProj = project.layout.projectDirectory.file("../tools/generateNUnitCSProj.xsl")

  @get:Internal
  val testCsProj = this.arch.map { project.layout.buildDirectory.file("${it}/TestDrivers/core.csproj").get() }

  @get:Internal
  val nunitCsProj = this.arch.map { project.layout.buildDirectory.file("${it}/NUnitTests/NUnitTests.csproj").get() }

  @get:Internal
  val appSlnFile = this.arch.map { project.layout.buildDirectory.file("${it}/SaxonCS/SaxonCS.sln").get() }

  @get:Internal
  val testSlnFile = this.arch.map { project.layout.buildDirectory.file("${it}/TestDrivers/TestDrivers.sln").get()  }

  @TaskAction
  fun makeCSProj() {
    val saxonBuild = if (saxonBuildExt is SaxonBuildExtension) {
      saxonBuildExt as SaxonBuildExtension
    } else {
      throw GradleException("Could not find 'saxonbuild' extension")
    }

    val saxonCS = if (saxonCSExt is SaxonCSExtension) {
      saxonCSExt as SaxonCSExtension
    } else {
      throw GradleException("Could not find 'saxoncs' extension")
    }

    val arch = this.arch.get()

    val platform = this.platform.get()
    val csrid = "${platform}-${arch}"

    ex.javaexec {
      classpath = saxonCS.classpath.get()
      mainClass.set("net.sf.saxon.Transform") // .set() WTF?
      args("-it",
           "-xsl:${generateAppCsProj}",
           "-o:${appCsProj.get()}",
           "saxonVersion=${saxonBuild.version.get()}",
           "edition=${saxonCS.edition.get()}",
           "optimize=true",
           "csrid=${csrid}")

    }

    ex.javaexec {
      classpath = saxonCS.classpath.get()
      mainClass.set("net.sf.saxon.Transform") // .set() WTF?
      args("-it",
           "-xsl:${generateTestCsProj}",
           "-o:${testCsProj.get()}",
           "saxonVersion=${saxonBuild.version.get()}",
           "configuration=${configuration.get()}",
           "edition=${saxonCS.edition.get()}",
           "optimize=true",
           "csrid=${csrid}")
    }

    ex.javaexec {
      classpath = saxonCS.classpath.get()
      mainClass.set("net.sf.saxon.Transform") // .set() WTF?
      args("-it",
           "-xsl:${generateNUnitCsProj}",
           "-o:${nunitCsProj.get()}",
           "saxonVersion=${saxonBuild.version.get()}",
           "edition=${saxonCS.edition.get()}",
           "configuration=${configuration.get()}",
           "optimize=true",
           "csrid=${csrid}")
    }

    var uuid1 = UUID.randomUUID()
    var uuid2 = UUID.randomUUID()

    File("${appSlnFile.get()}").writeText("""
Microsoft Visual Studio Solution File, Format Version 12.00
Project("{${uuid1}}") = "SaxonCS", "SaxonCS.csproj", "{${uuid2}}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{${uuid2}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{${uuid2}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{${uuid2}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{${uuid2}}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
EndGlobal
""")

    uuid1 = UUID.randomUUID()
    uuid2 = UUID.randomUUID()

    File("${testSlnFile.get()}").writeText("""
Microsoft Visual Studio Solution File, Format Version 12.00
Project("{${uuid1}}") = "TestDrivers", "TestDrivers.csproj", "{${uuid2}}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{${uuid2}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{${uuid2}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{${uuid2}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{${uuid2}}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
EndGlobal
""")

  }
}

abstract class DotNetBuild @Inject constructor(private val ex: ExecOperations): DotNetImplementation() {
  @get:Input
  abstract val target: Property<String>
  
  @TaskAction
  fun build() {
    val saxonBuild = project.extensions.findByType(SaxonBuildExtension::class.java)
    if (saxonBuild !is SaxonBuildExtension) {
      throw GradleException("Could not find 'saxonbuild' extension")
    }

    val saxonCS = project.extensions.findByType(SaxonCSExtension::class.java)
    if (saxonCS !is SaxonCSExtension) {
      throw GradleException("Could not find 'saxoncs' extension")
    }

    val configuration = this.configuration.get()

    val cwd = project.layout.buildDirectory.dir("${arch.get()}/${target.get()}").get().asFile
    println("-p:DefineConstants=${saxonCS.edition.get().uppercase()}")
    ex.exec {
      workingDir = cwd
      commandLine("${saxonBuild.dotnet.get()}",
                  "restore", "${target.get()}.sln", "--verbosity", "Normal",
                  "-p:Configuration=${configuration}", "-p:Platform=Any CPU")
    }

    ex.exec {
      workingDir = cwd
      commandLine("${saxonBuild.dotnet.get()}",
                  "build", "${target.get()}.sln", "--verbosity", "Normal",
                  "/nodereuse:false", "--no-restore",
                  "-p:Version=${saxonBuild.version.get()}",
                  "-p:PackageVersion=${saxonBuild.version.get()}",
                  "-p:Configuration=${configuration}", "-p:Platform=Any CPU",
                  "-p:DefineConstants=${saxonCS.edition.get().uppercase()}"
      )
    }
  }

  init {
    target.convention("SaxonCS") // by default
  }
}

abstract class DotNetPublish @Inject constructor(private val ex: ExecOperations): DotNetImplementation() {
  @TaskAction
  fun build() {
    val saxonBuild = project.extensions.findByType(SaxonBuildExtension::class.java)
    if (saxonBuild !is SaxonBuildExtension) {
      throw GradleException("Could not find 'saxonbuild' extension")
    }

    val saxonCS = project.extensions.findByType(SaxonCSExtension::class.java)
    if (saxonCS !is SaxonCSExtension) {
      throw GradleException("Could not find 'saxoncs' extension")
    }

    val arch = this.arch.get()

    val platform = this.platform.get()
    val configuration = this.configuration.get()
    val csrid = "${platform}-${arch}"
    val csver = saxonBuild.version.get()

    ex.exec {
      workingDir = project.layout.buildDirectory.file("${arch}/SaxonCS").get().asFile
      commandLine("${saxonBuild.dotnet.get()}",
                  "publish", "SaxonCS.sln", "-r", csrid,
                  "--self-contained", "true",
                  "-p:PublishSingleFile=true",
                  "-p:Version=${csver}", "-p:PackageVersion=${csver}",
                  "-p:Configuration=${configuration}", "-p:Platform=Any CPU")
    }
  }
}

abstract class DotNetMakeApp @Inject constructor(private val ex: ExecOperations): DotNetImplementation() {
  @get:Internal
  val execwd = this.arch.map { project.layout.buildDirectory.file("${it}/SaxonCS").get().asFile }

  @TaskAction
  fun build() {
    val saxonBuild = if (saxonBuildExt is SaxonBuildExtension) {
      saxonBuildExt as SaxonBuildExtension
    } else {
      throw GradleException("Could not find 'saxonbuild' extension")
    }

    val configuration = this.configuration.get()

    ex.exec {
      workingDir = execwd.get()
      commandLine("${saxonBuild.dotnet.get()}",
                  "msbuild", "-t:BundleApp",
                  "-p:Configuration=${configuration}",
                  "SaxonCS.csproj")
    }
  }
}
