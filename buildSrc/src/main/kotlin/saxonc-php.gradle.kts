plugins {
  base
  id("saxon-build")
}

val srcFolder = project.layout.projectDirectory.dir("../src")

val releaseFolderTask = tasks.register<Copy>("release_folder") {
  into(project.layout.buildDirectory.dir("release"))

  from(project.layout.projectDirectory.dir("../")) {
    include("README.md")
  }

  into("src") {
    from(srcFolder) {
      exclude("20-saxon.ini", "config.m4", "config.w32")
    }
    from(project.layout.projectDirectory) {
      include("config.m4")
      filter { line ->
        if (line == "m4_include([../saxon-config-base.m4])") "m4_include([saxon-config-base.m4])" else line
      }
    }
    from(project.layout.projectDirectory.dir("../")) {
      include("saxon-config-base.m4")
    }
  }

  into("samples") {
    from(project.layout.projectDirectory.dir("../samples"))
  }
}

val releaseFolder by configurations.consumable("releaseFolder")
releaseFolder.outgoing.artifact(releaseFolderTask.map { it.destinationDir }) { builtBy(releaseFolderTask) }
