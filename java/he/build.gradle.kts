import com.saxonica.build.java.PomGenerator
import com.saxonica.build.java.testsuite.JavaRunner

plugins {
  id("saxon-build")
  id("com.saxonica.build.testsuites")
  id("saxonj")
  id("maven-publish")
  id("signing")
}

saxonj {
  edition = "he"
  metaInfServices = mapOf(
    "javax.xml.transform.TransformerFactory" to "net.sf.saxon.TransformerFactoryImpl"
  )
  manifestMainClass = "net.sf.saxon.Transform"
  sourceRoot = layout.projectDirectory.dir("../src")
  testDataDir = layout.projectDirectory.dir("../../src/test/testdata")
}

// Tests run on JDK 21. Keep the distributed HE classes compatible with Java 8.
tasks.named<JavaCompile>("compileEditionTestJava") {
  sourceCompatibility = "17"
  targetCompatibility = "17"
}

val saxonBuild = the<SaxonBuildExtension>()

// Keep normal HE artifacts independent of checkout and generated-file mtimes.
tasks.withType<AbstractArchiveTask>().configureEach {
  isPreserveFileTimestamps = false
}

sourceSets.named("editionTest") {
  java {
    // Exclude everything that won't build in HE...
    exclude("xqjtck/**")
    exclude("com/saxonica/**/*.java")
    exclude("commands/AntTests.java")
    exclude("commands/QueryTests.java")
    exclude("commands/Saxon3483.java")
    exclude("commands/TransformTests.java")
    exclude("commands/ValidateTests.java")
    exclude("failing/AntTests.java")
    exclude("failing/FailingTests.java")
    exclude("internaltests/ActivateSaxon.java")
    exclude("internaltests/AlphaCodeTypeTest.java")
    exclude("internaltests/GizmoCommandTests.java")
    exclude("internaltests/GizmoCompleterTests.java")
    exclude("internaltests/IncludeImportJarUri.java")
    exclude("internaltests/LFUCacheTest.java")
    exclude("internaltests/LicenseTests.java")
    exclude("internaltests/NumberingTests.java")
    exclude("internaltests/OrderedMapTests.java")
    exclude("internaltests/PriorityTests.java")
    exclude("internaltests/RedirectTests.java")
    exclude("internaltests/ResourceTests.java")
    exclude("internaltests/SignatureTest.java")
    exclude("internaltests/TinyTreeTest.java")
    exclude("internaltests/UnionIteratorTest.java")
    exclude("iss5701/Test5701.java")
    exclude("jaxptest/AllowedProtocolsTest.java")
    exclude("jaxptest/BinarySerializationTest.java")
    exclude("jaxptest/ConfigTest.java")
    exclude("jaxptest/DOMTest.java")
    exclude("jaxptest/DefaultAttributeProblemTest.java")
    exclude("jaxptest/DocumentBuilderImplTest.java")
    exclude("jaxptest/EventSourceTest.java")
    exclude("jaxptest/ExtensionTest.java")
    exclude("jaxptest/JaxpPlusXqjTest.java")
    exclude("jaxptest/MemoryTest.java")
    exclude("jaxptest/MultithreadedExtensionTest.java")
    exclude("jaxptest/OracleDOMTest.java")
    exclude("jaxptest/ParserTest.java")
    exclude("jaxptest/SAXTest.java")
    exclude("jaxptest/SchemaTests.java")
    exclude("jaxptest/SerializerTests.java")
    exclude("jaxptest/StylesheetCopyLineNumbersTest.java")
    exclude("jaxptest/TemplatesHandlerTest.java")
    exclude("jaxptest/TestIdentityTransformer.java")
    exclude("jaxptest/ThreadTest.java")
    exclude("jaxptest/TransformCallingQueryTest.java")
    exclude("jaxptest/TransformChainTest.java")
    exclude("jaxptest/TransformMessageTest.java")
    exclude("jaxptest/TransformTest.java")
    exclude("jaxptest/TransformWithValidation.java")
    exclude("jaxptest/TraxTest.java")
    exclude("jaxptest/URIResolverTest.java")
    exclude("jaxptest/ValidationTest.java")
    exclude("jaxptest/ValidatorHandlerTest.java")
    exclude("jstests/IxslExtensionTest.java")
    exclude("modeltests/AxiomTreeTests.java")
    exclude("modeltests/DOM4JTreeTests.java")
    exclude("modeltests/DOMTreeTests.java")
    exclude("modeltests/JDOM2TreeTests.java")
    exclude("modeltests/LinkedTreeTests.java")
    exclude("modeltests/ObjectModelTests.java")
    exclude("modeltests/TinyTreeTests.java")
    exclude("modeltests/XOMTreeTests.java")
    exclude("nativetests/ConfigDeadlockTest.java")
    exclude("nativetests/ConfigurationTest.java")
    exclude("nativetests/FnSchemaTest.java")
    exclude("nativetests/ImportFunctionTest.java")
    exclude("nativetests/ProjectionTests.java")
    exclude("nativetests/SubtypingTests.java")
    exclude("nativetests/TestXQueryDOM.java")
    exclude("nativetests/TestXQueryNative.java")
    exclude("nativetests/TestXQueryUpdate.java")
    exclude("nativetests/ThreadedKeyTest.java")
    exclude("nativetests/ValidationTest.java")
    exclude("net/sf/saxon/option/cpp/**")
    exclude("performance/ArrayPerformance.java")
    exclude("performance/Fernandes.java")
    exclude("performance/MapPerformance.java")
    exclude("performance/PerformanceTestCase.java")
    exclude("performance/Playground.java")
    exclude("performance/QueryGroupingPerformance.java")
    exclude("performance/SimonSoftTest.java")
    exclude("performance/SimonSoftTest2.java")
    exclude("performance/TreeModelPerformance.java")
    exclude("performance/TreeModelPerformanceDescendantAxis.java")
    exclude("performance/XPathPerformance.java")
    exclude("pulltests/PullTest.java")
    exclude("pulltests/StaxBridgeTest.java")
    exclude("regextests/testRegex.java")
    exclude("resolvers/CatalogResolverTest.java")
    exclude("resolvers/CommandTests.java")
    exclude("resolvers/ResolverConfigurationTest.java")
    exclude("resolvers/SchemaURIResolverTest.java")
    exclude("s9apitest/SumanGantaTest.java")
    exclude("s9apitest/TestErrorListener.java")
    exclude("s9apitest/TestFunctionLibrary.java")
    exclude("s9apitest/TestPackage.java")
    exclude("s9apitest/TestReflexion.java")
    exclude("s9apitest/TestSaxonParam.java")
    exclude("s9apitest/TestSchemaProcessing.java")
    exclude("s9apitest/TestSerializer.java")
    exclude("s9apitest/TestValidator.java")
    exclude("s9apitest/TestXMLParserOptions.java")
    exclude("s9apitest/TestXMLParserOptionsEE.java")
    exclude("s9apitest/TestXPathCompiler.java")
    exclude("s9apitest/TestXPathCompilerEE.java")
    exclude("s9apitest/TestXPathErrorCode.java")
    exclude("s9apitest/TestXQueryCompiler.java")
    exclude("s9apitest/TestXQueryCompilerEE.java")
    exclude("s9apitest/TestXQueryEvaluator.java")
    exclude("s9apitest/TestXQueryStreaming.java")
    exclude("s9apitest/TestXQueryThreads.java")
    exclude("s9apitest/TestXQueryToSaxDestination.java")
    exclude("s9apitest/TestXslt30TransformerDebug.java")
    exclude("s9apitest/TestXslt30TransformerDebugEE.java")
    exclude("s9apitest/TestXslt30TransformerEE.java")
    exclude("s9apitest/TestXsltCompiler.java")
    exclude("s9apitest/TestXsltCompilerEE.java")
    exclude("s9apitest/TestXsltTransformerEE.java")
    exclude("s9apitest/ThreadCountTest.java")
    exclude("s9apitest/XQueryStreamingTest.java")
    exclude("xqjtests/RennauBugs.java")
    exclude("xqjtests/RennauChainedQueries.java")
    exclude("xqjtests/TestSaxonDuration.java")
    exclude("xqjtests/TestTreeWalker.java")
    exclude("xqjtests/TestXQueryUsingXQJ.java")
    exclude("bugs/Bug6547.java")
  }
}

