package jaxptest;

import com.saxonica.config.EnterpriseTransformerFactory;
import junit.framework.TestCase;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.Validation;
import org.xml.sax.InputSource;

import javax.xml.transform.*;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.File;

public class TransformWithValidation extends TestCase {

    private static int errorCount = 0;

    private static ErrorListener errorListener = new ErrorListener() {
        public void warning(TransformerException ex) throws TransformerException {
            System.out.println("Warning: " + ex + "\nLocation: " + ex.getLocator());
            fail("Unexpected warning");
        }

        public void fatalError(TransformerException ex) throws TransformerException {
            if (!ex.getMessage().contains("One validation error was reported")) {
                System.out.println("Fatal error: " + ex.getMessage());
                fail("Unexpected fatal error");
            }
        }

        public void error(TransformerException ex) throws TransformerException {
            System.out.println("Error: " + ex + "\nLocation: " + ex.getLocator());
            errorCount++;
        }
    };

    private static String dir = ConfigTest.DATA_DIR + "defaultAttTest/";

    private static String xslPath = dir+"samples/validation/bookstore/BookStore.xsl";
    private static String xmlPath = dir+"samples/validation/bookstore/BookStore.xml";
    private static String outPath = dir+"samples/validation/bookstore/out.xml";

    public void testWithInputValidation() {
        try {
            // Create transformer
            File xslFile = new File(xslPath);
            InputSource inputSource = new InputSource(xslFile.toURI().toString());
            Source source = new SAXSource(inputSource);

            TransformerFactoryImpl transformerFactory = new EnterpriseTransformerFactory();
            transformerFactory.setAttribute(FeatureKeys.OPTIMIZATION_LEVEL, "-jc");
            transformerFactory.setErrorListener(errorListener);
            transformerFactory.getConfiguration().setConfigurationProperty(FeatureKeys.SCHEMA_VALIDATION_MODE,
                                                                           Validation.toString(Validation.STRICT));
            Transformer transformer = transformerFactory.newTransformer(source);
            transformer.setErrorListener(errorListener);

            // Create XML SAX source
            Source xmlSource = new StreamSource(new File(xmlPath));

            // Output stream result
            File outFile = new File(outPath);
            if (outFile.exists()) {
                outFile.delete();
            }
            Result outputTarget = new StreamResult(outFile);

            // Run transformation
            System.out.println("Run transformation...");
            try {
                transformer.transform(xmlSource, outputTarget);
                System.out.println("Transformation done.");
                fail("Unexpected success");
            } catch (Exception e) {
                System.out.println("Failed as expected: " + e);
                assertEquals("errorCount", 1, errorCount);
            }
        } catch (TransformerConfigurationException e) {
            fail();
        }
    }
}
