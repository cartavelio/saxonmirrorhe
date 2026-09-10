package com.saxonica.build.cpp

import com.saxonica.build.java.JarFinder
import org.gradle.api.Action
import org.gradle.api.DefaultTask
import org.gradle.api.GradleException
import org.gradle.api.artifacts.Configuration
import org.gradle.api.file.ArchiveOperations
import org.gradle.api.file.CopySpec
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.RegularFile
import org.gradle.api.provider.Property
import org.gradle.api.tasks.Internal
import org.gradle.api.tasks.OutputDirectory
import org.gradle.api.tasks.TaskAction
import org.gradle.process.ExecOperations
import java.io.File
import javax.inject.Inject

abstract class UnjarBundleDependencies @Inject constructor(private val archive: ArchiveOperations): DefaultTask() {
  @get:Internal
  abstract val classpath: Property<Configuration>

  @get:OutputDirectory
  abstract val outputDir: DirectoryProperty

  private val jars = mutableListOf<JarEntry>()

  class JarEntry(val jar: File, val copySpec: Action<in CopySpec>) {}

  class MapperDsl(private val jars : MutableList<JarEntry>, private val jarfinder: JarFinder, private val defaultCopySpec: Action<in CopySpec>) {
    fun jar(jar: Any) {
      jar(jar, defaultCopySpec)
    }

    fun jar(jar: Any, copySpec: Action<in CopySpec>) {
      jars.add(JarEntry(resolveJar(jar), copySpec))
    }

    private fun resolveJar(jar: Any): File {
      return when (jar) {
        is String -> jarfinder.find(jar)
        is File -> jar
        is RegularFile -> jar.asFile
        else -> throw GradleException("Unsupported jar type: ${jar.javaClass.canonicalName}")
      }
    }
  }

  fun bundle(dsl: MapperDsl.() -> Unit) {
    val setter = MapperDsl(jars, JarFinder(classpath.get())) {
      include("**/*.class")
      include("**/*.*")
      include("**/*.properties")
      exclude("META-INF/MANIFEST.MF")
    }
    dsl.invoke(setter)
  }

  @TaskAction
  fun unjarThem() {
    jars.forEach { entry ->
      project.copy {
        from(archive.zipTree(entry.jar), entry.copySpec)
        into(outputDir)
      }
    }
  }
}