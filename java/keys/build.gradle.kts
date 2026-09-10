plugins {
  id("saxon-build")
}

repositories {
  mavenLocal()
  mavenCentral()
  maven { url = uri("https://maven.saxonica.com/maven") }
}

val transform by configurations.dependencyScope("transform")
val transformClasspath by configurations.resolvable("transformClasspath") {
    extendsFrom(transform)
}
val featureKeys by configurations.consumable("featureKeys")

dependencies {
  transform("net.sf.saxon:Saxon-HE:12.4")
}

tasks.register("clean") {
  delete(layout.buildDirectory)
}

val featureKeysTask = tasks.register<JavaExec>("featureKeys") {
  inputs.dir(layout.projectDirectory.dir("../../src/main/xml"))
  inputs.dir(layout.projectDirectory.dir("tools"))
  outputs.dir(layout.buildDirectory.dir("java"))
  classpath = transformClasspath
  mainClass = "net.sf.saxon.Transform"
  args("${layout.projectDirectory.file("../../src/main/xml/FeatureKeys.xml")}",
       "-xsl:${layout.projectDirectory.file("tools/FeatureKeysToJava.xsl")}",
       "-o:${layout.buildDirectory.file("java/net/sf/saxon/lib/establish-an-output-base-uri").get()}")

  doLast {
    delete("${layout.buildDirectory.file("java/net/sf/saxon/lib/establish-an-output-base-uri").get()}")
  }
}

featureKeys.outgoing.artifact(featureKeysTask)
