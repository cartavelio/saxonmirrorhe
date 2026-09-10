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
    edition = "ee"
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
        include("com/saxonica/functions/extfn/NativeCall.java")
    }
}

tasks.bundleFilteredSources {
    exclude("com/saxonica/config/JavaPlatformPE.java")
    exclude("com/saxonica/config/JavaPlatformEE.java")
    exclude("net/sf/saxon/option/axiom/**")
    exclude("net/sf/saxon/option/dom4j/**")
    exclude("net/sf/saxon/option/jdom/**")
    exclude("net/sf/saxon/option/jdom2/**")
    exclude("net/sf/saxon/option/xom/**")
    exclude("net/sf/saxon/option/sql/**")
    exclude("com/saxonica/ee/bytecode/**")
}

tasks.preprocessJava {
    opts {
        off("PE", "OPT", "BYTECODE", "CSHARP")
        on("SAXONC", "EE", "STREAM", "SCHEMA", "HOF", "PACKAGES")
    }
}

tasks.bundleJarDependencies {
    bundle {
        jar("htmlparser-${project.findProperty("htmlparserVersion")}.jar")
    }
}
