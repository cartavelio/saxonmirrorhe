package com.saxonica.build.doc

import com.saxonica.build.TargetHost

open class Model

data class SaxonCReadme(
  val productVersion: String,
  val edition: String,
  val host: TargetHost,
  val binaries: Array<String>,
  val binDirName: String
) : Model() {
  val binariesSentence: String get() = if (binaries.size < 3) {
    binaries.joinToString(" and ") { it }
  } else {
    "${binaries.take(binaries.size - 1).joinToString(", ") { it }}, and ${binaries.last()}"
  }
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

  val EDITION = edition.uppercase()
  val productName = "SaxonC-${EDITION}"
  val libsaxonc = "libsaxonc-${edition}"
  val libsaxoncCore = "libsaxonc-core-${edition}"
  val SaxonCPackage = "SaxonC${EDITION}"
  val SaxonCCorePackage = "SaxonCCore${EDITION}"

}