val signedReleaseArtifacts = findProperty("signedReleaseArtifacts") != "false"

val xqjDownloadJarBucket by configurations.dependencyScope("xqjDownloadJarBucket")
val xqjDownloadJar by configurations.resolvable("xqjDownloadJar") {
  extendsFrom(xqjDownloadJarBucket)
}
val xqjMavenJarBucket by configurations.dependencyScope("xqjMavenJarBucket")
val xqjMavenJar by configurations.resolvable("xqjMavenJar") {
  extendsFrom(xqjMavenJarBucket)
}
val xqjSourceMavenJarBucket by configurations.dependencyScope("xqjSourceMavenJarBucket")
val xqjSourceMavenJar by configurations.resolvable("xqjSourceMavenJar") {
  extendsFrom(xqjSourceMavenJarBucket)
}

val testsuiteExtra by configurations.resolvable("testsuiteExtraConfiguration") {
  extendsFrom(configurations["testImplementation"])
}

val samplesConfiguration by configurations.resolvable("samplesConfiguration") {
  extendsFrom(configurations["testImplementation"])
}

val sampleSourcesBucket by configurations.dependencyScope("sampleSourcesBucket")
val sampleSources = configurations.resolvable("sampleSources") {
  extendsFrom(sampleSourcesBucket)
}

