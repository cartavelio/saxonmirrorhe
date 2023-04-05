import org.gradle.api.GradleException
import org.gradle.api.Task
import org.gradle.api.Project
import org.gradle.api.tasks.Exec
import org.gradle.api.tasks.JavaExec
import org.gradle.internal.os.OperatingSystem;
import java.nio.file.Files
import java.nio.file.Paths
import java.nio.file.StandardCopyOption
import java.nio.charset.StandardCharsets
import groovy.util.slurpersupport.GPathResult

class SaxonTestSuite {
  static String fileSeparator = System.getProperty("file.separator")
  private def optionMap = [:]
  private def directoryMap = [:]

  def errorsReported = [:]
  Project project = null
  def testOptions = []
  def testReportArchive = null
  def testDriverOutputRedirect = false
  def debugExec = true
  def ignoreFailures = false
  def accumulateFailures = null

  SaxonTestSuite(Project project) {
    this.project = project

    project.properties.keySet().each { key ->
      if (key.startsWith("td_")) {
        String opt = key.substring(3)
        String value = project.properties.get(key).trim()
        if (value == "") {
          this.testOptions += ["-${opt}"]
        } else {
          this.testOptions += ["-${opt}:${value}"]
        }
      } else if (key == "saxonicaTestDriverReportArchive") {
        def dir = project.properties.get(key).trim()
        if (dir.endsWith("/") || dir.endsWith("\\")) {
          dir = dir.substring(0, dir.length() - 1)
        }
        this.testReportArchive = dir
      } else if (key == "saxonicaTestDriverOutputRedirect") {
        def bool = project.properties.get(key).trim()
        this.testDriverOutputRedirect = (bool == "true" || bool == "1" || bool == "yes")
      } else if (key == "saxonicaTestDriverDebugExec") {
        def bool = project.properties.get(key).trim()
        this.debugExec = (bool == "true" || bool == "1" || bool == "yes")
      } else if (key == "saxonicaIgnoreFailures") {
        def bool = project.properties.get(key).trim()
        this.ignoreFailures = (bool == "true" || bool == "1" || bool == "yes")
      } else if (key == "accumulateFailures") {
        def bool = project.properties.get(key).trim()
        if (bool == "true" || bool == "1" || bool == "yes") {
          this.accumulateFailures = []
        }
      }
    }

    this.testDriverOutputRedirect = this.testDriverOutputRedirect && this.testOptions.size() == 0
  }

  def configure(options) {
    if ("directoryMap" in options) {
      this.directoryMap = options["directoryMap"]
    } else {
      throw new GradleException("No directoryMap in testSuite options")
    }

    if ("optionMap" in options) {
      this.optionMap = options["optionMap"]
    } else {
      throw new GradleException("No optionMap in testSuite options")
    }
  }

  def task(options) {
    if (!('suite' in options)) {
      throw new GradleException("No test suite specified.")
    }
    if (!(options['suite'] in this.directoryMap)) {
      String testTaskName = options["name"]
      Task incomplete = project.task("${testTaskName}_test_suite") {
        doLast {
          throw new GradleException("Incomplete configuration for test suite ${options['suite']}")
        }
      }
      Task t = project.tasks.findByName("all_test_suites")
      if (t) {
        t.dependsOn incomplete
      }
      return
    }

    // Defaults
    options['catalog'] = options.getOrDefault('catalog', 'catalog.xml')

    def reportdir = "${project.buildDir}/test-results/"
    reportdir += "${options['product']}/${options['suite']}/"
    reportdir += "${options['language']}/${project.ext.DSTAMP}/"
    def reportfn = options['name']

    reportdir = options.getOrDefault('reportDirectory', reportdir)
    reportfn = options.getOrDefault('reportFilename', reportfn)
    if (reportdir.endsWith("/") || reportdir.endsWith("\\")) {
      reportdir = reportdir.substring(0, reportdir.length() - 1)
    }

    options['reportDirectory'] = reportdir
    options['reportFilename'] = reportfn
    options['report'] = "${reportdir}/${reportfn}"

    if (options["platform"] == "java") {
      this.javaTask(options)
    } else if (options["platform"] == "dotnet") {
      this.dotnetTask(options)
    } else {
      throw new GradleException("Unexpected platform: ${platform}")
    }
  }

