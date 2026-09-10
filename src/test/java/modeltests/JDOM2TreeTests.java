package modeltests;

import net.sf.saxon.om.TreeModel;
import net.sf.saxon.option.jdom2.JDOM2ObjectModel;
import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XdmNode;
import org.jdom2.Document;
import org.jdom2.JDOMException;
import org.jdom2.input.SAXBuilder;

import java.io.File;
import java.io.IOException;

import static org.junit.Assert.fail;

public class JDOM2TreeTests extends ObjectModelTests {

    @Override
    TreeModel getTreeModel() {
        return JDOM2ObjectModel.getInstance();
    }

    @Override
    XdmNode buildExternalDocument(Processor processor, String fileName) {
        try {
            SAXBuilder jdomBuilder = new SAXBuilder();
            Document jdomDocument = jdomBuilder.build(new File(configTest.getDataDir() + fileName).toURI().toString());
            DocumentBuilder builder = processor.newDocumentBuilder();
            return builder.wrap(jdomDocument);
        } catch (IOException | JDOMException e) {
            fail();
            return null;
        }
    }

    /**
     * To be implemented in each subclass - supply an XPath extension function call that finds
     * the string value of the first author in the document
     *
     * @return an XPath expression
     */
    @Override
    public String getExtensionFunctionCall() {
        return "Q{java:modeltests.JDOM2TreeTests}firstAuthor(/)";
    }

    public static String firstAuthor(Document doc) {
        return doc.getRootElement().getChild("BOOKS").getChild("ITEM").getChild("AUTHOR").getText();
    }
}

