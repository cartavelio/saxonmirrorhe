package jaxptest;

import net.sf.saxon.Configuration;
import net.sf.saxon.jaxp.SaxonTransformerFactory;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.Serializer;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;
import org.xml.sax.*;

import javax.xml.parsers.SAXParser;
import javax.xml.parsers.SAXParserFactory;
import javax.xml.transform.Templates;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.sax.SAXResult;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.io.StringWriter;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


/**
 *
 */
public class SAXTest {
    private final ConfigTest configTest = new ConfigTest();

    /**
     * Test using a SAXSource with a user-supplied ErrorHandler. In 9.1 Saxon replaces this with
     * its own ErrorHandler
     */

    @Test
    public void testErrorHandler() {
        try {
            String in = "<?xml version='1.0'?> \n" +
                    "<Document><e></f></Document>";

            SAXParserFactory factory = SAXParserFactory.newInstance();
            factory.setNamespaceAware(true);
            factory.setValidating(false);
            XMLReader parser = factory.newSAXParser().getXMLReader();
            parser.setErrorHandler(new ErrorHandler() {
                @Override
                public void warning(SAXParseException exception) throws SAXException {
                    System.err.println("Called warning");
                }

                @Override
                public void error(SAXParseException exception) throws SAXException {
                    System.err.println("Called error");
                }

                @Override
                public void fatalError(SAXParseException exception) throws SAXException {
                    System.err.println("Called fatalError");
                }
            });
            SAXSource source = new SAXSource(parser, new InputSource(new StringReader(in)));
            source.setSystemId("http://saxonica.com/");

            Configuration config = new Configuration();
            NodeInfo doc = config.buildDocumentTree(source).getRootNode();
            fail("parsing should have failed");
        }
        catch (Exception e) {
            assertTrue(e instanceof XPathException);
            assertTrue(e.getCause() instanceof SAXException);
            assertTrue(((XPathException)e).hasBeenReported());
        }
    }

    // Test using a SAXResult. We're particularly interested in ensuring that location information
    // is made available (Oxygen use this for "back mapping", where a piece of output is traceable
    // to the instructions in the stylesheet that created it).

    @Test
    public void testBackMapping() {
        String xml = "<a/>";
        String textUri = "file:///" + configTest.getDataDir() + "sample.json";
        String xsl = "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                "<xsl:template match='/'>\n" +
                "<out>\n" +
                "  <xsl:attribute name='pi' select='3.1459'/>\n" +
                "  <one><xsl:value-of select='1 to 20'/></one>\n" +
                "  <two><xsl:value-of select='unparsed-text(\"" + textUri + "\")'/></two>\n" +
                "</out>\n" +
                "</xsl:template>\n" +
                "</xsl:transform>";
        SaxonTransformerFactory factory = new SaxonTransformerFactory();
        SAXResult result = new SAXResult(new ContentHandler() {
            Locator loc;

            @Override
            public void setDocumentLocator(Locator locator) {
                loc = locator;
            }

            @Override
            public void startDocument() throws SAXException {

            }

            @Override
            public void endDocument() throws SAXException {

            }

            @Override
            public void startPrefixMapping(String prefix, String uri) throws SAXException {

            }

            @Override
            public void endPrefixMapping(String prefix) throws SAXException {

            }

            @Override
            public void startElement(String uri, String localName, String qName, Attributes atts) throws SAXException {
                switch (localName) {
                    case "out":
                        assertEquals(3, loc.getLineNumber());
                        break;
                    case "one":
                        assertEquals(5, loc.getLineNumber());
                        break;
                    case "two":
                        assertEquals(6, loc.getLineNumber());
                        break;
                    default:
                        fail(localName);
                        break;
                }
            }

            @Override
            public void endElement(String uri, String localName, String qName) throws SAXException {

            }

            @Override
            public void characters(char[] ch, int start, int length) throws SAXException {
                String val = new String(ch, start, length);
                if (val.startsWith("1 2")) {
                    assertEquals(5, loc.getLineNumber());
                } else if (val.contains("A green door")) {
                    assertEquals(6, loc.getLineNumber());
                } else {
                    fail();
                }
            }

            @Override
            public void ignorableWhitespace(char[] ch, int start, int length) throws SAXException {

            }

            @Override
            public void processingInstruction(String target, String data) throws SAXException {

            }

            @Override
            public void skippedEntity(String name) throws SAXException {

            }
        });
        try {
            Transformer t = factory.newTransformer(new StreamSource(new StringReader(xsl)));
            t.transform(new StreamSource(new StringReader(xml)), result);
        } catch (TransformerException e) {
            fail();
        }
    }

