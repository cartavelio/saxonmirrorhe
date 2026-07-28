package s9apitest;

import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.s9api.*;
import net.sf.saxon.sapling.SaplingDocument;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Source;
import java.io.StringWriter;

import static net.sf.saxon.sapling.Saplings.*;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

public class TestSapling {

    @Test
    public void testSimpleBuild() {
        try {
            Processor processor = new Processor(false);
            SaplingDocument doc = doc().withChild(elem("root").withChild(text("data")));
            String s = processor.newSerializer().serializeToString(doc);
            assertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>data</root>", s, "result");
        } catch (SaxonApiException e) {
            e.printStackTrace();
        }
    }

    @Test
    public void testMultipleChildren() {
        try {
            Processor processor = new Processor(false);
            StringWriter sw = new StringWriter();
            Serializer serializer = processor.newSerializer(sw);
            serializer.setOutputProperty(Serializer.Property.INDENT, "no");
            serializer.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            SaplingDocument doc = doc().withChild(elem("root").withChild(elem("p").withText("Hello"), elem("p").withText("World")));
            XdmNode xdoc = doc.toXdmNode(processor);
            String s = serializer.serializeNodeToString(xdoc);
            assertEquals("<root><p>Hello</p><p>World</p></root>", s, "result");
        } catch (SaxonApiException e) {
            e.printStackTrace();
        }
    }

    @Test
    public void testDirectSerialization() {
        try {
            Processor processor = new Processor(false);
            StringWriter sw = new StringWriter();
            Serializer serializer = processor.newSerializer(sw);
            serializer.setOutputProperty(Serializer.Property.INDENT, "no");
            serializer.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            SaplingDocument doc = doc().withChild(elem("root").withChild(elem("p").withText("Hello"), elem("p").withText("World")));
            String s = serializer.serializeToString(doc);
            assertEquals("<root><p>Hello</p><p>World</p></root>", s, "result");
        } catch (SaxonApiException e) {
            e.printStackTrace();
        }
    }

    @Test
    public void testSerializationToWriter() {
        try {
            Processor processor = new Processor(false);
            StringWriter sw = new StringWriter();
            Serializer serializer = processor.newSerializer(sw);
            serializer.setOutputProperty(Serializer.Property.INDENT, "no");
            serializer.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            serializer.setOutputWriter(sw);
            SaplingDocument doc = doc().withChild(elem("root").withChild(elem("p").withText("Hello"), elem("p").withText("World")));
            serializer.serialize(doc);
            assertEquals("<root><p>Hello</p><p>World</p></root>", sw.toString(), "result");
        } catch (SaxonApiException e) {
            e.printStackTrace();
        }
    }

    @Test
    public void testWithNamespaces() {
        try {
            Processor processor = new Processor(false);
            SaplingDocument doc = doc().withChild(elem(new QName("", "http://ns/", "root")).withChild(text("data")).withNamespace("foo", "http://foo/"));
            XdmNode xdoc = doc.toXdmNode(processor);
            String s = xdoc.toString();
            assertEquals("<root xmlns=\"http://ns/\" xmlns:foo=\"http://foo/\">data</root>", s, "result");
        } catch (SaxonApiException e) {
            e.printStackTrace();
        }
    }

    @Test
    public void testWithNamespaceConflict() {
        try {
            doc().withChild(elem(new QName("p", "http://ns/", "root")).withChild(text("data")).withNamespace("p", "http://foo/"));
            fail("Unexpected success");
        } catch (IllegalStateException e) {
            assertTrue(e.getMessage().contains("Inconsistent namespace bindings"));
        }
    }

    @Test
    public void testWithNamespaceConflictOnAttributes() {
        try {
            doc().withChild(elem(new QName("p", "http://ns/", "root")).withAttr(new QName("p", "http://foo/", "local"), "value"));
            fail("Unexpected success");
        } catch (IllegalStateException e) {
            assertTrue(e.getMessage().contains("Inconsistent namespace bindings"));
        }
    }

    @Test
    public void testWithUnprefixedAttributes() {
        try {
            doc().withChild(elem("root").withAttr(new QName("", "http://foo/", "local"), "value"));
            fail("Unexpected success");
        } catch (IllegalArgumentException e) {
            assertTrue(e.getMessage().contains("must have a prefix"));
        }
    }

    @Test
    public void testWithBadAttributes() {
        try {
            doc().withChild(elem("root").withAttr(new QName("p", "", "local"), "value"));
            fail("Unexpected success");
        } catch (IllegalArgumentException e) {
            //System.err.println(e.getMessage());
            assertTrue(e.getMessage().contains("Cannot bind non-empty prefix to empty URI"));
        }
    }

    /**
     * Test use of a sapling tree supplied directly as input where a stylesheet is expected
     * (it's actually a literal-result-element-as-stylesheet)
     */
    @Test
    public void testTransformation() {
        try {
            Source xml = doc().withChild(elem("x").withText("23"));
            Source xsl = doc("http://dummy/")
                    .withChild(elem("out")
                        .withAttr(new QName("xsl", NamespaceConstant.XSLT, "version"), "3.0")
                        .withChild(elem(new QName("xsl", NamespaceConstant.XSLT, "value-of"))
                            .withAttr("select", "/x")));

            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.compile(xsl).load30().transform(xml, proc.newSerializer(System.out));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }
}

