import com.saxonica.build.LifecycleTasks

plugins {
  id("java-library")
  id("saxon-build")
  id("com.saxonica.build.managebranches")
}

repositories {
  mavenLocal()
  mavenCentral()
  maven { url = uri("https://maven.saxonica.com/maven") }
}

val saxonBuild = the<SaxonBuildExtension>()
val manageBranches = the<ManageBranchesExtension>()

val saxonMajorVersion = saxonBuild.version.get().split('.').first()

val javaSourcesBucket by configurations.dependencyScope("javaSourcesBucket")
val javadocBucket by configurations.dependencyScope("javadocBucket")
val featureKeysBucket by configurations.dependencyScope("featureKeysBucket")
val xmldocletBucket by configurations.dependencyScope("xmldocletBucket")
val csharpApiSourcesBucket by configurations.dependencyScope("csharpApiSourcesBucket")
val csharpXmlDocBucket by configurations.dependencyScope("csharpXmlDocBucket")
val csharpHtmlDocBucket by configurations.dependencyScope("csharpHtmlDocBucket")
val doxygenBucket by configurations.dependencyScope("doxygenBucket")
val doxygenCBucket by configurations.dependencyScope("doxygenCBucket")

val documentation by configurations.creating {
  extendsFrom(configurations["implementation"])
}

val documentationx by configurations.creating {
  extendsFrom(configurations["implementation"])
}

dependencies {
  javaSourcesBucket(files(layout.projectDirectory.files("../java/src/main/java")))
  javadocBucket(project(mapOf("path" to ":java",
                              "configuration" to "javadocArtifacts")))
  featureKeysBucket(project(mapOf("path" to ":java:keys",
                                  "configuration" to "featureKeys")))
  xmldocletBucket(project(mapOf("path" to ":java",
                                "configuration" to "xmldocletArtifacts")))
  csharpApiSourcesBucket(project(mapOf("path" to ":csharp:ee",
                                       "configuration" to "apiSourceArtifacts")))
  csharpXmlDocBucket(project(mapOf("path" to ":csharp:ee",
                                   "configuration" to "xmlDocArtifacts")))
  csharpHtmlDocBucket(project(mapOf("path" to ":csharp:ee",
                                    "configuration" to "htmlDocArtifacts")))
  doxygenBucket(project(mapOf("path" to ":python:documentation",
                              "configuration" to "doxygenArtifact")))
  doxygenCBucket(project(mapOf("path" to ":libsaxonc:ee",
                              "configuration" to "doxygenOutput")))

  // N.B. the gen_doc_icu_uca class depeneds on using the current build!
  documentation(project(":java", "runtimeElements"))

  // We need a license to build SEFs
  documentation(files(saxonBuild.saxonLicenseDir))

  // The documentationx configuration doesn't have a license file
  documentationx(project(":java", "runtimeElements"))

  implementation("xom:xom:1.3.5")
  implementation("org.jdom:jdom:1.1.3")
  implementation("org.jdom:jdom2:2.0.6.1")
  implementation("dom4j:dom4j:1.6.1")
  implementation("nu.validator:htmlparser:${findProperty("htmlparserVersion")}")
  implementation("javax.mail:javax.mail-api:${findProperty("mailVersion")}")
  implementation("com.sun.mail:javax.mail:${findProperty("mailVersion")}")
  implementation("org.apache.ws.commons.axiom:axiom:1.2.15")
  implementation("org.apache.ws.commons.axiom:axiom-dom:1.2.15")
  implementation("org.apache.ws.commons.axiom:axiom-impl:1.2.15")
  implementation("org.xmlresolver:xmlresolver:${findProperty("xmlresolverVersion")}")
  implementation("jline:jline:${findProperty("jlineVersion")}")
  implementation("org.apache.xmlgraphics:fop:2.10")

  implementation("com.ibm.icu:icu4j:${findProperty("icu4jVersion")}")
  implementation("com.github.javaparser:javaparser-symbol-solver-core:3.17.0")
  implementation("com.github.javaparser:javaparser-core:3.17.0")
}

val javaSources = configurations.resolvable("javaSources") {
  extendsFrom(javaSourcesBucket)
}

val javadocSources = configurations.resolvable("javadocSources") {
  extendsFrom(javadocBucket)
}

val featureKeysSources = configurations.resolvable("featureKeysSources") {
  extendsFrom(featureKeysBucket)
}

val xmldocletSources = configurations.resolvable("xmldocletSources") {
  extendsFrom(xmldocletBucket)
}

