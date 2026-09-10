package jaxptest;

import com.saxonica.config.ProfessionalTransformerFactory;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.lib.StandardOutputResolver;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;
import org.xml.sax.XMLFilter;
import org.xml.sax.XMLReader;
import org.xml.sax.helpers.DefaultHandler;
import org.xml.sax.helpers.XMLFilterImpl;

import javax.xml.parsers.ParserConfigurationException;
import javax.xml.parsers.SAXParser;
import javax.xml.parsers.SAXParserFactory;
import javax.xml.transform.*;
import javax.xml.transform.sax.SAXResult;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.sax.SAXTransformerFactory;
import javax.xml.transform.sax.TransformerHandler;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.IOException;
import java.io.StringReader;
import java.io.StringWriter;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

public class TransformChainTest {
    private final ConfigTest configTest = new ConfigTest();

    private static final String IDENTITY_STYLESHEET
            = "<?xml version=\"1.0\"?> \n" +
            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
            "<xsl:template match=\"@*|node()\">\n" +
            "<xsl:copy>\n" +
            "  <xsl:apply-templates select=\"@*|node()\" /> \n" +
            "</xsl:copy>\n" +
            "</xsl:template>\n" +
            "</xsl:stylesheet>";

    private static final String[] STYLESHEETS = new String[]{
            IDENTITY_STYLESHEET, IDENTITY_STYLESHEET
    };

    @Test
    public void testFilterChaining() throws Exception {
        InputSource inputSource = new InputSource(new StringReader("<test/>"));

        SAXTransformerFactory tFactory = new net.sf.saxon.TransformerFactoryImpl();
        SAXParserFactory pFactory = SAXParserFactory.newInstance();
//        System.out.println("SAXParserFactory: " + pFactory.getClass().getName());
//        System.out.println("SAXTransformerFactory: " + tFactory.getClass().getName());
//        System.out.println("SAXON: " + net.sf.saxon.Version.getProductVersion());

        pFactory.setNamespaceAware(true);
        pFactory.setValidating(false);
        SAXParser parser = pFactory.newSAXParser();

        XMLReader parent = parser.getXMLReader();
        for (String xsltFile : STYLESHEETS) {
            XMLFilter filter = tFactory.newXMLFilter(new StreamSource(new StringReader(xsltFile)));
            filter.setParent(parent);
            parent = filter;
        }

        Source src = new SAXSource(parent, inputSource);

        Result res = new SAXResult(new DefaultHandler() {
            private boolean startDocumentCalled = false;
            private boolean endDocumentCalled = false;

            @Override
            public void startDocument() throws SAXException {
                if (!this.startDocumentCalled) {
                    this.startDocumentCalled = true;
                } else {
                    fail("startDocument() called twice. "
                            + "This breaks the SAX ContentHandler contract.");
                }
            }

            @Override
            public void endDocument() throws SAXException {
                if (!this.endDocumentCalled) {
                    this.endDocumentCalled = true;
                } else {
                    fail("endDocument() called twice. "
                            + "This breaks the SAX ContentHandler contract.");
                }
            }
        });

        Transformer transformer = tFactory.newTransformer();
        transformer.transform(src, res);
    }

    @Test
    public void testPipelineComment() {
        try {
            String style = "<xsl:stylesheet version=\"1.0\"\n" +
                    "    xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                    "\n" +
                    "    <xsl:template match=\"/\">\n" +
                    "        <xsl:comment>Nice comment</xsl:comment>\n" +
                    "        <test>[<xsl:value-of select=\"system-property('xsl:vendor')\" />] \n" +
                    "            (<xsl:value-of select=\"system-property('xsl:version')\" />\n" +
                    "            )[<xsl:value-of select=\"system-property('xsl:vendor-url')\" />]</test>\n" +
                    "    </xsl:template>\n" +
                    "</xsl:stylesheet>";
            TransformerFactory factory = new TransformerFactoryImpl();
            SAXTransformerFactory saxFactory = (SAXTransformerFactory) factory;
            SAXParserFactory parserFactory = SAXParserFactory.newInstance();

            parserFactory.setNamespaceAware(true);
            XMLReader parser = parserFactory.newSAXParser().getXMLReader();
            XMLFilter pre = new XMLFilterImpl(parser);
            XMLFilter xslFilter = saxFactory.newXMLFilter(new StreamSource(new StringReader(style)));
            xslFilter.setParent(pre);
            XMLFilter post = new XMLFilterImpl(xslFilter);

            TransformerHandler serializer = saxFactory.newTransformerHandler();
            serializer.setResult(new StreamResult(System.out));
            Transformer trans = serializer.getTransformer();
            trans.setOutputProperty(OutputKeys.METHOD, "xml");

            post.setContentHandler(serializer);
            post.parse(new InputSource(new StringReader("<doc/>")));
        } catch (SAXException e) {
            fail();
        } catch (ParserConfigurationException e) {
            fail();
        } catch (TransformerConfigurationException e) {
            fail();
        } catch (IOException e) {
            fail();
        }
    }

    /**
     * Test saxon:next-in-chain
     */

    @Test
    public void testNextInChain() {
        try {
            ProfessionalTransformerFactory factory = new ProfessionalTransformerFactory();
            factory.getConfiguration().setOutputURIResolver(new StandardOutputResolver() {
                StringWriter writer;

                @Override
                public Result resolve(String href, String base) throws XPathException {
                    writer = new StringWriter();
                    return new StreamResult(writer);
                }

                @Override
                public void close(Result result) throws XPathException {
                    assertTrue(writer.toString().equals("<second>secondary</second>"));
                }
            });
            Templates templates = factory.newTemplates(
                    new StreamSource(new File(configTest.getDataDir() + "next-in-chain/main.xsl")));
            Transformer transformer = templates.newTransformer();
            StringWriter sw = new StringWriter();
            transformer.transform(
                    new StreamSource(new File(configTest.getDataDir() + "next-in-chain/test.xml")),
                    new StreamResult(sw)
            );
            assertTrue(sw.toString().equals("<second>principal</second>"));

        } catch (TransformerException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }


}



