package s9apitest;

import jaxptest.ConfigTest;
import junit.framework.TestCase;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.s9api.*;
import org.xml.sax.InputSource;

import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import java.io.ByteArrayInputStream;
import java.io.InputStream;
import java.io.StringReader;

/**
 * test The features and properties of a XML Parser in s9api
 */
public class TestXMLParserOptions extends TestCase {

    public void testEnableOnExternalEntityExpansion() {
        try {
            Processor processor = new Processor(false);
            ParseOptions parserOptions = processor.getUnderlyingConfiguration().getParseOptions();

            parserOptions.addParserFeature("http://xml.org/sax/features/external-general-entities", true);
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
            XQueryEvaluator eval = c.compile("/saxonica/html").load();
            eval.setContextItem(doc);
            XdmValue val = eval.evaluate();
            assertEquals(val.size(), 1);

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    public void testXmlVersions(){
        //test

        InputSource is = new InputSource("http://");
                     InputStream ssTest = new ByteArrayInputStream(new String("<?xml version=\"1.1\"?><doc><rĳk RĲk=\"RIJK\"><bell>&#7;</bell><nbh>&#131;</nbh><nel>&#133;</nel><plain>vanilla</plain></rĳk></doc>").getBytes());
                is.setByteStream(ssTest);
                //end


    }

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
            assertEquals(((XdmAtomicValue) val).getBooleanValue(), true);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


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
            assertEquals(((XdmAtomicValue) val).getBooleanValue(), true);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


    public void testEnableOnExternalEntityExpansion2() {
        try {
            Processor processor = new Processor(false);
            processor.setConfigurationProperty(FeatureKeys.XML_PARSER_FEATURE + "http://xml.org/sax/features/external-general-entities", true);
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
            XQueryEvaluator eval = c.compile("/saxonica/html").load();
            eval.setContextItem(doc);
            XdmValue val = eval.evaluate();
            assertEquals(val.size(), 1);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    public void testSettingOfProperty() {
        Processor processor = new Processor(true);
        processor.setConfigurationProperty(FeatureKeys.XML_PARSER_FEATURE + "http://xml.org/sax/features/validation", true);
        processor.setConfigurationProperty(FeatureKeys.XML_PARSER_FEATURE + "http://apache.org/xml/features/validation/schema", true);
        processor.setConfigurationProperty(FeatureKeys.XML_PARSER_PROPERTY + "http://apache.org/xml/properties/schema/external-schemaLocation", "http://www.w3.org/2010/09/qt-fots-catalog " + ConfigTest.DATA_DIR + "catalog-schema.xsd");
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
            assertTrue("XML document is valid", true);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }

    }

    public void testErrorSettingOfProperty() {

        // MHK 2015-11-10: Having trouble with this test. When the outermost element is one for which no declaration is
        // available (catalogg), Xerces reports a validation error as expected. If we get the outermost element name right
        // but give it invalid content, no errors are reported. No idea why, but as it's not our product I'm not pursuing it.
        Processor processor = new Processor(true);
        processor.setConfigurationProperty(FeatureKeys.SOURCE_PARSER_CLASS, "org.apache.xerces.parsers.SAXParser");
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

    public void testUnknownFeature() {
        try {
            Processor processor = new Processor(false);
            processor.setConfigurationProperty(FeatureKeys.XML_PARSER_FEATURE + "http://xml.org/sax/features/xxxx", true);
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
            XQueryEvaluator eval = c.compile("/saxonica/html").load();
            eval.setContextItem(doc);
            XdmValue val = eval.evaluate();
            assertEquals(val.size(), 1);
            assertTrue("Test should still pass, but warning with unknown parser feature", true);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    public void testUnknownProperty() {
        try {
            Processor processor = new Processor(false);
            ParseOptions parserOptions = processor.getUnderlyingConfiguration().getParseOptions();
            processor.setConfigurationProperty(FeatureKeys.XML_PARSER_PROPERTY + "http://xml.org/sax/properties/xxxx", true);
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
            XQueryEvaluator eval = c.compile("/saxonica/html").load();
            eval.setContextItem(doc);
            XdmValue val = eval.evaluate();
            assertEquals(val.size(), 1);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    public void testEnableXInclude() {
        Processor proc = new Processor(true);

        proc.setConfigurationProperty(FeatureKeys.XML_PARSER_FEATURE + "http://apache.org/xml/features/xinclude", true);

        StringReader sr = new StringReader("\n" +
                "<saxonica xmlns:xi=\"http://www.w3.org/2001/XInclude\">\n" +
                "  <xi:include href=\"http://www.saxonica.com/welcome/welcome.xml\"/>\n" +
                "</saxonica>");

        SAXSource source = new SAXSource(new InputSource(sr));
        DocumentBuilder builder = proc.newDocumentBuilder();
        try {
            XdmNode doc = builder.build(source);
            XQueryCompiler c = proc.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("empty(/saxonica/*:include) and exists(/saxonica/html)").load();
            eval.setContextItem(doc);
            XdmValue val = eval.evaluate();
            assertEquals(((XdmAtomicValue) val).getBooleanValue(), true);
        } catch (SaxonApiException e) {
            fail();
        }

    }


    public void testDisableXInclude() {
        Processor proc = new Processor(true);
        proc.setConfigurationProperty(FeatureKeys.SOURCE_PARSER_CLASS, "org.apache.xerces.parsers.SAXParser");
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
            assertEquals(((XdmAtomicValue) val).getBooleanValue(), true);
        } catch (SaxonApiException e) {
            fail();
        }

    }
}
