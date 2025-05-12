package modeltests;

import net.sf.saxon.om.TreeModel;
import net.sf.saxon.s9api.*;

import java.io.File;

import static org.junit.jupiter.api.Assertions.fail;

public class TinyTreeTests extends ObjectModelTests {

    @Override
    TreeModel getTreeModel() {
        return TreeModel.TINY_TREE;
    }

    @Override
    XdmNode buildExternalDocument(Processor processor, String fileName) {
        try {
            DocumentBuilder builder = processor.newDocumentBuilder();
            return builder.build(new File(configTest.getDataDir() + fileName));
        } catch (SaxonApiException e) {
            fail();
            return null;
        }
    }

    @Override
    XdmDestination makeDestination(Processor processor) {
        return new XdmDestination();
    }

    @Override
    boolean stripsIgnorableWhitespace() {
        return true;
    }
}

