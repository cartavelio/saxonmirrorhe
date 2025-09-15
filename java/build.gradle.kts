import com.saxonica.build.java.PomGenerator
import com.saxonica.build.GatherReleaseArtifacts

plugins {
  id("saxon-build")
  id("com.saxonica.build.testsuites")
  id("com.saxonica.build.samples")
  id("saxonj")
  id("maven-publish")
  id("signing")
}

saxonj {
  edition = "ee"
  metaInfServices = mapOf(
    "javax.xml.transform.TransformerFactory" to "com.saxonica.config.EnterpriseTransformerFactory",
    "javax.xml.validation.SchemaFactory" to "com.saxonica.ee.jaxp.SchemaFactoryImpl"
  )
  manifestMainClass = "net.sf.saxon.Transform"
  sourceRoot = layout.projectDirectory.dir("src")
  testDataDir = layout.projectDirectory.dir("../src/test/testdata")
  jarManifestClasspath = jarClasspath("lib/icu4j-${findProperty("icu4jVersion")}.jar", "lib/icu4j.jar")
  editionJarExclusions = listOf("**/sql", "com/saxonica/StatsTransform*.class")
}

val saxonBuild = the<SaxonBuildExtension>()

val signedReleaseArtifacts = findProperty("signedReleaseArtifacts") != "false"

