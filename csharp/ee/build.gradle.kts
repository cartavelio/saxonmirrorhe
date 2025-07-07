import com.saxonica.build.*
import com.saxonica.build.java.JarFinder

plugins {
  id("java-library")
  id("saxon-build")
  id("com.saxonica.build.saxoncs")
  id("com.saxonica.build.testsuites")
  id("com.saxonica.build.samples")
}

repositories {
  mavenLocal()
  mavenCentral()
  maven { url = uri("https://maven.saxonica.com/maven") }
}

val saxonBuild = the<SaxonBuildExtension>()
val saxonCS = the<SaxonCSExtension>()

configure<SaxonCSExtension> {
  edition = "ee"
  classpath = configurations.compileClasspath
}

val javaSourcesBucket by configurations.dependencyScope("javaSourcesBucket")
val csharpFeatureKeysBucket by configurations.dependencyScope("csharpFeatureKeysBucket")
val javaFeatureKeysBucket by configurations.dependencyScope("javaFeatureKeysBucket")
val transpilerRuntime by configurations.dependencyScope("transpilerRuntime")
val javaEeRuntime by configurations.dependencyScope("javaEeRuntime")

val releaseArtifacts by configurations.consumable("releaseArtifacts")
val apiSourceArtifacts by configurations.consumable("apiSourceArtifacts")
val htmlDocArtifacts by configurations.consumable("htmlDocArtifacts")
val xmlDocArtifacts by configurations.consumable("xmlDocArtifacts")

val preprocessor by configurations.dependencyScope("preprocessor")
val preprocessorClasspath by configurations.resolvable("preprocessorClasspath") {
    extendsFrom(preprocessor)
}

val sampleSourcesBucket by configurations.dependencyScope("sampleSourcesBucket")
val sampleSources = configurations.resolvable("sampleSources") {
  extendsFrom(sampleSourcesBucket)
}

dependencies {
  javaSourcesBucket(files(layout.projectDirectory.files("../../java/src/main/java")))
  csharpFeatureKeysBucket(project(mapOf("path" to ":featurekeys",
                                    "configuration" to "csharpFeatureKeys")))
  javaFeatureKeysBucket(project(mapOf("path" to ":featurekeys",
                                      "configuration" to "javaFeatureKeys")))
  sampleSourcesBucket(project(mapOf("path" to ":samples",
                                    "configuration" to "sampleSources")))

  transpilerRuntime(project(":csharp:transpiler", "runtimeElements"))
  transpilerRuntime("com.ibm.icu:icu4j:${findProperty("icu4jVersion")}")
  transpilerRuntime("org.xmlresolver:xmlresolver:${findProperty("xmlresolverVersion")}")
  transpilerRuntime("org.xmlresolver:xmlresolver:${findProperty("xmlresolverVersion")}:data")
  transpilerRuntime(files(saxonBuild.saxonLicenseDir.get()))

  javaEeRuntime(project(":java", "runtimeElements"))

  compileOnly(project(":csharp:transpiler", "runtimeElements"))
  compileOnly("com.saxonica:Saxon-EE:12.4")

  preprocessor("com.igormaznitsa:jcp:7.0.4")
  preprocessor("com.tonicsystems:jarjar:0.6")
  preprocessor("com.saxonica:Saxon-EE:12.4")
  preprocessor(files(saxonBuild.saxonLicenseDir.get()))
}

val javaSources = configurations.resolvable("javaSources") {
  extendsFrom(javaSourcesBucket)
}

val csharpFeatureKeysSources = configurations.resolvable("csharpFeatureKeysSources") {
  extendsFrom(csharpFeatureKeysBucket)
}

val javaFeatureKeysSources = configurations.resolvable("javaFeatureKeysSources") {
  extendsFrom(javaFeatureKeysBucket)
}

val transpiler = configurations.resolvable("transpiler") {
  extendsFrom(transpilerRuntime)
  extendsFrom(javaEeRuntime)
}

val javaEe = configurations.resolvable("javaEe") {
  extendsFrom(javaEeRuntime)
}  