val javaSampleSourcesBucket by configurations.dependencyScope("javaSampleSourcesBucket")
val javaSampleSources = configurations.resolvable("javaSampleSources") {
  extendsFrom(javaSampleSourcesBucket)
}

dependencies {
  sampleSourcesBucket(project(mapOf("path" to ":samples",
                                    "configuration" to "sampleSources")))
  javaSampleSourcesBucket(project(mapOf("path" to ":samples",
                                        "configuration" to "javaSampleSources")))
  xqjDownloadJarBucket(project(path = ":java", configuration = "xqjDownloadJarArtifact"))
  xqjMavenJarBucket(project(path = ":java", configuration = "xqjMavenJarArtifact"))
  xqjSourceMavenJarBucket(project(path = ":java", configuration = "xqjSourceMavenJarArtifact"))

  testImplementation(project(path = ":java:testdriver", configuration = "mavenHEJar"))
  testImplementation("org.xmlresolver:xmlresolver:${findProperty("xmlresolverVersion")}")
  testImplementation("org.xmlresolver:xmlresolver:${findProperty("xmlresolverVersion")}:data")
  testImplementation("javax.mail:javax.mail-api:${findProperty("mailVersion")}")
  testImplementation("com.sun.mail:javax.mail:${findProperty("mailVersion")}")
  testImplementation("nu.validator:htmlparser:${findProperty("htmlparserVersion")}")
}

val downloadDriver = configurations.dependencyScope("downloadDriver")
val mavenDriver = configurations.dependencyScope("mavenDriver")
dependencies {
  add(downloadDriver.name, project(path = ":java:testdriver", configuration = "downloadHEJar"))
  add(mavenDriver.name, project(path = ":java:testdriver", configuration = "mavenHEJar"))
  add(mavenDriver.name, project(path = ":java:testdriver", configuration = "mavenHESourcesJar"))
}

val downloadDriverFiles = configurations.resolvable("downloadDriverFiles") { extendsFrom(downloadDriver.get()) }
val mavenDriverFiles = configurations.resolvable("mavenDriverFiles") { extendsFrom(mavenDriver.get()) }
tasks.named<Sync>("signingDownloadJars") { from(downloadDriverFiles) }
tasks.named<Sync>("signingMavenJars") { from(mavenDriverFiles) }

tasks.named<Test>("editionTest") {
  workingDir(rootProject.file("java"))
  environment("SAXON_BUGS_DATA", rootProject.file("src/test/bugs").absolutePath)
  useJUnitPlatform { excludeTags("requires-pe", "requires-ee") }

  testLogging {
    events("failed")
    exceptionFormat = org.gradle.api.tasks.testing.logging.TestExceptionFormat.FULL
  }

  addTestListener(object : TestListener {
    override fun afterSuite(suite: TestDescriptor, result: TestResult) {
      if (suite.parent != null) return
      val counts = "${result.successfulTestCount} passed · ${result.failedTestCount} failed · ${result.skippedTestCount} skipped"
      logger.lifecycle("HE test results: $counts (${result.testCount} tests)")
      System.getenv("GITHUB_STEP_SUMMARY")?.let {
        File(it).appendText("### HE test results\n\n$counts (${result.testCount} tests)\n")
      }
      val executed = result.testCount > result.skippedTestCount
      System.getenv("GITHUB_OUTPUT")?.let { File(it).appendText("executed=$executed\n") }
    }
  })
}