sourceSets.named("editionTest") {
  java {
    // TODO: this really needs to be per-edition.
    // For now, exclude everything that won't build in HE. :-(
//    exclude("xqjtck/**")
//    exclude("com/saxonica/functions/extfn/LastModifiedTest.java")
//    exclude("commands/AntTests.java")
//    exclude("commands/QueryTests.java")
//    exclude("commands/Saxon3483.java")
//    exclude("commands/TransformTests.java")
//    exclude("commands/ValidateTests.java")
//    exclude("failing/AntTests.java")
//    exclude("failing/FailingTests.java")
//    exclude("internaltests/ActivateSaxon.java")
//    exclude("internaltests/AlphaCodeTypeTest.java")
//    exclude("internaltests/GizmoCommandTests.java")
//    exclude("internaltests/GizmoCompleterTests.java")
//    exclude("internaltests/IncludeImportJarUri.java")
//    exclude("internaltests/LFUCacheTest.java")
//    exclude("internaltests/LicenseTests.java")
//    exclude("internaltests/NumberingTests.java")
//    exclude("internaltests/RedirectTests.java")
//    exclude("internaltests/ResourceTests.java")
//    exclude("internaltests/SignatureTest.java")
//    exclude("internaltests/TinyTreeTest.java")
//    exclude("internaltests/UnionIteratorTest.java")
//    exclude("iss5701/Test5701.java")
//    exclude("jaxptest/AllowedProtocolsTest.java")
//    exclude("jaxptest/BinarySerializationTest.java")
//    exclude("jaxptest/ConfigTest.java")
//    exclude("jaxptest/DOMTest.java")
//    exclude("jaxptest/DefaultAttributeProblemTest.java")
//    exclude("jaxptest/DocumentBuilderImplTest.java")
//    exclude("jaxptest/EventSourceTest.java")
//    exclude("jaxptest/ExtensionTest.java")
//    exclude("jaxptest/JaxpPlusXqjTest.java")
//    exclude("jaxptest/MemoryTest.java")
//    exclude("jaxptest/MultithreadedExtensionTest.java")
//    exclude("jaxptest/OracleDOMTest.java")
//    exclude("jaxptest/ParserTest.java")
//    exclude("jaxptest/SAXTest.java")
//    exclude("jaxptest/SchemaTests.java")
//    exclude("jaxptest/SerializerTests.java")
//    exclude("jaxptest/StylesheetCopyLineNumbersTest.java")
//    exclude("jaxptest/TemplatesHandlerTest.java")
//    exclude("jaxptest/TestIdentityTransformer.java")
//    exclude("jaxptest/ThreadTest.java")
//    exclude("jaxptest/TransformCallingQueryTest.java")
//    exclude("jaxptest/TransformChainTest.java")
//    exclude("jaxptest/TransformMessageTest.java")
//    exclude("jaxptest/TransformTest.java")
//    exclude("jaxptest/TransformWithValidation.java")
//    exclude("jaxptest/TraxTest.java")
//    exclude("jaxptest/URIResolverTest.java")
//    exclude("jaxptest/ValidationTest.java")
//    exclude("jaxptest/ValidatorHandlerTest.java")
//    exclude("jstests/IxslExtensionTest.java")
//    exclude("modeltests/AxiomTreeTests.java")
//    exclude("modeltests/DOM4JTreeTests.java")
//    exclude("modeltests/DOMTreeTests.java")
//    exclude("modeltests/JDOM2TreeTests.java")
//    exclude("modeltests/LinkedTreeTests.java")
//    exclude("modeltests/ObjectModelTests.java")
//    exclude("modeltests/TinyTreeTests.java")
//    exclude("modeltests/XOMTreeTests.java")
//    exclude("nativetests/ConfigDeadlockTest.java")
//    exclude("nativetests/ConfigurationTest.java")
//    exclude("nativetests/FnSchemaTest.java")
//    exclude("nativetests/ImportFunctionTest.java")
//    exclude("nativetests/ProjectionTests.java")
//    exclude("nativetests/SubtypingTests.java")
//    exclude("nativetests/TestXQueryDOM.java")
//    exclude("nativetests/TestXQueryNative.java")
//    exclude("nativetests/TestXQueryUpdate.java")
//    exclude("nativetests/ThreadedKeyTest.java")
//    exclude("nativetests/ValidationTest.java")
    exclude("net/sf/saxon/option/cpp/**")
//    exclude("performance/ArrayPerformance.java")
//    exclude("performance/Fernandes.java")
//    exclude("performance/PerformanceTestCase.java")
//    exclude("performance/Playground.java")
//    exclude("performance/QueryGroupingPerformance.java")
//    exclude("performance/SimonSoftTest.java")
//    exclude("performance/SimonSoftTest2.java")
//    exclude("performance/TreeModelPerformance.java")
//    exclude("performance/TreeModelPerformanceDescendantAxis.java")
//    exclude("performance/XPathPerformance.java")
//    exclude("pulltests/PullTest.java")
//    exclude("pulltests/StaxBridgeTest.java")
//    exclude("regextests/testRegex.java")
//    exclude("resolvers/CatalogResolverTest.java")
//    exclude("resolvers/CommandTests.java")
//    exclude("resolvers/ResolverConfigurationTest.java")
//    exclude("resolvers/SchemaURIResolverTest.java")
//    exclude("s9apitest/SumanGantaTest.java")
//    exclude("s9apitest/TestCanonicalSerializer.java")
//    exclude("s9apitest/TestChecksums.java")
//    exclude("s9apitest/TestCollections.java")
//    exclude("s9apitest/TestDocumentBuilder.java")
//    exclude("s9apitest/TestErrorListener.java")
//    exclude("s9apitest/TestFunctionLibrary.java")
//    exclude("s9apitest/TestIntegratedFunctions.java")
//    exclude("s9apitest/TestModuleURIResolver.java")
//    exclude("s9apitest/TestPackage.java")
//    exclude("s9apitest/TestPipeline.java")
//    exclude("s9apitest/TestQueryToTransform.java")
//    exclude("s9apitest/TestReflexion.java")
//    exclude("s9apitest/TestSaxonParam.java")
//    exclude("s9apitest/TestSchemaProcessing.java")
//    exclude("s9apitest/TestSchematron.java")
//    exclude("s9apitest/TestSerializer.java")
//    exclude("s9apitest/TestValidator.java")
//    exclude("s9apitest/TestXMLParserOptions.java")
    exclude("s9apitest/TestXMarkProjection.java")
//    exclude("s9apitest/TestXPathCompiler.java")
//    exclude("s9apitest/TestXPathErrorCode.java")
//    exclude("s9apitest/TestXQueryCompiler.java")
//    exclude("s9apitest/TestXQueryEvaluator.java")
//    exclude("s9apitest/TestXQueryStreaming.java")
//    exclude("s9apitest/TestXQueryThreads.java")
//    exclude("s9apitest/TestXQueryToSaxDestination.java")
//    exclude("s9apitest/TestXQueryUpdate.java")
//    exclude("s9apitest/TestXdmFunction.java")
//    exclude("s9apitest/TestXslt30Streaming.java")
//    exclude("s9apitest/TestXslt30Transformer.java")
//    exclude("s9apitest/TestXslt30TransformerDebug.java")
//    exclude("s9apitest/TestXsltCompiler.java")
//    exclude("s9apitest/TestXsltIn10Mode.java")
//    exclude("s9apitest/TestXsltTracing.java")
//    exclude("s9apitest/TestXsltTransformer.java")
//    exclude("s9apitest/ThreadCountTest.java")
//    exclude("s9apitest/XQueryStreamingTest.java")
//    exclude("xqjtests/RennauBugs.java")
//    exclude("xqjtests/RennauChainedQueries.java")
//    exclude("xqjtests/TestSaxonDuration.java")
//    exclude("xqjtests/TestTreeWalker.java")
//    exclude("xqjtests/TestXQueryUsingXQJ.java")
  }
}

