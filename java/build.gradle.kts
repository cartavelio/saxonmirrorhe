import com.saxonica.build.java.PomGenerator
import com.saxonica.build.GatherReleaseArtifacts
import org.gradle.kotlin.dsl.register

plugins {
  id("saxon-build")
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

  editionSourceRoots(layout.projectDirectory.dir("pe/src"),
    layout.projectDirectory.dir("ee/src"))

  testDataDir = layout.projectDirectory.dir("../src/test/testdata")
  jarManifestClasspath = jarClasspath("lib/icu4j-${findProperty("icu4jVersion")}.jar", "lib/icu4j.jar")
  editionJarExclusions = listOf("**/sql", "com/saxonica/StatsTransform*.class")
}

val saxonBuild = the<SaxonBuildExtension>()

val signedReleaseArtifacts = findProperty("signedReleaseArtifacts") != "false"

sourceSets.named("editionTest") {
  java {
    exclude("net/sf/saxon/option/cpp/**")
    exclude("s9apitest/TestXMarkProjection.java")
  }
}

val downloadReleaseBundled by configurations.getting
val junit4 by configurations.dependencyScope("junit4")
val junit4Resolvable = configurations.register("junit4Resolvable") {
  extendsFrom(junit4)
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

  compileOnly("org.slf4j:slf4j-api:1.7.30")

  testImplementation(files(layout.projectDirectory.dir("lib/Tidy.jar")))
  testImplementation("javax.mail:javax.mail-api:${findProperty("mailVersion")}")
  testImplementation("com.sun.mail:javax.mail:${findProperty("mailVersion")}")
  testImplementation("nu.validator:htmlparser:${findProperty("htmlparserVersion")}")

  downloadReleaseBundled("com.ibm.icu:icu4j:${findProperty("icu4jVersion")}")

  junit4("junit:junit:4.13.2")
}

tasks.filteredSources {
  from(configurations.javaSources) {
    include("net/sf/saxon/**/*.java")
    include("org/nineml/**/*.java")
    exclude("org/nineml/logging/SystemLogger.java")
    exclude("org/nineml/coffeegrinder/parser/AmbiguityReport.java")
    exclude("org/nineml/coffeegrinder/parser/HygieneReport.java")
    exclude("org/nineml/coffeegrinder/parser/HygieneReport.java")
    exclude("org/nineml/coffeegrinder/util/BricsAmbiguity.java")
    include("javax/xml/xquery/*.java")
    exclude("**/dotnet/**")
    include("net/sf/saxon/**/package.html")
    exclude("net/sf/saxon/option/cpp/**")
    exclude("com/saxonica/functions/extfn/cpp/*.java")
    include("com/saxonica/**/*.java")
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

val editionCompileJava = tasks.named("compileEditionJava")

val jarTaskBuilder = saxonj.jarTaskBuilder(project)

val (downloadSqlJar, mavenSqlJar) = jarTaskBuilder("sql") {
  downloadJarBaseName = "saxon-sql"
  jarClassifier = "sql"
  common {
    from(editionCompileJava)
    include("com/saxonica/option/sql/*.class")
    include("com/saxonica/functions/sql/*.class")
  }
}

val sqlMavenJarArtifact by configurations.consumable("sqlMavenJarArtifact")

val (_, mavenSqlSourceJar) = jarTaskBuilder("sqlSource") {
  jarClassifier = "sql-sources"
  common {
    from(tasks.preprocessJava) {
      include("com/saxonica/option/sql/*.java")
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
sqlMavenJarArtifact.outgoing.artifact(mavenSqlJar) { builtBy(mavenSqlJar) }

tasks.register<GatherReleaseArtifacts>("gather_release_artifacts") {
  artifacts.from(heReleaseZips)
  //artifacts.from(heReleaseMaven)
  artifacts.from(peReleaseZips)
  //artifacts.from(peReleaseMaven)
  artifacts.from(eeReleaseZips)
  //artifacts.from(eeReleaseMaven)
}

val copySampleSources = tasks.register<Copy>("copy_java_sample_sources") {
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

val copySampleResources = tasks.register<Copy>("copy_sample_sources") {
    into(layout.buildDirectory.dir("samples/resources"))
    from(sampleSources)
}

val copyConfigEE = tasks.register<Copy>("copy_config_EE") {
  dependsOn(copySampleResources)
  into(layout.buildDirectory.dir("samples/resources/config"))
  from(layout.projectDirectory.file("../samples/src/config/config.xml"))
}

val sampleStaging = tasks.register<Copy>("samples_staging") {
  dependsOn(tasks.named("release"))

  val filename = "SaxonEE${saxonBuild.publishVersion.get().replace(".","-")}J.zip"

  doFirst {
    mkdir(layout.buildDirectory.file("sample-staging"))
  }
  from(zipTree(layout.buildDirectory.file("releases/${filename}")))
  into(layout.buildDirectory.file("sample-staging"))
}

val run_classes = tasks.register<JavaCompile>("compile_sample_run_classes") {
  dependsOn(copySampleSources)
  dependsOn(copySampleResources)
  dependsOn(copyConfigEE)
  classpath = (files(sourceSets["edition"].output)
               + configurations.named("samplesConfiguration").get())
  source = project.fileTree(layout.buildDirectory.dir("samples/src/java/main").get())
  destinationDirectory = layout.buildDirectory.dir("samples/run/classes").get()
}

tasks.register<Exec>("samples") {
  dependsOn(run_classes)
  dependsOn(sampleStaging)
  commandLine = listOf("python", "../tools/run-samples.py",
                       "--edition", "ee",
                       "--platform", "java")
}

// ============================================================

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
    }
  }
  repositories {
    maven {
      url = uri(layout.buildDirectory.dir("maven-o-matic").get())
    }
  }
}

// add license dir to classpath for transform/query/gizmo executable tasks
arrayOf("transform", "query", "gizmo").forEach {taskName ->
  tasks.named<JavaExec>(taskName) {
    classpath += files(saxonBuild.saxonLicenseDir.get())
  }
}

// use the com.saxonica command classes
arrayOf("transform", "query").forEach { taskName ->
  tasks.named<JavaExec>(taskName) {
    mainClass = "com.saxonica.${ taskName.replaceFirstChar(Char::uppercase) }"
  }
}

val validate = tasks.register<JavaExec>("validate") {
  classpath = sourceSets["edition"].runtimeClasspath + files(saxonBuild.saxonLicenseDir.get())

  mainClass = "com.saxonica.Validate"
}

val xqjTckJarFile = layout.projectDirectory.file("../tools/xqj/tck/lib/xqjtck.jar")
val xqjTckPropFile = layout.projectDirectory.file("src/test/resources/saxon-xqj-tck.properties").asFile.toString()
val xqjTckTest = tasks.register<JavaExec>("xqjTckTest") {
  classpath(tasks.named<Jar>("editionMavenJar").map { it.outputs.files },
    mavenXqjJar.map { it.outputs.files }, xqjTckJarFile,
    configurations["testRuntimeClasspath"], configurations["junit4Resolvable"]
  )

  systemProperty("com.oracle.xqj.tck.datasource", xqjTckPropFile)

  mainClass = "org.junit.runner.JUnitCore"
  args("com.oracle.xqj.tck.AllTests")
}

tasks.named(LifecycleBasePlugin.CHECK_TASK_NAME) {
  dependsOn(xqjTckTest)
}

signing {
  sign(publishing.publications)
}