tasks.named("compileJava") {
  onlyIf { false }
}

tasks.filteredSources {
  from(configurations.javaSources) {
    include("net/sf/saxon/**/*.java")
    include("net/sf/saxon/**/package.html")
    exclude("javax/xml/xquery/*.java")
    exclude("**/dotnet/**")
    exclude("net/sf/saxon/option/sql/**")
    exclude("net/sf/saxon/option/cpp/**")

    filter { if (it.contains("import com.saxonica")) "//" + it else it }
  }
}

tasks.preprocessJava {
  opts {
    off("EE", "PE", "STREAM", "SCHEMA", "OPT", "SAXONC", "BYTECODE", "CSHARP")
  }
}

val jarJars = listOf("lib/xmlresolver-${findProperty("xmlresolverVersion")}.jar",
  "lib/xmlresolver.jar",
  "lib/xmlresolver-${findProperty("xmlresolverVersion")}-data.jar",
  "lib/xmlresolver-data.jar",
  "lib/jline-${findProperty("jlineVersion")}.jar",
  "lib/jline.jar")

tasks.named<Jar>("editionDownloadJar") {
  exclude("net/sf/saxon/option")
}
tasks.named<Jar>("editionMavenJar") {
  exclude("net/sf/saxon/option")
}

tasks.register<Sync>("stageHeRuntime") {
  description = "Stage the runnable HE JAR with its runtime dependencies and notices"
  into(layout.buildDirectory.dir("he-runtime"))
  from(tasks.named<Jar>("editionDownloadJar").flatMap { it.archiveFile })
  into("lib") { from(configurations.named("downloadReleaseBundled")) }
  into("notices") { from(rootProject.file("src/main/notices")) }
}

tasks.register<Zip>("source-code-release") {
  dependsOn("compileEditionTestJava")
  archiveBaseName = "saxon${saxonBuild.publishVersion.get().replace('.', '-')}source"
  destinationDirectory = layout.buildDirectory.dir("source-code-release")
  from(layout.buildDirectory.dir("src/main/java"))
  from(layout.buildDirectory.dir("src/main/resources"))
}


val copySampleSources = tasks.register<Copy>("copy_java_sample_sources") {
    into(layout.buildDirectory.dir("samples/src/java/main"))
    from(javaSampleSources) {
      exclude("ee/**")
    }
}

val copySampleResources = tasks.register<Copy>("copy_sample_sources") {
    into(layout.buildDirectory.dir("samples/resources"))
    from(sampleSources)
}

val copyConfigHE = tasks.register<Copy>("copy_config_HE") {
  dependsOn(copySampleResources)
  into(layout.buildDirectory.dir("samples/resources/config"))
  from(layout.projectDirectory.file("../../samples/src/config/config-HE.xml"))
}

val sampleStaging = tasks.register<Copy>("samples_staging") {
  dependsOn(tasks.named("release"))

  val filename = "SaxonHE${saxonBuild.publishVersion.get().replace(".","-")}J.zip"

  doFirst {
    mkdir(layout.buildDirectory.file("sample-staging"))
  }
  from(zipTree(layout.buildDirectory.file("releases/${filename}")))
  into(layout.buildDirectory.file("sample-staging"))
}

val run_classes = tasks.register<JavaCompile>("compile_sample_run_classes") {
  dependsOn(copySampleSources)
  dependsOn(copySampleResources)
  dependsOn(copyConfigHE)
  classpath = (files(sourceSets["edition"].output)
               + configurations.named("samplesConfiguration").get())
  source = project.fileTree(layout.buildDirectory.dir("samples/src/java/main").get())
  destinationDirectory = layout.buildDirectory.dir("samples/run/classes").get()
}

tasks.register<Exec>("samples") {
  dependsOn(run_classes)
  dependsOn(sampleStaging)
  commandLine = listOf("python", "../../tools/run-samples.py",
                       "--edition", "he",
                       "--platform", "java")
}

// ============================================================

val testSuitesClasspath = files(tasks.named<Jar>("editionDownloadJar").flatMap { it.archiveFile })
    .from(configurations.named("downloadReleaseBundled"))
    .from(testsuiteExtra)
testSuites {
  product = "java"
  edition = "he"
  testclasspath = testSuitesClasspath
  submodulesDir = layout.projectDirectory.dir("../../submodules")
  exceptionsDir = layout.projectDirectory.dir("../src/test/exceptions")
}