  def javaTask(options) {
    if (options['suite'] == "xsd") {
      this.javaXsdTask(options)
      return
    }

    String testTaskName = options["name"]
    String directory = this.directoryMap[options['suite']]
    def optionMap = this.optionMap

    Task run = project.task("run__${testTaskName}", type: JavaExec) {
      inputs.files project.fileTree(dir: "${project.projectDir}/src/main/java")
      outputs.file "${options['report']}.xml"
      outputs.upToDateWhen { false }

      doFirst {
        project.mkdir options["reportDirectory"]
      }

      String prod = options['product']
      
      def msg = "Run the ${options['suite']} test suite "
      msg += "for ${options['language']} with ${prod}"
      description msg

      def depends = ["${prod}Compile", "${prod}_compileTestDrivers"]
      depends += ["compile_function_library"]
      depends += options.getOrDefault("depends", [])
      depends.each { name ->
        dependsOn name
      }

      classpath = project.configurations.testDrivers \
        + project.files("${project.buildDir}/classes/${prod}") \
        + project.files("${project.buildDir}/testdriver_classes/${prod}") \
        + project.files("${project.buildDir}/resources/main")

      def testargs = [directory,
                      options["catalog"],
                      "-exceptions:${options['exceptions']}",
                      "-reportfilename:${options['reportFilename']}.xml",
                      "-o:${options['reportDirectory']}",
                      "-lang:${options['language']}"]

      optionMap.keySet().each { name ->
        if (name in options) {
          def argform = optionMap[name]
          if (argform.endsWith(":")) {
            argform += options[name]
          } else {
            if (options[name] != "on") {
              println("Warning: option '${options[name]}' to ${name} is ignored in ${testTaskName}...")
            }
          }
          testargs += [argform]
        }
      }

      testargs += testOptions

      doFirst {
        if (this.debugExec) {
          println("Running ${options['class']} ${testargs.join(' ')}")
        }
      }

      def stdout = new ByteArrayOutputStream()
      def stderr = new ByteArrayOutputStream()

      if (this.testDriverOutputRedirect) {
        standardOutput = stdout
        errorOutput = stderr

        doFirst {
          println("Redirecting output to .../${options['reportFilename']}.{out,err}")
          stderr.write("${options['class']} ${testargs.join(' ')}\n"
                       .getBytes(StandardCharsets.UTF_8))
        }

        doLast {
          def fos = new FileOutputStream(new File("${options['report']}.out"))
          fos.write(stdout.toByteArray())
          fos = new FileOutputStream(new File("${options['report']}.err"))
          fos.write(stderr.toByteArray())
        }

        doLast {
          def result = executionResult.get()
          result.assertNormalExitValue()
        }
      }

      mainClass = options['class']
      args testargs
      ignoreExitValue = true
    }

    Task testTask = project.task("${testTaskName}_test_suite", dependsOn: ["run__${testTaskName}"]) {
      String resultOutputPath = "${project.buildDir}/test-results/"
      def reportdir = options['reportDirectory']

      doLast {
        if (run.executionResult.isPresent()
            && run.executionResult.get().getExitValue() == 0) {
          summarize(options, new File("${options['report']}.xml"))
          if (testReportArchive != null
              && reportdir.startsWith(resultOutputPath)) {
            def arcpath = "${this.testReportArchive}/${options['product']}/"
            arcpath += "${project.ext.saxonVersion}/"
            arcpath += "${options['suite']}/${project.ext.DSTAMP}"
            saveTheResults(options, arcpath)
          }
        }
      }
    }

    Task t = project.tasks.findByName("all_test_suites")
    if (t) {
      t.dependsOn testTask
    }

    t = project.tasks.findByName("all_${options['platform']}_test_suites")
    if (t) {
      t.dependsOn testTask
    }

    t = project.tasks.findByName("all_${options['product']}_test_suites")
    if (t) {
      t.dependsOn testTask
    }

    t = project.tasks.findByName("all_${options['product']}_${options['suite']}_test_suites")
    if (t) {
      t.dependsOn testTask
    }
  }