val csharpApiSources = configurations.resolvable("csharpApiSources") {
  extendsFrom(csharpApiSourcesBucket)
}

val csharpXmlDocumentation = configurations.resolvable("csharpXmlDocumentation") {
  extendsFrom(csharpXmlDocBucket)
}

val csharpHtmlDocumentation = configurations.resolvable("csharpHtmlDocumentation") {
  extendsFrom(csharpHtmlDocBucket)
}

val doxygenDocumentation = configurations.resolvable("doxygenDocumentation") {
  extendsFrom(doxygenBucket)
}

val doxygenCDocumentation = configurations.resolvable("doxygenCDocumentation") {
  extendsFrom(doxygenCBucket)
}

// ============================================================

val element_doc = tasks.register("element_doc") {
  group = "Internal"
  // just somewhere to hang dependencies
}

tasks.register("userdoc_xml") {
  group = "Internal"
  description = "Creates the XML version of the user documentation"
  dependsOn("userdoc_articles")
  dependsOn("app_javadoc")
  dependsOn("app_dotnetdoc")
  // Just a place to hang dependencies
}

tasks.register("userdoc_articles") {
  group = "Internal"
  description = "Process all the articles"
  dependsOn("expand_userdoc_sections")
  dependsOn("userdoc_changes")
  dependsOn("userdoc_functions")
  // Just a place to hang dependencies
}

listOf<String>("instructions", "serialization-params", "sql-extension").forEach { target ->
  val t = tasks.register<JavaExec>("gen_element_doc_${target}") {
    group = "Internal"
    description = "Expand element documentation for ${target}"

    // Why does sql-extension go in a different location?
    val outputPath = if (target == "sql-extension") {
      "src/userdoc/${target}.xml"
    } else {
      "src/userdoc/extensions/${target}.xml"
    }

    inputs.file(layout.projectDirectory.file("src/userdoc/extensions/${target}.xml"))
    inputs.dir(layout.projectDirectory.dir("tools/userdoc"))
    outputs.file(layout.buildDirectory.file(outputPath))
    classpath = configurations.named("documentation").get()
    mainClass = "com.saxonica.Transform"
    args("-s:${layout.projectDirectory.file("src/userdoc/extensions/${target}.xml")}",
      "-xsl:${layout.projectDirectory.file("tools/userdoc/elements-body.xsl")}",
      "-o:${layout.buildDirectory.file(outputPath).get().asFile}")
  }
  element_doc { dependsOn(t) }
}

tasks.register<Copy>("userdoc_publish_viewer_appJS") {
  group = "Internal"
  description = "Copy viewer_appJS to build"

  into(layout.buildDirectory.dir("userdoc/viewer_appJS"))
  from(layout.projectDirectory.dir("src/viewer_appJS")) {
    exclude("readme.txt")
  }
}

tasks.register<JavaExec>("viewer_app_sef") {
  group = "Internal"
  description = "Compile the viewer_app stylesheet"

  inputs.dir(layout.projectDirectory.dir("src/viewer_appJS/xsl"))
  outputs.file(layout.buildDirectory.file("userdoc/viewer_appJS/xsl/viewer_app.sef.json"))
  classpath = configurations.named("documentation").get()
  mainClass = "com.saxonica.Transform"
  args("-xsl:${layout.projectDirectory.file("src/viewer_appJS/xsl/viewer_app.xsl")}",
    "-export:${layout.buildDirectory.file("userdoc/viewer_appJS/xsl/viewer_app.sef.json").get().asFile}",
    "-target:JS", "-nogo", "-relocate:on", "-ns:##html5",
    "--generateByteCode:off",
    "SEFbuildDate=${saxonBuild.dateStamp}")
}

tasks.register<JavaExec>("product_xsljavadoc") {
  group = "Internal"
  description = "Convert javadoc XML into format for documentation"
  inputs.files(xmldocletSources)
  inputs.files(layout.projectDirectory.dir("tools/javadoc"))
  outputs.dir(layout.buildDirectory.dir("javadoc-xml"))

  classpath = configurations.named("documentationx").get()
  mainClass = "com.saxonica.Transform"
  args("-s:${files(xmldocletSources).first()}/doclet.xml",
       "-xsl:${layout.projectDirectory.file("tools/javadoc/chunk-xmljavadoc.xsl")}",
       "-o:${layout.buildDirectory.file("javadoc-xml/out").get().asFile}",
       "base-output=${layout.buildDirectory.file("javadoc-xml").get().asFile}")
}

