import com.saxonica.build.java.PomGenerator

plugins {
  id("saxon-build")
  id("com.saxonica.build.testsuites")
  id("com.saxonica.build.samples")
  id("saxonj")
  id("maven-publish")
  id("signing")
}

saxonj {
  edition = "pe"
  metaInfServices = mapOf(
    "javax.xml.transform.TransformerFactory" to "com.saxonica.config.ProfessionalTransformerFactory"
  )
  manifestMainClass = "net.sf.saxon.Transform"
  sourceRoot = layout.projectDirectory.dir("../src")
  testDataDir = layout.projectDirectory.dir("../../src/test/testdata")
  editionJarExclusions = listOf(
    "**/sql",
    "com/saxonica/CompileStylesheet.class",
    "com/saxonica/CompileStylesheet\$TracingObjectOutputStream.class",
    "com/saxonica/Validate.class",
    "com/saxonica/config/EnterpriseConfiguration.class",
    "com/saxonica/config/EnterpriseConfiguration\$SurrogateSchema.class",
    "com/saxonica/config/EnterpriseTransformerFactory.class",
    "com/saxonica/config/EnterpriseXPathFactory.class",
    "com/saxonica/config/StreamingTransformerFactory.class",
    "com/saxonica/ee",
    "com/saxonica/expr/CastToList.class",
    "com/saxonica/expr/CastToUnion.class",
    "com/saxonica/expr/CastableToList.class",
    "com/saxonica/expr/CastableToUnion.class"
  )
}

val saxonBuild = the<SaxonBuildExtension>()

val signedReleaseArtifacts = findProperty("signedReleaseArtifacts") != "false"

