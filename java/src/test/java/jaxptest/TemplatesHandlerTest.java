package jaxptest;

import net.sf.saxon.junit.Testdata;
import net.sf.saxon.lib.FeatureKeys;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;
import org.xml.sax.XMLReader;

import javax.xml.parsers.ParserConfigurationException;
import javax.xml.parsers.SAXParserFactory;
import javax.xml.transform.Templates;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.sax.SAXTransformerFactory;
import javax.xml.transform.sax.TemplatesHandler;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.*;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 *
 */
public class TemplatesHandlerTest {
    @Test
    public void testTemplatesHandler() throws SAXException,
            ParserConfigurationException,
            IOException, TransformerException {

        File xsl = Testdata.file("books.xsl");
        File xml = Testdata.file("books.xml");

        SAXParserFactory factory = SAXParserFactory.newInstance();
        factory.setNamespaceAware(true);
        factory.setValidating(false);
        XMLReader reader = factory.newSAXParser().getXMLReader();

        SAXTransformerFactory transformerFactory = new net.sf.saxon.TransformerFactoryImpl();
        //((TransformerFactoryImpl)transformerFactory).getConfiguration().setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
//(SAXTransformerFactory) TransformerFactory.newInstance();

        TemplatesHandler templatesHandler =
                transformerFactory.newTemplatesHandler();
        reader.setContentHandler(templatesHandler);
        reader.parse(xsl.toURI().toASCIIString());

        Templates templates = templatesHandler.getTemplates();
        Transformer transformer = templates.newTransformer();
        StringWriter sw = new StringWriter();
        transformer.transform(new StreamSource(xml), new StreamResult(sw));
        assertTrue(sw.toString().startsWith("<!DOCTYPE html>")
            || sw.toString().startsWith("<!DOCTYPE HTML>"));

    }

    @Test
    public void testTemplatesHandlerStaticExpressions() {

        // Bug 4684

        try {
            String xsl = "<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>" +
                    "<xsl:variable name='x' static='yes' xmlns:t='http://www.w3.org/1999/XSL/Transform' select='system-property(\"t:schema-aware\")'/>" +
                    "<xsl:template match='/'><out x='{$x}'/></xsl:template>" +
                    "</xsl:transform>";
            File xml = Testdata.file("books.xml");
            SAXParserFactory factory = SAXParserFactory.newInstance();
            factory.setNamespaceAware(true);
            factory.setValidating(false);
            XMLReader reader = factory.newSAXParser().getXMLReader();

            SAXTransformerFactory transformerFactory = new net.sf.saxon.TransformerFactoryImpl();

            TemplatesHandler templatesHandler =
                    transformerFactory.newTemplatesHandler();
            reader.setContentHandler(templatesHandler);
            reader.parse(new InputSource(new StringReader(xsl)));

            Templates templates = templatesHandler.getTemplates();
            Transformer transformer = templates.newTransformer();
            transformer.transform(new StreamSource(xml), new StreamResult(System.out));
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }

    }

    /**
     * Test use of a per-stylesheet extension library
     */

    @Test
    public void testExtensionNotFound() {
        try {
            File inputUri = Testdata.file("base64/books.xml");
            File xslt = Testdata.file("base64/TextTobase64.xsl");

            TransformerFactory tFactory = new net.sf.saxon.TransformerFactoryImpl();
            tFactory.setAttribute(FeatureKeys.RECOGNIZE_URI_QUERY_PARAMETERS, Boolean.TRUE);
            tFactory.setAttribute(FeatureKeys.STRIP_WHITESPACE, "ignorable");
            tFactory.setAttribute(FeatureKeys.VERSION_WARNING, Boolean.FALSE);

            // disable JIT compilation
            tFactory.setAttribute(FeatureKeys.OPTIMIZATION_LEVEL, "-j");

            Transformer transformer = tFactory.newTransformer(new StreamSource(xslt));


            System.out.println();
            System.out.println("XSLT starting.");
            transformer.transform(new StreamSource(inputUri),
                    new StreamResult(new OutputStreamWriter(System.out)));
            System.out.println();
            System.out.println("XSLT finished.");
            fail(); // should not succeed
        } catch (TransformerException e) {
            assertTrue(e.getMessage().contains("Cannot find a 1-argument function")) ;
            assertTrue(e.getMessage().contains("Cannot load Java class"));
            //e.printStackTrace();
        }
    }


}


