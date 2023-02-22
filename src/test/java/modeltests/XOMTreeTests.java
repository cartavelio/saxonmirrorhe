package modeltests;

import net.sf.saxon.om.TreeModel;
import net.sf.saxon.option.xom.XOMObjectModel;
import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XdmNode;
import nu.xom.Builder;
import nu.xom.Document;
import nu.xom.ParsingException;

import java.io.File;
import java.io.IOException;

import static org.junit.jupiter.api.Assertions.fail;

public class XOMTreeTests extends ObjectModelTests {

    @Override
    TreeModel getTreeModel() {
        return XOMObjectModel.getInstance();
    }

    @Override
    XdmNode buildExternalDocument(Processor processor, String fileName) {
        try {
            Builder xomBuilder = new Builder();
            Document xomDocument = xomBuilder.build(new File(configTest.getDataDir() + fileName).toURI().toString());
            DocumentBuilder builder = processor.newDocumentBuilder();
            return builder.wrap(xomDocument);
        } catch (IOException | ParsingException e) {
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
        return "Q{java:modeltests.XOMTreeTests}firstAuthor(/)";
    }

    public static String firstAuthor(Document doc) {
        return doc.getRootElement().getFirstChildElement("BOOKS").getFirstChildElement("ITEM").getFirstChildElement("AUTHOR").getValue();
    }
}