val downloadReleaseBundled by configurations.getting
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

val heReleaseZipsBucket by configurations.dependencyScope("heReleaseZipsBucket")
val heReleaseZips by configurations.resolvable("heReleaseZips") {
  extendsFrom(heReleaseZipsBucket)
}

val heReleaseMavenBucket by configurations.dependencyScope("heReleaseMavenBucket")
val heReleaseMaven by configurations.resolvable("heReleaseMaven") {
  extendsFrom(heReleaseMavenBucket)
}

val peReleaseZipsBucket by configurations.dependencyScope("peReleaseZipsBucket")
val peReleaseZips by configurations.resolvable("peReleaseZips") {
  extendsFrom(peReleaseZipsBucket)
}

val peReleaseMavenBucket by configurations.dependencyScope("peReleaseMavenBucket")
val peReleaseMaven by configurations.resolvable("peReleaseMaven") {
  extendsFrom(peReleaseMavenBucket)
}

val eeReleaseZipsBucket by configurations.dependencyScope("eeReleaseZipsBucket")
val eeReleaseZips by configurations.resolvable("eeReleaseZips") {
  extendsFrom(eeReleaseZipsBucket)
}

val eeReleaseMavenBucket by configurations.dependencyScope("eeReleaseMavenBucket")
val eeReleaseMaven by configurations.resolvable("eeReleaseMaven") {
  extendsFrom(eeReleaseMavenBucket)
}

dependencies {
  heReleaseZipsBucket(project(path = ":java:he", configuration = "releaseZips"))
  heReleaseMavenBucket(project(path = ":java:he", configuration = "releaseMaven"))
  peReleaseZipsBucket(project(path = ":java:pe", configuration = "releaseZips"))
  peReleaseMavenBucket(project(path = ":java:pe", configuration = "releaseMaven"))
  eeReleaseZipsBucket(project(path = ":java", configuration = "releaseZips"))
  eeReleaseMavenBucket(project(path = ":java", configuration = "releaseMaven"))

  sampleSourcesBucket(project(mapOf("path" to ":samples",
                                    "configuration" to "sampleSources")))
  javaSampleSourcesBucket(project(mapOf("path" to ":samples",
                                        "configuration" to "javaSampleSources")))

  testDrivers(files(layout.projectDirectory.dir("../lib/Tidy.jar")))

  testImplementation("org.xmlresolver:xmlresolver:${findProperty("xmlresolverVersion")}")
  testImplementation("org.xmlresolver:xmlresolver:${findProperty("xmlresolverVersion")}:data")
  testImplementation("javax.mail:javax.mail-api:${findProperty("mailVersion")}")
  testImplementation("com.sun.mail:javax.mail:${findProperty("mailVersion")}")
  testImplementation("nu.validator:htmlparser:${findProperty("htmlparserVersion")}")

  downloadReleaseBundled("com.ibm.icu:icu4j:${findProperty("icu4jVersion")}")
}

