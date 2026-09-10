plugins {
    id("saxon-build")
    id("com.saxonica.build.managebranches")
    id("saxonc-libsaxonc")
}

val doxygenOutput by configurations.consumable("doxygenOutput")
doxygenOutput.outgoing.artifact(tasks.named("doxygen"))

libsaxonc {
    edition = "ee"
}

val sampleSourcesBucket by configurations.dependencyScope("sampleSourcesBucket")
val sampleSources = configurations.resolvable("sampleSources") {
  extendsFrom(sampleSourcesBucket)
}

dependencies {
  sampleSourcesBucket(project(mapOf("path" to ":samples",
                                    "configuration" to "sampleSources")))
}

val copySampleResources = tasks.register<Copy>("copy_sample_sources") {
    into(layout.buildDirectory.dir("samples/resources"))
    from(sampleSources)
}

tasks.register<Exec>("samples") {
  dependsOn(copySampleResources)
  dependsOn("build")

  commandLine = listOf("python", "../../tools/run-samples.py",
                       "--edition", libsaxonc.edition.get(),
                       "--platform", "saxonc")
}