tasks.register<JavaExec>("javadoc_linkchecker") {
  group = "Internal"
  description = "Generate report of links"
  dependsOn("product_xsljavadoc")

  inputs.files(layout.buildDirectory.files("javadoc-xml"))
  inputs.file(layout.projectDirectory.file("tools/javadoc/check-tag-links.xsl"))
  outputs.file(layout.buildDirectory.file("javadoc-tag-check.xml"))
  classpath = configurations.named("documentation").get()
  mainClass = "com.saxonica.Transform"
  args("-s:${layout.buildDirectory.file("javadoc-xml/javadoc-types.xml").get().asFile}",
       "-xsl:${layout.projectDirectory.file("tools/javadoc/check-tag-links.xsl")}",
       "-o:${layout.buildDirectory.file("javadoc-tag-check.xml").get().asFile}",
       "base-dir=${layout.buildDirectory.file("javadoc-xml").get().asFile}/")
}

tasks.register<Copy>("copy_userdoc_implement") {
  group = "Internal"
  description = "Copy the app implement file"

  from(layout.projectDirectory.dir("src/viewer_appJS/doc"))
  into(layout.buildDirectory.dir("src/userdoc"))
}

tasks.register<Copy>("copy_userdoc_sources") {
  group = "Internal"
  description = "Copy the 'real', unpreprocessed sources"
  dependsOn("copy_userdoc_implement")

  into(layout.buildDirectory.dir("src/userdoc"))
  from(layout.projectDirectory.dir("src/userdoc")) {
    exclude("extensions/**")
    exclude("elements/**")
  }
}

tasks.register<JavaExec>("doc_FeatureKeysDoc") {
  group = "Internal"
  description = "Construct the documentation for feature keys"
  dependsOn("copy_userdoc_sources")

  inputs.files(layout.projectDirectory.dir("../java/keys/tools"))
  outputs.file(layout.buildDirectory.file("src/userdoc/featureKeys/FeatureKeysDoc.xml"))
  classpath = configurations.named("documentation").get()
  mainClass = "net.sf.saxon.Transform"
  args("${layout.projectDirectory.file("../src/main/xml/FeatureKeys.xml")}",
       "-xsl:${layout.projectDirectory.file("tools/featureKeys/FeatureKeysToDoc.xsl")}",
       "-o:${layout.buildDirectory.file("src/userdoc/featureKeys/FeatureKeysDoc.xml").get().asFile}")
}

val dotnet_doc_html = tasks.register<Copy>("dotnet_doc_html") {
  group = "Documentation"
  description = "Copy the HTML .NET documentation"

  from(csharpHtmlDocumentation)
  into(layout.buildDirectory.dir("dotnetdoc-html"))
}

val dotnetDocHtmlArtifacts by configurations.consumable("dotnetDocHtmlArtifacts")
dotnetDocHtmlArtifacts.outgoing.artifact(dotnet_doc_html) { builtBy(dotnet_doc_html) }

tasks.register<Copy>("dotnet_doc_xml") {
  group = "Documentation"
  description = "Copy the XML .NET documentation"

  from(csharpXmlDocumentation)
  into(layout.buildDirectory.dir("dotnetdoc-xml"))
}

tasks.register<Copy>("dotnet_doc") {
  group = "Documentation"
  description = "Copy the HTML and XML .NET documentation"
  dependsOn("dotnet_doc_html")
  dependsOn("dotnet_doc_xml")
}

tasks.register<Copy>("app_dotnetdoc") {
  group = "Internal"
  description = "Copy the XML dotnet docs to the viewer app"
  dependsOn("dotnet_doc_xml")

  into(layout.buildDirectory.dir("userdoc/viewer_appJS/dotnetdoc-xml"))
  from(layout.buildDirectory.dir("dotnetdoc-xml"))
}

tasks.register<Copy>("app_javadoc") {
  group = "Internal"
  description = "Copy the XML javadoc docs to the viewer app"
  dependsOn("product_xsljavadoc")

  into(layout.buildDirectory.dir("userdoc/viewer_appJS/javadoc-xml"))
  from(layout.buildDirectory.dir("javadoc-xml"))
}

tasks.register<Copy>("userdoc_viewer_appJS") {
  group = "Internal"
  description = "Copy transformed articles to app docs"
  dependsOn("userdoc_xml")
  dependsOn("userdoc_publish_viewer_appJS")
  dependsOn("viewer_app_sef")

  into(layout.buildDirectory.dir("userdoc/viewer_appJS/doc"))
  from(layout.buildDirectory.dir("userdoc/expanded")) {
    include("*.xml")
    exclude("functions-article.xml")
    exclude("changes-article.xml")
  }
}

