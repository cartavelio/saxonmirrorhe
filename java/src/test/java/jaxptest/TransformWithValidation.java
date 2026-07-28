package jaxptest;

import com.saxonica.config.EnterpriseTransformerFactory;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.junit.Testdata;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.Validation;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.*;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.nio.file.Path;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

public class TransformWithValidation {
    private static int errorCount = 0;

    private static final ErrorListener errorListener = new ErrorListener() {
        @Override
        public void warning(TransformerException ex) throws TransformerException {
            System.out.println("Warning: " + ex + "\nLocation: " + ex.getLocator());
            fail("Unexpected warning");
        }

        @Override
        public void fatalError(TransformerException ex) throws TransformerException {
            errorCount++;
            if (!ex.getMessage().contains("contravenes the pattern facet")) {
                System.out.println("Fatal error: " + ex.getMessage());
                fail("Unexpected fatal error");
            }
        }

        @Override
        public void error(TransformerException ex) throws TransformerException {
            System.out.println("Error: " + ex + "\nLocation: " + ex.getLocator());
            errorCount++;
        }
    };

    private final Path dir = Testdata.path("defaultAttTest");

    private final File xslFile = dir.resolve("samples/validation/bookstore/BookStore.xsl").toFile();
    private final File xmlFile = dir.resolve("samples/validation/bookstore/BookStore.xml").toFile();
    private final File outFile = dir.resolve("samples/validation/bookstore/out.xml").toFile();

    @Test
    public void testWithInputValidation() {
        try {
            // Create transformer
            InputSource inputSource = new InputSource(xslFile.toURI().toString());
            Source source = new SAXSource(inputSource);

            TransformerFactoryImpl transformerFactory = new EnterpriseTransformerFactory();
            transformerFactory.setAttribute(FeatureKeys.OPTIMIZATION_LEVEL, "-jc");
            transformerFactory.setErrorListener(errorListener);
            transformerFactory.getConfiguration().setConfigurationProperty(FeatureKeys.SCHEMA_VALIDATION_MODE,
                                                                           Validation.describe(Validation.STRICT));
            Transformer transformer = transformerFactory.newTransformer(source);
            transformer.setErrorListener(errorListener);

            // Create source
            Source xmlSource = new StreamSource(xmlFile);

            // Output stream result
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
                assertEquals(1, errorCount, "errorCount");
            }
        } catch (TransformerConfigurationException e) {
            fail();
        }
    }
}