val preprocessJava = tasks.register<JavaExec>("preprocessJava") {
  inputs.files(javaSources)
  inputs.files(javaFeatureKeysSources)
  outputs.dir(layout.buildDirectory.dir("src/main/java"))

  doFirst {
    copy {
      from(files(javaSources)
               .asFileTree
               .matching {
                 include("net/sf/saxon/**/*.java")
                 include("com/saxonica/**/*.java")
                 include("net/sf/saxon/data/**")
                 exclude("**/package-info.java")
                 exclude("net/sf/saxon/dotnet/**")
                 exclude("net/sf/saxon/event/ContentHandlerProxy.java")
                 exclude("net/sf/saxon/event/ContentHandlerProxyLocator.java")
                 exclude("net/sf/saxon/event/PIGrabber.java")
                 exclude("net/sf/saxon/event/ReceivingContentHandler.java")
                 exclude("net/sf/saxon/event/RepairingContentHandler.java")
                 exclude("net/sf/saxon/event/StreamWriterToReceiver.java")
                 exclude("net/sf/saxon/expr/JavaExtensionFunctionCall.java")
                 exclude("net/sf/saxon/expr/JPConverter.java")
                 exclude("net/sf/saxon/expr/PJConverter.java")
                 exclude("net/sf/saxon/expr/SimpleExpression.java")
                 exclude("net/sf/saxon/expr/sort/ItemOrderComparer.java")
                 exclude("net/sf/saxon/expr/sort/RuleBasedSubstringMatcher.java")
                 exclude("net/sf/saxon/expr/sort/UcaCollatorUsingJava.java")
                 exclude("net/sf/saxon/functions/registry/OnDemandFunctionSet.java")
                 exclude("net/sf/saxon/functions/registry/OnDemandFunctionSet.java")
                 exclude("net/sf/saxon/lib/CatalogResourceResolver.java")
                 exclude("net/sf/saxon/lib/ChainedEntityResolver.java")
                 exclude("net/sf/saxon/lib/ConfigurableResourceResolver.java")
                 exclude("net/sf/saxon/lib/DirectResourceResolver.java")
                 exclude("net/sf/saxon/lib/EntityResolverWrappingResourceResolver.java")
                 exclude("net/sf/saxon/lib/ErrorReporterToListener.java")
                 exclude("net/sf/saxon/lib/ExternalObjectModel.java")
                 exclude("net/sf/saxon/lib/InvalidityHandlerWrappingErrorListener.java"  )
                 exclude("net/sf/saxon/lib/RelativeURIResolver.java" )
                 exclude("net/sf/saxon/lib/ResourceResolverWrappingURIResolver.java" )
                 exclude("net/sf/saxon/lib/StandardEntityResolver.java")
                 exclude("net/sf/saxon/lib/StandardErrorHandler.java")
                 exclude("net/sf/saxon/lib/StandardErrorListener.java")
                 exclude("net/sf/saxon/lib/StandardLogger.java")
                 exclude("net/sf/saxon/lib/StandardMessageHandler.java")
                 //exclude("net/sf/saxon/lib/StandardModuleURIResolver.java")
                 exclude("net/sf/saxon/lib/StandardOutputResolver.java")
                 //exclude("net/sf/saxon/lib/StandardUnparsedTextResolver.java")
                 exclude("net/sf/saxon/lib/StandardURIResolver.java")
                 exclude("net/sf/saxon/lib/SystemLogger.java")
                 exclude("net/sf/saxon/lib/DigestMaker.java")
                 exclude("net/sf/saxon/ma/arrays/ImmutableArrayItem.java")
                 exclude("net/sf/saxon/ma/map/HashTrieMap.java")
                 exclude("net/sf/saxon/ma/trie/**")
                 exclude("net/sf/saxon/om/EnumSetTool.java")
                 exclude("net/sf/saxon/om/One.java")
                 exclude("net/sf/saxon/om/OneOrMore.java")
                 exclude("net/sf/saxon/om/ZeroOrMore.java")
                 exclude("net/sf/saxon/om/ZeroOrOne.java")
                 exclude("net/sf/saxon/pull/ActiveStAXSource.java")
                 exclude("net/sf/saxon/pull/NamespaceContextImpl.java")
                 exclude("net/sf/saxon/pull/StaxBridge.java")
                 exclude("net/sf/saxon/resource/ActiveSAXSource.java")
                 exclude("net/sf/saxon/resource/ActiveStreamSource.java")
                 exclude("net/sf/saxon/resource/JarCollection.java")
                 exclude("net/sf/saxon/resource/ResourceLoader.java")
                 exclude("net/sf/saxon/sapling/*.java")
                 exclude("net/sf/saxon/s9api/BuildingContentHandler.java")
                 exclude("net/sf/saxon/s9api/BuildingStreamWriter.java")
                 exclude("net/sf/saxon/s9api/BuildingStreamWriterImpl.java")
                 exclude("net/sf/saxon/s9api/DOMDestination.java")
                 exclude("net/sf/saxon/s9api/HostLanguage.java")
                 exclude("net/sf/saxon/s9api/ItemType.java")
                 exclude("net/sf/saxon/s9api/ItemTypeFactory.java")
                 exclude("net/sf/saxon/s9api/JdomBuilder.java")
                 exclude("net/sf/saxon/s9api/MessageListener2Proxy.java")
                 exclude("net/sf/saxon/s9api/SaxonApiException.java")
                 exclude("net/sf/saxon/s9api/SaxonApiUncheckedException.java")
                 exclude("net/sf/saxon/s9api/SequenceType.java")
                 exclude("net/sf/saxon/s9api/SAXDestination.java")
                 exclude("net/sf/saxon/s9api/SequenceType.java")
                 exclude("net/sf/saxon/s9api/streams/**")
                 exclude("net/sf/saxon/s9api/QName.java")
                 exclude("net/sf/saxon/s9api/ValidationStatistics.java")
                 exclude("net/sf/saxon/s9api/WhitespaceStrippingPolicy.java")
                 exclude("net/sf/saxon/s9api/XdmExternalObject.java")
                 exclude("net/sf/saxon/s9api/XdmArray.java")
                 exclude("net/sf/saxon/s9api/XdmAtomicValue.java")
                 exclude("net/sf/saxon/s9api/XdmEmptySequence.java")
                 exclude("net/sf/saxon/s9api/XdmFunctionItem.java")
                 exclude("net/sf/saxon/s9api/XdmItem.java")
                 exclude("net/sf/saxon/s9api/XdmItemType.java")
                 exclude("net/sf/saxon/s9api/XdmMap.java")
                 exclude("net/sf/saxon/s9api/XdmNode.java")
                 exclude("net/sf/saxon/s9api/XdmValue.java")
                 exclude("net/sf/saxon/serialize/charcode/JavaCharacterSet.java")
                 exclude("net/sf/saxon/serialize/codenorm/NormalizerData.java")
                 exclude("net/sf/saxon/sort/ItemOrderComparer.java")
                 exclude("net/sf/saxon/trace/ExitCounter.java")
                 exclude("net/sf/saxon/trace/Locksmith.java")
                 exclude("net/sf/saxon/trace/TracingFilter.java")
                 exclude("net/sf/saxon/trace/TimingTraceListener.java")
                 exclude("net/sf/saxon/trace/TracingFilter.java")
                 exclude("net/sf/saxon/trans/DynamicLoader.java")
                 exclude("net/sf/saxon/trans/NonDelegatingURIResolver.java")
                 exclude("net/sf/saxon/trans/XmlCatalogResolver.java")
                 exclude("net/sf/saxon/tree/iter/NodeWrappingAxisIterator.java")
                 exclude("net/sf/saxon/tree/iter/NodeWrappingFunction.java")
                 exclude("net/sf/saxon/tree/jiter/TopDownStackIterable.java")
                 exclude("net/sf/saxon/tree/jiter/MappingJavaIterator.java")
                 exclude("net/sf/saxon/tree/lib/StandardEntityResolver.java")
                 exclude("net/sf/saxon/tree/lib/StandardErrorHandler.java")
                 exclude("net/sf/saxon/tree/util/AttributeCollectionImpl.java")
                 exclude("net/sf/saxon/tree/util/ProcInstParser.java")
                 exclude("net/sf/saxon/value/SaxonDuration.java")
                 exclude("net/sf/saxon/value/SaxonXMLGregorianCalendar.java")
                 exclude("**/TransformerFactoryImpl.java")
                 exclude("net/sf/saxon/BasicTransformerFactory.java")
                 exclude("net/sf/saxon/Gizmo.java")
                 exclude("net/sf/saxon/ma/parray/**")
                 exclude("**/ant/**")
                 exclude("**/dom/**")
                 exclude("**/dom4j/**")
                 exclude("**/gizmo/**")
                 exclude("**/java/**")
                 exclude("**/option/**")
                 exclude("**/testdriver/**")
                 exclude("**/jaxp/**")
                 exclude("**/jdom/**")
                 exclude("**/jdom2/**")
                 exclude("**/axiom/**")
                 //exclude("**/sapling/**")
                 exclude("**/sql/**")
                 exclude("**/transpile/**")
                 exclude("**/xom/**")
                 exclude("**/xpath/**")
                 exclude("**/xqj/**")
                 exclude("**/stax/**")
                 exclude("com/saxonica/Idempotency.java")
                 exclude("com/saxonica/config/DotNetExtensionFunctionFactory.java")
                 exclude("com/saxonica/config/DotNetExtensionLibrary.java")
                 exclude("com/saxonica/config/DotNetLocalizerFactory.java")
                 exclude("com/saxonica/config/DotNetPlatformPE.java")
                 exclude("com/saxonica/config/EnterpriseTransformerFactory.java")
                 exclude("com/saxonica/config/EnterpriseXPathFactory.java")
                 exclude("com/saxonica/config/ICULibrary.java")
                 exclude("com/saxonica/config/JavaExtensionFunctionFactory.java")
                 exclude("com/saxonica/config/JavaExtensionLibrary.java")
                 exclude("com/saxonica/config/JavaLocalizerFactory.java")
                 exclude("com/saxonica/config/JavaPlatformEE.java")
                 exclude("com/saxonica/config/JavaPlatformPE.java")
                 exclude("com/saxonica/config/JavaVerifier.java")
                 exclude("com/saxonica/config/NativePlatformPE.java")
                 exclude("com/saxonica/config/ProfessionalTransformerFactory.java")
                 exclude("com/saxonica/config/ProfessionalXPathFactory.java")
                 exclude("com/saxonica/config/StreamingTransformerFactory.java")
                 exclude("com/saxonica/config/pe/DotNetExtensionFunctionFactory.java")
                 exclude("com/saxonica/config/pe/DotNetExtensionLibrary.java")
                 exclude("com/saxonica/config/pe/DynamicLoaderPE.java")
                 exclude("com/saxonica/config/pe/ICUNumberer_de.java")
                 exclude("com/saxonica/config/pe/ICUNumbererEnUnhyphenated.java")
                 exclude("com/saxonica/config/pe/ICUNumbererPE.java")
                 exclude("com/saxonica/config/pe/NativePlatformPE.java")
                 exclude("com/saxonica/ee/bytecode/**")
                 exclude("com/saxonica/ee/domino/**")
                 exclude("com/saxonica/ee/config/DynamicLoaderEE.java")
                 exclude("com/saxonica/ee/extfn/ExperimentalSequenceTypeMarshaller.java")
                 exclude("com/saxonica/ee/extfn/ExperimentalXPathCompiler.java")
                 exclude("com/saxonica/ee/jaxp/**")
                 exclude("com/saxonica/ee/parallel/MultithreadedContextMappingIterator.java")
                 exclude("com/saxonica/ee/parallel/MultithreadedItemMappingIterator.java")
                 exclude("com/saxonica/ee/parallel/PushToPull.java")
                 exclude("com/saxonica/ee/schema/sdoc/LSSchemaResolver.java")
                 exclude("com/saxonica/ee/validate/StatisticsReporter.java")
                 exclude("com/saxonica/expr/DotNetExtensionFunctionCall.java")
                 exclude("com/saxonica/expr/JavaExtensionFunctionCall.java")
                 exclude("com/saxonica/expr/ObjectLookupExpression.java")
                 //exclude("com/saxonica/expr/sort/UcaCollationKeyUsingIcu.java")
                 //exclude("com/saxonica/expr/sort/UcaCollatorUsingIcu.java")
                 exclude("com/saxonica/functions/sql/**")
                 exclude("com/saxonica/functions/extfn/cpp/**")
                 exclude("com/saxonica/functions/extfn/EXPathArchive/**")
                 exclude("com/saxonica/functions/extfn/LastModified.java")
                 exclude("com/saxonica/functions/extfn/Mailer2.java")
                 exclude("com/saxonica/functions/extfn/ObjectMap.java")
                 exclude("com/saxonica/functions/qt4/ParseHtml.java")
                 //exclude("com/saxonica/functions/extfn/ReadBinaryResource.java")
                 exclude("com/saxonica/functions/extfn/SendMail.java")
                 exclude("com/saxonica/xqj/**")
                 exclude("com/saxonica/functions/qt4/ixml/**")
               })
      into(layout.buildDirectory.dir("tmp/java"))
      filter { line ->
        if (line.contains("net.sf.saxon.xpath.JAXPXPathStaticContext")
                || line.contains("import com.saxonica.ee.jaxp.SchemaFactoryImpl")
                || line.contains("import net.sf.saxon.stax")
                || line.contains("import net.sf.saxon.jaxp")) {
          "// " + line
        } else {
          line
        }
      }
    }

    copy {
      from(files(javaFeatureKeysSources))
      into(layout.buildDirectory.dir("tmp/java"))
    }
  }

  doFirst {
    delete(layout.buildDirectory.dir("tmp/java"))
    mkdir(layout.buildDirectory.dir("tmp/java"))
  }

  classpath = preprocessorClasspath
  mainClass = "com.igormaznitsa.jcp.JcpPreprocessor"
  args("/c",
       "/i:${layout.buildDirectory.dir("tmp/java").get()}",
       "/o:${layout.buildDirectory.dir("src/main/java").get()}",
       "/p:EE=true", "/p:PE=false",
       "/p:SAXONC=false", "/p:BYTECODE=false", "/p:HOF=true",
       "/p:STREAM=true", "/p:SCHEMA=true", "/p:CSHARP=true")
}

