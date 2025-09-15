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
import org.gradle.internal.os.OperatingSystem;
import org.gradle.kotlin.dsl.create

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

    extension.release.convention("Release")
    extension.arch.convention(if (System.getProperty("os.arch") == "aarch64") "arm64" else "x64")
    extension.platform.convention(if (OperatingSystem.current().isMacOsX())
                                    "osx" else if (OperatingSystem.current().isWindows())
                                      "windows" else "linux")
  }
}

abstract class DotNetImplementation: DefaultTask() {
  @get:Input
  abstract val platform: Property<String>

  @get:Input
  abstract val arch: Property<String>

  @get:Input
  abstract val configuration: Property<String>

  init {
    configuration.convention("Release")
    platform.convention(if (OperatingSystem.current().isMacOsX())
                        "osx" else if (OperatingSystem.current().isWindows())
                          "windows" else "linux")
    arch.convention(if (System.getProperty("os.arch") == "aarch64") "arm64" else "x64")
  }
}

abstract class DotNetConfiguration: DotNetImplementation() {
  @TaskAction
  fun makeCSProj() {
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
    val csrid = "${platform}-${arch}"

    project.javaexec {
      classpath = saxonCS.classpath.get()
      mainClass.set("net.sf.saxon.Transform") // .set() WTF?
      args("-it",
           "-xsl:${project.layout.projectDirectory.file("../tools/generateCSProj.xsl")}",
           "-o:${project.layout.buildDirectory.file("${arch}/SaxonCS/core.csproj").get()}",
           "saxonVersion=${saxonBuild.version.get()}",
           "edition=${saxonCS.edition.get()}",
           "optimize=true",
           "csrid=${csrid}")

    }

    project.javaexec {
      classpath = saxonCS.classpath.get()
      mainClass.set("net.sf.saxon.Transform") // .set() WTF?
      args("-it",
           "-xsl:${project.layout.projectDirectory.file("../tools/generateTestDriverCSProj.xsl")}",
           "-o:${project.layout.buildDirectory.file("${arch}/TestDrivers/core.csproj").get()}",
           "saxonVersion=${saxonBuild.version.get()}",
           "edition=${saxonCS.edition.get()}",
           "optimize=true",
           "csrid=${csrid}")
    }

    var uuid1 = UUID.randomUUID()
    var uuid2 = UUID.randomUUID()
    var slnfile = project.layout.buildDirectory.file("${arch}/SaxonCS/SaxonCS.sln").get()

    File("${slnfile}").writeText("""
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
    slnfile = project.layout.buildDirectory.file("${arch}/TestDrivers/TestDrivers.sln").get()

    File("${slnfile}").writeText("""
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

abstract class DotNetBuild: DotNetImplementation() {
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

    println("TARGET: ${target.get()}")

    val cwd = project.layout.buildDirectory.dir("${arch.get()}/${target.get()}").get().asFile

    project.exec {
      workingDir = cwd
      commandLine("${saxonBuild.dotnet.get()}",
                  "restore", "${target.get()}.sln", "--verbosity", "Normal",
                  "-p:Configuration=${configuration}", "-p:Platform=Any CPU")
    }

    project.exec {
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

abstract class DotNetPublish: DotNetImplementation() {
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

    if (platform == "osx") {
      project.exec {
        workingDir = project.layout.buildDirectory.file("${arch}/SaxonCS").get().asFile
        commandLine("${saxonBuild.dotnet.get()}",
                    "restore", "SaxonCS.sln",
                    "-p:Configuration=${configuration}", "-p:Platform=Any CPU")
      }

      project.exec {
        workingDir = project.layout.buildDirectory.file("${arch}/SaxonCS").get().asFile
        commandLine("${saxonBuild.dotnet.get()}",
                    "msbuild", "-t:BundleApp",
                    "-p:Version=${csver}", "-p:PackageVersion=${csver}",
                    "-p:Configuration=${configuration}",
                    "SaxonCS.csproj")
      }
    } else {
      project.exec {
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
}
