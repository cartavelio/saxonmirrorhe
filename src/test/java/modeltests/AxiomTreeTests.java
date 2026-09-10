package modeltests;

import net.sf.saxon.om.TreeModel;
import net.sf.saxon.option.axiom.AxiomObjectModel;
import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XdmNode;
import org.apache.axiom.om.OMDocument;
import org.apache.axiom.om.OMXMLBuilderFactory;
import org.apache.axiom.om.OMXMLParserWrapper;

import javax.xml.namespace.QName;
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


    /**
     * To be implemented in each subclass - supply an XPath extension function call that finds
     * the string value of the first author in the document
     *
     * @return an XPath expression
     */
    @Override
    public String getExtensionFunctionCall() {
        return "Q{java:modeltests.AxiomTreeTests}firstAuthor(/)";
    }

    public static String firstAuthor(OMDocument doc) {
        return doc.getOMDocumentElement()
                .getFirstChildWithName(new QName("BOOKS"))
                .getFirstChildWithName(new QName("ITEM"))
                .getFirstChildWithName(new QName("AUTHOR"))
                .getText();
    }
}