tasks.filteredSources {
  from(configurations.javaSources) {
    include("net/sf/saxon/**/*.java")
    include("javax/xml/xquery/*.java")
    exclude("**/dotnet/**")
    include("net/sf/saxon/**/package.html")
    exclude("net/sf/saxon/testdriver/**")
    exclude("net/sf/saxon/option/cpp/**")
    exclude("com/saxonica/functions/extfn/cpp/*.java")
    include("com/saxonica/**/*.java")
    exclude("com/saxonica/testdriver/**/*.java")
    exclude("com/saxonica/config/DotNetExtensionFunctionFactory.java")
    exclude("com/saxonica/config/DotNetExtensionLibrary.java")
    exclude("com/saxonica/config/DotNetPlatformPE.java")
    exclude("com/saxonica/config/NativePlatformPE.java")
    exclude("com/saxonica/expr/DotNetExtensionFunctionCall.java")
    exclude("com/saxonica/functions/extfn/NativeCall.java")
    include("com/saxonica/**/package.html")
    exclude("com/saxonica/js/**")

    filter { line ->
      if (line.contains("org.objectweb"))
        line.replace("org.objectweb", "com.saxonica.objectweb").trimEnd()
      else
        line.trimEnd()
    }
  }
}

tasks.preprocessJava {
  opts {
    on("EE", "STREAM", "SCHEMA", "PACKAGES", "BYTECODE", "HOF")
    off("PE", "SAXONC", "CSHARP")
  }
}

val compileDrivers = tasks.compileTestDrivers {
  include("com/saxonica/testdriver/**/*.java")
}

val testDriverOutput = tasks.register<Copy>("_testDriverOutput") {
  dependsOn("compileTestDrivers")
  outputs.dir(layout.buildDirectory.dir("XXX"))
  from(tasks.named("compileTestDrivers").get().outputs)
  into(layout.buildDirectory.dir("XXX"))
}

val testDriversArtifacts by configurations.consumable("testDriversArtifacts")
testDriversArtifacts.outgoing.artifact(testDriverOutput) { builtBy(testDriverOutput) }

val editionCompileJava = tasks.named("compileEditionJava")

val jarTaskBuilder = saxonj.jarTaskBuilder(project)

val (downloadSqlJar, mavenSqlJar) = jarTaskBuilder("sql") {
  downloadJarBaseName = "saxon-sql"
  jarClassifier = "sql"
  common {
    from(editionCompileJava)
    include("net/sf/saxon/option/sql/*.class")
    include("com/saxonica/functions/sql/*.class")
  }
}

val (_, mavenSqlSourceJar) = jarTaskBuilder("sqlSource") {
  jarClassifier = "sql-sources"
  common {
    from(tasks.preprocessJava) {
      include("net/sf/saxon/option/sql/*.java")
      include("com/saxonica/functions/sql/*.java")
    }
  }
}

val xqjDownloadJarArtifact by configurations.consumable("xqjDownloadJarArtifact")
val xqjMavenJarArtifact by configurations.consumable("xqjMavenJarArtifact")

val (downloadXqjJar, mavenXqjJar) = jarTaskBuilder("xqj") {
  edition = "he"
  jarClassifier = "xqj"
  common {
    from(editionCompileJava)
    include("com/saxonica/xqj/**/*.class")
    include("javax/xml/xquery/**/*.class")
  }
}

