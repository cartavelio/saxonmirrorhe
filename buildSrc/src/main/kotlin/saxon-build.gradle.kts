import com.saxonica.build.LifecycleTasks
import com.saxonica.build.ExecutableFinder
import com.saxonica.build.TargetHost
import com.saxonica.build.java.testsuite.JavaRunner
import java.text.SimpleDateFormat
import java.util.Date
import org.gradle.internal.os.OperatingSystem
import org.gradle.kotlin.dsl.*


val extension = project.extensions.create<SaxonBuildExtension>("saxonbuild")

val version = project.findProperty("saxonVersion").toString()

extension.project.set(project)
extension.version.set(version)

if (version.split('.').last() == "0") {
  extension.publishVersion.set(version.substring(0, version.length - 2))
} else {
  extension.publishVersion.set(version)
}

extension.majorVersion.set(version.replace("\\..+$".toRegex(), ""))
extension.releaseBuild.set(project.findProperty("releaseBuild")?.toString() == "true")
extension.saxonLicenseDir.set(project.findProperty("saxonLicenseDir")?.toString()
  ?: (System.getenv("SAXON_LICENSE_DIR")
    ?: System.getenv("HOME") + "/java"))
extension.dateStamp.set(SimpleDateFormat("yyyyMMdd").format(Date()).toString())
extension.pathSeparator.set(System.getProperty("path.separator") ?: ":")
extension.fileSeparator.set(System.getProperty("file.separator") ?: "/")

val exeFinder = ExecutableFinder(extension.pathSeparator.get(), extension.fileSeparator.get())

val systemOs = OperatingSystem.current()
val arch = System.getProperty("os.arch")

val os = (if (systemOs.isMacOsX())
  "mac" else if (systemOs.isLinux())
  "linux" else if (systemOs.isWindows())
  "windows" else "unknown")

extension.targetHost.convention(TargetHost.current())
extension.systemOs.convention(systemOs)
extension.systemOsName.convention(os)
extension.platform.convention(if (os == "mac") { "osx" } else { os })
extension.arch.convention(arch)
val systemOsArch = if (arch == "amd64") "x86_64" else arch
extension.systemOsArch.convention(systemOsArch)
extension.dotnet.convention(exeFinder.find("dotnet",
  listOf("/usr/local/share/dotnet", "/usr/local/share/dotnet/x64")))
extension.dotnetversion.convention("8.0")
extension.macosx_min_version.convention(project.findProperty("macosx_min_version_${systemOsArch}")?.toString())

tasks.register(LifecycleTasks.RELEASE_TASK_NAME) {
  group = LifecycleTasks.RELEASE_GROUP
  description = "Build release artifacts"
}

tasks.register(LifecycleTasks.DEBUG_TASK_NAME) {
  group = LifecycleTasks.DEBUG_GROUP
  description = "Build debug-enabled release artifacts"
}

tasks.register(LifecycleTasks.ACCEPTANCE_TEST_TASK_NAME) {
  group = LifecycleTasks.ACCEPTANCE_TEST_GROUP
  description = "Run acceptance tests, like the W3C/QTCG test suites"
  doLast {
    val archive = project.findProperty("saxonicaTestDriverReportArchive")?.toString()
    if (archive != null) {
      println("Archiving results: ${archive}")

      val root = File(archive)
      if (!root.exists() && root.isDirectory()) {
        root.mkdirs()
      }

      val dateStamp = extension.dateStamp.get()

      // The build directory depends on what subproject runs this task...
      var saxonRoot: String? = null
      val buildDir = "${layout.buildDirectory.get()}"
      
      var pos = buildDir.indexOf("/java/")
      if (pos > 0) {
        saxonRoot = buildDir.substring(0, pos+1)
      } else {
        pos = buildDir.indexOf("/csharp/")
        if (pos > 0) {
          saxonRoot = buildDir.substring(0, pos+1)
        }
      }

      if (saxonRoot == null) {
        println("FAILED TO PARSE: ${buildDir}")
      } else {
        val relativePath = mapOf(
            "eej" to "java/build/test-results/",
            "hej" to "java/he/build/test-results/",
            "pej" to "java/pe/build/test-results/"
        )

        for (edition in listOf("hej", "pej", "eej")) {
          val testroot = "${saxonRoot}${relativePath[edition]}"
          val copyroot = "${archive}${edition}/${version}/"
          println("${testroot}->${copyroot}")
          project.copy {
            into(copyroot)
            from(testroot) {
              include("**/${dateStamp}/**")
            }
          }
        }

        for (edition in listOf("he", "pe", "ee")) {
          val testroot = "${saxonRoot}csharp/${edition}/build/test-results/"
          val copyroot = "${archive}${edition}dotnet/${version}/"
          println("${testroot}->${copyroot}")
          project.copy {
            into(copyroot)
            from(testroot) {
              include("**/${dateStamp}/**")
            }
          }
        }
      }
    }
  }
}

tasks.register(LifecycleTasks.DOCUMENTATION_TASK_NAME) {
  group = LifecycleTasks.DOCUMENTATION_GROUP
  description = "Generate documentation"
}
