package jaxptest;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.config.EnterpriseTransformerFactory;
import com.saxonica.ee.jaxp.SchemaFactoryImpl;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.Validation;
import org.junit.jupiter.api.Test;
import org.xml.sax.*;

import javax.xml.transform.Result;
import javax.xml.transform.Source;
import javax.xml.transform.Transformer;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import javax.xml.validation.Schema;
import javax.xml.validation.SchemaFactory;
import javax.xml.validation.Validator;
import java.io.File;
import java.io.IOException;
import java.io.StringReader;
import java.io.StringWriter;
import java.util.ArrayList;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Tests for JAXP validation interface
 */
public class ValidationTest {


    @Test
    public void testDefaultErrorHandling() {
        try {
            SchemaFactoryImpl schemaFactory = new com.saxonica.ee.jaxp.SchemaFactoryImpl();
            Schema grammar = schemaFactory.newSchema(new File(ConfigTest.DATA_DIR + "books.xsd"));
            Validator validator = grammar.newValidator();
            validator.validate(new StreamSource(new File(ConfigTest.DATA_DIR + "books-invalid.xml")));
            fail("Unexpected success");

        } catch (SAXException e) {
            // OK
        } catch (IOException e) {
            e.printStackTrace();
            fail();
        }
    }

    // Switch to allow running tests with Xerces, for comparison purposes
    private final static boolean USE_XERCES = false;

    private SchemaFactory makeSchemaFactory() {
        if (USE_XERCES) {
            return new org.apache.xerces.jaxp.validation.XMLSchemaFactory();
        } else {
            return new com.saxonica.ee.jaxp.SchemaFactoryImpl();
        }
    }

    @Test
    public void testRecoverableErrorHandling() {
        final List<Integer> counts = new ArrayList<Integer>();
        counts.add(0);
        counts.add(0);
        counts.add(0);
        try {
            SchemaFactory schemaFactory = makeSchemaFactory();
            Schema grammar = schemaFactory.newSchema(new File(ConfigTest.DATA_DIR + "books.xsd"));
            Validator validator = grammar.newValidator();

            validator.setErrorHandler(new ErrorHandler() {
                @Override
                public void warning(SAXParseException exception) throws SAXException {
                    counts.set(0, counts.get(0) + 1);
                }

                @Override
                public void error(SAXParseException exception) throws SAXException {
                    counts.set(1, counts.get(1) + 1);
                }

                @Override
                public void fatalError(SAXParseException exception) throws SAXException {
                    counts.set(2, counts.get(2) + 1);
                }
            });
            validator.validate(new StreamSource(new File(ConfigTest.DATA_DIR + "books-invalid.xml")));
            assertEquals(0, (int) counts.get(0), "warnings");
            assertEquals(3, (int) counts.get(1), "errors");
            assertEquals(0, (int) counts.get(2), "fatals");

        } catch (SAXException e) {
            e.printStackTrace();
            fail();
        } catch (IOException e) {
            e.printStackTrace();
            fail();
        }
    }


    @Test
    public void testInvalidSchema() {
        final List<Integer> counts = new ArrayList<Integer>();
        counts.add(0);
        counts.add(0);
        counts.add(0);
        try {
            SchemaFactory schemaFactory = makeSchemaFactory();
            schemaFactory.setErrorHandler(new ErrorHandler() {
                @Override
                public void warning(SAXParseException exception) throws SAXException {
                    counts.set(0, counts.get(0) + 1);
                }

                @Override
                public void error(SAXParseException exception) throws SAXException {
                    counts.set(1, counts.get(1) + 1);
                }

                @Override
                public void fatalError(SAXParseException exception) throws SAXException {
                    counts.set(2, counts.get(2) + 1);
                }
            });
            Schema grammar = schemaFactory.newSchema(new File(ConfigTest.DATA_DIR + "books-invalid.xsd"));
//            Validator validator = grammar.newValidator();
//
//            validator.setErrorHandler(
//            validator.validate(new StreamSource(new File(ConfigTest.DATA_DIR + "books-invalid.xml")));

            fail("Expected exception to be thrown, though we didn't do so in 9.9");
        } catch (SAXException e) {
            assertEquals(0, (int) counts.get(0), "warnings");
            assertEquals(4, (int) counts.get(1), "errors");
            assertEquals(0, (int) counts.get(2), "fatals");

        }
    }