  def javaXsdTask(options) {
    String testTaskName = options["name"]
    String directory = this.directoryMap[options['suite']]
    def optionMap = this.optionMap

    def version = "1.0"
    if (options['language'] == "XSD11") {
      version = "1.1"
    }

    def resultFn = "${options['reportDirectory']}/"
    resultFn += "SaxonResults${project.ext.saxonVersion}-"
    resultFn += options['language'] + ".xml"

    def errorFn = "${options['reportDirectory']}/"
    errorFn += "SaxonFailures${project.ext.saxonVersion}-"
    errorFn += options['language'] + "-errors.xml"

    Task run = project.task("run__${testTaskName}", type: JavaExec) {
      inputs.files project.fileTree(dir: "${project.projectDir}/src/main/java")
      outputs.file resultFn
      outputs.file errorFn
      outputs.upToDateWhen { false }

      doFirst {
        project.mkdir options["reportDirectory"]
      }

      String prod = options['product']
      
      def msg = "Run the ${options['suite']} test suite "
      msg += "for ${options['language']} with ${prod}"
      description msg

      def depends = ["${prod}Compile", "${prod}_compileTestDrivers"]
      depends += options.getOrDefault("depends", [])
      depends.each { name ->
        dependsOn name
      }

      classpath = project.configurations.testDrivers \
        + project.files("${project.buildDir}/classes/${prod}") \
        + project.files("${project.buildDir}/testdriver_classes/${prod}")

      def testargs = [directory,
                      options["catalog"],
                      "-exceptions:${options['exceptions']}",
                      "-reportfilename:${resultFn}",
                      "-errorfilename:${errorFn}",
                      "-local:${options['reportDirectory']}",
                      "-v:${version}"]

      optionMap.keySet().each { name ->
        if (name in options) {
          def argform = optionMap[name]
          if (argform.endsWith(":")) {
            argform += options[name]
          } else {
            if (options[name] != "on") {
              println("Warning: option '${options[name]}' to ${name} is ignored in ${testTaskName}...")
            }
          }
          testargs += [argform]
        }
      }

      doFirst {
        if (this.debugExec) {
          println("Running ${options['class']} ${testargs.join(' ')}")
        }
      }

      def stdout = new ByteArrayOutputStream()
      def stderr = new ByteArrayOutputStream()

      if (this.testDriverOutputRedirect) {
        standardOutput = stdout
        errorOutput = stderr

        doFirst {
          println("Redirecting output to .../${options['reportFilename']}.{out,err}")
          stderr.write("${options['class']} ${testargs.join(' ')}\n"
                       .getBytes(StandardCharsets.UTF_8))
        }

        doLast {
          def fos = new FileOutputStream(new File("${options['report']}.out"))
          fos.write(stdout.toByteArray())
          fos = new FileOutputStream(new File("${options['report']}.err"))
          fos.write(stderr.toByteArray())
        }

        doLast {
          def result = executionResult.get()
          result.assertNormalExitValue()
        }
      }

      mainClass = options['class']
      args testargs
      ignoreExitValue true
    }

    Task testTask = project.task("${testTaskName}_test_suite", dependsOn: ["run__${testTaskName}"]) {
      String resultOutputPath = "${project.buildDir}/test-results/"
      def reportdir = options['reportDirectory']

      doLast {
        if (run.executionResult.isPresent()
            && run.executionResult.get().getExitValue() == 0) {
          //summarize(options, new File("${options['report']}.xml"))
          if (testReportArchive != null
              && reportdir.startsWith(resultOutputPath)) {
            def arcpath = "${this.testReportArchive}/${options['product']}/"
            arcpath += "${project.ext.saxonVersion}/"
            arcpath += "${options['suite']}/${project.ext.DSTAMP}"
            saveTheXsdResults(options, arcpath)
          }
        }
      }
    }

    Task t = project.tasks.findByName("all_test_suites")
    if (t) {
      t.dependsOn testTask
    }

    t = project.tasks.findByName("all_${options['platform']}_test_suites")
    if (t) {
      t.dependsOn testTask
    }

    t = project.tasks.findByName("all_${options['product']}_test_suites")
    if (t) {
      t.dependsOn testTask
    }

    t = project.tasks.findByName("all_${options['product']}_${options['suite']}_test_suites")
    if (t) {
      t.dependsOn testTask
    }
  }

