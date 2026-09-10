import com.saxonica.build.*

plugins {
  id("saxoncs")
  id("com.saxonica.build.testsuites")
}

repositories {
  mavenLocal()
  mavenCentral()
  maven { url = uri("https://maven.saxonica.com/maven") }
}

val saxonBuild = the<SaxonBuildExtension>()

saxoncs {
  edition = "he"
}

dependencies {
  compileOnly("net.sf.saxon:Saxon-HE:12.6")
  preprocessor("net.sf.saxon:Saxon-HE:12.6")
}

tasks.filteredSources {
  from(configurations.javaSources) {
    include("net/sf/saxon/**/*.java")
    include("net/sf/saxon/data/**")
    exclude("**/package-info.java")
    exclude("net/sf/saxon/CommandExitException.java")
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
}

tasks.preprocessJava {
  opts {
    on( "HOF", "CSHARP")
    off("STREAM", "EE", "PE", "SAXONC", "SCHEMA", "BYTECODE")
  }
}

testSuites {
  product = "csharp"
  edition = "he"
  submodulesDir = layout.projectDirectory.dir("../../submodules")
  exceptionsDir = layout.projectDirectory.dir("../src/test/exceptions")
}

testSuites.configure("qt3", "XP31")
testSuites.configure("qt3", "XQ31")
testSuites.configure("xslt30", "XT30")
testSuites.configure("xslt30extra", "XT30")
