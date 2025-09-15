import com.saxonica.build.TestSuitesExtension

plugins {
  id("saxon-build")
  id("com.saxonica.build.testsuites")
  id("com.saxonica.build.samples")
  id("saxonj")
}

saxonj {
  edition = "he"
  metaInfServices = mapOf(
    "javax.xml.transform.TransformerFactory" to "net.sf.saxon.TransformerFactoryImpl"
  )
  manifestMainClass = "net.sf.saxon.Transform"
  sourceRoot = layout.projectDirectory.dir("../src")
}

val xqjDownloadJarBucket by configurations.dependencyScope("xqjDownloadJarBucket")
val xqjDownloadJar by configurations.resolvable("xqjDownloadJar") {
  extendsFrom(xqjDownloadJarBucket)
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
    exclude("net/sf/saxon/testdriver/**")
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
  edition = "he"
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

samples.bookSample("transform", "html", "Transform_1",
                   listOf("-s:data/books.xml", "-xsl:styles/books.xsl"))

samples.bookSample("transform", "html", "Transform_2",
                   listOf("-s:data/books.xml", "-xsl:styles/books.xsl", "top-author=Thomas Hardy"))

samples.bookSample("transform", "txt", "Transform_3",
                   listOf("-s:data/books.xml", "-xsl:styles/books-csv.xsl"))

samples.bookSample("query", "html", "Query_1",
                   listOf("-s:data/books.xml", "-q:query/books.xq"))

// ============================================================

samples.sampleClass("JAXPExamples", listOf("all"))
samples.sampleClass("ApplyXPathJAXP_saxon",
                    listOf("data/books.xml", "/BOOKLIST/BOOKS/ITEM[@CAT='MMP']/TITLE", "saxon"),
                    "he.ApplyXPathJAXP")
samples.sampleClass("ApplyXPathJAXP_dom",
                    listOf("data/books.xml", "/BOOKLIST/BOOKS/ITEM[@CAT='MMP']/TITLE", "dom"),
                    "he.ApplyXPathJAXP")
samples.sampleClass("ExampleContentHandler", listOf("data/books.xml"), "he.ExampleContentHandler")
//samples.sampleClass("S9APIExamplesOM_he", listOf("all"), "he.S9APIExamplesOM") // requires setup
samples.sampleClass("S9APIExamples_he", listOf("nonschema"), "he.S9APIExamples")

/*
samples.sampleClass("S9APIExamplesOM_pe":
samples.sampleClass("S9APIExamples_pe":
*/


// ============================================================

val compile_testdrivers = tasks.named<JavaCompile>("compileTestDrivers")
val testSuitesClasspath = files(sourceSets["edition"].output)
    .from(configurations.testDrivers)
    .from(compile_testdrivers)
testSuites {
  product = "java"
  edition = "he"
  testclasspath = testSuitesClasspath
  submodulesDir = layout.projectDirectory.dir("../../submodules")
  exceptionsDir = layout.projectDirectory.dir("../src/test/exceptions")
}

testSuites.configure("qt3", "XP31")
testSuites.configure("qt3", "XQ31")
testSuites.configure("xslt30", "XT30")
testSuites.configure("xslt30extra", "XT30")

val downloadRelease = tasks.named<Zip>("downloadRelease") {
  into(archiveBaseName) {
    from(xqjDownloadJar)
  }
}
