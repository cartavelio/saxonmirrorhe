package s9apitest;

import jaxptest.ConfigTest;
import junit.framework.TestCase;
import net.sf.saxon.event.Builder;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.om.TreeModel;
import net.sf.saxon.s9api.*;

import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.Iterator;

import static net.sf.saxon.s9api.streams.Steps.child;
import static net.sf.saxon.s9api.streams.Steps.descendant;

/**
 * JUnit tests for XQuery Update using the s9api XQueryCompiler
 */
public class TestXQueryUpdate extends TestCase {

    public void testUpdate() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.TREE_MODEL, Builder.LINKED_TREE);
            proc.setConfigurationProperty(Feature.TREE_MODEL_NAME, "linkedTree");
            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.setBaseURI(new URI("file:///" + ConfigTest.DATA_DIR));
            comp.setUpdatingEnabled(true);
            XQueryExecutable exp =
                comp.compile("for $p in doc('books.xml')//PRICE "
                        + "return replace value of node $p with ($p + 0.05)");

            XQueryEvaluator eval = exp.load();
            eval.run();
            for (Iterator<XdmNode> iter = eval.getUpdatedDocuments(); iter.hasNext();) {
                XdmNode root = iter.next();
                URI rootUri = root.getDocumentURI();
                if (rootUri != null && rootUri.getScheme().equals("file")) {
                    Serializer out = proc.newSerializer();
                    out.setOutputProperty(Serializer.Property.METHOD, "xml");
                    out.setOutputProperty(Serializer.Property.INDENT, "yes");
                    out.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
                    try {
                        System.err.println("Rewriting " + rootUri);
                        out.setOutputStream(new FileOutputStream(new File(rootUri)));
                        proc.writeXdmValue(root, out);
                    }
                    catch (FileNotFoundException e) {
                        System.err.println("Could not write to file " + rootUri);
                    }
                }
                else {
                    System.err
                        .println("Updated document not rewritten: location unknown or not updatable");
                }
            }

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        } catch (URISyntaxException e) {
            fail(e.getMessage());
        }
    }

    public void testCopyModify() {
        try {
            Processor proc = new Processor(true);
            //proc.setConfigurationProperty(FeatureKeys.TREE_MODEL, Builder.LINKED_TREE);
            //proc.setConfigurationProperty(FeatureKeys.TREE_MODEL_NAME, "linkedTree");
            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setTreeModel(TreeModel.LINKED_TREE);
            XdmNode input = builder.build(new File(ConfigTest.DATA_DIR + "books.xml"));
            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.setBaseURI(new URI("file:///" + ConfigTest.DATA_DIR));
            comp.setUpdatingEnabled(true);
            XQueryExecutable exp =
                comp.compile("copy $doc := .\n" +
                        "     modify rename node ($doc//PRICE) [1]\n " +
                        "            as QName('', 'NEW-PRICE')\n" +
                        "     return $doc\n");

            XQueryEvaluator eval = exp.load();
            eval.setContextItem(input);
            Serializer ser = proc.newSerializer();
            StringWriter writer = new StringWriter();
            ser.setOutputWriter(writer);
            eval.setDestination(ser);
            eval.run();
            System.err.println(writer);
            assertTrue("renamed", writer.toString().contains("NEW-PRICE"));

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        } catch (URISyntaxException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test that after deletion of attribute nodes, or the content of element nodes,
     * the revalidated content contains expanded default values.
     */

    public void testRevalidationWithDefaults() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.TREE_MODEL, Builder.LINKED_TREE);
            proc.setConfigurationProperty(Feature.TREE_MODEL_NAME, "linkedTree");
            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.setBaseURI(new URI("file:///" + ConfigTest.DATA_DIR));
            comp.setUpdatingEnabled(true);
            comp.setSchemaAware(true);

            String xml = "<e a='17'>content</e>";
            String xsd = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                    "<xs:element name='e' default='DEFAULT'>" +
                    "<xs:complexType><xs:simpleContent><xs:extension base='xs:string'>" +
                    "<xs:attribute name='a' type='xs:integer' default='42'/></xs:extension></xs:simpleContent></xs:complexType>" +
                    "</xs:element></xs:schema>";

            SchemaManager manager = proc.getSchemaManager();
            manager.load(new StreamSource(new StringReader(xsd)));
            SchemaValidator validator = manager.newSchemaValidator();
            XdmDestination docDest = new XdmDestination();
            docDest.setTreeModel(TreeModel.LINKED_TREE);
            validator.setDestination(docDest);
            validator.validate(new StreamSource(new StringReader(xml)));
            XQueryExecutable exp =
                    comp.compile("declare revalidation strict; replace value of node e with '', delete node e/@a");

            XQueryEvaluator eval = exp.load();
            eval.setContextItem(docDest.getXdmNode());
            eval.run();

            XdmNode newDoc = eval.getUpdatedDocuments().next();
            XdmNode newElem = newDoc.select(child("e")).asNode();
            System.err.println(newElem);
            assertEquals("DEFAULT", newElem.getStringValue());
            assertEquals("42", newElem.getAttributeValue(new QName("a")));

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        } catch (URISyntaxException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test that after an update to a document, stale indexes are not used
     */

    public void testIndexesCleared() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.TREE_MODEL, Builder.LINKED_TREE);

            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setTreeModel(TreeModel.LINKED_TREE);
            XdmNode doc = builder.build(new File (ConfigTest.DATA_DIR + "books.xml"));

            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.setBaseURI(new URI("file:///" + ConfigTest.DATA_DIR));
            comp.setUpdatingEnabled(true);

            XQueryExecutable exp1 =
                    comp.compile("for $p in //ITEM[AUTHOR='Jasper Fforde']"
                                         + "return (replace value of node $p/AUTHOR with 'Sir Jasper Fforde',"
                                         + "        insert node <marker/> as last into $p)");

            XQueryEvaluator eval = exp1.load();
            eval.setContextItem(doc);
            eval.run();

            // Run the same query again
            XdmNode newDoc = eval.getUpdatedDocuments().next();
            eval.setContextItem(newDoc);
            eval.run();

            // Check that there is at most one marker element on an item
            Iterator<XdmNode> updatedDocs = eval.getUpdatedDocuments();
            XdmNode newDoc2 = updatedDocs.hasNext() ? updatedDocs.next() : newDoc;

            newDoc2.select(descendant("ITEM")).forEach(node -> {
                assertTrue(node.select(child("marker")).count() < 2);
            });


        } catch (SaxonApiException e) {
            fail(e.getMessage());
        } catch (URISyntaxException e) {
            fail(e.getMessage());
        }
    }

}