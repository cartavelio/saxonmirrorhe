package com.saxonica.build

import org.gradle.internal.os.OperatingSystem

/**
 * Represents a target OS / arch for building / publishing release artifacts
 *
 *
 */
sealed interface TargetHost {
  companion object {
    fun current(): TargetHost {
      return fromName(System.getProperty("os.name").lowercase())
    }

    private fun windows(osArch: String): TargetHost {
      return when (osArch) {
        "x86_64", "amd64" -> Windows.X86_64()
        else -> Windows.Arm64()
      }
    }

    private fun macOS(osArch: String): TargetHost {
      return when (osArch) {
        "x86_64", "amd64" -> MacOS.X86_64()
        else -> MacOS.Arm64()
      }
    }

    private fun linux(osArch: String): TargetHost {
      return when (osArch) {
        "x86_64", "amd64" -> Linux.X86_64()
        else -> Linux.Arm64()
      }
    }

    fun fromName(name: String): TargetHost {
      val osArch = System.getProperty("os.arch")
      return if (name.contains("windows")) {
        windows(osArch)
      } else if (name.contains("mac os x") || name.contains("darwin" ) || name.contains("osx")) {
        macOS(osArch)
      } else {
        linux(osArch)
      }
    }

    fun fromOS(os: OperatingSystem): TargetHost {
      val osArch = System.getProperty("os.arch")
      return if (os.isWindows) windows(osArch) else if (os.isMacOsX) macOS(osArch) else linux(osArch)
    }
  }

  val name: String
  val architecture: String

  val isWindows: Boolean get() = false
  val isLinux: Boolean get() = false
  val isMacOS: Boolean get() = false

  val isX86_64: Boolean get() = false
  val isArm64: Boolean get() = false

  operator fun component1() = name
  operator fun component2() = architecture

  sealed class Windows(override val architecture: String) : TargetHost {
    override val name = "windows"
    override val isWindows = true

    class X86_64(): Windows("x86_64") {
      override val isX86_64 = true
    }

    class Arm64(): Windows("aarch64") {
      override val isArm64 = true
    }
  }

  sealed class Linux(override val architecture: String) : TargetHost {
    override val name = "linux"
    override val isLinux = true

    class X86_64(): Linux("x86_64") {
      override val isX86_64 = true
    }

    class Arm64(): Linux("aarch64") {
      override val isArm64 = true
    }
  }

  sealed class MacOS(override val architecture: String) : TargetHost {
    override val name = "macos"
    override val isMacOS = true

    class X86_64(): MacOS("x86_64") {
      override val isX86_64 = true
    }

    class Arm64(): MacOS("aarch64") {
      override val isArm64 = true
    }
  }
}