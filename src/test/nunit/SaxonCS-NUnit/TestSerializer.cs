using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Saxon.Api;
using Saxon.Impl.Overrides;
using NamespaceConstant = Saxon.Api.NamespaceConstant;

namespace SaxonCS_NUnit {
    public class TestSerializer {

        [Test]
        public void testXQuerySerialization() {
            try {
                Processor proc = new Processor();
                StringReader sr = new StringReader("<doc><e>text</e></doc>");
                DocumentBuilder builder = proc.NewDocumentBuilder();
                XdmNode node = builder.Build(sr);
                XQueryCompiler xqcomp = proc.NewXQueryCompiler();
                XQueryExecutable xqexec = xqcomp.Compile("declare option saxon:output 'cdata-section-elements=e';.");
                XQueryEvaluator xqeval = xqexec.Load();
                xqeval.ContextItem = node;

                MemoryStream os = new MemoryStream();
                Serializer serializer = proc.NewSerializer(os);

                serializer.SetOutputProperty(Serializer.BYTE_ORDER_MARK, "no");
                serializer.SetOutputProperty(Serializer.ENCODING, "utf-8");
                serializer.SetOutputProperty(Serializer.INDENT, "no");
                serializer.SetOutputProperty(Serializer.METHOD, "xml");
                serializer.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");

                xqeval.Run(serializer);

                string result = ReadMemoryStream(os);
                Assert.False(result.Contains("<?xml"), "XML declaration exists");
                Assert.That(result.Contains("doc><e"), "Indented");
                Assert.That(result.Contains("<e><![CDATA"), "Cdata section");

            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        private static string ReadMemoryStream(MemoryStream ms) {
            ms.Position = 0;
            var reader = new StreamReader(ms);
            return reader.ReadToEnd();
        }

        [Test]
        public void testXsltSerialization() {
            try {
                Processor proc = new Processor();
                StringReader sr = new StringReader("<doc><e>text</e></doc>");
                DocumentBuilder builder = proc.NewDocumentBuilder();
                XdmNode node = builder.Build(sr);
                XsltCompiler xqcomp = proc.NewXsltCompiler();
                sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:output indent='yes' cdata-section-elements='e'/>" +
                    "<xsl:template match='/'><xsl:copy-of select='.'/></xsl:template>" +
                    "</xsl:stylesheet>");
                XsltTransformer trans = xqcomp.Compile(sr).Load();

                MemoryStream os = new MemoryStream();
                Serializer serializer = proc.NewSerializer(os);

                serializer.SetOutputProperty(Serializer.BYTE_ORDER_MARK, "no");
                serializer.SetOutputProperty(Serializer.ENCODING, "utf-8");
                serializer.SetOutputProperty(Serializer.INDENT, "no");
                serializer.SetOutputProperty(Serializer.METHOD, "xml");
                serializer.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");

                trans.InitialContextNode = node;
                trans.Run(serializer);

                string result = ReadMemoryStream(os);
                Assert.False(result.Contains("<?xml"), "XML declaration exists");
                Assert.That(result.Contains("doc><e"), "Indented");
                Assert.That(result.Contains("<e><![CDATA"), "Cdata section");

            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void testXsltCharacterMaps() {
            try {
                Processor proc = new Processor();
                StringReader sr = new StringReader("<doc><e>text</e></doc>");
                DocumentBuilder builder = proc.NewDocumentBuilder();
                XdmNode node = builder.Build(sr);
                XsltCompiler xqcomp = proc.NewXsltCompiler();
                sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:output method='xml' use-character-maps='m'/>" +
                    "<xsl:character-map name='m'><xsl:output-character character='e' string='f'/></xsl:character-map>" +
                    "<xsl:template match='/'><xsl:copy-of select='.'/></xsl:template>" +
                    "</xsl:stylesheet>");
                XsltTransformer trans = xqcomp.Compile(sr).Load();

                Serializer serializer = proc.NewSerializer();
                MemoryStream os = new MemoryStream();
                serializer.OutputStream = os;

                trans.InitialContextNode = node;
                trans.Run(serializer);

                string result = ReadMemoryStream(os);
                Assert.That(result.Contains("tfxt"), "Charmapped");

            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void testXsltCharacterMapsFromApi() {
            try {
                Processor proc = new Processor();
                StringReader sr = new StringReader("<doc><e>text</e></doc>");
                DocumentBuilder builder = proc.NewDocumentBuilder();
                XdmNode node = builder.Build(sr);
                XsltCompiler xqcomp = proc.NewXsltCompiler();
                sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template match='/'><xsl:copy-of select='.'/></xsl:template>" +
                    "</xsl:stylesheet>");
                XsltTransformer trans = xqcomp.Compile(sr).Load();

                Serializer serializer = proc.NewSerializer();
                IDictionary<int, string> charMap = new Dictionary<int, string>();
                charMap.Add('e', "f");
                serializer.SetCharacterMap(new QName("", "m"), charMap);
                serializer.SetOutputProperty(new QName("use-character-maps"), "{}m");
                serializer.SetOutputProperty(new QName("method"), "xml");
                MemoryStream os = new MemoryStream();
                serializer.OutputStream = os;

                trans.InitialContextNode = node;
                trans.Run(serializer);

                string result = ReadMemoryStream(os);
                Assert.That(result.Contains("tfxt"), "Charmapped");

            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void testSecondaryOutputDestination() {
            try {
                new Saxon.Ejava.io.File(ConfigTest.SANDPIT_DIR + "temp/first.xml").delete();
                new Saxon.Ejava.io.File(ConfigTest.SANDPIT_DIR + "temp/second.xml").delete();
                Processor proc = new Processor();
                proc.SetProperty(Saxon.Api.Feature.TIMING, true);
                StringReader sr = new StringReader("<doc><e>text</e></doc>");
                DocumentBuilder builder = proc.NewDocumentBuilder();
                XdmNode node = builder.Build(sr);
                XsltCompiler xqcomp = proc.NewXsltCompiler();
                sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:output method='xml'/>" +
                    "<xsl:template match='/'>" +
                    "  <xsl:copy-of select='.'/>" +
                    "  <xsl:result-document href='second.xml'><xsl:copy-of select='.'/></xsl:result-document>" +
                    "</xsl:template>" +
                    "</xsl:stylesheet>");
                Xslt30Transformer trans = xqcomp.Compile(sr).Load30();

                Serializer serializer = proc.NewSerializer();
                serializer.OutputFile = ConfigTest.SANDPIT_DIR + "temp/first.xml";
                trans.ApplyTemplates(node, serializer);

                Assert.That(new Saxon.Ejava.io.File(ConfigTest.SANDPIT_DIR + "temp/first.xml").exists(),
                    "first exists");
                Assert.That(new Saxon.Ejava.io.File(ConfigTest.SANDPIT_DIR + "temp/second.xml").exists(),
                    "second exists");

            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public static void testWriteXdmValue() {
            try {
                Processor proc = new Processor();
                StringReader sr = new StringReader("<doc><e>text</e></doc>");
                DocumentBuilder builder = proc.NewDocumentBuilder();
                XdmNode node = builder.Build(sr);
                proc.WriteXdmValue(node, new XdmDestination());
            }
            catch (SaxonApiException err) {
                Assert.Fail(err.Message);
            }
        }

        [Test]
        public void testS9apiImport() {
            // tests use of dynamic serialization properties chosen by xsl:result-document
            try {
                Processor processor = new Processor(true);

                XsltCompiler xsltCompiler = processor.NewXsltCompiler();

                
                XsltExecutable xsltExecutable = xsltCompiler.Compile(new Uri(ConfigTest.DataDirUri, "welch-ser/A.xsl"));
                XsltTransformer xsltTransformer = xsltExecutable.Load();
                xsltTransformer.InitialTemplate = new QName("main");

                StringWriter sw = new StringWriter();
                Serializer serializer = processor.NewSerializer();
                serializer.OutputWriter = (sw);
                xsltTransformer.ResultDocumentHandler = (href, baseUri) => serializer;

                xsltTransformer.Run(serializer);
                Assert.That(sw.ToString().Contains("<foo>bar</foo>")); // proving that method="xml" was used
            }
            catch (SaxonApiException e) {
                Assert.Fail();
                //e.printStackTrace();
            }
        }

        [Test]
        public void testCannotSerializeMap() {
            try {
                var processor = new Processor();

                var xpathCompiler = processor.NewXPathCompiler();

                xpathCompiler.DeclareVariable(new QName("value"));
                xpathCompiler.DeclareVariable(new QName("serialization-params"));

                var xpathExpression = xpathCompiler.Compile("serialize($value, $serialization-params)").Load();

                xpathExpression.SetVariable(new QName("value"), new XdmMap());
                xpathExpression.SetVariable(new QName("serialization-params"), XdmMap.MakeMap(new Dictionary<string, string>() { { "method", "xml" } }));

                var result = xpathExpression.EvaluateSingle();

                Console.WriteLine(result.StringValue);
                Assert.Fail("Unexpected success");
            }
            catch (SaxonApiException e) {
                Assert.AreEqual(e.Message, "Cannot serialize a map using this output method");
            }
        }

        // [Test]
        // public void testAsSaxContentHandler() throws Exception {
        //     try {
        //
        //         Processor proc = new Processor(false);
        //         StringReader sr = new StringReader("<doc><e>text</e></doc>");
        //
        //         SAXParserFactory factory = SAXParserFactory.NewInstance();
        //         factory.setNamespaceAware(true);
        //         factory.setValidating(false);
        //         XMLReader reader = factory.NewSAXParser().getXMLReader();
        //
        //         MemoryStream os = new MemoryStream();
        //         Serializer serializer = proc.NewSerializer(os);
        //         serializer.SetOutputProperty(Serializer.BYTE_ORDER_MARK, "no");
        //         serializer.SetOutputProperty(Serializer.ENCODING, "utf-8");
        //         serializer.SetOutputProperty(Serializer.INDENT, "no");
        //         serializer.SetOutputProperty(Serializer.METHOD, "xml");
        //         serializer.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
        //         serializer.SetOutputProperty(Serializer.CDATA_SECTION_ELEMENTS, "e");
        //
        //         ContentHandler ch = serializer.getContentHandler();
        //         reader.setContentHandler(ch);
        //         reader.parse(new InputSource(sr));
        //
        //         String result = os.ToString();
        //         Assert.False(result.Contains("<?xml"), "XML declaration exists");
        //         Assert.That(result.Contains("doc><e"), "Indented");
        //         Assert.That(result.Contains("<e><![CDATA"), "Cdata section");
        //
        //     } catch (SaxonApiException e) {
        //         Assert.Fail(e.Message);
        //     }
        // }

        // [Test]
        // public void test3631() {
        //     try {
        //         string dir = ConfigTest.DATA_DIR + "nadolu3631/";
        //         string sourceID = dir + "test.xml";
        //         string xslID = dir + "main.xsl";
        //
        //         // Create a transform factory instance.
        //         Processor tfactory = new Processor();
        //
        //         // Create a transformer for the stylesheet.
        //         Xslt30Transformer transformer = tfactory.NewXslt30Transformer(new StreamSource(xslID));
        //
        //         // Obtain the encoding
        //         string transformerEncoding = transformer.getOutputProperty("encoding");
        //
        //         // Transform the source XML.
        //         StringWriter writer = new StringWriter();
        //         transformer.transform(new StreamSource(sourceID), new StreamResult(writer));
        //         string outs = writer.ToString();
        //         Assert.That(outs.Contains("<second_processing>"));
        //     } catch (TransformerException e) {
        //         //e.printStackTrace();
        //         Assert.Fail();
        //     }
        // }

        [Test]
        public void testBase64BinarySerialization() {
            // bug 4794
            String xslt =
                "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:saxon='http://saxon.sf.net/'>" +
                "<xsl:output method='saxon:base64Binary'/>" +
                "<xsl:template name = 'xsl:initial-template'>" +
                "<xsl:text>iVBORw0KGgoAAAANSUhEUgAAAAoAAAAKCAYAAACNMs+9AAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAA"
                + "LEwAACxMBAJqcGAAAAAd0SU1FB9YGARc5KB0XV+IAAAAddEVYdENvbW1lbnQAQ3JlYXRlZCB3aXRoIFRoZSBH"
                + "SU1Q72QlbgAAAF1JREFUGNO9zL0NglAAxPEfdLTs4BZM4DIO4C7OwQg2JoQ9LE1exdlYvBBeZ7jqch9//q1uH"
                + "4TLzw4d6+ErXMMcXuHWxId3KOETnnXXV6MJpcq2MLaI97CER3N0vr4MkhoXe0rZigAAAABJRU5ErkJggg==</xsl:text>" +
                "</xsl:template>" +
                "</xsl:transform>";

            MemoryStream ms = new MemoryStream();
            try {
                Processor processor = new Processor(true);
                XsltCompiler comp = processor.NewXsltCompiler();
                Xslt30Transformer transformer = comp.Compile(new StringReader(xslt)).Load30();
                Serializer ser = processor.NewSerializer();
                ser.OutputStream = ms;
                transformer.CallTemplate(null, ser);
            }
            catch (SaxonApiException e) {
                //System.err.println(e.Message);
                //e.printStackTrace();
                Assert.Fail();
            }

            byte[] x = ms.ToArray();
            Assert.AreEqual(137, x[0]);
            Assert.AreEqual(80, x[1]);
            Assert.AreEqual(78, x[2]);
            Assert.AreEqual(71, x[3]);
            Assert.AreEqual(247, x.Length);
        }


        [Test]
        public void testHexBinarySerialization() {
            // bug 4794
            string xslt =

                "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:saxon='http://saxon.sf.net/'>" +
                "<xsl:output method='saxon:hexBinary'/>" +
                "<xsl:template name = 'xsl:initial-template'>" +
                "<xsl:text>CAFEBABE</xsl:text>" +
                "</xsl:template>" +
                "</xsl:transform>";

            MemoryStream ms = new MemoryStream();
            try {
                Processor processor = new Processor(true);
                XsltCompiler comp = processor.NewXsltCompiler();
                Xslt30Transformer transformer = comp.Compile(new StringReader(xslt)).Load30();
                Serializer ser = processor.NewSerializer();
                ser.OutputStream = ms;
                transformer.CallTemplate(null, ser);
            }
            catch (SaxonApiException e) {
                //System.err.println(e.Message);
                //e.printStackTrace();
                Assert.Fail();
            }

            byte[] x = ms.ToArray();
            Assert.AreEqual(202, x[0]);
            Assert.AreEqual(254, x[1]);
            Assert.AreEqual(186, x[2]);
            Assert.AreEqual(190, x[3]);
            Assert.AreEqual(4, x.Length);
        }

        [Test]
        public void testXQuerySerializationMethod() {
            string xslt =

                "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                "<xsl:output method='saxon:xquery' xmlns:saxon='http://saxon.sf.net/'/>" +
                "<xsl:template name = 'xsl:initial-template'>" +
                "<a>{$a &lt; '&lt;'}</a>" +
                "</xsl:template>" +
                "</xsl:transform>";

            StringWriter ms = new StringWriter();
            try {
                Processor processor = new Processor(true);
                XsltCompiler comp = processor.NewXsltCompiler();
                Xslt30Transformer transformer = comp.Compile(new StringReader(xslt)).Load30();
                Serializer ser = processor.NewSerializer();
                ser.OutputWriter = (ms);
                transformer.CallTemplate(null, ser);
                Assert.AreEqual("<a>{$a < '&lt;'}</a>", ms.ToString());
            }
            catch (SaxonApiException e) {
                //System.err.println(e.Message);
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }

            Assert.AreEqual("<a>{$a < '&lt;'}</a>", ms.ToString());

        }


        [Test]
        public void testIndentation() {
            try {
                string input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                               + "<root>\n"
                               + "    <parent>\n"
                               + "        <foo>\n"
                               + "            <bar>foo</bar>\n"
                               + "        </foo>        \n"
                               + "    </parent>\n"
                               + "</root>";
                Processor proc = new Processor();
                DocumentBuilder builder = proc.NewDocumentBuilder();
                //builder.setWhitespaceStrippingPolicy(WhitespaceStrippingPolicy.ALL);
                XdmNode doc = builder.Build(new StringReader(input));
                StringWriter sw = new StringWriter();
                Serializer serializer = proc.NewSerializer(sw);
                serializer.SetOutputProperty(Serializer.METHOD, "xml");
                serializer.SetOutputProperty(Serializer.INDENT, "yes");
                XdmNode foo = doc.Select(Steps.Descendant("foo")).First();
                serializer.SerializeXdmNode(foo);
                Console.Error.WriteLine(sw);
                Assert.That(sw.ToString().Contains("?>\n<foo>\n   <bar>"));
            }
            catch (SaxonApiException err) {
                Assert.Fail(err.Message);
            }
        }

        [Test]
        public void testElementToString() {
            try {
                string input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                               + "<root><parent><foo>\n"
                               + "            <bar>foo</bar>\n"
                               + "</foo></parent></root>";
                Processor proc = new Processor();
                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.WhitespacePolicy = WhitespacePolicy.StripAll;
                XdmNode doc = builder.Build(new StringReader(input));
                XdmNode root = doc.Select(Steps.Child("root")).First();
                string output = root.ToString();
                Console.Error.WriteLine(output);
                Assert.That(output.EndsWith("</root>"), "noFinalNewline");
            }
            catch (SaxonApiException err) {
                Assert.Fail();
            }
        }

        [Test]
        public void testXmlStreamWriter() {
            // Bug 4929
            try {
                string xsl = "<xsl:stylesheet \n"
                             + "  xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n"
                             + "  exclude-result-prefixes=\"#all\"\n"
                             + "  version=\"3.0\">\n"
                             + "  \n"
                             + "  <xsl:mode on-no-match=\"shallow-copy\"/>\n"
                             + "  \n"
                             + "</xsl:stylesheet>";
                Processor processor = new Processor();
                XdmNode inputDoc = processor.NewDocumentBuilder().Build(new Uri(ConfigTest.DataDirUri, "renvooi.xml"));
                StringWriter sw = new StringWriter();
                Serializer serializer = processor.NewSerializer(sw);
                XsltExecutable executable = processor.NewXsltCompiler().Compile(new StringReader(xsl));
                executable.Load30().ApplyTemplates(inputDoc, serializer);

                XdmNode result = processor.NewDocumentBuilder().Build(new StringReader(sw.ToString()));
                XdmFunctionItem deepEq = processor.GetSystemFunction(
                    new QName(NamespaceConstant.FN, "deep-equal"), 2);
                //assert deepEq != null;
                XdmValue compare = deepEq.Invoke(new XdmValue[] { inputDoc, result }, processor);
                Assert.That(compare is XdmAtomicValue);
                Assert.That(((XdmAtomicValue)compare).AsBoolean());

            }
            catch (Exception e) {
                Assert.Fail(e.Message);
                //e.printStackTrace(System.err);
            }
        }

        // [Test]
        // public void testXmlStreamWriterNamespaces() {    // Bug 5075
        //     try {
        //         StringWriter sw = new StringWriter();
        //         Serializer serializer = new Processor(false).NewSerializer();
        //         // Setup properties for indent
        //         serializer.SetOutputProperty(Serializer.INDENT, "no");
        //         serializer.OutputWriter = sw;
        //         XMLStreamWriter writer = serializer.getXMLStreamWriter();
        //
        //         // Start the document
        //         writer.writeStartDocument("UTF-8", "1.0");
        //
        //         string uri0 = "http://www.sample.com/level/0";
        //         string uri1 = "http://www.sample.com/level/1";
        //         string uri2 = "http://www.sample.com/level/2";
        //
        //         writer.writeStartElement("ns0", "root", "uri0");
        //
        //         writer.writeNamespace("ns0", uri0);
        //         writer.writeNamespace("ns1", uri1);
        //         writer.writeNamespace("ns2", uri2);
        //
        //         writer.writeStartElement("ns1", "orders", uri1);
        //         writer.writeStartElement("ns2", "order", uri2);
        //
        //         writer.writeEmptyElement("ns1", "item1", uri1);
        //         writer.writeEmptyElement("ns2", "item2", uri2);
        //
        //         writer.writeEndElement();//close l2
        //         writer.writeEndElement();//close L1
        //         writer.writeEndElement();//close L0
        //
        //         writer.writeEndDocument(); // close document
        //
        //         writer.flush();
        //         writer.close();
        //         //System.err.println(sw);
        //
        //         Assert.That(sw.ToString().Contains("<ns0:root xmlns:ns0=\"http://www.sample.com/level/0\" xmlns:ns1=\"http://www.sample.com/level/1\" xmlns:ns2=\"http://www.sample.com/level/2\">"));
        //             // namespace declarations could be in a different order...
        //         Assert.That(sw.ToString().Contains("<ns1:orders><ns2:order><ns1:item1/><ns2:item2/></ns2:order></ns1:orders>"));
        //     } catch (SaxonApiException e) {
        //         Assert.Fail();
        //     } catch (XMLStreamException e) {
        //         Assert.Fail();
        //     }
    }
}

    
   