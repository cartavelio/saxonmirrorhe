package jaxptest;


import org.junit.jupiter.api.Test;

import javax.xml.transform.ErrorListener;
import javax.xml.transform.TransformerException;

//import org.apache.xerces.parsers.SAXParser;

public class DefaultAttributeProblemTest {

    private static ErrorListener errorListener = new ErrorListener() {
        public void warning(TransformerException ex) {
            System.out.println("Warning: " + ex + "\nLocation: " + ex.getLocator());
        }

        public void fatalError(TransformerException ex) {
            System.out.println("Error: " + ex.getMessage());
        }

        public void error(TransformerException ex) {
            System.out.println("Error: " + ex + "\nLocation: " + ex.getLocator());
        }
    };

    private static String dir = ConfigTest.DATA_DIR + "defaultAttTest/";
    private static String xslPath = dir + "samples/dita_xsd_based/copy_stylesheet.xsl";
    private static String xmlPath = dir + "samples/dita_xsd_based/defAttr.xml";
    private static String outPath = dir + "samples/dita_xsd_based/out.xml";

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
//            SAXSource xmlSource = new SAXSource(new InputSource(new File(xmlPath).toURI().toString()));
//            SAXParser saxParser = new SAXParser();
//
//            //EXM-11081 Set this feature so that the default attributes from the schema are reported...
//            saxParser.setFeature("http://apache.org/xml/features/validation/schema", true);
//
//            xmlSource.setXMLReader(saxParser);
//
//            // Output stream result
//            File outFile = new File(outPath);
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
