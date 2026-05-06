package com.saxonica.build.doc

import com.saxonica.build.TargetHost

open class Model

data class SaxonCReadme(
  val productVersion: String,
  val edition: String,
  val host: TargetHost,
  val releaseDirName: String,
  val releaseFileExt: String = "zip",
  val binaries: Array<String>,
) : Model() {
  val binariesSentence: String get() = if (binaries.size < 3) {
    binaries.joinToString(" and ") { it }
  } else {
    "${binaries.take(binaries.size - 1).joinToString(", ") { it }}, and ${binaries.last()}"
  }
  val releaseFileName = "${releaseDirName}.${releaseFileExt}"
  val isUnix = !host.isWindows
  val soExtension = when(host.platform) {
    "windows" -> "dll"
    "linux" -> "so"
    else -> "dylib"
  }
  val runtimeLinkerVar = when(host.platform) {
    "macos" -> "DYLD_LIBRARY_PATH"
    "linux" -> "LD_LIBRARY_PATH "
    else -> ""
  }
  val pathSeparator = when(host.platform) {
    "windows" -> "\\"
    else -> "/"
  }
  val EDITION = edition.uppercase()
  val productName = "SaxonC-${EDITION}"
  val libsaxonc = "libsaxonc-${edition}"
  val libsaxoncCore = "libsaxonc-core-${edition}"
  val SaxonCPackage = "SaxonC${EDITION}"
  val SaxonCCorePackage = "SaxonCCore${EDITION}"
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
  val SaxonCCorePackageLibPath = binariesPath(SaxonCCorePackage, "lib")
}

