package com.saxonica.build.python

import com.saxonica.build.Architectures
import com.saxonica.build.TargetHost
import kotlinx.serialization.Serializable
import kotlinx.serialization.json.Json

import java.io.File

@Serializable
data class PythonVersion(val version: String, val specificVersion: String = "", val unreleased: Boolean = false, val macOsMinVersions: Map<Architectures, String>) : java.io.Serializable {
  fun macOsMinVersion(targetHost: TargetHost): String {
    return macOsMinVersion(targetHost.architectureConst)
  }

  fun macOsMinVersion(arch: Architectures): String {
    return macOsMinVersions.get(arch)!!
  }

  val hasReleaseArtifact = !unreleased

  val buildName: String get() = if (specificVersion == "") version else specificVersion
}

class PythonVersions(val releaseJson: File) {
  val supported: List<PythonVersion> by lazy {
    Json.decodeFromString<List<PythonVersion>>(releaseJson.readText())
  }

  val released: List<PythonVersion> get() {
    return supported.filter { v -> v.hasReleaseArtifact }
  }

  fun findSupported(version: String): PythonVersion {
    return supported.find { it.version == version }!!
  }
}
