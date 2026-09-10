package modeltests;

import net.sf.saxon.om.TreeModel;
import net.sf.saxon.option.dom4j.DOM4JObjectModel;
import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XdmNode;
import org.dom4j.Document;
import org.dom4j.DocumentException;
import org.dom4j.io.SAXReader;

import java.io.File;

import static org.junit.jupiter.api.Assertions.fail;

public class DOM4JTreeTests extends ObjectModelTests {

    @Override
    TreeModel getTreeModel() {
        return DOM4JObjectModel.getInstance();
    }

    @Override
    XdmNode buildExternalDocument(Processor processor, String fileName) {
        try {
            SAXReader dom4jBuilder = new SAXReader();
            dom4jBuilder.setMergeAdjacentText(true);
            Document doc = dom4jBuilder.read(new File(configTest.getDataDir() + fileName).toURI().toString());
            DocumentBuilder builder = processor.newDocumentBuilder();
            return builder.wrap(doc);
        } catch (DocumentException e) {
            fail();
            return null;
        }
    }

    @Override
    boolean stripsIgnorableWhitespace() {
        return true;
    }

    boolean mergesAdjacentTextNodes() {
        return false;
    }

    @Override
    public void testId() {
        // No action. DOM4J doesn't support access to elements by ID (It has an elementsById() method, but that treats
        // an attribute as an ID attribute if its name is "ID".)
    }

    /**
     * To be implemented in each subclass - supply an XPath extension function call that finds
     * the string value of the first author in the document
     *
     * @return an XPath expression
     */
    @Override
    public String getExtensionFunctionCall() {
        return "Q{java:modeltests.DOM4JTreeTests}firstAuthor(/)";
    }

    public static String firstAuthor(Document doc) {
        return doc.getRootElement().element("BOOKS").element("ITEM").element("AUTHOR").getStringValue();
    }


}