val jarFinder = JarFinder(configurations.named("transpiler").get())

val transpileToXml = tasks.register<JavaExec>("transpileToXml") {
  group = LifecycleBasePlugin.BUILD_GROUP
  inputs.files(preprocessJava)
  outputs.dir(layout.buildDirectory.dir("tmp/xml"))
  doFirst {
    mkdir("${layout.buildDirectory.dir("tmp/xml").get()}")
  }

  classpath = configurations.named("transpiler").get()
  mainClass = "ToXml"
  args("-j:${layout.buildDirectory.dir("src").get()}",
       "-repo:${layout.projectDirectory.file("../transpiler/")}",
       "-xml:${layout.buildDirectory.dir("tmp/xml").get()}",
       "-h:${layout.buildDirectory.dir("src/main/java").get()}",
       "-src:${layout.buildDirectory.dir("src/main/java").get()}",
       "-ee:${javaEe.get().map { it }.get(0)}",
       "-icu4j:${jarFinder.find("icu4j-${findProperty("icu4jVersion")}.jar")}")
}

val transpileToDigest = tasks.register<JavaExec>("transpileToDigest") {
  group = LifecycleBasePlugin.BUILD_GROUP
  inputs.dir(layout.projectDirectory.dir("../transpiler/tools/xslt"))
  inputs.files(transpileToXml)
  outputs.file(layout.buildDirectory.file("tmp/digest.xml"))

  // Fix Windows filenames so that Java doesn't get confused
  val xdigest = saxonBuild.puri("${layout.projectDirectory.file("../transpiler/tools/xslt/xml-to-digest.xsl")}")
  val xmldir = saxonBuild.puri("${layout.buildDirectory.dir("tmp/xml").get()}")
  val tmpdigest = saxonBuild.puri("${layout.buildDirectory.file("tmp/digest.xml").get()}")

  classpath = preprocessorClasspath
  mainClass = "net.sf.saxon.Transform"
  args("-it",
       "-xsl:${xdigest}",
       "-o:${tmpdigest}",
       "xmlDir=${xmldir}")
}

