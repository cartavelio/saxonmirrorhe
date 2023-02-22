package jaxptest;

import com.saxonica.config.EnterpriseTransformerFactory;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.om.TreeModel;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;
import org.xml.sax.SAXNotRecognizedException;
import org.xml.sax.SAXNotSupportedException;
import org.xml.sax.XMLReader;

import javax.xml.parsers.SAXParserFactory;
import javax.xml.transform.*;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.sax.SAXTransformerFactory;
import javax.xml.transform.sax.TemplatesHandler;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;
import java.io.StringWriter;

import static net.sf.saxon.s9api.streams.Steps.child;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 *
 */
public class ParserTest {

    @Test
    public void testValidationError() throws Exception {
        try {
            TransformerFactory factory = new net.sf.saxon.TransformerFactoryImpl();
            System.out.println("Using " + factory.getClass().getName());
            factory.setAttribute(FeatureKeys.VERSION_WARNING, Boolean.FALSE);
            factory.setAttribute(FeatureKeys.DTD_VALIDATION, Boolean.TRUE);
            factory.setAttribute(FeatureKeys.DTD_VALIDATION_RECOVERABLE, Boolean.FALSE);
            Transformer trans = factory.newTransformer(new StreamSource(ConfigTest.DATA_DIR + "bills.xsl"));
            trans.transform(new StreamSource(ConfigTest.DATA_DIR + "bills.xml"), new StreamResult(System.out));
            fail("Transformation should fail");
        } catch (TransformerException err) {
            // failure expected
        }
    }

    @Test
    public void testValidationErrorRecovered() throws Exception {
        try {
            TransformerFactory factory = new net.sf.saxon.TransformerFactoryImpl();
            System.out.println("Using " + factory.getClass().getName());
            factory.setAttribute(FeatureKeys.VERSION_WARNING, Boolean.FALSE);
            factory.setAttribute(FeatureKeys.DTD_VALIDATION, Boolean.TRUE);
            factory.setAttribute(FeatureKeys.DTD_VALIDATION_RECOVERABLE, Boolean.TRUE);
            Transformer trans = factory.newTransformer(new StreamSource(ConfigTest.DATA_DIR + "bills.xsl"));
            trans.transform(new StreamSource(ConfigTest.DATA_DIR + "bills.xml"), new StreamResult(System.out));
        } catch (XPathException err) {
            fail("Transformation should not fail");
        }
    }

    @Test
    public void testTransformerHandler() throws Exception {

        try {
            String template = ConfigTest.DATA_DIR + "bills.xsl";

            SAXParserFactory factory = SAXParserFactory.newInstance();
            factory.setNamespaceAware(true);
            factory.setValidating(false);
            XMLReader reader = factory.newSAXParser().getXMLReader();

            SAXTransformerFactory transformerFactory = new TransformerFactoryImpl();
            //transformerFactory.setAttribute(FeatureKeys.XSLT_VERSION, "2.0");
            TemplatesHandler templatesHandler = transformerFactory.newTemplatesHandler();
            reader.setContentHandler(templatesHandler);
            reader.parse(new File(template).toURI().toASCIIString());

            Templates templates = templatesHandler.getTemplates();
//            Templates templates = transformerFactory.newTemplates(new StreamSource(new File(template)));


        } catch (Exception err) {
            err.printStackTrace();
            fail();
        }


    }

    @Test
    public void testSetParserProperties() {
        try {
            String xsl = ConfigTest.DATA_DIR + "bills.xsl";
            String xml = ConfigTest.DATA_DIR + "bills.xml";
            TransformerFactory f = new TransformerFactoryImpl();
            f.setAttribute(FeatureKeys.XSLT_VERSION, "2.0");
            f.setAttribute(FeatureKeys.XML_PARSER_PROPERTY + "http://apache.org/xml/properties/input-buffer-size", 1000);
            Templates templates = f.newTemplates(new StreamSource(new File(xsl)));
            Transformer transformer = templates.newTransformer();
            transformer.transform(new StreamSource(new File(xml)), new StreamResult(System.out));
        } catch (TransformerException err) {
            fail();
        }
    }