    @Test
    public void testDefaultAttributeHandling() {
        try {
            File xslFile = new File(ConfigTest.BUGS_DIR + "4338/copy_stylesheet.xsl");
            InputSource inputSource = new InputSource(xslFile.toURI().toString());
            Source source = new SAXSource(inputSource);

            TransformerFactoryImpl transformerFactory = new EnterpriseTransformerFactory();

            transformerFactory.getConfiguration().setConfigurationProperty(
                    FeatureKeys.SCHEMA_VALIDATION_MODE,
                    Validation.describe(Validation.STRICT));

            transformerFactory.setAttribute(net.sf.saxon.lib.FeatureKeys.XSLT_SCHEMA_AWARE, true);

            Transformer transformer = transformerFactory.newTransformer(source);

            // Create XML SAX source
            InputSource inputSource2 = new InputSource(new File(ConfigTest.BUGS_DIR + "4338/defAttr.xml").toURI().toString());
            SAXSource xmlSource = new SAXSource(inputSource2);
            XMLReader saxParser = transformerFactory.getConfiguration().getSourceParser();

            //EXM-11081 Set this feature so that the default attributes from the schema are reported...
            //saxParser.setFeature("http://apache.org/xml/features/validation/schema", true);
            //saxParser.setFeature(Constants.SAX_FEATURE_PREFIX + Constants.VALIDATION_FEATURE, true);

            xmlSource.setXMLReader(saxParser);

            // Output stream result
            StringWriter sw = new StringWriter();
            Result outputTarget = new StreamResult(sw);

            // Run transformation
            transformer.transform(xmlSource, outputTarget);
            System.err.println(sw.toString());
            assertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?><root xmlns:gns1=\"alreadyGenerated\" xmlns:gns2=\"testNS\" xmlns:ns2=\"http://dita.oasis-open.org/architecture/2005/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"main.xsd\" ns2:DITAArchVersion=\"defVal\" gns1:attr1=\"defVal\"/>", sw.toString(), "result");
        } catch (Exception err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    private void validate(String xsd, String xml, int expectedErrors) {
        try {
            EnterpriseConfiguration conf = new EnterpriseConfiguration();
            SchemaFactory schemaFactory = new com.saxonica.ee.jaxp.SchemaFactoryImpl(conf);
            Schema schema = schemaFactory.newSchema(new StreamSource(new StringReader(xsd)));

            Validator validator = schema.newValidator();
            CustomHandler handler = new CustomHandler();
            validator.setErrorHandler(handler);
            validator.validate(new StreamSource(new StringReader(xml)));
            assertEquals(expectedErrors, handler.errorCount());
        } catch (Exception err) {
            fail(err);
        }
    }

    // Following tests are related to bug 5208

    private final static String listOfElements = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>\n"
            + "\n"
            + " <xs:element name='list'>\n"
            + "  <xs:complexType>\n"
            + "   <xs:sequence>\n"
            + "    <xs:element ref='item' maxOccurs='unbounded'/>\n"
            + "   </xs:sequence>\n"
            + "  </xs:complexType>\n"
            + "  <xs:unique name='u'>\n"
            + "   <xs:selector xpath='item'/>\n"
            + "   <xs:field xpath='value'/>\n"
            + "  </xs:unique>\n"
            + " </xs:element>\n"
            + "\n"
            + " <xs:element name='item'>\n"
            + "  <xs:complexType>\n"
            + "   <xs:sequence>\n"
            + "    <xs:element name='value' type='xs:$$$$'/>\n"
            + "   </xs:sequence>\n"
            + "  </xs:complexType>\n"
            + " </xs:element>\n"
            + "    \n"
            + "</xs:schema>";

    private final static String listOfAttributes = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>\n"
            + "\n"
            + " <xs:element name='list'>\n"
            + "  <xs:complexType>\n"
            + "   <xs:sequence>\n"
            + "    <xs:element ref='item' maxOccurs='unbounded'/>\n"
            + "   </xs:sequence>\n"
            + "  </xs:complexType>\n"
            + "  <xs:unique name='u'>\n"
            + "   <xs:selector xpath='item'/>\n"
            + "   <xs:field xpath='@value'/>\n"
            + "  </xs:unique>\n"
            + " </xs:element>\n"
            + "\n"
            + " <xs:element name='item'>\n"
            + "  <xs:complexType>\n"
            + "   <xs:sequence/>\n"
            + "   <xs:attribute name='value' type='xs:$$$$'/>\n"
            + "  </xs:complexType>\n"
            + " </xs:element>\n"
            + "    \n"
            + "</xs:schema>";

    @Test
    public void testUniqueIntsE() {
        String xsd = listOfElements.replace("$$$$", "int");
        validate(xsd, "<list><item><value>1</value></item><item><value>10</value></item><item><value>20</value></item></list>", 0);
        validate(xsd, "<list><item><value>1</value></item><item><value>10</value></item><item><value>1</value></item></list>", 1);
        validate(xsd, "<list><item><value>1</value></item><item><value>Z</value></item><item><value>1</value></item></list>", 2);
    }

    @Test
    public void testUniqueStringsE() {
        String xsd = listOfElements.replace("$$$$", "string");
        validate(xsd, "<list><item><value>X1</value></item><item><value>X10</value></item><item><value>X20</value></item></list>", 0);
        validate(xsd, "<list><item><value>X1</value></item><item><value>X10</value></item><item><value>X1</value></item></list>", 1);
    }

    @Test
    public void testUniqueQNamesE() {
        String xsd = listOfElements.replace("$$$$", "QName");
        validate(xsd, "<list xmlns:X='X' xmlns:Y='X'><item><value>X:A</value></item><item><value>X:B</value></item><item><value>Y:C</value></item></list>", 0);
        validate(xsd, "<list xmlns:X='X' xmlns:Y='X'><item><value>X:A</value></item><item><value>X:B</value></item><item><value>Y:A</value></item></list>", 1);
        validate(xsd, "<list xmlns:X='X' xmlns:Y='X'><item><value>X:A</value></item><item><value>X:22</value></item><item><value>Y:A</value></item></list>", 2);
    }

    @Test
    public void testUniqueIntsA() {
        String xsd = listOfAttributes.replace("$$$$", "int");
        validate(xsd, "<list><item value='1'/><item value='20'/><item value='10'/></list>", 0);
        validate(xsd, "<list><item value='1'/><item value='-20'/><item value='1'/></list>", 1);
        validate(xsd, "<list><item value='1'/><item value='XIII'/><item value='1'/></list>", 2);
    }

    @Test
    public void testUniqueStringsA() {
        String xsd = listOfAttributes.replace("$$$$", "string");
        validate(xsd, "<list><item value='x1'/><item value='x20'/><item value='x10'/></list>", 0);
        validate(xsd, "<list><item value='x1'/><item value='x-20'/><item value='x1'/></list>", 1);
    }

    @Test
    public void testUniqueQNamesA() {
        String xsd = listOfAttributes.replace("$$$$", "QName");
        validate(xsd, "<list xmlns:X='X' xmlns:Y='X'><item value='X:A'/><item value='X:B'/><item value='Y:C'/></list>", 0);
        validate(xsd, "<list xmlns:X='X' xmlns:Y='X'><item value='X:A'/><item value='X:B'/><item value='Y:A'/></list>", 1);
        validate(xsd, "<list xmlns:X='X' xmlns:Y='X'><item value='X:A'/><item value='X:22'/><item value='X:A'/></list>", 2);
    }

    private static class CustomHandler implements ErrorHandler {

        int errors = 0;

        @Override
        public void warning(SAXParseException exception) throws SAXException {
            System.out.println("Warning: " + exception);
        }

        @Override
        public void error(SAXParseException exception) throws SAXException {
            System.out.println("Error: " + exception);
            errors++;
        }

        @Override
        public void fatalError(SAXParseException exception) throws SAXException {
            System.out.println("Fatal error: " + exception);
        }

        public int errorCount() {
            return errors;
        }
    }


}


