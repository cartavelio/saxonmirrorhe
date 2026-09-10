plugins {
  id("java-library")
  id("saxon-build")
}

val saxonBuild = the<SaxonBuildExtension>()

java {
    sourceCompatibility = JavaVersion.VERSION_17
    targetCompatibility = JavaVersion.VERSION_17
}

val inputSaxonJarBucket by configurations.dependencyScope("inputSaxonJarBucket")
val inputSaxonJar = configurations.resolvable("inputSaxonJar") {
  extendsFrom(inputSaxonJarBucket)
}

val inputSqlJarBucket by configurations.dependencyScope("inputSqlJarBucket")
val inputSqlJar = configurations.resolvable("inputSqlJar") {
  extendsFrom(inputSqlJarBucket)
}

val testDriverClasses by configurations.consumable("testDriverClasses")

dependencies {
  inputSaxonJarBucket(project(":java", "saxonJarArtifact"))
  inputSqlJarBucket(project(":java", "sqlMavenJarArtifact"))
}

configurations {
  implementation { extendsFrom(inputSaxonJar) }
  implementation { extendsFrom(inputSqlJar) }
}

val downloadJarNames = mapOf("ee" to "saxon-ee", "he" to "saxon-he", "pe" to "saxon-pe")
val mavenJarNames = mapOf("ee" to "Saxon-EE", "he" to "Saxon-HE", "pe" to "Saxon-PE")

testDriverClasses.outgoing.artifacts(tasks.named<JavaCompile>("compileJava").map
                                     { it.outputs.files.filter { 
                                         it.name != "previous-compilation-data.bin" }}) { builtBy("compileJava") }

fun registerJarTaskAndArtifactForEdition(edition: String, version: String, prefix: String, outputDir: Provider<Directory>, jarName: (String, String) -> String, jarCopySpec: CopySpec.() -> Unit) {
  val jar = tasks.register<Jar>("${prefix}-${edition}-jar") {
    group = LifecycleBasePlugin.BUILD_GROUP
    archiveFileName = jarName(edition, version)
    destinationDirectory = outputDir.get().asFile
    from(sourceSets["main"].output, jarCopySpec)
  }

  val artifact by configurations.consumable("${prefix}${edition.uppercase()}Jar")
  artifact.outgoing.artifact(jar.map { it.archiveFile }) { builtBy(jar) }
}

fun registerMavenSourceJarTaskAndArtifactForEdition(edition: String, version: String, outputDir: Provider<Directory>, jarName: (String, String) -> String, jarCopySpec: CopySpec.() -> Unit) {
  val jar = tasks.register<Jar>("maven-${edition}-sources-jar") {
    group = LifecycleBasePlugin.BUILD_GROUP
    archiveFileName = jarName(edition, version)
    destinationDirectory = outputDir.get().asFile
    from(sourceSets["main"].java, jarCopySpec)
  }

  val artifact by configurations.consumable("maven${edition.uppercase()}SourcesJar")
  artifact.outgoing.artifact(jar.map { it.archiveFile }) { builtBy(jar) }
}

fun registerJarTasksAndArtifacts(archiveVersion: String, prefix: String, outputDir: Provider<Directory>, jarName: (String, String) -> String) {
  registerJarTaskAndArtifactForEdition("he", archiveVersion, prefix, outputDir, jarName) {
    exclude("com/**")
    exclude("previous-compilation-data.bin")
  }

  registerJarTaskAndArtifactForEdition("pe", archiveVersion, prefix, outputDir, jarName) {
    exclude("com/saxonica/testdriver/QT3TestDriverEE*")
    exclude("com/saxonica/testdriver/Xslt30TestSuiteDriverEE*")
    exclude("com/saxonica/testdriver/Xslt30TestSuiteDriverJS*")
    exclude("com/saxonica/testdriver/XMarkBenchmark*")
    exclude("com/saxonica/testdriver/ee/SchemaTestSuiteDriver*")
    exclude("com/saxonica/testdriver/ee/XQTSCollectionFinder*")
    exclude("previous-compilation-data.bin")
  }

  registerJarTaskAndArtifactForEdition("ee", archiveVersion, prefix, outputDir, jarName) {
    exclude("previous-compilation-data.bin")
  }
}

fun registerMavenSourceJarTasksAndArtifacts(archiveVersion: String, outputDir: Provider<Directory>, jarName: (String, String) -> String) {
  registerMavenSourceJarTaskAndArtifactForEdition("he", archiveVersion, outputDir, jarName) {
    include("net/sf/saxon/testdriver/**")
  }

  registerMavenSourceJarTaskAndArtifactForEdition("pe", archiveVersion, outputDir, jarName) {
    exclude("com/saxonica/testdriver/QT3TestDriverEE*")
    exclude("com/saxonica/testdriver/Xslt30TestSuiteDriverEE*")
    exclude("com/saxonica/testdriver/Xslt30TestSuiteDriverJS*")
    exclude("com/saxonica/testdriver/XMarkBenchmark*")
    exclude("com/saxonica/testdriver/ee/SchemaTestSuiteDriver*")
    exclude("com/saxonica/testdriver/ee/XQTSCollectionFinder*")
  }

  registerMavenSourceJarTaskAndArtifactForEdition("ee", archiveVersion, outputDir, jarName) {
    // this copyspec left intentionally blank
  }
}

val parts = saxonBuild.version.get().split('.')
val usefulParts = if (parts.last() == "0") parts.dropLast(1) else parts
val archiveVersion = usefulParts.joinToString(".")

registerJarTasksAndArtifacts(archiveVersion, "download", layout.buildDirectory.dir("downloadJars")) { edition, version ->
  "saxon-${edition}-test-${version}.jar"
}

registerJarTasksAndArtifacts(archiveVersion, "maven", layout.buildDirectory.dir("mavenJars")) { edition, version ->
  "Saxon-${edition.uppercase()}-${version}-test.jar"
}

registerMavenSourceJarTasksAndArtifacts(archiveVersion, layout.buildDirectory.dir("mavenJars")) { edition, version ->
  "Saxon-${edition.uppercase()}-${version}-test-sources.jar"
}
