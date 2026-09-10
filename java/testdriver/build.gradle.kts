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

tasks.register<Jar>("he_jar") {
  archiveFileName = "saxon-he-test-${saxonBuild.version.get()}.jar"
  dependsOn("compileJava")
  from(sourceSets["main"].output) {
    exclude("com/**")
    exclude("previous-compilation-data.bin")
  }
}  

tasks.register<Jar>("pe_jar") {
  archiveFileName = "saxon-pe-test-${saxonBuild.version.get()}.jar"
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

tasks.register<Jar>("ee_jar") {
  archiveFileName = "saxon-ee-test-${saxonBuild.version.get()}.jar"
}  

tasks.register<Jar>("pe_sources_jar") {
  archiveFileName = "saxon-pe-test-${saxonBuild.version.get()}-test-sources.jar"
  from(sourceSets["main"].java) {
    exclude("com/saxonica/testdriver/QT3TestDriverEE*")
    exclude("com/saxonica/testdriver/Xslt30TestSuiteDriverEE*")
    exclude("com/saxonica/testdriver/Xslt30TestSuiteDriverJS*")
    exclude("com/saxonica/testdriver/XMarkBenchmark*")
    exclude("com/saxonica/testdriver/ee/SchemaTestSuiteDriver*")
    exclude("com/saxonica/testdriver/ee/XQTSCollectionFinder*")
  }
}

tasks.register<Jar>("he_sources_jar") {
  archiveFileName = "saxon-he-test-${saxonBuild.version.get()}-test-sources.jar"
  from(sourceSets["main"].java) {
    include("net/sf/saxon/testdriver/**")
  }
}

