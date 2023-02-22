package modeltests;

import jaxptest.ConfigTest;
import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XdmNode;

import java.io.File;

import static org.junit.Assert.fail;

public class TinyTreeTests extends ObjectModelTests {

    @Override
    XdmNode buildExternalDocument(Processor processor, String fileName) {
        try {
            DocumentBuilder builder = processor.newDocumentBuilder();
            return builder.build(new File(ConfigTest.DATA_DIR + fileName));
        } catch (SaxonApiException e) {
            fail();
            return null;
        }
    }
}

