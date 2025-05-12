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
val saxonMajorVersion = saxonBuild.version.get().split('.').first()

val documentation by configurations.creating {
  extendsFrom(configurations["implementation"])
}

val javaSourcesBucket by configurations.dependencyScope("javaSourcesBucket")
val javaSources by configurations.resolvable("javaSources") {
    extendsFrom(javaSourcesBucket)
}

val javadocBucket by configurations.dependencyScope("javadocBucket")
val javadocSources = configurations.resolvable("javadocSources") {
  extendsFrom(javadocBucket)
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

val dotnetDocHtmlBucket by configurations.dependencyScope("dotnetDocHtmlBucket")
val dotnetDocHtml by configurations.resolvable("dotnetDocHtml") {
  extendsFrom(dotnetDocHtmlBucket)
}

val userdocViewerAppJSBucket by configurations.dependencyScope("userdocViewerAppJSBucket")
val userdocViewerAppJS by configurations.resolvable("userdocViewerAppJS") {
  extendsFrom(userdocViewerAppJSBucket)
}

dependencies {
  javaSourcesBucket(files(layout.projectDirectory.files("../java/src/main/java")))

  javadocBucket(project(mapOf("path" to ":java",
                              "configuration" to "javadocArtifacts")))

  testDriversBucket(project(mapOf("path" to ":java",
                                  "configuration" to "testDriversArtifacts")))
  saxoncApiDocBucket(project(mapOf("path" to ":documentation",
                                   "configuration" to "saxoncApiDocArtifacts")))
  featureKeysBucket(project(mapOf("path" to ":java:keys",
                                  "configuration" to "featureKeys")))
  dotnetDocHtmlBucket(project(mapOf("path" to ":documentation",
                                  "configuration" to "dotnetDocHtmlArtifacts")))
  userdocViewerAppJSBucket(project(mapOf("path" to ":documentation",
                                  "configuration" to "userdocViewerAppJS")))
  documentation(project(":java", "runtimeElements"))
}

val resources = tasks.register<Zip>("resources") {
  group = "Internal"
  destinationDirectory = layout.buildDirectory
  archiveFileName = "saxon-resources.zip"
  from(layout.buildDirectory.dir("stage"))
  into("/")
}

val stageTestDriverSources = tasks.register<Copy>("stageTestDriverSources") {
  group = "Internal"
  from(javaSources)
  into(layout.buildDirectory.dir("stage/drivers/java"))
  include("net/sf/saxon/testdriver/**")
  include("com/saxonica/testdriver/**")
}
resources { dependsOn(stageTestDriverSources) }

val stageTestDriverClasses = tasks.register<Copy>("stageTestDriverClasses") {
  group = "Internal"
  from(testDrivers.elements.map { it.first().asFile })
  into(layout.buildDirectory.dir("stage/drivers/java/classes"))
  include("com/**")
  include("net/**")
}
resources { dependsOn(stageTestDriverClasses) }

val stageSaxoncApiDocZip = tasks.register<Zip>("stageSaxoncApiDoc") {
  group = "Internal"
  destinationDirectory = layout.buildDirectory.dir("stage")
  archiveFileName = "saxonc-userdoc.zip"
  from(saxoncApiDoc.elements.map { it.first().asFile })
  into("/")

  from(layout.projectDirectory.dir("../documentation/src/docs/saxonc-doc")) {
    exclude("xsl/*.xsl")
  }
  into("/")
}
resources { dependsOn(stageSaxoncApiDocZip) }

val stageSourceUserdoc = tasks.register<Zip>("stageSourceUserdoc") {
  group = "Internal"
  destinationDirectory = layout.buildDirectory.dir("stage")
  archiveFileName = "source-userdoc.zip"
  from(userdocViewerAppJS.elements.map { it.first().asFile })
  into("/viewer_appJS")
  exclude("javadoc-xml/out")
}
resources { dependsOn(stageSourceUserdoc) }

val stageNotices = tasks.register<Copy>("stageNotices") {
  group = "Internal"
  from(layout.projectDirectory.dir("../READMEs/src/notices"))
  into(layout.buildDirectory.dir("stage/notices"))
  include("JAMESCLARK.txt")
  include("ICU-J.txt")
  include("JLINE2.txt")
  include("LICENSE.txt")
}
resources { dependsOn(stageNotices) }

val stageSamples = tasks.register<Copy>("stageSamples") {
  group = "Internal"
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
  group = "Internal"
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
  group = "Internal"
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

val stageDotNetDocHtml = tasks.register<Copy>("stageDotNetDocHtml") {
  group = "Internal"
  from(dotnetDocHtml)
  into(layout.buildDirectory.dir("stage/doc/dotnetdoc"))
}
resources { dependsOn(stageDotNetDocHtml) }

val stageJavadocHtml = tasks.register<Copy>("stageJavadocHtml") {
  group = "Internal"
  from(javadocSources)
  into(layout.buildDirectory.dir("stage/doc/javadoc"))
}
resources { dependsOn(stageJavadocHtml) }

val stageDocumentationResources = tasks.register<Copy>("stageDocumentationResources") {
  group = "Internal"
  from(layout.projectDirectory.dir("../documentation/src/main/resources"))
  into(layout.buildDirectory.dir("stage/doc"))
}
resources { dependsOn(stageDocumentationResources) }

// FIXME: sample classes

val readme = tasks.register<JavaExec>("readme") {
  group = "Internal"
  inputs.files(layout.projectDirectory.file("src/main/resources/readme.xsl"))
  outputs.files(layout.buildDirectory.file("stage/readme.txt"))

  classpath = configurations.named("documentation").get()
  mainClass = "com.saxonica.Transform"
  args("-xsl:${layout.projectDirectory.file("src/main/resources/readme.xsl")}",
       "-it:main",
       "-o:${layout.buildDirectory.file("stage/readme.txt").get()}")
}
resources { dependsOn(readme) }
