plugins {
  id("saxon-build")
  id("com.saxonica.build.testsuites")
  id("saxonj")
}

saxonj {
  edition = "pe"
  metaInfServices = mapOf(
    "javax.xml.transform.TransformerFactory" to "com.saxonica.config.ProfessionalTransformerFactory"
  )
  manifestMainClass = "net.sf.saxon.Transform"
  sourceRoot = layout.projectDirectory.dir("../src")
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

val downloadReleaseBundled by configurations.getting
dependencies {
  downloadReleaseBundled("com.ibm.icu:icu4j:${findProperty("icu4jVersion")}")
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

val compile_testdrivers = tasks.named<JavaCompile>("compileTestDrivers")
val testSuitesClasspath = files(sourceSets["edition"].java.classesDirectory).from(configurations.testDrivers).from(compile_testdrivers)
testSuites {
  product = "java"
  edition = "pe"
  testclasspath = testSuitesClasspath
  submodulesDir = layout.projectDirectory.dir("../../submodules")
  exceptionsDir = layout.projectDirectory.dir("../src/test/exceptions")
}

tasks.register("all_test_suites") {
  // Just somewhere to hang dependencies
}

testSuites.configure("qt3", "XP31")
testSuites.configure("qt3", "XQ31")

testSuites.configure("qt3extra", "XP31")
testSuites.configure("qt3extra", "XQ31")

testSuites.configure("expath", "XP31")
testSuites.configure("expath", "XQ31")

testSuites.configure("xslt30", "XT30")

testSuites.configure("xslt30extra", "XT30")

val downloadRelease = tasks.named<Zip>("downloadRelease") {
  into(archiveBaseName) {
    into("notices") {
      from(layout.projectDirectory.dir("../../src/docs/notices"))
      include("ICU-J.txt")
    }
  }
}

tasks.register("helloWorld") {
  doLast {
    compile_testdrivers.get().outputs.getFiles().forEach { file ->
      println(file)
    }
  }
}
