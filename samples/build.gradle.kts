import com.saxonica.build.LifecycleTasks
import java.io.PrintStream
import java.io.FileOutputStream

plugins {
  id("java-library")
  id("saxon-build")
}

repositories {
  mavenLocal()
  mavenCentral()
  maven { url = uri("https://maven.saxonica.com/maven") }
}

val saxonBuild = the<SaxonBuildExtension>()

val samplesBuild by configurations.creating {
  extendsFrom(configurations["implementation"])
}

val sampleSources by configurations.consumable("sampleSources")
val configXsd by configurations.consumable("configXsd")

val featureKeysSourcesBucket by configurations.dependencyScope("featureKeysSourcesBucket")
val featureKeysSources = configurations.resolvable("featureKeysSources") {
  extendsFrom(featureKeysSourcesBucket)
}

val javaSampleSourcesConfiguration by configurations.creating {}
val javaSampleSources by configurations.consumable("javaSampleSources") {
  extendsFrom(javaSampleSourcesConfiguration)
}

dependencies {
  samplesBuild(project(":java", "runtimeElements"))
  samplesBuild(files(saxonBuild.saxonLicenseDir))

  javaSampleSourcesConfiguration(files(layout.projectDirectory.dir("src/java")))

  implementation("xom:xom:1.3.5")
  implementation("org.jdom:jdom:1.1.3")
  implementation("org.jdom:jdom2:2.0.6.1")
  implementation("dom4j:dom4j:1.6.1")
  implementation("nu.validator:htmlparser:${findProperty("htmlparserVersion")}")
  //implementation("javax.mail:javax.mail-api:${findProperty("mailVersion")}")
  //implementation("com.sun.mail:javax.mail:${findProperty("mailVersion")}")
  implementation("org.apache.ws.commons.axiom:axiom:1.2.15")
  implementation("org.apache.ws.commons.axiom:axiom-dom:1.2.15")
  implementation("org.apache.ws.commons.axiom:axiom-impl:1.2.15")
  implementation("org.xmlresolver:xmlresolver:${findProperty("xmlresolverVersion")}")
  //implementation("jline:jline:${findProperty("jlineVersion")}")
  //implementation("org.apache.xmlgraphics:fop:2.10")

  implementation("com.ibm.icu:icu4j:${findProperty("icu4jVersion")}")
  //implementation("com.github.javaparser:javaparser-symbol-solver-core:3.17.0")
  //implementation("com.github.javaparser:javaparser-core:3.17.0")

  featureKeysSourcesBucket(project(mapOf("path" to ":featurekeys",
                                         "configuration" to "featureKeysSources")))
}

// ============================================================

val copy_feature_keys = tasks.register<Copy>("copy_feature_keys") {
  group = "Internal"
  into(layout.buildDirectory.dir("featureKeys"))
  from(featureKeysSources)
}

val create_config_xsd = tasks.register<JavaExec>("create_config_xsd") {
  group = "Internal"
  inputs.files(layout.projectDirectory.file("src/config/saxon-attributes.xsd"))
  inputs.files(layout.projectDirectory.file("src/config/config-raw.xsd"))
  inputs.files(copy_feature_keys)
  outputs.files(layout.buildDirectory.file("config/config.xsd"))

  doFirst {
    copy {
      into(layout.buildDirectory.file("config").get())
      from(layout.projectDirectory.file("src/config")) {
        include("saxon-attributes.xsd")
      }
    }
  }

  classpath = configurations.named("samplesBuild").get()
  mainClass = "com.saxonica.Transform"
  args("-xsl:${layout.buildDirectory.file("featureKeys/xsl/FeatureKeysToSchema.xsl").get()}",
       "-o:${layout.buildDirectory.file("config/config.xsd").get()}",
       "-it:main",
       "FeatureKeys=${layout.buildDirectory.file("featureKeys/xml/FeatureKeys.xml").get()}",
       "AdditionalFeatures=${layout.buildDirectory.file("featureKeys/xml/AdditionalFeatures.xml").get()}",
       "ConfigSchema=file://${layout.projectDirectory.file("src/config/config-raw.xsd")}")
}
configXsd.outgoing.artifact(create_config_xsd)