    @Test
    public void testSAXParserFactory() {
        try {
            Configuration configuration = new Configuration();
            Processor processor = new Processor(configuration);
            StringWriter sw = new StringWriter();
            Serializer serializer = processor.newSerializer(sw);

            SAXParser parser = SAXParserFactory.newInstance().newSAXParser();
            XMLReader xmlReader = parser.getXMLReader();
            ParseOptions parseOptions = new ParseOptions().withXMLReader(xmlReader);

            NodeInfo node = configuration
                    .buildDocumentTree(new StreamSource(new StringReader("<b xmlns='a'/>")), parseOptions)
                    .getRootNode();
            serializer.serialize(node);
            assertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?><b xmlns=\"a\"/>", sw.toString());
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testSAXResult() {
        String xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                + "<colors>\n"
                + "    <red>10</red>\n"
                + "    <blue>20</blue>\n"
                + "    <green>15</green>  \n"
                + "</colors>";
        String xsl = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                + "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"3.0\">\n"
                + "    <xsl:output method=\"xml\" indent=\"yes\"/>\n"
                + "    <xsl:template match=\"/\">\n"
                + "        <svg xmlns=\"http://www.w3.org/2000/svg\">\n"
                + "            <xsl:apply-templates/>\n"
                + "        </svg>\n"
                + "    </xsl:template>\n"
                + "    <xsl:template match=\"colors/*\">\n"
                + "        <circle cx=\"{(position() - 1) * 100}\" cy=\"100\" r=\"{.}\"/>\n"
                + "    </xsl:template>\n"
                + "</xsl:stylesheet>";

        try {
            ContentHandler ch = new ContentHandler() {

                int eventNumber = 0;
                public void setDocumentLocator(Locator locator) {
                    System.out.println("setDocumentLocator");
                }

                @Override
                public void startDocument() throws SAXException {
                    assertEquals(0, eventNumber++);
                }

                @Override
                public void endDocument() throws SAXException {
                    assertEquals(21, eventNumber++);
                    //System.out.println("endDocument");
                }

                @Override
                public void startPrefixMapping(String prefix, String uri)
                        throws SAXException {
                    if (eventNumber == 1) {
                        assertEquals("", prefix);
                        assertEquals("http://www.w3.org/2000/svg", uri);
                    } else if (eventNumber == 4 || eventNumber == 9 || eventNumber == 14) {
                        assertEquals("", prefix);
                        assertEquals("", uri);
                    } else {
                        fail();
                    }
                    eventNumber++;
                    //System.out.println("startPrefixMapping: " + prefix + ", " + uri);
                }

                @Override
                public void endPrefixMapping(String prefix) throws SAXException {
                    if (eventNumber == 7 || eventNumber == 12
                            || eventNumber == 17 || eventNumber == 20) {
                        assertEquals("", prefix);
                    } else {
                        fail();
                    }
                    eventNumber++;
                    //System.out.println("endPrefixMapping: " + prefix);
                }

                @Override
                public void startElement(
                        String namespaceURI, String localName, String qName, Attributes atts)
                        throws SAXException {

                    if (eventNumber == 2) {
                        assertEquals("svg", localName);
                        assertEquals("svg", qName);
                        assertEquals("http://www.w3.org/2000/svg", namespaceURI);
                        assertEquals(0, atts.getLength());
                    } else if (eventNumber == 5 || eventNumber == 10 || eventNumber == 15) {
                        assertEquals("circle", localName);
                        assertEquals("circle", qName);
                        assertEquals("", namespaceURI);
                        assertEquals(3, atts.getLength());
                    } else {
                        fail();
                    }

                    eventNumber++;
                    // System.out.print("startElement: " + namespaceURI + ", " + localName + ", " + qName);
                }

                @Override
                public void endElement(
                        String namespaceURI, String localName, String qName)
                        throws SAXException {
                    if (eventNumber == 6 || eventNumber == 11 || eventNumber == 16) {
                        assertEquals("circle", localName);
                        assertEquals("circle", qName);
                        assertEquals("", namespaceURI);
                    } else if (eventNumber == 19) {
                        assertEquals("svg", localName);
                        assertEquals("svg", qName);
                        assertEquals("http://www.w3.org/2000/svg", namespaceURI);
                    } else {
                        fail();
                    }
                    eventNumber++;
                    //System.out.println("endElement: " + namespaceURI + ", " + localName + ", " + qName);
                }

                @Override
                public void characters(char[] ch, int start, int length)
                        throws SAXException {
                    assertTrue(eventNumber == 3 || eventNumber == 8 || eventNumber == 13 || eventNumber == 18);
                    eventNumber++;
                }

                @Override
                public void ignorableWhitespace(char[] ch, int start, int length)
                        throws SAXException {
                    fail();
                }

                @Override
                public void processingInstruction(String target, String data)
                        throws SAXException {
                    System.out.println("processingInstruction: " + target + ", "
                                               + data);
                }

                @Override
                public void skippedEntity(String name) throws SAXException {
                    System.out.println("skippedEntity: " + name);
                }
            };


            TransformerFactory f = new SaxonTransformerFactory();
            Templates templates = f.newTemplates(new StreamSource(new StringReader(xsl)));
            Transformer t = templates.newTransformer();
            t.transform(new StreamSource(new StringReader(xml)), new SAXResult(ch));

        } catch (TransformerException te) {
            fail();
        }
    }



}
