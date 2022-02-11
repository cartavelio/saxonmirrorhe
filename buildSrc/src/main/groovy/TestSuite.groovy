import org.gradle.api.GradleException
import org.gradle.api.Task
import org.gradle.api.Project
import org.gradle.api.tasks.Exec
import org.gradle.api.tasks.JavaExec

class TestSuite {
  private static String fileSeparator = System.getProperty("file.separator")
  private static def langCodeMap = ["XP20": "XPath2.0",
                   "XP30": "XPath3.0",
                   "XP31": "XPath3.1",
                   "XT30": "XSLT3.0",
                   "XT40": "XSLT4.0",
                   "XP31": "XPath3.1",
                   "XQ10": "XQuery1.0",                                    
                   "XQ20": "XQuery2.0",
                   "XQ30": "XQuery3.0",
                   "XQ31": "XQuery3.1"]
  private def errorsReported = [:]

  Project project
  boolean configOk = true
  String suite = null
  String testPlatform = null
  String testDotnet = null
  String testRelease = null
  String testTaskName = null
  String testClass = null
  String testLang = null
  String testDir = null
  String testEx = null
  String testReportFilename = null
  String testReportDirectory = null
  String testCatalog = null
  Boolean testByteCode = null
  Boolean testUnfolded = null
  Boolean testTree = null
  Boolean testExport = null
  Boolean testJit = null
  ArrayList testOptions = []
  Boolean testDebug = null

  TestSuite(Project project) {
    this.project = project
  }

  private void reset(String suiteName) {
    configOk = true
    suite = suiteName
    testPlatform = null
    testDotnet = null
    testRelease = null
    testTaskName = null
    testClass = null
    testLang = null
    testDir = null
    testEx = null
    testReportFilename = null
    testReportDirectory = null
    testCatalog = null
    testByteCode = null
    testUnfolded = null
    testTree = null
    testExport = null
    testJit = null
    testDebug = null

    // This is kind of a hack...
    testOptions = []
    project.properties.keySet().each { key ->
      if (key.startsWith("td_")) {
        String opt = key.substring(3)
        String value = project.properties.get(key).trim()
        if (value == "") {
          testOptions += ["-${opt}"]
        } else {
          testOptions += ["-${opt}:${value}"]
        }
      }
    }
  }

  private void dumpConfiguration() {
    println("==== TestSuite configuration: ====")
    println("Test suite: ${suite}")
    dumpState("Platform", testPlatform)
    dumpState("Dotnet", testDotnet)
    dumpState("Release", testRelease)
    dumpState("Task", testTaskName)
    dumpState("Class", testClass)
    dumpState("Language", testLang)
    dumpState("Directory", testDir)
    dumpState("Exceptions", testEx)
    dumpState("Report", testReportFilename)
    dumpState("Output", testReportDirectory)
    dumpState("Catalog", testCatalog)
    dumpState("Bytecode", testByteCode)
    dumpState("Unfolded", testUnfolded)
    dumpState("Tree", testTree)
    dumpState("Export", testExport)
    dumpState("Jit", testJit)
    if (testOptions.size() != 0) {
      dumpState("Options", testOptions.join(" "))
    }
  }

  private void dumpState(String name, Object value) {
    if (value != null) {
      name = "${name}                              ".substring(0, 12)
      println("${name}: ${value}")
    }
  }

  private void require(String name, Object value) {
    if (value == null) {
      configOk = false
      if (!errorsReported.containsKey(suite)) {
        println("Missing required property in ${suite} test configuration: ${name}")
        errorsReported[suite] = true
      }
    }
  }

  private void forbid(String name, Object value) {
    if (value != null) {
      if (!errorsReported.containsKey(suite)) {
        println("Unknown property in ${suite} test configuration: ${name}")
        errorsReported[suite] = true
      }
      configOk = false
    }
  }

  private void onlyOnce(String name, Object value) {
    if (value != null) {
      throw new GradleException("Only one value for ${name} is allowed in test configuration")
    }
  }

  private String getReportFilename(String reportFilename) {
    if (reportFilename.contains(fileSeparator)) {
      def pos = reportFilename.lastIndexOf(fileSeparator)
      return reportFilename.substring(pos + 1)
    } else {
      // This should never happen, the reportFilename should always be
      // absolute when this function is called.
      reportFilename
    }
  }

  private String getReportDirectory(String reportFilename) {
    if (reportFilename.contains(fileSeparator)) {
      def pos = reportFilename.lastIndexOf(fileSeparator)
      return reportFilename.substring(0, pos)
    } else {
      // This should never happen, the reportFilename should always be
      // absolute when this function is called.
      return System.getProperty("user.dir")
    }
  }

  void taskName(String name) {
    onlyOnce("taskName", testTaskName)
    testTaskName = name
  }

  void platform(String platform) {
    if (platform != "java" && platform != "dotnet") {
      throw new GradleException("Unsupported platform: ${platform}")
    }
    onlyOnce("platform", testPlatform)
    testPlatform = platform
  }

  void dotnet(String dotnet) {
    onlyOnce("dotnet", testDotnet)
    testDotnet = dotnet
  }

  void release(String release) {
    onlyOnce("release", testRelease)
    testRelease = release
  }