val validate_ee_config = tasks.register<JavaExec>("validate_ee_config") {
  group = "Internal"
  description = "Validate the config.xml sample configuration file"

  inputs.files(create_config_xsd)
  inputs.files(layout.projectDirectory.file("src/config/config.xml"))
  outputs.files(layout.buildDirectory.file("ee_validated.xml"))

  classpath = configurations.named("samplesBuild").get()
  mainClass = "com.saxonica.Validate"

  args("-s:${layout.projectDirectory.file("src/config/config.xml")}",
       "-xsd:${create_config_xsd.get().outputs.files.singleFile}",
       "-report:${layout.buildDirectory.file("ee_validated.xml").get()}")
}

val validate_he_config = tasks.register<JavaExec>("validate_he_config") {
  group = "Internal"
  description = "Validate the config.xml sample configuration file"

  inputs.files(create_config_xsd)
  inputs.files(layout.projectDirectory.file("src/config/config-HE.xml"))
  outputs.files(layout.buildDirectory.file("he_validated.xml"))

  classpath = configurations.named("samplesBuild").get()
  mainClass = "com.saxonica.Validate"

  args("-s:${layout.projectDirectory.file("src/config/config-HE.xml")}",
       "-xsd:${create_config_xsd.get().outputs.files.singleFile}",
       "-report:${layout.buildDirectory.file("he_validated.xml").get()}")
}

/*
val run_with_config_xsd = tasks.register<JavaExec>("run_with_config_xsd") {
  group = "Internal"
  inputs.files(layout.projectDirectory.file("src/config/saxon-attributes.xsd"))
  inputs.files(layout.projectDirectory.file("src/config/config-raw.xsd"))
  inputs.files(copy_feature_keys)
  outputs.files(layout.buildDirectory.file("config/config.xsd"))

  doFirst {
    copy {
      into(layout.buildDirectory.file("config").get())
      from(layout.projectDirectory.file("src/config")) {
        include("saxon-attributes.xsd")
      }
    }
  }

  classpath = configurations.named("samplesBuild").get()
  mainClass = "com.saxonica.Transform"
  args("-xsl:${layout.buildDirectory.file("featureKeys/xsl/FeatureKeysToSchema.xsl").get()}",
       "-o:${layout.buildDirectory.file("config/config.xsd").get()}",
       "-it:main",
       "FeatureKeys=${layout.buildDirectory.file("featureKeys/xml/FeatureKeys.xml").get()}",
       "AdditionalFeatures=${layout.buildDirectory.file("featureKeys/xml/AdditionalFeatures.xml").get()}",
       "ConfigSchema=file://${layout.projectDirectory.file("src/config/config-raw.xsd")}")
}
*/

val setup_sample_sources = tasks.register<Copy>("setup_samples") {
  group = "Internal"
  dependsOn("validate_ee_config", "validate_he_config")
  inputs.files(create_config_xsd)
  outputs.dir(layout.buildDirectory.dir("resources/samples"))

  into(layout.buildDirectory.dir("resources/samples"))
  from(layout.projectDirectory.dir("src")) {
    include("data/**")
    include("query/**")
    include("schemas/**")
    include("scm/**")
    include("styles/**")
  }

  doLast {
    copy {
      into(layout.buildDirectory.dir("resources/samples/config"))
      from(layout.buildDirectory.dir("config"))
    }
  }

  doLast {
    copy {
      into(layout.buildDirectory.dir("resources/samples/bible"))
      from(layout.projectDirectory.dir("../src/test/testdata/bible"))
    }
  }
}
sampleSources.outgoing.artifact(setup_sample_sources)

// ============================================================

// These tests of generated configuration files are intended to check for
// consistency between ConfigurationReader and the FeatureKeys source. If the
// tests fail, then there is some inconsistency to resolve. The generated
// configuration files are not actually included in the samples in
// saxon-resources (see run_config_samples below for testing of the sample
// configuration files.)

