package s9apitest;

import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.s9api.*;
import net.sf.saxon.s9api.streams.Predicates;
import net.sf.saxon.s9api.streams.Steps;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

import static net.sf.saxon.s9api.streams.Steps.attribute;
import static net.sf.saxon.s9api.streams.Steps.child;
import static net.sf.saxon.s9api.streams.Steps.followingSibling;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertNull;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

public class TestXdmNode {

    @Test
    public void testToString() throws SaxonApiException {
        Processor processor = new Processor(false);

        String xml = "<document><?foo  bar?><p a='3&amp;4'>A &lt;paragraph&gt;</p> some &lt; text ";
        xml += "<!--comment-->";
        xml += "<documentation xmlns='http://www.w3.org/ns/xproc'>some doc</documentation></document>";

        DocumentBuilder builder = processor.newDocumentBuilder();
        XdmNode doc = builder.build(new SAXSource(new InputSource(new StringReader(xml))));

        String output = doc.toString();
        assertTrue(output.startsWith("<document>"), "doc1a");
        assertTrue(output.endsWith("</document>"), "doc1b");
        assertTrue(output.contains("> some &lt; text <"), "doc2");
        assertTrue(output.contains(">\n "), "indented");

        XdmNode root = doc.select(child()).firstItem();
        output = root.toString();
        assertTrue(output.startsWith("<document>"), "root1a");
        assertTrue(output.endsWith("</document>"), "root1b");
        assertTrue(output.contains("> some &lt; text <"), "root2");
        assertTrue(output.contains(">\n "), "indented");

        XdmNode foo = root.select(child()).firstItem();
        assertEquals("<?foo bar?>", foo.toString(), "pi");

        XdmNode para = foo.select(followingSibling()).firstItem();
        assertEquals("<p a=\"3&amp;4\">A &lt;paragraph&gt;</p>", para.toString(), "para");

        XdmNode att = para.select(attribute("a")).asNode();
        assertEquals("a=\"3&amp;4\"", att.toString(), "att");

        XdmNode text = para.select(followingSibling()).firstItem();
        assertEquals(" some &lt; text ", text.toString(), "text");

        XdmNode comment = text.select(followingSibling()).firstItem();
        assertEquals("<!--comment-->", comment.toString(), "comment");


    }