  void main(String klass) {
    onlyOnce("main", testClass)
    testClass = klass
  }

  void language(String lang) {
    onlyOnce("language", testLang)
    testLang = lang
  }

  void directory(String dir) {
    onlyOnce("directory", testDir)
    testDir = dir
  }

  void exceptions(String ex) {
    onlyOnce("exceptions", testEx)
    testEx = ex
  }

  void bytecode(boolean bc) {
    onlyOnce("bytecode", testByteCode)
    testByteCode = bc
  }

  void unfolded(boolean un) {
    onlyOnce("unfolded", testUnfolded)
    testUnfolded = un
  }

  void tree(boolean t) {
    onlyOnce("tree", testTree)
    testTree = t
  }

  void export(boolean ex) {
    onlyOnce("export", testExport)
    testExport = ex
  }

  void jit(boolean j) {
    onlyOnce("jit", testJit)
    testJit = j
  }

  void option(String opt) {
    testOptions += ["${opt}"]
  }

  void debug(boolean debugSetting) {
    onlyOnce("debug", testDebug)
    testDebug = debugSetting
  }

  private String computeTaskName() {
    def taskroot = ["qt3" : "QT3TestSuite",
                    "qt3extra": "QT3ExtraTestSuite",
                    "qt4" : "QT4TestSuite",
                    "xslt30": "Xslt30TestSuite",
                    "xslt30extra": "Xslt30ExtraTestSuite",
                    "xsd": "XsdTestSuite"]

    if (!taskroot.containsKey(suite)) {
      throw new GradleException("Unexpected test suite: ${suite}")
    }

    String computedTaskName = "${testRelease}${taskroot[suite]}_${testLang}"

    if (testPlatform == "dotnet") {
      computedTaskName = "dotnet${computedTaskName}"
    }

    if (testByteCode != null) {
      computedTaskName += "_bc${testByteCode ? 'on' : 'off'}"
    }
    if (testUnfolded != null && testUnfolded) {
      computedTaskName += "_un"
    }
    if (testTree != null && testTree) {
      computedTaskName += "_tree"
    }
    if (testExport != null && testExport) {
      computedTaskName += "_export"
    }
    if (testJit != null && testJit) {
      computedTaskName += "_jit"
    }

    return computedTaskName
  }

  private File computeReport() {
    if (project.findProperty("tdReport") != null) {
      return new File(project.findProperty("tdReport"))
    }

    if (suite == "xsd") {
      throw new GradleException("Internal error, computeReport for xsd")
    }

    String computedName = "${project.buildDir}/test-results/${testRelease}j"

    if (testPlatform == "dotnet") {
      computedName = "${project.buildDir}/test-results/${testRelease}dotnet"
    }

    computedName = "${computedName}/${suite}/${project.ext.DSTAMP}"
    computedName = "${computedName}/report_${langCodeMap[testLang]}_${project.ext.saxonVersion}"
    if (testByteCode != null && testByteCode) {
      computedName += "_bc${testByteCode ? 'on' : 'off'}"
    }
    if (testUnfolded != null && testUnfolded) {
      computedName += "_un"
    }
    if (testTree != null && testTree) {
      computedName += "_tree"
    }
    if (testExport != null && testExport) {
      computedName += "_export"
    }
    if (testJit != null && testJit) {
      computedName += "_jit"
    }
    return new File(computedName)
  }

  private String computeClass() {
    def mainclass = ["qt3" : "QT3TestDriver${testRelease.toUpperCase()}",
                     "qt3extra": "QT3TestDriver${testRelease.toUpperCase()}",
                     "qt4" : "QT3TestDriver${testRelease.toUpperCase()}",
                     "xslt30": "Xslt30TestSuiteDriver${testRelease.toUpperCase()}",
                     "xslt30extra": "Xslt30TestSuiteDriver${testRelease.toUpperCase()}",
                     "xsd": "ee.SchemaTestSuiteDriver"]

    if (!mainclass.containsKey(suite)) {
      throw new GradleException("Unexpected test suite: ${suite}")
    }

    String jpackage = "com.saxonica.testdriver"
    if (testRelease == "he") {
      jpackage = "net.sf.saxon.testdriver"
    }

    return "${jpackage}.${mainclass[suite]}"
  }

  private String computeExceptions() {
    String path = "${project.projectDir}/src/test/exceptions/${suite}.xml"
    if (suite == "xsd" && testLang == "1.1") {
      path = "${project.projectDir}/src/test/exceptions/xsd11.xml"
    }
    return path
  }

  def setupInvalidConfig(String testName) {
    project.task(testTaskName) {
      doLast {
        throw new GradleException("Cannot run task, configuration is incorrect.")
      }
    }
    project.task("debug_${testTaskName}") {
      doLast {
        throw new GradleException("Cannot run task, configuration is incorrect.")
      }
    }
  }

  def qt3(Closure cl) {
    reset("qt3")
    cl.delegate = this
    cl()
    if (testPlatform == "dotnet") {
      qt3Dotnet(cl)
    } else {
      qt3Java(cl)
    }
  }

