package com.saxonica.build.xslt

import net.sf.saxon.s9api.Processor
import nu.validator.htmlparser.common.XmlViolationPolicy
import nu.validator.htmlparser.dom.HtmlDocumentBuilder
import org.gradle.api.DefaultTask
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.tasks.InputFile
import org.gradle.api.tasks.OutputFile
import org.gradle.api.tasks.TaskAction
import javax.xml.transform.Source
import javax.xml.transform.dom.DOMSource
import javax.xml.transform.stream.StreamSource

abstract class BaseXsltTask : DefaultTask() {
  @get:InputFile
  abstract val xslt: RegularFileProperty

  @get:InputFile
  abstract val input: RegularFileProperty

  @get:OutputFile
  abstract val output: RegularFileProperty

  private val processor by lazy { Processor() }
  private val compiler by lazy { processor.newXsltCompiler() }
  private val executable by lazy { compiler.compile(xslt.get().asFile) }
  private val transformer by lazy { executable.load30() }
  private val serializer by lazy { transformer.newSerializer(output.get().asFile) }

  /**
   * Build a Source suitable for feeding to applyTemplates, callTemplate or callFunction.
   */
  abstract fun buildInputSource(): Source

  @TaskAction
  fun transform() {
    transformer.applyTemplates(buildInputSource(), serializer)
  }
}

abstract class XsltTask : BaseXsltTask() {
  override fun buildInputSource(): Source {
    return StreamSource(input.get().asFile)
  }
}

abstract class XsltOnHtmlTask : BaseXsltTask() {
  override fun buildInputSource(): Source {
    val htmlBuilder = HtmlDocumentBuilder(XmlViolationPolicy.ALTER_INFOSET)
    return DOMSource(htmlBuilder.parse(input.get().asFile.inputStream()))
  }
}