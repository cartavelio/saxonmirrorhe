package modeltests;

import jaxptest.ConfigTest;
import net.sf.saxon.om.TreeModel;
import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XdmNode;

import java.io.File;

import static org.junit.Assert.fail;

public class LinkedTreeTests extends ObjectModelTests {

    @Override
    XdmNode buildExternalDocument(Processor processor, String fileName) {
        try {
            DocumentBuilder builder = processor.newDocumentBuilder();
            builder.setTreeModel(TreeModel.LINKED_TREE);
            return builder.build(new File(ConfigTest.DATA_DIR + fileName));
        } catch (SaxonApiException e) {
            fail();
            return null;
        }
    }

    @Override
    boolean stripsIgnorableWhitespace() {
        return true;
    }
}

