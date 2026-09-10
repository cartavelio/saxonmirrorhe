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

fun registerJarTasksAndArtifacts(prefix: String, outputDir: Provider<Directory>, jarNames: Map<String, String>) {
    val parts = saxonBuild.version.get().split('.')
    val usefulParts = if (parts.last() == "0") parts.dropLast(1) else parts
    val archiveVersion = usefulParts.joinToString(".")

  val heJar = tasks.register<Jar>("${prefix}-he-jar") {
    archiveFileName = "${jarNames["he"]}-${archiveVersion}-test.jar"
    destinationDirectory = outputDir.get().asFile
    dependsOn("compileJava")
    from(sourceSets["main"].output) {
      exclude("com/**")
      exclude("previous-compilation-data.bin")
    }
  }

  val peJar = tasks.register<Jar>("${prefix}-pe-jar") {
    archiveFileName = "${jarNames["pe"]}-${archiveVersion}-test.jar"
    destinationDirectory = outputDir.get().asFile
    from(sourceSets["main"].output) {
      exclude("com/saxonica/testdriver/QT3TestDriverEE*")
      exclude("com/saxonica/testdriver/Xslt30TestSuiteDriverEE*")
      exclude("com/saxonica/testdriver/Xslt30TestSuiteDriverJS*")
      exclude("com/saxonica/testdriver/XMarkBenchmark*")
      exclude("com/saxonica/testdriver/ee/SchemaTestSuiteDriver*")
      exclude("com/saxonica/testdriver/ee/XQTSCollectionFinder*")
      exclude("previous-compilation-data.bin")
    }
  }

  val eeJar = tasks.register<Jar>("${prefix}-ee-jar") {
    archiveFileName = "${jarNames["ee"]}-${archiveVersion}-test.jar"
    destinationDirectory = outputDir.get().asFile
  }

  val peSourcesJar = tasks.register<Jar>("${prefix}-pe-sources-jar") {
    archiveFileName = "${jarNames["pe"]}-${archiveVersion}-test-sources.jar"
    destinationDirectory = outputDir.get().asFile
    from(sourceSets["main"].java) {
      exclude("com/saxonica/testdriver/QT3TestDriverEE*")
      exclude("com/saxonica/testdriver/Xslt30TestSuiteDriverEE*")
      exclude("com/saxonica/testdriver/Xslt30TestSuiteDriverJS*")
      exclude("com/saxonica/testdriver/XMarkBenchmark*")
      exclude("com/saxonica/testdriver/ee/SchemaTestSuiteDriver*")
      exclude("com/saxonica/testdriver/ee/XQTSCollectionFinder*")
    }
  }

  val heSourcesJar = tasks.register<Jar>("${prefix}-he-sources-jar") {
    archiveFileName = "${jarNames["he"]}-${archiveVersion}-test-sources.jar"
    destinationDirectory = outputDir.get().asFile
    from(sourceSets["main"].java) {
      include("net/sf/saxon/testdriver/**")
    }
  }

  val eeSourcesJar = tasks.register<Jar>("${prefix}-ee-sources-jar") {
    archiveFileName = "${jarNames["ee"]}-${archiveVersion}-test-sources.jar"
    destinationDirectory = outputDir.get().asFile
    from(java.sourceSets["main"].java)
  }

  val heJarArtifact by configurations.consumable("${prefix}HEJar")
  heJarArtifact.outgoing.artifact(heJar.map { it.archiveFile }) { builtBy(heJar) }

  val peJarArtifact by configurations.consumable("${prefix}PEJar")
  peJarArtifact.outgoing.artifact(peJar.map { it.archiveFile }) { builtBy(peJar) }

  val eeJarArtifact by configurations.consumable("${prefix}EEJar")
  eeJarArtifact.outgoing.artifact(eeJar.map { it.archiveFile }) { builtBy(eeJar) }

  val heSourceJarArtifact by configurations.consumable("${prefix}HESourcesJar")
  heSourceJarArtifact.outgoing.artifact(heSourcesJar.map { it.archiveFile }) { builtBy(heSourcesJar) }

  val peSourceJarArtifact by configurations.consumable("${prefix}PESourcesJar")
  peSourceJarArtifact.outgoing.artifact(peSourcesJar.map { it.archiveFile }) { builtBy(peSourcesJar) }

  // cheating
  val eeSourceJarArtifact by configurations.consumable("${prefix}EESourcesJar")
  eeSourceJarArtifact.outgoing.artifact(eeSourcesJar.map { it.archiveFile }) { builtBy(eeSourcesJar) }
}

registerJarTasksAndArtifacts("maven", layout.buildDirectory.dir("mavenJars"), mavenJarNames)
registerJarTasksAndArtifacts("download", layout.buildDirectory.dir("downloadJars"), downloadJarNames)
