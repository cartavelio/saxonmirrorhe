plugins {
  id("saxon-build")
  id("com.saxonica.build.managebranches")
  id("saxonc")
}

repositories {
  mavenLocal()
  mavenCentral()
  maven { url = uri("https://maven.saxonica.com/maven") }
}

saxonc {
  edition = "he"
  classpath = configurations.compileClasspath
}

tasks.filteredSources {
    from(configurations.javaSources) {
      include("net/sf/saxon/option/cpp/**/*.java")
      include("net/sf/saxon/option/cpp/**/package.html")

      filter { if (it.contains("import com.saxonica")) "//" + it else it }
    }
}

tasks.preprocessJava {
  opts {
    off("EE", "PE", "STREAM", "SCHEMA", "OPT", "BYTECODE", "CSHARP")
    on("SAXONC")
  }
}
