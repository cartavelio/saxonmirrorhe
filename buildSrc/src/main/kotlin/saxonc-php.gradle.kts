import com.saxonica.build.LazyExec
import com.saxonica.build.LifecycleTasks
import com.saxonica.build.TemplatedDocTask
import com.saxonica.build.php.PhpFinder
import com.saxonica.build.doc.SaxonCPhpReadme
import com.saxonica.build.php.PhpExec
import org.apache.tools.ant.filters.ReplaceTokens
import org.gradle.kotlin.dsl.filter
import org.gradle.kotlin.dsl.register

plugins {
  base
  id("saxon-build")
}

val phpFinder = PhpFinder(providers)
val extension = project.extensions.create<SaxonCPhpExtension>("saxoncPhp")
extension.phpExe.convention(phpFinder.php)
extension.phpizeExe.convention(phpFinder.phpize)
extension.makeExe.convention(phpFinder.make)

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

val phpComposerBucket by configurations.dependencyScope("phpComposerBucket")
val phpComposer by configurations.resolvable("phpComposer") {
  extendsFrom(phpComposerBucket)
}

val saxoncReleaseZipBucket by configurations.dependencyScope("saxoncReleaseZipBucket")
val saxoncReleaseZip by configurations.resolvable("saxoncReleaseZip") {
  extendsFrom(saxoncReleaseZipBucket)
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
  phpComposerBucket(project(":php", configuration = "composer"))

  saxoncReleaseZipBucket(extension.edition.map { edition ->
    project(":libsaxonc:${edition}", configuration = "releaseZips")
  })
}

val srcFolder = project.layout.projectDirectory.dir("../src")
val releaseBuildFolder = project.layout.buildDirectory.dir("release")

val saxonBuild = the<SaxonBuildExtension>()

val phpBuildRoot = project.layout.buildDirectory.dir("build-root")
val phpExtensionLibDir = project.layout.buildDirectory.dir("lib")

val phpizeOutputs = arrayOf("configure", "config.h.in", "configure.ac", "run-tests.php")
val phpizeBuildOutputs = arrayOf("ax_check_compile_flag.m4", "ax_gcc_func_attribute.m4", "config.guess", "config.sub",
  "gen_stub.php", "libtool.m4", "ltmain.sh", "Makefile.global", "php_cxx_compile_stdcxx.m4", "php.m4", "pkg.m4",
  "shtool")
val configureOutputs = arrayOf("config.h", "config.log", "config.nice", "config.status", "configure~",
  "libtool", "Makefile", "Makefile.fragments", "Makefile.objects"
)

val buildRootInputs = layout.projectDirectory.files("config.m4", "../saxon-config-base.m4", "../composer.json")
val createPhpBuildRoot = tasks.register<Copy>("create-build-root") {
  inputs.files(buildRootInputs)
  outputs.files(phpBuildRoot.map { it.files("config.m4", "saxon-config-base.m4", "saxon-config-extdir.m4", "composer.json") })

  into(phpBuildRoot)
  from(buildRootInputs)

  doLast {
    phpBuildRoot.get().file("saxon-config-extdir.m4").asFile.writeText("EXTENSION_DIR=\"\$SAXON_EXTENSION_DIR\"\n")
  }
}

val phpizeTask = tasks.register<LazyExec>("phpize") {
  inputs.files(createPhpBuildRoot.map { it.outputs.files }) // avoid marking the build root as an input...
  inputs.dir(srcFolder)

  outputs.files(phpBuildRoot.map { it.files(phpizeOutputs) })
  outputs.dir(phpBuildRoot.map { it.dir("build") })
  outputs.dir(phpBuildRoot.map { it.dir("autom4te.cache") })

  workingDir = phpBuildRoot
  executable(extension.phpizeExe)
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
  outputs.files(phpBuildRoot.map { it.files(configureOutputs) })

  doLast {
    val libsaxoncDir = libsaxoncReleaseArtifactsDir.elements.get().first().asFile
    val libsaxoncCoreDir = libsaxoncCoreReleaseArtifactsDir.elements.get().first().asFile

    ex.exec {
      workingDir = phpBuildRoot.get().asFile
      executable("./configure")

      environment("SAXON_VERSION" to saxonBuild.version.get(), "SAXON_EXTENSION_DIR" to phpExtensionLibDir.get().asFile)
      args("--with-saxonc=${libsaxoncDir}", "--with-saxonc-core=${libsaxoncCoreDir}")
    }
  }
}

