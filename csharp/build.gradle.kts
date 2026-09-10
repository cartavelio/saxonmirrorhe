import com.saxonica.build.GatherReleaseArtifacts

val heReleaseZipsBucket by configurations.dependencyScope("heReleaseZipsBucket")
val heReleaseZips by configurations.resolvable("heReleaseZips") {
  extendsFrom(heReleaseZipsBucket)
}

val eeReleaseZipsBucket by configurations.dependencyScope("eeReleaseZipsBucket")
val eeReleaseZips by configurations.resolvable("eeReleaseZips") {
  extendsFrom(eeReleaseZipsBucket)
}


dependencies {
  eeReleaseZipsBucket(project(path = ":csharp:ee", configuration = "releaseArtifacts"))
  heReleaseZipsBucket(project(path = ":csharp:he", configuration = "releaseArtifacts"))
}

val gather = tasks.register<GatherReleaseArtifacts>("gather_release_artifacts") {
  artifacts.from(heReleaseZips)
  artifacts.from(eeReleaseZips)
}
