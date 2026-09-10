package com.saxonica.build

import java.nio.file.Files
import java.nio.file.Paths
import java.text.SimpleDateFormat
import java.util.Date
import org.gradle.api.GradleException
import org.gradle.api.Plugin
import org.gradle.api.Project
import org.gradle.api.provider.MapProperty
import org.gradle.api.provider.Property
import org.gradle.api.tasks.Copy
import org.gradle.api.tasks.Exec
import org.gradle.api.tasks.JavaExec
import org.gradle.api.tasks.TaskContainer
import org.gradle.kotlin.dsl.create
import org.gradle.kotlin.dsl.register
import SaxonBuildExtension
import javax.inject.Inject

import com.saxonica.build.java.testsuite.JavaRunner
import org.gradle.api.file.*
import org.gradle.api.provider.Provider
import org.gradle.kotlin.dsl.assign

abstract class TestSuitesExtension
  @Inject constructor(private val layout: ProjectLayout,
                      private val tasks: TaskContainer) {

  abstract val testclasspath: Property<FileCollection>
  abstract val propertyOptions: MapProperty<String,String>
  abstract val product: Property<String>
  abstract val platform: Property<String>
  abstract val arch: Property<String>
  abstract val edition: Property<String>
  abstract val dotnet: Property<String>
  abstract val netver: Property<String>
  abstract val submodulesDir: DirectoryProperty
  abstract val exceptionsDir: DirectoryProperty

  fun configure(suite: String, language: String,
                options: Map<String,String> = emptyMap(),
                taskName: String? = null) {

    val name = if (taskName == null) {
      val keys = options.keys.sorted()
      if (keys.isEmpty()) {
        "test_suite_${suite}_${language}"
      } else {
        "test_suite_${suite}_${language}_${keys.joinToString("_")}"
      }
    } else {
      "test_suite_${taskName}"
    }

    when (product.get()) {
      "java" -> if (suite.startsWith("xsd")) {
                  configureJavaXsd(suite, language, options, name)
                } else {
                  configureJava(suite, language, options, name)
                }
      "csharp" -> if (suite.startsWith("xsd")) {
                    configureCsharpXsd(suite, language, options, name)
                } else {
                  configureCsharp(suite, language, options, name)
                }
      else -> throw GradleException("Unexpected test product: ${product.get()}")
    }
  }

  fun suiteDir(suite: String): Provider<Directory> {
    return when (suite) {
      "qt3" -> submodulesDir.dir("qt3")
      "qt3extra" -> submodulesDir.dir("extra/suites/qt3")
      "expath" -> layout.buildDirectory.dir("suite/expath")
      "xslt30" -> submodulesDir.dir("xslt30")
      "xslt30extra" -> submodulesDir.dir("extra/suites/xslt30")
      "qt4" -> submodulesDir.dir("qt4")
      "xslt40" -> submodulesDir.dir("xslt40")
      "xsd" -> submodulesDir.dir("xsd")
      "xsdextra" -> submodulesDir.dir("extra/suites/xsd")
      else -> throw GradleException("Unknown directory for test suite: ${suite}")
    }
  }

  fun exceptions(suite: String): Provider<RegularFile> {
    return when (suite) {
      "qt3" -> exceptionsDir.file("qt3.xml")
      "qt3extra" -> exceptionsDir.file("qt3extra.xml")
      "expath" -> exceptionsDir.file("expath.xml")
      "xslt30" -> exceptionsDir.file("xslt30.xml")
      "xslt30extra" -> exceptionsDir.file("xslt30extra.xml")
      "qt4" -> exceptionsDir.file("qt4.xml")
      "xslt40" -> exceptionsDir.file("xslt40.xml")
      "xsd" -> exceptionsDir.file("xsd11.xml")
      "xsdextra" -> exceptionsDir.file("xsd-extra.xml")
      else -> throw GradleException("Unknown exceptions for test suite: ${suite}")
    }
  }

  
  fun configureJava(suite: String, language: String,
                    @Suppress("UNUSED_PARAMETER") options: Map<String,String>,
                    taskName: String) {
    val plugin = this
    val thisSuite = tasks.register<JavaRunner>(taskName) {
      group = LifecycleTasks.ACCEPTANCE_TEST_GROUP
      this.edition = plugin.edition
      this.suite = suite
      this.language = language
      this.suiteDir = suiteDir(suite)
      exceptionsFile = exceptions(suite)
      reportingRootDir = layout.buildDirectory.dir("test-results")
      classpath.from(testclasspath)
    }

    if (suite == "expath") {
      // This is annoying. We have to copy the suite because the sandpit is writable
      val copySuite = tasks.register<Copy>("${taskName}_copy_suite") {
        group = "Internal"
        description = "Copy the expath test suite"
        into(layout.buildDirectory.dir("suite/expath"))
        from(submodulesDir.dir("expath/tests/qt3"))
      }
      val copySandpit = tasks.register<Copy>("${taskName}_copy_sandpit") {
        group = "Internal"
        description = "Copy the expath test suite sandpit" 
        dependsOn(copySuite)
        into(layout.buildDirectory.dir("suite/expath/archive/sandpit"))
        from(submodulesDir.dir("extra/data/sandpit"))
      }
      thisSuite.configure {
        dependsOn(copySandpit)
      }
    }

    tasks.named(LifecycleTasks.ACCEPTANCE_TEST_TASK_NAME) {
      dependsOn(thisSuite)
    }
  }

  fun configureJavaXsd(suite: String, language: String, options: Map<String,String>, taskName: String) {
    val dateStamp = SimpleDateFormat("yyyyMMdd").format(Date()).toString()

    val xsdVersion = "1.1"

    val driverClass = "com.saxonica.testdriver.ee.SchemaTestSuiteDriver"

    val directory = suiteDir(suite)
    val exceptionsFile = exceptions(suite)

    val catalog = "catalog.xml"

    val reportFn = "SaxonResults-${language}.xml"
    val errorFn = "SaxonFailures-${language}-errors.xml"

    val reportDir = layout.buildDirectory.file("test-results/${suite}/${dateStamp}").get()

    val summarizeResults = submodulesDir.file("extra/tools/summarize-results.xsl").get()

    val testargs = mutableListOf<String>("${directory.get()}",
                                         "${catalog}",
                                         "-exceptions:${exceptionsFile.get()}",
                                         "-reportfilename:${reportFn}",
                                         "-errorfilename:${errorFn}",
                                         "-local:${reportDir}",
                                         "-summarizeresults:${summarizeResults}",
                                         "-v:${xsdVersion}")
    for ((optname, optvalue) in options) {
      testargs.add("-${optname}:${optvalue}")
    }

    val thisSuite = tasks.register<JavaExec>(taskName) {
      group = LifecycleTasks.ACCEPTANCE_TEST_GROUP
      mainClass.set(driverClass)
      workingDir = directory.get().asFile
      classpath = testclasspath.get()
      args(testargs)

      doFirst {
        Files.createDirectories(Paths.get(reportDir.toString()))
      }
    }

    tasks.named(LifecycleTasks.ACCEPTANCE_TEST_TASK_NAME) {
      dependsOn(thisSuite)
    }
  }

  fun configureCsharp(suite: String, language: String, options: Map<String,String>, taskName: String) {
    // If this fails, make a task named all_test_suites in your build before calling configureSuite()
    val allSuites = tasks.named(LifecycleTasks.ACCEPTANCE_TEST_TASK_NAME)
    val build = tasks.named("dotnetBuildTestDrivers")

    val dateStamp = SimpleDateFormat("yyyyMMdd").format(Date()).toString()

    val directory = suiteDir(suite)
    val exceptionsFile = exceptions(suite)

    val catalog = when (suite) {
      "qt3extra" -> "j-catalog.xml"
      else -> "catalog.xml"
    }

    val reportDir = layout.buildDirectory.file("test-results/${suite}/${dateStamp}").get()

    val relprefix = "${arch.get()}/TestDrivers/bin/Release"
    val relarch = "${netver.get()}/${platform.get()}-${arch.get()}/TestDrivers.dll"

    val testDrivers = layout.buildDirectory.file("${relprefix}/${relarch}")

    val testargs = mutableListOf<String>("${dotnet.get()}",
                                         "${testDrivers.get().asFile}",
                                         "${directory.get()}",
                                         "${catalog}",
                                         "-exceptions:${exceptionsFile.get()}",
                                         "-reportfilename:${taskName}",
                                         "-o:${reportDir}",
                                         "-lang:${language}")
    for ((optname, optvalue) in options) {
      testargs.add("-${optname}:${optvalue}")
    }
    for((optname, optvalue) in propertyOptions.get()) {
      if (optvalue == "") {
        testargs.add("-${optname}")
      } else {
        testargs.add("-${optname}:${optvalue}")
      }
    }

    val thisSuite = tasks.register<Exec>(taskName) {
      group = LifecycleTasks.ACCEPTANCE_TEST_GROUP

      dependsOn(build)
      workingDir = directory.get().asFile
      commandLine = testargs
      //ignoreExitValue = true

      doFirst {
        println(testargs)
        println("${product.get()}, ${platform.get()}, ${arch.get()}, ${edition.get()}")
        Files.createDirectories(Paths.get(reportDir.toString()))
      }
    }
    if (suite == "expath") {
      // This is annoying. We have to copy the suite because the sandpit is writable
      val copySuite = tasks.register<Copy>("${taskName}_copy_suite") {
        group = "Internal"
        description = "Copy the expath test suite"
        into(layout.buildDirectory.dir("suite/expath"))
        from(submodulesDir.dir("expath/tests/qt3"))
      }
      val copySandpit = tasks.register<Copy>("${taskName}_copy_sandpit") {
        group = "Internal"
        description = "Copy the expath test suite sandpit"
        dependsOn(copySuite)
        into(layout.buildDirectory.dir("suite/expath/archive/sandpit"))
        from(submodulesDir.dir("extra/data/sandpit"))
      }
      thisSuite.configure {
        dependsOn(copySandpit)
      }
    }

    allSuites.configure {
      dependsOn(thisSuite)
    }
  }

  fun configureCsharpXsd(suite: String, 
                         @Suppress("UNUSED_PARAMETER") language: String,
                         options: Map<String,String>, taskName: String) {
    // If this fails, make a task named all_test_suites in your build before calling configureSuite()
    val allSuites = tasks.named(LifecycleTasks.ACCEPTANCE_TEST_TASK_NAME).get()
    val build = tasks.named("dotnetBuildTestDrivers").get()

    val dateStamp = SimpleDateFormat("yyyyMMdd").format(Date()).toString()

    val xsdVersion = "11"

    val directory = suiteDir(suite)
    val exceptionsFile = exceptions(suite)

    val reportDir = layout.buildDirectory.file("test-results/${suite}/${dateStamp}").get()

    val relprefix = "${arch.get()}/TestDrivers/bin/Release"
    val relarch = "${netver.get()}/${platform.get()}-${arch.get()}/TestDrivers.dll"

    val testDrivers = layout.buildDirectory.file("${relprefix}/${relarch}")

    val testargs = mutableListOf<String>("${dotnet.get()}",
                                         "${testDrivers.get().asFile}",
                                         "${directory.get()}",
                                         "-o:${reportDir}",
                                         "-lang:XSD${xsdVersion}",
                                         "-debug",
                                         "-exceptions:${exceptionsFile.get()}")
    for ((optname, optvalue) in options) {
      testargs.add("-${optname}:${optvalue}")
    }
    for((optname, optvalue) in propertyOptions.get()) {
      if (optvalue == "") {
        testargs.add("-${optname}")
      } else {
        testargs.add("-${optname}:${optvalue}")
      }
    }

    val thisSuite = tasks.register<Exec>(taskName) {
      dependsOn(build)

      workingDir = directory.get().asFile
      commandLine = testargs
      //ignoreExitValue = true

      doFirst {
        println(testargs)
        println("${product.get()}, ${platform.get()}, ${arch.get()}, ${edition.get()}")
        Files.createDirectories(Paths.get(reportDir.toString()))
      }
    }

    allSuites.dependsOn(thisSuite)
  }
}

class TestSuitesPlugin : Plugin<Project> {
  override fun apply(project: Project): Unit = project.run {
    val extension = project.extensions.create<TestSuitesExtension>("testSuites")

    val saxonBuild = project.extensions.findByType(SaxonBuildExtension::class.java)
    if (saxonBuild !is SaxonBuildExtension) {
      throw GradleException("Could not find 'saxonbuild' extension")
    }

    val pmap = mutableMapOf<String,String>()
    for ((propName, propValue) in project.properties) {
      if (propName.startsWith("td_")) {
        pmap[propName.substring(3)] = "${propValue}"
      }
    }
    extension.propertyOptions.convention(pmap)
    extension.product.convention("")
    extension.platform.convention(saxonBuild.platform.get())
    extension.arch.convention(if (saxonBuild.arch.get() == "aarch64") "arm64" else "x64")
    extension.edition.convention("")
    extension.dotnet.convention(saxonBuild.dotnet.get())
    extension.netver.convention("net${saxonBuild.dotnetversion.get()}")
  }
}