testSuites.configure("qt3", "XP20")
testSuites.configure("qt3", "XP31")
testSuites.configure("qt3", "XQ31")

testSuites.configure("xslt30", "XT30")
testSuites.configure("xslt30", "XT30", mapOf("sef-roundtrip" to ""), taskName = "xslt30_XT30_sef")
tasks.named<JavaRunner>("test_suite_xslt30_XT30_sef") {
  reportingRootDir = layout.buildDirectory.dir("test-results-sef")
}

testSuites.configure("xslt30extra", "XT30")

tasks.register("aggregated-test-results") {
  inputs.files(tasks.named("test_suite_qt3_XP31"))
  inputs.files(tasks.named("test_suite_qt3_XQ31"))
  inputs.files(tasks.named("test_suite_xslt30_XT30"))
  inputs.files(tasks.named("test_suite_xslt30extra_XT30"))

  doLast {
    println(inputs.files.files)
  }
}

tasks.named<Sync>("signingDownloadJars") {
  into(layout.buildDirectory.dir("signing/download"))
  from(xqjDownloadJar)
}

val pseudoXqjMavenJar = tasks.register("pseudoXqjMavenJar") {
  inputs.file(xqjMavenJar)
  outputs.file(xqjMavenJar)
}

val pseudoXqjSourceMavenJar = tasks.register("pseudoXqjSourceMavenJar") {
  inputs.file(xqjSourceMavenJar)
  outputs.file(xqjSourceMavenJar)
}


tasks.named<Sync>("signingMavenJars") {
  into(layout.buildDirectory.dir("signing/mavendownload"))
  from(pseudoXqjMavenJar)
  from(pseudoXqjSourceMavenJar)
}

val signTask = tasks.named("downloadSignedMavenJars")
val mavenArtifactId = "Saxon-HE"
val mavenVersion = saxonBuild.publishVersion.get()
val signRootName = "signed-maven/${mavenArtifactId}-${mavenVersion}"

val mavenPom = PomGenerator() {
  name = mavenArtifactId
  license {
    name = "Mozilla Public License Version 2.0"
    url = "http://www.mozilla.org/MPL/2.0/"
    distribution = "repo"
  }
  scm  {
    url = "https://saxonica.plan.io/projects/saxonmirrorhe/repository"
    connection = "scm:git:https://saxonica.plan.io/projects/saxonmirrorhe/repository"
    developerConnection = "scm:git:https://saxonica.plan.io/projects/saxonmirrorhe/repository"
  }
  dependencies = configurations.editionImplementation.get().allDependencies
}

val mavenOMaticDir = layout.buildDirectory.dir("maven-o-matic")

publishing {
  publications {
    create<MavenPublication>("maven") {
      groupId = "net.sf.saxon"
      artifactId = mavenArtifactId
      version = mavenVersion

      pom(mavenPom.generate())

      artifact(signTask.map { layout.buildDirectory.file("${signRootName}.jar") })
      artifact(signTask.map { layout.buildDirectory.file("${signRootName}-javadoc.jar") }) {
        classifier = "javadoc"
      }
      artifact(signTask.map { layout.buildDirectory.file("${signRootName}-sources.jar") }) {
        classifier = "sources"
      }
      artifact(signTask.map { layout.buildDirectory.file("${signRootName}-xqj.jar") }) {
        classifier = "xqj"
      }
      artifact(signTask.map { layout.buildDirectory.file("${signRootName}-xqj-sources.jar") }) {
        classifier = "xqj-sources"
      }
      artifact(signTask.map { layout.buildDirectory.file("${signRootName}-test.jar") }) {
        classifier = "test"
      }
      artifact(signTask.map { layout.buildDirectory.file("${signRootName}-test-sources.jar") }) {
        classifier = "test-sources"
      }
    }
  }
  repositories {
    maven {
      name = "maven-o-matic"
      url = uri(mavenOMaticDir.get())
    }
  }
}

signing {
  sign(publishing.publications)
}

val heMavenPubTask = tasks.named("publishMavenPublicationToMaven-o-maticRepository")

tasks.register<Zip>("sonatype_bundle") {
  dependsOn(heMavenPubTask)
  archiveFileName = "he_sonatype_bundle.zip"
  from(mavenOMaticDir) {
    exclude("com")
  }
}