tasks.register<Copy>("copy_xml_documentation") {
  group = "Internal"
  description = "Update XML version of documentation from viewer app"
  dependsOn("userdoc_viewer_appJS")

  into(layout.buildDirectory.dir("website/documentation${saxonMajorVersion}"))
  from(layout.buildDirectory.dir("userdoc/viewer_appJS"))
}

tasks.register("gen_doc") {
  group = "Internal"
  description = "Collects up, generates and copies the documentation source into the canonical location"
  dependsOn("gen_doc_icu_uca")
  dependsOn("gen_xslt_element_reference")
  dependsOn("element_doc")
  dependsOn("expath_doc")
  dependsOn("doc_FeatureKeysDoc")
  dependsOn("doc_ConfigFileDoc")
  // Just a place to hang dependencies
}

tasks.register<JavaExec>("doc_ConfigFileDoc") {
  group = "Internal"
  description = "Construct the documentation for the configuration file"
  dependsOn("copy_userdoc_sources")
  dependsOn("doc_FeatureKeysDoc")

  inputs.files(layout.projectDirectory.dir("../src/main/xml"))
  outputs.dir(layout.buildDirectory.dir("src/userdoc/featureKeys"))
  classpath = configurations.named("documentation").get()
  mainClass = "net.sf.saxon.Transform"
  args("-s:${layout.projectDirectory.file("../src/main/xml/AdditionalFeatures.xml")}",
       "-xsl:${layout.projectDirectory.file("tools/featureKeys/ToConfigFileDoc.xsl")}",
       "-o:${layout.buildDirectory.file("src/userdoc/featureKeys/ConfigFileDoc.xml").get().asFile}")
}

tasks.register<JavaExec>("gen_doc_icu_uca") {
  group = "Internal"
  description = "Generate the ICU/UCA documentation"
  dependsOn("copy_userdoc_sources")

  inputs.files(layout.projectDirectory.dir("src/userdoc"))
  inputs.files(layout.projectDirectory.dir("tools/userdoc"))
  outputs.dir(layout.buildDirectory.dir("src/userdoc/extensibility"))
  // Must be the current EE in this build...
  classpath = configurations.named("documentation").get()
  mainClass = "com.saxonica.Transform"
  args("-it:go",
       "-s:${layout.projectDirectory.file("src/userdoc/catalog.xml")}",
       "-xsl:${layout.projectDirectory.file("tools/userdoc/document-ICU.xsl")}",
       "output=${layout.buildDirectory.file("src/userdoc/extensibility").get().asFile}/")
}

tasks.register<JavaExec>("gen_xslt_element_reference_preprocess") {
  group = "Internal"
  description = "Preprocess input for the XSLT Element reference documentation"
  dependsOn("copy_userdoc_sources")

  inputs.files(layout.projectDirectory.dir("src/userdoc/elements"))
  inputs.files(layout.projectDirectory.dir("tools/userdoc"))
  outputs.file(layout.buildDirectory.file("userdoc/tmp/xsl-elements-expanded.xml"))
  classpath = configurations.named("documentation").get()
  mainClass = "com.saxonica.Transform"
  args("-s:${layout.projectDirectory.file("src/userdoc/elements/xsl-elements.xml")}",
       "-o:${layout.buildDirectory.file("userdoc/tmp/xsl-elements-expanded.xml").get().asFile}",
       "-xsl:${layout.projectDirectory.file("tools/userdoc/genElementDoc.xsl")}")
}

tasks.register<JavaExec>("gen_xslt_element_reference") {
  group = "Internal"
  description = "Generate the XSLT Element reference documentation"

  val preprocess = tasks.named("gen_xslt_element_reference_preprocess")
  dependsOn(preprocess)

  inputs.file(preprocess.get().outputs.getFiles().getSingleFile())
  inputs.files(layout.projectDirectory.dir("tools/userdoc"))
  outputs.file(layout.buildDirectory.file("src/userdoc/xsl-elements.xml"))
  classpath = configurations.named("documentation").get()
  mainClass = "com.saxonica.Transform"
  args ("-s:${layout.buildDirectory.file("userdoc/tmp/xsl-elements-expanded.xml").get().asFile}",
        "-xsl:${layout.projectDirectory.file("tools/userdoc/elements-body.xsl")}",
        "-o:${layout.buildDirectory.file("src/userdoc/xsl-elements.xml").get().asFile}")
}

// ============================================================

