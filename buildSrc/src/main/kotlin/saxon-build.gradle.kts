import com.saxonica.build.LifecycleTasks
import com.saxonica.build.ExecutableFinder
import java.text.SimpleDateFormat
import java.util.Date
import org.gradle.internal.os.OperatingSystem
import org.gradle.kotlin.dsl.*


val extension = project.extensions.create<SaxonBuildExtension>("saxonbuild")

val version = project.findProperty("saxonVersion").toString()

extension.project.set(project)
extension.version.set(version)
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

extension.systemOs.convention(systemOs)
extension.systemOsName.convention(os)
extension.platform.convention(if (os == "mac") { "osx" } else { os })
extension.arch.convention(arch)
extension.systemOsArch.convention(if (arch == "amd64") "x86_64" else arch)
extension.dotnet.convention(exeFinder.find("dotnet",
  listOf("/usr/local/share/dotnet", "/usr/local/share/dotnet/x64")))

extension.dotnetversion.convention("6.0")

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
}

tasks.register(LifecycleTasks.DOCUMENTATION_TASK_NAME) {
  group = LifecycleTasks.DOCUMENTATION_GROUP
  description = "Generate documentation"
}