  def qt3Java(Closure cl) {
    require("language", testLang)
    require("directory", testDir)
    require("release", testRelease)
    forbid("jit", testJit)
    forbid("export", testExport)

    testCatalog = testCatalog ?: "catalog.xml"
    testTaskName = testTaskName ?: computeTaskName()
    testClass = testClass ?: computeClass()
    testEx = testEx ?: computeExceptions()

    def testReport = computeReport()
    testReportDirectory = getReportDirectory(testReport.getAbsolutePath())
    testReportFilename = getReportFilename(testReport.getAbsolutePath())

    if (testDebug) {
      dumpConfiguration()
    }

    if (!configOk) {
      setupInvalidConfig(testTaskName)
      return
    }

    project.task(testTaskName, type: JavaExec) {
      inputs.files project.fileTree(dir: "${project.projectDir}/src")
      outputs.file "${testReportDirectory}/${testReportFilename}.xml"
      outputs.upToDateWhen { false }

      def msg = "Run the QT3 test suite driver for ${testLang} with ${testRelease.toUpperCase()}"
      if (testByteCode != null || testUnfolded != null) {
        msg += " (bytecode: ${testByteCode ? 'on' : 'off'}, unfolded: ${testUnfolded ? 'yes' : 'no'})"
      }
      description msg
      dependsOn "${testRelease}jCompile", "${testRelease}j_compileTestDrivers", "cloneQT3TestSuite"
      classpath = project.configurations.testDrivers \
      + project.files("${project.buildDir}/classes/${testRelease}j") \
      + project.files("${project.buildDir}/testdriver_classes/${testRelease}j")

      doFirst {
        project.mkdir testReportDirectory
      }

      main = testClass
      def testargs = [testDir, testCatalog,
                      "-exceptions:${testEx}",
                      "-reportfilename:${testReportFilename}.xml",
                      "-o:${testReportDirectory}",
                      "-lang:${testLang}",
                      "-bytecode:${testByteCode ? 'on' : 'off'}"]

      if (testUnfolded) {
        testargs += ["-unfolded"]
      }

      testargs += testOptions

      //println(testargs)

      args testargs
    }

    project.task("debug_${testTaskName}", type: JavaExec) {
      inputs.files project.fileTree(dir: "${project.projectDir}/src")
      outputs.file "${testReportDirectory}/${testReportFilename}.xml"
      outputs.upToDateWhen { false }

      def msg = "Debug the QT3 test suite driver for ${testLang} with ${testRelease.toUpperCase()}"
      if (testByteCode != null || testUnfolded != null) {
        msg += " (bytecode: ${testByteCode ? 'on' : 'off'}, unfolded: ${testUnfolded ? 'yes' : 'no'})"
      }
      description msg
      dependsOn "compileJava", "cloneQT3TestSuite"
      classpath = project.configurations.testDrivers \
      + project.files("${project.buildDir}/classes/java/main")

      doFirst {
        project.mkdir testReportDirectory
      }

      main = testClass
      def testargs = [testDir, testCatalog,
                      "-exceptions:${testEx}",
                      "-reportfilename:${testReportFilename}.xml",
                      "-o:${testReportDirectory}",
                      "-lang:${testLang}",
                      "-bytecode:${testByteCode ? 'on' : 'off'}",
                      "-unfolded:${testUnfolded ? 'on' : 'off'}"]

      testargs += testOptions
      args testargs
    }

    Task t = project.tasks.findByName("qt3TestsJava")
    if (t) {
      t.dependsOn testTaskName
    }
    t = project.tasks.findByName("${testRelease}jTestSuitesJava")
    if (t) {
      t.dependsOn testTaskName
    }
  }

  def qt3Dotnet(Closure cl) {
    require("language", testLang)
    require("directory", testDir)
    require("release", testRelease)
    forbid("jit", testJit)
    forbid("export", testExport)

    testCatalog = testCatalog ?: "catalog.xml"
    testTaskName = testTaskName ?: computeTaskName()
    testEx = testEx ?: computeExceptions()

    def testReport = computeReport()
    testReportDirectory = getReportDirectory(testReport.getAbsolutePath())
    testReportFilename = getReportFilename(testReport.getAbsolutePath())

    if (testDebug) {
      dumpConfiguration()
    }

    if (!configOk) {
      setupInvalidConfig(testTaskName)
      return
    }

    // There's no bytecode generation and it's only EE
    testTaskName = testTaskName.replace("_bcoff", "").replace("dotnetee", "dotnet")

    project.task(testTaskName, type: Exec) {
      inputs.files project.fileTree(dir: "${project.projectDir}/src")
      outputs.file "${testReportDirectory}/${testReportFilename}.xml"
      outputs.upToDateWhen { false }

      def msg = "Run the QT3 test suite driver for ${testLang} with dotnet"
      if (testByteCode != null || testUnfolded != null) {
        msg += " (bytecode: ${testByteCode ? 'on' : 'off'}, unfolded: ${testUnfolded ? 'yes' : 'no'})"
      }
      description msg
      dependsOn "dotnetTestDrivers", "cloneQT3TestSuite"

      doFirst {
        project.mkdir testReportDirectory
      }

      def driver = "${project.buildDir}/testdrivers/bin/Release/net5.0/TestDrivers.dll"

      def testargs = [testDotnet, driver,
                      testDir,
                      "-exceptions:${testEx}",
                      "-reportfilename:${testReportFilename}.xml",
                      "-o:${testReportDirectory}",
                      "-lang:${testLang}"]

      if (testUnfolded) {
        testargs += ["-unfolded"]
      }

      testargs += testOptions

      commandLine testargs
    }

    Task t = project.tasks.findByName("qt3TestsDotnet")
    if (t) {
      t.dependsOn testTaskName
    }
    t = project.tasks.findByName("${testRelease}jTestSuitesDotnet")
    if (t) {
      t.dependsOn testTaskName
    }
  }