val transpile = tasks.register<JavaExec>("transpile") {
  group = LifecycleBasePlugin.BUILD_GROUP
  inputs.files(transpileToDigest)
  outputs.file(layout.buildDirectory.file("digest.xml"))

  // Fix Windows filenames so that Java doesn't get confused
  val rdigest = saxonBuild.puri("${layout.projectDirectory.file("../transpiler/tools/xslt/refine-digest.xsl")}")
  val xmldir = saxonBuild.puri("${layout.buildDirectory.dir("tmp/xml").get()}")
  val tmpdigest = saxonBuild.puri("${layout.buildDirectory.file("tmp/digest.xml").get()}")
  val digest = saxonBuild.puri("${layout.buildDirectory.file("digest.xml").get()}")

  classpath = preprocessorClasspath
  mainClass = "net.sf.saxon.Transform"
  args("-xsl:${rdigest}",
       "-s:${tmpdigest}",
       "-o:${digest}",
       "xmlDir=${xmldir}")
}

val generateCode = tasks.register<JavaExec>("generateCode") {
  group = LifecycleBasePlugin.BUILD_GROUP
  inputs.files(transpile)
  inputs.files(layout.buildDirectory.dir("tmp/xml"))
  outputs.dir(layout.buildDirectory.dir("generated/Saxon"))
  
  classpath = configurations.named("transpiler").get()
  classpath += javaEe.get()

  mainClass = "ToXml"
  args("-cs:${layout.buildDirectory.dir("generated").get()}",
       "-xml:${layout.buildDirectory.dir("tmp/xml/main/java").get()}",
       "-repo:${layout.projectDirectory}/../transpiler/",
       "-digest:${layout.buildDirectory.file("digest.xml").get()}",
       "-ee:${javaEe.get().map { it }.get(0)}",
       "-icu4j:${jarFinder.find("icu4j-${findProperty("icu4jVersion")}.jar")}",
       "-debug")

}

