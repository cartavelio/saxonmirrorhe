import org.gradle.api.Action
import org.gradle.api.Project
import org.gradle.api.artifacts.Configuration
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.tasks.Nested
import java.io.File
import java.nio.file.Path
import com.saxonica.build.java.JarTaskBuilder
import com.saxonica.build.java.JarTaskBuilder.SaxonJarTasks
import org.gradle.api.GradleException
import org.gradle.api.provider.*
import org.gradle.internal.os.OperatingSystem

interface ManageBranchesExtension {
    abstract val currentBranch: Property<String>
}

interface SaxonBuildExtension {
    abstract val project: Property<Project>
    abstract val version: Property<String>
    abstract val majorVersion: Property<String>
    abstract val releaseBuild: Property<Boolean>
    abstract val saxonLicenseDir: Property<String>
    abstract val dateStamp: Property<String>
    abstract val pathSeparator: Property<String>
    abstract val fileSeparator: Property<String>
    abstract val systemOs: Property<OperatingSystem>
    abstract val systemOsName: Property<String>
    abstract val platform: Property<String>
    abstract val arch: Property<String>
    abstract val systemOsArch: Property<String>
    abstract val dotnet: Property<String>
    abstract val dotnetversion: Property<String>

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
}

interface SaxonCPythonExtension {
    val edition: Property<String>
    val venvBasePath: Property<String>
    val venvBaseDir: DirectoryProperty
    val pythonBinDirName: Property<String>
    val venvBinarySuffix: Property<String>
    val version: Property<String>
    val manylinuxTarget: Property<String>
}
