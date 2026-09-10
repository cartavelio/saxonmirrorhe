package s9apitest;

import junit.framework.TestCase;
import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XdmNode;
import org.xml.sax.InputSource;

import javax.xml.transform.sax.SAXSource;
import java.io.StringReader;

import static net.sf.saxon.s9api.streams.Steps.*;

public class TestXdmNode extends TestCase {

    public void testToString() throws SaxonApiException {
        Processor processor = new Processor(false);

        String xml = "<document><?foo  bar?><p a='3&amp;4'>A &lt;paragraph&gt;</p> some &lt; text ";
        xml += "<!--comment-->";
        xml += "<documentation xmlns='http://www.w3.org/ns/xproc'>some doc</documentation></document>";

        DocumentBuilder builder = processor.newDocumentBuilder();
        XdmNode doc = builder.build(new SAXSource(new InputSource(new StringReader(xml))));

        assertTrue("doc1a", doc.toString().startsWith("<document>"));
        assertTrue("doc1b", doc.toString().endsWith("</document>"));
        assertTrue("doc2", doc.toString().contains("> some &lt; text <"));

        XdmNode root = doc.select(child().first()).asNode();
        assertTrue("root1a", root.toString().startsWith("<document>"));
        assertTrue("root1b", root.toString().endsWith("</document>"));
        assertTrue("root2", root.toString().contains("> some &lt; text <"));

        XdmNode foo = root.select(child().first()).asNode();
        assertEquals("pi", "<?foo bar?>", foo.toString());

        XdmNode para = foo.select(followingSibling().first()).asNode();
        assertEquals("para", "<p a=\"3&amp;4\">A &lt;paragraph&gt;</p>", para.toString());

        XdmNode att = para.select(attribute("a")).asNode();
        assertEquals("att", "a=\"3&amp;4\"", att.toString());

        XdmNode text = para.select(followingSibling().first()).asNode();
        assertEquals("text", " some &lt; text ", text.toString());

        XdmNode comment = text.select(followingSibling().first()).asNode();
        assertEquals("comment", "<!--comment-->", comment.toString());


    }
}