val makeTask = tasks.register<LazyExec>("make") {
  inputs.files(phpConfigureTask)
  outputs.files(phpBuildRoot.map { it.files("saxon.la", ".dep", ".loT") })
  outputs.dir(phpBuildRoot.map { it.dir(".libs") })
  outputs.dir(phpBuildRoot.map { it.dir("modules") })

  workingDir = phpBuildRoot
  executable(extension.makeExe)
}

val makeInstallTask = tasks.register<LazyExec>("make-install") {
  inputs.files(phpConfigureTask)
  outputs.dir(phpExtensionLibDir)

  workingDir = phpBuildRoot
  executable(extension.makeExe)
  args("install")
}

val makeCleanTask = tasks.register<PhpDefaultWithExec>("make-clean") {
  doLast {
    if (phpBuildRoot.get().file("Makefile").asFile.exists()) {
      ex.exec {
        workingDir = phpBuildRoot.get().asFile
        executable(extension.makeExe.get())
        args("clean")
      }
    }

    val lotFile = phpBuildRoot.get().file(".loT").asFile
    if (lotFile.exists()) {
      fs.delete {
        delete(lotFile)
      }
    }
  }
}

val testRunnerDir = layout.buildDirectory.dir("testrunner")
val vendorDir = testRunnerDir.map { it.dir("vendor") }
val composerJson = layout.projectDirectory.file("../composer.json")
val composerLock = testRunnerDir.get().file("composer.lock")

val composerInstallTask = tasks.register<PhpExec>("composer-install") {
  inputs.file(composerJson)
  inputs.files(composerUpdateTask)
  outputs.file(vendorDir.map { it.file("bin/phpunit") })
  workingDir = testRunnerDir
  phpExe = extension.phpExe
  args(phpComposer.elements.map { it.single().asFile.toString() })
  args("install")
}

val composerUpdateTask = tasks.register<PhpExec>("composer-update") {
  inputs.file(composerJson)
  outputs.file(composerLock)
  workingDir = testRunnerDir
  phpExe = extension.phpExe
  args(phpComposer.elements.map { it.single().asFile.toString() })
  args("update")

  doFirst {
    project.copy {
      into(testRunnerDir)
      from(composerJson)
    }
  }

  doLast {
    project.copy {
      into(layout.buildDirectory)
      from(phpBuildRoot.map { it.file("composer.lock") })
    }
  }
}

private fun libpath(configuration: ResolvableConfiguration): Provider<File> {
  return configuration.elements.map {
    it.single().asFile.resolve("lib")
  }
}

val phpUnitTask = tasks.register<PhpExec>("phpunit") {
  phpExe = extension.phpExe
  workingDir = phpBuildRoot
  saxonPhpExt = makeInstallTask.map { layout.projectDirectory.dir(it.outputs.files.singleFile.toString()).file("saxon.so") }
  libsaxoncLibPath = libpath(libsaxoncReleaseArtifactsDir)
  libsaxoncCoreLibPath = libpath(libsaxoncCoreReleaseArtifactsDir)

  args(composerInstallTask.map { it.outputs.files.single().toString() })
  args(layout.projectDirectory.file("../samples/SaxonCPHPTests.php").asFile.toString())
}

val readmeDir = layout.buildDirectory.dir("release-readme")
val editionReadmeTemplateDir = layout.projectDirectory.dir("README.in")

val readmeTask = tasks.register<TemplatedDocTask>("readme") {
  group = LifecycleTasks.DOCUMENTATION_GROUP
  templateRoot = layout.projectDirectory.dir("../README.in")
  if (editionReadmeTemplateDir.asFile.exists()) {
    editionTemplateRoot = editionReadmeTemplateDir
  }
  template = "README.md.peb"
  renderedDoc = readmeDir.map { it.file("README.md") }
  model = SaxonCPhpReadme(
    productVersion = saxonBuild.version.get(),
    majorProductVersion = saxonBuild.majorVersion.get(),
    edition = extension.edition.get(),
    host = saxonBuild.targetHost.get(),
  )
}

