package com.saxonica.build

import kotlinx.serialization.Serializable
import kotlinx.serialization.encodeToString
import kotlinx.serialization.json.Json
import org.gradle.internal.os.OperatingSystem

/**
 * Represents an OS ('platform') we support
 */
@Serializable
enum class Platforms(val value: String) {
  MACOS("macos"),
  LINUX("linux"),
  WINDOWS("windows");

  val msbuild: String get() {
    return when (value) {
      "macos" -> "osx"
      "windows" -> "win"
      else -> value
    }
  }

  override fun toString(): String {
    return value
  }

}

/**
 * Represents an processor architecture we support
 */
@Serializable
enum class Architectures(val value: String) {
  X86_64("x86_64"),
  ARM64("arm64");

  val msbuild: String get() {
    return when (value) {
      "x86_64" -> "x64"
      else -> value
    }
  }

  override fun toString(): String {
    return value
  }
}

/**
 * Represents a target OS / arch for building / publishing release artifacts
 *
 *
 */
@Serializable
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

    val platforms: Map<Platforms, List<TargetHost>> get() {
      return mapOf(
        Platforms.LINUX to listOf(Linux.X86_64(), Linux.Arm64()),
        Platforms.MACOS to listOf(MacOS.X86_64(), MacOS.Arm64()),
        Platforms.WINDOWS to listOf(Windows.X86_64()),
      )
    }
  }

  val platformConst: Platforms
  val architectureConst: Architectures

  val isWindows: Boolean get() = platformConst === Platforms.WINDOWS
  val isLinux: Boolean get() = platformConst === Platforms.LINUX
  val isMacOS: Boolean get() = platformConst === Platforms.MACOS

  val isX86_64: Boolean get() = architectureConst === Architectures.X86_64
  val isArm64: Boolean get() = architectureConst === Architectures.ARM64

  val platform: String get() = platformConst.value
  val architecture: String get() = architectureConst.value

  operator fun component1() = platform
  operator fun component2() = architecture

  sealed class Windows(override val architectureConst: Architectures) : TargetHost {
    override val platformConst = Platforms.WINDOWS

    class X86_64(): Windows(Architectures.X86_64)
    class Arm64(): Windows(Architectures.ARM64)
  }

  sealed class Linux(override val architectureConst: Architectures) : TargetHost {
    override val platformConst = Platforms.LINUX

    class X86_64(): Linux(Architectures.X86_64)
    class Arm64(): Linux(Architectures.ARM64)
  }

  sealed class MacOS(override val architectureConst: Architectures) : TargetHost {
    override val platformConst = Platforms.MACOS

    class X86_64(): MacOS(Architectures.X86_64)
    class Arm64(): MacOS(Architectures.ARM64)
  }
}
