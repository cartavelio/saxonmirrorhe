package com.saxonica.build.java.testsuite

import java.io.File
import java.io.ByteArrayOutputStream
import java.io.FileOutputStream
import java.io.PrintStream
import org.gradle.api.DefaultTask
import org.gradle.api.GradleException
import org.gradle.api.file.*
import org.gradle.api.provider.ListProperty
import org.gradle.api.provider.MapProperty
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.*
import org.gradle.api.tasks.options.Option
import org.gradle.kotlin.dsl.assign
import org.gradle.process.ExecOperations
import java.text.SimpleDateFormat
import java.util.*
import javax.inject.Inject

abstract class CSharpRunner @Inject constructor(private val ex: ExecOperations): DefaultTask() {
  override fun getGroup(): String {
    return "Testsuite"
  }

  @get:Input
  abstract val suite: Property<String>
  @get:Input
  abstract val language: Property<String>
  @get:Input
  abstract val edition: Property<String>
  @get:Input
  abstract val showOutput: Property<Boolean>
  @get:InputDirectory
  abstract val suiteDir: DirectoryProperty
  @get:InputFile
  abstract val exceptionsFile: RegularFileProperty
  @get:Internal
  abstract val reportingRootDir: DirectoryProperty
  @get:Internal
  abstract val arch: Property<String>
  @get:Internal
  abstract val netver: Property<String>
  @get:Internal
  abstract val dotnet: Property<String>
  @get:Internal
  abstract val dotnetBuildType: Property<String>
  @get:Internal
  abstract val platform: Property<String>
  @get:Internal
  abstract val testargs: ListProperty<String>

  @get:Internal
  val dateStamp = SimpleDateFormat("yyyyMMdd").format(Date()).toString()

  @get:Internal
  val catalog: String
    get() = when (suite.get()) {
      "qt3extra" -> "j-catalog.xml"
      else -> "catalog.xml"
    }

  @OutputDirectory
  val reportDir = reportingRootDir.map { it.dir("${suite.get()}/${dateStamp}") }

  @get:Internal
  val testOpts = mutableMapOf<String, String>()

  @Option(option = "s", description = "request specific test set")
  fun setTestSet(arg: String): CSharpRunner {
    testOpts["s"] = arg
    return this;
  }

  @Option(option = "t", description = "set test matching pattern")
  fun setTestPattern(arg: String): CSharpRunner {
    testOpts["t"] = arg
    return this;
  }

  @TaskAction
  fun execute() {
    val stdOutput = ByteArrayOutputStream()
    val errOutput = ByteArrayOutputStream()

    val relprefix = "${arch.get()}/TestDrivers/bin/${dotnetBuildType.get()}"
    val relarch = "${netver.get()}/${platform.get()}-${arch.get()}/TestDrivers.dll"
    val testDrivers = project.layout.buildDirectory.file("${relprefix}/${relarch}")

    // This makes the report name match what the regression report builder
    // expects. Since I don't think any one else cares...
    val reportName = if (name.startsWith("test_suite_")) {
      "${edition.get()}dotnet_${name.substring(11)}.xml"
    } else {
      "${name}.xml"
    }

    val driverargs = mutableListOf<String>("${dotnet.get()}",
                                           "${testDrivers.get().asFile}",
                                           "${suiteDir.get()}",
                                           "-catalog:${catalog}",
                                           "-exceptions:${exceptionsFile.get()}",
                                           "-reportfilename:${reportName}",
                                           "-o:${reportDir.get()}",
                                           "-lang:${language.get()}")
    driverargs.addAll(testargs.get())

    //testargs.add("-debug")

    println(listOf(driverargs))

    ex.exec {
      if (!showOutput.get()) {
        println("(buffering output)")
        standardOutput = stdOutput
        errorOutput = errOutput
      }

      workingDir = suiteDir.get().asFile

      commandLine = driverargs
    }

    if (!showOutput.get()) {
      println("==> ${reportDir.get()}/stderr.txt")
      var outputStream = PrintStream(File("${reportDir.get()}/stderr.txt"))
      outputStream.println(driverargs.joinToString(" "))
      var lines = errOutput.toString("utf-8")
      outputStream.print(lines)
      outputStream.close()

      println("==> ${reportDir.get()}/stdout.txt")
      outputStream = PrintStream(File("${reportDir.get()}/stdout.txt"))
      lines = stdOutput.toString("utf-8")
      outputStream.print(lines)
      outputStream.close()

      val regex = "^\\d+ successes, .*".toRegex()
      var failures = false
      val failureList = mutableListOf<String>()
      for (line in lines.split("\\n+".toRegex())) {
        if (line.startsWith("Failing tests:")) {
          failures = true
        }
        if (failures && line.trim() != "") {
          failureList.add(line)
        }

        if (regex.matches(line)) {
          println(line)
        }
      }
      
      if (failureList.size > 1) {
        println("⋮")
        for (fline in failureList) {
          println(fline)
        }
      }
    }
  }
}
