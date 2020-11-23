package s9apitest;

import com.saxonica.config.EnterpriseConfiguration;
import junit.framework.TestCase;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SchemaManager;
import net.sf.saxon.s9api.SchemaValidator;

import javax.xml.transform.ErrorListener;
import javax.xml.transform.TransformerException;
import javax.xml.transform.stream.StreamSource;


public class TestErrorListener extends TestCase {

    /**
     * Test that warnings from compiling a schema referenced by xsI:schemaLocation go to the right
     * ErrorListener; also test that an exception thrown by the ErrorListener prevents further processing
     */
    public static void testWarnings() {
        try {
            EnterpriseConfiguration configuration = new EnterpriseConfiguration();
            MyErrorListener configListener = new MyErrorListener();
            configuration.setErrorListener(configListener);
            Processor processor = new Processor(configuration);

            // http://www.saxonica.com/documentation/index.html#!javadoc/net.sf.saxon.s9api/SchemaManager
            SchemaManager schemaManager = processor.getSchemaManager();
            MyErrorListener listener = new MyErrorListener();
            schemaManager.setErrorListener(listener);
            schemaManager.load(new StreamSource("/Users/mike/bugs/2019/4099-abbink/SaxonTester.xsd"));

            SchemaValidator schemaValidator = schemaManager.newSchemaValidator();
            try {
                schemaValidator.setErrorListener(new MyErrorListener());
                schemaValidator.validate(new StreamSource("/Users/mike/bugs/2019/4099-abbink/SaxonTester.xml"));
                assertEquals("warning-count1", listener.warnings, 2);
                assertEquals("warning-count2", configListener.warnings, 0);
            } catch (Exception e) {
            }
            System.out.println("=============");
            System.out.println("");
        } catch (Exception e) {
            System.out.println(String.format("EXCEPTION %s %s", e.getClass().getName(), e.getMessage()));
        }
    }

    private static class MyErrorListener implements ErrorListener {
        int warnings = 0;
        int errors = 0;
        public void error(TransformerException e) throws TransformerException {
            assertEquals("onlyOneError", 0, errors);
            errors++;
            System.out.println(String.format("error - %s", e.getMessage()));
            System.out.println("I WILL THROW TransformerException");
            throw new TransformerException("STOP IT");
        }

        public void fatalError(TransformerException e) throws TransformerException {
            System.out.println(String.format("fatalError - %s", e.getMessage()));
        }

        public void warning(TransformerException e) throws TransformerException {
            System.out.println(String.format("my warning - %s", e.getMessage()));
            warnings++;
        }
    }
}