tasks.named(LifecycleTasks.DOCUMENTATION_TASK_NAME) {
  description = "Build the documentation and pages for website"
  dependsOn("update_documentation")
  dependsOn("gen_contents_ref")
  dependsOn("gen_feature_matrix")
  dependsOn("saxonc_documentation")
  // Just a place to hang dependencies
}

tasks.register("website") {
  group = LifecycleTasks.DOCUMENTATION_GROUP
  description = "Build the documentation, product descriptions and pages for website"
  dependsOn("documentation")
  dependsOn("copy_prod_desc_pdf")
  dependsOn("saxonc_documentation")
}

// ============================================================

val expath_doc = tasks.register("expath_doc") {
  // Just a place to hang dependencies
}

listOf<String>("binary", "archive", "file").forEach { target ->
  val t = tasks.register<JavaExec>("expath_doc_${target}") {
    group = "Internal"
    description = "Expand EXPath documentation for ${target}"

    val expath_root = layout.projectDirectory.file("../submodules/expath")

    inputs.file(layout.projectDirectory.file("src/userdoc/functions/EXPath/functions-${target}.xml"))
    inputs.files(layout.projectDirectory.dir("tools/userdoc"))
    outputs.file(layout.buildDirectory.file("src/userdoc/functions/EXPath/functions-${target}.xml"))
    classpath = configurations.named("documentation").get()
    mainClass = "com.saxonica.Transform"
    args("-xi:on",
         "-s:${layout.projectDirectory.file("src/userdoc/functions/EXPath/functions-${target}.xml")}",
         "-xsl:${layout.projectDirectory.file("tools/userdoc/genFunctionDoc.xsl")}",
         "-o:${layout.buildDirectory.file("src/userdoc/functions/EXPath/functions-${target}.xml").get().asFile}",
         // FIXME: What is this?
         "GitHub=${expath_root}/")
  }
  expath_doc { dependsOn(t) }
}

tasks.register<JavaExec>("expand_userdoc_sections") {
  group = "Internal"
  description = "Expand documentation sections from inclusions for app"
  dependsOn("gen_doc")

  inputs.file(layout.buildDirectory.file("src/userdoc/catalog.xml"))
  inputs.files(layout.buildDirectory.dir("src/userdoc"))
  inputs.files(layout.projectDirectory.dir("tools/userdoc"))
  outputs.file(layout.buildDirectory.file("userdoc/expanded/catalog.xml"))
  classpath = configurations.named("documentation").get()
  mainClass = "com.saxonica.Transform"
  args("-xi:on",
       "-s:${layout.buildDirectory.file("src/userdoc/catalog.xml").get().asFile}",
       "-xsl:${layout.projectDirectory.file("tools/userdoc/doc-include.xsl")}",
       "-o:${layout.buildDirectory.file("userdoc/expanded/catalog.xml").get().asFile}",
       "output-uri=${layout.buildDirectory.file("userdoc/expanded").get().asFile}/")
}

tasks.register<JavaExec>("userdoc_changes") {
  group = "Internal"
  description = "Preprocess the changes.xml file to turn it into a standard article"
  dependsOn("expand_userdoc_sections")

  inputs.file(layout.buildDirectory.file("userdoc/expanded/changes.xml"))
  inputs.files(layout.projectDirectory.dir("tools/to-static"))
  outputs.file(layout.buildDirectory.file("userdoc/expanded/changes-article.xml"))
  classpath = configurations.named("documentation").get()
  mainClass = "com.saxonica.Transform"
  args("-s:${layout.buildDirectory.file("userdoc/expanded/changes.xml").get().asFile}",
       "-xsl:${layout.projectDirectory.file("tools/to-static/c-changes.xsl")}",
       "-o:${layout.buildDirectory.file("userdoc/expanded/changes-article.xml").get().asFile}")
}

tasks.register<JavaExec>("userdoc_functions") {
  group = "Internal"
  description = "Preprocess the functions.xml file to turn it into a standard article"
  dependsOn("expand_userdoc_sections")

  inputs.file(layout.buildDirectory.file("userdoc/expanded/functions.xml"))
  inputs.files(layout.projectDirectory.dir("tools/to-static"))
  outputs.file(layout.buildDirectory.file("userdoc/expanded/functions-article.xml"))
  classpath = configurations.named("documentation").get()
  mainClass = "com.saxonica.Transform"
  args("-s:${layout.buildDirectory.file("userdoc/expanded/functions.xml").get().asFile}",
       "-xsl:${layout.projectDirectory.file("tools/to-static/c-functions.xsl")}",
       "-o:${layout.buildDirectory.file("userdoc/expanded/functions-article.xml").get().asFile}")
}

