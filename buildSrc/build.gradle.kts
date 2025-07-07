plugins {
  `kotlin-dsl`
}

repositories {
  mavenCentral()
}

dependencies {
  implementation(platform("software.amazon.awssdk:bom:2.30.18"))
  implementation("software.amazon.awssdk:cloudformation")
  implementation("software.amazon.awssdk:s3")
  implementation("software.amazon.awssdk:s3-transfer-manager")
  implementation("software.amazon.awssdk:sqs")
  implementation("software.amazon.awssdk:sso")
  implementation("software.amazon.awssdk:ssooidc")
}

//sourceCompatibility = "1.8"
//targetCompatibility = "1.8"
