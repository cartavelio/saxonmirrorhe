package s9apitest;

import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XdmNode;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.sax.SAXSource;
import java.io.StringReader;

import static net.sf.saxon.s9api.streams.Steps.*;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

public class TestXdmNode {

    @Test
    public void testToString() throws SaxonApiException {
        Processor processor = new Processor(false);

        String xml = "<document><?foo  bar?><p a='3&amp;4'>A &lt;paragraph&gt;</p> some &lt; text ";
        xml += "<!--comment-->";
        xml += "<documentation xmlns='http://www.w3.org/ns/xproc'>some doc</documentation></document>";

        DocumentBuilder builder = processor.newDocumentBuilder();
        XdmNode doc = builder.build(new SAXSource(new InputSource(new StringReader(xml))));

        assertTrue(doc.toString().startsWith("<document>"), "doc1a");
        assertTrue(doc.toString().endsWith("</document>"), "doc1b");
        assertTrue(doc.toString().contains("> some &lt; text <"), "doc2");

        XdmNode root = doc.select(child().first()).asNode();
        assertTrue(root.toString().startsWith("<document>"), "root1a");
        assertTrue(root.toString().endsWith("</document>"), "root1b");
        assertTrue(root.toString().contains("> some &lt; text <"), "root2");

        XdmNode foo = root.select(child().first()).asNode();
        assertEquals("<?foo bar?>", foo.toString(), "pi");

        XdmNode para = foo.select(followingSibling().first()).asNode();
        assertEquals("<p a=\"3&amp;4\">A &lt;paragraph&gt;</p>", para.toString(), "para");

        XdmNode att = para.select(attribute("a")).asNode();
        assertEquals("a=\"3&amp;4\"", att.toString(), "att");

        XdmNode text = para.select(followingSibling().first()).asNode();
        assertEquals(" some &lt; text ", text.toString(), "text");

        XdmNode comment = text.select(followingSibling().first()).asNode();
        assertEquals("<!--comment-->", comment.toString(), "comment");


    }
}
