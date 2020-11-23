package jaxptest;

import com.saxonica.ee.jaxp.SchemaFactoryImpl;
import junit.framework.TestCase;
import org.xml.sax.ErrorHandler;
import org.xml.sax.SAXException;
import org.xml.sax.SAXParseException;

import javax.xml.transform.stream.StreamSource;
import javax.xml.validation.Schema;
import javax.xml.validation.SchemaFactory;
import javax.xml.validation.Validator;
import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

/**
 * Tests for JAXP validation interface
 */
public class ValidationTest extends TestCase {


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
                public void warning(SAXParseException exception) throws SAXException {
                    counts.set(0, counts.get(0) + 1);
                }

                public void error(SAXParseException exception) throws SAXException {
                    counts.set(1, counts.get(1) + 1);
                }

                public void fatalError(SAXParseException exception) throws SAXException {
                    counts.set(2, counts.get(2) + 1);
                }
            });
            validator.validate(new StreamSource(new File(ConfigTest.DATA_DIR + "books-invalid.xml")));
            assertEquals("warnings", 0, (int) counts.get(0));
            assertEquals("errors", 3, (int) counts.get(1));
            assertEquals("fatals", 0, (int) counts.get(2));

        } catch (SAXException e) {
            e.printStackTrace();
            fail();
        } catch (IOException e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testInvalidSchema() {
        final List<Integer> counts = new ArrayList<Integer>();
        counts.add(0);
        counts.add(0);
        counts.add(0);
        try {
            SchemaFactory schemaFactory = makeSchemaFactory();
            schemaFactory.setErrorHandler(new ErrorHandler() {
                public void warning(SAXParseException exception) throws SAXException {
                    counts.set(0, counts.get(0) + 1);
                }

                public void error(SAXParseException exception) throws SAXException {
                    counts.set(1, counts.get(1) + 1);
                }

                public void fatalError(SAXParseException exception) throws SAXException {
                    counts.set(2, counts.get(2) + 1);
                }
            });
            Schema grammar = schemaFactory.newSchema(new File(ConfigTest.DATA_DIR + "books-invalid.xsd"));
//            Validator validator = grammar.newValidator();
//
//            validator.setErrorHandler(
//            validator.validate(new StreamSource(new File(ConfigTest.DATA_DIR + "books-invalid.xml")));
            assertEquals("warnings", 0, (int) counts.get(0));
            assertEquals("errors", 3, (int) counts.get(1));
            assertEquals("fatals", 0, (int) counts.get(2));

        } catch (SAXException e) {
            e.printStackTrace();
            fail();
        }
    }


}


