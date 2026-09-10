using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Saxon.Api;
using NUnit.Framework;
using System.Xml;
using Saxon.Eej.config;
using Saxon.Hej;
using Saxon.Hej.functions;
using Saxon.Hej.om;
using Saxon.Hej.type;
using Saxon.Hej.value;
using DocumentBuilder = Saxon.Api.DocumentBuilder;
using Error = Saxon.Api.Error;
using Location = Saxon.Api.Location;
using Processor = Saxon.Api.Processor;
using RawDestination = Saxon.Api.RawDestination;
using SchemaManager = Saxon.Api.SchemaManager;
using SchemaValidator = Saxon.Api.SchemaValidator;
using Serializer = Saxon.Api.Serializer;
using XdmDestination = Saxon.Api.XdmDestination;
using XdmNodeKind = Saxon.Api.XdmNodeKind;
using Xslt30Transformer = Saxon.Api.Xslt30Transformer;
using XsltCompiler = Saxon.Api.XsltCompiler;
using XsltExecutable = Saxon.Api.XsltExecutable;
using XsltPackage = Saxon.Api.XsltPackage;
using XsltTransformer = Saxon.Api.XsltTransformer;


namespace SaxonCS_NUnit {
    /**
     * JUnit tests for the s9api XsltTransformer
     */
    public class TestXsltTransformer {
        /**
         * Simple test
         */
        [Test]
        public void testSimple() {
            // Create a Processor instance.
            try {
                Processor processor = new(true);
                processor.SetProperty("http://saxon.sf.net/feature/timing", "true");

                // Create a transformer for the stylesheet.
                XsltTransformer transformer = processor.NewXsltCompiler().Compile(
                    new Uri(ConfigTest.DATA_DIR + "caffo/Transform2.xslt")).Load();

                // Create a serializer
                XdmDestination serializer = new();

                // Transform the source XML to System.out.
                XdmNode input = processor.NewDocumentBuilder().Build(
                    new Uri(ConfigTest.DATA_DIR + "caffo/Input.xml"));
                // Set the root node of the source document to be the initial context node
                transformer.InitialContextNode = input;
                transformer.Run(serializer);
            }
            catch (SaxonApiException e) {
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }
        }
        