  def qt3extra(Closure cl) {
    reset("qt3extra")
    cl.delegate = this
    cl()
    if (testPlatform == "dotnet") {
      qt3extraDotnet(cl)
    } else {
      qt3extraJava(cl)
    }
  }

  def qt3extraJava(Closure cl) {
    require("language", testLang)
    require("directory", testDir)
    require("release", testRelease)
    forbid("unfolded", testUnfolded)
    forbid("jit", testJit)
    forbid("export", testExport)

    testCatalog = testCatalog ?: "catalog.xml"
    testTaskName = testTaskName ?: computeTaskName()
    testClass = testClass ?: computeClass()
    testEx = testEx ?: computeExceptions()

    def testReport = computeReport()
    testReportDirectory = getReportDirectory(testReport.getAbsolutePath())
    testReportFilename = getReportFilename(testReport.getAbsolutePath())

    if (testDebug) {
      dumpConfiguration()
    }

    if (!configOk) {
      setupInvalidConfig(testTaskName)
      return
    }

    project.task(testTaskName, type: JavaExec) {
      inputs.files project.fileTree(dir: "${project.projectDir}/src")
      outputs.file "${testReportDirectory}/${testReportFilename}.xml"
      outputs.upToDateWhen { false }

      def msg = "Run the QT3 extra test suite driver for ${testLang} with ${testRelease.toUpperCase()}"
      if (testByteCode != null || testUnfolded != null) {
        msg += " (bytecode: ${testByteCode ? 'on' : 'off'}, unfolded: ${testUnfolded ? 'yes' : 'no'})"
      }
      description msg
      dependsOn "${testRelease}jCompile", "${testRelease}j_compileTestDrivers"
      classpath = project.configurations.testDrivers \
      + project.files("${project.buildDir}/classes/${testRelease}j") \
      + project.files("${project.buildDir}/testdriver_classes/${testRelease}j")

      doFirst {
        project.mkdir testReportDirectory
      }

      main = testClass
      def testargs = [testDir, testCatalog,
                      "-exceptions:${testEx}",
                      "-reportfilename:${testReportFilename}.xml",
                      "-o:${testReportDirectory}",
                      "-lang:${testLang}",
                      "-bytecode:${testByteCode ? 'on' : 'off'}",
                      "-unfolded:${testUnfolded ? 'on' : 'off'}"]

      testargs += testOptions

      //println(testargs)

      args testargs
    }

    project.task("debug_${testTaskName}", type: JavaExec) {
      inputs.files project.fileTree(dir: "${project.projectDir}/src")
      outputs.file "${testReportDirectory}/${testReportFilename}.xml"
      outputs.upToDateWhen { false }

      def msg = "Debug the QT3 extra test suite driver for ${testLang} with ${testRelease.toUpperCase()}"
      if (testByteCode != null || testUnfolded != null) {
        msg += " (bytecode: ${testByteCode ? 'on' : 'off'}, unfolded: ${testUnfolded ? 'yes' : 'no'})"
      }
      description msg
      dependsOn "compileJava",
      classpath = project.configurations.testDrivers \
      + project.files("${project.buildDir}/classes/java/main")

      doFirst {
        project.mkdir testReportDirectory
      }

      main = testClass
      def testargs = [testDir, testCatalog,
                      "-exceptions:${testEx}",
                      "-reportfilename:${testReportFilename}.xml",
                      "-o:${testReportDirectory}",
                      "-lang:${testLang}",
                      "-bytecode:${testByteCode ? 'on' : 'off'}",
                      "-unfolded:${testUnfolded ? 'on' : 'off'}"]

      testargs += testOptions
      args testargs
    }

    Task t = project.tasks.findByName("qt3ExtraTestsJava")
    if (t) {
      t.dependsOn testTaskName
    }
    t = project.tasks.findByName("${testRelease}jTestSuitesJava")
    if (t) {
      t.dependsOn testTaskName
    }
  }

