package modeltests;

import jaxptest.ConfigTest;
import net.sf.saxon.om.TreeModel;
import net.sf.saxon.option.axiom.AxiomObjectModel;
import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XdmNode;
import org.apache.axiom.om.OMDocument;
import org.apache.axiom.om.OMXMLBuilderFactory;
import org.apache.axiom.om.OMXMLParserWrapper;

import javax.xml.transform.stream.StreamSource;
import java.io.File;

import static org.junit.Assert.fail;

public class AxiomTreeTests extends ObjectModelTests {

    @Override
    TreeModel getTreeModel() {
        return AxiomObjectModel.getInstance();
    }

    @Override
    XdmNode buildExternalDocument(Processor processor, String fileName) {
        try {
            //InputStream in = new FileInputStream(configTest.getDataDir() + fileName);
            StreamSource ss = new StreamSource(new File(configTest.getDataDir() + fileName));
            OMXMLParserWrapper axiomBuilder = OMXMLBuilderFactory.createOMBuilder(ss);
            OMDocument doc = axiomBuilder.getDocument();
            DocumentBuilder builder = processor.newDocumentBuilder();
            return builder.wrap(doc);
        } catch (Exception e) {
            fail();
            return null;
        }
    }
}

