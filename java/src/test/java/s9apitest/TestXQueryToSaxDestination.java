package s9apitest;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;
import org.xml.sax.Attributes;
import org.xml.sax.InputSource;
import org.xml.sax.Locator;
import org.xml.sax.SAXException;

import javax.xml.transform.sax.SAXSource;
import java.io.StringReader;
import java.net.URI;

import static org.junit.jupiter.api.Assertions.assertEquals;

/**
 * XQuery executor sample class.
 */
public class TestXQueryToSaxDestination {

    static class ContentHandlerAdapter implements org.xml.sax.ContentHandler {

        private Locator locator;

        @Override
        public void setDocumentLocator(Locator locator) {
            this.locator = locator;

        }

        @Override
        public void characters(char[] ch, int start, int length) throws SAXException {
            String text = new String(ch, start, length);
            if (text.equals("Big")) {
                assertEquals("file://my.xqy", locator.getSystemId());
                assertEquals(6, locator.getLineNumber());
            } else if (text.equals("Boss")) {
                assertEquals("file://my.xqy", locator.getSystemId());
                assertEquals(6, locator.getLineNumber());
            } else if (text.equals(" - ")) {
                assertEquals("file://my.xqy", locator.getSystemId());
                assertEquals(6, locator.getLineNumber());
            }
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
        }

        @Override
        public void endElement(String uri, String localName, String qName) throws SAXException {
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
    }

    @Test
    public void testQueryToSaxDestination() throws Exception {
        try {
            Processor proc = new Processor(false);
            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.setBaseURI(new URI("file://my.xqy"));
            XQueryExecutable exp = comp.compile(
                            "<persons>\n" +
                                    "    {\n" +
                                    "        for $person in /personnel/person\n" +
                                    "        return\n" +
                                    "            <person id=\"{$person/@id}\">\n" +
                                    "                <name>{$person/name/given/text(), \" - \", $person/name/family/text()}</name>\n" +
                                    "            </person>\n" +
                                    "    }\n" +
                                    "</persons>");

            SAXDestination result = new SAXDestination(new ContentHandlerAdapter());
            System.out.println("Run query...");

            XQueryEvaluator eval = exp.load();

            XdmNode document = proc.newDocumentBuilder().build(new SAXSource(new InputSource(new StringReader(
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<personnel>\n" +
                            "    <person id=\"Big.Boss\">\n" +
                            "        <name>\n" +
                            "            <family>Boss</family>\n" +
                            "            <given>Big</given>\n" +
                            "        </name>\n" +
                            "    </person>\n" +
                            "</personnel>\n"))));

            eval.setContextItem(document);
            eval.run(result);

            System.out.println("Done.");
        } catch (Exception e) {
            System.out.println("Transformation failed: " + e);
            e.printStackTrace();
        }
    }

    @Test
    public void testQueryToSaxResult() {
        try {
            Processor proc = new Processor(false);

            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.setBaseURI(new URI("file://my.xqy"));

            XQueryExecutable queryExpression =
                    comp.compile(new StringReader(
                            """
                               <persons> {
                                    for $person in /personnel/person
                                    return
                                        <person id="{$person/@id}">
                                            <name>{$person/name/given/text(), " - ", $person/name/family/text()}</name>
                                        </person>
                                } </persons>
                            """));

            SAXDestination result = new SAXDestination(new ContentHandlerAdapter());
            System.out.println("Run query...");


            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setLineNumbering(true);
            XdmNode document = builder.build(new SAXSource(new InputSource(new StringReader(
                    """
                            <?xml version="1.0" encoding="UTF-8"?>
                            <personnel>
                                <person id="Big.Boss">
                                    <name>
                                        <family>Boss</family>
                                        <given>Big</given>
                                    </name>
                                </person>
                            </personnel>"""))));

            XQueryEvaluator eval = queryExpression.load();
            eval.setContextItem(document);

            eval.run(result);

            System.out.println("Done.");
        } catch (Exception e) {
            System.out.println("Query failed: " + e);
            e.printStackTrace();
        }
    }
}
