package modeltests;

import jaxptest.ConfigTest;
import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XdmNode;
import org.dom4j.Document;
import org.dom4j.DocumentException;
import org.dom4j.io.SAXReader;

import java.io.File;

import static org.junit.Assert.fail;

public class DOM4JTreeTests extends ObjectModelTests {

    @Override
    XdmNode buildExternalDocument(Processor processor, String fileName) {
        try {
            SAXReader dom4jBuilder = new SAXReader();
            dom4jBuilder.setMergeAdjacentText(true);
            Document doc = dom4jBuilder.read(new File(ConfigTest.DATA_DIR + fileName).toURI().toString());
            DocumentBuilder builder = processor.newDocumentBuilder();
            return builder.wrap(doc);
        } catch (DocumentException e) {
            fail();
            return null;
        }
    }
}

