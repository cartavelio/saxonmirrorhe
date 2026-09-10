package modeltests;

import jaxptest.ConfigTest;
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
    XdmNode buildExternalDocument(Processor processor, String fileName) {
        try {
            SAXBuilder jdomBuilder = new SAXBuilder();
            Document jdomDocument = jdomBuilder.build(new File(ConfigTest.DATA_DIR + fileName).toURI().toString());
            DocumentBuilder builder = processor.newDocumentBuilder();
            return builder.wrap(jdomDocument);
        } catch (IOException | JDOMException e) {
            fail();
            return null;
        }
    }
}