val (_, mavenXqjSourceJar) = jarTaskBuilder("xqjSource") {
  edition = "he"
  jarClassifier = "xqj-sources"
  common {
    from(tasks.preprocessJava) {
      include("com/saxonica/xqj/**/*.java")
      include("javax/xml/xquery/**/*.java")
    }
  }
}
val xqjSourceMavenJarArtifact by configurations.consumable("xqjSourceMavenJarArtifact")

xqjDownloadJarArtifact.outgoing.artifact(downloadXqjJar)
xqjMavenJarArtifact.outgoing.artifact(mavenXqjJar) { builtBy(mavenXqjJar) }
xqjSourceMavenJarArtifact.outgoing.artifact(mavenXqjSourceJar) { builtBy(mavenXqjSourceJar) }

tasks.register<GatherReleaseArtifacts>("gather_release_artifacts") {
  artifacts.from(heReleaseZips)
  artifacts.from(heReleaseMaven)
  artifacts.from(peReleaseZips)
  artifacts.from(peReleaseMaven)
  artifacts.from(eeReleaseZips)
  artifacts.from(eeReleaseMaven)
}

// In principle, this should be in the samples-plugin...
tasks.register<Copy>("copy_java_sample_sources") {
    into(layout.buildDirectory.dir("samples/src/java/main"))
    from(javaSampleSources)
}

val jar_sample_run_classes = tasks.register<Jar>("jar_sample_run_classes") {
  dependsOn("compile_sample_run_classes")
  archiveBaseName = "classes"
  destinationDirectory = layout.buildDirectory.dir("tmp")
  into(archiveBaseName)
  from(tasks.named("compile_sample_run_classes")) {
    exclude("previous-compilation-data.bin")
  }
}

val javaSampleClasses by configurations.consumable("javaSampleClasses")
javaSampleClasses.outgoing.artifact(jar_sample_run_classes)

samples {
  runclasspath = (files(sourceSets["edition"].output)
                  + configurations.named("samplesConfiguration").get())
  debugclasspath = (files(layout.buildDirectory.dir("classes/java/main"))
                    + files(layout.buildDirectory.dir("build/resources/main"))
                    + configurations.named("samplesConfiguration").get())

  sources = sampleSources
  product = "java"
  edition = "ee"
}

samples.transform("Books_Transform_1", "books.xml", "books.xsl", listOf())
samples.transform("Books_Transform_2", "books.xml", "books.xsl", 
                  listOf("top-author=Thomas Hardy"))

samples.transform("Books_Transform_3", "books.xml", "books-csv.xsl")

samples.query("Books_Query_1", "books.xml", "books.xq")

samples.queryString("Books_Query_2", "books.xml", "saxon:line-number((//*)[last()])", listOf("-l:on"))

samples.validate("Books_Validate_1", "books.xml", "books.xsd")

samples.validate("Books_Validate_2", "books-invalid.xml", "books.xsd", listOf("-quit:off"))

samples.transform("Tour_Transform_1", null, "tour.xsl", listOf("-it:main"))
samples.transform("Tour_Transform_2", null, "tour.xsl", listOf("-it:main", "start=a6"))
samples.transform("Tour_Transform_3", null, "tour.xsl", listOf("-it:main", "start=g2"))

samples.query("Tour_Query_1", null, "tour.xq", listOf("start=e5"))
samples.query("Tour_Query_2", null, "tour.xq", listOf("start=d2", "end=42"))
samples.query("Tour_Query_3", null, "tour.xq", listOf("end=61"))

samples.transform("Shakespeare_1", "othello.xml", "play.xsl",
                  listOf("dir=file:///${layout.buildDirectory.file("sample_results/playhtml").get()}"),
                  layout.buildDirectory.file("sample_results/playhtml/play.html").get().asFile)

samples.transform("Bible_1", "../bible/nt/nt.xml", "bible.xsl",
                  listOf("dir=${layout.buildDirectory.file("sample_results/biblehtml").get()}"),
                  layout.buildDirectory.file("sample_results/biblehtml/index.html").get().asFile)

// ============================================================