  def qt3extraDotnet(Closure cl) {
    require("language", testLang)
    require("directory", testDir)
    require("release", testRelease)
    forbid("unfolded", testUnfolded)
    forbid("jit", testJit)
    forbid("export", testExport)

    testCatalog = testCatalog ?: "catalog.xml"
    testTaskName = testTaskName ?: computeTaskName()
    testClass = testClass ?: computeClass()
    testEx = testEx ?: computeExceptions()

    def testReport = computeReport()
    testReportDirectory = getReportDirectory(testReport.getAbsolutePath())
    testReportFilename = getReportFilename(testReport.getAbsolutePath())

    if (testDebug) {
      dumpConfiguration()
    }

    if (!configOk) {
      setupInvalidConfig(testTaskName)
      return
    }

    // There's no bytecode generation and it's only EE
    testTaskName = testTaskName.replace("_bcoff", "").replace("dotnetee", "dotnet")

    project.task(testTaskName, type: Exec) {
      inputs.files project.fileTree(dir: "${project.projectDir}/src")
      outputs.file "${testReportDirectory}/${testReportFilename}.xml"
      outputs.upToDateWhen { false }

      def msg = "Run the QT3 extra test suite driver for ${testLang} with dotnet}"
      if (testByteCode != null || testUnfolded != null) {
        msg += " (bytecode: ${testByteCode ? 'on' : 'off'}, unfolded: ${testUnfolded ? 'yes' : 'no'})"
      }
      description msg
      dependsOn "dotnetTestDrivers", "cloneQT3TestSuite"

      doFirst {
        project.mkdir testReportDirectory
      }

      def driver = "${project.buildDir}/testdrivers/bin/Release/net5.0/TestDrivers.dll"

      def testargs = [testDotnet, driver,
                      testDir,
                      "-exceptions:${testEx}",
                      "-reportfilename:${testReportFilename}.xml",
                      "-o:${testReportDirectory}",
                      "-lang:${testLang}"]

      testargs += testOptions

      //println(testargs)

      commandLine testargs
    }

    Task t = project.tasks.findByName("qt3ExtraTestsDotnet")
    if (t) {
      t.dependsOn testTaskName
    }
    t = project.tasks.findByName("${testRelease}jTestSuitesDotnet")
    if (t) {
      t.dependsOn testTaskName
    }
  }

  def qt4(Closure cl) {
    reset("qt4")
    cl.delegate = this
    cl()

    require("language", testLang)
    require("directory", testDir)
    require("release", testRelease)
    forbid("jit", testJit)
    forbid("export", testExport)

    testCatalog = testCatalog ?: "catalog.xml"
    testTaskName = testTaskName ?: computeTaskName()
    testClass = testClass ?: computeClass()
    testEx = testEx ?: computeExceptions()

    def testReport = computeReport()
    testReportDirectory = getReportDirectory(testReport.getAbsolutePath())
    testReportFilename = getReportFilename(testReport.getAbsolutePath())

    if (testDebug) {
      dumpConfiguration()
    }

    if (!configOk) {
      setupInvalidConfig(testTaskName)
      return
    }

    project.task(testTaskName, type: JavaExec) {
      inputs.files project.fileTree(dir: "${project.projectDir}/src")
      outputs.file "${testReportDirectory}/${testReportFilename}.xml"
      outputs.upToDateWhen { false }

      def msg = "Run the QT4 test suite driver for ${testLang} with ${testRelease.toUpperCase()}"
      if (testByteCode != null || testUnfolded != null) {
        msg += " (bytecode: ${testByteCode ? 'on' : 'off'}, unfolded: ${testUnfolded ? 'yes' : 'no'})"
      }
      description msg
      dependsOn "${testRelease}jCompile", "${testRelease}j_compileTestDrivers", "cloneQT4TestSuite"
      classpath = project.configurations.testDrivers \
      + project.files("${project.buildDir}/classes/${testRelease}j") \
      + project.files("${project.buildDir}/testdriver_classes/${testRelease}j")

      doFirst {
        project.mkdir testReportDirectory
      }

      main = testClass
      def testargs = [testDir, testCatalog,
                      "-exceptions:${testEx}",
                      "-reportfilename:${testReportFilename}.xml",
                      "-o:${testReportDirectory}",
                      "-lang:${testLang}",
                      "-bytecode:${testByteCode ? 'on' : 'off'}"]

      if (testUnfolded) {
        testargs += ["-unfolded"]
      }

      testargs += testOptions

      //println(testargs)

      args testargs
    }

    project.task("debug_${testTaskName}", type: JavaExec) {
      inputs.files project.fileTree(dir: "${project.projectDir}/src")
      outputs.file "${testReportDirectory}/${testReportFilename}.xml"
      outputs.upToDateWhen { false }

      def msg = "Debug the QT4 test suite driver for ${testLang} with ${testRelease.toUpperCase()}"
      if (testByteCode != null || testUnfolded != null) {
        msg += " (bytecode: ${testByteCode ? 'on' : 'off'}, unfolded: ${testUnfolded ? 'yes' : 'no'})"
      }
      description msg
      dependsOn "compileJava", "cloneQT4TestSuite"
      classpath = project.configurations.testDrivers \
      + project.files("${project.buildDir}/classes/java/main")

      doFirst {
        project.mkdir testReportDirectory
      }

      main = testClass
      def testargs = [testDir, testCatalog,
                      "-exceptions:${testEx}",
                      "-reportfilename:${testReportFilename}.xml",
                      "-o:${testReportDirectory}",
                      "-lang:${testLang}",
                      "-bytecode:${testByteCode ? 'on' : 'off'}",
                      "-unfolded:${testUnfolded ? 'on' : 'off'}"]

      testargs += testOptions
      args testargs
    }

    Task t = project.tasks.findByName("qt4TestsJava")
    if (t) {
      t.dependsOn testTaskName
    }
    t = project.tasks.findByName("${testRelease}jTestSuitesJava")
    if (t) {
      t.dependsOn testTaskName
    }
  }

