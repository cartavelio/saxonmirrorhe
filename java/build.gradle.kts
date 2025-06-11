plugins {
  id("saxon-build")
  id("com.saxonica.build.testsuites")
  id("saxonj")
}

saxonj {
  edition = "ee"
  metaInfServices = mapOf(
    "javax.xml.transform.TransformerFactory" to "com.saxonica.config.EnterpriseTransformerFactory",
    "javax.xml.validation.SchemaFactory" to "com.saxonica.config.EnterpriseTransformerFactory"
  )
  manifestMainClass = "net.sf.saxon.Transform"
  sourceRoot = layout.projectDirectory.dir("src")
  jarManifestClasspath = jarClasspath("lib/icu4j-${findProperty("icu4jVersion")}.jar", "lib/icu4j.jar")
  editionJarExclusions = listOf("**/sql", "com/saxonica/StatsTransform*.class")
}
val downloadReleaseBundled by configurations.getting

dependencies {
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

downloadSqlJar.configure {
  include("thing")
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

val (downloadXqjJar, mavenXqjJar) = jarTaskBuilder("xqj") {
  edition = "he"
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

xqjDownloadJarArtifact.outgoing.artifact(downloadXqjJar)

val compile_testdrivers = tasks.named<JavaCompile>("compileTestDrivers") {
  include("com/saxonica/testdriver/**")
  exclude("com/saxonica/testdriver/ee/**")
}
val testSuitesClasspath = files(sourceSets["edition"].java.classesDirectory).from(configurations.testDrivers).from(compile_testdrivers)
testSuites {
  product = "java"
  edition = "ee"
  testclasspath = testSuitesClasspath
  submodulesDir = layout.projectDirectory.dir("../submodules")
  exceptionsDir = layout.projectDirectory.dir("src/test/exceptions")
}

testSuites.configure(suite = "qt3", language = "XP31")
testSuites.configure(suite = "qt3", language = "XP31", options = mapOf("unfolded" to "on"))
testSuites.configure(suite = "qt3", language = "XQ31")
testSuites.configure(suite = "qt3", language = "XQ31", options = mapOf("unfolded" to "on"))

testSuites.configure("qt3extra", "XP31")
testSuites.configure("qt3extra", "XQ31")
testSuites.configure("qt3extra", "XQ40")

testSuites.configure("expath", "XP31")
testSuites.configure("expath", "XP31", mapOf("unfolded" to "on"))
testSuites.configure("expath", "XQ31")
testSuites.configure("expath", "XQ40")

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

val downloadRelease = tasks.named<Zip>("downloadRelease") {
  into(archiveBaseName) {
    from(downloadSqlJar)
    into("notices") {
      from(layout.projectDirectory.dir("../src/docs/notices"))
      include("ICU-J.txt")
    }
  }
}

tasks.named("allJars") {
  finalizedBy("sqlDownloadJar")
}
