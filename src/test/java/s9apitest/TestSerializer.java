package s9apitest;

import com.saxonica.config.ProfessionalTransformerFactory;
import jaxptest.ConfigTest;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.jaxp.TransformerImpl;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.OutputURIResolver;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.s9api.*;
import net.sf.saxon.serialize.CharacterMap;
import net.sf.saxon.serialize.CharacterMapIndex;
import net.sf.saxon.stax.XMLStreamWriterDestination;
import net.sf.saxon.z.IntHashMap;
import org.junit.jupiter.api.Test;
import org.xml.sax.ContentHandler;
import org.xml.sax.InputSource;
import org.xml.sax.XMLReader;

import javax.xml.parsers.SAXParserFactory;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamWriter;
import javax.xml.transform.Result;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.StringReader;
import java.io.StringWriter;

import static net.sf.saxon.s9api.streams.Steps.child;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests for the s9api XQueryCompiler
 */
public class TestSerializer {
    private final ConfigTest configTest = new ConfigTest();
    
    @Test
    public void testXQuerySerialization() {
        try {
            Processor proc = new Processor(false);
            StringReader sr = new StringReader("<doc><e>text</e></doc>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode node = builder.build(source);
            XQueryCompiler xqcomp = proc.newXQueryCompiler();
            XQueryExecutable xqexec = xqcomp.compile("declare option saxon:output 'cdata-section-elements=e';.");
            XQueryEvaluator xqeval = xqexec.load();
            xqeval.setContextItem(node);

            ByteArrayOutputStream os = new ByteArrayOutputStream();
            Serializer serializer = proc.newSerializer(os);

            serializer.setOutputProperty(Serializer.Property.BYTE_ORDER_MARK, "no");
            serializer.setOutputProperty(Serializer.Property.ENCODING, "utf-8");
            serializer.setOutputProperty(Serializer.Property.INDENT, "no");
            serializer.setOutputProperty(Serializer.Property.METHOD, "xml");
            serializer.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");

            xqeval.setDestination(serializer);
            xqeval.run();

            String result = os.toString();
            assertFalse(result.contains("<?xml"), "XML declaration exists");
            assertTrue(result.contains("doc><e"), "Indented");
            assertTrue(result.contains("<e><![CDATA"), "Cdata section");

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testXsltSerialization() {
        try {
            Processor proc = new Processor(false);
            StringReader sr = new StringReader("<doc><e>text</e></doc>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode node = builder.build(source);
            XsltCompiler xqcomp = proc.newXsltCompiler();
            sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                                          "<xsl:output indent='yes' cdata-section-elements='e'/>" +
                                          "<xsl:template match='/'><xsl:copy-of select='.'/></xsl:template>" +
                                          "</xsl:stylesheet>");
            SAXSource style = new SAXSource(new InputSource(sr));
            XsltTransformer trans = xqcomp.compile(style).load();

            ByteArrayOutputStream os = new ByteArrayOutputStream();
            Serializer serializer = proc.newSerializer(os);

            serializer.setOutputProperty(Serializer.Property.BYTE_ORDER_MARK, "no");
            serializer.setOutputProperty(Serializer.Property.ENCODING, "utf-8");
            serializer.setOutputProperty(Serializer.Property.INDENT, "no");
            serializer.setOutputProperty(Serializer.Property.METHOD, "xml");
            serializer.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");

            trans.setSource(node.asSource());
            trans.setDestination(serializer);
            trans.transform();

            String result = os.toString();
            assertFalse(result.contains("<?xml"), "XML declaration exists");
            assertTrue(result.contains("doc><e"), "Indented");
            assertTrue(result.contains("<e><![CDATA"), "Cdata section");

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testXsltCharacterMaps() {
        try {
            Processor proc = new Processor(false);
            StringReader sr = new StringReader("<doc><e>text</e></doc>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode node = builder.build(source);
            XsltCompiler xqcomp = proc.newXsltCompiler();
            sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                                          "<xsl:output method='xml' use-character-maps='m'/>" +
                                          "<xsl:character-map name='m'><xsl:output-character character='e' string='f'/></xsl:character-map>" +
                                          "<xsl:template match='/'><xsl:copy-of select='.'/></xsl:template>" +
                                          "</xsl:stylesheet>");
            SAXSource style = new SAXSource(new InputSource(sr));
            XsltTransformer trans = xqcomp.compile(style).load();

            Serializer serializer = proc.newSerializer();
            ByteArrayOutputStream os = new ByteArrayOutputStream();
            serializer.setOutputStream(os);

            trans.setSource(node.asSource());
            trans.setDestination(serializer);
            trans.transform();

            String result = os.toString();
            assertTrue(result.contains("tfxt"), "Charmapped");

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testXsltCharacterMapsFromApi() {
        try {
            Processor proc = new Processor(false);
            StringReader sr = new StringReader("<doc><e>text</e></doc>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode node = builder.build(source);
            XsltCompiler xqcomp = proc.newXsltCompiler();
            sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                                          "<xsl:output method='xml' use-character-maps='m'/>" +
                                          "<xsl:character-map name='m'><xsl:output-character character='e' string='f'/></xsl:character-map>" +
                                          "<xsl:template match='/'><xsl:copy-of select='.'/></xsl:template>" +
                                          "</xsl:stylesheet>");
            SAXSource style = new SAXSource(new InputSource(sr));
            XsltTransformer trans = xqcomp.compile(style).load();

            Serializer serializer = proc.newSerializer();
            serializer.setOutputProperty(Serializer.Property.METHOD, "xml");
            CharacterMapIndex mapIndex = new CharacterMapIndex();
            IntHashMap<String> map = new IntHashMap<>();
            map.put('e', "f");
            CharacterMap namedMap = new CharacterMap(new StructuredQName("", "", "m"), map);
            mapIndex.putCharacterMap(new StructuredQName("", "", "m"), namedMap);
            serializer.setCharacterMap(mapIndex);
            ByteArrayOutputStream os = new ByteArrayOutputStream();
            serializer.setOutputStream(os);

            trans.setSource(node.asSource());
            trans.setDestination(serializer);
            trans.transform();

            String result = os.toString();
            assertTrue(result.contains("tfxt"), "Charmapped");

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


    @Test
    public void testSecondaryOutputDestination() {
        try {
            new File(configTest.getDataDir() + "temp/first.xml").delete();
            new File(configTest.getDataDir() + "temp/second.xml").delete();
            Processor proc = new Processor(false);
            proc.setConfigurationProperty(Feature.TIMING, true);
            StringReader sr = new StringReader("<doc><e>text</e></doc>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode node = builder.build(source);
            XsltCompiler xqcomp = proc.newXsltCompiler();
            sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                                          "<xsl:output method='xml'/>" +
                                          "<xsl:template match='/'>" +
                                          "  <xsl:copy-of select='.'/>" +
                                          "  <xsl:result-document href='second.xml'><xsl:copy-of select='.'/></xsl:result-document>" +
                                          "</xsl:template>" +
                                          "</xsl:stylesheet>");
            SAXSource style = new SAXSource(new InputSource(sr));
            XsltTransformer trans = xqcomp.compile(style).load();

            Serializer serializer = proc.newSerializer();
            serializer.setOutputFile(new File(configTest.getDataDir() + "temp/first.xml"));
            trans.setSource(node.asSource());
            trans.setDestination(serializer);
            trans.transform();

            assertTrue(new File(configTest.getDataDir() + "temp/first.xml").exists(), "first exists");
            assertTrue(new File(configTest.getDataDir() + "temp/second.xml").exists(), "second exists");

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    public static void testWriteXdmValue() {
        try {
            Processor proc = new Processor(false);
            StringReader sr = new StringReader("<doc><e>text</e></doc>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode node = builder.build(source);
            proc.writeXdmValue(node, new XdmDestination());
        } catch (SaxonApiException err) {
            fail(err.getMessage());
        }
    }

    @Test
    public void testS9apiImport() throws Exception {
        // tests use of dynamic serialization properties chosen by xsl:result-document
        try {
            final Processor processor = new Processor(true);

            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            StreamSource xsltSource = new StreamSource(configTest.getDataDir() + "welch-ser/A.xsl");

            XsltExecutable xsltExecutable = xsltCompiler.compile(xsltSource);
            XsltTransformer xsltTransformer = xsltExecutable.load();
            xsltTransformer.setInitialTemplate(new QName("main"));

            StringWriter sw = new StringWriter();
            Serializer serializer = processor.newSerializer();
            serializer.setOutputWriter(sw);
            xsltTransformer.setDestination(serializer);
            xsltTransformer.setResultDocumentHandler(uri -> serializer);

            xsltTransformer.transform();
            assertTrue(sw.toString().contains("<foo>bar</foo>")); // proving that method="xml" was used
        } catch (SaxonApiException e) {
            fail();
            e.printStackTrace();
        }
    }

    @Test
    public void testJaxpImport() throws Exception {
        try {
            // same test as above but with JAXP interface
            TransformerFactory factory = new net.sf.saxon.TransformerFactoryImpl();
            StringWriter sw = new StringWriter();
            factory.setAttribute(FeatureKeys.OUTPUT_URI_RESOLVER, new OutputURIResolver() {
                @Override
                public OutputURIResolver newInstance() {
                    return this;
                }

                @Override
                public Result resolve(String href, String base) throws TransformerException {
                    return new StreamResult(sw);
                }

                @Override
                public void close(Result result) throws TransformerException {
                    // no action
                }
            } );
            Transformer transformer = factory.newTransformer(new StreamSource(configTest.getDataDir() + "welch-ser/A.xsl"));

            ((TransformerImpl) transformer).setInitialTemplate("main");

            transformer.transform(null, new StreamResult(sw));
            assertTrue(sw.toString().contains("<foo>bar</foo>")); // proving that method="xml" was used
        } catch (TransformerException e) {
            fail();
            e.printStackTrace();
        }
    }

    @Test
    public void testAsSaxContentHandler() throws Exception {
        try {

            Processor proc = new Processor(false);
            StringReader sr = new StringReader("<doc><e>text</e></doc>");

            SAXParserFactory factory = SAXParserFactory.newInstance();
            factory.setNamespaceAware(true);
            factory.setValidating(false);
            XMLReader reader = factory.newSAXParser().getXMLReader();

            ByteArrayOutputStream os = new ByteArrayOutputStream();
            Serializer serializer = proc.newSerializer(os);
            serializer.setOutputProperty(Serializer.Property.BYTE_ORDER_MARK, "no");
            serializer.setOutputProperty(Serializer.Property.ENCODING, "utf-8");
            serializer.setOutputProperty(Serializer.Property.INDENT, "no");
            serializer.setOutputProperty(Serializer.Property.METHOD, "xml");
            serializer.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            serializer.setOutputProperty(Serializer.Property.CDATA_SECTION_ELEMENTS, "e");

            ContentHandler ch = serializer.getContentHandler();
            reader.setContentHandler(ch);
            reader.parse(new InputSource(sr));

            String result = os.toString();
            assertFalse(result.contains("<?xml"), "XML declaration exists");
            assertTrue(result.contains("doc><e"), "Indented");
            assertTrue(result.contains("<e><![CDATA"), "Cdata section");

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void test3631() {
        try {
            String dir = configTest.getDataDir() + "nadolu3631/";
            String sourceID = dir + "test.xml";
            String xslID = dir + "main.xsl";

            // Create a transform factory instance.
            TransformerFactoryImpl tfactory = new ProfessionalTransformerFactory();

            // Create a transformer for the stylesheet.
            Transformer transformer = tfactory.newTransformer(new StreamSource(xslID));

            // Obtain the encoding
            String transformerEncoding = transformer.getOutputProperty("encoding");

            // Transform the source XML.
            StringWriter writer = new StringWriter();
            transformer.transform(new StreamSource(sourceID), new StreamResult(writer));
            String out = writer.toString();
            assertTrue(out.contains("<second_processing>"));
        } catch (TransformerException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testBase64BinarySerialization() { // bug 4794
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

        ByteArrayOutputStream ms = new ByteArrayOutputStream();
        try {
            Processor processor = new Processor(true);
            XsltCompiler comp = processor.newXsltCompiler();
            Xslt30Transformer transformer = comp.compile(new StreamSource(new StringReader(xslt))).load30();
            Serializer ser = processor.newSerializer();
            ser.setOutputStream(ms);
            transformer.callTemplate(null, ser);
        } catch (SaxonApiException e) {
            System.err.println(e.getMessage());
            e.printStackTrace();
            fail();
        }
        byte[] x = ms.toByteArray();
        assertEquals(-119, x[0]);
        assertEquals(80, x[1]);
        assertEquals(78, x[2]);
        assertEquals(71, x[3]);
        assertEquals(247, x.length);
    }


    @Test
    public void testHexBinarySerialization() { // bug 4794
        String xslt =

                "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:saxon='http://saxon.sf.net/'>" +
                        "<xsl:output method='saxon:hexBinary'/>" +
                        "<xsl:template name = 'xsl:initial-template'>" +
                        "<xsl:text>CAFEBABE</xsl:text>" +
                        "</xsl:template>" +
                        "</xsl:transform>";

        ByteArrayOutputStream ms = new ByteArrayOutputStream();
        try {
            Processor processor = new Processor(true);
            XsltCompiler comp = processor.newXsltCompiler();
            Xslt30Transformer transformer = comp.compile(new StreamSource(new StringReader(xslt))).load30();
            Serializer ser = processor.newSerializer();
            ser.setOutputStream(ms);
            transformer.callTemplate(null, ser);
        } catch (SaxonApiException e) {
            System.err.println(e.getMessage());
            e.printStackTrace();
            fail();
        }
        byte[] x = ms.toByteArray();
        assertEquals(-54, x[0]);
        assertEquals(-2, x[1]);
        assertEquals(-70, x[2]);
        assertEquals(-66, x[3]);
        assertEquals(4, x.length);
    }

    @Test
    public void testXQuerySerializationMethod() {
        String xslt =

                "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                        "<xsl:output method='saxon:xquery' xmlns:saxon='http://saxon.sf.net/'/>" +
                        "<xsl:template name = 'xsl:initial-template'>" +
                        "<a>{$a &lt; '&lt;'}</a>" +
                        "</xsl:template>" +
                        "</xsl:transform>";

        StringWriter ms = new StringWriter();
        try {
            Processor processor = new Processor(true);
            XsltCompiler comp = processor.newXsltCompiler();
            Xslt30Transformer transformer = comp.compile(new StreamSource(new StringReader(xslt))).load30();
            Serializer ser = processor.newSerializer();
            ser.setOutputWriter(ms);
            transformer.callTemplate(null, ser);
        } catch (SaxonApiException e) {
            System.err.println(e.getMessage());
            e.printStackTrace();
            fail();
        }

        assertEquals("<a>{$a < '&lt;'}</a>", ms.toString());

    }


    @Test
    public void testIndentation() {
        try {
            String in = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                    + "<root>\n"
                    + "    <parent>\n"
                    + "        <foo>\n"
                    + "            <bar>foo</bar>\n"
                    + "        </foo>        \n"
                    + "    </parent>\n"
                    + "</root>";
            Processor proc = new Processor(false);
            DocumentBuilder builder = proc.newDocumentBuilder();
            //builder.setWhitespaceStrippingPolicy(WhitespaceStrippingPolicy.ALL);
            XdmNode doc = builder.build(new StreamSource(new StringReader(in)));
            StringWriter sw = new StringWriter();
            Serializer serializer = proc.newSerializer(sw);
            serializer.setOutputProperty(Serializer.Property.METHOD, "xml");
            serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
            XdmNode foo = doc.axisIterator(Axis.DESCENDANT, new QName("foo")).next();
            serializer.serializeNode(foo);
            System.err.println(sw);
        } catch (SaxonApiException err) {
            fail();
        }
    }

    @Test
    public void testElementToString() {
        try {
            String in = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                    + "<root><parent><foo>\n"
                    + "            <bar>foo</bar>\n"
                    + "</foo></parent></root>";
            Processor proc = new Processor(false);
            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setWhitespaceStrippingPolicy(WhitespaceStrippingPolicy.ALL);
            XdmNode doc = builder.build(new StreamSource(new StringReader(in)));
            XdmNode root = doc.select(child("root")).asNode();
            String out = root.toString();
            System.err.println(out);
            assertTrue(out.endsWith("</root>"), "noFinalNewline");
        } catch (SaxonApiException err) {
            fail();
        }
    }

    @Test
    public void testXmlStreamWriter() {    // Bug 4929
        try {
            String xsl = "<xsl:stylesheet \n"
                    + "  xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n"
                    + "  exclude-result-prefixes=\"#all\"\n"
                    + "  version=\"3.0\">\n"
                    + "  \n"
                    + "  <xsl:mode on-no-match=\"shallow-copy\"/>\n"
                    + "  \n"
                    + "</xsl:stylesheet>";
            Processor processor = new Processor(false);
            XdmNode inputDoc = processor.newDocumentBuilder().build(new File(configTest.getDataDir() + "renvooi.xml"));
            StringWriter sw = new StringWriter();
            Serializer serializer = processor.newSerializer(sw);
            XMLStreamWriter xsw = serializer.getXMLStreamWriter();
            XsltExecutable executable = processor.newXsltCompiler().compile(new StreamSource(new StringReader(xsl)));
            executable.load30().applyTemplates(inputDoc, new XMLStreamWriterDestination(xsw));

            XdmNode result = processor.newDocumentBuilder().build(new StreamSource(new StringReader(sw.toString())));
            XdmFunctionItem deepEq = XdmFunctionItem.getSystemFunction(
                    processor, new QName(NamespaceConstant.FN, "deep-equal"), 2);
            assert deepEq != null;
            XdmValue compare = deepEq.call(processor, inputDoc, result);
            assertTrue(compare instanceof XdmAtomicValue);
            assertTrue(((XdmAtomicValue) compare).getBooleanValue());

        } catch (Exception e) {
            e.printStackTrace(System.err);
        }
    }

    @Test
    public void testXmlStreamWriterNamespaces() {    // Bug 5075
        try {
            StringWriter sw = new StringWriter();
            Serializer serializer = new Processor(false).newSerializer();
            // Setup properties for indent
            serializer.setOutputProperty(Serializer.Property.INDENT, "no");
            serializer.setOutputWriter(sw);
            XMLStreamWriter writer = serializer.getXMLStreamWriter();

            // Start the document
            writer.writeStartDocument("UTF-8", "1.0");

            String uri0 = "http://www.sample.com/level/0";
            String uri1 = "http://www.sample.com/level/1";
            String uri2 = "http://www.sample.com/level/2";

            writer.writeStartElement("ns0", "root", "uri0");

            writer.writeNamespace("ns0", uri0);
            writer.writeNamespace("ns1", uri1);
            writer.writeNamespace("ns2", uri2);

            writer.writeStartElement("ns1", "orders", uri1);
            writer.writeStartElement("ns2", "order", uri2);

            writer.writeEmptyElement("ns1", "item1", uri1);
            writer.writeEmptyElement("ns2", "item2", uri2);

            writer.writeEndElement();//close l2
            writer.writeEndElement();//close L1
            writer.writeEndElement();//close L0

            writer.writeEndDocument(); // close document

            writer.flush();
            writer.close();
            //System.err.println(sw);

            assertTrue(sw.toString().contains("<ns0:root xmlns:ns0=\"http://www.sample.com/level/0\" xmlns:ns1=\"http://www.sample.com/level/1\" xmlns:ns2=\"http://www.sample.com/level/2\">"));
                // namespace declarations could be in a different order...
            assertTrue(sw.toString().contains("<ns1:orders><ns2:order><ns1:item1/><ns2:item2/></ns2:order></ns1:orders>"));
        } catch (SaxonApiException e) {
            fail();
        } catch (XMLStreamException e) {
            fail();
        }
    }
   

//    public void testBulkIndent() {
          // Test to check that indentation produces reasonable results. Commented out because
          // there is no automated checking of the results, it relies on visual inspection.
//
//        File dir = new File(configTest.getDataDir());
//        File[] files = dir.listFiles(
//                new FilenameFilter() {
//                    public boolean accept(File dir, String name) {
//                        return name.endsWith(".xml") || name.endsWith(".xsl") || name.endsWith(".xsd");
//                    }
//                }
//        );
//
//        Processor proc = new Processor(false);
//        DocumentBuilder builder = proc.newDocumentBuilder();
//        //builder.setWhitespaceStrippingPolicy(WhitespaceStrippingPolicy.ALL);
//        Serializer serializer = proc.newSerializer(System.err);
//        serializer.setOutputProperty(Serializer.Property.METHOD, "xml");
//        serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
//
//        for (File file : files) {
//            System.err.println(file.getPath());
//            try {
//                XdmNode doc = builder.build(new StreamSource(file));
//                serializer.serializeNode(doc);
//
//            } catch (SaxonApiException e) {
//                System.err.println("**** Failed to process document: " + e.getMessage());
//            }
//        }
//    }


}
