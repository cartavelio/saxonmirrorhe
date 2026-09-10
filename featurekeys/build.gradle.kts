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

val featureKeysSourcesConfiguration by configurations.creating {}

val javaFeatureKeys by configurations.consumable("javaFeatureKeys")
val csharpFeatureKeys by configurations.consumable("csharpFeatureKeys")
val featureKeysSources by configurations.consumable("featureKeysSources") {
  extendsFrom(featureKeysSourcesConfiguration)
}

dependencies {
  transform("net.sf.saxon:Saxon-HE:12.5")
  featureKeysSourcesConfiguration(files(layout.projectDirectory.dir("src/main")))
}

tasks.register("clean") {
  delete(layout.buildDirectory)
}

val javaFeatureKeysTask = tasks.register<JavaExec>("javaFeatureKeys") {
  inputs.dir(layout.projectDirectory.dir("src/main/xml"))
  inputs.dir(layout.projectDirectory.dir("src/main/xsl"))
  outputs.dir(layout.buildDirectory.dir("java"))
  classpath = transformClasspath
  mainClass = "net.sf.saxon.Transform"
  args("${layout.projectDirectory.file("src/main/xml/FeatureKeys.xml")}",
       "-xsl:${layout.projectDirectory.file("src/main/xsl/FeatureKeysToJava.xsl")}",
       "-o:${layout.buildDirectory.file("java/net/sf/saxon/lib/establish-an-output-base-uri").get()}")

  doLast {
    delete("${layout.buildDirectory.file("java/net/sf/saxon/lib/establish-an-output-base-uri").get()}")
  }
}
javaFeatureKeys.outgoing.artifact(javaFeatureKeysTask)

val csharpFeatureKeysTask = tasks.register<JavaExec>("csharpFeatureKeys") {
  inputs.dir(layout.projectDirectory.dir("src/main/xml"))
  inputs.dir(layout.projectDirectory.dir("src/main/xsl"))
  outputs.dir(layout.buildDirectory.dir("cs"))
  classpath = transformClasspath
  mainClass = "net.sf.saxon.Transform"
  args("${layout.projectDirectory.file("src/main/xml/FeatureKeys.xml")}",
       "-xsl:${layout.projectDirectory.file("src/main/xsl/FeatureKeysForSaxonCS.xsl")}",
       "-o:${layout.buildDirectory.file("cs/Saxon/Api/establish-an-output-base-uri").get()}")

  doLast {
    delete("${layout.buildDirectory.file("cs/Saxon/Api/establish-an-output-base-uri").get()}")
  }
}
csharpFeatureKeys.outgoing.artifact(csharpFeatureKeysTask)
