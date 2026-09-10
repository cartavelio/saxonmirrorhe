plugins {
  id("java-library")
  id("saxon-build")
}

repositories {
  mavenLocal()
  mavenCentral()
  maven { url = uri("https://maven.saxonica.com/maven") }
}

dependencies {
  implementation("com.saxonica:Saxon-EE:12.10")
  implementation("com.ibm.icu:icu4j:${findProperty("icu4jVersion")}")
  implementation("com.github.javaparser:javaparser-symbol-solver-core:3.27.1")
}