  def xslt30(Closure cl) {
    reset("xslt30")
    cl.delegate = this
    cl()

    if (testPlatform == "dotnet") {
      xslt30Dotnet(cl)
    } else {
      xslt30Java(cl)
    }
  }

  def xslt30Java(Closure cl) {
    require("language", testLang)
    require("directory", testDir)
    require("release", testRelease)
    forbid("unfolded", testUnfolded)

    testCatalog = testCatalog ?: "catalog.xml"
    testTaskName = testTaskName ?: computeTaskName()
    testClass = testClass ?: computeClass()
    testEx = testEx ?: computeExceptions()

    def testReport = computeReport()
    testReportDirectory = getReportDirectory(testReport.getAbsolutePath())
    testReportFilename = getReportFilename(testReport.getAbsolutePath())

    if (testDebug) {
      dumpConfiguration()
    }

    if (!configOk) {
      setupInvalidConfig(testTaskName)
      return
    }

    project.task(testTaskName, type: JavaExec) {
      inputs.files project.fileTree(dir: "${project.projectDir}/src")
      outputs.file "${testReportDirectory}/${testReportFilename}.xml"
      outputs.upToDateWhen { false }

      def msg = "Run the XSLT 3.0 test suite driver for ${testLang} with ${testRelease.toUpperCase()}"
      if (testByteCode != null || testExport != null || testJit != null) {
        msg += " (bytecode: ${testByteCode ? 'on' : 'off'}, export: ${testExport ? 'yes' : 'no'}, "
        msg += "jit: ${testJit ? 'on' : 'off'})"
      }
      description msg
      dependsOn "${testRelease}jCompile", "${testRelease}j_compileTestDrivers", "cloneXslt30TestSuite"
      classpath = project.configurations.testDrivers \
      + project.files("${project.buildDir}/classes/${testRelease}j") \
      + project.files("${project.buildDir}/testdriver_classes/${testRelease}j")

      doFirst {
        project.mkdir testReportDirectory
      }

      main = testClass
      def testargs = [testDir, testCatalog,
                      "-exceptions:${testEx}",
                      "-reportfilename:${testReportFilename}.xml",
                      "-o:${testReportDirectory}",
                      "-lang:${testLang}",
                      "-bytecode:${testByteCode ? 'on' : 'off'}"]
      if (testExport) {
        testargs += ["-export"]
      }
      if (testJit) {
        testargs += ["-jit"]
      }

      testargs += testOptions

      //println(testargs)

      args testargs
    }

    project.task("debug_${testTaskName}", type: JavaExec) {
      inputs.files project.fileTree(dir: "${project.projectDir}/src")
      outputs.file "${testReportDirectory}/${testReportFilename}.xml"
      outputs.upToDateWhen { false }

      def msg = "Debug the XSLT 3.0 test suite driver for ${testLang} with ${testRelease.toUpperCase()}"
      if (testByteCode != null || testExport != null || testJit != null) {
        msg += " (bytecode: ${testByteCode ? 'on' : 'off'}, export: ${testExport ? 'yes' : 'no'}, "
        msg += "jit: ${testJit ? 'on' : 'off'})"
      }
      description msg
      dependsOn "compileJava", "cloneXslt30TestSuite"
      classpath = project.configurations.testDrivers \
      + project.files("${project.buildDir}/classes/java/main")

      doFirst {
        project.mkdir testReportDirectory
      }

      main = testClass
      def testargs = [testDir, testCatalog,
                      "-exceptions:${testEx}",
                      "-reportfilename:${testReportFilename}.xml",
                      "-o:${testReportDirectory}",
                      "-lang:${testLang}",
                      "-bytecode:${testByteCode ? 'on' : 'off'}"]
      if (testExport) {
        testargs += ["-export"]
      }
      if (testJit) {
        testargs += ["-jit"]
      }

      testargs += testOptions
      args testargs
    }

    Task t = project.tasks.findByName("xslt30TestsJava")
    if (t) {
      t.dependsOn testTaskName
    }
    t = project.tasks.findByName("${testRelease}jTestSuitesJava")
    if (t) {
      t.dependsOn testTaskName
    }
  }

