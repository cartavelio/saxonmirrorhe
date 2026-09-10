package nativetests;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.PreparedSchema;
import jaxptest.ConfigTest;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.Sender;
import net.sf.saxon.lib.ErrorReporter;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.s9api.XmlProcessingError;
import net.sf.saxon.serialize.SerializationProperties;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;
import java.io.StringWriter;
import java.util.Properties;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Test options on schema validation interfaces
 */
public class ValidationTest {
    private final ConfigTest configTest = new ConfigTest();

    private class LocalErrorListener implements ErrorReporter {
        public int warnings = 0;
        public int errors = 0;

        @Override
        public void report(XmlProcessingError error) {
            if (error.isWarning()) {
                warnings++;
            } else {
                System.err.println(error.getMessage());
                errors++;
            }
        }
    }

    /**
     * Fail on first validation error
     */

    @Test
    public void testFatalValidationError() {
        LocalErrorListener listener = new LocalErrorListener();
        try {
            EnterpriseConfiguration config = new EnterpriseConfiguration();
            PreparedSchema schema = new PreparedSchema(
                    config,
                    new StreamSource(new File(configTest.getDataDir() + "books.xsd")),
                    listener);
            StringWriter sw = new StringWriter();
            Receiver out = config.getSerializerFactory().getReceiver(new StreamResult(sw));
            ParseOptions options = new ParseOptions()
                    .withSchema(schema)
                    .withSchemaValidationMode(Validation.STRICT)
                    .withValidationErrorLimit(1)
                    .withErrorReporter(listener);
            Receiver val = schema.getDocumentValidator(out, "", options, null);
            val.setPipelineConfiguration(out.getPipelineConfiguration());
            String badBooks = "<BOOKLIST><BOOKS/><CATEGORIES><CATEGORY/></CATEGORIES></BOOKLIST>";
            Sender.send(new StreamSource(new StringReader(badBooks)), val, null);
            fail("Validation error undetected");
        } catch (Exception e) {
            assertEquals(0, listener.warnings, "warnings");
            assertEquals(1, listener.errors, "errors");
        }
    }

    /**
     * Report all validation errors
     */

    @Test
    public void testNonFatalValidationError() {
        LocalErrorListener listener = new LocalErrorListener();
        String badBooks = "<BOOKLIST><BOOKS/><CATEGORIES><CATEGORY/></CATEGORIES></BOOKLIST>";
        StringWriter sw = new StringWriter();
        try {
            EnterpriseConfiguration config = new EnterpriseConfiguration();
            config.setConfigurationProperty(FeatureKeys.VALIDATION_WARNINGS, true);
            PreparedSchema schema = new PreparedSchema(
                    config,
                    new StreamSource(new File(configTest.getDataDir() + "books.xsd")),
                    listener);
            SerializationProperties props = new SerializationProperties();
            props.setProperty("omit-xml-declaration", "yes");
            Receiver out = config.getSerializerFactory().getReceiver(new StreamResult(sw), props);
            ParseOptions options = new ParseOptions()
                    .withSchema(schema)
                    .withSchemaValidationMode(Validation.STRICT)
                    .withErrorReporter(listener);
            Receiver val = schema.getDocumentValidator(out, "", options, null);
            val.setPipelineConfiguration(out.getPipelineConfiguration());
            Sender.send(new StreamSource(new StringReader(badBooks)), val, null);
            assertEquals(0, listener.warnings, "warnings");
            assertEquals(4, listener.errors, "errors");
            assertEquals(badBooks, sw.toString(), "outputOK");
        } catch (Exception e) {
            fail("Validation error causes exception");
        }
    }

    /**
     * Report all validation errors and add error messages as comments into the result file
     */

    @Test
    public void testNonFatalValidationErrorWithComments() {
        LocalErrorListener listener = new LocalErrorListener();
        String badBooks = "<BOOKLIST><BOOKS/><CATEGORIES><CATEGORY/></CATEGORIES></BOOKLIST>";
        StringWriter sw = new StringWriter();
        try {
            EnterpriseConfiguration config = new EnterpriseConfiguration();
            config.setConfigurationProperty(FeatureKeys.VALIDATION_WARNINGS, true);
            config.setConfigurationProperty(FeatureKeys.VALIDATION_COMMENTS, true);
            PreparedSchema schema = new PreparedSchema(
                    config,
                    new StreamSource(new File(configTest.getDataDir() + "books.xsd")),
                    listener);
            Properties props = new Properties();
            props.setProperty("omit-xml-declaration", "yes");
            Receiver out = config.getSerializerFactory().getReceiver(new StreamResult(sw), new SerializationProperties(props));
            ParseOptions options = new ParseOptions()
                    .withSchema(schema)
                    .withErrorReporter(listener)
                    .withSchemaValidationMode(Validation.STRICT);
            Receiver val = schema.getDocumentValidator(out, "", options, null);
            val.setPipelineConfiguration(out.getPipelineConfiguration());
            Sender.send(new StreamSource(new StringReader(badBooks)), val, null);
            assertEquals(0, listener.warnings, "warnings");
            assertEquals(4, listener.errors, "errors");
            assertTrue(sw.toString().contains("<!--"));
            assertTrue(sw.toString().contains("VALIDATION ERROR"));
        } catch (Exception e) {
            fail("Validation error causes exception");
        }
    }


}

