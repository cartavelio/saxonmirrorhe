package nativetests;

import com.saxonica.config.EnterpriseConfiguration;
import jaxptest.ConfigTest;
import junit.framework.TestCase;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.Sender;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.serialize.SerializationProperties;

import javax.xml.transform.ErrorListener;
import javax.xml.transform.TransformerException;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;
import java.io.StringWriter;
import java.util.Properties;

/**
 * Test options on schema validation interfaces
 */
public class ValidationTest extends TestCase {

    private class LocalErrorListener implements ErrorListener {
        public int warnings = 0;
        public int errors = 0;
        public int fatalErrors = 0;

        public void warning(TransformerException exception) throws TransformerException {
            warnings++;
        }

        public void error(TransformerException exception) throws TransformerException {
            System.err.println(exception.getMessage());
            errors++;
        }

        public void fatalError(TransformerException exception) throws TransformerException {
            //System.err.println(exception.getMessage());
            fatalErrors++;
        }
    }

    /**
     * Fail on first validation error
     */

    public void testFatalValidationError() {
        LocalErrorListener listener = new LocalErrorListener();
        try {
            EnterpriseConfiguration config = new EnterpriseConfiguration();
            config.addSchemaSource(new StreamSource(new File(ConfigTest.DATA_DIR + "books.xsd")), listener);
            StringWriter sw = new StringWriter();
            Receiver out = config.getSerializerFactory().getReceiver(new StreamResult(sw));
            ParseOptions options = new ParseOptions();
            options.setSchemaValidationMode(Validation.STRICT);
            options.setValidationErrorLimit(1);
            options.setErrorListener(listener);
            Receiver val = config.getDocumentValidator(out, "", options, null);
            val.setPipelineConfiguration(out.getPipelineConfiguration());
            String badBooks = "<BOOKLIST><BOOKS/><CATEGORIES><CATEGORY/></CATEGORIES></BOOKLIST>";
            Sender.send(new StreamSource(new StringReader(badBooks)), val, null);
            fail("Validation error undetected");
        } catch (Exception e) {
            assertTrue("warnings", listener.warnings == 0);
            assertTrue("errors", listener.errors == 1);
            assertTrue("fatalErrors", listener.fatalErrors == 0);
        }
    }

    /**
     * Report all validation errors
     */

    public void testNonFatalValidationError() {
        LocalErrorListener listener = new LocalErrorListener();
        String badBooks = "<BOOKLIST><BOOKS/><CATEGORIES><CATEGORY/></CATEGORIES></BOOKLIST>";
        StringWriter sw = new StringWriter();
        try {
            EnterpriseConfiguration config = new EnterpriseConfiguration();
            config.setConfigurationProperty(FeatureKeys.VALIDATION_WARNINGS, true);
            config.addSchemaSource(new StreamSource(new File(ConfigTest.DATA_DIR + "books.xsd")), listener);
            SerializationProperties props = new SerializationProperties();
            props.setProperty("omit-xml-declaration", "yes");
            Receiver out = config.getSerializerFactory().getReceiver(new StreamResult(sw), props);
            ParseOptions options = new ParseOptions();
            options.setSchemaValidationMode(Validation.STRICT);
            options.setErrorListener(listener);
            Receiver val = config.getDocumentValidator(out, "", options, null);
            val.setPipelineConfiguration(out.getPipelineConfiguration());
            Sender.send(new StreamSource(new StringReader(badBooks)), val, null);
            assertTrue("warnings", listener.warnings == 0);
            assertTrue("errors", listener.errors == 4);
            assertTrue("fatalErrors", listener.fatalErrors == 0);
            assertEquals("outputOK", sw.toString(), badBooks);
        } catch (Exception e) {
            fail("Validation error causes exception");
        }
    }

    /**
     * Report all validation errors and add error messages as comments into the result file
     */

    public void testNonFatalValidationErrorWithComments() {
        LocalErrorListener listener = new LocalErrorListener();
        String badBooks = "<BOOKLIST><BOOKS/><CATEGORIES><CATEGORY/></CATEGORIES></BOOKLIST>";
        StringWriter sw = new StringWriter();
        try {
            EnterpriseConfiguration config = new EnterpriseConfiguration();
            config.setConfigurationProperty(FeatureKeys.VALIDATION_WARNINGS, true);
            config.setConfigurationProperty(FeatureKeys.VALIDATION_COMMENTS, true);
            config.addSchemaSource(new StreamSource(new File(ConfigTest.DATA_DIR + "books.xsd")), listener);
            Properties props = new Properties();
            props.setProperty("omit-xml-declaration", "yes");
            Receiver out = config.getSerializerFactory().getReceiver(new StreamResult(sw), new SerializationProperties(props));
            ParseOptions options = new ParseOptions();
            options.setErrorListener(listener);
            options.setSchemaValidationMode(Validation.STRICT);
            Receiver val = config.getDocumentValidator(out, "", options, null);
            val.setPipelineConfiguration(out.getPipelineConfiguration());
            Sender.send(new StreamSource(new StringReader(badBooks)), val, null);
            assertTrue("warnings", listener.warnings == 0);
            assertTrue("errors", listener.errors == 4);
            assertTrue("fatalErrors", listener.fatalErrors == 0);
            assertTrue(sw.toString().contains("<!--"));
            assertTrue(sw.toString().contains("VALIDATION ERROR"));
        } catch (Exception e) {
            fail("Validation error causes exception");
        }
    }


}

//
// The contents of this file are subject to the Mozilla Public License Version
// 1.0 (the "License");
// you may not use this file except in compliance with the License. You may
// obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations
// under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Michael Kay,
//
// Portions created by (your name) are Copyright (C) (your legal entity). All
// Rights Reserved.
//
// Contributor(s): none.
//



