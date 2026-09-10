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
    edition = "pe"
    classpath = configurations.compileClasspath
}

dependencies {
    compileOnly("nu.validator:htmlparser:${findProperty("htmlparserVersion")}")
}

tasks.filteredSources {
    from(configurations.javaSources) {
        include("net/sf/saxon/option/cpp/**/*.java")
        include("net/sf/saxon/option/cpp/**/package.html")
        include("com/saxonica/functions/extfn/cpp/*.java")
        include("com/saxonica/config/NativePlatformPE.java")

        filter { if (it.contains("import com.saxonica.ee")
            || it.contains("import com.saxonica.config.ee")) "//" + it else it }
    }
}

tasks.preprocessJava {
    opts {
        off("EE", "STREAM", "SCHEMA", "OPT", "BYTECODE", "CSHARP")
        on("SAXONC", "PE")
    }
}

tasks.bundleJarDependencies {
    bundle {
        jar("htmlparser-${project.findProperty("htmlparserVersion")}.jar")
    }
}