val run_with_config = tasks.register<JavaExec>("run_with_config") {
  doFirst {
    mkdir(layout.buildDirectory.file("sample_results"))
  }

  classpath = (files(sourceSets["edition"].output)
               + configurations.named("samplesConfiguration").get())
  mainClass = "net.sf.saxon.Transform"
  args("-config:${layout.projectDirectory.file("../samples/src/config/config.xml")}",
       "-s:${layout.projectDirectory.file("../samples/src/data/books.xml")}",
       "-xsl:${layout.projectDirectory.file("../samples/src/styles/books.xsl")}",
       "-o:${layout.buildDirectory.file("sample_results/books_with_config.html").get()}",
       "--traceListenerOutputFile:${layout.buildDirectory.file("sample_results/traceOutput_config.log").get()}")
}
tasks.named("all_samples").get().dependsOn(run_with_config)

// ============================================================


samples.sampleClass("JAXPExamples", listOf("all"), "he.JAXPExamples")
samples.sampleClass("ApplyXPathJAXP_saxon",
                    listOf("data/books.xml", "/BOOKLIST/BOOKS/ITEM[@CAT='MMP']/TITLE", "saxon"),
                    "he.ApplyXPathJAXP")
samples.sampleClass("ApplyXPathJAXP_dom",
                    listOf("data/books.xml", "/BOOKLIST/BOOKS/ITEM[@CAT='MMP']/TITLE", "dom"),
                    "he.ApplyXPathJAXP")
samples.sampleClass("ExampleContentHandler", listOf("data/books.xml"), "he.ExampleContentHandler")
samples.sampleClass("S9APIExamplesOM_ee", listOf("all"), "he.S9APIExamplesOM")
samples.sampleClass("S9APIExamples_ee", listOf("all"), "he.S9APIExamples")
samples.sampleClass("JDOM2Example", listOf("data/books.xml", "styles/books.xsl"))
samples.sampleClass("PullExamples", listOf())
samples.sampleClass("QuickValidator", listOf("data/books.xml", "data/books.xsd"))

samples.sampleClass("SchemaValidatorExample_1", listOf("data/books.xsd", "data/books.xml"),
                    "ee.SchemaValidatorExample")
samples.sampleClass("SchemaValidatorExample_2", listOf("--", "data/books.xml"),
                    "ee.SchemaValidatorExample")
samples.sampleClass("SchemaValidatorHandlerExample_1", listOf("data/books.xsd", "data/books.xml"),
                    "ee.SchemaValidatorHandlerExample")
samples.sampleClass("SchemaValidatorHandlerExample_2", listOf("--", "data/books.xml"),
                    "ee.SchemaValidatorHandlerExample")
samples.sampleClass("XPathExampleSA", listOf("data/books.xml", "data/books.xsd"))
samples.sampleClass("XQJExamples", listOf("all"))

/*
// The XMarkBenchmark runs slowly, so only add it on request
samples.sampleClass("XMarkBenchmark_1":
if (project.findProperty("xmark") == "true") {
  eeSampleConfigurations += ["XMarkBenchmark_1"]
}
// FIXME: Automate testing of SaxonSAServlet
// Only necessary if the configuration key is not the class name
                         "XMarkBenchmark_1": "XMarkBenchmark"]
                    "XMarkBenchmark_1": ["-dir:${buildDir}/tests/extra/data/xmark/"],
*/


// ============================================================


val compile_testdrivers = tasks.named<JavaCompile>("compileTestDrivers") {
  include("com/saxonica/testdriver/**")
  include("com/saxonica/testdriver/ee/**")
}
val testSuitesClasspath = files(sourceSets["edition"].output)
    .from(configurations.testDrivers)
    .from(compile_testdrivers)
testSuites {
  product = "java"
  edition = "ee"
  testclasspath = testSuitesClasspath
  submodulesDir = layout.projectDirectory.dir("../submodules")
  exceptionsDir = layout.projectDirectory.dir("src/test/exceptions")
}

