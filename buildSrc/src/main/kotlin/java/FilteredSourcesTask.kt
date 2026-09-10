package com.saxonica.build.java

import org.gradle.api.Action
import org.gradle.api.DefaultTask
import org.gradle.api.file.*
import org.gradle.api.tasks.OutputDirectory
import org.gradle.api.tasks.TaskAction
import javax.inject.Inject

abstract class FilteredSourcesTask @Inject constructor(private val fs: FileSystemOperations, private val layout: ProjectLayout) : DefaultTask() {
    @get:OutputDirectory
    val destinationDir
        get() = layout.buildDirectory.dir("tmp/filtered-sources-${name}")

    private val copySpecInvocations = mutableListOf<Pair<Any, Action<CopySpec>>>()

    fun from(sourcePath: Any, copySpecBody: Action<CopySpec>) {
        inputs.files(sourcePath)
        copySpecInvocations.add(Pair(sourcePath, copySpecBody))
    }

    fun from(sourcePath: Any) {
        from(sourcePath, { })
    }

    @TaskAction
    fun filter() {
        fs.delete {
            delete(destinationDir)
        }
        fs.copy {
            copySpecInvocations.forEach { pair ->
                from(pair.first, pair.second)
            }
            duplicatesStrategy = DuplicatesStrategy.WARN
            into(destinationDir)
        }
    }
}