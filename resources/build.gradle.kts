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

val javaSampleClassesBucket by configurations.dependencyScope("javaSampleClassesBucket")
val javaSampleClasses by configurations.resolvable("javaSampleClasses") {
  extendsFrom(javaSampleClassesBucket)
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

val saxoncDocSefBucket by configurations.dependencyScope("saxoncDocSefBucket")
val saxoncDocSef by configurations.resolvable("saxoncDocSef") {
  extendsFrom(saxoncDocSefBucket)
}

val copySaxoncDocBucket by configurations.dependencyScope("copySaxoncDocBucket")
val copySaxoncDoc by configurations.resolvable("copySaxoncDoc") {
  extendsFrom(copySaxoncDocBucket)
}

val featureKeysSourcesBucket by configurations.dependencyScope("featureKeysSourcesBucket")
val featureKeysSources = configurations.resolvable("featureKeysSources") {
  extendsFrom(featureKeysSourcesBucket)
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

  javaSampleClassesBucket(project(mapOf("path" to ":java",
                                        "configuration" to "javaSampleClasses")))

  javadocBucket(project(mapOf("path" to ":java",
                              "configuration" to "javadocArtifacts")))

  testDriversBucket(project(mapOf("path" to ":java",
                                  "configuration" to "testDriversArtifacts")))
  saxoncApiDocBucket(project(mapOf("path" to ":documentation",
                                   "configuration" to "saxoncApiDocArtifacts")))
  saxoncDocSefBucket(project(mapOf("path" to ":documentation",
                                   "configuration" to "saxoncDocSefArtifacts")))
  copySaxoncDocBucket(project(mapOf("path" to ":documentation",
                                   "configuration" to "copySaxoncDoc")))

  featureKeysSourcesBucket(project(mapOf("path" to ":featurekeys",
                                         "configuration" to "featureKeysSources")))

  dotnetDocHtmlBucket(project(mapOf("path" to ":documentation",
                                  "configuration" to "dotnetDocHtmlArtifacts")))
  userdocViewerAppJSBucket(project(mapOf("path" to ":documentation",
                                  "configuration" to "userdocViewerAppJS")))
  documentation(project(":java", "runtimeElements"))
}

val resources = tasks.register<Zip>("resources") {
  group = "Internal"
  destinationDirectory = layout.buildDirectory
  archiveFileName = "saxon-resources-${saxonMajorVersion}.zip"
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
  dependsOn(saxoncDocSef)
  dependsOn(copySaxoncDoc)
  archiveFileName = "saxonc-userdoc.zip"
  from(saxoncApiDoc.elements.map { it.first().asFile })
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
  from(layout.projectDirectory.dir("../src/main/notices"))
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
  from(layout.projectDirectory.dir("../samples/src"))
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

val stageSampleClasses = tasks.register<Copy>("stageSampleClasses") {
  group = "Internal"
  dependsOn(javaSampleClasses)
  from(zipTree(javaSampleClasses.singleFile)) {
    exclude("classes/META-INF/**")
  }
  into(layout.buildDirectory.dir("stage/samples/java"))
}
resources { dependsOn(stageSampleClasses) }

val copy_feature_keys = tasks.register<Copy>("copy_feature_keys") {
  group = "Internal"
  into(layout.buildDirectory.dir("featureKeys"))
  from(featureKeysSources)
}

val stageConfigSchema = tasks.register<JavaExec>("stageConfigSchema") {
  group = "Internal"
  inputs.files(copy_feature_keys)
  inputs.files(layout.projectDirectory.file("../samples/src/config/config-raw.xsd"))
  outputs.files(layout.buildDirectory.file("stage/samples/config/config.xsd"))

  classpath = configurations.named("documentation").get()
  mainClass = "com.saxonica.Transform"
  args("-xsl:${layout.buildDirectory.file("featureKeys/xsl/FeatureKeysToSchema.xsl").get()}",
       "-o:${layout.buildDirectory.file("stage/samples/config/config.xsd").get()}",
       "-it:main",
       "FeatureKeys=${layout.buildDirectory.file("featureKeys/xml/FeatureKeys.xml").get()}",
       "AdditionalFeatures=${layout.buildDirectory.file("featureKeys/xml/AdditionalFeatures.xml").get()}",
       "ConfigSchema=file://${layout.projectDirectory.file("../samples/src/config/config-raw.xsd")}")
}
resources { dependsOn(stageConfigSchema) }

val stageSampleSources = tasks.register("stageSampleSources") {
  group = "Internal"
  doLast {
    copy {
      from(layout.projectDirectory.dir("../samples/src/java"))
      into(layout.buildDirectory.dir("stage/samples/java"))
    }
    copy {
      from(layout.projectDirectory.dir("../samples/src/cs"))
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

val stageResourcesDoc = tasks.register<Copy>("stageResourcesDoc") {
  group = "Internal"
  from(layout.projectDirectory.dir("src/main/resources/doc"))
  into(layout.buildDirectory.dir("stage/doc"))
}
resources { dependsOn(stageResourcesDoc) }

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