testSuites.configure("expath", "XP31")
testSuites.configure("expath", "XP31", mapOf("unfolded" to "on"))
testSuites.configure("expath", "XQ31")
testSuites.configure("expath", "XQ40")

testSuites.configure("qt3", "XP31")
testSuites.configure("qt3", "XP31", mapOf("unfolded" to "on"))
testSuites.configure("qt3", "XQ31")
testSuites.configure("qt3", "XQ31", mapOf("unfolded" to "on"))

testSuites.configure("qt3extra", "XP31")
testSuites.configure("qt3extra", "XQ31")
testSuites.configure("qt3extra", "XQ40")

testSuites.configure("qt4", "XP40")
testSuites.configure("qt4", "XQ31")
testSuites.configure("qt4", "XQ31", mapOf("unfolded" to "on"))
testSuites.configure("qt4", "XQ40")

testSuites.configure("xslt30", "XT30")
testSuites.configure("xslt30", "XT30", mapOf("export" to "on"))
testSuites.configure("xslt30", "XT30", mapOf("strict" to "on"))

testSuites.configure("xslt30extra", "XT30")
testSuites.configure("xslt30extra", "XT30", mapOf("export" to "on"))
testSuites.configure("xslt30extra", "XT30", mapOf("strict" to "on"))
testSuites.configure("xslt30extra", "XT40")
testSuites.configure("xslt30extra", "XT40", mapOf("export" to "on"))
testSuites.configure("xslt30extra", "XT40", mapOf("strict" to "on"))

testSuites.configure("xslt40", "XT40")
testSuites.configure("xslt40", "XT40", mapOf("strict" to "on"))

testSuites.configure("xsd", "XSD11")
testSuites.configure("xsdextra", "XSD11")

tasks.named<Sync>("signingDownloadJars") {
  into(layout.buildDirectory.dir("signing/download"))
  from(downloadSqlJar)
}

tasks.named<Sync>("signingMavenJars") {
  into(layout.buildDirectory.dir("signing/mavendownload"))
  from(mavenSqlJar)
  from(mavenSqlSourceJar)
}

val downloadRelease = tasks.named<Zip>("downloadRelease") {
  if (signedReleaseArtifacts) {
    val name = "signed-download/saxon"
    val vers = saxonBuild.publishVersion.get()
    from(tasks.named("downloadSignedDownloadJars").map {layout.buildDirectory.file("${name}-sql-${vers}.jar") })
  } else {
    from(downloadSqlJar)
  }
  into("notices") {
    from(layout.projectDirectory.dir("../src/main/notices"))
    include("ICU-J.txt")
  }
}

tasks.named("allJars") {
  finalizedBy("sqlDownloadJar")
}

val signTask = tasks.named("downloadSignedMavenJars")
val mavenArtifactId = "Saxon-EE"
val mavenVersion = saxonBuild.publishVersion.get()
val signRootName = "signed-maven/${mavenArtifactId}-${mavenVersion}"

val mavenPom = PomGenerator() {
  name = mavenArtifactId
  license(PomGenerator.COMMERCIAL_LICENSE)
  dependencies = configurations.editionImplementation.get().allDependencies
}

publishing {
  publications {
    create<MavenPublication>("maven") {
      groupId = "com.saxonica"
      artifactId = mavenArtifactId
      version = mavenVersion

      pom(mavenPom.generate())

      artifact(signTask.map { layout.buildDirectory.file("${signRootName}.jar") })
      artifact(signTask.map { layout.buildDirectory.file("${signRootName}-javadoc.jar") }) {
        classifier = "javadoc"
      }
      artifact(signTask.map { layout.buildDirectory.file("${signRootName}-sql.jar") }) {
        classifier = "sql"
      }
      artifact(signTask.map { layout.buildDirectory.file("${signRootName}-sql-sources.jar") }) {
        classifier = "sql-sources"
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
      url = uri(layout.buildDirectory.dir("maven-o-matic").get())
    }
  }
}

signing {
  sign(publishing.publications)
}
