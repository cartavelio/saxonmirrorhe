import com.saxonica.build.LifecycleTasks
import org.apache.tools.ant.filters.ReplaceTokens
import org.gradle.kotlin.dsl.filter

plugins {
  base
  id("saxon-build")
}

val extension = project.extensions.create<SaxonCPhpExtension>("saxoncPhp")

val libsaxoncReleaseArtifactsBucket by configurations.dependencyScope("libsaxoncReleaseArtifactsBucket")
val libsaxoncReleaseArtifactsDir by configurations.resolvable("libsaxoncReleaseArtifactsDir") {
  extendsFrom(libsaxoncReleaseArtifactsBucket)
}
val libsaxoncDebugArtifactsBucket by configurations.dependencyScope("libsaxoncDebugArtifactsBucket")
val libsaxoncDebugArtifactsDir by configurations.resolvable("libsaxoncDebugArtifactsDir") {
  extendsFrom(libsaxoncDebugArtifactsBucket)
}
val libsaxoncCoreReleaseArtifactsBucket by configurations.dependencyScope("libsaxoncCoreReleaseArtifactsBucket")
val libsaxoncCoreReleaseArtifactsDir by configurations.resolvable("libsaxoncCoreReleaseArtifactsDir") {
  extendsFrom(libsaxoncCoreReleaseArtifactsBucket)
}
val libsaxoncCoreDebugArtifactsBucket by configurations.dependencyScope("libsaxoncCoreDebugArtifactsBucket")
val libsaxoncCoreDebugArtifactsDir by configurations.resolvable("libsaxoncCoreDebugArtifactsDir") {
  extendsFrom(libsaxoncCoreDebugArtifactsBucket)
}

dependencies {
  libsaxoncReleaseArtifactsBucket(extension.edition.map { edition ->
    project(path = ":libsaxonc:${edition}", configuration = "releaseArtifactsDir")
  })
  libsaxoncDebugArtifactsBucket(extension.edition.map { edition ->
    project(path = ":libsaxonc:${edition}", configuration = "debugArtifactsDir")
  })
  libsaxoncCoreReleaseArtifactsBucket(extension.edition.map { edition ->
    project(path = ":libsaxonc-core:${edition}", configuration = "releaseCMakePackageDir")
  })
  libsaxoncCoreDebugArtifactsBucket(extension.edition.map { edition ->
    project(path = ":libsaxonc-core:${edition}", configuration = "debugCMakePackageDir")
  })
}

val srcFolder = project.layout.projectDirectory.dir("../src")
val releaseBuildFolder = project.layout.buildDirectory.dir("release")

val saxonBuild = the<SaxonBuildExtension>()

val phpizeOutputs = arrayOf("configure", "config.h.in", "configure.ac", "run-tests.php")
val phpizeBuildOutputs = arrayOf("ax_check_compile_flag.m4", "ax_gcc_func_attribute.m4", "config.guess", "config.sub",
  "gen_stub.php", "libtool.m4", "ltmain.sh", "Makefile.global", "php_cxx_compile_stdcxx.m4", "php.m4", "pkg.m4",
  "shtool")
val configureOutputs = arrayOf("config.h", "config.log", "config.nice", "config.status", "configure~",
  "libtool", "Makefile", "Makefile.fragments", "Makefile.objects"
)

val phpizeTask = tasks.register<Exec>("phpize") {
  inputs.file(layout.projectDirectory.file("config.m4"))
  inputs.dir(srcFolder)
  inputs.file(layout.projectDirectory.file("../saxon-config-base.m4"))

  outputs.files(layout.projectDirectory.files(phpizeOutputs))
  outputs.files(layout.projectDirectory.dir("build").files(phpizeBuildOutputs))
  outputs.dir(layout.projectDirectory.dir("autom4te.cache"))

  workingDir = layout.projectDirectory.asFile
  executable("phpize")
}

abstract class PhpDefaultWithExec: DefaultTask() {
  @get:Inject
  abstract val ex: ExecOperations

  @get:Inject
  abstract val fs: FileSystemOperations
}

