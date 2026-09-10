package modeltests;

import jaxptest.ConfigTest;
import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XdmNode;
import org.apache.axiom.om.OMDocument;
import org.apache.axiom.om.OMXMLBuilderFactory;
import org.apache.axiom.om.OMXMLParserWrapper;

import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;

import static org.junit.Assert.fail;

public class AxiomTreeTests extends ObjectModelTests {

    @Override
    XdmNode buildExternalDocument(Processor processor, String fileName) {
        try {
            InputStream in = new FileInputStream(ConfigTest.DATA_DIR + fileName);
            OMXMLParserWrapper axiomBuilder = OMXMLBuilderFactory.createOMBuilder(in);
            OMDocument doc = axiomBuilder.getDocument();
            DocumentBuilder builder = processor.newDocumentBuilder();
            return builder.wrap(doc);
        } catch (IOException e) {
            fail();
            return null;
        }
    }
}

