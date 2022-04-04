package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.lib.*;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;
import org.xml.sax.XMLReader;

import javax.xml.parsers.SAXParserFactory;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import java.io.*;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * test The features and properties of a XML Parser in s9api
 */
public class TestXMLParserOptions {
    private ConfigTest configTest = new ConfigTest();
    
    @Test
    public void testEnableOnExternalEntityExpansion() {
        try {
            Processor processor = new Processor(false);
            ParseOptions parserOptions = processor.getUnderlyingConfiguration().getParseOptions();

            parserOptions.addParserFeature("http://xml.org/sax/features/external-general-entities", true);
            StringReader sr = new StringReader("<?xml version=\"1.0\" standalone=\"no\" ?>\n" +
                    "<!DOCTYPE saxonica [\n" +
                    "  <!ELEMENT saxonica (#PCDATA)>\n" +
                    "  <!ENTITY welcome PUBLIC \"-//W3C//TEXT copyright//EN\"\n" +
                    "    \"https://www.saxonica.com/welcome/welcome.xml\">\n" +
                    "]>\n" +
                    "<saxonica>&welcome;</saxonica>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode doc = builder.build(source);
            XQueryCompiler c = processor.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("/saxonica/html").load();
            eval.setContextItem(doc);
            XdmValue val = eval.evaluate();
            assertEquals(1, val.size());

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testXmlVersions(){
        //test

        InputSource is = new InputSource("http://");
                     InputStream ssTest = new ByteArrayInputStream("<?xml version=\"1.1\"?><doc><rĳk RĲk=\"RIJK\"><bell>&#7;</bell><nbh>&#131;</nbh><nel>&#133;</nel><plain>vanilla</plain></rĳk></doc>".getBytes());
                is.setByteStream(ssTest);
                //end


    }

    @Test
    public void testDisableOnExternalEntityExpansion() {
        try {
            Processor processor = new Processor(false);
            ParseOptions parserOptions = processor.getUnderlyingConfiguration().getParseOptions();
            parserOptions.addParserFeature("http://xml.org/sax/features/external-general-entities", false);
            StringReader sr = new StringReader("<?xml version=\"1.0\" standalone=\"no\" ?>\n" +
                    "<!DOCTYPE saxonica [\n" +
                    "  <!ELEMENT saxonica (#PCDATA)>\n" +
                    "  <!ENTITY welcome PUBLIC \"-//W3C//TEXT copyright//EN\"\n" +
                    "    \"http://www.saxonica.com/welcome/welcome.xml\">\n" +
                    "]>\n" +
                    "<saxonica>&welcome;</saxonica>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode doc = builder.build(source);
            XQueryCompiler c = processor.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("empty(/saxonica/*)").load();
            eval.setContextItem(doc);
            XdmValue val = eval.evaluate();
            assertEquals(true, ((XdmAtomicValue) val).getBooleanValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


    @Test
    public void testDisableOnExternalEntityExpansion2() {
        try {
            Processor processor = new Processor(false);
            processor.setConfigurationProperty(FeatureKeys.XML_PARSER_FEATURE + "http://xml.org/sax/features/external-general-entities", false);
            StringReader sr = new StringReader("<?xml version=\"1.0\" standalone=\"no\" ?>\n" +
                    "<!DOCTYPE saxonica [\n" +
                    "  <!ELEMENT saxonica (#PCDATA)>\n" +
                    "  <!ENTITY welcome PUBLIC \"-//W3C//TEXT copyright//EN\"\n" +
                    "    \"http://www.saxonica.com/welcome/welcome.xml\">\n" +
                    "]>\n" +
                    "<saxonica>&welcome;</saxonica>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode doc = builder.build(source);
            XQueryCompiler c = processor.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("empty(/saxonica/*)").load();
            eval.setContextItem(doc);
            XdmValue val = eval.evaluate();
            assertEquals(true, ((XdmAtomicValue) val).getBooleanValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


    @Test
    public void testEnableOnExternalEntityExpansion2() {
        try {
            Processor processor = new Processor(false);
            processor.setConfigurationProperty(FeatureKeys.XML_PARSER_FEATURE + "http://xml.org/sax/features/external-general-entities", true);
            StringReader sr = new StringReader("<?xml version=\"1.0\" standalone=\"no\" ?>\n" +
                    "<!DOCTYPE saxonica [\n" +
                    "  <!ELEMENT saxonica (#PCDATA)>\n" +
                    "  <!ENTITY welcome PUBLIC \"-//W3C//TEXT copyright//EN\"\n" +
                    "    \"https://www.saxonica.com/welcome/welcome.xml\">\n" +
                    "]>\n" +
                    "<saxonica>&welcome;</saxonica>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode doc = builder.build(source);
            XQueryCompiler c = processor.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("/saxonica/html").load();
            eval.setContextItem(doc);
            XdmValue val = eval.evaluate();
            assertEquals(1, val.size());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testSettingOfProperty() {
        // Make sure the local schema location is a reasonable URI even on Windows
        // where the data directory location might begin "C:\...". Note that
        // getDataDir() should always return an absolute location, so I think a
        // Windows drive letter is the only way it can fail to begin with a "/".
        String localSchemaLocation = configTest.getDataDir().startsWith("/")
                ? "file://" + configTest.getDataDir() + "catalog-schema.xsd"
                : "file:///" + configTest.getDataDir() + "catalog-schema.xsd";

        Processor processor = new Processor(true);
        processor.setConfigurationProperty(FeatureKeys.XML_PARSER_FEATURE + "http://xml.org/sax/features/validation", true);
        processor.setConfigurationProperty(FeatureKeys.XML_PARSER_FEATURE + "http://apache.org/xml/features/validation/schema", true);
        processor.setConfigurationProperty(FeatureKeys.XML_PARSER_PROPERTY + "http://apache.org/xml/properties/schema/external-schemaLocation",
                "http://www.w3.org/2010/09/qt-fots-catalog " + localSchemaLocation);
        StringReader sr = new StringReader("<catalog xmlns=\"http://www.w3.org/2010/09/qt-fots-catalog\" \n" +
                "    test-suite=\"FOTS\" version=\"0.6\">\n" +
                "    <environment name=\"empty\">\n" +
                "        <!-- The context item MUST be undefined/absent for queries run in this environment. -->\n" +
                "        <!-- The test query will always be an expression, and if necessary an empty context can be achieved by wrapping\n" +
                "             the expression as the body of a function -->\n" +
                "    </environment>" +
                "   <!-- FUNCTIONS -->\n" +
                "   \n" +
                "    <test-set name=\"fn-abs\"                             file=\"fn/abs.xml\"/>  \n" +
                "    <test-set name=\"fn-adjust-date-to-timezone\"         file=\"fn/adjust-date-to-timezone.xml\"/>  \n" +
                "    <test-set name=\"fn-adjust-dateTime-to-timezone\"     file=\"fn/adjust-dateTime-to-timezone.xml\"/>" +
                "</catalog>");
        SAXSource source = new SAXSource(new InputSource(sr));
        DocumentBuilder builder = processor.newDocumentBuilder();
        try {
            XdmNode doc = builder.build(source);
            assertTrue(true, "XML document is valid");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }

    }

    @Test
    public void testErrorSettingOfProperty() {

        // MHK 2015-11-10: Having trouble with this test. When the outermost element is one for which no declaration is
        // available (catalogg), Xerces reports a validation error as expected. If we get the outermost element name right
        // but give it invalid content, no errors are reported. No idea why, but as it's not our product I'm not pursuing it.
        Processor processor = new Processor(true);
        processor.setConfigurationProperty(Feature.SOURCE_PARSER_CLASS, "org.apache.xerces.parsers.SAXParser");
        processor.setConfigurationProperty(FeatureKeys.XML_PARSER_FEATURE + "http://xml.org/sax/features/validation", false);
        processor.setConfigurationProperty(FeatureKeys.XML_PARSER_FEATURE + "http://apache.org/xml/features/validation/schema", true);
        processor.setConfigurationProperty(FeatureKeys.XML_PARSER_PROPERTY + "http://apache.org/xml/properties/schema/external-schemaLocation",
                                           "http://www.w3.org/2010/09/qt-fots-catalog http://dev.w3.org/2011/QT3-test-suite/catalog-schema.xsd");
        StringReader sr = new StringReader("<catalogg xmlns=\"http://www.w3.org/2010/09/qt-fots-catalog\" \n" +
                "    test-suite=\"FOTS\" version=\"0.6\" banana='republic'>\n" +
                "    <environmentalism name=\"empty\">\n" +
                "        <!-- The context item MUST be undefined/absent for queries run in this environment. -->\n" +
                "        <!-- The test query will always be an expression, and if necessary an empty context can be achieved by wrapping\n" +
                "             the expression as the body of a function -->\n" +
                "    </environmentalism>" +
                "   <!-- FUNCTIONS -->\n" +
                "   \n" +
                "    <test-set name=\"fn-abs\"                             file=\"fn/abs.xml\"/>  \n" +
                "    <test-set name=\"fn-adjust-date-to-timezone\"         file=\"fn/adjust-date-to-timezone.xml\"/>  \n" +
                "    <test-set name=\"fn-adjust-dateTime-to-timezone\"     file=\"fn/adjust-dateTime-to-timezone.xml\"/>" +
                "</catalogg>");
        //SAXSource source = new SAXSource(new InputSource(sr));
        StreamSource source = new StreamSource(sr);
        DocumentBuilder builder = processor.newDocumentBuilder();
        try {
            XdmNode doc = builder.build(source);

            fail("Validation errors not detected");

        } catch (SaxonApiException e) {
            assertTrue(true);

        }

    }

    @Test
    public void testUnknownFeature() {
        try {
            Processor processor = new Processor(false);
            processor.setConfigurationProperty(FeatureKeys.XML_PARSER_FEATURE + "http://xml.org/sax/features/xxxx", true);
            StringReader sr = new StringReader("<?xml version=\"1.0\" standalone=\"no\" ?>\n" +
                    "<!DOCTYPE saxonica [\n" +
                    "  <!ELEMENT saxonica (#PCDATA)>\n" +
                    "  <!ENTITY welcome PUBLIC \"-//W3C//TEXT copyright//EN\"\n" +
                    "    \"https://www.saxonica.com/welcome/welcome.xml\">\n" +
                    "]>\n" +
                    "<saxonica>&welcome;</saxonica>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode doc = builder.build(source);
            XQueryCompiler c = processor.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("/saxonica/html").load();
            eval.setContextItem(doc);
            XdmValue val = eval.evaluate();
            assertEquals(1, val.size());
            assertTrue(true, "Test should still pass, but warning with unknown parser feature");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testUnknownProperty() {
        try {
            Processor processor = new Processor(false);
            ParseOptions parserOptions = processor.getUnderlyingConfiguration().getParseOptions();
            processor.setConfigurationProperty(FeatureKeys.XML_PARSER_PROPERTY + "http://xml.org/sax/properties/xxxx", true);
            StringReader sr = new StringReader("<?xml version=\"1.0\" standalone=\"no\" ?>\n" +
                    "<!DOCTYPE saxonica [\n" +
                    "  <!ELEMENT saxonica (#PCDATA)>\n" +
                    "  <!ENTITY welcome PUBLIC \"-//W3C//TEXT copyright//EN\"\n" +
                    "    \"https://www.saxonica.com/welcome/welcome.xml\">\n" +
                    "]>\n" +
                    "<saxonica>&welcome;</saxonica>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode doc = builder.build(source);
            XQueryCompiler c = processor.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("/saxonica/html").load();
            eval.setContextItem(doc);
            XdmValue val = eval.evaluate();
            assertEquals(1, val.size());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testEnableXInclude() {
        Processor proc = new Processor(true);

        proc.setConfigurationProperty(FeatureKeys.XML_PARSER_FEATURE + "http://apache.org/xml/features/xinclude", true);

        StringReader sr = new StringReader("\n" +
                "<saxonica xmlns:xi=\"http://www.w3.org/2001/XInclude\">\n" +
                "  <xi:include href=\"https://www.saxonica.com/welcome/welcome.xml\"/>\n" +
                "</saxonica>");

        SAXSource source = new SAXSource(new InputSource(sr));
        DocumentBuilder builder = proc.newDocumentBuilder();
        try {
            XdmNode doc = builder.build(source);
            XQueryCompiler c = proc.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("empty(/saxonica/*:include) and exists(/saxonica/html)").load();
            eval.setContextItem(doc);
            XdmValue val = eval.evaluate();
            assertEquals(true, ((XdmAtomicValue) val).getBooleanValue());
        } catch (SaxonApiException e) {
            fail();
        }

    }


    @Test
    public void testDisableXInclude() {
        Processor proc = new Processor(true);
        proc.setConfigurationProperty(Feature.SOURCE_PARSER_CLASS, "org.apache.xerces.parsers.SAXParser");
        proc.setConfigurationProperty(FeatureKeys.XML_PARSER_FEATURE + "http://apache.org/xml/features/xinclude", false);

        StringReader sr = new StringReader("\n" +
                "<saxonica xmlns:xi=\"http://www.w3.org/2001/XInclude\">\n" +
                "  <xi:include href=\"http://www.saxonica.com/welcome/welcome.xml\"/>\n" +
                "</saxonica>");

        SAXSource source = new SAXSource(new InputSource(sr));
        DocumentBuilder builder = proc.newDocumentBuilder();
        try {
            XdmNode doc = builder.build(source);
            XQueryCompiler c = proc.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("exists(/saxonica/*:include) and empty(/saxonica/html)").load();
            eval.setContextItem(doc);
            XdmValue val = eval.evaluate();
            assertEquals(true, ((XdmAtomicValue) val).getBooleanValue());
        } catch (SaxonApiException e) {
            fail();
        }

    }


    @Test
    public void testDefaultedXsdAttributeWithNamespaceXercesSuppress() {
        runDefaultedXsdAttributeWithNamespaceXerces(true);
    }

    @Test
    public void testDefaultedXsdAttributeWithNamespaceXercesNoSuppress() {
        // Bug 4966
        runDefaultedXsdAttributeWithNamespaceXerces(false);
    }

    private void runDefaultedXsdAttributeWithNamespaceXerces(boolean suppress) {
        // Read XML file via Xerces parser and Xerces schema validator expanding attribute defaults
        try {
            Processor proc = new Processor(false);
            if (suppress) {
                proc.setConfigurationProperty(Feature.EXPAND_ATTRIBUTE_DEFAULTS, false);
            }
            XsltCompiler comp = proc.newXsltCompiler();
            SAXParserFactory spf;
            spf = SAXParserFactory.newInstance();
            spf.setNamespaceAware(true);
            XMLReader xmr = spf.newSAXParser().getXMLReader();
            xmr.setFeature("http://apache.org/xml/features/validation/schema", true);

            XsltExecutable exp = comp.compile(new StreamSource(new File(configTest.getBugsDir() + "/4996/test.xsl")));

            StringWriter sw = new StringWriter();
            Serializer out = proc.newSerializer(sw);

            File dataFile = new File(configTest.getBugsDir() + "/4996/xmlSchemaDefaultAttributeA.xml");
            InputSource inputSource = new InputSource(dataFile.toURI().toString());
            SAXSource source1 = new SAXSource(xmr, inputSource);

            XsltTransformer trans = exp.load();
            trans.setSource(source1);
            trans.setDestination(out);
            trans.transform();
            //System.err.println(sw);
            if (suppress) {
                assertFalse(sw.toString().contains("=\"http://com.deltaxml.test/namespace\""), "no namespace declaration");
                assertFalse(sw.toString().contains("test=\"yes\""), "no attribute");
            } else {
                assertTrue(sw.toString().contains("=\"http://com.deltaxml.test/namespace\""), "contains namespace declaration");
                assertTrue(sw.toString().contains(":test=\"yes\""), "attribute is prefixed");
            }
        } catch (Exception e) {
            fail();
        }
    }

    @Test
    public void testDefaultedXsdAttributeWithNamespaceSaxonSuppress() {
        testDefaultedXsdAttributeWithNamespaceSaxon(true);
    }

    @Test
    public void testDefaultedXsdAttributeWithNamespaceSaxonNoSuppress() {
        testDefaultedXsdAttributeWithNamespaceSaxon(false);
    }

    private void testDefaultedXsdAttributeWithNamespaceSaxon(boolean suppress) {
        // Read XML file via Xerces parser and Saxon schema validator expanding attribute defaults
        try {
            Processor proc = new Processor(true);
            if (suppress) {
                proc.setConfigurationProperty(Feature.EXPAND_ATTRIBUTE_DEFAULTS, false);
            }
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            SAXParserFactory spf;
            spf = SAXParserFactory.newInstance();
            spf.setNamespaceAware(true);
            XMLReader xmr = spf.newSAXParser().getXMLReader();
            xmr.setFeature("http://apache.org/xml/features/validation/schema", false);

            XsltExecutable exp = comp.compile(new StreamSource(new File(configTest.getBugsDir() + "/4996/test.xsl")));

            StringWriter sw = new StringWriter();
            Serializer out = proc.newSerializer(sw);

            File dataFile = new File(configTest.getBugsDir() + "/4996/xmlSchemaDefaultAttributeA.xml");
            InputSource inputSource = new InputSource(dataFile.toURI().toString());
            SAXSource source1 = new SAXSource(xmr, inputSource);
            ParseOptions options = new ParseOptions();
            options.setSchemaValidationMode(Validation.STRICT);
            options.setUseXsiSchemaLocation(true);
            AugmentedSource as = new AugmentedSource(source1, options);
            XsltTransformer trans = exp.load();
            trans.setSource(as);
            trans.setDestination(out);
            trans.transform();
            if (suppress) {
                assertFalse(sw.toString().contains("=\"http://com.deltaxml.test/namespace\""), "no namespace declaration");
                assertFalse(sw.toString().contains("test=\"yes\""), "no attribute");
            } else {
                assertTrue(sw.toString().contains("=\"http://com.deltaxml.test/namespace\""), "contains namespace declaration");
                assertTrue(sw.toString().contains(":test=\"yes\""), "attribute is prefixed");
            }
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

}