sourceSets.named("editionTest") {
  java {
    // TODO Exclude everything that won't build in PE...
    exclude("xqjtck/**")
    exclude("com/saxonica/functions/extfn/LastModifiedTest.java")
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
    exclude("s9apitest/TestCanonicalSerializer.java")
    exclude("s9apitest/TestChecksums.java")
    exclude("s9apitest/TestCollections.java")
    exclude("s9apitest/TestDocumentBuilder.java")
    exclude("s9apitest/TestErrorListener.java")
    exclude("s9apitest/TestFunctionLibrary.java")
    exclude("s9apitest/TestIntegratedFunctions.java")
    exclude("s9apitest/TestModuleURIResolver.java")
    exclude("s9apitest/TestPackage.java")
    exclude("s9apitest/TestPipeline.java")
    exclude("s9apitest/TestQueryToTransform.java")
    exclude("s9apitest/TestReflexion.java")
    exclude("s9apitest/TestSaxonParam.java")
    exclude("s9apitest/TestSchemaProcessing.java")
    exclude("s9apitest/TestSchematron.java")
    exclude("s9apitest/TestSerializer.java")
    exclude("s9apitest/TestValidator.java")
    exclude("s9apitest/TestXMLParserOptions.java")
    exclude("s9apitest/TestXMarkProjection.java")
    exclude("s9apitest/TestXPathCompiler.java")
    exclude("s9apitest/TestXPathErrorCode.java")
    exclude("s9apitest/TestXQueryCompiler.java")
    exclude("s9apitest/TestXQueryEvaluator.java")
    exclude("s9apitest/TestXQueryStreaming.java")
    exclude("s9apitest/TestXQueryThreads.java")
    exclude("s9apitest/TestXQueryToSaxDestination.java")
    exclude("s9apitest/TestXQueryUpdate.java")
    exclude("s9apitest/TestXdmFunction.java")
    exclude("s9apitest/TestXslt30Streaming.java")
    exclude("s9apitest/TestXslt30Transformer.java")
    exclude("s9apitest/TestXslt30TransformerDebug.java")
    exclude("s9apitest/TestXsltCompiler.java")
    exclude("s9apitest/TestXsltIn10Mode.java")
    exclude("s9apitest/TestXsltTracing.java")
    exclude("s9apitest/TestXsltTransformer.java")
    exclude("s9apitest/ThreadCountTest.java")
    exclude("s9apitest/XQueryStreamingTest.java")
    exclude("xqjtests/RennauBugs.java")
    exclude("xqjtests/RennauChainedQueries.java")
    exclude("xqjtests/TestSaxonDuration.java")
    exclude("xqjtests/TestTreeWalker.java")
    exclude("xqjtests/TestXQueryUsingXQJ.java")
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

dependencies {
  downloadReleaseBundled("com.ibm.icu:icu4j:${findProperty("icu4jVersion")}")
  sampleSourcesBucket(project(mapOf("path" to ":samples",
                                    "configuration" to "sampleSources")))
  javaSampleSourcesBucket(project(mapOf("path" to ":samples",
                                        "configuration" to "javaSampleSources")))
}

tasks.named("compileJava") {
  onlyIf { false }
}

tasks.named("allJars") {
  finalizedBy("sqlDownloadJar")
}

tasks.filteredSources {
  from(configurations.javaSources) {
    include("net/sf/saxon/**/*.java")
    include("com/saxonica/**/*.java")
    include("javax/xml/xquery/*.java")
    include("net/sf/saxon/**/package.html")
    exclude("com/saxonica/ee/**")
    exclude("com/saxonica/functions/extfn/cpp/*.java")
    exclude("com/saxonica/testdriver/**")
    exclude("net/sf/saxon/testdriver/**")
    exclude("net/sf/saxon/option/cpp/**")
    exclude("com/saxonica/Validate.java")
    exclude("com/saxonica/config/EnterpriseConfiguration.java")
    exclude("com/saxonica/config/EnterpriseTransformerFactory.java")
    exclude("com/saxonica/config/EnterpriseXPathFactory.java")
    exclude("com/saxonica/config/JavaPlatformEE.java")
    exclude("com/saxonica/config/NativePlatformPE.java")
    exclude("com/saxonica/config/StreamingTransformerFactory.java")
    filter {
      if (it.contains("import com.saxonica.ee") || it.contains("import com.saxonica.config.ee")) {
        "//" + it
      } else {
        it
      }
    }
  }
}

tasks.preprocessJava {
  opts {
    on("PE")
    off("EE", "STREAM", "SCHEMA", "OPT", "SAXONC", "BYTECODE", "CSHARP")
  }
}

tasks.jar {
  exclude("**/sql")
  exclude("com/saxonica/CompileStylesheet.class")
  exclude("com/saxonica/CompileStylesheet\$TracingObjectOutputStream.class")
  exclude("com/saxonica/Validate.class")
  exclude("com/saxonica/config/EnterpriseConfiguration.class")
  exclude("com/saxonica/config/EnterpriseConfiguration\$SurrogateSchema.class")
  exclude("com/saxonica/config/EnterpriseTransformerFactory.class")
  exclude("com/saxonica/config/EnterpriseXPathFactory.class")
  exclude("com/saxonica/config/StreamingTransformerFactory.class")
  exclude("com/saxonica/ee")
  exclude("com/saxonica/expr/CastToList.class")
  exclude("com/saxonica/expr/CastToUnion.class")
  exclude("com/saxonica/expr/CastableToList.class")
  exclude("com/saxonica/expr/CastableToUnion.class")

  manifest {
    attributes("Project-Name" to "Saxon-PE",
      "Main-Class" to "net.sf.saxon.Transform")
  }
}

tasks.compileTestDrivers {
  include("com/saxonica/testdriver/QT3TestDriverPE.java")
  include("com/saxonica/testdriver/Xslt30TestSuiteDriverPE.java")
  include("com/saxonica/testdriver/Licensor.java")
}

val jarTaskBuilder = saxonj.jarTaskBuilder(project)
val editionCompileJava = tasks.named("compileEditionJava")

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

tasks.named<Sync>("signingDownloadJars") {
  into(layout.buildDirectory.dir("signing/download"))
  from(downloadSqlJar)
}

tasks.named<Sync>("signingMavenJars") {
  into(layout.buildDirectory.dir("signing/mavendownload"))
  from(mavenSqlJar)
  from(mavenSqlSourceJar)
}

val compile_testdrivers = tasks.named<JavaCompile>("compileTestDrivers")

val testSuitesClasspath = files(sourceSets["edition"].output)
    .from(configurations.testDrivers)
    .from(compile_testdrivers)
testSuites {
  product = "java"
  edition = "pe"
  testclasspath = testSuitesClasspath
  submodulesDir = layout.projectDirectory.dir("../../submodules")
  exceptionsDir = layout.projectDirectory.dir("../src/test/exceptions")
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
    from(layout.projectDirectory.dir("../../src/main/notices"))
    include("ICU-J.txt")
  }
}

// In principle, this should be in the samples-plugin...
tasks.register<Copy>("copy_java_sample_sources") {
    into(layout.buildDirectory.dir("samples/src/java/main"))
    from(javaSampleSources) {
      exclude("ee/**")
    }
}

samples {
  runclasspath = (files(sourceSets["edition"].output)
                  + configurations.named("samplesConfiguration").get())
  debugclasspath = (files(layout.buildDirectory.dir("classes/java/main"))
                    + files(layout.buildDirectory.dir("build/resources/main"))
                    + configurations.named("samplesConfiguration").get())

  sources = sampleSources
  product = "java"
  edition = "pe"
}

samples.transform("Books_Transform_1", "books.xml", "books.xsl", listOf())
samples.transform("Books_Transform_2", "books.xml", "books.xsl", 
                  listOf("top-author=Thomas Hardy"))

samples.transform("Books_Transform_3", "books.xml", "books-csv.xsl")

samples.query("Books_Query_1", "books.xml", "books.xq")

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

samples.sampleClass("JAXPExamples", listOf("all"), "he.JAXPExamples")
samples.sampleClass("ApplyXPathJAXP_saxon",
                    listOf("data/books.xml", "/BOOKLIST/BOOKS/ITEM[@CAT='MMP']/TITLE", "saxon"),
                    "he.ApplyXPathJAXP")
samples.sampleClass("ApplyXPathJAXP_dom",
                    listOf("data/books.xml", "/BOOKLIST/BOOKS/ITEM[@CAT='MMP']/TITLE", "dom"),
                    "he.ApplyXPathJAXP")
samples.sampleClass("ExampleContentHandler", listOf("data/books.xml"), "he.ExampleContentHandler")
samples.sampleClass("S9APIExamplesOM_pe", listOf("nonschema"), "he.S9APIExamplesOM")
samples.sampleClass("S9APIExamples_pe", listOf("nonschema"), "he.S9APIExamples")

// ============================================================

tasks.register<JavaExec>("pe_run_test") {
  classpath = (files(sourceSets["edition"].java.classesDirectory)
               + configurations.named("downloadReleaseBundled").get())
  mainClass = "net.sf.saxon.Transform"
}

testSuites.configure("expath", "XP31")
testSuites.configure("expath", "XQ31")

testSuites.configure("qt3", "XP31")
testSuites.configure("qt3", "XQ31")

testSuites.configure("qt3extra", "XP31")
testSuites.configure("qt3extra", "XQ31")

testSuites.configure("xslt30", "XT30")
testSuites.configure("xslt30", "XT30", mapOf("strict" to "on"))

testSuites.configure("xslt30extra", "XT30")

val signTask = tasks.named("downloadSignedMavenJars")
val mavenArtifactId = "Saxon-PE"
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