// ============================================================

tasks.register<JavaExec>("userdoc_html") {
  group = "Documentation"
  description = "Creates the static HTML version of the user documentation"
  dependsOn("userdoc_articles")

  inputs.files(layout.buildDirectory.dir("userdoc/expanded"))
  inputs.files(layout.projectDirectory.dir("tools/to-static"))
  outputs.dir(layout.buildDirectory.dir("userdoc/html"))
  classpath = configurations.named("documentation").get()
  mainClass = "com.saxonica.Transform"
  args("-s:${layout.buildDirectory.file("userdoc/expanded/catalog.xml").get().asFile}",
       "-xsl:${layout.projectDirectory.file("tools/to-static/s-viewer_app.xsl")}",
       "-o:${layout.buildDirectory.file("userdoc/html/dummy.xml").get().asFile}",
       "changes-article=${layout.buildDirectory.file("userdoc/expanded/changes-article.xml").get().asFile}",
       "functions-article=${layout.buildDirectory.file("userdoc/expanded/functions-article.xml").get().asFile}",
       "base-uri=${layout.buildDirectory.file("userdoc/html").get().asFile}/")
}

tasks.register<Copy>("userdoc_publish_javadoc_html") {
  group = "Internal"
  description = "Copy javadoc docs into HTML output"

  into(layout.buildDirectory.dir("userdoc/html/javadoc"))
  from(javadocSources)
}

tasks.register<Copy>("userdoc_publish_dotnet_html") {
  group = "Internal"
  description = "Copy dotnetdoc docs into HTML output"
  dependsOn("dotnet_doc_html")

  into(layout.buildDirectory.dir("userdoc/html/dotnetdoc"))
  from(layout.buildDirectory.dir("dotnetdoc-html"))
}

tasks.register<Copy>("userdoc_publish_html_images") {
  group = "Internal"
  description = "Copy viewer_appJS images to the html output directory"

  into(layout.buildDirectory.dir("userdoc/html/image"))
  from(layout.projectDirectory.dir("src/viewer_appJS/image"))
}

tasks.register<Copy>("userdoc_publish_html_style") {
  group = "Internal"
  description = "Copy viewer_appJS CSS to the html output directory"

  into(layout.buildDirectory.dir("userdoc/html"))
  from(layout.projectDirectory.file("src/viewer_appJS/viewer_app.css"))
  filter { line ->
    if (line.contains("url('saxon-base.css')")) {
      line.replace("saxon-base.css", "../../saxon-base.css")
    } else {
      line
    }
  }
}

tasks.register("userdoc_publish_html") {
  group = "Documentation"
  description = "Publish the HTML documentation"
  dependsOn("userdoc_html")
  dependsOn("userdoc_publish_javadoc_html")
  dependsOn("userdoc_publish_dotnet_html")
  dependsOn("userdoc_publish_html_style")
  dependsOn("userdoc_publish_html_images")
  // Just a place to hang dependencies
}

// ============================================================

val update_documentation = tasks.register("update_documentation") {
  group = "Documentation"
  description = "Update documentation from viewer app"
  outputs.dir(layout.buildDirectory.dir("userdoc/viewer_appJS"))
  dependsOn("copy_xml_documentation")
  dependsOn("copy_html_documentation")
  dependsOn("copy_base_css")
}
val userdocViewerAppJS by configurations.consumable("userdocViewerAppJS")
userdocViewerAppJS.outgoing.artifact(update_documentation) { builtBy(update_documentation) }

tasks.register<Copy>("copy_html_documentation") {
  group = "Internal"
  description = "Update HTML version of user documentation"
  dependsOn("userdoc_publish_html")
  into(layout.buildDirectory.dir("website/html/documentation${saxonMajorVersion}"))
  from(layout.buildDirectory.dir("userdoc/html"))
}

tasks.register<Copy>("copy_base_css") {
  group = "Internal"
  description = "Copy base CSS for HTML documentation"
  dependsOn("gen_contents_ref")
  into(layout.buildDirectory.dir("website"))
  from(layout.projectDirectory.dir("src/viewer_appJS")) {
    include("saxon-base.css")
  }
}

// ============================================================