val phpConfigureTask = tasks.register<PhpDefaultWithExec>("configure") {
  inputs.files(phpizeTask)
  inputs.files(libsaxoncReleaseArtifactsDir)
  inputs.files(libsaxoncCoreReleaseArtifactsDir)
  outputs.files(layout.projectDirectory.files(configureOutputs))
  outputs.dir(layout.projectDirectory.dir("modules"))

  doLast {
    val libsaxoncDir = libsaxoncReleaseArtifactsDir.elements.get().first().asFile
    val libsaxoncCoreDir = libsaxoncCoreReleaseArtifactsDir.elements.get().first().asFile

    ex.exec {
      workingDir = layout.projectDirectory.asFile
      executable("./configure")

      environment("SAXON_VERSION" to saxonBuild.version.get())
      args("--with-saxonc=${libsaxoncDir}", "--with-saxonc-core=${libsaxoncCoreDir}")
    }
  }
}

val makeTask = tasks.register<Exec>("make") {
  inputs.files(phpConfigureTask)
  outputs.files(layout.projectDirectory.files("saxon.la", ".dep", ".loT"))
  outputs.dir(layout.projectDirectory.dir(".libs"))

  workingDir = layout.projectDirectory.asFile
  executable("make")
}

val makeInstall = tasks.register<Exec>("makeInstall") {
  dependsOn(makeTask)
  workingDir = layout.projectDirectory.asFile
  executable("make")
  args("install")
}

val makeCleanTask = tasks.register<PhpDefaultWithExec>("makeClean") {
  doLast {
    if (layout.projectDirectory.file("Makefile").asFile.exists()) {
      ex.exec {
        workingDir = layout.projectDirectory.asFile
        executable("make")
        args("clean")
      }
    }

    val lotFile = layout.projectDirectory.file(".loT").asFile
    if (lotFile.exists()) {
      fs.delete {
        delete(lotFile)
      }
    }
  }
}

val phpizeCleanTask = tasks.register<Exec>("phpizeClean") {
  workingDir = layout.projectDirectory.asFile
  executable("phpize")
  args("--clean")

  mustRunAfter(makeCleanTask)
}

tasks.named(LifecycleBasePlugin.CLEAN_TASK_NAME) {
  dependsOn(phpizeCleanTask, makeCleanTask)
}

val releaseFolderTask = tasks.register<Copy>("release_folder") {
  into(releaseBuildFolder)

  from(project.layout.projectDirectory.dir("../")) {
    include("README.md")
  }

  into("src") {
    from(srcFolder) {
      exclude("20-saxon.ini", "php8_saxon.cpp")
    }
    from(srcFolder) {
      include("php8_saxon.cpp")
      filter<ReplaceTokens>("tokens" to mapOf(
        "version" to saxonBuild.version.get(),
      ))
    }
  }

  into("samples") {
    from(project.layout.projectDirectory.dir("../samples"))
  }
}

val configM4Task = tasks.register("config_m4") {
  val prelims = project.layout.projectDirectory.file("config.m4")
  val body = project.layout.projectDirectory.file("../saxon-config-base.m4")
  val outFile = releaseBuildFolder.map { it.file("src/config.m4") }

  inputs.file(prelims)
  inputs.file(body)
  outputs.file(outFile)

  doLast {
    val configLines = mutableListOf<String>()

    prelims.asFile.useLines { lines ->
      var prelimsTakeable = true
      lines.takeWhile { line ->
        if (line == "dnl Autogenerated. Do not edit below this line") prelimsTakeable = false
        prelimsTakeable
      }.forEach { line -> configLines.add(line) }
    }

    configLines.add("SAXON_VERSION=\"${saxonBuild.version.get()}\"")

    val newExtRegex = Regex("""^(\s+)PHP_NEW_EXTENSION\(saxon, \.\./src/(.+\.cpp).+""")
    val newExtReplace: (String) -> String = { line ->
      val (indent, srcFile) = newExtRegex.find(line)!!.destructured
      "${indent}PHP_NEW_EXTENSION(saxon, ${srcFile}, \$ext_shared)"
    }
    body.asFile.useLines { lines ->
      configLines.addAll(lines.map { line ->
        if (line.matches(newExtRegex)) newExtReplace.invoke(line) else line
      })
    }

    outFile.get().asFile.printWriter().use { writer ->
      configLines.forEach { line ->
        writer.println(line)
      }
    }
  }
}

val releaseTask = tasks.named(LifecycleTasks.RELEASE_TASK_NAME) {
  dependsOn(releaseFolderTask, configM4Task)
}

val releaseFolder by configurations.consumable("releaseFolder")
releaseFolder.outgoing.artifact(releaseBuildFolder) { builtBy(releaseTask) }
