plugins {
    id("org.gradle.toolchains.foojay-resolver-convention") version "0.9.0"
}

dependencyResolutionManagement {
    repositories {
        mavenCentral()
    }
}

rootProject.name = "Saxon"

include("java:he")
include("java:pe")
//include("java:ee")
include("java")
include("csharp:transpiler")
include("csharp:ee")
include("libsaxonc-core:ee")
include("libsaxonc-core:pe")
include("libsaxonc-core:he")
include("libsaxonc:ee")
include("libsaxonc:pe")
include("libsaxonc:he")
include("libsaxonc")
include("php:ee")
include("php:pe")
include("php:he")
include("python")
include("python:ee")
include("python:pe")
include("python:he")
include("unicode")
include("documentation")
include("python:documentation")
include("resources")
include("samples")
include("featurekeys")
