import java.io.BufferedReader
import java.io.File
import java.io.FileReader
import java.io.InputStreamReader
import org.gradle.api.GradleException

plugins {
  id("saxon-build")
}

val saxonHejRelease by configurations.dependencyScope("saxonHejRelease")
val saxonPejRelease by configurations.dependencyScope("saxonPejRelease")
val saxonEejRelease by configurations.dependencyScope("saxonEejRelease")

val saxonHecRelease by configurations.dependencyScope("saxonHecRelease")
val saxonPecRelease by configurations.dependencyScope("saxonPecRelease")
val saxonEecRelease by configurations.dependencyScope("saxonEecRelease")

val saxonCsEeRelease by configurations.dependencyScope("saxonCsEeRelease")

val currentBranch by configurations.consumable("currentBranch")

dependencies {
  saxonHejRelease(project(mapOf("path" to ":java:he",
                                "configuration" to "releaseArtifacts")))
  saxonPejRelease(project(mapOf("path" to ":java:pe",
                                "configuration" to "releaseArtifacts")))
  saxonEejRelease(project(mapOf("path" to ":java",
                                "configuration" to "releaseArtifacts")))

  saxonCsEeRelease(project(mapOf("path" to ":csharp:ee",
                                 "configuration" to "releaseArtifacts")))

  saxonHecRelease(project(mapOf("path" to ":libsaxonc-core:he",
                                "configuration" to "releaseArtifacts")))
  saxonPecRelease(project(mapOf("path" to ":libsaxonc-core:pe",
                                "configuration" to "releaseArtifacts")))
  saxonEecRelease(project(mapOf("path" to ":libsaxonc-core:ee",
                                "configuration" to "releaseArtifacts")))
}

val saxonHej = configurations.resolvable("saxonHej") {
  extendsFrom(saxonHejRelease)
}
val saxonPej = configurations.resolvable("saxonPej") {
  extendsFrom(saxonPejRelease)
}
val saxonEej = configurations.resolvable("saxonEej") {
  extendsFrom(saxonEejRelease)
}

val saxonCsEe = configurations.resolvable("saxonCsEe") {
  extendsFrom(saxonCsEeRelease)
}

val saxonHec = configurations.resolvable("saxonHec") {
  extendsFrom(saxonHecRelease)
}
val saxonPec = configurations.resolvable("saxonPec") {
  extendsFrom(saxonPecRelease)
}
val saxonEec = configurations.resolvable("saxonEec") {
  extendsFrom(saxonEecRelease)
}

// ============================================================

val currentBranchTask = tasks.register("currentBranch") {
  outputs.files(layout.buildDirectory.file("current-branch"))

  doLast {
    // Where is git?
    var git: String? = null
    System.getenv("PATH").split(System.getProperty("path.separator")).forEach { dir ->
      if (git == null) {
        val fn = File(dir + "/git")
        if (fn.exists() && !fn.isDirectory() && fn.canExecute()) {
          git = fn.toString()
        } else {
          val wfn = File(dir + "\\git.exe")
          if (wfn.exists() && wfn.canExecute()) {
            git = wfn.toString()
          }
        }
      }
    }

    if (git == null) {
      throw GradleException("Cannot find git on PATH!?")
    }

    val pb = ProcessBuilder().command(git, "rev-parse", "--abbrev-ref", "HEAD")
    pb.directory(project.projectDir)
    val proc = pb.start()
    proc.waitFor()

    if (proc.exitValue() != 0) {
      throw GradleException("Git failed to return current branch!?")
    }

    val currentBranch = InputStreamReader(proc.inputStream).use { instream ->
      BufferedReader(instream).use { reader ->
        reader.readLine()
      }
    }

    val currentBranchFile = project.layout.buildDirectory.file("current-branch").get().asFile
    project.mkdir(project.layout.buildDirectory)
    currentBranchFile.writeText(currentBranch)
  }
}

currentBranch.outgoing.artifact(currentBranchTask)

// ============================================================

tasks.register("clean") {
  doLast {
    delete(layout.buildDirectory)
  }
}

val java = tasks.register<Copy>("javaReleases") {
  into(layout.buildDirectory.dir("releases"))
  from(saxonHej)
  from(saxonPej)
  from(saxonEej)
}

val cpp = tasks.register<Copy>("cppReleases") {
  into(layout.buildDirectory.dir("releases"))
  from(saxonHec)
  from(saxonPec)
  from(saxonEec)
}

val csharp = tasks.register<Copy>("dotnetReleases") {
  into(layout.buildDirectory.dir("releases"))
  from(saxonCsEe)
}

tasks.register("allReleases") {
  dependsOn(java)
  dependsOn(cpp)
  dependsOn(csharp)
}

