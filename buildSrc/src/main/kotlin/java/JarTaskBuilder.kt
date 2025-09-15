package com.saxonica.build.java

import SaxonJExtension
import org.gradle.api.Project
import org.gradle.api.provider.Property
import org.gradle.api.tasks.Internal
import org.gradle.api.tasks.TaskProvider
import org.gradle.api.tasks.bundling.Jar
import org.gradle.kotlin.dsl.assign
import org.gradle.kotlin.dsl.property
import org.gradle.kotlin.dsl.register

class JarTaskBuilder(val extension: SaxonJExtension, val project: Project, val taskNamePrefix: String?, val classifierOrSuffix: String?) {
  companion object {
    fun createBuilder(project: Project, extension: SaxonJExtension): (String, JarTaskBuilder.() -> Unit) -> SaxonJarTasks {
      return { name, configuration ->
        create(project, extension, name, configuration)
      }
    }

    fun createEditionBuilder(project: Project, extension: SaxonJExtension): (JarTaskBuilder.() -> Unit) -> SaxonJarTasks {
      return { configuration ->
        create(project, extension, null, configuration)
      }
    }

    fun create(project: Project, extension: SaxonJExtension, name: String? = null, configuration: JarTaskBuilder.() -> Unit): SaxonJarTasks {
      val taskNamePrefix = if (name != null) name else "edition"
      val classifierOrSuffix = if (name != null) name else null
      val builder = JarTaskBuilder(extension, project, taskNamePrefix, classifierOrSuffix)
      builder.configuration()
      return builder.invoke()
    }
  }
  data class SaxonJarTasks(val download: TaskProvider<Jar>, val maven: TaskProvider<Jar>)

  @get:Internal
  public val edition: Property<String> = project.objects.property()
  @get:Internal
  public val downloadJarBaseName: Property<String> = project.objects.property()
  @get:Internal
  public val jarClassifier: Property<String> = project.objects.property()

  init {
    edition.convention(extension.edition)
    if (classifierOrSuffix != null) jarClassifier.convention(classifierOrSuffix)
    downloadJarBaseName.convention(edition.map { edition ->
      val args = mutableListOf(edition)
      if (jarClassifier.isPresent) args.add(jarClassifier.get())
      buildArchiveBaseName(*args.toTypedArray()).lowercase()
    })
  }

  private var mavenConfiguration: Jar.() -> Unit = {}
  private var downloadConfiguration: Jar.() -> Unit = {}
  private var commonConfiguration: Jar.() -> Unit = {}

  private fun buildArchiveBaseName(vararg chunks: String): String {
    val nameChunks = listOf("Saxon") + chunks
    return nameChunks.joinToString("-")
  }
  private val mavenArchiveBaseName: String by lazy {
    buildArchiveBaseName(edition.map { edition -> edition.uppercase() }.get())
  }
  private val commonBaseConfiguration: Jar.() -> Unit = {
    archiveVersion = extension.version
  }
  private val mavenBaseConfiguration: Jar.() -> Unit = {
    archiveBaseName = mavenArchiveBaseName
    archiveClassifier = jarClassifier
    destinationDirectory = project.layout.buildDirectory.dir("libs/maven")
  }
  private val downloadBaseConfiguration: Jar.() -> Unit = {
    archiveBaseName = downloadJarBaseName
    destinationDirectory = project.layout.buildDirectory.dir("libs/download")
  }

  fun maven(configuration: Jar.() -> Unit) {
    mavenConfiguration = configuration
  }

  fun download(configuration: Jar.() -> Unit) {
    downloadConfiguration = configuration
  }

  fun common(configuration: Jar.() -> Unit) {
    commonConfiguration = configuration
  }

  protected fun invoke(): SaxonJarTasks {
    val downloadJar = project.tasks.register<Jar>("${taskNamePrefix}DownloadJar", commonBaseConfiguration)
    val mavenJar = project.tasks.register<Jar>("${taskNamePrefix}MavenJar", commonBaseConfiguration)
    downloadJar.configure(downloadBaseConfiguration)
    downloadJar.configure(commonConfiguration)
    downloadJar.configure(downloadConfiguration)

    mavenJar.configure(mavenBaseConfiguration)
    mavenJar.configure(commonConfiguration)
    mavenJar.configure(mavenConfiguration)

    return SaxonJarTasks(downloadJar, mavenJar)
  }
}

