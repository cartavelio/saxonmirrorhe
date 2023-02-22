package modeltests;

import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XdmNode;
import org.junit.Test;

import static net.sf.saxon.s9api.streams.Predicates.hasLocalName;
import static net.sf.saxon.s9api.streams.Predicates.hasName;
import static net.sf.saxon.s9api.streams.Predicates.isComment;
import static net.sf.saxon.s9api.streams.Predicates.isElement;
import static net.sf.saxon.s9api.streams.Predicates.isProcessingInstruction;
import static net.sf.saxon.s9api.streams.Predicates.isText;
import static net.sf.saxon.s9api.streams.Steps.attribute;
import static net.sf.saxon.s9api.streams.Steps.child;
import static net.sf.saxon.s9api.streams.Steps.descendant;
import static org.junit.Assert.assertEquals;

/**
 * This is an abstract class containing tests designed to exercise common functionality against
 * different supported object models (that is, implementations of the NodeInfo interface).
 * Subclasses are defined for each specific object model.
 */

public abstract class ObjectModelTests {

    abstract XdmNode buildExternalDocument(Processor processor, String fileName);

    private int count(Iterable<XdmNode> collection) {
        int c = 0;
        for (XdmNode node : collection) {
            c++;
        }
        return c;
    }

    @Test
    public void testChildAxis1() {
        Processor proc = new Processor(true);
        XdmNode doc = buildExternalDocument(proc, "books.xml");
        assertEquals(2, count(doc.children()));
        assertEquals(1, count(doc.children(isProcessingInstruction())));
        assertEquals(1, count(doc.children("BOOKLIST")));
        XdmNode booklist = doc.select(child("BOOKLIST")).asNode();
        assertEquals(2, count(booklist.children("*")));
        XdmNode books = booklist.select(child("BOOKS")).asNode();
        assertEquals(6, count(books.children("*")));
        assertEquals(13, count(books.children()));
    }

    @Test
    public void testChildAxis2() {
        Processor proc = new Processor(true);
        XdmNode doc = buildExternalDocument(proc, "books-sql.xsl");
        assertEquals(3, count(doc.children()));
        assertEquals(1, count(doc.children(isElement())));
        assertEquals(0, count(doc.children(isText())));
        assertEquals(2, count(doc.children(isComment())));
        assertEquals(1, count(doc.children(NamespaceConstant.XSLT, "stylesheet")));
        assertEquals(1, count(doc.children("stylesheet")));
    }

    @Test
    public void testAttributeAxis() {
        Processor proc = new Processor(true);
        XdmNode doc = buildExternalDocument(proc, "books.xml");
        XdmNode booklist = doc.select(child("BOOKLIST")).asNode();
        assertEquals(1, booklist.select(attribute()).count());
        assertEquals(1, booklist.select(attribute(hasLocalName("noNamespaceSchemaLocation"))).count());
        assertEquals(1, booklist.select(attribute(hasName(NamespaceConstant.SCHEMA_INSTANCE, "noNamespaceSchemaLocation"))).count());
        assertEquals(6, booklist.select(descendant("ITEM").then(attribute("CAT"))).count());
        assertEquals(6, booklist.select(descendant("ITEM").then(attribute("", "CAT"))).count());
    }
}

