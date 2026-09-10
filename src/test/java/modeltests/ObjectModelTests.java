package modeltests;

import jaxptest.ConfigTest;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.TreeModel;
import net.sf.saxon.s9api.*;
import org.junit.Test;

import java.io.File;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;

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
import static org.junit.Assert.assertTrue;

/**
 * This is an abstract class containing tests designed to exercise common functionality against
 * different supported object models (that is, implementations of the NodeInfo interface).
 * Subclasses are defined for each specific object model.
 */

public abstract class ObjectModelTests {
    protected final ConfigTest configTest = new ConfigTest();
    
    abstract TreeModel getTreeModel();

    abstract XdmNode buildExternalDocument(Processor processor, String fileName);

    XdmDestination makeDestination(Processor processor) {
        try {
            XdmDestination dest = new XdmDestination();
            dest.setTreeModel(getTreeModel());
            return dest;
        } catch (Exception e) {
            fail(e);
            return null;
        }
    }

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

    @Test
    public void testDocumentOrder() {
        try {
            Processor proc = new Processor(true);
            XdmNode doc = buildExternalDocument(proc, "books.xml");
            XdmValue selection = XdmEmptySequence.getInstance();
            Object[] items = doc.select(descendant("ITEM")).toArray();
            selection = selection.append((XdmNode) items[4]);
            selection = selection.append((XdmNode) items[2]);
            selection = selection.append((XdmNode) items[1]);
            selection = selection.append((XdmNode) items[5]);
            selection = selection.append((XdmNode) items[3]);
            selection = selection.append((XdmNode) items[1]);
            for (XdmItem n : selection) {
                selection = selection.append(n.select(child("DIMENSIONS").then(attribute("UNIT"))).asXdmValue());
                selection = selection.append(n.select(attribute("CAT")).asXdmValue());
                selection = selection.append(n.select(child("AUTHOR").then(child(isText()))).asXdmValue());
            }
            selection = selection.append(doc);
            selection = selection.documentOrder();
            assertEquals(21, selection.size());
            StringBuilder builder = new StringBuilder();
            for (XdmItem v : selection) {
                builder.append(((XdmNode) v).getNodeKind()).append(':');
                QName qn = ((XdmNode) v).getNodeName();
                builder.append(qn==null ? "~" : qn.getLocalName()).append(':');
                if (!((XdmNode)v).select(child("*")).exists()) {
                    builder.append(((XdmNode) v).getStringValue());
                }
                builder.append('|');
            }
            assertEquals("DOCUMENT:~:|ELEMENT:ITEM:|ATTRIBUTE:CAT:P|TEXT:~:Charlotte Brontë|ATTRIBUTE:UNIT:in|ELEMENT:ITEM:|ATTRIBUTE:CAT:P|TEXT:~:Thomas Hardy|ATTRIBUTE:UNIT:in|ELEMENT:ITEM:|ATTRIBUTE:CAT:P|TEXT:~:Thomas Hardy|ATTRIBUTE:UNIT:in|ELEMENT:ITEM:|ATTRIBUTE:CAT:H|TEXT:~:Jasper Fforde|ATTRIBUTE:UNIT:cm|ELEMENT:ITEM:|ATTRIBUTE:CAT:P|TEXT:~:Jasper Fforde|ATTRIBUTE:UNIT:in|",
                         builder.toString());
        } catch (SaxonApiException e) {
            fail(e);
        }

    }

    @Test
    public void testNamespaces() {
        try {
            Processor proc = new Processor(true);
            XdmNode doc = buildExternalDocument(proc, "books-sql.xsl");
            XdmNode root = doc.children("stylesheet").iterator().next();
            Set<String> bindingsFound = new HashSet<>();
            XdmSequenceIterator<XdmNode> iter = root.axisIterator(Axis.NAMESPACE);
            while (iter.hasNext()) {
                XdmNode ns = iter.next();
                String binding = ns.getNodeName().getLocalName() + "=" + ns.getStringValue();
                bindingsFound.add(binding);
            }
            assertEquals(5, bindingsFound.size());
            assertTrue(bindingsFound.contains("sql=http://saxon.sf.net/sql"));
            assertTrue(bindingsFound.contains("xsl=http://www.w3.org/1999/XSL/Transform"));
            assertTrue(bindingsFound.contains("saxon=http://saxon.sf.net/"));
            assertTrue(bindingsFound.contains("java=http://saxon.sf.net/java-type"));
            assertTrue(bindingsFound.contains("xml=" + NamespaceConstant.XML));
        } catch (Exception e) {
            fail(e);
        }
    }

    @Test
    public void testDestination() {
        try {
            Processor proc = new Processor(true);
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode source = builder.build(new File(configTest.getDataDir() + "books-sql.xsl"));
            XdmDestination dest = makeDestination(proc);
            XQueryCompiler compiler = proc.newXQueryCompiler();
            XQueryExecutable exec = compiler.compile("copy-of(.)");
            XQueryEvaluator eval = exec.load();
            eval.setContextItem(source);
            eval.run(dest);
            XdmFunctionItem deepEq = XdmFunctionItem.getSystemFunction(proc,new QName(NamespaceConstant.FN, "deep-equal"), 2);
            XdmValue comparison = deepEq.call(proc, source, dest.getXdmNode());
            assertTrue(((XdmAtomicValue)comparison).getBooleanValue());
        } catch (Exception e) {
            fail(e);
        }
    }

    @Test
    public void testId() {
        try {
            Processor proc = new Processor(true);
            XdmNode doc = buildExternalDocument(proc, "books-with-dtd.xml");
            XdmFunctionItem id = XdmFunctionItem.getSystemFunction(proc, new QName(NamespaceConstant.FN, "id"), 2);
            XdmValue val = id.call(proc, new XdmAtomicValue("MMP"), doc);
            assertTrue(val instanceof XdmNode);
            assertEquals("Mass-market Paperback", ((XdmNode)val).attribute("DESC"));
        } catch (SaxonApiException e) {
            e.printStackTrace();
        }

    }

    @Test
    public void testXPath() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.TRACE_EXTERNAL_FUNCTIONS, true);
            XdmNode doc = buildExternalDocument(proc, "books.xml");
            XPathCompiler xpc = proc.newXPathCompiler();
            XPathSelector xp = xpc.compile(getExtensionFunctionCall()).load();
            xp.setContextItem(doc);
            XdmValue result = xp.evaluate();
            assertEquals("Jane Austen", ((XdmAtomicValue) result).getStringValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testReflexiveExtension() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.TRACE_EXTERNAL_FUNCTIONS, true);
            XdmNode doc = buildExternalDocument(proc, "books.xml");
            XPathCompiler xpc = proc.newXPathCompiler();
            XPathSelector xp = xpc.compile("string((//AUTHOR)[1])").load();
            xp.setContextItem(doc);
            XdmValue result = xp.evaluate();
            assertEquals("Jane Austen", ((XdmAtomicValue)result).getStringValue() );
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    /**
     * To be implemented in each subclass - supply an XPath extension function call that finds
     * the string value of the first author in the document
     * @return an XPath expression
     */
    public String getExtensionFunctionCall() {
        // default implementation
        return "string((//AUTHOR)[1])";
    }







}