  def xslt30Dotnet(Closure cl) {
    require("language", testLang)
    require("directory", testDir)
    require("release", testRelease)
    forbid("unfolded", testUnfolded)

    testCatalog = testCatalog ?: "catalog.xml"
    testTaskName = testTaskName ?: computeTaskName()
    testClass = testClass ?: computeClass()
    testEx = testEx ?: computeExceptions()

    def testReport = computeReport()
    testReportDirectory = getReportDirectory(testReport.getAbsolutePath())
    testReportFilename = getReportFilename(testReport.getAbsolutePath())

    if (testDebug) {
      dumpConfiguration()
    }

    // There's no bytecode generation and it's only EE
    testTaskName = testTaskName.replace("_bcoff", "").replace("dotnetee", "dotnet")

    if (!configOk) {
      setupInvalidConfig(testTaskName)
      return
    }

    project.task(testTaskName, type: Exec) {
      inputs.files project.fileTree(dir: "${project.projectDir}/src")
      outputs.file "${testReportDirectory}/${testReportFilename}.xml"
      outputs.upToDateWhen { false }

      def msg = "Run the XSLT 3.0 test suite driver for ${testLang} with dotnet"
      if (testByteCode != null || testExport != null || testJit != null) {
        msg += " (bytecode: ${testByteCode ? 'on' : 'off'}, export: ${testExport ? 'yes' : 'no'}, "
        msg += "jit: ${testJit ? 'on' : 'off'})"
      }
      description msg
      dependsOn "dotnetTestDrivers", "cloneQT3TestSuite"

      doFirst {
        project.mkdir testReportDirectory
      }

      def driver = "${project.buildDir}/testdrivers/bin/Release/net5.0/TestDrivers.dll"

      def testargs = [testDotnet, driver,
                      testDir,
                      "-exceptions:${testEx}",
                      "-reportfilename:${testReportFilename}.xml",
                      "-o:${testReportDirectory}",
                      "-lang:${testLang}"]

      testargs += testOptions

      //println(testargs)

      commandLine testargs
    }

    Task t = project.tasks.findByName("xslt30TestsDotnet")
    if (t) {
      t.dependsOn testTaskName
    }
    t = project.tasks.findByName("${testRelease}jTestSuitesDotnet")
    if (t) {
      t.dependsOn testTaskName
    }
  }

  def xslt30extra(Closure cl) {
    reset("xslt30extra")
    cl.delegate = this
    cl()

    require("language", testLang)
    require("directory", testDir)
    require("release", testRelease)
    forbid("unfolded", testUnfolded)

    testCatalog = testCatalog ?: "catalog.xml"
    testTaskName = testTaskName ?: computeTaskName()
    testClass = testClass ?: computeClass()
    testEx = testEx ?: computeExceptions()

    def testReport = computeReport()
    testReportDirectory = getReportDirectory(testReport.getAbsolutePath())
    testReportFilename = getReportFilename(testReport.getAbsolutePath())

    if (testDebug) {
      dumpConfiguration()
    }

    if (!configOk) {
      setupInvalidConfig(testTaskName)
      return
    }

    project.task(testTaskName, type: JavaExec) {
      inputs.files project.fileTree(dir: "${project.projectDir}/src")
      outputs.file "${testReportDirectory}/${testReportFilename}.xml"
      outputs.upToDateWhen { false }

      def msg = "Run the XSLT 3.0 extra test suite driver for ${testLang} with ${testRelease.toUpperCase()}"
      if (testByteCode != null || testExport != null || testJit != null) {
        msg += " (bytecode: ${testByteCode ? 'on' : 'off'}, export: ${testExport ? 'yes' : 'no'}, "
        msg += "jit: ${testJit ? 'on' : 'off'})"
      }
      description msg
      dependsOn "${testRelease}jCompile", "${testRelease}j_compileTestDrivers"
      classpath = project.configurations.testDrivers \
      + project.files("${project.buildDir}/classes/${testRelease}j") \
      + project.files("${project.buildDir}/testdriver_classes/${testRelease}j")

      doFirst {
        project.mkdir testReportDirectory
      }

      main = testClass
      def testargs = [testDir, testCatalog,
                      "-exceptions:${testEx}",
                      "-reportfilename:${testReportFilename}.xml",
                      "-o:${testReportDirectory}",
                      "-lang:${testLang}",
                      "-bytecode:${testByteCode ? 'on' : 'off'}"]
      if (testExport) {
        testargs += ["-export"]
      }
      if (testJit) {
        testargs += ["-jit"]
      }

      testargs += testOptions

      //println(testargs)

      args testargs
    }

    project.task("debug_${testTaskName}", type: JavaExec) {
      inputs.files project.fileTree(dir: "${project.projectDir}/src")
      outputs.file "${testReportDirectory}/${testReportFilename}.xml"
      outputs.upToDateWhen { false }

      def msg = "Debug the XSLT 3.0 extra test suite driver for ${testLang} with ${testRelease.toUpperCase()}"
      if (testByteCode != null || testExport != null || testJit != null) {
        msg += " (bytecode: ${testByteCode ? 'on' : 'off'}, export: ${testExport ? 'yes' : 'no'}, "
        msg += "jit: ${testJit ? 'on' : 'off'})"
      }
      description msg
      dependsOn "compileJava"
      classpath = project.configurations.testDrivers \
      + project.files("${project.buildDir}/classes/java/main")

      doFirst {
        project.mkdir testReportDirectory
      }

      main = testClass
      def testargs = [testDir, testCatalog,
                      "-exceptions:${testEx}",
                      "-reportfilename:${testReportFilename}.xml",
                      "-o:${testReportDirectory}",
                      "-lang:${testLang}",
                      "-bytecode:${testByteCode ? 'on' : 'off'}"]
      if (testExport) {
        testargs += ["-export"]
      }
      if (testJit) {
        testargs += ["-jit"]
      }

      testargs += testOptions
      args testargs
    }

    Task t = project.tasks.findByName("xslt30ExtraTestsJava")
    if (t) {
      t.dependsOn testTaskName
    }
    t = project.tasks.findByName("${testRelease}jTestSuitesJava")
    if (t) {
      t.dependsOn testTaskName
    }
  }

