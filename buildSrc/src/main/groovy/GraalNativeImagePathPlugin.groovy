import org.gradle.api.DefaultTask
import org.gradle.api.GradleException
import org.gradle.api.Plugin
import org.gradle.api.Project
import org.gradle.api.file.ConfigurableFileCollection
import org.gradle.api.file.Directory
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.provider.ListProperty
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.*
import org.gradle.internal.os.OperatingSystem
import org.gradle.work.Incremental

interface GraalNativeImagePathExtension {
    Property<String> getHome()
    Property<String> getEdition()
    Property<String> getNativeImage()
}

class GraalNativeImagePathPlugin implements Plugin<Project> {
    void apply(Project project) {
        def extension = project.extensions.create('graalvm', GraalNativeImagePathExtension)

        extension.edition.convention("ce")
        extension.home.convention(extension.edition.map { findGraalHome(it, project) })
        extension.nativeImage.convention(extension.home.map {nativeImage(it) })
    }

    private String findGraalHome(String edition, Project project) {
        String home = System.getenv("GRAALVM_${edition.toUpperCase()}_HOME")
        if (home == null) {
            String prefix = "graalvm-${edition.toLowerCase()}"
            // Where might we find GraalVM?
            ["/Library/Java/JavaVirtualMachines", "/lib/jvm", "/opt", "C:/GraalVM"].each { root ->
                File rdir = new File(root)
                if (rdir.exists() && rdir.isDirectory()) {
                    rdir.list().each { fn ->
                        if (fn.startsWith(prefix) && fn.contains(project.graalVersion)) {
                            home = "${root}/${fn}"
                        }
                    }
                }
            }
        }
        if (home == null) {
            throw new GradleException("Cannot build SaxonC, cannot find Graal ${edition}")
        }
        return home
    }

    private String findGraalBin(String home) {
        File bin = new File("${home}/bin")
        if (! bin.exists()) {
            bin = new File("${home}/Contents/Home/bin")
            if (! bin.exists()) {
                return null
            }
        }
        if (bin == null) {
            throw new GradleException("Cannot build SaxonC, cannot find Graal bin dir under path <${home}>")
        }
        return bin.toString().replace("\\", "/")
    }

    private String nativeImage(String home) {
        def bin = findGraalBin(home)
        File image = new File("${bin}/native-image")
        if (!image.exists()) {
            // For Windows
            image = new File("${bin}/native-image.cmd")
        }
        if (!image.exists()) {
            throw new GradleException("Cannot build SaxonC, cannot find native-image executable in Graal bin dir <${bin}>")
        }
        return image.toString()
    }
}

abstract class GraalNativeImageTask extends DefaultTask {
    @Incremental
    @InputFile
    abstract RegularFileProperty getSourceJar()

    @Input
    abstract Property<String> getLibVersionedName()

    @Input
    abstract ListProperty<String> getSpecifiedArgs()

    @Input
    @Optional
    abstract Property<Boolean> getDebug()

    @Internal
    abstract DirectoryProperty getOutputDir()

    GraalNativeImageTask args(Object... args) {
        getSpecifiedArgs().addAll(args)
        return this;
    }

    @Internal
    final Provider<List<String>> libraryFilenames = libVersionedName.map {name ->
        platformSharedLibFileExtensions().collect { "${name}.${it}" }
    }

    @Internal
    final Provider<List<String>> headerFilenames = libVersionedName.map {name ->
        ["", "_dynamic"].collectMany {suffix ->
            ["graal_isolate", name].collect {
                "${it}${suffix}.h"
            }
        }
    }

    @OutputFiles
    final ConfigurableFileCollection sharedLibraryFiles = project.files(libraryFilenames.map { names ->
        def dir = outputDir.get()
        names.collect { name -> dir.file(name) }
    })

    @OutputFiles
    final ConfigurableFileCollection headerFiles = project.files(headerFilenames.map { names ->
        def dir = outputDir.get()
        names.collect { name -> dir.file(name) }
    })

    // This gets cast to a Provider so it's not updateable
    @Internal
    final Provider<Directory> libraryDir = outputDir

    private List<Object> generatedArgs() {
        def nativeArgs = ["-cp",
                          sourceJar.get().asFile.toString(),
                          "-o", outputDir.get().file(libVersionedName.get()),
                          "--no-fallback",
                          "-H:Path=${outputDir.get().asFile}",
                          "-H:IncludeResourceBundles=com.sun.org.apache.xerces.internal.impl.msg.SAXMessages",
                          "-H:IncludeResourceBundles=com.sun.org.apache.xerces.internal.impl.msg.XMLMessages",
                          "-H:IncludeResourceBundles=com.sun.org.apache.xerces.internal.impl.msg.XMLMessages",
                          "-H:+ReportExceptionStackTraces",
                          "-H:Log=registerResource:3",
                          "-H:+AddAllCharsets",
                          "-Dfile.encoding=UTF-8",
                          "--trace-class-initialization=javax.xml.parsers.FactoryFinder",
                          "--trace-class-initialization=net.sf.saxon.s9api.streams.Steps",
                          "--trace-class-initialization=jdk.xml.internal.SecuritySupport",
                          "--enable-url-protocols=https",
                          "--enable-url-protocols=http",
                          "--shared"]
        if (debug.getOrElse(false)) {
            nativeArgs << "-g" << "-O0"
        }
        nativeArgs
    }

    List<String> platformSharedLibFileExtensions() {
        def systemOs = OperatingSystem.current()
        systemOs.isMacOsX() ? ["dylib"] : (systemOs.isLinux() ? ["so"] : ["dll", "lib"])
    }

    @TaskAction
    void exec() {
        def firstArgs = generatedArgs()
        def remainingArgs = specifiedArgs.get()

        // project.exec {
        //     executable project.file("dumpenv.bat")
        // }

        project.exec {
            executable project.graalvm.getNativeImage().get()
            args firstArgs
            args remainingArgs
        }
    }
}