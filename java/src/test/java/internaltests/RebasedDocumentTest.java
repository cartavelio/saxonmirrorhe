package internaltests;

import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.TreeInfo;
import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XdmNode;
import net.sf.saxon.tree.tiny.TinyDocumentImpl;
import net.sf.saxon.tree.wrapper.RebasedDocument;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.sax.SAXSource;
import java.io.StringReader;
import java.net.URI;
import java.util.function.Function;

import static net.sf.saxon.s9api.streams.Steps.descendant;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 *  Tests of the virtual rebased document
 */
public class RebasedDocumentTest {

    @Test
    public void testSimpleRebase() {
        try {
            StringReader sr = new StringReader("<doc><chap><p n='1'/><p n='2'/></chap><chap><p n='3'/><p n='4'/></chap></doc>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = new Processor(false).newDocumentBuilder();
            builder.setBaseURI(new URI("http://saxonica.com/"));
            XdmNode d = builder.build(source);
            TinyDocumentImpl doc = (TinyDocumentImpl) d.getUnderlyingNode();
            TreeInfo tree = doc.getTree();
            RebasedDocument tree2 = new RebasedDocument(tree, s -> "http://saxon.sf.net/" , s -> "http://dev.saxonica.com/");
            XdmNode d2 = new XdmNode(tree2.getRootNode());
            assertEquals("http://saxon.sf.net/", d2.getBaseURI().toASCIIString());
            assertEquals("http://dev.saxonica.com/", d2.getUnderlyingNode().getSystemId());
            XdmNode p2 = d2.select(descendant("p")).findFirst().get();
            assertEquals("http://saxon.sf.net/", p2.getBaseURI().toASCIIString());
            assertEquals("http://dev.saxonica.com/", p2.getUnderlyingNode().getSystemId());
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testRebaseRelative() {
        try {
            StringReader sr = new StringReader("<doc><chap><p n='1'/><p n='2'/></chap><chap><p n='3'/><q xml:base='sub/' n='4'/></chap></doc>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = new Processor(false).newDocumentBuilder();
            builder.setBaseURI(new URI("http://saxonica.com/"));
            XdmNode d = builder.build(source);
            TinyDocumentImpl doc = (TinyDocumentImpl) d.getUnderlyingNode();
            TreeInfo tree = doc.getTree();
            Function<NodeInfo, String> rebase = n -> {
                String s = n.getBaseURI();
                if (s.startsWith("http://saxonica.com/")) {
                    String suffix = s.substring("http://saxonica.com/".length());
                    return "http://saxon.sf.net/" + suffix;
                } else {
                    return s;
                }
            };
            RebasedDocument tree2 = new RebasedDocument(tree, rebase, rebase);
            XdmNode d2 = new XdmNode(tree2.getRootNode());
            assertEquals("http://saxon.sf.net/", d2.getBaseURI().toASCIIString());
            XdmNode p2 = d2.select(descendant("q")).findFirst().get();
            assertEquals("http://saxon.sf.net/sub/", p2.getBaseURI().toASCIIString());
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

}

