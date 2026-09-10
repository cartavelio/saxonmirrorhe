package com.saxonica.build.java.testsuite

import java.io.File
import java.io.ByteArrayOutputStream
import java.io.FileOutputStream
import java.io.PrintStream
import org.gradle.api.DefaultTask
import org.gradle.api.GradleException
import org.gradle.api.file.*
import org.gradle.api.provider.Property
import org.gradle.api.provider.ListProperty
import org.gradle.api.provider.MapProperty
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.*
import org.gradle.api.tasks.options.Option
import org.gradle.kotlin.dsl.assign
import org.gradle.process.ExecOperations
import java.text.SimpleDateFormat
import java.util.*
import javax.inject.Inject

abstract class JavaRunner @Inject constructor(private val ex: ExecOperations): DefaultTask() {
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
  @get:InputFiles
  abstract val classpath: ConfigurableFileCollection
  @get:Internal
  abstract val testargs: ListProperty<String>

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
      "xsltfromqt4" -> "${driverPackage}.Xslt30TestSuiteDriver${EDITION}"
      "qt4" -> "${driverPackage}.QT3TestDriver${EDITION}"
      "xslt40" -> "${driverPackage}.Xslt30TestSuiteDriver${EDITION}"
      else -> throw GradleException("Unknown test driver class for test suite: ${suite}")
    }

  @get:Internal
  val catalog: String
    get() {
      // This is ugly. I'm threading a need to allow the tree model tests
      // to run with different catalogs. But because I don't want to check
      // those catalogs into the submodule repositories directly, I have 
      // to do ... gestures wildly ... this
      var tree = "none"
      for (arg in testargs.get()) {
        if (arg.startsWith("-tree:")) {
          tree = arg.substring(6).lowercase()
        }
      }

      if (tree != "none") {
        return "../../java/src/test/catalogs/${suite.get()}-tree-catalog.xml"
      }

      return when (suite.get()) {
        "qt3extra" -> "j-catalog.xml"
        else -> "catalog.xml"
      }
    } 

  @OutputDirectory
  val reportDir = reportingRootDir.map { it.dir("${suite.get()}/${dateStamp}") }

  @get:Input
  @get:Option(option = "s", description = "request specific test set")
  abstract val testSet: Property<String>

  @get:Input
  @get:Option(option = "t", description = "set test matching pattern")
  abstract val testPattern: Property<String>

  init {
    testSet.convention("")
    testPattern.convention("")
  }

  @TaskAction
  fun execute() {
    val testclasspath = classpath
    val stdOutput = ByteArrayOutputStream()
    val errOutput = ByteArrayOutputStream()

    val javaArgs = mutableListOf<String>()

    // This makes the report name match what the regression report builder
    // expects. Since I don't think any one else cares...
    val reportName = if (name.startsWith("test_suite_")) {
      "${edition.get()}j_${name.substring(11)}"
    } else {
      name
    }

    javaArgs.addAll(listOf(
                        "${suiteDir.get()}",
                        catalog,
                        "-exceptions:${exceptionsFile.get()}",
                        "-reportfilename:${reportName}",
                        "-o:${reportDir.get()}",
                        "-lang:${language.get()}"
                    ))

    javaArgs.addAll(testargs.get())

    if (testSet.get() != "") {
      javaArgs.add("-s:${testSet.get()}")
    }

    if (testPattern.get() != "") {
      javaArgs.add("-t:${testPattern.get()}")
    }

    println("MAIN: ${driverClass}")
    println("JARG: ${javaArgs}")

    ex.javaexec {
      mainClass = driverClass
      workingDir = suiteDir.asFile.get()
      classpath = testclasspath

      if (!showOutput.get()) {
        println("(buffering output)")
        standardOutput = stdOutput
        errorOutput = errOutput
      }

      args(javaArgs)
    }

    if (!showOutput.get()) {
      println("==> ${reportDir.get()}/stderr.txt")
      var outputStream = PrintStream(File("${reportDir.get()}/stderr.txt"))
      var lines = errOutput.toString("utf-8")
      outputStream.print(lines)
      outputStream.close()

      var failures = false
      val failureList = mutableListOf<String>()
      for (line in lines.split("\\n+".toRegex())) {
        if (line.startsWith("Failures by Test Set:")) {
          failures = true
        }
        if (line.startsWith("Finished in")) {
          failures = false
        }
        if (failures) {
          failureList.add(line)
        }
        if (line.startsWith("Result:")) {
          if (failureList.size > 1) {
            println("⋮")
            for (fline in failureList) {
              println(fline)
            }
          }
          println(line)
          break;
        }
      }

      if (stdOutput.size() > 0) {
        println("==> ${reportDir.get()}/stdout.txt")
        outputStream = PrintStream(File("${reportDir.get()}/stdout.txt"))
        lines = stdOutput.toString("utf-8")
        outputStream.print(lines)
        outputStream.close()
      }
    }
  }
}