    @Test
    public void testExpandDefaultAttributes() throws Exception {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.VERSION_WARNING, Boolean.FALSE);
            proc.setConfigurationProperty(Feature.DTD_VALIDATION, Boolean.TRUE);
            proc.setConfigurationProperty(Feature.DTD_VALIDATION_RECOVERABLE, Boolean.FALSE);
            XQueryEvaluator q = proc.newXQueryCompiler().compile("string-join(//WEIGHT/@EXTRA, ' ')").load();
            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setDTDValidation(true);
            XdmNode source = builder.build(new File(ConfigTest.DATA_DIR + "books-with-dtd.xml"));
            q.setContextItem(source);
            XdmValue r = q.evaluateSingle();
            assertEquals("Extra Extra Extra Extra Extra Extra", r.toString());
        } catch (SaxonApiException err) {
            fail(err.toString());
        }
    }

    @Test
    public void testSuppressDefaultAttributes() throws Exception {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.VERSION_WARNING, false);
            proc.setConfigurationProperty(Feature.DTD_VALIDATION, true);
            proc.setConfigurationProperty(Feature.DTD_VALIDATION_RECOVERABLE, false);
            proc.setConfigurationProperty(Feature.EXPAND_ATTRIBUTE_DEFAULTS, false);
            XQueryEvaluator q = proc.newXQueryCompiler().compile("string-join(//WEIGHT/@EXTRA, ' ')").load();
            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setDTDValidation(true);
            XdmNode source = builder.build(new File(ConfigTest.DATA_DIR + "books-with-dtd.xml"));
            q.setContextItem(source);
            XdmValue r = q.evaluateSingle();
            assertEquals("", r.toString());
        } catch (SaxonApiException err) {
            fail(err.toString());
        }
    }

    @Test
    public void testMarkDefaultAttributes() throws Exception {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.VERSION_WARNING, false);
            proc.setConfigurationProperty(Feature.DTD_VALIDATION, true);
            proc.setConfigurationProperty(Feature.DTD_VALIDATION_RECOVERABLE, false);
            proc.setConfigurationProperty(Feature.MARK_DEFAULTED_ATTRIBUTES, true);

            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setDTDValidation(true);
            XdmNode source = builder.build(new File(ConfigTest.DATA_DIR + "books-with-dtd.xml"));

            XQueryEvaluator q1 = proc.newXQueryCompiler().compile(
                "string-join(//WEIGHT/@EXTRA!Q{http://saxon.sf.net/}is-defaulted(.)!string(.), ' ')").load();
            q1.setContextItem(source);
            XdmValue r1 = q1.evaluateSingle();
            assertEquals("true true true true true true", r1.toString());

            XQueryEvaluator q2 = proc.newXQueryCompiler().compile(
                "string-join(//WEIGHT/@UNIT!Q{http://saxon.sf.net/}is-defaulted(.)!string(.), ' ')").load();
            q2.setContextItem(source);
            XdmValue r2 = q2.evaluateSingle();
            assertEquals("false false false false false false", r2.toString());
        } catch (SaxonApiException err) {
            fail(err.toString());
        }
    }

    @Test
    public void testXercesSchemaDefaultedAttributes() {

        // bug 3068

        try {
            // Create transformer
            TransformerFactoryImpl transformerFactory = new EnterpriseTransformerFactory();
            Transformer transformer = transformerFactory.newTransformer();

            // Create XML SAX source
            SAXSource xmlSource = new SAXSource(new InputSource(new File(ConfigTest.DATA_DIR + "bug3068/defAttr.xml").toURI().toString()));
            org.apache.xerces.parsers.SAXParser saxParser = new org.apache.xerces.parsers.SAXParser();

            //EXM-11081 Set this feature so that the default attributes from the schema are reported...
            saxParser.setFeature("http://apache.org/xml/features/validation/schema", true);

            xmlSource.setXMLReader(saxParser);

            // Output stream result
            StringWriter out = new StringWriter();
            Result outputTarget = new StreamResult(out);

            // Run transformation
            System.out.println("Run transformation...");
            try {
                transformer.transform(xmlSource, outputTarget);
                System.out.println("Transformation done.");
                // Check the result is well-formed
                org.apache.xerces.parsers.SAXParser sp2 = new org.apache.xerces.parsers.SAXParser();
                sp2.parse(new InputSource(new StringReader(out.toString())));
                //System.out.println(out.toString());
            } catch (Exception e) {
                e.printStackTrace();
                fail(e.getMessage());
            }
//        } catch (TransformerConfigurationException e) {
//            fail();
        } catch (SAXNotRecognizedException e) {
            fail();
        } catch (SAXNotSupportedException e) {
            fail();
        }
    }

    @Test
    public void testXercesSchemaDefaultedAttributesXmlSpace() {

        // bug 3068

        try {
            // Create transformer
            TransformerFactoryImpl transformerFactory = new EnterpriseTransformerFactory();
            Transformer transformer = transformerFactory.newTransformer();

            // Create XML SAX source
            SAXSource xmlSource = new SAXSource(new InputSource(new File(ConfigTest.DATA_DIR + "bug3068/defAttr2.xml").toURI().toString()));
            org.apache.xerces.parsers.SAXParser saxParser = new org.apache.xerces.parsers.SAXParser();

            //EXM-11081 Set this feature so that the default attributes from the schema are reported...
            saxParser.setFeature("http://apache.org/xml/features/validation/schema", true);

            xmlSource.setXMLReader(saxParser);

            // Output stream result
            StringWriter out = new StringWriter();
            Result outputTarget = new StreamResult(out);

            // Run transformation
            System.out.println("Run transformation...");
            try {
                transformer.transform(xmlSource, outputTarget);
                System.out.println("Transformation done.");
                // Check the result is well-formed
                org.apache.xerces.parsers.SAXParser sp2 = new org.apache.xerces.parsers.SAXParser();
                sp2.parse(new InputSource(new StringReader(out.toString())));
                //System.out.println(out.toString());
            } catch (Exception e) {
                e.printStackTrace();
                fail(e.getMessage());
            }
        } catch (SAXNotRecognizedException | SAXNotSupportedException e) {
            fail();
        }
    }

    private final String lnTestXml = ""
            + "<a b='2'>\n"
            + "  <!-- comment -->\n"
            + "  <?pi?>\n"
            + "  <x>text</x>\n"
            + "  <y>multi-\n"
            + "     line</y>"
            + "</a>";

    private void expectLocation(XPathCompiler xpc, XdmNode doc, String path, int line, int column) throws SaxonApiException {
        XdmNode found = (XdmNode)xpc.evaluateSingle(path, doc);
        assertEquals(line, found.getLineNumber(), path + " - line");
        assertEquals(column, found.getColumnNumber(), path + " - column");
    }

    @Test
    public void testLineNumberingTinyTree() {
        try {
            Processor proc = new Processor(true);
            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setLineNumbering(true);
            builder.setTreeModel(TreeModel.TINY_TREE);
            XdmNode doc = builder.build(new StreamSource(new StringReader(lnTestXml), "urn:dummy"));
            XPathCompiler xpc = proc.newXPathCompiler();
            expectLocation(xpc, doc,"/a", 1, 10);
            expectLocation(xpc, doc, "/a/comment()", 2, 19);
            expectLocation(xpc, doc, "/a/processing-instruction()", 3, 9);
            expectLocation(xpc, doc, "/a/x", 4, 6);
            expectLocation(xpc, doc, "/a/x/text()", 4, 6);
            expectLocation(xpc, doc, "/a/y/text()", 5, 6);
        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
    }

    @Test
    public void testLineNumberingLinkedTree() {
        try {
            Processor proc = new Processor(true);
            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setLineNumbering(true);
            builder.setTreeModel(TreeModel.LINKED_TREE);
            XdmNode doc = builder.build(new StreamSource(new StringReader(lnTestXml), "urn:dummy"));
            XPathCompiler xpc = proc.newXPathCompiler();
            expectLocation(xpc, doc, "/a", 1, 10);
            expectLocation(xpc, doc, "/a/comment()", 2, 19);
            expectLocation(xpc, doc, "/a/processing-instruction()", 3, 9);
            expectLocation(xpc, doc, "/a/x", 4, 6);
            expectLocation(xpc, doc, "/a/x/text()", 4, 6);
        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
    }

    @Test
    public void testParseXmlFragment() {
        // Bug 4127
        try {
            Processor proc = new Processor(true);
            //proc.setConfigurationProperty(Feature.SOURCE_PARSER_CLASS, "org.apache.xml.resolver.tools.ResolvingXMLReader");
            XQueryCompiler compiler = proc.newXQueryCompiler();
            XQueryExecutable exec = compiler.compile("parse-xml-fragment('<a/><b/>')");
            XdmValue value = exec.load().evaluate();
            assertEquals(1, value.size());
            assertTrue(value instanceof XdmNode);
            assertEquals(XdmNodeKind.DOCUMENT, ((XdmNode)value).getNodeKind());
            assertEquals(2, value.select(child()).count());
        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
    }



}