val preprocessCS = tasks.register<Copy>("preprocessCS") {
  from(layout.projectDirectory.dir("../src/main/cs"))
  into(layout.buildDirectory.dir("cs"))
}

val copyResources = tasks.register("copyResources") {
  doLast {
    copy {
      from(layout.projectDirectory.dir(".."))
      into(layout.buildDirectory)
      include("LICENSE.txt")
      include("README.md")
    }
    copy {
      from(layout.projectDirectory.dir("../../java/src/main/resources"))
      into(layout.buildDirectory.dir("resources"))
      exclude("**/.DS_Store")
    }
  }
}

// I'm trying to make this build practical to manage and yet flexible enough
// to handle the complexity of CS builds. For CS, we build for x64 or arm64,
// depending on the platform and architecture. In addition, for MacOS in a
// Release build, we build *both* of those and make a universal binary.
val architectures = if (findProperty("buildArch") != null) {
  listOf(findProperty("buildArch").toString())
} else if (saxonBuild.systemOsName.get() == "mac"
               && saxonCS.release.get() == "Release") {
  listOf("arm64", "x64")
} else {
  listOf(saxonCS.arch.get())
}

val configure_task = tasks.register("dotnetConfigure") {
  dependsOn("generateCode")
  dependsOn("copyResources")
}

