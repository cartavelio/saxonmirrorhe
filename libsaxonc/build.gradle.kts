import com.saxonica.build.GatherReleaseArtifacts

plugins {
  base
}

val heReleaseZipsBucket by configurations.dependencyScope("heReleaseZipsBucket")
val heReleaseZips by configurations.resolvable("heReleaseZips") {
  extendsFrom(heReleaseZipsBucket)
}

val peReleaseZipsBucket by configurations.dependencyScope("peReleaseZipsBucket")
val peReleaseZips by configurations.resolvable("peReleaseZips") {
  extendsFrom(peReleaseZipsBucket)
}

val eeReleaseZipsBucket by configurations.dependencyScope("eeReleaseZipsBucket")
val eeReleaseZips by configurations.resolvable("eeReleaseZips") {
  extendsFrom(eeReleaseZipsBucket)
}

dependencies {
  heReleaseZipsBucket(project(path = ":libsaxonc:he", configuration = "releaseZips"))
  peReleaseZipsBucket(project(path = ":libsaxonc:pe", configuration = "releaseZips"))
  eeReleaseZipsBucket(project(path = ":libsaxonc:ee", configuration = "releaseZips"))
}

val gather = tasks.register<GatherReleaseArtifacts>("gather_release_artifacts") {
  artifacts.from(heReleaseZips)
  artifacts.from(peReleaseZips)
  artifacts.from(eeReleaseZips)
}