  def xsd(Closure cl) {
    reset("xsd")
    cl.delegate = this
    cl()

    require("language", testLang)
    require("directory", testDir)
    require("release", testRelease)
    forbid("unfolded", testUnfolded)
    forbid("jit", testJit)
    forbid("export", testExport)

    testCatalog = testCatalog ?: "catalog.xml"
    testTaskName = testTaskName ?: computeTaskName()
    testClass = testClass ?: computeClass()
    testEx = testEx ?: computeExceptions()

    def computedReportFn = "${project.buildDir}/test-results/${testRelease}j/xsd/${project.ext.DSTAMP}"
    computedReportFn = "${computedReportFn}/SaxonResults${project.ext.saxonVersion}-${testLang}"
    if (testByteCode != null && testByteCode) {
      computedReportFn += "_bc${testByteCode ? 'on' : 'off'}"
    }

    def computedErrorFn = "${project.buildDir}/test-results/${testRelease}j/xsd/${project.ext.DSTAMP}"
    computedErrorFn = "${computedErrorFn}/SaxonFailures${project.ext.saxonVersion}-${testLang}"
    if (testByteCode != null && testByteCode) {
      computedErrorFn += "_bc${testByteCode ? 'on' : 'off'}"
    }

    def testReport = null
    if (project.findProperty("tdReport") != null) {
      testReport = new File(project.findProperty("tdReport"))
    } else {
      testReport = new File(computedReportFn)
    }

    testReportDirectory = getReportDirectory(testReport.getAbsolutePath())
    testReportFilename = getReportFilename(testReport.getAbsolutePath())
    def testErrorFilename = getReportFilename(new File(computedErrorFn).getAbsolutePath())

    if (testDebug) {
      dumpConfiguration()
    }

    if (!configOk) {
      setupInvalidConfig(testTaskName)
      return
    }

    project.task(testTaskName, type: JavaExec) {
      inputs.files project.fileTree(dir: "${project.projectDir}/src")
      outputs.file "${testReportDirectory}/${testReportFilename}.xml"
      outputs.file "${testReportDirectory}/${testErrorFilename}.xml"
      outputs.upToDateWhen { false }

      def msg = "Run the XSD test suite driver for ${testLang} with ${testRelease.toUpperCase()}"
      if (testByteCode != null) {
        msg += " (bytecode: ${testByteCode ? 'on' : 'off'})"
      }
      description msg
      dependsOn "${testRelease}jCompile", "${testRelease}j_compileTestDrivers", "cloneXsdTestSuite"
      classpath = project.configurations.testDrivers \
      + project.files("${project.buildDir}/classes/${testRelease}j") \
      + project.files("${project.buildDir}/testdriver_classes/${testRelease}j")

      doFirst {
        project.mkdir testReportDirectory
      }

      main = testClass
      def testargs = [testDir, testCatalog,
                      "-exceptions:${testEx}",
                      "-reportfilename:${testReportFilename}.xml",
                      "-errorfilename:${testErrorFilename}.xml",
                      "-local:${testReportDirectory}",
                      "-summarizeresults:${project.projectDir}/src/test/extra/tools/summarize-results.xsl",
                      "-v:${testLang}",
                      "-bytecode:${testByteCode ? 'on' : 'off'}"]

      testargs += testOptions

      // println(testargs)

      args testargs
    }

    project.task("debug_${testTaskName}", type: JavaExec) {
      inputs.files project.fileTree(dir: "${project.projectDir}/src")
      outputs.file "${testReportDirectory}/${testReportFilename}.xml"
      outputs.file "${testReportDirectory}/${testErrorFilename}.xml"
      outputs.upToDateWhen { false }

      def msg = "Debug the XSD test suite driver for ${testLang} with ${testRelease.toUpperCase()}"
      if (testByteCode != null) {
        msg += " (bytecode: ${testByteCode ? 'on' : 'off'})"
      }
      description msg
      dependsOn "compileJava", "cloneXsdTestSuite"
      classpath = project.configurations.testDrivers \
      + project.files("${project.buildDir}/classes/java/main")

      doFirst {
        project.mkdir testReportDirectory
      }

      main = testClass
      def testargs = [testDir, testCatalog,
                      "-exceptions:${testEx}",
                      "-reportfilename:${testReportFilename}.xml",
                      "-errorfilename:${testErrorFilename}.xml",
                      "-local:${testReportDirectory}",
                      "-summarizeresults:${project.projectDir}/src/test/extra/tools/summarize-results.xsl",
                      "-v:${testLang}",
                      "-bytecode:${testByteCode ? 'on' : 'off'}"]

      testargs += testOptions

      args testargs
    }

    Task t = project.tasks.findByName("XsdTestsJava")
    if (t) {
      t.dependsOn testTaskName
    }
    t = project.tasks.findByName("${testRelease}jTestSuitesJava")
    if (t) {
      t.dependsOn testTaskName
    }
  }
}