val build_task = tasks.register("dotnetBuild")
val publish_task = tasks.register("dotnetPublish")
val makeapp_task = tasks.register("dotnetMakeApp")
for (buildArch in architectures) {
  // These tasks run every time because the .nupkg file changes every time.
  // The DotNet tasks use the outputDir as an output directory. This should
  // probably be reworked so that only the /obj/ directory is considered,
  // or something.
  val configure = tasks.register<DotNetConfiguration>("configure_${buildArch}") {
    group = LifecycleBasePlugin.BUILD_GROUP
    arch = buildArch
    inputs.files(layout.projectDirectory.file("../tools/generateCSProj.xsl"))
    inputs.files(layout.projectDirectory.file("../tools/generateTestDriverCSProj.xsl"))
    outputs.files(layout.buildDirectory.file("${buildArch}/SaxonCS/SaxonCS.csproj"))
    outputs.files(layout.buildDirectory.file("${buildArch}/TestDrivers/TestDrivers.csproj"))
  }
  configure_task { dependsOn(configure) }

  val build = tasks.register<DotNetBuild>("dotnetBuild_${buildArch}") {
    group = LifecycleBasePlugin.BUILD_GROUP
    dependsOn("dotnetConfigure")
    target = "SaxonCS"
    arch = buildArch
    inputs.files(configure)
    inputs.files(preprocessCS)
    inputs.files(generateCode)
    inputs.files(copyResources)
    inputs.files(csharpFeatureKeysSources)
    outputs.dir(layout.buildDirectory.dir("${buildArch}/SaxonCS"))
  }
  build_task { dependsOn(build) }


  val platform = saxonCS.platform.get()
  val csrid = "${platform}-${buildArch}"
  val release = saxonCS.release.get()

  val publish = tasks.register<DotNetPublish>("dotnetPublish_${buildArch}") {
    LifecycleTasks.RELEASE_GROUP
    arch = buildArch
    inputs.files(build)
    outputs.dir(layout.buildDirectory.dir("${buildArch}/SaxonCS/bin/${release}/net8.0/${csrid}/publish"))
  }
  publish_task { dependsOn(publish) }

  val makeapp = tasks.register<DotNetMakeApp>("dotnetMakeApp_${buildArch}") {
    LifecycleTasks.RELEASE_GROUP
    arch = buildArch
    inputs.files(build)
  }
  makeapp_task { dependsOn(makeapp) }
}

if ((saxonBuild.systemOsName.get() == "mac"
         && saxonCS.release.get() == "Release"
         && architectures.size == 2)) {
  val buildOsxApp = tasks.register<Exec>("buildOsxApp") {
    group = LifecycleTasks.RELEASE_GROUP
    inputs.files(makeapp_task)
    outputs.file(layout.buildDirectory.file("SaxonCS.app/Contents/MacOS/SaxonCS"))

    val arm64 = layout.buildDirectory.dir("arm64/SaxonCS/bin/Release/net${saxonBuild.dotnetversion.get()}").get()
    val x64 = layout.buildDirectory.dir("x64/SaxonCS/bin/Release/net${saxonBuild.dotnetversion.get()}").get()

    doFirst {
      // If a previous build put the docs in there, delete them; they aren't
      // allowed in a signed build
      delete(layout.buildDirectory.dir("SaxonCS.app/doc"))
      delete(layout.buildDirectory.dir("SaxonCS.app/SaxonCS"))
    }

    doFirst {
      copy {
        from("${arm64}/osx-arm64/publish/SaxonCS.app")
        into(layout.buildDirectory.dir("SaxonCS.app"))
        exclude("Contents/MacOS/SaxonCS")
      }
    }

    workingDir = layout.buildDirectory.get().asFile
    commandLine("lipo", "-create", "-output",
                "${layout.buildDirectory.file("SaxonCS.app/Contents/MacOS/SaxonCS").get()}",
                "${arm64}/osx-arm64/publish/SaxonCS.app/Contents/MacOS/SaxonCS",
                "${x64}/osx-x64/publish/SaxonCS.app/Contents/MacOS/SaxonCS")
  }

  val osxApp = tasks.register<Exec>("osxApp") {
    inputs.files(buildOsxApp)
    outputs.dir(layout.buildDirectory.dir("SaxonCS.app"))

    // Putting documentation in the app causes the signature to fail
    // "unsealed content is present in the bundle root"
    commandLine("codesign", "--force", "--options", "runtime",
                "--entitlements",
                "${layout.projectDirectory.file("../tools/entitlements.plist")}",
                "--deep", "--sign", "Developer ID Application: Saxonica Limited",
                "--timestamp",
                "${layout.buildDirectory.file("SaxonCS.app").get()}")
  }
  makeapp_task { finalizedBy(osxApp) }

  // We can't copy the documentation into the distribution because we've
  // signed it
  tasks.register<Zip>("dotnetPublishZip") {
    archiveFileName = "SaxonCS-${saxonBuild.version.get().replace(".", "-")}-MacOS.zip"
    from(osxApp)
    into("/SaxonCS.app/")
  }
} else {
  val ziparch = if (architectures[0] == "arm64") "aarch64" else "x86_64"
  val zipver = saxonBuild.version.get().replace(".", "-")
  val zipname = if (saxonBuild.systemOs.get().isWindows()) {
    "Windows"
  } else if (saxonBuild.systemOs.get().isMacOsX()) {
    "MacOS"
  } else {
    "Linux"
  }

  val stageZip = tasks.register("stageZip") {
    inputs.files(publish_task)
    inputs.files(layout.projectDirectory.dir("../../src/main/zipdoc"))
    outputs.dir(layout.buildDirectory.dir("SaxonCS.app"))

    doFirst {
      delete(layout.buildDirectory.dir("SaxonCS.app"))
    }

    doLast {
      copy {
	from(tasks.named("dotnetPublish_${architectures[0]}").get().outputs)
        into(layout.buildDirectory.dir("SaxonCS.app/SaxonCS"))
      }
      
      copy {
        from(layout.projectDirectory.dir("../../src/main/zipdoc"))
        into(layout.buildDirectory.dir("SaxonCS.app/doc"))
      }
    }
  }

  tasks.register<Zip>("dotnetPublishZip") {
    var dirver = saxonBuild.version.get()
    if (dirver.endsWith(".0")) {
       dirver = dirver.substring(0, dirver.length - 2)
     }

    archiveFileName = "SaxonCS-${zipver}-${zipname}-${ziparch}.zip"
    from(stageZip)
    into("/SaxonCS-${dirver}/")
  }
}

