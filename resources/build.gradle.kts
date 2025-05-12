plugins {
  id("java-library")
  id("saxon-build")
  id("com.saxonica.build.managebranches")
}

repositories {
  mavenLocal()
  mavenCentral()
  maven { url = uri("https://maven.saxonica.com/maven") }
}

val saxonBuild = the<SaxonBuildExtension>()

val documentation by configurations.creating {
  extendsFrom(configurations["implementation"])
}

val javaSourcesBucket by configurations.dependencyScope("javaSourcesBucket")
val javaSources by configurations.resolvable("javaSources") {
    extendsFrom(javaSourcesBucket)
}

val testDriversBucket by configurations.dependencyScope("testDriversBucket")
val testDrivers by configurations.resolvable("testDrivers") {
  extendsFrom(testDriversBucket)
}

val saxoncApiDocBucket by configurations.dependencyScope("saxoncApiDocBucket")
val saxoncApiDoc by configurations.resolvable("saxoncApiDoc") {
  extendsFrom(saxoncApiDocBucket)
}

val featureKeysBucket by configurations.dependencyScope("featureKeysBucket")
val featureKeysSources = configurations.resolvable("featureKeysSources") {
  extendsFrom(featureKeysBucket)
}

dependencies {
  javaSourcesBucket(files(layout.projectDirectory.files("../java/src/main/java")))
  testDriversBucket(project(mapOf("path" to ":java",
                                  "configuration" to "testDriversArtifacts")))
  saxoncApiDocBucket(project(mapOf("path" to ":documentation",
                                   "configuration" to "saxoncApiDocArtifacts")))
  featureKeysBucket(project(mapOf("path" to ":java:keys",
                                  "configuration" to "featureKeys")))
  documentation(project(":java", "runtimeElements"))
}

val resources = tasks.register("resources") {
  // Somewhere to hang dependencies
}

val stageTestDriverSources = tasks.register<Copy>("stageTestDriverSources") {
  from(javaSources)
  into(layout.buildDirectory.dir("stage/drivers/java"))
  include("net/sf/saxon/testdriver/**")
}
resources { dependsOn(stageTestDriverSources) }

val stageTestDriverClasses = tasks.register<Copy>("stageTestDriverClasses") {
  from(testDrivers.elements.map { it.first().asFile })
  into(layout.buildDirectory.dir("stage/drivers/java/classes"))
  include("com/**")
  include("net/**")
}
resources { dependsOn(stageTestDriverClasses) }

val stageSaxoncApiDocZip = tasks.register<Zip>("stageSaxoncApiDoc") {
  destinationDirectory = layout.buildDirectory.dir("stage")
  archiveFileName = "saxonc-userdoc.zip"
  from(saxoncApiDoc.elements.map { it.first().asFile })
  into("/")
}
resources { dependsOn(stageSaxoncApiDocZip) }

val stageNotices = tasks.register<Copy>("stageNotices") {
  from(layout.projectDirectory.dir("../READMEs/src/notices"))
  into(layout.buildDirectory.dir("stage/notices"))
  include("JAMESCLARK.txt")
  include("ICU-J.txt")
  include("JLINE2.txt")
  include("LICENSE.txt")
}
resources { dependsOn(stageNotices) }

val stageSamples = tasks.register<Copy>("stageSamples") {
  into(layout.buildDirectory.dir("stage/samples"))
  from(layout.projectDirectory.dir("../src/samples"))
  include("config/config.xml")
  include("config/config-HE.xml")
  include("config/saxon-attributes.xsd")
  include("data/books.xml")
  include("data/books-invalid.xml")
  include("data/more-books.xml")
  include("data/othello.xml")
  include("data/books.xsd")
  include("data/books.dtd")
  include("data/play.dtd")
  include("query/tour.xq")
  include("query/books.xq")
  include("query/books-to-html.xq")
  include("scm/scmschema.*")
  include("schemas/validation-reports.xsd")
  include("styles/add-ids.xsl")
  include("styles/bible.xsl")
  include("styles/books.xsl")
  include("styles/books.txt")
  include("styles/play.xsl")
  include("styles/identity.xsl")
  include("styles/books-csv.xsl")
  include("styles/books-sql.xsl")
  include("styles/rename-to-lowercase.xsl")
  include("styles/total.xsl")
  include("styles/tour.xsl")
  include("styles/summarize.xsl")
}
resources { dependsOn(stageSamples) }

val stageConfigSchema = tasks.register<JavaExec>("stageConfigSchema") {
  inputs.files(featureKeysSources)
  inputs.files(layout.projectDirectory.file("../src/samples/config/config-raw.xsd"))
  outputs.files(layout.buildDirectory.file("stage/samples/config/config.xsd"))

  classpath = configurations.named("documentation").get()
  mainClass = "com.saxonica.Transform"
  args("-xsl:${layout.projectDirectory.file("tools/featureKeys/FeatureKeysToSchema.xsl")}",
       "-o:${layout.buildDirectory.file("stage/samples/config/config.xsd").get()}",
       "-it:main",
       "FeatureKeys=${layout.projectDirectory.file("../src/main/xml/FeatureKeys.xml")}",
       "AdditionalFeatures=${layout.projectDirectory.file("../src/main/xml/AdditionalFeatures.xml")}",
       "ConfigSchema=file://${layout.projectDirectory.file("../src/samples/config/config-raw.xsd")}")
}
resources { dependsOn(stageConfigSchema) }

val stageSampleSources = tasks.register("stageSampleSources") {
  doLast {
    copy {
      from(layout.projectDirectory.dir("../src/samples/java"))
      into(layout.buildDirectory.dir("stage/samples/java"))
    }
    copy {
      from(layout.projectDirectory.dir("../src/samples/cs"))
      into(layout.buildDirectory.dir("stage/samples/cs"))
    }
  }
}
resources { dependsOn(stageSampleSources) }

// FIXME: sample classes

// FIXME: readme
/*
task gen_readme(
        type: JavaExec,
        description: "Generate readme.txt for saxon-resources download"
) {
    inputs.file "${projectDir}/src/docs/resources/readme.xsl"
    outputs.file "${buildDir}/saxon-resources/readme.txt"
    classpath = configurations.documentation
    mainClass = "com.saxonica.Transform"
    args "-xsl:${projectDir}/src/docs/resources/readme.xsl",
            "-it:main",
            "-o:${buildDir}/saxon-resources/readme.txt"
}
*/
