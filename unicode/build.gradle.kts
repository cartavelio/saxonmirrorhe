import com.saxonica.build.UnicodeCaseMapper
import de.undercouch.gradle.tasks.download.Download

import java.io.PrintStream

plugins {
  id("saxon-build")
  id("de.undercouch.download") version "5.6.0"
}

repositories {
  mavenLocal()
  mavenCentral()
  maven { url = uri("https://maven.saxonica.com/maven") }
}

val saxonBuild = the<SaxonBuildExtension>()
val unicodeVersion = findProperty("unicodeVersion")

val saxon by configurations.creating {}
val saxonClasspath by configurations.resolvable("saxonClasspath") {
  extendsFrom(saxon)
}

dependencies {
  saxon("net.sf.saxon:Saxon-HE:12.4")
}

val unicode = tasks.register("unicode")

val downloadUnicode = tasks.register("downloadUnicode")

for (name in listOf("UnicodeData", "Blocks", "SpecialCasing", "CaseFolding")) {
  val t = tasks.register<Download>("download${name}") {
    src("https://www.unicode.org/Public/${unicodeVersion}/ucd/${name}.txt")
    dest(layout.buildDirectory.file("cache/${name}-${unicodeVersion}.txt"))
    onlyIf { !layout.buildDirectory.file("cache/${name}-${unicodeVersion}.txt").get().getAsFile().exists() }
    doFirst {
      mkdir(layout.buildDirectory.dir("cache"))
    }
  }
  downloadUnicode { dependsOn(t) }
}

val makeBlocks = tasks.register<JavaExec>("makeBlocks") {
  dependsOn("downloadUnicode")

  inputs.file(layout.buildDirectory.file("cache/Blocks-${unicodeVersion}.txt"))
  inputs.file(layout.projectDirectory.file("tools/blocks.xsl"))
  outputs.file(layout.buildDirectory.file("unicodeBlocks.xml"))

  classpath = saxonClasspath
  mainClass = "net.sf.saxon.Transform"
  args("-it",
       "-xsl:${layout.projectDirectory.file("tools/blocks.xsl")}",
       "-o:${layout.buildDirectory.file("unicodeBlocks.xml").get()}",
       "unicodeVersion=${unicodeVersion}")
}
unicode { dependsOn(makeBlocks) }

val makeCategories = tasks.register<JavaExec>("makeCategories") {
  dependsOn("downloadUnicode")

  inputs.file(layout.buildDirectory.file("cache/UnicodeData-${unicodeVersion}.txt"))
  inputs.file(layout.projectDirectory.file("tools/categories.xsl"))
  outputs.file(layout.buildDirectory.file("categories.xml"))

  classpath = saxonClasspath
  mainClass = "net.sf.saxon.Transform"
  args("-it",
       "-xsl:${layout.projectDirectory.file("tools/categories.xsl")}",
       "-o:${layout.buildDirectory.file("categories.xml").get()}",
       "unicodeVersion=${unicodeVersion}")
}
unicode { dependsOn(makeCategories) }

val toLower = tasks.register("makeToLower") {
  dependsOn("downloadUnicode")

  inputs.file(layout.projectDirectory.file("src/ToLower.src"))
  outputs.file(layout.buildDirectory.file("ToLower.java"))

  doLast {
    val mapper = UnicodeCaseMapper("${unicodeVersion}", layout.buildDirectory.file("cache").get().getAsFile())
    mapper.makeToLower(layout.projectDirectory.file("src/ToLower.src").getAsFile(),
                       PrintStream(layout.buildDirectory.file("ToLower.java").get().getAsFile()))
  }
}
unicode { dependsOn(toLower) }

val toUpper = tasks.register("makeToUpper") {
  dependsOn("downloadUnicode")

  inputs.file(layout.projectDirectory.file("src/ToUpper.src"))
  outputs.file(layout.buildDirectory.file("ToUpper.java"))

  doLast {
    val mapper = UnicodeCaseMapper("${unicodeVersion}", layout.buildDirectory.file("cache").get().getAsFile())
    mapper.makeToUpper(layout.projectDirectory.file("src/ToUpper.src").getAsFile(),
                       PrintStream(layout.buildDirectory.file("ToUpper.java").get().getAsFile()))
  }
}
unicode { dependsOn(toUpper) }
