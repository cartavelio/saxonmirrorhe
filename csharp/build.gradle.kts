import com.saxonica.build.GatherReleaseArtifacts

val eeReleaseZipsBucket by configurations.dependencyScope("eeReleaseZipsBucket")
val eeReleaseZips by configurations.resolvable("eeReleaseZips") {
  extendsFrom(eeReleaseZipsBucket)
}

dependencies {
  eeReleaseZipsBucket(project(path = ":csharp:ee", configuration = "releaseArtifacts"))
}

val gather = tasks.register<GatherReleaseArtifacts>("gather_release_artifacts") {
  artifacts.from(eeReleaseZips)
}
