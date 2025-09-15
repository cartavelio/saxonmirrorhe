package com.saxonica.build.java.testsuite

import org.gradle.api.DefaultTask
import org.gradle.api.GradleException
import org.gradle.api.file.*
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.*
import org.gradle.api.tasks.options.Option
import org.gradle.kotlin.dsl.assign
import java.text.SimpleDateFormat
import java.util.*

abstract class JavaRunner: DefaultTask() {
  override fun getGroup(): String {
    return "Testsuite"
  }

  @get:Input
  abstract val suite: Property<String>
  @get:Input
  abstract val language: Property<String>
  @get:Input
  abstract val edition: Property<String>
  @get:InputDirectory
  abstract val suiteDir: DirectoryProperty
  @get:InputFile
  abstract val exceptionsFile: RegularFileProperty
  @get:Internal
  abstract val reportingRootDir: DirectoryProperty
  @get:InputFiles
  abstract val classpath: ConfigurableFileCollection

  @get:Internal
  val EDITION: String by lazy { edition.get().uppercase() }
  @get:Internal
  val dateStamp = SimpleDateFormat("yyyyMMdd").format(Date()).toString()

  @get:Internal
  val driverPackage: String
    get() = if (edition.get() == "he") "net.sf.saxon.testdriver" else "com.saxonica.testdriver"

  @get:Internal
  val driverClass: String
    get() = when (suite.get()) {
      "qt3" -> "${driverPackage}.QT3TestDriver${EDITION}"
      "qt3extra" -> "${driverPackage}.QT3TestDriver${EDITION}"
      "expath" -> "${driverPackage}.QT3TestDriver${EDITION}"
      "xslt30" -> "${driverPackage}.Xslt30TestSuiteDriver${EDITION}"
      "xslt30extra" -> "${driverPackage}.Xslt30TestSuiteDriver${EDITION}"
      "qt4" -> "${driverPackage}.QT3TestDriver${EDITION}"
      "xslt40" -> "${driverPackage}.Xslt30TestSuiteDriver${EDITION}"
      else -> throw GradleException("Unknown test driver class for test suite: ${suite}")
    }

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
  fun setTestSet(arg: String): JavaRunner {
    testOpts["s"] = arg
    return this;
  }

  @Option(option = "t", description = "set test matching pattern")
  fun setTestPattern(arg: String): JavaRunner {
    testOpts["t"] = arg
    return this;
  }

  @TaskAction
  fun execute() {
    val testclasspath = classpath
    project.javaexec {
      mainClass = driverClass
      workingDir = project.file(suiteDir)
      classpath = testclasspath
      args(
        "${suiteDir.get()}",
        catalog,
        "-exceptions:${exceptionsFile.get()}",
        "-reportfilename:${name}",
        "-o:${reportDir.get()}",
        "-lang:${language.get()}"
      )
      testOpts.forEach { (opt, arg) ->
        args("-${opt}:${arg}")
      }
    }
  }
}