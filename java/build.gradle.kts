plugins {
  id("saxon-build")
  id("com.saxonica.build.testsuites")
  id("com.saxonica.build.samples")
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

samples.transform("Books_Transform_1", "books.xml", "books.xsl",
                  listOf("-t"))

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

samples.bookSample("transform", "html", "Transform_1",
                   listOf("-s:data/books.xml", "-xsl:styles/books.xsl"))

samples.bookSample("transform", "html", "Transform_2",
                   listOf("-s:data/books.xml", "-xsl:styles/books.xsl", "top-author=Thomas Hardy"))

samples.bookSample("transform", "txt", "Transform_3",
                   listOf("-s:data/books.xml", "-xsl:styles/books-csv.xsl"))

samples.bookSample("query", "html", "Query_1",
                   listOf("-s:data/books.xml", "-q:query/books.xq"))

samples.bookSample("query", "html", "Query_2",
                   listOf("-s:data/books.xml", "-qs:saxon:line-number((//*)[last()])", "-l:on"))

samples.bookSample("validate", "xml", "Validate_1",
                   listOf("-s:data/books.xml", "-xsd:data/books.xsd"))

samples.bookSample("validate", "xml", "Validate_2",
                   listOf("-s:data/books-invalid.xml", "-xsd:data/books.xsd", "-quit:off"))

// ============================================================

val copy_java_sample_sources = tasks.register<Copy>("copy_java_sample_sources") {
  into(layout.buildDirectory.dir("samples/src/java/main"))
  from(javaSampleSources)
}

samples.javaTest("ApplyXPathJAXP_saxon", "he.ApplyXPathJAXP", "ApplyXPathJAXP",
                 "books.xml", listOf("/BOOKLIST/BOOKS/ITEM[@CAT='MMP']/TITLE", "saxon"))


/*


  val sampleClass = "he.ApplyXPathJAXP"
  


      if (!ignoreThisCombination) {
        def sampleClass = "he.${sample}"
        if (heSampleMainClass.containsKey(sample)) {
          sampleClass = "he.${heSampleMainClass[sample]}"
        }
        def sampleArgs = heSampleArgs[sample]

        task "${tver}_he_samples_${sample}_${edition}"(
          type: JavaExec,
          dependsOn: ["${edition}_samples_common"]
        ) {
          def resdir = "${buildDir}/samples/results/he"

          if (tver == "run") {
            dependsOn "${tver}_${edition}j_samples_he", "${edition}jJar"
            description "Run test of HE ${sample} with ${edition}"
            outputs.file "${resdir}/${sample}_${edition}.txt"

            classpath = configurations.samples \
            + files("${buildDir}/samples/classes/${tver}/${edition}") \
            + files("${buildDir}/libs/saxon-${edition}-${saxonVersion}.jar")

          } else {
            dependsOn "${tver}_${edition}j_samples_he", "compileJava"
            description "Debug test of HE ${sample} with ${edition}"

            classpath = configurations.samples \
            + files("${buildDir}/samples/classes/${tver}/${edition}") \
            + project.files("${buildDir}/classes/java/main") \
            + files("${projectDir}/src/main/resources")
          }

          inputs.files fileTree(dir: "${buildDir}/samples/classes/${tver}/${edition}")
          inputs.files "${projectDir}/src/samples/java/he"
          workingDir = "${buildDir}/output/he/resources/samples"

          doFirst {
            if (tver == "run") {
              mkdir resdir
              standardOutput = new FileOutputStream(new File("${resdir}/${sample}_${edition}.txt"))
            }
          }

          systemProperties = ["javax.xml.transform.TransformerFactory": "net.sf.saxon.TransformerFactoryImpl"]

          mainClass = sampleClass
          args sampleArgs
        }

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
