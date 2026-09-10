package xqjtests;

import com.saxonica.xqj.pull.TreeWalker;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.pull.PullProvider;
import net.sf.saxon.pull.PullPushCopier;
import net.sf.saxon.serialize.SerializationProperties;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.tiny.TinyNodeImpl;
import net.sf.saxon.tree.tiny.TinyTree;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.io.StringWriter;
import java.util.Properties;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

public class TestTreeWalker {
    @Test
    public void testTreeWalker1a() {
        tryTreeWalker("<a x=\"1\"/>", true);
    }

    @Test
    public void testTreeWalker1b() {
        tryTreeWalker("<a x=\"1\"/>", false);
    }

    @Test
    public void testTreeWalker2a() {
        tryTreeWalker("<a><b/></a>", true);
    }

    @Test
    public void testTreeWalker2b() {
        tryTreeWalker("<a><b/></a>", false);
    }

    @Test
    public void testTreeWalker3a() {
        tryTreeWalker("<a>xx<b>yy</b>zz</a>", true);
    }

    @Test
    public void testTreeWalker3b() {
        tryTreeWalker("<a>xx<b>yy</b>zz</a>", false);
    }

    @Test
    public void testTreeWalker4a() {
        tryTreeWalker("<a>xx<b>yy</b><b>yy</b>zz</a>", true);
    }

    @Test
    public void testTreeWalker4b() {
        tryTreeWalker("<a>xx<b>yy</b><b>yy</b>zz</a>", false);
    }

    @Test
    public void testTreeWalker5a() {
        tryTreeWalker("<a>xx<b>yy</b><b z=\"1\">yy</b>zz</a>", true);
    }

    @Test
    public void testTreeWalker5b() {
        tryTreeWalker("<a>xx<b>yy</b><b z=\"1\">yy</b>zz</a>", false);
    }

    @Test
    public void testTreeWalker6a() {
        tryTreeWalker("<?pi?><a><!--xx--><b>yy</b><b>yy</b>zz</a><!--qq--><?pi?>", true);
    }

    @Test
    public void testTreeWalker7a() {
        tryTreeWalker("<a>xx<b>yy</b><b xmlns=\"bbb\">yy</b>zz</a>", true);
    }

    @Test
    public void testTreeWalker7b() {
        tryTreeWalker("<a>xx<b>yy</b><b xmlns=\"bbb\">yy</b>zz</a>", false);
    }

    // Test round-tripping of document via TinyTreeWalker followed by re-serialization

    private void tryTreeWalker(String xml, boolean root) {
        try {
            Configuration config = new Configuration();
            TinyTree tree = (TinyTree) config.buildDocumentTree(new StreamSource(new StringReader(xml)));
            TinyNodeImpl rootNode = tree.getNode(root ? 0 : 1);
            PullProvider walker = TreeWalker.makeTreeWalker(rootNode);
            walker.setPipelineConfiguration(config.makePipelineConfiguration());
            StringWriter sw = new StringWriter();
            StreamResult result = new StreamResult(sw);
            Properties p = new Properties();
            p.setProperty("omit-xml-declaration", "yes");
            SerializationProperties props = new SerializationProperties(p);
            Receiver out = config.getSerializerFactory().getReceiver(result, props);
            PullPushCopier copier = new PullPushCopier(walker, out);
            copier.copy();
            assertEquals(xml, sw.toString(), "result");
        } catch (XPathException e) {
            fail();
        }
    }

}

