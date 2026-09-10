import com.saxonica.build.php.PhpFinder
import com.saxonica.build.php.PhpExec
import de.undercouch.gradle.tasks.download.Download
import de.undercouch.gradle.tasks.download.Verify

plugins {
  base
  id("saxon-build")
  id("de.undercouch.download") version "5.6.0"
}

val thePhpExe = PhpFinder(providers).php

val composerSetupSha = "c8b085408188070d5f52bcfe4ecfbee5f727afa458b2573b8eaaf77b3419b0bf2768dc67c86944da1544f06fa544fd47"
val composerFile = layout.buildDirectory.file("composer-setup.php")

val composerSetupDownload = tasks.register<Download>("composer") {
  group = LifecycleBasePlugin.BUILD_GROUP
  description = "Compose for desktop builds"

  src("https://getcomposer.org/installer")
  dest(composerFile)
  onlyIfModified(true)
  useETag(true)
}

val verifyComposerSetup = tasks.register<Verify>("verify-composer") {
  group = LifecycleBasePlugin.BUILD_GROUP
  inputs.file(composerSetupDownload.map { it.outputs.files.singleFile })
  src(composerFile)
  algorithm("SHA-384")
  checksum(composerSetupSha)
}

val vendorDir = layout.buildDirectory.dir("vendor")
val composerInstallTask = tasks.register<PhpExec>("install-composer") {
  group = LifecycleBasePlugin.BUILD_GROUP
  inputs.file(verifyComposerSetup.map { composerFile.get() })
  outputs.file(vendorDir.map { it.file("composer.phar") })

  phpExe = thePhpExe
  workingDir = layout.buildDirectory
  args(composerFile.map { it.asFile.toString() })
  args(vendorDir.map { "--install-dir=${it.asFile}" })
}

val composer by configurations.consumable("composer")
composer.outgoing.artifact(composerInstallTask.map { it.outputs.files.singleFile })