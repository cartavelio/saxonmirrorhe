package jaxptest;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.jaxp.SchemaFactoryImpl;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.SchemaURIResolver;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.SchemaManager;
import net.sf.saxon.s9api.SchemaValidator;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.xml.sax.ErrorHandler;
import org.xml.sax.SAXException;
import org.xml.sax.SAXParseException;

import javax.xml.XMLConstants;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import javax.xml.transform.Source;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamSource;
import javax.xml.validation.Schema;
import javax.xml.validation.SchemaFactory;
import javax.xml.validation.Validator;
import java.io.*;
import java.util.ArrayList;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Tests for JAXP schema validation interface
 */
public class SchemaTests {

    @Test
    public void testStreamNotClosed() {
        try {
            InputStream inStream = new BufferedInputStream(new FileInputStream(new
                    File(ConfigTest.DATA_DIR + "books.xml")), 100000);  // size of blah is << 100000

            inStream.mark(99999);

            InputStream isWrapper = new BufferedInputStream(inStream) {
                @Override
                public void close() throws IOException {
                    //no action;
                }
            };

            EnterpriseConfiguration config = new EnterpriseConfiguration();
            SchemaFactory schemaFactory = new com.saxonica.ee.jaxp.SchemaFactoryImpl(config);
            Schema schemaGrammar = schemaFactory.newSchema(
                    new StreamSource[]{new StreamSource(new File(ConfigTest.DATA_DIR + "books.xsd"))});

            Validator schemaValidator = schemaGrammar.newValidator();

            schemaValidator.validate(new StreamSource(isWrapper));

            inStream.reset();

            System.out.println(inStream.read());
        } catch (SAXException | IOException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testSupplyElementNode() {    // bug 2217
        try {
            DocumentBuilderFactory docFactory = DocumentBuilderFactory.newInstance();
            docFactory.setNamespaceAware(true);
            Document doc = docFactory.newDocumentBuilder().parse(new File(ConfigTest.DATA_DIR + "books.xsd"));
            Element top = doc.getDocumentElement();
            DOMSource source = new DOMSource(top);

            EnterpriseConfiguration config = new EnterpriseConfiguration();
            SchemaFactory schemaFactory = new com.saxonica.ee.jaxp.SchemaFactoryImpl(config);
            Schema schemaGrammar = schemaFactory.newSchema(source);

            Validator schemaValidator = schemaGrammar.newValidator();

            schemaValidator.validate(new StreamSource(new File(ConfigTest.DATA_DIR + "books.xml")));

        } catch (SAXException e) {
            e.printStackTrace();
            fail();
        } catch (IOException e) {
            e.printStackTrace();
            fail();
        } catch (ParserConfigurationException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testSupplyDocumentNode() {
        try {
            DocumentBuilderFactory docFactory = DocumentBuilderFactory.newInstance();
            docFactory.setNamespaceAware(true);
            Document doc = docFactory.newDocumentBuilder().parse(new File(ConfigTest.DATA_DIR + "books.xsd"));
            DOMSource source = new DOMSource(doc);

            EnterpriseConfiguration config = new EnterpriseConfiguration();
            SchemaFactory schemaFactory = new com.saxonica.ee.jaxp.SchemaFactoryImpl(config);
            Schema schemaGrammar = schemaFactory.newSchema(source);

            Validator schemaValidator = schemaGrammar.newValidator();

            schemaValidator.validate(new StreamSource(new File(ConfigTest.DATA_DIR + "books.xml")));

        } catch (SAXException e) {
            e.printStackTrace();
            fail();
        } catch (IOException e) {
            e.printStackTrace();
            fail();
        } catch (ParserConfigurationException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testErrorHandler() {
        final List<String> reports = new ArrayList<String>();
        try {
            String in = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\">\n" +
                "   <xs:element name=\"root\">\n" +
                "       <xs:complexType>\n" +
                "           <xs:sequence>\n" +
                "               <xs:element ref=\"elem\"/>\n" +
                "           </xs:sequence>\n" +
                "       </xs:complexType>\n" +
                "   </xs:element>\n" +
                "</xs:schema>";
            SchemaFactoryImpl schemaFactory = new com.saxonica.ee.jaxp.SchemaFactoryImpl();

            schemaFactory.setErrorHandler(new ErrorHandler() {
                @Override
                public void warning(SAXParseException exception) throws SAXException {
                    reports.add("W: " + exception.getMessage());
                }

                @Override
                public void error(SAXParseException exception) throws SAXException {
                    reports.add("E: " + exception.getMessage());
                }

                @Override
                public void fatalError(SAXParseException exception) throws SAXException {
                    reports.add("F: " + exception.getMessage());
                }
            });
            // Load schema. The errors should be reported through the attached error handler.
            Schema schemaGrammar = schemaFactory.newSchema(new StreamSource(new StringReader(in)));
            fail("Unexpected success");

        } catch (SAXException e) {
            assertTrue(reports.size() == 1, "size");
            assertTrue(reports.get(0).startsWith("E:"), "E:");
        }
    }

    @Test
    public void testErrorHandlerIllFormed() {
        final List<String> reports = new ArrayList<String>();
        try {
            String in = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\">\n" +
                    "   <xs:element name=\"root\">\n" +
                    "       <xs:complexType>\n" +
                    "           <xs:sequence>\n" +
                    "               <xs:element ref=\"elem\"/>\n" +
                    "           </xs:sequence>\n" +
                    "       </xs:complexTypical>\n" +
                    "   </xs:element>\n" +
                    "</xs:schema>";
            SchemaFactoryImpl schemaFactory = new com.saxonica.ee.jaxp.SchemaFactoryImpl();

            schemaFactory.setErrorHandler(new ErrorHandler() {
                @Override
                public void warning(SAXParseException exception) throws SAXException {
                    reports.add("W: " + exception.getMessage());
                }

                @Override
                public void error(SAXParseException exception) throws SAXException {
                    reports.add("E: " + exception.getMessage());
                }

                @Override
                public void fatalError(SAXParseException exception) throws SAXException {
                    reports.add("F: " + exception.getMessage());
                }
            });
            // Load schema. The errors should be reported through the attached error handler.
            Schema schemaGrammar = schemaFactory.newSchema(new StreamSource(new StringReader(in)));
            fail("Unexpected success");

        } catch (SAXException e) {
            assertTrue(reports.size() >= 1);
            assertTrue(reports.get(0).startsWith("F:"));
        }
    }

    @Test
    public void testOverrideUsingURN() {

        try {
            final String imp = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" targetNamespace=\"http://example.com/\"/>\n";

            final String in = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\">\n" +
                "   <xs:import namespace='http://example.com/' schemaLocation='urn:p.q.r'/>\n" +
                "   <xs:element name=\"root\">\n" +
                "       <xs:complexType>\n" +
                "           <xs:sequence>\n" +
                "               <xs:element name=\"elem\"/>\n" +
                "           </xs:sequence>\n" +
                "       </xs:complexType>\n" +
                "   </xs:element>\n" +
                "</xs:schema>";

            String over = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\">\n" +
                "   <xs:override schemaLocation='urn:a.b.c'>" +
                "   <xs:element name=\"root\">\n" +
                "       <xs:complexType>\n" +
                "           <xs:sequence>\n" +
                "               <xs:element name=\"mele\"/>\n" +
                "           </xs:sequence>\n" +
                "       </xs:complexType>\n" +
                "   </xs:element>\n" +
                "   </xs:override>\n" +
                "</xs:schema>";

            String data = "<root><mele/></root>";

            Processor proc = new Processor(true);
            SchemaManager mgr = proc.getSchemaManager();
            mgr.setXsdVersion("1.1");
            mgr.setSchemaURIResolver(new SchemaURIResolver() {
                @Override
                public Source[] resolve(String targetNamespace, String baseURI, String[] locations) throws XPathException {
                    if (locations[0].equals("urn:a.b.c")) {
                        return new Source[]{new StreamSource(new StringReader(in), "http://dummy/in")};
                    } else if (locations[0].equals("urn:p.q.r")) {
                        return new Source[]{new StreamSource(new StringReader(imp), "http://dummy/imp")};
                    } else {
                        return new Source[]{};
                    }
                }

                @Override
                public void setConfiguration(Configuration config) {

                }
            });
            mgr.load(new StreamSource(new StringReader(over), "http://dummy/over"));
            SchemaValidator val = mgr.newSchemaValidator();
            val.validate(new StreamSource(new StringReader(data)));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }

    }

    /**
     * It's not clear what was originally intended, but this tests some interesting paths and then fails
     * saying that the schema contained unresolved external references.
     */

    @Test
    public void testOverrideUsingURNinDITA() {

        try {
            String over = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\">\n" +
                "   <xs:override schemaLocation=\"urn:oasis:names:tc:dita:xsd:topicMod.xsd:1.2\"> \n" +
                "<xs:group name=\"prolog.content\"/>" +
                "</xs:override>" +
                "</xs:schema>";

            String catalog = "/Applications/oxygen-2/frameworks/dita/catalog.xml";

            String data = "<root><mele/></root>";

            Processor proc = new Processor(true);
            SchemaManager mgr = proc.getSchemaManager();
            mgr.setXsdVersion("1.1");
            proc.setCatalogFiles(catalog);

            mgr.load(new StreamSource(new StringReader(over), "http://dummy/over"));
            SchemaValidator val = mgr.newSchemaValidator();
            val.validate(new StreamSource(new StringReader(data)));
        } catch (SaxonApiException e) {
            // expected failure
        }

    }

    // Use ACCESS_EXTERNAL_SCHEMA to disallow "file" protocol

    @Test
    public void testValidatorFeatures01() {    // bug 4234
        try {
            DocumentBuilderFactory docFactory = DocumentBuilderFactory.newInstance();
            docFactory.setNamespaceAware(true);
            Document doc = docFactory.newDocumentBuilder().parse(new File(ConfigTest.DATA_DIR + "books-wrapper.xsd"));
            Element top = doc.getDocumentElement();
            DOMSource source = new DOMSource(top);

            EnterpriseConfiguration config = new EnterpriseConfiguration();
            SchemaFactory schemaFactory = new com.saxonica.ee.jaxp.SchemaFactoryImpl(config);
            schemaFactory.setProperty(XMLConstants.ACCESS_EXTERNAL_SCHEMA, "http https");
            Schema schemaGrammar = schemaFactory.newSchema(source);

            Validator schemaValidator = schemaGrammar.newValidator();

            schemaValidator.validate(new StreamSource(new File(ConfigTest.DATA_DIR + "books.xml")));
            fail("undetected failure");
        } catch (SAXException | IOException | ParserConfigurationException e) {
            // failed as expected
            assertTrue(e instanceof SAXParseException);
        }
    }
}


