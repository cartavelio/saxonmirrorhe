package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.Invalidity;
import net.sf.saxon.lib.InvalidityHandler;
import net.sf.saxon.lib.StandardLogger;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.TreeModel;
import net.sf.saxon.option.dom4j.DOM4JNodeWrapper;
import net.sf.saxon.option.dom4j.DOM4JObjectModel;
import net.sf.saxon.option.jdom2.JDOM2ObjectModel;
import net.sf.saxon.option.xom.XOMDocumentWrapper;
import net.sf.saxon.option.xom.XOMObjectModel;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;
import java.io.StringWriter;
import java.net.URI;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.function.Predicate;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests for the s9api DocumentBuilder
 */
public class TestDocumentBuilder {
    private final ConfigTest configTest = new ConfigTest();

    @Test
    public void testSaxSourceNoSystemId() {
        try {
            StringReader sr = new StringReader("<doc/>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = new Processor(false).newDocumentBuilder();
            XdmNode d = builder.build(source);
            assertNotNull(d);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testSaxSourceDTDValidation() {
        String validXML = "<!DOCTYPE a [<!ELEMENT a EMPTY>]><a/>";
        String invalidXML = "<!DOCTYPE a [<!ELEMENT a (b+)>]><a/>";
        DocumentBuilder builder = new Processor(false).newDocumentBuilder();
        builder.setDTDValidation(true);

        StringReader sr = new StringReader(validXML);
        SAXSource source = new SAXSource(new InputSource(sr));
        try {
            XdmNode d = builder.build(source);
            assertNotNull(d);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }

        sr = new StringReader(invalidXML);
        source = new SAXSource(new InputSource(sr));
        try {
            XdmNode d = builder.build(source);
            fail("invalid document parsed without error");
        } catch (SaxonApiException e) {
            // expected failure;
        }

        builder.setDTDValidation(false);
        sr = new StringReader(invalidXML);
        source = new SAXSource(new InputSource(sr));
        try {
            XdmNode d = builder.build(source);
            assertNotNull(d);
        } catch (SaxonApiException e) {
            fail("Parsing failed but validation was not requested: " + e.getMessage());
        }

    }

    @Test
    public void testNamespaceNodes() {
        try {
            StringReader sr = new StringReader("<doc xmlns='abc.uri'><e xmlns:a='one.uri' xmlns:b='two.uri'/></doc>");
            SAXSource source = new SAXSource(new InputSource(sr));
            Processor processor = new Processor(false);
            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode d = builder.build(source);
            assertNotNull(d);
            XPathSelector xp = processor.newXPathCompiler().compile("/*/*").load();
            xp.setContextItem(d);
            XdmNode e = (XdmNode)xp.evaluateSingle();
            assertNotNull(e); XdmSequenceIterator nsIter = e.axisIterator(Axis.NAMESPACE);
            HashMap<String, String> nsBindings = new HashMap<>();
            while (nsIter.hasNext()) {
                XdmNode ns = (XdmNode) nsIter.next();
                System.err.println(ns);
                nsBindings.put(ns.getNodeName()==null ? "" : ns.getNodeName().getLocalName(), ns.getStringValue());
            }
            assertEquals(4, nsBindings.size());


        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testDOMElementNode() {
        try {

            String in = "<?xml version='1.0'?> \n" +
                    "<Document> <e> <f/><f/> </e> </Document>";

            // Read an XML file to a DOM
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            javax.xml.parsers.DocumentBuilder db = factory.newDocumentBuilder();
            InputSource is = new InputSource(new StringReader(in));
            org.w3c.dom.Document doc = db.parse(is);

            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            XsltExecutable exec = comp.compile(new StreamSource(new
                    File(configTest.getDataDir() + "identity.xsl")));
            XsltTransformer xsl = exec.load();
            xsl.setSource(new DOMSource(doc.getDocumentElement()));
            StringWriter writer = new StringWriter();
            Serializer out = proc.newSerializer();
            out.setOutputWriter(writer);
            xsl.setDestination(out);
            xsl.transform();
            assertTrue(writer.toString().contains("<f/>"));
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testXomBuilder() {
        try {
            String in = "<?xml version='1.0'?> \n" +
                    "<Document> <e a='13' xml:lang='en'> <c:f xmlns:c='http://c/'/><f xmlns='http://c/'/> </e> </Document>";
            Processor proc = new Processor(true);
            DocumentBuilder db = proc.newDocumentBuilder();
            db.setTreeModel(XOMObjectModel.getInstance());
            XdmNode doc = db.build(new StreamSource(new StringReader(in)));
            assertTrue(doc.getUnderlyingNode() instanceof XOMDocumentWrapper, "isXOM");
            System.err.println(doc.toString());
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testJdom2Builder() {
        try {
            String in = "<?xml version='1.0'?> \n" +
                    "<Document> <e a='13' xml:lang='en'> <c:f xmlns:c='http://c/'/><f xmlns='http://c/'/> </e> </Document>";
            Processor proc = new Processor(true);
            DocumentBuilder db = proc.newDocumentBuilder();
            db.setTreeModel(JDOM2ObjectModel.getInstance());
            XdmNode doc = db.build(new StreamSource(new StringReader(in)));
            assertTrue(doc.getUnderlyingNode() instanceof net.sf.saxon.option.jdom2.JDOM2NodeWrapper, "isJDOM");
            System.err.println(doc.toString());
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testDom4JBuilder() {
        try {
            String in = "<?xml version='1.0'?> \n" +
                    "<Document> <e a='13' xml:lang='en'> <c:f xmlns:c='http://c/'/><f xmlns='http://c/'/> </e> </Document>";
            Processor proc = new Processor(true);
            DocumentBuilder db = proc.newDocumentBuilder();
            db.setTreeModel(DOM4JObjectModel.getInstance());
            XdmNode doc = db.build(new StreamSource(new StringReader(in)));
            assertTrue(doc.getUnderlyingNode() instanceof DOM4JNodeWrapper, "isDom4J");
            System.err.println(doc.toString());
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    /**
     * Test that when a document is loaded from multiple external entities, the base URIs of
     * elements are preserved
     */

    @Test
    public void testBaseUriFromEntities() {
        try {
            Processor processor = new Processor(false);
            StreamSource source = new StreamSource(new File(configTest.getDataDir() + "book-wrapper.xml"));
            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode d = builder.build(source);
            XPathCompiler xpc = processor.newXPathCompiler();
            XPathSelector s = xpc.compile("distinct-values(//*/base-uri())").load();
            s.setContextItem(d);
            XdmValue v = s.evaluate();
            assertEquals(3, v.size(), "3 distinct base URIs");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test that when a document with multiple base URIs is copied, the base URIs of
     * elements are preserved
     */

    @Test
    public void testBaseUriFromCopy() {
        try {
            Processor processor = new Processor(false);
            StreamSource source = new StreamSource(new File(configTest.getDataDir() + "book-wrapper.xml"));
            DocumentBuilder builder = processor.newDocumentBuilder();
            builder.setTreeModel(TreeModel.TINY_TREE);
            XdmNode d = builder.build(source);
            builder.setTreeModel(TreeModel.LINKED_TREE);
            XdmNode e = builder.build(d.asSource());
            XPathCompiler xpc = processor.newXPathCompiler();
            XPathSelector s = xpc.compile("distinct-values(//*/base-uri())").load();
            s.setContextItem(d);
            XdmValue v = s.evaluate();
            assertEquals(3, v.size(), "3 distinct base URIs in d");
            s.setContextItem(e);
            v = s.evaluate();
            assertEquals(3, v.size(), "3 distinct base URIs in e");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test that when a document is loaded from multiple external entities, the base URIs of
     * elements are preserved
     */

    @Test
    public void testBaseUriOfAttribute() {
        try {
            Processor processor = new Processor(false);
            StreamSource source = new StreamSource(new File(configTest.getDataDir() + "book-wrapper.xml"));
            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode d = builder.build(source);
            XPathCompiler xpc = processor.newXPathCompiler();
            XPathSelector s = xpc.compile("//@CODE").load();
            s.setContextItem(d);
            for (XdmItem i:s.evaluate()) {
                assertTrue(((XdmNode)i).getBaseURI().toString().endsWith("categories.xml"), "baseURI");
            }
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test document builder with document projection
     */

    @Test
    public void testProjection001() {
        try {

            String fileName = configTest.getDataDir() + "books.xml";

            Processor processor = new Processor(true);
            XQueryCompiler xpc = processor.newXQueryCompiler();
            XQueryExecutable exp = xpc.compile("<out>{//CATEGORY}</out>");

            DocumentBuilder builder = processor.newDocumentBuilder();
            builder.setDocumentProjectionQuery(exp);
            XdmNode doc = builder.build(new File(fileName));

            // Test that the projected doc contains the CATEGORY elements

            StringWriter sw = new StringWriter();
            Serializer destination = processor.newSerializer();
            destination.setOutputWriter(sw);
            XQueryEvaluator eval = exp.load();
            eval.setContextItem(doc);
            eval.run(destination);

            assertTrue(sw.toString().contains("CODE=\"P\" DESC=\"Paperback\""), "contains CAT");

            // Test that the projected doc omits the ITEM elements

            exp = xpc.compile("count(//ITEM)");
            StringWriter sw2 = new StringWriter();
            Serializer destination2 = processor.newSerializer();
            destination2.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            destination2.setOutputWriter(sw2);
            XQueryEvaluator eval2 = exp.load();
            eval2.setContextItem(doc);
            eval2.run(destination2);

            assertTrue(sw2.toString().equals("0"), "omits ITEM");

        } catch(Exception ex) {
            ex.printStackTrace();
            fail(ex.getMessage());
        }
    }

    @Test
    public void testSvg11Dtd() {
        // Test that the SVG 1.1 DTD can be retrieved from the local cache
        try {
            Processor proc = new Processor(false);
            proc.setConfigurationProperty(Feature.TIMING, true);
            DocumentBuilder b = proc.newDocumentBuilder();
            b.setDTDValidation(true);
            StringReader sr = new StringReader(
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<!-- Generated by graphviz version 2.28.0 (20110901.0847)\n" +
                            " --><!-- Title: g Pages: 1 --><!DOCTYPE svg\n" +
                            "  PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">\n" +
                            "<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\"\n" +
                            "     width=\"206pt\"\n" +
                            "     height=\"98pt\"\n" +
                            "     viewBox=\"0.00pt 0.00pt 206.00pt 98.00pt\"\n" +
                            "     preserveAspectRatio=\"xMidYMid meet\"\n" +
                            "     zoomAndPan=\"magnify\"\n" +
                            "     version=\"1.1\"\n" +
                            "     contentScriptType=\"application/ecmascript\"\n" +
                            "     contentStyleType=\"text/css\">\n" +
                            "   <g id=\"graph1\" class=\"graph\" transform=\"scale(1 1) rotate(0) translate(4 94)\">\n" +
                            "      <title>g</title>\n" +
                            "      <polygon fill=\"white\" stroke=\"white\" points=\"-4,5 -4,-94 203,-94 203,5 -4,5\"/>\n" +
                            "      <!-- d7e7880 --><g id=\"node2\" class=\"node\">\n" +
                            "         <title>d7e7880</title>\n" +
                            "         <polygon fill=\"none\" stroke=\"black\" points=\"54,-63 0,-63 0,-27 54,-27 54,-63\"/>\n" +
                            "         <text text-anchor=\"middle\" x=\"27\" y=\"-40.4\" font-family=\"Times,serif\"\n" +
                            "               font-size=\"14.00\">A</text>\n" +
                            "      </g>\n" +
                            "   </g>\n" +
                            "</svg>");
            StreamSource ss = new StreamSource(sr, "http://www.saxonica.com/notthere");
            b.build(ss);
        } catch (SaxonApiException sae) {
            sae.printStackTrace();
            fail();
        }
    }

    @Test
    public void testSvg10Dtd() {
        // Test that the SVG 1.0 DTD can be retrieved from the local cache
        try {
            Processor proc = new Processor(false);
            proc.setConfigurationProperty(Feature.TIMING, true);
            DocumentBuilder b = proc.newDocumentBuilder();
            b.setDTDValidation(true);
            StringReader sr = new StringReader(
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<!-- Generated by graphviz version 2.28.0 (20110901.0847)\n" +
                            " --><!-- Title: g Pages: 1 --><!DOCTYPE svg\n" +
                            "  PUBLIC \"-//W3C//DTD SVG 1.0//EN\" \"http://www.w3.org/TR/2001/REC-SVG-20010904/DTD/svg10.dtd\">\n" +
                            "<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\"\n" +
                            "     width=\"206pt\"\n" +
                            "     height=\"98pt\"\n" +
                            "     viewBox=\"0.00pt 0.00pt 206.00pt 98.00pt\"\n" +
                            "     preserveAspectRatio=\"xMidYMid meet\"\n" +
                            "     zoomAndPan=\"magnify\"\n" +
                            "     version=\"1.0\"\n" +
                            "     contentScriptType=\"application/ecmascript\"\n" +
                            "     contentStyleType=\"text/css\">\n" +
                            "   <g id=\"graph1\" class=\"graph\" transform=\"scale(1 1) rotate(0) translate(4 94)\">\n" +
                            "      <title>g</title>\n" +
                            "      <polygon fill=\"white\" stroke=\"white\" points=\"-4,5 -4,-94 203,-94 203,5 -4,5\"/>\n" +
                            "      <!-- d7e7880 --><g id=\"node2\" class=\"node\">\n" +
                            "         <title>d7e7880</title>\n" +
                            "         <polygon fill=\"none\" stroke=\"black\" points=\"54,-63 0,-63 0,-27 54,-27 54,-63\"/>\n" +
                            "         <text text-anchor=\"middle\" x=\"27\" y=\"-40.4\" font-family=\"Times,serif\"\n" +
                            "               font-size=\"14.00\">A</text>\n" +
                            "      </g>\n" +
                            "   </g>\n" +
                            "</svg>");
            StreamSource ss = new StreamSource(sr, "http://www.saxonica.com/notthere");
            b.build(ss);
        } catch (SaxonApiException sae) {
            sae.printStackTrace();
            fail();
        }
    }

    @Test
    public void testWhitespacePolicyUntyped() {
        try {
            Processor proc = new Processor(false);
            StringReader sr = new StringReader("<doc> </doc>");
            DocumentBuilder builder = proc.newDocumentBuilder();

            SAXSource source = new SAXSource(new InputSource(sr));
            builder.setWhitespaceStrippingPolicy(WhitespaceStrippingPolicy.ALL);
            String s = builder.build(source).toString();
            assertTrue(s.contains("<doc/>"));

            sr = new StringReader("<doc> </doc>");
            source = new SAXSource(new InputSource(sr));
            builder.setWhitespaceStrippingPolicy(WhitespaceStrippingPolicy.NONE);
            s = builder.build(source).toString();
            assertTrue(s.contains("<doc> </doc"));

            sr = new StringReader("<doc> </doc>");
            source = new SAXSource(new InputSource(sr));
            builder.setWhitespaceStrippingPolicy(WhitespaceStrippingPolicy.IGNORABLE);
            s = builder.build(source).toString();
            assertTrue(s.contains("<doc> </doc"));

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testWhitespacePolicySchemaTyped() {
        String s;
        s = buildWithWhitespacePolicyTyped(WhitespaceStrippingPolicy.IGNORABLE, true);
        assertTrue(s.contains("<BOOKS><ITEM"));
        assertTrue(s.contains("TAX=\"V\""));
        s = buildWithWhitespacePolicyTyped(WhitespaceStrippingPolicy.IGNORABLE, false);
        assertTrue(s.contains("<BOOKS><ITEM"));
        assertFalse(s.contains("TAX=\"V\""));
        s = buildWithWhitespacePolicyTyped(WhitespaceStrippingPolicy.ALL, true);
        assertTrue(s.startsWith("rejected"));
        s = buildWithWhitespacePolicyTyped(WhitespaceStrippingPolicy.NONE, true);
        assertTrue(s.startsWith("rejected"));
    }

    private String buildWithWhitespacePolicyTyped(WhitespaceStrippingPolicy policy, boolean expandDefaults) {
        try {
            Processor proc = new Processor(true);

            XsdCompiler mgr = proc.newXsdCompiler();
            XsdSchema schema = mgr.compile(new StreamSource(new File(configTest.getDataDir() + "books.xsd")));
            SchemaValidator v = schema.newValidator();
            v.setExpandAttributeDefaults(expandDefaults);

            Serializer serializer = proc.newSerializer();
            serializer.setOutputProperty(Serializer.Property.METHOD, "xml");
            serializer.setOutputProperty(Serializer.Property.INDENT, "no");
            StringWriter sw = new StringWriter();

            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setSchemaValidator(v);

            builder.setWhitespaceStrippingPolicy(policy);
            XdmNode doc = builder.build(new File(configTest.getDataDir() + "books.xml"));
            serializer.setOutputWriter(sw);
            serializer.serializeXdmValue(doc);
            return sw.toString();

        } catch (SaxonApiException e) {
            return "rejected: " + e.getMessage();
        }
    }

    @Test
    public void testWhitespacePolicyWithDTDValidation() {
        String s;
        s = useDTDIgnorableWhitespace(WhitespaceStrippingPolicy.ALL);
        assertTrue(s.startsWith("rejected"));
        s = useDTDIgnorableWhitespace(WhitespaceStrippingPolicy.IGNORABLE);
        assertTrue(s.contains("<a><b/><c> </c></a>"));
        s = useDTDIgnorableWhitespace(WhitespaceStrippingPolicy.NONE);
        assertTrue(s.startsWith("rejected"));
    }

    private String useDTDIgnorableWhitespace(WhitespaceStrippingPolicy policy) {
        String validXML = "<!DOCTYPE a [<!ELEMENT a ((b|c)*)><!ELEMENT b EMPTY><!ELEMENT c (#PCDATA)>]><a> <b/> <c> </c> </a>";
        Processor proc = new Processor(false);
        DocumentBuilder builder = new Processor(false).newDocumentBuilder();
        builder.setWhitespaceStrippingPolicy(policy);
        builder.setDTDValidation(true);

        StringReader sr = new StringReader(validXML);
        SAXSource source = new SAXSource(new InputSource(sr));
        try {
            XdmNode d = builder.build(source);

            Serializer serializer = proc.newSerializer();
            serializer.setOutputProperty(Serializer.Property.METHOD, "xml");
            serializer.setOutputProperty(Serializer.Property.INDENT, "no");
            StringWriter sw = new StringWriter();
            serializer.setOutputWriter(sw);
            serializer.serializeXdmValue(d);
            return sw.toString();
        } catch (SaxonApiException e) {
            return "rejected: " + e.getMessage();
        }


    }

    @Test
    public void testCustomWhitespacePolicy() {
        try {
            Processor proc = new Processor(false);
            StringReader sr = new StringReader("<doc><b> </b> <i> </i></doc>");
            DocumentBuilder builder = proc.newDocumentBuilder();

            SAXSource source = new SAXSource(new InputSource(sr));
            builder.setWhitespaceStrippingPolicy(WhitespaceStrippingPolicy.makeCustomPolicy(
                new Predicate<QName>() {
                    @Override
                    public boolean test(QName value) {
                        return value.getLocalName().equals("doc") || value.getLocalName().equals("i");
                    }
                }
            ));
            XdmNode doc = builder.build(source);
            Serializer serializer = proc.newSerializer();
            serializer.setOutputProperty(Serializer.Property.METHOD, "xml");
            serializer.setOutputProperty(Serializer.Property.INDENT, "no");
            StringWriter sw = new StringWriter();
            serializer.setOutputWriter(sw);
            serializer.serializeXdmValue(doc);
            assertTrue(sw.toString().contains("<doc><b> </b><i/></doc>"));

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void TestCustomWhitespacePolicyWithXmlSpace() {
        try {
            Processor proc = new Processor(false);
            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setBaseURI(new URI("http://example.com"));
            StringReader xml = new StringReader("<doc><b> <i> </i></b> <i xml:space='preserve'><sup><i> </i></sup> </i> </doc>");
            builder.setWhitespaceStrippingPolicy(WhitespaceStrippingPolicy.makeCustomPolicy(
                    value -> value.getLocalName().equals("doc") || value.getLocalName().equals("i")));

            XdmNode node = builder.build(new StreamSource(xml));

            Serializer ss = proc.newSerializer();
            ss.setOutputProperty(Serializer.Property.METHOD, "xml");
            ss.setOutputProperty(Serializer.Property.INDENT, "no");
            StringWriter writer = new StringWriter();
            ss.setOutputWriter(writer);
            ss.serializeXdmValue(node);
            assertTrue(writer.toString().contains("<doc><b> <i/></b><i xml:space=\"preserve\"><sup><i/></sup></i></doc>"));
        } catch (Exception ex) {
            fail(ex.getMessage());
        }
    }

    @Test
    public void testValidationErrors() {
        List<Invalidity> report = new ArrayList<>();
        try {
            Processor proc = new Processor(true);
            // Validation errors should not go to the configuration's logger
            proc.getUnderlyingConfiguration().setLogger(new StandardLogger() {
                /**
                 * Log a message
                 *
                 * @param message the message to be logged
                 */
                @Override
                public void error(String message) {
                    if (!message.contains("validation errors were reported")) {
                        fail("Unexpected message: " + message);
                    }
                    super.error(message);
                }
            });
            XsdCompiler mgr = proc.newXsdCompiler();
            XsdSchema schema = mgr.compile(new StreamSource(new File(configTest.getDataDir() + "books.xsd")));
            SchemaValidator v = schema.newValidator();

            v.setInvalidityHandler(new InvalidityHandler() {
                @Override
                public void startReporting(String systemId) throws XPathException {

                }

                @Override
                public void reportInvalidity(Invalidity failure) throws XPathException {
                    report.add(failure);
                }

                @Override
                public Sequence endReporting() throws XPathException {
                    return null;
                }
            });

            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setSchemaValidator(v);

            builder.build(new File(configTest.getDataDir() + "books-invalid.xml"));
            fail("unexpected success");

        } catch (SaxonApiException e) {
            assertTrue(e.getMessage().contains("validation errors were reported"));
            assertTrue(report.size() >= 3);
        }
    }


    @Test
    public void testParseFromSax() {
        try {
            Processor proc = new Processor(false);
            StringReader sr = new StringReader("<doc> <a/> </doc>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setWhitespaceStrippingPolicy(WhitespaceStrippingPolicy.ALL);
            StringWriter sw = new StringWriter();
            builder.parse(source, proc.newSerializer(sw));
            assertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?><doc><a/></doc>", sw.toString(), "serialization");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testParseFromFile() {
        try {
            Processor proc = new Processor(false);
            StringWriter sw = new StringWriter();
            File file = new File(configTest.getDataDir() + "books.xml");
            proc.newDocumentBuilder().parse(file, proc.newSerializer(sw));
            assertTrue(sw.toString().startsWith("<?xml version=\"1.0\" encoding=\"UTF-8\"?><?xml-stylesheet"), "serialization");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


}
