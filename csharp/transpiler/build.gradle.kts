plugins {
  id("java-library")
  id("saxon-build")
}

repositories {
  mavenLocal()
  mavenCentral()
  maven { url = uri("https://maven.saxonica.com/maven") }
}

val tcompiler by configurations.creating {
  extendsFrom(configurations["implementation"])
}

dependencies {
  tcompiler(project(":java", "runtimeElements"))
  implementation("com.saxonica:Saxon-EE:12.4")
  implementation("com.ibm.icu:icu4j:${findProperty("icu4jVersion")}")
  implementation("com.github.javaparser:javaparser-symbol-solver-core:3.17.0")
  implementation("com.github.javaparser:javaparser-core:3.17.0")
}
