package s9apitest;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;
import org.xml.sax.SAXException;
import org.xml.sax.helpers.AttributesImpl;

import java.net.URI;
import java.net.URISyntaxException;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;


/**
 * test BuildingStreamWriter in s9api
 */
public class TestBuildingContentHandler {

    @Test
    public void testSimple() {
        try {
            Processor proc = new Processor(false);
            BuildingContentHandler bch = proc.newDocumentBuilder().newBuildingContentHandler();
            bch.startDocument();
            AttributesImpl ai = new AttributesImpl();
            ai.addAttribute("", "data", "data", "CDATA", "3");
            bch.startElement("", "out", "out", ai);
            ai.clear();
            bch.startElement("", "br", "br", ai);
            bch.endElement("", "br", "br");
            ai.addAttribute("", "style", "style", "CDATA", "bold");
            bch.startElement("", "br", "br", ai);
            bch.endElement("", "br", "br");
            bch.endElement("", "out", "out");
            bch.endDocument();
            Serializer sr = proc.newSerializer();
            sr.setOutputProperty(Serializer.Property.INDENT, "no");
            sr.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            System.err.println(sr.serializeNodeToString(bch.getDocumentNode()));
            assertEquals("<out data=\"3\"><br/><br style=\"bold\"/></out>",
                         sr.serializeNodeToString(bch.getDocumentNode())
                                            );
        } catch (SaxonApiException | SAXException e) {
            fail(e.toString());
        }
    }

    @Test
    public void testWithPrefixes() {
        try {
            Processor proc = new Processor(false);
            BuildingContentHandler bch = proc.newDocumentBuilder().newBuildingContentHandler();
            bch.startDocument();
            AttributesImpl ai = new AttributesImpl();
            ai.addAttribute("", "data", "data", "CDATA", "3");
            bch.startPrefixMapping("ns0", "http://outer/");
            bch.startElement("http://outer/", "out", "ns0:out", ai);
            ai.clear();
            bch.startElement("http://outer/", "br", "ns0:br", ai);
            ai.addAttribute("", "style", "style", "CDATA", "bold");
            bch.endElement("http://outer/", "br", "ns1:br");
            bch.startPrefixMapping("ns1", "http://inner/");
            bch.startElement("http://inner/", "br", "ns1:br", ai);
            bch.endElement("http://inner/", "br", "ns1:br");
            bch.endPrefixMapping("ns1");
            bch.endElement("http://outer/", "out", "ns0:out");
            bch.endPrefixMapping("ns0");
            bch.endDocument();
            Serializer sr = proc.newSerializer();
            sr.setOutputProperty(Serializer.Property.INDENT, "no");
            sr.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            //System.err.println(sr.serializeNodeToString(bch.getDocumentNode()));
            assertEquals(
                    "<ns0:out xmlns:ns0=\"http://outer/\" data=\"3\"><ns0:br/><ns1:br xmlns:ns1=\"http://inner/\" style=\"bold\"/></ns0:out>",
                    sr.serializeNodeToString(bch.getDocumentNode()));
        } catch (SaxonApiException e) {
            fail(e.toString());
        } catch (SAXException e) {
            fail(e.toString());
        }
    }

    @Test
    public void testBaseUri() {
        try {
            Processor proc = new Processor(false);
            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setBaseURI(new URI("file:///test.xml/"));
            BuildingContentHandler bch = builder.newBuildingContentHandler();
            bch.startDocument();
            AttributesImpl ai = new AttributesImpl();
            ai.addAttribute("", "data", "data", "CDATA", "3");
            bch.startElement("", "out", "out", ai);
            bch.endElement("", "out", "out");
            bch.endDocument();
            assertEquals("file:///test.xml/", bch.getDocumentNode().getBaseURI().toString());
            Serializer sr = proc.newSerializer();
            sr.setOutputProperty(Serializer.Property.INDENT, "no");
            sr.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            System.err.println(sr.serializeNodeToString(bch.getDocumentNode()));
            assertEquals("<out data=\"3\"/>",
                         sr.serializeNodeToString(bch.getDocumentNode())
            );
        } catch (SaxonApiException e) {
            fail(e.toString());
        } catch (SAXException e) {
            fail(e.toString());
        } catch (URISyntaxException e) {
            fail(e.toString());
        }
    }


}