val artifacts = tasks.register("downloadRelease") {
  inputs.files(tasks.named("dotnetPublishZip"))
  outputs.dir(layout.buildDirectory.dir("releases"))
  group = "build"

  doLast {
    copy {
      into(layout.buildDirectory.dir("releases"))
      from(tasks.named("dotnetPublishZip"))
    }
    
    copy {
      into(layout.buildDirectory.dir("releases"))
      from(layout.buildDirectory.dir("bin/${architectures[0]}/SaxonCS.${saxonBuild.version.get()}.nupkg"))
    }
  }
}

releaseArtifacts.outgoing.artifact(artifacts)

val copyTestDrivers = tasks.register<Copy>("copyTestDrivers") {
  group = "Internal"
  description = "Build the C# test drivers"
  from(layout.projectDirectory.dir("../src/test/testrunner"))
  into(layout.buildDirectory.dir("src/testrunner"))
}

// ============================================================ 
// Documentation tasks

// Work out where *this* build of *this* release put the API documentation
// and move it to a consistent location we can refer to from elsewhere.
// If more than one architecture is being built, we only need the first one.
val apiDocumentation = tasks.register<Copy>("apiDocumentation") {
  val arch = architectures.first()
  val release = saxonCS.release.get()
  val archdir = "${saxonCS.platform.get()}-${arch}"
  val dotnetver = saxonBuild.dotnetversion.get()
  val buildtask = tasks.named("dotnetBuild_${arch}")
  dependsOn(buildtask)
  into(layout.buildDirectory.dir("apidoc"))
  from("${buildtask.get().outputs.getFiles().first()}/obj/${release}/net${dotnetver}/${archdir}") {
    include("SaxonCS.xml")
  }
}

val docParse = tasks.register<JavaExec>("docParse") {
  group = "Internal"
  description = "Parse dotnet cs files for docs"
  dependsOn("dotnetBuild")

  inputs.files(layout.projectDirectory.dir("../src/main/cs/Saxon/Api"))
  inputs.files(csharpFeatureKeysSources)
  inputs.file(layout.projectDirectory.file("../tools/parsecs.xsl"))
  outputs.file(layout.buildDirectory.file("dotnetdoc/names.xml"))

  classpath = preprocessorClasspath
  mainClass = "com.saxonica.Transform"
  args("-it:main",
       "-xsl:${layout.projectDirectory.file("../tools/parsecs.xsl")}",
       "-o:${layout.buildDirectory.file("dotnetdoc/names.xml").get().asFile}",
       "inputdir=file://${layout.projectDirectory.file("../src/main/cs/Saxon/Api")}",
       "featurekeysdir=file://${layout.projectDirectory.file("../../featurekeys/build/cs/Saxon/Api")}")
}

tasks.register<JavaExec>("docMerge") {
  group = "Internal"
  description = "Merge API docs for dotnet"

  val arch = architectures.first()
  val buildtask = tasks.named("dotnetBuild_${arch}")

  dependsOn(buildtask)
  inputs.files(apiDocumentation)
  inputs.files(docParse)
  inputs.file(layout.projectDirectory.file("../tools/merge.xsl"))
  outputs.file(layout.buildDirectory.file("dotnetdoc/merged.xml"))

  classpath = preprocessorClasspath
  mainClass = "com.saxonica.Transform"
  args("-it:main",
       "-xsl:${layout.projectDirectory.file("../tools/merge.xsl")}",
       "-o:${layout.buildDirectory.file("dotnetdoc/merged.xml").get().asFile}",
       "names-file=${docParse.get().outputs.getFiles().getSingleFile()}",
       "api-doc-file=${layout.buildDirectory.file("apidoc/SaxonCS.xml").get().asFile}")
}

tasks.register<Copy>("docCss") {
  group = "Internal"
  description = "Copies the CSS for dotnet docs"

  into(layout.buildDirectory.dir("dotnetdoc-html"))
  from(layout.projectDirectory.dir("../tools")) {
    include("saxon-dotnetdoc.css")
  }
}

val dotnetDocHtml = tasks.register<JavaExec>("docHtml") {
  group = LifecycleTasks.DOCUMENTATION_GROUP
  description = "Create HTML output for dotnet docs"
  dependsOn("docMerge")
  dependsOn("docCss")

  inputs.file(layout.buildDirectory.file("dotnetdoc/merged.xml"))
  inputs.file(layout.projectDirectory.file("../tools/doctohtml.xsl"))
  outputs.dir(layout.buildDirectory.file("dotnetdoc-html"))

  classpath = preprocessorClasspath
  mainClass = "com.saxonica.Transform"
  args("-s:${layout.buildDirectory.file("dotnetdoc/merged.xml").get().asFile}",
       "-xsl:${layout.projectDirectory.file("../tools/doctohtml.xsl")}",
       "-o:${layout.buildDirectory.file("dotnetdoc-html/index.html").get().asFile}")
}

