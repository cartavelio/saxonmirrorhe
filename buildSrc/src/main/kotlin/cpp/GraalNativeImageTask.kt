package com.saxonica.build.cpp

import org.gradle.api.DefaultTask
import org.gradle.api.file.*
import org.gradle.api.provider.ListProperty
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.*
import org.gradle.internal.os.OperatingSystem
import org.gradle.process.ExecOperations
import javax.inject.Inject


/*
Building a shared library is a pain, because each platform has different ways to do things, different expectations,
and different requirements.

Linux:
Linux shared libraries are expected by the system to provide library and symlinks as follows:

the library is saxonc-core-he
SONAME: libsaxonc-core-he.so.12
REAL NAME: libsaxonc-core-he.so.12.5.0
LINKER NAME: libsaxonc-core-he.so

The actual library should be in the REAL NAME file, and the SONAME and LINKER NAME files are symlinks to the REAL NAME.
However the SONAME elf header in the REAL NAME .so should be the SONAME string.

macOS:
The shared libraries are again expected to be provided as a library plus symlinks. Of course, everything is a bit
different.

the library is saxon-hec

SONAME: libsaxonc-core-he.12.dylib
REAL NAME: libsaxonc-core-he.12.5.0.dylib
LINKER NAME: libsaxonc-core-he.dylib

Once again the REAL NAME file contains the library, and the others are symlinks to it. Once again The SONAME is the
legit ID included in the dylib mach-O header. However, this library ID, LC_ID_DYLIB, is the absolute filesystyem path
to the SONAME symlink. The absolute path is treated as the ID. There are special values you can replace the dir path
part with like @rpath, or @loader_path that make the macOS linker do magic things to look up where the library is, which
is useful for relocatable packages.

Windows

Windows is totally different. There's no magic version / linker location stuff with Windows.

The .dll just contains the library, and there's a .lib file which contains all the import and symbol table stuff and
that's the thing the linker uses.

By convention, there's no 'lib' prefix for the files, so:

the library is saxon-hec

.dll: saxonc-core-he.dll
.lib: saxonc-core-he.lib

Version management is left up to the user: controlling which paths get searched for .dll and .lib files.

Graal's native-image just spits out a single .dylib or .so, but does generate the .dll/.lib pair for Windows. It does
not bother with niceties like automatically generating filenames from a library name based on platform conventions, or
varying the naming conventions for library files and header files.

We would need to vary the arguments to native-image to generate the expected filenames between unices and Windows.

HOWEVER.

libsaxonc-core-he is not intended for public consumption, only for use by our libsaxonc-he (-ee, -pe). So, we should
probably use the full version (real name) for SONAME because we're never going to want to ship a new version of Saxon C
that includes an older version of libsaxonc-core-Xe.

So, we don't need to worry about generating the symlinks, but we still want the filenames to meet expectations.

Naive inclusion of the version, without tweaking behaviours of native-image between macOS, Linux, and Windows, we'd end
up with something like:

libsaxonc-core-he.12.5.0.dylib (right!)
libsaxonc-core-he.12.5.0.so (wrong!)
libsaxonc-core-he.12.5.0.dll (also wrong!)

What we want is:

libsaxonc-core-he.12.5.0.dylib
libsaxonc-core-he.so.12.5.0
saxonc-core-he.dll

But, if our aim is to ensure maximum agreement about this effectively-private dependency, we may want to include the
version in the DLL filename, because there really isn't anywhere else to put it.

That, however, makes the CMake setup for libsaxon more complex, so let's try ignoring that and doing it as above. Once
we have, say, a nuget package we can see how that stands up to real use and we may want to tweak it then.

We do need to ensure that the SONAME and DYLIB ID are correctly set. We want an @rpath using Dylib ID, and the .so.1.2.3
using SONAME.

This task will produce correctly named headers and incorrectly named binaries that have the right SONAME or Dylib ID.
 */
abstract class GraalNativeImageTask @Inject constructor(private val ex: ExecOperations): DefaultTask() {
    private val systemOs: OperatingSystem = OperatingSystem.current()