val releaseFolderTask = tasks.register<Copy>("release_folder") {
  into(releaseBuildFolder)

  from(readmeTask) {
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

    val newExtRegex = Regex("""^(\s+)PHP_NEW_EXTENSION\(saxon, (?:\.\./)+src/(.+\.cpp).+""")
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

val smoketestReleaseZip = saxoncReleaseZip.elements.map { it.single().asFile }
val smoketestDir = layout.buildDirectory.dir("smoketest")
val smoketestSrcDir = smoketestDir.map { it.dir(smoketestReleaseZip.get().nameWithoutExtension) }
val smoketestPhpBuildRoot = smoketestDir.map { it.dir("php") }

// This has to be an Exec shelling out to native unzip or the .so symlinks get trashed
val smoketestUnzip = tasks.register<Exec>("smoketest-unzip") {
  inputs.file(smoketestReleaseZip)
  outputs.dir(smoketestSrcDir)
  workingDir(smoketestDir)
  executable("unzip")
  val zipFilePath = CommandLineArgumentProvider { listOf(smoketestReleaseZip.map { it.toString() }.get()) }
  argumentProviders.add(zipFilePath)

  doFirst {
    delete(smoketestSrcDir)
  }
}

val smoketestCreatePhpBuildRoot = tasks.register<Copy>("smoketest-create-php-build-root") {
  from(smoketestUnzip.map { smoketestSrcDir.get().dir("php/src") })
  into(smoketestPhpBuildRoot)

  doFirst {
    delete(smoketestPhpBuildRoot)
  }
}

val smoketestPhpize = tasks.register<LazyExec>("smoketest-phpize") {
  inputs.files(smoketestCreatePhpBuildRoot.map { fileTree(it.destinationDir).filter {
    it.extension == "cpp"
  } }) // avoid marking the build root as an input...
  outputs.files(smoketestPhpBuildRoot.map { it.files(phpizeOutputs) })

  workingDir = smoketestPhpBuildRoot
  executable(extension.phpizeExe)
}

val smoketestLibsaxoncDir = smoketestUnzip.map { smoketestSrcDir.map { it.dir("SaxonC${extension.edition.get().uppercase()}") }.get() }
val smoketestLibsaxoncCoreDir = smoketestUnzip.map { smoketestSrcDir.map { it.dir("SaxonCCore${extension.edition.get().uppercase()}") }.get() }

val smoketestConfigure = tasks.register<PhpDefaultWithExec>("smoketest-configure") {
  inputs.files(smoketestPhpize)
  outputs.files(smoketestPhpBuildRoot.map { it.files(configureOutputs) })

  doLast {

    ex.exec {
      workingDir(smoketestPhpBuildRoot)
      executable("./configure")

      args("--with-saxonc=${smoketestLibsaxoncDir.get()}", "--with-saxonc-core=${smoketestLibsaxoncCoreDir.get()}")
    }
  }
}

val smoketestMake = tasks.register<LazyExec>("smoketest-make") {
  inputs.files(smoketestConfigure)
  outputs.file(smoketestPhpBuildRoot.map { it.file("modules/saxon.so") })

  workingDir = smoketestPhpBuildRoot
  executable(extension.makeExe)
}

val smoketestTestRunner = tasks.register<PhpExec>("smoketest-runner") {
  inputs.files(smoketestMake.map { it.outputs.files })
  phpExe = extension.phpExe
  workingDir = smoketestSrcDir.map { it.dir("php/samples") }
  saxonPhpExt = smoketestMake.map { it.outputs.files.singleFile }
  libsaxoncLibPath = smoketestLibsaxoncDir.map { it.dir("lib") }
  libsaxoncCoreLibPath = smoketestLibsaxoncCoreDir.map { it.dir("lib") }

  args("xpathExamples.php")
}

tasks.named(LifecycleTasks.SMOKETEST_TASK_NAME) {
  dependsOn(smoketestTestRunner)
}
