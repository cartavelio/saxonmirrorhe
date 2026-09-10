plugins {
  `kotlin-dsl`
}

repositories {
  mavenCentral()
  gradlePluginPortal()
  maven { url = uri("https://jitpack.io") }
}

dependencies {
  implementation(libs.kotlin.gradle.plugin)
  implementation("cc.ekblad:4koma:1.2.0")
  implementation(platform("software.amazon.awssdk:bom:2.30.18"))
  implementation("software.amazon.awssdk:cloudformation")
  implementation("software.amazon.awssdk:s3")
  implementation("software.amazon.awssdk:s3-transfer-manager")
  implementation("software.amazon.awssdk:sqs")
  implementation("software.amazon.awssdk:sso")
  implementation("software.amazon.awssdk:ssooidc")
  implementation(platform("org.apache.logging.log4j:log4j-bom:2.20.0"))
  implementation("org.apache.logging.log4j:log4j-slf4j2-impl")
  implementation("org.apache.logging.log4j:log4j-1.2-api")
}

gradlePlugin {
  plugins {
//    create("saxon-build") {
//      id = "com.saxonica.build.saxon-build"
//      implementationClass = "com.saxonica.build.SaxonBuildPlugin"
//    }
    create("jar-signer") {
      id = "com.saxonica.build.jar-signer"
      implementationClass = "com.saxonica.build.JarSignPlugin"
    }
    create("saxoncs") {
      id = "com.saxonica.build.saxoncs"
      implementationClass = "com.saxonica.build.SaxonCSPlugin"
    }
    create("managebranches") {
      id = "com.saxonica.build.managebranches"
      implementationClass = "com.saxonica.build.ManageBranchesPlugin"
    }
    create("testsuites") {
      id = "com.saxonica.build.testsuites"
      implementationClass = "com.saxonica.build.TestSuitesPlugin"
    }
  }
}
