package modeltests;

import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.s9api.*;
import org.junit.Test;

import java.util.HashMap;
import java.util.Map;

import static com.ibm.icu.impl.Assert.fail;
import static net.sf.saxon.s9api.streams.Predicates.attributeEq;
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

    /**
     * Ask whether this tree model strips whitespace-only text nodes in element that are
     * defined by the DTD to have an element-only content model
     * @return true if such text nodes are stripped while parsing; false if not.
     */
    boolean stripsIgnorableWhitespace() {
        return false;
    }

    /**
     * Ask whether Saxon's wrapper for this tree model merges adjacent sibling text nodes
     * when found on the external tree
     * @return if Saxon merges such text nodes, or if they are merged automatically during parsing
     * and tree construction.
     */

    boolean mergesAdjacentTextNodes() {
        return true;
    }

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
        XdmNode doc = buildExternalDocument(proc, "books-with-dtd.xml");
        assertEquals(1, count(doc.children()));
        assertEquals(1, count(doc.children("BOOKLIST")));
        XdmNode booklist = doc.select(child("BOOKLIST")).asNode();
        assertEquals(2, count(booklist.children("*")));
        XdmNode books = booklist.select(child("BOOKS")).asNode();
        assertEquals(6, count(books.children("*")));
        assertEquals(stripsIgnorableWhitespace() ? 6 : 13, count(books.children()));
    }

    @Test
    public void testChildAxis3() {
        Processor proc = new Processor(true);
        XdmNode doc = buildExternalDocument(proc, "books-sql.xsl");
        assertEquals(3, count(doc.children()));
        assertEquals(1, count(doc.children(isElement())));
        assertEquals(0, count(doc.children(isText())));
        assertEquals(2, count(doc.children(isComment())));
        assertEquals(1, count(doc.children(NamespaceConstant.XSLT, "stylesheet")));
        assertEquals(1, count(doc.children("stylesheet")));
        XdmNode cDataElement = doc.select(child("stylesheet")
                                           .then(child("template")
                                                         .where(attributeEq("name", "UNUSED")))).asNode();
        assertEquals("(This is CDATA)", cDataElement.getStringValue());
        if (mergesAdjacentTextNodes()) {
            assertEquals(1, count(cDataElement.children()));
            assertEquals("(This is CDATA)", cDataElement.select(child()).first().asNode().getStringValue());
        }
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

    @Test
    public void testGenerateId() {
        // Test that generate-id produces unique IDs and that they have the correct lexical form
        try {
            Processor proc = new Processor(true);
            XdmNode doc = buildExternalDocument(proc, "books-sql.xsl");
            XdmFunctionItem genId = XdmFunctionItem.getSystemFunction(proc, new QName(NamespaceConstant.FN, "generate-id"), 1);
            Map<String, XdmNode> idMap = new HashMap<>();
            doc.select(descendant()).forEach(node -> {
                try {
                    String id = ((XdmAtomicValue)genId.call(proc, node)).getStringValue();
                    if (!id.matches("[A-Za-z][A-Za-z0-9]*")) {
                        fail("Invalid id " + id);
                    }
                    if (idMap.containsKey(id)) {
                        fail("Duplicate id " + id);
                    }
                    idMap.put(id, node);
                    node.select(attribute()).forEach(att -> {
                        try {
                            String attId = ((XdmAtomicValue)genId.call(proc, att)).getStringValue();
                            if (!attId.matches("[A-Za-z][A-Za-z0-9]*")) {
                                fail("Invalid id " + id);
                            }
                            if (idMap.containsKey(attId)) {
                                fail("Duplicate attribute id " + attId);
                            }
                            idMap.put(attId, att);
                        } catch (SaxonApiException e) {
                            fail(e);
                        }
                    });
                } catch (SaxonApiException e) {
                    fail(e);
                }
            });
        } catch (SaxonApiException e) {
            fail(e);
        }
    }
}

