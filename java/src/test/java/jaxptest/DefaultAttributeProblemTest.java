package jaxptest;


import net.sf.saxon.junit.Testdata;
import org.junit.jupiter.api.Test;

import javax.xml.transform.ErrorListener;
import javax.xml.transform.TransformerException;
import java.io.File;
import java.net.URI;
import java.nio.file.Path;

//import org.apache.xerces.parsers.SAXParser;

public class DefaultAttributeProblemTest {
    private static final ErrorListener errorListener = new ErrorListener() {
        @Override
        public void warning(TransformerException ex) {
            System.out.println("Warning: " + ex + "\nLocation: " + ex.getLocator());
        }

        @Override
        public void fatalError(TransformerException ex) {
            System.out.println("Error: " + ex.getMessage());
        }

        @Override
        public void error(TransformerException ex) {
            System.out.println("Error: " + ex + "\nLocation: " + ex.getLocator());
        }
    };

    private final Path dir = Testdata.path("defaultAttTest");
    private final File xslFile = dir.resolve("samples/dita_xsd_based/copy_stylesheet.xsl").toFile();
    private final URI xmlUri = dir.resolve("samples/dita_xsd_based/defAttr.xml").toUri();
    private final File outFile = dir.resolve("samples/dita_xsd_based/out.xml").toFile();

    @Test
    public void testDummyTest() {
        // to avoid the "no tests in module" warning
    }

//    public void testDefaultAttributes() {
//        try {
//            // Create transformer
//            File xslFile = new File(xslPath);
//            InputSource inputSource = new InputSource(xslFile.toURI().toString());
//            Source source = new SAXSource(inputSource);
//
//            TransformerFactoryImpl transformerFactory = new EnterpriseTransformerFactory();
//            transformerFactory.setAttribute(FeatureKeys.OPTIMIZATION_LEVEL, "-jc");
//            transformerFactory.setErrorListener(errorListener);
//
//            transformerFactory.getConfiguration().setConfigurationProperty(
//                    FeatureKeys.SCHEMA_VALIDATION_MODE,
//                    Validation.toString(Validation.STRICT));
//
//            transformerFactory.setAttribute(FeatureKeys.XSLT_SCHEMA_AWARE, true);
//
//            Transformer transformer = transformerFactory.newTransformer(source);
//            transformer.setErrorListener(errorListener);
//
//            // Create XML SAX source
//            SAXSource xmlSource = new SAXSource(new InputSource(xmlUri.toString()));
//            SAXParser saxParser = new SAXParser();
//
//            //EXM-11081 Set this feature so that the default attributes from the schema are reported...
//            saxParser.setFeature("http://apache.org/xml/features/validation/schema", true);
//
//            xmlSource.setXMLReader(saxParser);
//
//            // Output stream result
//            if (outFile.exists()) {
//                //noinspection ResultOfMethodCallIgnored
//                outFile.delete();
//            }
//            Result outputTarget = new StreamResult(outFile);
//
//            // Run transformation
//            System.out.println("Run transformation...");
//            try {
//                transformer.transform(xmlSource, outputTarget);
//                System.out.println("Transformation done.");
//            } catch (Exception e) {
//                System.out.println("Transformation failed: " + e);
//            }
//        } catch (TransformerConfigurationException | SAXNotRecognizedException | SAXNotSupportedException e) {
//            e.printStackTrace();
//            fail();
//        }
//    }
}
