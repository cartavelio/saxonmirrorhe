plugins {
  id("java-library")
  id("saxon-build")
}

val saxonBuild = the<SaxonBuildExtension>()

java {
  sourceCompatibility = JavaVersion.VERSION_17
  targetCompatibility = JavaVersion.VERSION_17
}

dependencies {
  compileOnly(project(path = ":java:he", configuration = "saxonJarArtifact"))
}

val testDriverClasses by configurations.consumable("testDriverClasses")
testDriverClasses.outgoing.artifact(tasks.compileJava.flatMap { it.destinationDirectory })

val driverVersion = saxonBuild.version.get().removeSuffix(".0")
val downloadJar = tasks.register<Jar>("download-he-jar") {
  archiveFileName = "saxon-he-test-${driverVersion}.jar"
  destinationDirectory = layout.buildDirectory.dir("downloadJars")
  from(sourceSets.main.get().output)
}

val mavenJar = tasks.register<Jar>("maven-he-jar") {
  archiveFileName = "Saxon-HE-${driverVersion}-test.jar"
  destinationDirectory = layout.buildDirectory.dir("mavenJars")
  from(sourceSets.main.get().output)
}

val mavenSourcesJar = tasks.register<Jar>("maven-he-sources-jar") {
  archiveFileName = "Saxon-HE-${driverVersion}-test-sources.jar"
  destinationDirectory = layout.buildDirectory.dir("mavenJars")
  from(sourceSets.main.get().java)
}

configurations.consumable("downloadHEJar") { outgoing.artifact(downloadJar) }
configurations.consumable("mavenHEJar") { outgoing.artifact(mavenJar) }
configurations.consumable("mavenHESourcesJar") { outgoing.artifact(mavenSourcesJar) }
