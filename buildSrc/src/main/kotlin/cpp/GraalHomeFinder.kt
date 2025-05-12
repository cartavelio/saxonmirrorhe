package com.saxonica.build.cpp

import org.gradle.api.GradleException
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import java.io.File
import java.nio.file.Path
import kotlin.io.path.exists
import kotlin.io.path.isDirectory

class GraalHomeFinder(val release: Property<String>) {
    fun home(): Provider<Path> {
        return release.map { edition -> findHome(edition) }
    }

    fun nativeImage(): Provider<File> {
        return release.map { release ->
            val bin = findGraalBin(findHome(release))
            var image = bin.resolve("native-image")
            if (!image.toFile().exists()) {
                // For Windows
                image = bin.resolve("native-image.cmd")
            }
            if (!image.toFile().exists()) {
                throw GradleException("Cannot build SaxonC, cannot find native-image executable in Graal bin dir <${bin}>")
            }
            image.toFile()
        }
    }

    private fun findHome(release: String): Path {
        val homeEnv = System.getenv("GRAALVM_HOME")
        return if (homeEnv == null) searchForGraalHome(release) else File(homeEnv).toPath()
    }


    private fun searchForGraalHome(release: String) : Path {
        listOf("/Library/Java/JavaVirtualMachines", "/lib/jvm", "/opt", "C:/GraalVM").forEach { root ->
            val rdir = File(root).toPath()
            if (rdir.exists() && rdir.isDirectory()) {
                rdir.toFile().list().forEach { fn ->
                    if (fn.startsWith(release)) {
                        return rdir.resolve(fn)
                    }
                }
            }
        }
        throw GradleException("Cannot build SaxonC, cannot find Graal ${release}")
    }

    private fun findGraalBin(home: Path): Path {
        var bin = home.resolve("bin")
        if (! bin.toFile().isDirectory()) {
            bin = home.resolve("Contents/Home/bin")
        }
        if (! bin.toFile().isDirectory()) {
            throw GradleException("Cannot build SaxonC, cannot find Graal bin dir under path <${home}>")
        }
        return bin
    }
}