  // ============================================================

  def dotnetTask(options) {
    if (options['suite'] == "xsd") {
      this.dotnetXsdTask(options)
      return
    }

    String testTaskName = options["name"]
    String directory = this.directoryMap[options['suite']]
    def optionMap = this.optionMap

    Task run = project.task("run__${testTaskName}", type: Exec) {
      inputs.files project.fileTree(dir: "${project.projectDir}/src")
      outputs.file "${options['report']}.xml"
      outputs.upToDateWhen { false }
      dependsOn "eecs_featureKeys"

      doFirst {
        project.mkdir options["reportDirectory"]
      }

      String prod = options['product']
      
      def msg = "Run the ${options['suite']} test suite "
      msg += "for ${options['language']} with ${prod}"
      description msg

      def depends = ["dotnetTestDrivers"]
      depends += options.getOrDefault("depends", [])
      depends.each { name ->
        dependsOn name
      }

      def testargs = [options["dotnet"],
                      "${project.buildDir}/testdrivers/bin/Release/net6.0/TestDrivers.dll",
                      directory,
                      "-exceptions:${options['exceptions']}",
                      "-reportfilename:${options['reportFilename']}.xml",
                      "-o:${options['reportDirectory']}",
                      "-lang:${options['language']}"]

      optionMap.keySet().each { name ->
        if (name in options) {
          def argform = optionMap[name]
          if (argform.endsWith(":")) {
            argform += options[name]
          } else {
            if (options[name] != "on") {
              println("Warning: option '${options[name]}' to ${name} is ignored in ${testTaskName}...")
            }
          }
          testargs += [argform]
        }
      }

      doFirst {
        if (this.debugExec) {
          println("Running ${testargs.join(' ')}")
        }
      }

      def stdout = new ByteArrayOutputStream()
      def stderr = new ByteArrayOutputStream()

      if (this.testDriverOutputRedirect) {
        standardOutput = stdout
        errorOutput = stderr

        doFirst {
          println("Redirecting output to .../${options['reportFilename']}.{out,err}")
          stderr.write("${testargs.join(' ')}\n"
                       .getBytes(StandardCharsets.UTF_8))
        }

        doLast {
          def fos = new FileOutputStream(new File("${options['report']}.out"))
          fos.write(stdout.toByteArray())
          fos = new FileOutputStream(new File("${options['report']}.err"))
          fos.write(stderr.toByteArray())
        }

        doLast {
          def result = executionResult.get()
          if (!this.ignoreFailures) {
            result.assertNormalExitValue()
          }
        }
      }

      commandLine testargs
      ignoreExitValue = true
    }

    Task testTask = project.task("${testTaskName}_test_suite",
                                 dependsOn: ["run__${testTaskName}"]) {
      String resultOutputPath = "${project.buildDir}/test-results/"
      def reportdir = options['reportDirectory']

      doLast {
        if (run.executionResult.isPresent()
            && run.executionResult.get().getExitValue() == 0) {
          summarize(options, new File("${options['report']}.xml"))
          if (testReportArchive != null
              && reportdir.startsWith(resultOutputPath)) {
            def arcpath = "${this.testReportArchive}/${options['product']}/"
            arcpath += "${project.ext.saxonVersion}/"
            arcpath += "${options['suite']}/${project.ext.DSTAMP}"
            saveTheResults(options, arcpath)
          }
        }
      }
    }

    Task t = project.tasks.findByName("all_test_suites")
    if (t) {
      t.dependsOn testTask
    }

    t = project.tasks.findByName("all_${options['platform']}_test_suites")
    if (t) {
      t.dependsOn testTask
    }

    t = project.tasks.findByName("all_${options['product']}_test_suites")
    if (t) {
      t.dependsOn testTask
    }

    t = project.tasks.findByName("all_${options['product']}_${options['suite']}_test_suites")
    if (t) {
      t.dependsOn testTask
    }
  }

