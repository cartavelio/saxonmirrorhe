package modeltests;

import net.sf.saxon.junit.Testdata;
import net.sf.saxon.om.TreeModel;
import net.sf.saxon.s9api.*;

import java.io.File;

import static org.junit.jupiter.api.Assertions.fail;

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
            return builder.build(Testdata.file(fileName));
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