        [Test]
        public void testUseDestinationProperty() {
            // Create a Processor instance.
            try {
                Processor processor = new(true);
                processor.SetProperty("http://saxon.sf.net/feature/timing", "true");

                // Create a transformer for the stylesheet.
                XsltTransformer transformer = processor.NewXsltCompiler().Compile(
                    new Uri(ConfigTest.DATA_DIR + "caffo/Transform2.xslt")).Load();

                // Create a serializer
                XdmDestination serializer = new();
                transformer.Destination = serializer;

                // Transform the source XML to System.out.
                XdmNode input = processor.NewDocumentBuilder().Build(
                    new Uri(ConfigTest.DATA_DIR + "caffo/Input.xml"));
                // Set the root node of the source document to be the initial context node
                transformer.InitialContextNode = input;
                transformer.Run();
            }
            catch (SaxonApiException e) {
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void testXmlDeclaration() {
            try {
                Processor proc = new(true);
                StringReader sr = new("<input/>");
                DocumentBuilder builder = proc.NewDocumentBuilder();
                XdmNode node = builder.Build(sr);
                XsltCompiler xqcomp = proc.NewXsltCompiler();
                sr = new StringReader(
                    "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                    "  <xsl:output omit-xml-declaration=\"yes\"/>\n" +
                    "  <xsl:template match=\"/\"><output1/><output2/></xsl:template>\n" +
                    "</xsl:stylesheet>");
                XsltTransformer trans = xqcomp.Compile(sr).Load();

                MemoryStream os = new();
                Serializer serializer = proc.NewSerializer(os);


                trans.InitialContextNode = node;
                trans.Run(serializer);

                string result = stringFromMemoryStream(os);
                Assert.True(result.Equals("<output1/><output2/>"), "output");
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }


        /**
         * Test for the case where the context item is not the root
         */
        [Test]
        public void testContextNotRoot() {
            try {
                Processor proc = new(true);
                StringReader sr = new("<doc><e>text</e></doc>");
                DocumentBuilder builder = proc.NewDocumentBuilder();
                XdmNode node = builder.Build(sr);
                XsltCompiler xqcomp = proc.NewXsltCompiler();
                sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:variable name='x' select='.'/>" +
                    "<xsl:template match='/'>errorA</xsl:template>" +
                    "<xsl:template match='e'>[<xsl:value-of select='name($x)'/>]</xsl:template>" +
                    "</xsl:stylesheet>");
                XsltTransformer trans = xqcomp.Compile(sr).Load();

                MemoryStream os = new();
                Serializer serializer = proc.NewSerializer(os);

                IEnumerator iter = node.Select(Steps.Descendant("e")).GetEnumerator();
                if (iter.MoveNext()) {
                    XdmNode e = (XdmNode)iter.Current;
                    trans.InitialContextNode = e;
                    trans.Run(serializer);
                    QName q = trans.InitialContextNode.NodeName;
                    Assert.AreEqual(q.LocalName, "e");
                }
                else {
                    Assert.Fail("No input node found");
                }

                string result = stringFromMemoryStream(os);
                Assert.False(result.Contains("errorA"), "matched root");
                Assert.True(result.Contains("[]"), "matched e");
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        private string stringFromMemoryStream(MemoryStream ms) {
            ms.Position = 0;
            StreamReader sr = new StreamReader(ms, Encoding.UTF8);
            return sr.ReadToEnd();
        }


        [Test]
        public void testSaxonEscapeNCNameFunction() {
            Processor processor = new(true);
            XsltCompiler compiler = processor.NewXsltCompiler();
            compiler.ErrorReporter = error => Console.Error.WriteLine(error.Message);
            try {
                StringReader source = new("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n" +
                                          "\n" +
                                          "<xsl:stylesheet version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                                          "\txmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:fn=\"http://www.w3.org/2005/xpath-functions\"\n" +
                                          "\txmlns:saxon=\"http://saxon.sf.net/\">\n" +
                                          "\t<xsl:template name='main'>\n" +
                                          "\t\t<o><xsl:value-of select=\"saxon:escape-NCName('H%B')\"/></o>\n" +
                                          "\t</xsl:template>\n" +
                                          "</xsl:stylesheet>");

                XsltTransformer transformer = compiler.Compile(source).Load();
                transformer.InitialTemplate = new QName("main");
                XdmDestination dest = new();
                transformer.Run(dest);
                XdmNode result = dest.XdmNode;
                Assert.True(result.ToString().Contains("H_25_B"),
                    "Result should H_25_B:" + result.ToString());
            }
            catch (SaxonApiException ex) {
                Assert.Fail(ex.Message);
            }
        }

        /**
         * Test location information for run-time errors when stylesheet built as an XdmNode
         */
        [Test]
        public void testErrorLocationFromXdm() {
            try {
                // Norm Walsh reported getting line numbers > 2^20. Can't reproduce.
                Processor proc = new(true);
                String sr = "<doc><e>text</e></doc>";
                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.LineNumbering = true;
                XdmNode node = builder.Build(new StringReader(sr));
                XsltCompiler xqcomp = proc.NewXsltCompiler();

                sr = (
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                    "<xsl:import href='file:///" + ConfigTest.DATA_DIR + "identity.xsl'/>\n" +
                    "<xsl:import href='http://imported'/>\n" +
                    "<xsl:param name='x' select='0'/>\n" +
                    "<xsl:template match='/'>\n" +
                    "<xsl:apply-templates select='*'>\n" +
                    "<xsl:with-param name='p' select='()'/>\n" +
                    "</xsl:apply-templates>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>");
                builder.BaseUri = new Uri("http://importer/");
                XdmNode style2 = builder.Build(new StringReader(sr));

                sr = 
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                    "<xsl:template match='e' as='node()'>\n" +
                    "<xsl:param name='x' select='0'/>\n" +
                    "<xsl:param name='p' as='node()*' select='()'/>\n" +
                    //"<e1/>" +
                    //"<e2/>" +
                    "<xsl:sequence select='$p'/>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>";
                XdmNode style1 = builder.Build(new StringReader(sr));

                xqcomp.StylesheetModuleResolver = (request => {
                    if (request.Uri.Scheme == ("http")) {
                        return new TextResource(sr, request.Uri);
                    }
                    else {
                        return null;
                    }
                });

                XsltTransformer trans = xqcomp.Compile(style2).Load();

                Serializer serializer = proc.NewSerializer();
                MemoryStream os = new();
                serializer.OutputStream = os;

                trans.InitialContextNode = node;
                trans.Run(serializer);

                Assert.Fail("Transformation should fail");
            }
            catch (SaxonApiException e) {
                int line = e.LineNumber;
                Assert.True(line >= 2 && line <= 6, "line=5");
            }
        }
        
        [Test]
        public void TestXmlResolver() {
            try {
                Processor proc =  new();
                string source = "<doc><e>text</e></doc>";

                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.LineNumbering = true;
                builder.BaseUri = new Uri("http://importer/");
                XdmNode node = builder.Build(new StringReader(source));
                XsltCompiler xqcomp = proc.NewXsltCompiler();
                StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                    "<xsl:template match='e' as='node()'>\n" +
                    "<xsl:param name='x' select='0'/>\n" +
                    "<xsl:param name='p' as='node()*' select='()'/>\n" +
                    "<e1/>" +
                    "<xsl:sequence select='$p'/>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>");

                xqcomp.StylesheetModuleResolver = (request) => new TextResource(sr, request.Uri);
                XsltExecutable exec = xqcomp.Compile(new Uri("http://random-stylesheet.org/"));
                XsltTransformer trans = exec.Load();
                trans.Destination = new RawDestination();
                trans.InitialContextNode = node;
                trans.Run();
                Assert.True(((RawDestination)trans.Destination).XdmValue[0] is XdmNode);
                Assert.True(((XdmNode)((RawDestination)trans.Destination).XdmValue[0]).NodeName.LocalName.Equals("e1"));
            }
            catch (Exception exc) {
                Assert.Fail(exc.Message);
            }
        }


        /**
         * Test created ErrorListener picks up errors in imported stylesheet
         */
        [Test]
        public void testErrorInImportedStylesheet() {
            ArrayList warnings = new ArrayList(4);
            ArrayList errors = new ArrayList(4);
 
            try {
                Processor proc = new(true);


                XsltCompiler xqcomp = proc.NewXsltCompiler();
                xqcomp.ErrorReporter = error => {
                    if (error.IsWarning) {
                        warnings.Add(error);
                    }
                    else {
                        errors.Add(error);
                    }
                };

                XsltTransformer trans = xqcomp.Compile(new Uri(ConfigTest.DATA_DIR + "main.xsl"))
                    .Load();

                Serializer serializer = proc.NewSerializer();
                MemoryStream os = new();
                serializer.OutputStream = (os);

                trans.InitialTemplate = new QName("main");
                trans.Run(serializer);

                Assert.Fail("Transformation should fail");
            }
            catch (SaxonApiException e) {
                Assert.True(warnings.Count == 0, "warnings");
                Assert.False(errors.Count == 0, "errors");
            }
        }


        /**
         * Test for the case where the context item is not the root, when starting at a named template
         */
        [Test]
        public void testContextNotRootNamedTemplate() {
            try {
                Processor proc = new(true);
                StringReader sr = new("<doc><e>text</e></doc>");
                DocumentBuilder builder = proc.NewDocumentBuilder();
                XdmNode node = builder.Build(sr);
                XsltCompiler xqcomp = proc.NewXsltCompiler();
                sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:variable name='x' select='.'/>" +
                    "<xsl:template match='/'>errorA</xsl:template>" +
                    "<xsl:template name='main'>[<xsl:value-of select='name($x)'/>]</xsl:template>" +
                    "</xsl:stylesheet>");
                XsltTransformer trans = xqcomp.Compile(sr).Load();

                Serializer serializer = proc.NewSerializer();
                MemoryStream os = new();
                serializer.OutputStream = os;

                foreach (XdmNode e in node.Select(Steps.Descendant("e"))) {
                    trans.InitialContextNode = e;
                    trans.InitialTemplate = new QName("main");
                    trans.Destination = serializer;
                    trans.Run(serializer);
                }

                string result = stringFromMemoryStream(os);
                Assert.False(result.Contains("errorA"), "matched root");
                Assert.True(result.Contains("[]"), "matched e");
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        /**
         * Build a document from a non-root element of a DOMSource
         */
        [Test]
        public void testNonRootDomSource() {
            try {
                Processor proc = new(true);
                XsltCompiler comp = proc.NewXsltCompiler();
                StringReader sr =
                    new("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                        "<xsl:template match='/'>///<xsl:apply-templates/></xsl:template>" +
                        "<xsl:template match='*'>***<xsl:apply-templates/></xsl:template>" +
                        "</xsl:stylesheet>");
                XsltExecutable exec = comp.Compile(sr);
                XsltTransformer xsl = exec.Load();
                string inn = "<?xml version='1.0'?> \n" +
                             "<doc><a><b/></a></doc>";

                // Read an XML file to a DOM
                XmlDocument doc = new();
                doc.Load(new StringReader(inn));
                XdmNode xdmNode = proc.NewDocumentBuilder().Wrap(doc);
                xsl.InitialContextNode = xdmNode.Select(Steps.Child("*")).First();
                StringWriter writer = new();
                Serializer outt = proc.NewSerializer(writer);
                xsl.Destination = outt;
                xsl.Run(outt);
                Assert.False(writer.ToString().Contains("///"), "started at element node");
            }
            catch (Exception e) {
                Console.Error.WriteLine(e.StackTrace);
                Assert.Fail(e.Message);
            }
        }

        /**
     * Send output to an XdmDestination
     */
        [Test]
        public void testXdmDestination() {
            try {
                Processor proc = new(true);
                XsltCompiler comp = proc.NewXsltCompiler();
                StringReader sr =
                    new("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                        "<xsl:template name='go'><a/></xsl:template>" +
                        "</xsl:stylesheet>");
                XsltExecutable exec = comp.Compile(sr);
                XsltTransformer xsl = exec.Load();
                xsl.InitialTemplate = new QName("go");
                XdmDestination outt = new();
                xsl.Destination = outt;
                xsl.Run(outt);
                XdmNode root = outt.XdmNode;
                Assert.True(root.NodeKind == XdmNodeKind.Document, "result is document node");
            }
            catch (Exception e) {
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void testExtensionWithSchemaAccecss() {
            
            try {
                Processor proc = new(true);
                proc.SetProperty("http://saxon.sf.net/feature/timing", "true");
                proc.RegisterExtensionFunction(
                    new QName("my", "java:s9apitest.TestXsltTransformer", "isOptional"),
                    proc.ParseItemType("function(node(), xs:QName) as xs:boolean"),
                    args => {
                        XdmNode parent = (XdmNode)args[0];
                        XdmAtomicValue name = (XdmAtomicValue)args[1];
                        QName childName = name.AsQName();
                        Configuration config = proc.Implementation;
                        SchemaType parentType = ((NodeInfo)parent.Unwrap()).getSchemaType();
                        int fp = config.getNamePool().allocateFingerprint(
                            NamespaceUri.of(childName.Uri), childName.LocalName);
                        int cardinality = ((ComplexType)parentType).getElementParticleCardinality(fp, true);
                        return new XdmAtomicValue(Cardinality.allowsZero(cardinality));
                    });
                XsltCompiler comp = proc.NewXsltCompiler();
                
                comp.SchemaAware = true;
                StringReader sr =
                    new(
                        "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:my='java:s9apitest.TestXsltTransformer' exclude-result-prefixes='my'>" +
                        "<xsl:template match='*'><xsl:copy><xsl:copy-of select='@*'/><xsl:apply-templates/></xsl:copy></xsl:template>\n" +
                        "<xsl:template match='*/*/*/*[my:isOptional(.., node-name(.))]'/> \n" +
                        "</xsl:stylesheet>");
                XsltExecutable exec = comp.Compile(sr);
                XsltTransformer transformer = exec.Load();

                SchemaManager sm = proc.SchemaManager;
                sm.Compile(new Uri(ConfigTest.DATA_DIR + "books.xsd"));
                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.SchemaValidator = sm.NewSchemaValidator();
                XdmNode books = builder.Build(new Uri(ConfigTest.DATA_DIR + "books.xml"));
                transformer.InitialContextNode = books;
                // Create a serializer
                //String outfile = @"C:/MyJava/users/caffo/Output.xml";
                StringWriter sw = new();
                Serializer serializer = proc.NewSerializer(sw);
                serializer.SetOutputProperty(Serializer.INDENT, "yes");
                transformer.Run(serializer);
                Assert.True(sw.ToString().Contains("TITLE"));
                Assert.False(sw.ToString().Contains("AUTHOR"));
            }
            catch (SaxonApiException e) {
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
                Assert.Fail(e.Message);
            }
        }

        /**
     * Sample extension function - explores schema information
     *
     * @return true if the element defined by childName is optional in the context of the governing
     *         type of parent.
     */
        public static bool isOptional(NodeInfo parent, QNameValue childName) {
            Configuration config = parent.getConfiguration();
            SchemaType parentType = parent.getSchemaType();
            int fp = config.getNamePool().allocateFingerprint(
                NamespaceUri.of(childName.getComponent(AccessorFn.Component.NAMESPACE).getStringValue()),
                childName.getComponent(AccessorFn.Component.LOCALNAME).getStringValue());
            int cardinality = ((ComplexType)parentType).getElementParticleCardinality(fp, true);
            return Cardinality.allowsZero(cardinality);
        }

        /**
     * s9api transformation pipeline
     */
        [Test]
        public void testShortPipeline() {
            try {
                Processor proc = new(true);
                XsltCompiler comp = proc.NewXsltCompiler();

                StringWriter sw = new();
                Serializer outt = proc.NewSerializer(sw);
                string xsl = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                             "<xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template>" +
                             "</xsl:stylesheet>";
                string xml = "<z/>";
                XdmNode inn = proc.NewDocumentBuilder().Build(new StringReader(xml));
                XsltTransformer stage1 = comp.Compile(new StringReader(xsl)).Load();
                XsltTransformer stage2 = comp.Compile(new StringReader(xsl)).Load();
                stage2.Destination = outt;
                stage1.InitialContextNode = inn;
                stage1.Run(stage2);
                Assert.True(sw.ToString().Contains("<a><a><z/></a></a>"), "output");
            }
            catch (SaxonApiException err) {
                Assert.Fail(err.Message);
            }
        }

        [Test]
        public void testPipeline() {
            try {
                Processor proc = new(true);
                XsltCompiler comp = proc.NewXsltCompiler();

                StringWriter sw = new();
                Serializer outt = proc.NewSerializer(sw);
                string xsl = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                             "<xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template>" +
                             "</xsl:stylesheet>";
                string xml = "<z/>";
                XdmNode inn = proc.NewDocumentBuilder().Build(new StringReader(xml));
                XsltTransformer stage1 = comp.Compile(new StringReader(xsl)).Load();
                XsltTransformer stage2 = comp.Compile(new StringReader(xsl)).Load();
                XsltTransformer stage3 = comp.Compile(new StringReader(xsl)).Load();
                XsltTransformer stage4 = comp.Compile(new StringReader(xsl)).Load();
                XsltTransformer stage5 = comp.Compile(new StringReader(xsl)).Load();
                stage1.Destination = (stage2);
                stage2.Destination = (stage3);
                stage3.Destination = (stage4);
                stage4.Destination = (stage5);
                stage5.Destination = (outt);
                stage1.InitialContextNode = (inn);
                stage1.Run(stage2);
                Assert.True(sw.ToString().Contains("<a><a><a><a><a><z/></a></a></a></a></a>"), "output");
            }
            catch (SaxonApiException err) {
                Assert.Fail(err.Message);
            }
        }

  

        [Test]
        public void testSchemaAware11() {
            // Create a Processor instance.
            try {
                Processor proc = new(true);
                proc.SetProperty(FeatureKeys.TIMING, "true");
                proc.SetProperty(FeatureKeys.XSD_VERSION, "1.1");
                XsltCompiler comp = proc.NewXsltCompiler();
                comp.SchemaAware = true;
                StringReader sr = new(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:import-schema><xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                    "<xs:element name='e'><xs:complexType><xs:sequence><xs:element name='p'/></xs:sequence><xs:assert test='exists(p)'/></xs:complexType></xs:element>" +
                    "</xs:schema></xsl:import-schema>" +
                    "<xsl:variable name='v'><e><p/></e></xsl:variable>" +
                    "<xsl:template name='main'><xsl:copy-of select='$v' validation='strict'/></xsl:template>" +
                    "</xsl:stylesheet>");
                XsltExecutable exec = comp.Compile(sr);
                XsltTransformer transformer = exec.Load();
                transformer.InitialTemplate = new QName("main");

                StringWriter sw = new();
                Serializer serializer = proc.NewSerializer(sw);
                serializer.SetOutputProperty(Serializer.INDENT, "no");
                transformer.Run(serializer);
                Assert.True(sw.ToString().Contains("<e>"));
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void testValidationWarnings() {
            try {
                Processor processor = new(true);
                processor.SetProperty(FeatureKeys.VALIDATION_WARNINGS, "true");
                processor.SchemaManager.Compile(new Uri(ConfigTest.DATA_DIR + "books.xsd"));

                Serializer serializer = processor.NewSerializer();
                serializer.OutputFile = ConfigTest.SANDPIT_DIR + "validationWarnings.xml";

                XsltCompiler compiler = processor.NewXsltCompiler();
                compiler.SchemaAware = true;

                string xsl = "<xsl:stylesheet version=\"2.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                             "\n" +
                             "    <xsl:output indent=\"yes\" encoding=\"UTF-8\" />\n" +
                             "\n" +
                             "    <xsl:template match=\"/\">\n" +
                             "        <xsl:document validation='strict'>\n" +
                             "            <xsl:next-match />\n" +
                             "        </xsl:document>\n" +
                             "    </xsl:template>\n" +
                             "\n" +
                             "    <xsl:template match=\"*\">\n" +
                             "        <xsl:element name=\"{name()}\" namespace=\"{namespace-uri()}\">\n" +
                             "            <invalid-element/>\n" +
                             "        </xsl:element>\n" +
                             "    </xsl:template>\n" +
                             "</xsl:stylesheet> ";
                XsltTransformer transformer = compiler.Compile(new StringReader(xsl)).Load();

                transformer.SetInputTextReader(new StringReader(xsl), new Uri("http://dummy/"));
                transformer.Run(serializer);
            }
            catch (SaxonApiException e) {
                Assert.Fail();
            }
            catch (SchemaException se) {
                Assert.Fail();
            }
        }

        /**
     * Test that the base URI of the result document is the base output URI
     */
        [Test]
        public void testOutputBaseUri() {
            Processor processor = new(true);

            DocumentBuilder builder = processor.NewDocumentBuilder();
            builder.DtdValidation = false;
            builder.LineNumbering = true;

            XdmNode document = builder.Build(new Uri(ConfigTest.DATA_DIR + "books.xml"));
            XdmNode stylesheet = builder.Build(new Uri(ConfigTest.DATA_DIR + "books.xsl"));

            XsltCompiler compiler = processor.NewXsltCompiler();
            //compiler.SchemaAware = (processor.IsSchemaAware);
            XsltExecutable exec = compiler.Compile(stylesheet);
            XsltTransformer transformer = exec.Load();

            if (document != null) transformer.InitialContextNode = document;
            XdmDestination result = new();

            transformer.BaseOutputUri = new Uri("file://tmp/out.xml");

            transformer.SchemaValidationMode = SchemaValidationMode.Unspecified;
            transformer.Run(result);

            XdmNode xformed = result.XdmNode;
            Assert.AreEqual("file://tmp/out.xml", xformed.BaseUri.ToString(), "output base uri");
        }

        /**
     * test a call on XsltExecutable.getGlobalParameters
     * @throws SaxonApiException
     */
        [Test]
        public void testGlobalParameters() {
            Configuration configuration = new EnterpriseConfiguration();
            Processor processor = new(configuration);
            XsltCompiler xsltCompiler = processor.NewXsltCompiler();

            string source = "<?xml version='1.0'?>" +
                            "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                            "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                            "                version='2.0'>" +
                            "                <xsl:param name='surname' as='xs:string' />" +
                            "                <xsl:param name='age' as='xs:integer' />" +
                            "                <xsl:template match='/'><xsl:element name='root' /></xsl:template>" +
                            "                </xsl:stylesheet>";

            XsltExecutable xsltExecutable =
                xsltCompiler.Compile(new StringReader(source));
            // Map<QName, XsltExecutable.ParameterDetails> params = xsltExecutable.getGlobalParameters();
            //
            // System.out.println();
            // int i = 0;
            // for (QName key : params.
            // keySet()) {
            //     System.out.println("Parameter n." + ++i + ": " + key);
            // }
            // System.out.println();
            // Assert.AreEqual(2, 2);
        }

 
        /**
         * Test result document handler
         */
        [Test]
        public void testResultDocHandler() {
            Processor processor = new(true);

            string xsl = "<?xml version='1.0'?>" +
                         "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                         "                version='3.0'>" +
                         "                <xsl:template name='main'>" +
                         "                   <xsl:for-each select='5 to 10'> " +
                         "                      <xsl:result-document href='out{position()}.xml'>" +
                         "                          <out nr='{.}'/>" +
                         "                      </xsl:result-document>" +
                         "                   </xsl:for-each>" +
                         "                </xsl:template>" +
                         "                </xsl:stylesheet>";


            XsltCompiler compiler = processor.NewXsltCompiler();
            compiler.SchemaAware = false;
            XsltExecutable exec = compiler.Compile(new StringReader(xsl));
            XsltTransformer transformer = exec.Load();
            transformer.BaseOutputUri = new Uri("http://base.output.com/");
            Dictionary<string, XdmDestination> results = new ();
            transformer.ResultDocumentHandler = (href, bace) => {
                XdmDestination dest = new();
                Assert.True(bace.ToString().StartsWith("http://base.output.com/"));
                Uri abs = new Uri(bace, href);
                dest.BaseUri = abs;
                string key = abs.ToString().Replace("http://base.output.com/", "");
                results.Add(key, dest);
                return dest;
            };
            XdmDestination result = new();
            transformer.InitialTemplate = new QName("main");
            transformer.Run(result);

            Assert.AreEqual(6, results.Count());
            Assert.NotNull(results[("out1.xml")]);
            //System.err.println(results.get("out1.xml").getXdmNode().ToString());
            Assert.True(results[("out1.xml")].XdmNode.ToString().Contains("<out nr=\"5\"/>"));
            Assert.AreEqual("http://base.output.com/out1.xml",
                results["out1.xml"].XdmNode.BaseUri.ToString(), "base URI incorrect");
        }


        /**
         * Test calling a stylesheet named template getting raw results back as a Destination
         */
        [Test]
        public void testCallNamedTemplateRawDestination() {
            Configuration configuration = new EnterpriseConfiguration();
            Processor processor = new(configuration);
            XsltCompiler xsltCompiler = processor.NewXsltCompiler();

            string source = "<?xml version='1.0'?>" +
                            "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                            "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                            "                version='3.0'>" +
                            "                <xsl:template name='t'>" +
                            "                   <xsl:param name='a' as='xs:double' select='3'/>" +
                            "                   <xsl:param name='b' as='xs:float' select='4'/>" +
                            "                   <xsl:sequence select='$a+10, $b+2'/>" +
                            "                </xsl:template>" +
                            "                </xsl:stylesheet>";

            XsltExecutable xsltExecutable = xsltCompiler.Compile(new StringReader(source));
            XsltTransformer t = xsltExecutable.Load();
            t.InitialTemplate = new QName("t");
            RawDestination outt = new();
            t.Run(outt);
            XdmValue val = outt.XdmValue;
            Assert.AreEqual(2, val.Count());
            Assert.AreEqual(13, ((XdmAtomicValue)val.ItemAt(0)).AsLong());
            Assert.AreEqual(6, ((XdmAtomicValue)val.ItemAt(1)).AsLong());
        }


        [Test]
        public void testInitialTemplate() {
            // Test running xsl:initial-template by providing no source
            try {
                Processor processor = new(true);

                string xsl = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n"
                             + "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n"
                             + "    exclude-result-prefixes=\"xs\"\n"
                             + "    version=\"3.0\" expand-text='yes'>\n"
                             + "    <xsl:param name=\"x\" as=\"xs:integer\"/>\n"
                             + "    <xsl:template name=\"xsl:initial-template\" as=\"xs:integer\">{$x + 2}</xsl:template>\n"
                             + "</xsl:stylesheet>";

                // Create a transformer for the stylesheet.
                XsltTransformer transformer = processor.NewXsltCompiler().Compile(
                    new StringReader(xsl)).Load();

                // Create a serializer
                StringWriter sw = new();
                Serializer outt = processor.NewSerializer(sw);
                outt.SetOutputProperty(Serializer.METHOD, "text");
                transformer.SetParameter(new QName("x"), new XdmAtomicValue(2));
                transformer.Run(outt);
                Assert.AreEqual("4", sw.ToString());
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }
    }
}


