package modeltests;

import jaxptest.ConfigTest;
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

import static org.junit.Assert.fail;

public class XOMTreeTests extends ObjectModelTests {

    @Override
    TreeModel getTreeModel() {
        return XOMObjectModel.getInstance();
    }

    @Override
    XdmNode buildExternalDocument(Processor processor, String fileName) {
        try {
            Builder xomBuilder = new Builder();
            Document jdomDocument = xomBuilder.build(new File(configTest.getDataDir() + fileName).toURI().toString());
            DocumentBuilder builder = processor.newDocumentBuilder();
            return builder.wrap(jdomDocument);
        } catch (IOException | ParsingException e) {
            fail();
            return null;
        }
    }
}