    @get:InputFile
    abstract val sourceJar: RegularFileProperty

    @get:Input
    abstract val specifiedArgs: ListProperty<String>

    @get:Input
    abstract val libName: Property<String>

    @get:Input
    abstract val version: Property<String>

    @get:Input
    abstract val majorVersion: Property<String>

    @get:Input
    @get:Optional
    abstract val debug: Property<Boolean>

    @get:Input
    @get:Optional
    abstract val macosx_min_version: Property<String>

    @get:Internal
    abstract val outputDir: DirectoryProperty

    @get:InputFile
    abstract val graalNativeImage: RegularFileProperty

    fun args(vararg args: String): GraalNativeImageTask {
        specifiedArgs.addAll(args.toList())
        return this;
    }

    @get:Internal
    val sharedLibFilename: Provider<String> = project.providers.provider {
        platformSharedLibFilename()
    }

    @get:Internal
    val importLibFilename: Provider<List<String>> = project.providers.provider {
        platformImportLibFilename().map { it }
    }

    @get:Internal
    val headerFilenames: Provider<List<String>> = libName.map { filename ->
        listOf("", "_dynamic").map { suffix ->
            listOf("graal_isolate", filename).map { name ->
                "${name}${suffix}.h"
            }
        }.flatten()
    }

    @get:OutputFiles
    val sharedLibFiles: ConfigurableFileCollection = project.files(sharedLibFilename.map { name ->
        outputDir.get().file(name)
    })

    // Only on Windows
    @get:OutputFiles
    val importLibFiles: ConfigurableFileCollection = project.files(importLibFilename.map { names ->
        names.map { name -> outputDir.get().file(name) }
    })

    @get:OutputFiles
    val headerFiles: ConfigurableFileCollection = project.files(headerFilenames.map { names ->
        val dir = outputDir.get()
        names.map { dir.file(it) }
    })

    // This gets cast to a Provider so it's not updateable
    @get:Internal
    val libraryDir: Provider<Directory> = outputDir

    private fun addSonameOptions(args: ArrayList<String>) {
        if (systemOs.isWindows) { return }
        if (systemOs.isLinux) {
            args.add("--native-compiler-options=-Wl,-soname,lib${libName.get()}.so.${majorVersion.get()}")
        } else {
            args.add("--native-compiler-options=-Wl,-install_name,@rpath/lib${libName.get()}.${majorVersion.get()}.dylib")
        }
    }
    private fun hasNativeCompilerOptions(): Boolean {
        return if (systemOs.isWindows) false else true
    }

    private fun generatedArgs(): List<Any> {
        var nativeArgs = arrayListOf<String>("-cp", sourceJar.get().toString(),
            "-o", libName.get(),
            "--no-fallback", "-H:Path=${outputDir.get().asFile}",
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
            "--shared")
        if(macosx_min_version.isPresent) {
            nativeArgs.add("-EMACOSX_DEPLOYMENT_TARGET=${macosx_min_version.get()}")
        }
        if (debug.getOrElse(false)) {
            nativeArgs.add("-g")
            nativeArgs.add("-O0")
        }
        addSonameOptions(nativeArgs)
        return nativeArgs
    }

    private fun platformSharedLibFilename(): String  {
        val basename = libName.get()
        return if (systemOs.isMacOsX) {
            "${basename}.dylib"
        } else if (systemOs.isLinux) {
            "${basename}.so"
        } else {
            "${basename}.dll"
        }
    }

    private fun platformImportLibFilename(): List<String> {
        val basename = libName.get()
        return if (systemOs.isWindows) {
            listOf("${basename}.lib")
        } else {
            return listOf()
        }
    }

    @TaskAction
    fun exec() {
        val firstArgs = generatedArgs()
        val remainingArgs = specifiedArgs.get()

        ex.exec {
            executable(graalNativeImage.get())
            args(firstArgs)
            args(remainingArgs)
        }
    }
}