tasks.register<JavaExec>("gen_contents_ref") {
  group = "Internal"
  description = "Creates the contents reference file for the user documentation (used for website pages)"
  dependsOn("copy_xml_documentation")
  
  val catalog = "website/documentation${saxonMajorVersion}/doc/catalog.xml"
  val contents = "website/documentation${saxonMajorVersion}/contents.xml"

  inputs.file(layout.buildDirectory.file(catalog))
  inputs.file(layout.projectDirectory.file("tools/userdoc/contents-builder.xsl"))
  outputs.file(layout.buildDirectory.file(contents))
  classpath = configurations.named("documentation").get()
  mainClass = "com.saxonica.Transform"
  args("-s:${layout.buildDirectory.file(catalog).get().asFile}",
       "-xsl:${layout.projectDirectory.file("tools/userdoc/contents-builder.xsl")}",
       "-o:${layout.buildDirectory.file(contents).get().asFile}",
       "version-number=${saxonMajorVersion}")
}

tasks.register<JavaExec>("gen_feature_matrix") {
  group = "Internal"
  description = "Creates the base feature matrix file for the website"

  val matrix = "website/products/feature-matrix-${saxonMajorVersion}.xml"

  inputs.file(layout.projectDirectory.file("tools/feature-matrix/feature-matrix.xml"))
  inputs.file(layout.projectDirectory.file("tools/feature-matrix/feature-matrix-to-overview-chart.xsl"))
  outputs.file(layout.buildDirectory.file(matrix))
  classpath = configurations.named("documentation").get()
  mainClass = "com.saxonica.Transform"
  args("-s:${layout.projectDirectory.file("tools/feature-matrix/feature-matrix.xml")}",
       "-xsl:${layout.projectDirectory.file("tools/feature-matrix/feature-matrix-to-overview-chart.xsl")}",
       "-o:${layout.buildDirectory.file(matrix).get().asFile}")
}

tasks.register("gen_prod_desc") {
  group = "Internal"
  description = "Creates the HTML and PDF versions of the product descriptions"
  dependsOn("gen_prod_desc_fo")
  dependsOn("gen_prod_desc_pdf")
  // Just a place to hang dependencies
}

tasks.register<JavaExec>("gen_prod_desc_xml") {
  group = "Internal"
  description = "Creates the base product description file"

  inputs.file(layout.projectDirectory.file("tools/feature-matrix/feature-matrix.xml"))
  inputs.file(layout.projectDirectory.file("tools/feature-matrix/feature-matrix-to-prod-desc.xsl"))
  outputs.file(layout.buildDirectory.file("proddesc/prod-desc.xml"))
  classpath = configurations.named("documentation").get()
  mainClass = "com.saxonica.Transform"
  args("-s:${layout.projectDirectory.file("tools/feature-matrix/feature-matrix.xml")}",
       "-xsl:${layout.projectDirectory.file("tools/feature-matrix/feature-matrix-to-prod-desc.xsl")}",
       "-o:${layout.buildDirectory.file("proddesc/prod-desc.xml").get().asFile}")
}

tasks.register<JavaExec>("gen_prod_desc_fo") {
  group = "Internal"
  description = "Creates the FO and HTML versions of the product descriptions"
  dependsOn("gen_prod_desc_xml")
  dependsOn("setup_fop")

  inputs.file(layout.buildDirectory.file("proddesc/prod-desc.xml"))
  inputs.file(layout.projectDirectory.file("tools/feature-matrix/PD/prod-desc-to-fo.xsl"))
  outputs.file(layout.buildDirectory.file("proddesc/PD/dummy.xml"))
  classpath = configurations.named("documentation").get()
  mainClass = "com.saxonica.Transform"
  args("-s:${layout.buildDirectory.file("proddesc/prod-desc.xml").get().asFile}",
       "-xsl:${layout.projectDirectory.file("tools/feature-matrix/PD/prod-desc-to-fo.xsl")}",
       "-o:${layout.buildDirectory.file("proddesc/PD/dummy.xml").get().asFile}")
}

tasks.register<Copy>("setup_fop") {
  group = "Internal"
  into(layout.buildDirectory.file("proddesc"))
  from(layout.projectDirectory.file("src/main/resources/img")) {
    include("saxonica_logo.gif")
  }
}

val gen_prod_desc_pdf = tasks.register("gen_prod_desc_pdf") {
  group = "Internal"
  description = "Creates the PDF versions of the product descriptions"
  // Just a place to hang dependencies
}

val variants = mutableListOf("JS")
listOf("J", "C").forEach { plat ->
  listOf("EEQ", "EET", "EEV", "EE", "PE", "HE").forEach { pk ->
    variants.add("${plat}-${pk}")
  }
}
listOf("EEQ", "EET", "EEV", "EE").forEach { pk ->
  variants.add("CS-${pk}")
}