val configuration_tests = tasks.register("configuration_tests") {
  group = "Internal"
  // Somewhere to hang dependencies
}

for (edition in listOf("he", "pe", "ee")) {
  val EDITION = edition.uppercase()
  for (features in listOf("all", "current")) {
    val includeObsolete = if (features == "current") "false()" else "true()"
    val options = "${EDITION}-${features}"
    val t = tasks.register<JavaExec>("gen_config_file_${options}") {
      group = "Internal"
      description = "Generate test configuration file for ${EDITION}"

      inputs.files(copy_feature_keys)
      inputs.files(create_config_xsd)
      outputs.files(layout.buildDirectory.file("config/generated-config-${options}.xml"))

      classpath = configurations.named("samplesBuild").get()
      mainClass = "net.sf.saxon.Transform"
      args("-xsl:${layout.buildDirectory.file("featureKeys/xsl/FeatureKeysToConfigFile.xsl").get()}",
           "-s:${layout.buildDirectory.file("featureKeys/xml/config-base.xml").get()}",
           "-o:${layout.buildDirectory.file("config/generated-config-${options}.xml").get()}",
           "edition=${EDITION}",
           "platform=J",
           "?include-obsolete=${includeObsolete}")
    }

    val t2 = tasks.register<JavaExec>("validate_gen_config_file_${options}") {
      group = "Internal"
      dependsOn(t)
      inputs.files(create_config_xsd)
      outputs.files(layout.buildDirectory.file("config/reportfile-${options}.xml"))

      classpath = configurations.named("samplesBuild").get()
      mainClass = "com.saxonica.Validate"

      args("-s:${layout.buildDirectory.file("config/generated-config-${options}.xml").get()}",
           "-xsd:${create_config_xsd.get().outputs.files.singleFile}",
           "-report:${layout.buildDirectory.file("config/reportfile-${options}.xml").get()}")
    }

    val t3 = tasks.register<JavaExec>("run_with_gen_config_file_${options}") {
      group = "Internal"
      dependsOn(t2)
      inputs.files(t)
      inputs.files(setup_sample_sources)
      outputs.files(layout.buildDirectory.file("results/config/books-with-generated-config-${options}.html"))

      doFirst {
        mkdir("${layout.buildDirectory.file("results/config").get()}")
      }

      classpath = configurations.named("samplesBuild").get()
      /*
      if (edition != "he") {
        classpath += files("${buildDir}/libs/saxon-${edition}-sql-${saxonVersion}.jar")
      }
       */

      mainClass = "net.sf.saxon.Transform"
      args("-config:${layout.buildDirectory.file("config/generated-config-${options}.xml").get()}",
           "-s:${layout.buildDirectory.file("resources/samples/data/books.xml").get()}",
           "-xsl:${layout.buildDirectory.file("resources/samples/styles/books.xsl").get()}",
           "-o:${layout.buildDirectory.file(
                     "results/config/books-with-generated-config-${options}.html").get()}",
           "--traceListenerOutputFile:${layout.buildDirectory.file(
                                            "results/config/traceOutput-${options}.log").get()}")
    }

    val t4 = tasks.register<JavaExec>("validate_report_for_generated_config-${options}") {
      group = "Internal"
      description = "Validate the validation report for generated-config-${options}.xml"

      dependsOn(t3)
      dependsOn(setup_sample_sources)

      inputs.files(layout.buildDirectory.file("samples/config/generated-config-${options}.xml"))
      inputs.files(create_config_xsd)

      classpath = configurations.named("samplesBuild").get()
      mainClass = "com.saxonica.Validate"
      args("-s:${t2.get().outputs.files.singleFile}",
           "-xsd:${layout.buildDirectory.file("resources/samples/schemas/validation-reports.xsd").get()}")

      // All validation tests should result in "Validation successful"
      // (without -quit:off, the task will fail if validation fails)
    }
    configuration_tests { dependsOn(t4) }
  }
}
