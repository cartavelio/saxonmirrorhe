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
val xqjMavenJarBucket by configurations.dependencyScope("xqjMavenJarBucket")
val xqjMavenJar by configurations.resolvable("xqjMavenJar") {
  extendsFrom(xqjMavenJarBucket)
}
val xqjSourceMavenJarBucket by configurations.dependencyScope("xqjSourceMavenJarBucket")
val xqjSourceMavenJar by configurations.resolvable("xqjSourceMavenJar") {
  extendsFrom(xqjSourceMavenJarBucket)
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

// ============================================================

val run_with_config = tasks.register<JavaExec>("run_with_config-HE") {
  doFirst {
    mkdir(layout.buildDirectory.file("sample_results"))
  }

  classpath = (files(sourceSets["edition"].output)
               + configurations.named("samplesConfiguration").get())
  mainClass = "net.sf.saxon.Transform"
  args("-config:${layout.projectDirectory.file("../../samples/src/config/config-HE.xml")}",
       "-s:${layout.projectDirectory.file("../../samples/src/data/books.xml")}",
       "-xsl:${layout.projectDirectory.file("../../samples/src/styles/books.xsl")}",
       "-o:${layout.buildDirectory.file("sample_results/books_with_config-HE.html").get()}",
       "--traceListenerOutputFile:${layout.buildDirectory.file("sample_results/traceOutput_config.log").get()}")
}
tasks.named("all_samples").get().dependsOn(run_with_config)

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

val saxonBuild = the<SaxonBuildExtension>()

val pseudoXqjMavenJar = tasks.register("pseudoXqjMavenJar") {
  inputs.file(xqjMavenJar)
  outputs.file(xqjMavenJar)
}

val pseudoXqjSourceMavenJar = tasks.register("pseudoXqjSourceMavenJar") {
  inputs.file(xqjSourceMavenJar)
  outputs.file(xqjSourceMavenJar)
}
val mavenArtifactId = "Saxon-HE"
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

publishing {
  publications {
    create<MavenPublication>("maven") {
      groupId = "net.sf.saxon"
      artifactId = mavenArtifactId
      version = saxonBuild.version.get()

      pom(mavenPom.generate())

      artifact(tasks.named<Jar>("editionMavenJar"))
      artifact(tasks.named<Jar>("javadocMavenJar"))
      artifact(tasks.named<Jar>("editionSourceMavenJar"))
      artifact(pseudoXqjMavenJar) {
        classifier = "xqj"
      }
      artifact(pseudoXqjSourceMavenJar) {
        classifier = "xqj-sources"
      }
      artifact(tasks.named<Jar>("testDriversMavenJar"))
      artifact(tasks.named<Jar>("testDriversSourceMavenJar"))
    }
  }
  repositories {
    maven {
      url = uri(layout.buildDirectory.dir("maven-o-matic").get())
    }
    maven {
      url = uri("https://oss.sonatype.org/service/local/staging/deploy/maven2/")
      credentials {
        username = project.findProperty("sonatypeUsername")?.toString() ?: "NOUSER"
        password = project.findProperty("sonatypePassword")?.toString() ?: "NOPASS"
      }
    }
  }
}

signing {
  sign(publishing.publications)
}