  def dotnetXsdTask(options) {
    String testTaskName = options["name"]
    String directory = this.directoryMap[options['suite']]
    def optionMap = this.optionMap

    def version = "1.0"
    if (options['language'] == "XSD11") {
      version = "1.1"
    }

    def resultFn = "${options['reportDirectory']}/"
    resultFn += "SaxonResults${project.ext.saxonVersion}-"
    resultFn += options['language'] + ".xml"

    def errorFn = "${options['reportDirectory']}/"
    errorFn += "SaxonFailures${project.ext.saxonVersion}-"
    errorFn += options['language'] + "-errors.xml"

    Task run = project.task("run__${testTaskName}", type: Exec) {
      inputs.files project.fileTree(dir: "${project.projectDir}/src")
      outputs.file resultFn
      outputs.file errorFn
      outputs.upToDateWhen { false }
      dependsOn "eecs_featureKeys"

      doFirst {
        project.mkdir options["reportDirectory"]
      }

      String prod = options['product']
      
      def msg = "Run the ${options['suite']} test suite "
      msg += "for ${options['language']} with ${prod}"
      description msg

      def depends = ["dotnetTestDrivers"]
      depends += options.getOrDefault("depends", [])
      depends.each { name ->
        dependsOn name
      }

      def testargs = [options["dotnet"],
                      "${project.buildDir}/testdrivers/bin/Release/net6.0/TestDrivers.dll",
                      directory,
                      "-o:${options['reportDirectory']}",
                      "-lang:${options['language']}",
                      '-debug',
                      "-exceptions:${options['exceptions']}"]
      optionMap.keySet().each { name ->
        if (name in options) {
          def argform = optionMap[name]
          if (argform.endsWith(":")) {
            argform += options[name]
          } else {
            if (options[name] != "on") {
              println("Warning: option '${options[name]}' to ${name} is ignored in ${testTaskName}...")
            }
          }
          testargs += [argform]
        }
      }

      doFirst {
        if (this.debugExec) {
          println("Running ${testargs.join(' ')}")
        }
      }

      def stdout = new ByteArrayOutputStream()
      def stderr = new ByteArrayOutputStream()

      if (this.testDriverOutputRedirect) {
        standardOutput = stdout
        errorOutput = stderr

        doFirst {
          println("Redirecting output to .../${options['reportFilename']}.{out,err}")
          stderr.write("${testargs.join(' ')}\n"
                       .getBytes(StandardCharsets.UTF_8))
        }

        doLast {
          def fos = new FileOutputStream(new File("${options['report']}.out"))
          fos.write(stdout.toByteArray())
          fos = new FileOutputStream(new File("${options['report']}.err"))
          fos.write(stderr.toByteArray())
        }

        doLast {
          def result = executionResult.get()
          result.assertNormalExitValue()
        }
      }

      commandLine testargs
      ignoreExitValue true
    }

    Task testTask = project.task("${testTaskName}_test_suite", dependsOn: ["run__${testTaskName}"]) {
      String resultOutputPath = "${project.buildDir}/test-results/"
      def reportdir = options['reportDirectory']

      doLast {
        if (run.executionResult.isPresent()
            && run.executionResult.get().getExitValue() == 0) {
          //summarize(options, new File("${options['report']}.xml"))
          if (testReportArchive != null
              && reportdir.startsWith(resultOutputPath)) {
            def arcpath = "${this.testReportArchive}/${options['product']}/"
            arcpath += "${project.ext.saxonVersion}/"
            arcpath += "${options['suite']}/${project.ext.DSTAMP}"
            saveTheXsdResults(options, arcpath)
          }
        }
      }
    }

    Task t = project.tasks.findByName("all_test_suites")
    if (t) {
      t.dependsOn testTask
    }

    t = project.tasks.findByName("all_${options['platform']}_test_suites")
    if (t) {
      t.dependsOn testTask
    }

    t = project.tasks.findByName("all_${options['product']}_test_suites")
    if (t) {
      t.dependsOn testTask
    }

    t = project.tasks.findByName("all_${options['product']}_${options['suite']}_test_suites")
    if (t) {
      t.dependsOn testTask
    }
  }

