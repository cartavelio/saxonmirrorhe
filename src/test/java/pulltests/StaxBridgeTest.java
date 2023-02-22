package pulltests;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.NamespaceReducer;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.pull.PullEvent;
import net.sf.saxon.pull.PullPushCopier;
import net.sf.saxon.pull.StaxBridge;
import net.sf.saxon.serialize.UTF8Writer;
import net.sf.saxon.serialize.XMLEmitter;
import net.sf.saxon.str.UnicodeWriterToWriter;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;

import java.io.*;
import java.nio.charset.StandardCharsets;
import java.util.Properties;

import static org.junit.jupiter.api.Assertions.*;

/**
 * Test input from a Stax (JSR173) parser
 */
public class StaxBridgeTest {
    private final ConfigTest configTest = new ConfigTest();

    @Test
    public void testFromStaxToSerializer() {
        try {
            // Use Woodstox as the pull parser by default. The Sun (Zephyr) parser is too buggy...
            System.setProperty("javax.xml.stream.XMLInputFactory", "com.ctc.wstx.stax.WstxInputFactory");
            PipelineConfiguration pipe = new Configuration().makePipelineConfiguration();
            StaxBridge puller = new StaxBridge();
            File f = new File(configTest.getDataDir() + "books.xml");
            puller.setInputStream(f.toURI().toString(), new FileInputStream(f));
            XMLEmitter emitter = new XMLEmitter();
            emitter.setPipelineConfiguration(pipe);
            emitter.setOutputProperties(new Properties());
            emitter.setUnicodeWriter(new UTF8Writer(new FileOutputStream(configTest.getDataDir() + "temp/books.xml")));
            NamespaceReducer r = new NamespaceReducer(emitter);
            puller.setPipelineConfiguration(pipe);
            r.setPipelineConfiguration(pipe);
            new PullPushCopier(puller, r).copy();
        } catch (XPathException | FileNotFoundException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Variant of the above where the XmlStreamReader is a PullToStax
     */

    @Test
    public void testPullToStax() {
//        try {
//            Configuration config = new Configuration();
//            PipelineConfiguration pipe = config.makePipelineConfiguration();
//            StringReader sr = new StringReader("<a>text</a>");
//            NodeInfo doc = config.buildDocumentTree(new StreamSource(sr)).getRootNode();
//            TinyTreeWalker ttw = new TinyTreeWalker((TinyDocumentImpl) doc);
//            ttw.setPipelineConfiguration(pipe);
//            PullToStax xmlReader = new PullToStax(ttw);
//
//            assertNotNull("null XMLStreamReader", xmlReader);
//            assertEquals("unexpected first event returned by XMLStreamReader", XMLStreamReader.START_DOCUMENT, xmlReader.getEventType());
//            assertEquals("unexpected second event returned by XMLStreamReader", XMLStreamReader.START_ELEMENT, xmlReader.next());
//            assertEquals("unexpected third event returned by XMLStreamReader", XMLStreamReader.CHARACTERS, xmlReader.next());
//            assertEquals("unexpected fourth event returned by XMLStreamReader", XMLStreamReader.END_ELEMENT, xmlReader.next());
//            assertEquals("unexpected fifth event returned by XMLStreamReader", XMLStreamReader.END_DOCUMENT, xmlReader.next());
//        } catch (XMLStreamException e) {
//            fail("A-XQIA-9.1: XMLStreamReader.next() failed with message: " + e.getMessage());
//        } catch (XPathException err) {
//            fail("buildDocument");
//        }

    }

    /**
     * Variant of the above with comments and processing instructions
     */

    @Test
    public void testPullToStaxPIandComments() {
        try {
            // Use Woodstox as the pull parser by default. The Sun (Zephyr) parser is too buggy...
            System.setProperty("javax.xml.stream.XMLInputFactory", "com.ctc.wstx.stax.WstxInputFactory");
            PipelineConfiguration pipe = new Configuration().makePipelineConfiguration();
            StaxBridge puller = new StaxBridge();
            String comment = null;
            String piName = null;
            String piContent = null;
            File f = new File(configTest.getDataDir() + "birthdays.xml");
            puller.setInputStream(f.toURI().toString(), new FileInputStream(f));
            PullEvent ev;
            while ((ev = puller.next()) != PullEvent.END_OF_INPUT) {
                if (ev == PullEvent.COMMENT) {
                    comment = puller.getStringValue().toString();
                } else if (ev == PullEvent.PROCESSING_INSTRUCTION) {
                    piName = puller.getNodeName().getLocalPart();
                    piContent = puller.getStringValue().toString();
                }
            }
            assertEquals("instance for use with schema-with-param.xsd", comment);
            assertEquals("xml-stylesheet", piName);
            assertEquals("type=\"text/xsl\" href=\"bills.xsl\"", piContent);
        } catch (XPathException | FileNotFoundException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testXsltFromStax() {
        // This is particularly designed to check that namespace information is getting through;
        // XSLT makes heavy use of QNames in content
        try {
            System.setProperty("javax.xml.stream.XMLInputFactory", "com.ctc.wstx.stax.WstxInputFactory");
            PipelineConfiguration pipe = new Configuration().makePipelineConfiguration();
            StaxBridge puller = new StaxBridge();
            String xsl = "<xsl:transform\n" +
                    " xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                    " xmlns:f=\"http://f/\"\n" +
                    " version=\"2.0\">\n" +
                    "\n" +
                    "<xsl:template match=\"*|f:*\">\n" +
                    "  <xsl:copy>\n" +
                    "    <xsl:copy-of select=\"@*\"/>\n" +
                    "    <xsl:apply-templates/>\n" +
                    "  </xsl:copy>\n" +
                    "</xsl:template>\n" +
                    "\n" +
                    "</xsl:transform>";
            byte[] in = xsl.getBytes(StandardCharsets.UTF_8);
            puller.setInputStream("dummy.xsl", new ByteArrayInputStream(in));
            XMLEmitter emitter = new XMLEmitter();
            emitter.setPipelineConfiguration(pipe);
            emitter.setOutputProperties(new Properties());
            StringWriter sw = new StringWriter();
            emitter.setUnicodeWriter(new UnicodeWriterToWriter(sw));
            NamespaceReducer r = new NamespaceReducer(emitter);
            puller.setPipelineConfiguration(pipe);
            r.setPipelineConfiguration(pipe);
            new PullPushCopier(puller, r).copy();
            //System.err.println(sw.toString());
            assertTrue(sw.toString().contains("xmlns:f=\"http://f/\""));
//            assertTrue(sw.toString().contains("<?xml-stylesheet type=\"text/xsl\" href=\"bills.xsl\"?>"));
//            assertTrue(sw.toString().contains("<Bill No=\"10030\" Name=\"John Doe\">"));
//            assertTrue(sw.toString().contains("<!--Observation-->"));
        } catch (XPathException e) {
            fail(e.getMessage());
        }
    }


    public void testDOMToStax() {
//        try {
//            String xml = "<a><b> <c>x</c></b></a>";
//
//            DOMSource domSource = new DOMSource(
//                    DocumentBuilderFactory.newInstance().newDocumentBuilder().
//                            parse(new InputSource(new StringReader(xml))));
//
//            Configuration configuration = new Configuration();
//            Processor processor = new Processor(configuration);
//            NodeInfo node = processor.newDocumentBuilder().wrap(domSource.getNode()).getUnderlyingNode();
//            PullProvider treeWalker = TreeWalker.makeTreeWalker(node);
//            treeWalker.setPipelineConfiguration(new PipelineConfiguration(configuration));
//
//            for (PullToStax xmlStreamReader = new PullToStax(treeWalker); xmlStreamReader.hasNext(); ) {
//                int xmlParsingEvent = xmlStreamReader.next();
//                switch (xmlParsingEvent) {
//                    case XMLStreamConstants.START_DOCUMENT:
//                        System.out.println("START_DOCUMENT");
//                        break;
//                    case XMLStreamConstants.END_DOCUMENT:
//                        System.out.println("END_DOCUMENT");
//                        break;
//                    case XMLStreamConstants.START_ELEMENT:
//                        System.out.println("START_ELEMENT " + xmlStreamReader.getLocalName());
//                        break;
//                    case XMLStreamConstants.END_ELEMENT:
//                        System.out.println("END_ELEMENT " + xmlStreamReader.getLocalName());
//                        break;
//                    case XMLStreamConstants.CHARACTERS:
//                        System.out.println("CHARACTERS");
//                        break;
//                    default:
//                        System.out.println("event code " + xmlParsingEvent);
//                }
//            }
//        } catch (Exception err) {
//            err.printStackTrace();
//            fail();
//        }
    }
}