    @Test
    public void TestElementProperties() {
        try {
            Processor proc = new Processor(false);
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode doc = builder.build(new StreamSource(new StringReader("<in xmlns='http://ns.com/' foo='bar'><baz/></in>")));
            XdmNode element = doc.select(child()).firstItem();
            assertEquals("in", element.getNodeName().getLocalName());
            assertEquals(NamespaceUri.of("http://ns.com/"), element.getNodeName().getNamespaceUri());
            assertEquals("", element.getNodeName().getPrefix());
            assertEquals(XdmNodeKind.ELEMENT, element.getNodeKind());
            assertEquals(new QName("xs", NamespaceConstant.SCHEMA, "untyped"), element.getTypeAnnotationName());
            assertEquals("", element.getStringValue());
            assertEquals(doc, element.getParent());
            assertEquals(doc, element.getRoot());
            assertFalse(element.isAtomicValue());
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void TestChildren() {
        try {
            Processor proc = new Processor(false);
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode doc = builder.build(new StreamSource(new StringReader("<in xmlns='http://ns.com/' foo='bar'><baz/></in>")));
            assertEquals(1, count(doc.children()));
            assertEquals(1, count(doc.children("in")));
            assertEquals(1, count(doc.children("http://ns.com/", "in")));
            assertEquals(0, count(doc.children("out")));
            XdmNode element = first(doc.children());
            assertEquals(1, count(element.children()));
            assertEquals(1, count(element.children("baz")));
            assertEquals(1, count(element.children("http://ns.com/", "baz")));
            assertEquals(0, count(element.children("", "baz")));
            assertEquals(0, count(element.children(Predicates.isText())));
            assertEquals(1, count(element.children(Predicates.isElement())));
            XdmNode baz = first(element.children());
            assertEquals(0, count(baz.children()));
            assertEquals(0, count(baz.children("baz")));
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void TestOutermostElement() {
        try {
            Processor proc = new Processor(false);
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode doc = builder.build(new StreamSource(new StringReader("<in xmlns='http://ns.com/' foo='bar'><baz/></in>")));
            assertEquals("in", doc.getOutermostElement().getNodeName().getLocalName());
            XdmNode el1 = doc.getOutermostElement();
            assertEquals("in", el1.getOutermostElement().getNodeName().getLocalName());
            XdmNode baz = el1.select(child("baz")).firstItem();
            assertEquals("in", baz.getOutermostElement().getNodeName().getLocalName());
            XdmNode foo = el1.select(attribute("foo")).firstItem();
            assertEquals("in", foo.getOutermostElement().getNodeName().getLocalName());
        } catch (SaxonApiException e) {
            fail();
        }
    }

    private static int count(Iterable<? extends XdmItem> seq) {
        int counter=0;
        for (XdmItem item : seq) {
            counter++;
        }
        return counter;
    }

    private static <T> T first(Iterable<T> seq) {
        for (T item : seq) {
            return item;
        }
        return null;
    }


    @Test
    public void TestChildrenRepeatable() {
        try {
            // Children() returns an Enumerable so we need to be able to process it more than once...
            Processor proc = new Processor(false);
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode doc = builder.build(new StreamSource(new StringReader("<in><foo/><foo/><foo/></in>")));
            XdmNode element = doc.children().iterator().next();
            Iterable<XdmNode> grandkids = element.children();
            assertEquals(3, count(grandkids));
            assertEquals(3, count(grandkids));
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void TestAttributes() {
        try {
            Processor proc = new Processor(false);
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode doc =
                    builder.build(
                            new StreamSource(new StringReader("<in xmlns='http://ns.com/' foo='bar' xml:space='preserve'><baz/></in>")));
            XdmNode element = first(doc.children());
            assertEquals("bar", element.attribute("foo"));
            assertEquals("bar", element.getAttributeValue(new QName("foo")));
            assertEquals("preserve", element.getAttributeValue(new QName(NamespaceConstant.XML, "space")));
            assertNull(element.attribute("pooh"));
        } catch (Exception e) {
            fail();
        }
    }

    @Test
    public void TestAxisSteps() {
        try {
            Processor proc = new Processor(false);
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode doc =
                    builder.build(
                            new StreamSource(new StringReader("<one foo='bar'>[<two xmlns='http://ns.com/'><three/></two>]</one>")));
            assertEquals(XdmNodeKind.DOCUMENT, doc.getNodeKind());
            XdmNode one = doc.select(Steps.child("one")).firstItem();
            assertNotNull(one);
            assertEquals(XdmNodeKind.ELEMENT, one.getNodeKind());
            XdmNode doc2 = one.select(Steps.parent((Predicates.isDocument()))).firstItem();
            assertNotNull(doc2);
            assertTrue(doc.equals(doc2)); // For some reason, assertEquals fails with stack overflow
            assertEquals("untyped", doc.getTypeAnnotationName().getLocalName());
            XdmNode doc3 = one.select(Steps.ancestor()).firstItem();
            assertNotNull(doc3);
            assertTrue(doc.equals(doc3));
            XdmNode three = doc.select(Steps.descendant("three")).lastItem();
            assertNotNull(three);
            assertEquals("three", three.getNodeName().getLocalName());
            XdmNode openBra = three.select(Steps.preceding(Predicates.isText())).firstItem();
            assertNotNull(openBra);
            assertEquals("[", openBra.getStringValue());
            assertEquals(XdmNodeKind.TEXT, openBra.getNodeKind());
            assertNull(openBra.getTypeAnnotationName());
            XdmNode closeBra = three.select(Steps.following(Predicates.isText())).firstItem();
            assertNotNull(closeBra);
            assertEquals("]", closeBra.getStringValue());
            assertEquals(XdmNodeKind.TEXT, closeBra.getNodeKind());
            XdmNode foo = one.select(Steps.attribute("foo")).firstItem();
            assertEquals(XdmNodeKind.ATTRIBUTE, foo.getNodeKind());
            assertEquals(new QName("foo"), foo.getNodeName());
            assertEquals("bar", foo.getStringValue());
            assertEquals(new QName("xs", NamespaceConstant.SCHEMA, "untypedAtomic"), foo.getTypeAnnotationName());
        } catch (Exception e) {
            fail();
        }
    }

    @Test
    public void TestMultipleSteps() {
        try {
            Processor proc = new Processor(false);
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode doc =
                    builder.build(
                            new StreamSource(new StringReader("<one foo='bar'>[<two xmlns='http://ns.com/'><three/></two>]</one>")));
            XdmNode grandKid = doc.select(Steps.child("one").then(Steps.child("two"))).firstItem();
            assertEquals("two", grandKid.getNodeName().getLocalName());
            assertEquals(NamespaceUri.of("http://ns.com/"), grandKid.getNodeName().getNamespaceUri());
            String foo = grandKid.select(Steps.parent().then(Steps.attribute("foo"))).firstItem().getStringValue();
            assertEquals("bar", foo);
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void TestPath() {
        try {
            Processor proc = new Processor(false);
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode doc =
                    builder.build(
                            new StreamSource(new StringReader("<one foo='bar'>[<two xmlns='http://ns.com/'><three/></two>]</one>")));
            XdmNode three = doc.select(Steps.path("one", "two", "three")).firstItem();
            assertEquals("three", three.getNodeName().getLocalName());
            XdmNode three2 = doc.select(Steps.path("one", "//", "three")).firstItem();
            assertEquals("three", three2.getNodeName().getLocalName());
            XdmNode foo = doc.select(Steps.path("one", "@foo")).firstItem();
            assertEquals("bar", foo.getStringValue());
        } catch (SaxonApiException e) {
            fail();
        }
    }

}