variants.forEach { ds ->
  val t = tasks.register<JavaExec>("gen_${ds}_desc_pdf") {
    group = "Internal"
    description = "Creates the PDF version of the ${ds} product description"
    dependsOn("setup_fop")
    dependsOn("gen_prod_desc_fo")

    inputs.file(layout.buildDirectory.file("proddesc/PD/fo/${ds}.fo"))
    outputs.file(layout.buildDirectory.file("proddesc/PD/pdf/Saxon${ds}.pdf"))
    classpath = configurations.named("documentation").get()
    mainClass = "org.apache.fop.cli.Main"
    args("-q",
         "${layout.buildDirectory.file("proddesc/PD/fo/${ds}.fo").get().asFile}",
         "-pdf",
         "${layout.buildDirectory.file("proddesc/PD/pdf/Saxon${ds}.pdf").get().asFile}")
  }
  gen_prod_desc_pdf { dependsOn(t) }
}

tasks.register<Copy>("copy_prod_desc_pdf") {
  group = "Internal"
  description = "Copy the generated product descriptions to the website"
  dependsOn("gen_prod_desc_pdf")

  into(layout.buildDirectory.dir("website/products/PD${saxonMajorVersion}"))
  from(layout.buildDirectory.dir("proddesc/PD/pdf"))
}

// ============================================================

tasks.register<Copy>("saxonc_documentation") {
  group = "Documentation"
  description = "Build the SaxonC documentation for website"
  dependsOn("copy_saxonc_doc")
  dependsOn("copy_saxonc_css")
  dependsOn("saxonc_doc_sef")
  dependsOn("saxonc_api_doc")

  into(layout.buildDirectory.dir("website/saxon-c"))
  from(layout.buildDirectory.dir("saxonc-doc")) {
    exclude("documentation${saxonMajorVersion}/*.css")
  }
}

tasks.register<Copy>("copy_saxonc_doc") {
  group = "Internal"
  description = "Copy the SaxonC documentation source"

  into(layout.buildDirectory.dir("saxonc-doc/documentation${saxonMajorVersion}"))
  from(layout.projectDirectory.dir("src/docs/saxonc-doc")) {
    exclude("xsl/*.xsl")
  }
}

tasks.register<Copy>("copy_saxonc_css") {
  group = "Internal"
  description = "Copy SaxonC documentation CSS"

  into(layout.buildDirectory.dir("website/saxon-c/documentation${saxonMajorVersion}"))
  from(layout.projectDirectory.dir("src/docs/saxonc-doc")) {
    include("jstree.css")
    include("prism.css")
  }
  filter { line ->
    if (line.contains("url('saxon-base.css')")) {
      line.replace("saxon-base.css", "../../saxon-base.css")
    } else {
      line
    }
  }
}

tasks.register<JavaExec>("saxonc_doc_sef") {
  group = "Internal"
  description = "Compile the viewer_app stylesheet for the SaxonC documentation"

  val sef = "saxonc-doc/documentation${saxonMajorVersion}/xsl/jstree.sef.json"

  inputs.files(layout.projectDirectory.dir("src/docs/saxonc-doc/xsl"))
  outputs.file(layout.buildDirectory.file(sef))
  classpath = configurations.named("documentation").get()
  mainClass = "com.saxonica.Transform"
  args("-xsl:${layout.projectDirectory.file("src/docs/saxonc-doc/xsl/jstree.xsl")}",
       "-export:${layout.buildDirectory.file(sef).get().asFile}",
       "-target:JS", "-nogo", "-relocate:on", "-ns:##html5",
       "--generateByteCode:off")
}

val saxonc_api_doc = tasks.register("saxonc_api_doc") {
  inputs.files(doxygenDocumentation)
  inputs.files(doxygenCDocumentation)
  outputs.dir(layout.buildDirectory.dir("saxonc-doc"))

  val outputDir = layout.buildDirectory.dir("saxonc-doc/doc${saxonMajorVersion}")

  doLast {
    copy {
      into(outputDir.map { it.dir("html") })
      from(doxygenDocumentation)
        include("saxoncee.html")
        rename {
          "saxonc.html"
        }
        filter { line ->
          line.replace("saxoncee.html", "saxonc.html")
        }
    }
  }

  doLast {
    copy {
      from(doxygenCDocumentation)
      into(outputDir)
    }
  }
}
val saxoncApiDoc by configurations.consumable("saxoncApiDocArtifacts")
saxoncApiDoc.outgoing.artifact(saxonc_api_doc) { builtBy(saxonc_api_doc) }
