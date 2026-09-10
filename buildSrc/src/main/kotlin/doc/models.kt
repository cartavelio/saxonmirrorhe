package com.saxonica.build.doc

import com.saxonica.build.TargetHost

open class Model

abstract class SaxonCBaseModel: Model() {
  abstract val edition: String
  abstract val host: TargetHost
  abstract val majorProductVersion: String

  val isUnix: Boolean get() = !host.isWindows
  val soExtension: String get() = when(host.platform) {
    "windows" -> "dll"
    "linux" -> "so"
    else -> "dylib"
  }
  val runtimeLinkerVar: String get() = when(host.platform) {
    "macos" -> "DYLD_LIBRARY_PATH"
    "linux" -> "LD_LIBRARY_PATH "
    else -> ""
  }
  val pathSeparator: String get() = when(host.platform) {
    "windows" -> "\\"
    else -> "/"
  }
  val EDITION: String get() = edition.uppercase()
  val productName: String get() = "SaxonC-${EDITION}"
  val libsaxonc: String get() = "libsaxonc-${edition}"
  val libsaxoncCore: String get() = "libsaxonc-core-${edition}"

  val saxonDocumentationUrl: String get() = "https://www.saxonica.com/documentation${majorProductVersion}/index.html"
  val saxonCDocumentationUrl: String get() = "https://www.saxonica.com/saxon-c/documentation${majorProductVersion}/index.html"
}

data class SaxonCReadme(
  val productVersion: String,
  override val majorProductVersion: String,
  override val edition: String,
  override val host: TargetHost,
  val releaseDirName: String,
  val releaseFileExt: String = "zip",
  val binaries: Array<String>,
) : SaxonCBaseModel() {
  val binariesSentence: String get() = if (binaries.size < 3) {
    binaries.joinToString(" and ") { it }
  } else {
    "${binaries.take(binaries.size - 1).joinToString(", ") { it }}, and ${binaries.last()}"
  }
  val releaseFileName = "${releaseDirName}.${releaseFileExt}"

  val SaxonCPackage = "SaxonC${EDITION}"
  val SaxonCCorePackage = "SaxonCCore${EDITION}"
  val SaxonCTarget = "saxonc-${edition}"
  val installPrefix = when(host.platform) {
    "windows" -> "C:\\Program Files\\Saxonica"
    else -> "/opt/saxonica"
  }
  val cmakeReleasesPath = when(host.platform) {
    "windows" -> installPrefix.replace("\\", "/")
    else -> installPrefix
  }
  val installPath = "${installPrefix}${pathSeparator}${releaseDirName}"
  val cmakeCompatInstallPath = "${installPrefix}${pathSeparator}${releaseDirName}".replace("\\", "/")
  private fun binariesPath(pkg: String, dir: String): String {
    return listOf(installPath, pkg, dir).joinToString(pathSeparator)
  }
  val SaxonCPackageBinPath = binariesPath(SaxonCPackage, "bin")
  val SaxonCPackageLibPath = binariesPath(SaxonCPackage, "lib")
  val SaxonCCorePackageBinPath = binariesPath(SaxonCCorePackage, "bin")
  val SaxonCCorePackageLibPath = binariesPath(SaxonCCorePackage, "lib")
}

data class SaxonCPhpReadme(
  val productVersion: String,
  override val majorProductVersion: String,
  override val edition: String,
  override val host: TargetHost
) : SaxonCBaseModel()
