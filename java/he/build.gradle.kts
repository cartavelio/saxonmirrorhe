import com.saxonica.build.TestSuitesExtension

plugins {
  id("saxon-build")
  id("com.saxonica.build.testsuites")
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

dependencies {
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

val compile_testdrivers = tasks.named<JavaCompile>("compileTestDrivers")
val testSuitesClasspath = files(sourceSets["edition"].java.classesDirectory).from(configurations.testDrivers).from(compile_testdrivers)
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
