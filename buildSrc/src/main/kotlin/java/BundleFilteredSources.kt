package com.saxonica.build.java

import org.gradle.api.DefaultTask
import org.gradle.api.artifacts.ResolvableConfiguration
import org.gradle.api.file.*
import org.gradle.api.tasks.OutputDirectory
import org.gradle.api.tasks.TaskAction
import javax.inject.Inject

abstract class BundleFilteredSources
    @Inject constructor(private val fs: FileSystemOperations, private val layout: ProjectLayout) : DefaultTask() {

    @get:OutputDirectory
    val bundledSources
        get() = layout.buildDirectory.dir("tmp/bundled-filtered-sources-${name}")

    private val sourceDirs = mutableSetOf<Any>()
    private val exclusions = mutableListOf<String>()

    fun add(configuration: ResolvableConfiguration) {
        add(configuration.elements.map { it.first().asFile })
    }

    fun add(sourceDir: Any) {
        inputs.files(sourceDir)
        sourceDirs.add(sourceDir)
    }

    class CleanupDsl(private val exclusions: MutableList<String>) {
        fun exclude(vararg excludes: String) {
            exclusions.addAll(excludes)
        }
    }

    fun cleanup(definition: CleanupDsl.() -> Unit) {
        val dsl = CleanupDsl(exclusions)
        definition.invoke(dsl)
    }

    @TaskAction
    fun bundle() {
        val destination = layout.buildDirectory.dir("tmp/bundled-filtered-sources-${name}")
        fs.delete {
            delete(destination)
        }
        fs.copy {
            into(destination)
            sourceDirs.forEach { source ->
                from(project.files(source))
            }
            exclusions.forEach { exclusion ->
                exclude(exclusion)
            }
            duplicatesStrategy = DuplicatesStrategy.WARN
        }
    }
}