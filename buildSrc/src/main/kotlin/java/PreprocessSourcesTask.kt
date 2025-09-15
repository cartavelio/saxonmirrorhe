package com.saxonica.build.java

import org.gradle.api.DefaultTask
import org.gradle.api.file.ConfigurableFileCollection
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.ProjectLayout
import org.gradle.api.tasks.InputDirectory
import org.gradle.api.tasks.InputFiles
import org.gradle.api.tasks.OutputDirectory
import org.gradle.api.tasks.TaskAction
import org.gradle.kotlin.dsl.assign
import org.gradle.process.ExecOperations
import javax.inject.Inject

abstract class PreprocessSourcesTask @Inject constructor(private val layout: ProjectLayout, private val ex: ExecOperations) : DefaultTask() {
    @get:InputFiles
    abstract val classpath: ConfigurableFileCollection

    @get:InputDirectory
    abstract val sources: DirectoryProperty

    @get:OutputDirectory
    abstract val output: DirectoryProperty

    private val preprocessorOpts = mutableMapOf<String, String>()

    class PreprocessorOptsSetter(private val preprocessorOpts : MutableMap<String, String>) {
        fun p(name: String, value: String) {
            preprocessorOpts.put(name.uppercase(), value)
        }

        fun p(name: String, value: Boolean) {
            p(name, if (value) "true" else "false")
        }

        fun on(vararg names: String) {
            names.forEach { p(it, true) }
        }

        fun off(vararg names: String) {
            names.forEach { p(it, false) }
        }
    }

    fun opts(dsl: PreprocessorOptsSetter.() -> Unit) {
        val setter = PreprocessorOptsSetter(preprocessorOpts)
        dsl.invoke(setter)
    }

    private fun getOpts(): Array<String> {
        return preprocessorOpts.map { (name, value) -> "/p:$name=$value" }.toTypedArray()
    }

    @TaskAction
    fun preprocess() {
        val cls = classpath
        println("Preprocessing ${sources.get().asFile}")
        ex.javaexec {
            classpath = cls
            mainClass = "com.igormaznitsa.jcp.JcpPreprocessor"
            args("/c",
                "/i:${sources.get()}",
                "/o:${layout.buildDirectory.dir("src/main/java").get()}",
                *getOpts())
        }

    }
}