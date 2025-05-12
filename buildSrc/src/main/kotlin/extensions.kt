import com.saxonica.build.TargetHost
import com.saxonica.build.java.BranchInfo
import com.saxonica.build.java.JarTaskBuilder
import com.saxonica.build.java.JarTaskBuilder.SaxonJarTasks
import org.gradle.api.Action
import org.gradle.api.GradleException
import org.gradle.api.Project
import org.gradle.api.artifacts.Configuration
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.provider.ListProperty
import org.gradle.api.provider.MapProperty
import org.gradle.api.provider.Property
import org.gradle.api.tasks.Nested
import org.gradle.internal.os.OperatingSystem
import java.io.BufferedReader
import java.io.File
import java.io.FileInputStream
import java.io.InputStreamReader
import java.io.PrintStream
import java.nio.file.Path
import java.text.SimpleDateFormat
import java.util.*
import kotlin.collections.component1
import kotlin.collections.component2
import kotlin.collections.iterator

interface ManageBranchesExtension {
    abstract val currentBranch: Property<String>
}

interface SaxonBuildExtension {
    abstract val project: Property<Project>
    abstract val version: Property<String>
    abstract val publishVersion: Property<String>
    abstract val majorVersion: Property<String>
    abstract val releaseBuild: Property<Boolean>
    abstract val saxonLicenseDir: Property<String>
    abstract val dateStamp: Property<String>
    abstract val pathSeparator: Property<String>
    abstract val fileSeparator: Property<String>
    abstract val systemOs: Property<OperatingSystem>
    abstract val targetHost: Property<TargetHost>
    abstract val systemOsName: Property<String>
    abstract val platform: Property<String>
    abstract val arch: Property<String>
    abstract val systemOsArch: Property<String>
    abstract val dotnet: Property<String>
    abstract val dotnetversion: Property<String>
    abstract val macosx_min_version: Property<String>

    fun findJar(configuration: Configuration, jarname: String): String {
        val jar = configuration.getFiles()
            .filter { jar -> jar.toString().endsWith(jarname) }
            .elementAtOrNull(0)
        if (jar == null) {
            throw GradleException("Cannot find ${jarname} in configuration")
        }
        return jar.toString()
    }

    fun unexpectedReleaseName(name: String) {
        throw GradleException("Unexpected release: ${name}")
    }

    // "Platform neutral URI"
    fun puri(path: String): String {
      if (systemOsName.get() == "windows") {
        val fixed = path.replace("\\", "/")
        if (!fixed.startsWith("/") && fixed.length > 1 && fixed[1] == ':') {
          return "file:/${fixed}"
        }
        return fixed
      }
      return path
    }

    fun findGraalBin(edition: String): String? {
        val graalVersion = project.get().findProperty("graalVersion").toString()
        var home = System.getenv("GRAALVM_${edition.uppercase()}_HOME")
        if (home == null) {
            val prefix = "graalvm-${edition.lowercase()}"
            listOf("/Library/Java/JavaVirtualMachines", "/lib/jvm", "/opt", "C:/GraalVM").forEach { root ->
                val rdir = File(root)
                if (rdir.exists() && rdir.isDirectory()) {
                    rdir.list().forEach { fn ->
                        if (fn.startsWith(prefix) && fn.contains(graalVersion)) {
                            home = "${root}/${fn}"
                        }
                    }
                }
            }
        }

        if (home != null) {
            val bin = if (File("${home}/bin").exists()) {
                File("${home}/bin")
            } else {
                File("${home}/Contents/Home/bin")
            }

            if (bin.exists()) {
                return bin.toString().replace("\\", "/")
            }
        }

        return null
    }

    fun graalNativeImage(edition: String): String {
        val bin = findGraalBin(edition)
        if (bin == null) {
            throw GradleException("Cannot find Graal VM binaries")
        }
        val image = File("${bin}/native-image")
        if (image.exists()) {
            return image.toString()
        } else {
            if (!image.exists()) {
                val wimage = File("${bin}/native-image.cmd")
                if (wimage.exists()) {
                    return wimage.toString()
                }
            }
            throw GradleException("Cannot find GraalVM native image compiler")
        }
    }
}

interface SaxonJExtension {
    abstract val edition: Property<String>
    abstract val version: Property<String>
    abstract val metaInfServices: MapProperty<String, String>
    abstract val manifestMainClass: Property<String>
    abstract val sourceRoot: DirectoryProperty
    abstract val jarManifestClasspath: ListProperty<String>
    abstract val jarManifestClasspathDefaults: ListProperty<String>
    abstract val editionJarInclusions: ListProperty<String>
    abstract val editionJarExclusions: ListProperty<String>

    fun jarClasspath(vararg entries: String): List<String> {
        return jarManifestClasspathDefaults.get().plus(entries)
    }