  def checkAccumulatedFailures() {
    if (this.accumulateFailures != null && this.accumulateFailures.size() > 0) {
      def count = 0
      this.accumulateFailures.each { msg ->
        println(msg)
        count += 1
      }
      throw new GradleException("Regression: ${count} failures.")
    }
  }

  // ============================================================

  public void saveTheResults(options, arcpath) {
    def reportfn = "${options['report']}.xml"
    def arcfn = "${arcpath}/${options['reportFilename']}.xml"

    String mostRecent = null
    File compare = null

    File report = new File(reportfn)
    if (!report.exists()) {
      if (this.ignoreFailures) {
        println("${options['name']} did not generate a test suite report.")
        return
      }
      throw new GradleException("${options['name']} did not generate a test suite report.")
    }

    File path = new File(arcpath)
    if (! path.exists()) {
      path.mkdirs()
    }

    File parent = path.getParentFile()
    for (File entry : parent.listFiles()) {
      if (entry.isDirectory()) {
        for (File fn : entry.listFiles()) {
          if (fn.getName() == report.getName()) {
            if (mostRecent == null || mostRecent < entry.getName()) {
              mostRecent = entry.getName()
              compare = fn
            }
          }
        }
      }
    }

    if (compare != null) {
      if (!this.compareTheResults(options, compare, new File(reportfn))) {
        return
      }
    } else {
      println("No previous results for ${options['name']}.")
    }

    Files.copy(Paths.get(reportfn), Paths.get(arcfn),
               StandardCopyOption.REPLACE_EXISTING)
  }

  public void saveTheXsdResults(options, arcpath) {
    def arcfn = "${arcpath}/${options['reportFilename']}.xml"

    // This is all an awful hack because the XSD tests write
    // results using filenames calculated internally, e.g.:
    //   SaxonResults11.0-XSD10.xml
    //   SaxonFailures11.0-XSD10-errors.xml
    def parts = arcfn.split("/");
    def pver = parts[parts.size() - 4];

    parts = options['reportFilename'].split("_")
    def psuite = parts[2];
    def reportfn = "${options['reportDirectory']}/SaxonResults${pver}-${psuite}.xml";

    // N.B. The eej and eedotnet patterns are different!
    def eedotnet = (parts[0] == "eedotnet")
    if (eedotnet) {
      reportfn = "${options['reportDirectory']}/SaxonResults${pver}n.xml";
    }

    String mostRecent = null
    File compare = null

    File report = new File(reportfn)
    if (!report.exists()) {
      if (this.ignoreFailures) {
        println("${options['name']} did not generate a test suite report.")
        return
      }
      throw new GradleException("${options['name']} did not generate a test suite report.")
    }

    File path = new File(arcpath)
    if (! path.exists()) {
      path.mkdirs()
    }

    /* look for previous version when I've sorted out how to compare them
    File parent = path.getParentFile()
    for (File entry : parent.listFiles()) {
      if (entry.isDirectory()) {
        for (File fn : entry.listFiles()) {
          if (fn.getName() == report.getName()) {
            if (mostRecent == null || mostRecent < entry.getName()) {
              mostRecent = entry.getName()
              compare = fn
            }
          }
        }
      }
    }
     */

    if (compare != null) {
      if (!this.compareTheResults(options, compare, new File(reportfn))) {
        return
      }
    } else {
      println("Comparisons are unavailable for XSD reports: ${options['name']}.")
    }

    Files.copy(Paths.get(reportfn), Paths.get(arcfn),
               StandardCopyOption.REPLACE_EXISTING)

    if (!eedotnet) {
      reportfn = "${options['reportDirectory']}/SaxonFailures${pver}-${psuite}-errors.xml";
      arcfn = "${arcpath}/${options['reportFilename']}-errors.xml"

      Files.copy(Paths.get(reportfn), Paths.get(arcfn),
                 StandardCopyOption.REPLACE_EXISTING)
    }
  }

