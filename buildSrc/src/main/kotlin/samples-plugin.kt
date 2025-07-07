package com.saxonica.build

import SaxonBuildExtension
import java.io.ByteArrayOutputStream
import java.io.File
import java.io.FileOutputStream
import java.io.PrintStream
import org.gradle.api.GradleException
import org.gradle.api.Plugin
import org.gradle.api.Project
import org.gradle.api.file.*
import org.gradle.api.provider.Property
import org.gradle.api.tasks.Copy
import org.gradle.api.tasks.Exec
import org.gradle.api.tasks.JavaExec
import org.gradle.api.tasks.TaskContainer
import org.gradle.api.tasks.compile.JavaCompile
import org.gradle.kotlin.dsl.assign
import org.gradle.kotlin.dsl.create
import org.gradle.kotlin.dsl.register
import org.gradle.process.ExecOperations
import javax.inject.Inject

abstract class SamplesExtension
  @Inject constructor(private val layout: ProjectLayout,
                      private val tasks: TaskContainer,
                      private val ex: ExecOperations) {

  abstract val runclasspath: Property<FileCollection>
  abstract val debugclasspath: Property<FileCollection>
  abstract val sources: Property<FileCollection>
  abstract val product: Property<String>
  abstract val platform: Property<String>
  abstract val arch: Property<String>
  abstract val edition: Property<String>

  private val copy_sample_sources = tasks.register<Copy>("copy_sample_sources") {
    into(layout.buildDirectory.dir("samples/resources"))
    from(sources)
  }

  private val run_cmd_books_sample_tests = tasks.register("run_cmd_books_sample_tests") {
    // Just somewhere to hang dependencies
  }

  private val all_samples = tasks.register("all_samples") {
  }

  private val run_classes = tasks.register<JavaCompile>("compile_sample_run_classes") {
    dependsOn("copy_java_sample_sources")
    classpath = runclasspath.get()
    source = project.fileTree(layout.buildDirectory.dir("samples/src/java/main").get())
    destinationDirectory = layout.buildDirectory.dir("samples/run/classes").get()
  }

  private val debug_classes = tasks.register<JavaCompile>("compile_sample_debug_classes") {
    dependsOn("copy_java_sample_sources")
    classpath = runclasspath.get()
    source = project.fileTree(layout.buildDirectory.dir("samples/src/java/main").get())
    destinationDirectory = layout.buildDirectory.dir("samples/debug/classes").get()
  }

  fun transform(sample: String, input: String?, stylesheet: String,
                args: List<String> = emptyList(), output: File? = null) {
    val name = "sample_${sample}"
    javaTransform(sample, input, stylesheet, args, name, output)
  }

  fun query(sample: String, input: String?, queryFile: String, 
            args: List<String> = emptyList(), taskName: String? = null) {
    val name = if (taskName == null) "sample_${sample}" else taskName
    javaQuery(sample, input, queryFile, null, args, name)
  }

  fun queryString(sample: String, input: String?, queryString: String, 
                  args: List<String> = emptyList(), taskName: String? = null) {
    val name = if (taskName == null) "sample_${sample}" else taskName
    javaQuery(sample, input, null, queryString, args, name)
  }

  fun validate(sample: String, input: String, schema: String, 
                args: List<String> = emptyList(), taskName: String? = null) {
    val name = if (taskName == null) "sample_${sample}" else taskName
    javaValidate(sample, input, schema, args, name)
  }

  fun sampleClass(sample: String, args: List<String>, className: String? = null) {
    val t = tasks.register<JavaExec>("sample_class_${sample}") {
      dependsOn("compile_sample_run_classes")
      inputs.files(copy_sample_sources.get().outputs)
      outputs.file(layout.buildDirectory.file("sample_results/${sample}.txt"))

      val stdOutput = ByteArrayOutputStream()
      val errOutput = ByteArrayOutputStream()
      
      standardOutput = stdOutput
      errorOutput = errOutput

      mainClass = className ?: "${edition.get()}.${sample}"
      classpath = runclasspath.get() + run_classes.get().outputs.files
      workingDir = layout.buildDirectory.dir("samples/resources").get().asFile
      args(args)

      doLast {
        if (stdOutput.size() > 0) {
          val outfile = layout.buildDirectory.file("sample_results/${sample}.txt").get().asFile
          println("==> ${outfile}")
          var outputStream = PrintStream(outfile)
          var lines = stdOutput.toString("utf-8")
          outputStream.print(lines)
          outputStream.close()
        }
        if (errOutput.size() > 0) {
          val outfile = layout.buildDirectory.file("sample_results/${sample}.err").get().asFile
          println("==> ${outfile}")
          var outputStream = PrintStream(outfile)
          var lines = errOutput.toString("utf-8")
          outputStream.print(lines)
          outputStream.close()
        }
      }
    }

    val t2 = tasks.register<Exec>("check_sample_class_${sample}") {
      dependsOn(t)

      val script = if (edition.get() == "ee") {
        layout.projectDirectory.file("../tools/sample-check.py")
      } else {
        layout.projectDirectory.file("../../tools/sample-check.py")
      }

      commandLine = listOf("python", "${script}", "--edition", "${edition.get()}",
                           "--sample", sample,
                           "${layout.buildDirectory.file("sample_results/${sample}.txt").get()}")
    }

    t.get().finalizedBy(t2)
    all_samples.get().dependsOn(t)
  }

  fun csSample(sample: String, input: String?, stylesheet: String,
               args: List<String> = emptyList(), output: File? = null) {

    tasks.register("cs_sample_${sample}") {
      dependsOn(copy_sample_sources)
      dependsOn("dotnetPublish")

      val saxonCS = project.extensions.findByType(SaxonCSExtension::class.java)!!
      val saxonBuild = project.extensions.findByType(SaxonBuildExtension::class.java)!!

      val arch = saxonCS.arch.get()
      val rel = saxonCS.release.get()
      val netver = "net${saxonBuild.dotnetversion.get()}"
      val target = "${saxonCS.platform.get()}-${arch}"
      val exec = "${arch}/SaxonCS/bin/${rel}/${netver}/${target}/SaxonCS"

      val cmdArgs = mutableListOf<String>()

      cmdArgs.add("${layout.buildDirectory.file(exec).get()}")
      cmdArgs.add("transform")

      input?.let {
        cmdArgs.add("-s:${layout.buildDirectory.file("samples/resources/data/${it}").get()}")
        inputs.files(layout.buildDirectory.file("samples/resources/data/${it}"))
      }
      cmdArgs.add("-xsl:${layout.buildDirectory.file("samples/resources/styles/${stylesheet}").get()}")
      cmdArgs.add("-o:file:${layout.buildDirectory.file("sample_results/${sample}.html").get()}")
      cmdArgs.addAll(args)

      doFirst {
        project.mkdir(layout.buildDirectory.file("sample_results/playhtml"))
        println(cmdArgs)

      }

      doLast {
        ex.exec {
          workingDir = layout.buildDirectory.file("samples/resources").get().asFile
          commandLine(cmdArgs)
        }
      }
    }
  }

  // ============================================================
  
  fun javaTransform(sample: String, input: String?, stylesheet: String,
                    args: List<String>, taskName: String, output: File?) {
    val t = tasks.register<JavaExec>(taskName) {
      inputs.files(copy_sample_sources.get().outputs)
      if (output == null) {
        outputs.files(layout.buildDirectory.file("sample_results/${sample}.html"))
      } else {
        outputs.file(output)
      }

      classpath = runclasspath.get()
      mainClass = "net.sf.saxon.Transform"

      val xformArgs = mutableListOf<String>()

      input?.let {
        xformArgs.add("-s:${layout.buildDirectory.file("samples/resources/data/${it}").get()}")
        inputs.files(layout.buildDirectory.file("samples/resources/data/${it}"))
      }
      xformArgs.add("-xsl:${layout.buildDirectory.file("samples/resources/styles/${stylesheet}").get()}")
      xformArgs.add("-o:${layout.buildDirectory.file("sample_results/${sample}.html").get()}")

      xformArgs.addAll(args)

      args(xformArgs)
    }

    all_samples.get().dependsOn(t)
  }

  fun javaQuery(sample: String, input: String?, queryFile: String?, queryString: String?,
                args: List<String>, taskName: String) {
    val t = tasks.register<JavaExec>(taskName) {
      inputs.files(copy_sample_sources.get().outputs)
      outputs.files(layout.buildDirectory.file("sample_results/${sample}.html"))

      classpath = runclasspath.get()
      mainClass = "net.sf.saxon.Query"

      val xformArgs = mutableListOf<String>()
      input?.let {
        xformArgs.add("-s:${layout.buildDirectory.file("samples/resources/data/${it}").get()}")
        inputs.files(layout.buildDirectory.file("samples/resources/data/${it}"))
      }
      queryFile?.let {
        xformArgs.add("-q:${layout.buildDirectory.file("samples/resources/query/${it}").get()}")
        inputs.files(layout.buildDirectory.file("samples/resources/query/${it}"))
      }
      queryString?.let { xformArgs.add("-qs:${it}") }
      xformArgs.add("-o:${layout.buildDirectory.file("sample_results/${sample}.html").get()}")
      xformArgs.addAll(args)

      args(xformArgs)
    }

      all_samples.get().dependsOn(t)
  }

  fun javaValidate(sample: String, input: String, schema: String,
                   args: List<String>, taskName: String) {
    val t = tasks.register<JavaExec>(taskName) {
      inputs.files(copy_sample_sources.get().outputs)
      inputs.files(layout.buildDirectory.file("samples/resources/data/${input}"))
      inputs.files(layout.buildDirectory.file("samples/resources/data/${schema}"))
      outputs.files(layout.buildDirectory.file("sample_results/${sample}.xml"))

      classpath = runclasspath.get()
      mainClass = "com.saxonica.Validate"

      val xformArgs = mutableListOf<String>(
          "-s:${layout.buildDirectory.file("samples/resources/data/${input}").get()}",
          "-xsd:${layout.buildDirectory.file("samples/resources/data/${schema}").get()}",
          "-report:${layout.buildDirectory.file("sample_results/${sample}.xml").get()}")

      xformArgs.addAll(args)

      args(xformArgs)
    }

    val t2 = tasks.register<JavaExec>("validate_${taskName}") {
      dependsOn(t)
      description = "Validate the ${sample} validation report against validation-reports.xsd"
      inputs.files(t.get().outputs.getFiles().getSingleFile())
      workingDir = layout.buildDirectory.dir("samples/resources").get().asFile
      classpath = runclasspath.get()
      mainClass = "com.saxonica.Validate"
      args("-s:${t.get().outputs.getFiles().getSingleFile()}",
           "-xsd:schemas/validation-reports.xsd")
    }

    t.get().finalizedBy(t2)
    all_samples.get().dependsOn(t)
  }

/*
  fun javaTest(sample: String, classFile: String, sampleClass: String,
               input: String, args: List<String>) {
    val tver = "run"

    if (tver == "run") {
      layout.buildDirectory.file("sample_results").get().asFile.mkdirs()
    }

    tasks.register<JavaExec>("${tver}_samples_${sample}") {
      val resultOutput = layout.buildDirectory.file("sample_results/${sample}.txt")

      // It's the weirdest dang thing. If I use the same name in outputs.files()
      // and in a FileOutputStream for standardOutput, the file doesn't get
      // written. I can't explain it. Workaround is to buffer in a byte array
      // output stream.

      val baos = ByteArrayOutputStream()
      if (tver == "run") {
        inputs.files(run_classes)
        outputs.files(resultOutput)
        classpath = runclasspath.get() + run_classes.get().outputs.files
        standardOutput = baos
      } else {
        inputs.files(debug_classes)
        outputs.files(resultOutput)
        classpath = debugclasspath.get() + debug_classes.get().outputs.files
        outputs.upToDateWhen { false }
      }

      systemProperties["javax.xml.transform.TransformerFactory"] = "net.sf.saxon.TransformerFactoryImpl"
      mainClass = classFile

      val sampleArgs = mutableListOf<String>(
          "${layout.buildDirectory.file("samples/resources/data/${input}").get()}"
      )
      sampleArgs.addAll(args)

      args(sampleArgs)

      doLast {
        if (tver == "run") {
          val stream = PrintStream(FileOutputStream(resultOutput.get().asFile))
          stream.println(baos.toString("utf-8"))
          stream.close()
        }
      }
    }
  }
*/
}

class SamplesPlugin : Plugin<Project> {
  override fun apply(project: Project): Unit = project.run {
    val extension = project.extensions.create<SamplesExtension>("samples")

    val saxonBuild = project.extensions.findByType(SaxonBuildExtension::class.java)
    if (saxonBuild !is SaxonBuildExtension) {
      throw GradleException("Could not find 'saxonbuild' extension")
    }

    extension.product.convention("")
    extension.platform.convention(saxonBuild.platform.get())
    extension.arch.convention(if (saxonBuild.arch.get() == "aarch64") "arm64" else "x64")
    extension.edition.convention("")
  }
}