    fun jarTaskBuilder(project: Project): (String, JarTaskBuilder.() -> Unit) -> SaxonJarTasks {
        return JarTaskBuilder.createBuilder(project, this)
    }

    fun editionJarTasks(project: Project, configuration: JarTaskBuilder.() -> Unit): SaxonJarTasks {
        return JarTaskBuilder.createEditionBuilder(project, this).invoke(configuration)
    }

    fun patchReleaseInfo(src: File, saxonVersion: String, majorDate: String) {
        val (saxonMajor, saxonMinor, saxonPatch) = saxonVersion.split(".")
        val minorDate = SimpleDateFormat("yyyy-MM-dd").format(Date())

        // Trim to 7 digits so that it will always fit in an int.
        val hash = BranchInfo.currentCommitHash(src.parentFile).substring(0, 7)

        val replacements = mapOf<Regex, String>(
            "\\d+; // @@MAJOR_VERSION@@.*$".toRegex() to "${saxonMajor};",
            "\\d+; // @@MINOR_VERSION@@.*$".toRegex() to "${saxonMinor};",
            "\\d+; // @@PATCH_VERSION@@.*$".toRegex() to "${saxonPatch};",
            "\\d+; // @@BUILD@@.*$".toRegex() to "0x${hash};",
            "\".*\"; // @@MAJOR_DATE@@.*$".toRegex() to "\"${majorDate}\";",
            "\".*\"; // @@MINOR_DATE@@.*$".toRegex() to "\"${minorDate}\";"
        )
        val matched = mutableSetOf<Regex>()

        val lines = mutableListOf<String>()
        src.forEachLine { line ->
            var patched = line.trimEnd()
            for ((rex, repl) in replacements) {
                if (rex.containsMatchIn(line)) {
                    matched.add(rex)
                    patched = line.replace(rex, repl)
                }
            }
            lines.add(patched)
        }
        if (matched.size != replacements.size) {
            throw GradleException("Failed to replace some values in Version.java!")
        }
        PrintStream(src).use { stream ->
            for (line in lines) {
                stream.println(line)
            }
        }
    }

    fun patchKey(src: File, keyType: String, keyFilename: String?) {
        if (keyFilename == null) {
            return
        }

        if (keyType !in listOf("dsa", "ed25519")) {
            throw GradleException("Invalid key type: ${keyType}!")
        }

        val publicKey = File(keyFilename).readText()

        val lines = mutableListOf<String>()
        FileInputStream(src).use { fis ->
            InputStreamReader(fis).use { inreader ->
                BufferedReader(inreader).use { reader ->
                    var line = reader.readLine()
                    while (line != null) {
                        if (keyType == "dsa" && line.indexOf("final String PUBLIC_KEY = ") > 0) {
                            while (line.indexOf(";") < 0) {
                                line = reader.readLine()
                            }
                            line = "    private static final String PUBLIC_KEY = \"${publicKey}\";"
                        }

                        if (keyType == "ed25519" && line.indexOf("final String PUBLIC_KEY2 = ") > 0) {
                            line = "    private static final String PUBLIC_KEY2 = \"${publicKey}\";"
                        }

                        lines.add(line)
                        line = reader.readLine()
                    }
                }
            }
        }

        PrintStream(src).use { stream ->
            for (line in lines) {
                stream.println(line)
            }
        }
    }
}

interface GraalNativeImagePathExtension {
    abstract val home: Property<Path>
    abstract val release: Property<String>
    abstract val nativeImage: Property<File>
}

interface SaxonCCoreExtension {
    abstract val project: Property<Project>
    abstract val edition: Property<String>
    abstract val classpath: Property<Configuration>

    @Nested
    abstract fun getGraal(): GraalNativeImagePathExtension?

    fun graal(action: Action<in GraalNativeImagePathExtension?>) {
        action.execute(getGraal())
    }

    fun graal(): GraalNativeImagePathExtension? {
        return getGraal()
    }
}

interface SaxonCExtension {
    abstract val edition: Property<String>
    abstract val package_name: Property<String>
    abstract val package_file_name: Property<String>
}

interface SaxonCPythonExtension {
    val edition: Property<String>
    val venvBasePath: Property<String>
    val venvBaseDir: DirectoryProperty
    val pythonBinDirName: Property<String>
    val venvBinarySuffix: Property<String>
    val version: Property<String>
    val releaseVersions: ListProperty<String>
    val manylinuxTarget: Property<String>
}

interface PyenvExtension {
    val devVersion: Property<String>
    val releaseVersions: ListProperty<String>
    val debugVariant: Property<String>
    val definitionsDir: DirectoryProperty
    val pyenvRoot: DirectoryProperty
}