  private HashMap findTests(GPathResult root) {
    def results = [:]

    def tests = root.'**'.findAll { it.name() == "test-case" }
    tests.each { t ->
      def parent = t.parent()
      while (parent && parent.name() != 'test-set') {
        parent = parent.parent()
      }

      def setName = parent.attributes()['name']

      def testName = t.attributes()['name']
      def result = t.attributes()['result']
      results["${setName}/${testName}"] = result
    }

    return results
  }

  private boolean compareTheResults(options, File previous, File current) {
    def slurper = new XmlSlurper()
    GPathResult root = slurper.parse(previous)
      .declareNamespace(fots: "http://www.w3.org/2012/08/qt-fots-results")

    def prevPass = findTests(root)

    root = slurper.parse(current)
      .declareNamespace(fots: "http://www.w3.org/2012/08/qt-fots-results")

    def curPass = findTests(root)

    int failing = 0
    curPass.each { key ->
      def result = curPass[key]
      if (key in prevPass) {
        prevResult = prevPass[key]
        if (prevResult == "pass" && result != "pass") {
          def msg = "👎 ${setName}/${testName} was ${prevResult} is now ${result}"
          println(msg)
          failing += 1
          if (this.accumulateFailures != null) {
            this.accumulateFailures += "${options['name']}: ${msg}"
          }
        }
      }
    }

    if (failing > 0) {
      def msg = "Regression: ${failing} previously passing"
      if (failing > 1) {
        msg += " tests are failing."
      } else {
        msg += " test is failing."
      }
      if (this.ignoreFailures || this.accumulateFailures != null) {
        println(msg)
      } else {
        throw new GradleException(msg)
      }
    }

    return failing == 0
  }

  public void summarize(options, File current) {
    def slurper = new XmlSlurper()
    GPathResult root = slurper.parse(current)
      .declareNamespace(fots: "http://www.w3.org/2012/08/qt-fots-results")

    def summary = findTests(root)
    def results = [:]
    def total = 0
    def pass = 0
    def fail = 0
    def other = 0

    summary.keySet().each { key ->
      def result = summary[key]
      if (result in results) {
        results[result] += 1
      } else {
        results[result] = 1
      }
      if (result != "n/a" && result != "notRun") {
        total += 1
        if (result == "pass") {
          pass += 1
        } else if (result == "fail") {
          fail += 1
        } else {
          other += 1
        }
      }
    }

    def os = "???"
    if (OperatingSystem.current().isLinux()) { os = "Lin" }
    if (OperatingSystem.current().isWindows()) { os = "Win" }
    if (OperatingSystem.current().isMacOsX()) { os = "Mac" }

    println("Summary ${options['name']} (${os}): ${pass} pass, ${fail} fail, ${other} other")

    // HACK
    def passing = "########################################################################"
    def failing = "........................................................................"
    def barlen = passing.length()

    if (total > 0) {
      def passperc = pass / total
      def perc = sprintf("%4.2f", passperc * 100.0)

      def passcount = Math.floor(passperc * barlen).intValue()
      def failcount = barlen - passcount

      println("${passing.substring(0, passcount)}${failing.substring(0, failcount)} ${perc}%")
    } else {
      println("No tests ran.")
    }
  }
}
