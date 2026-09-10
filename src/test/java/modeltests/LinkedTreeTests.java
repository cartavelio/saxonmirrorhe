package modeltests;

import jaxptest.ConfigTest;
import net.sf.saxon.om.TreeModel;
import net.sf.saxon.s9api.*;

import java.io.File;

import static org.junit.Assert.fail;

public class LinkedTreeTests extends ObjectModelTests {

    @Override
    TreeModel getTreeModel() {
        return TreeModel.LINKED_TREE;
    }

    @Override
    XdmNode buildExternalDocument(Processor processor, String fileName) {
        try {
            DocumentBuilder builder = processor.newDocumentBuilder();
            builder.setTreeModel(TreeModel.LINKED_TREE);
            return builder.build(new File(configTest.getDataDir() + fileName));
        } catch (SaxonApiException e) {
            fail();
            return null;
        }
    }

    @Override
    XdmDestination makeDestination(Processor processor) {
        XdmDestination dest = new XdmDestination();
        dest.setTreeModel(TreeModel.LINKED_TREE);
        return dest;
    }

    @Override
    boolean stripsIgnorableWhitespace() {
        return true;
    }
}