val dotnetDocXml = tasks.register<JavaExec>("docXml") {
  group = LifecycleTasks.DOCUMENTATION_GROUP
  description = "Create XML version of dotnet docs"
  dependsOn("docMerge")

  inputs.file(layout.buildDirectory.file("dotnetdoc/merged.xml"))
  inputs.file(layout.projectDirectory.file("../tools/convert-to-jel-package.xsl"))
  outputs.dir(layout.buildDirectory.file("dotnetdoc-xml"))
  classpath = preprocessorClasspath
  mainClass = "com.saxonica.Transform"
  args("-s:${layout.buildDirectory.file("dotnetdoc/merged.xml").get().asFile}",
       "-xsl:${layout.projectDirectory.file("../tools/convert-to-jel-package.xsl")}",
       "-o:${layout.buildDirectory.file("dotnetdoc-xml/packages/Saxon.Api.xml").get().asFile}")
}

htmlDocArtifacts.outgoing.artifact(dotnetDocHtml)
xmlDocArtifacts.outgoing.artifact(dotnetDocXml)

tasks.named(LifecycleTasks.DOCUMENTATION_TASK_NAME) {
  dependsOn(dotnetDocHtml)
  dependsOn(dotnetDocXml)
}

// ============================================================

tasks.register<Exec>("eecsTest") {
  group = "Verification"
  description = "Run SaxonCS nUnit tests"
  dependsOn("dotnetBuild")

  val reportDir = layout.buildDirectory.dir("reports/tests/eecsTest")
  inputs.dir(layout.projectDirectory.dir("../src/test/nunit"))
  outputs.dir(reportDir)

  workingDir = layout.projectDirectory.dir("../src/test/nunit").asFile
  commandLine("${saxonBuild.dotnet.get()}", "test", "--nologo", "--", "NUnit.TestOutputXml=${reportDir.get()}")
}

// ============================================================

samples {
  // Irrelevant in C#, but must be initialized. :-(
  runclasspath = sampleSources
  debugclasspath = sampleSources

  sources = sampleSources
  product = "csharp"
  edition = "ee"
}

samples.csSample("Shakespeare_1", "othello.xml", "play.xsl",
                 listOf("dir=file:${layout.buildDirectory.file("sample_results/playhtml").get()}"),
                 layout.buildDirectory.file("sample_results/playhtml/play.html").get().asFile)


// ============================================================

tasks.register<DotNetBuild>("dotnetBuildTestDrivers") {
  group = LifecycleTasks.ACCEPTANCE_TEST_GROUP
  dependsOn("dotnetBuild")
  target = "TestDrivers"
  arch = architectures.first()
  inputs.files(copyTestDrivers)
}

testSuites {
  product = "csharp"
  edition = "ee"
  submodulesDir = layout.projectDirectory.dir("../../submodules")
  exceptionsDir = layout.projectDirectory.dir("../src/test/exceptions")
}

testSuites.configure("expath", "XP31")
testSuites.configure("expath", "XQ31")
testSuites.configure("expath", "XQ40")
testSuites.configure("qt3", "XP31")
testSuites.configure("qt3", "XP31", mapOf("linq" to "on"))
testSuites.configure("qt3", "XP31", mapOf("unfolded" to "on"))
testSuites.configure("qt3", "XQ31")
testSuites.configure("qt3", "XQ31", mapOf("unfolded" to "on"))
testSuites.configure("qt3extra", "XP31")
testSuites.configure("qt3extra", "XQ31")
testSuites.configure("qt3extra", "XQ40")
testSuites.configure("qt4", "XP40")
testSuites.configure("qt4", "XQ31")
testSuites.configure("qt4", "XQ31", mapOf("unfolded" to "on"))
testSuites.configure("qt4", "XQ40")
testSuites.configure("xsd", "XSD11")
testSuites.configure("xslt30", "XT30")
testSuites.configure("xslt30", "XT30", mapOf("export" to "on"))
testSuites.configure("xslt30", "XT30", mapOf("strict" to "on"))
testSuites.configure("xslt30extra", "XT30")
testSuites.configure("xslt30extra", "XT30", mapOf("export" to "on"))
testSuites.configure("xslt30extra", "XT30", mapOf("strict" to "on"))
testSuites.configure("xslt30extra", "XT40")
testSuites.configure("xslt30extra", "XT40", mapOf("export" to "on"))
testSuites.configure("xslt30extra", "XT40", mapOf("strict" to "on"))
testSuites.configure("xslt40", "XT40")
testSuites.configure("xslt40", "XT40", mapOf("strict" to "on"))
