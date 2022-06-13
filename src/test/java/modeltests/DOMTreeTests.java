package modeltests;

import jaxptest.ConfigTest;
import net.sf.saxon.dom.DOMObjectModel;
import net.sf.saxon.om.TreeModel;
import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XdmNode;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;

import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import java.io.File;
import java.io.IOException;

import static org.junit.Assert.fail;

public class DOMTreeTests extends ObjectModelTests {

    @Override
    TreeModel getTreeModel() {
        return DOMObjectModel.getInstance();
    }

    @Override
    XdmNode buildExternalDocument(Processor processor, String fileName) {
        try {
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            javax.xml.parsers.DocumentBuilder db = factory.newDocumentBuilder();
            InputSource is = new InputSource(new File(configTest.getDataDir() + fileName).toURI().toString());
            org.w3c.dom.Document doc = db.parse(is);
            DocumentBuilder builder = processor.newDocumentBuilder();
            return builder.wrap(doc);
        } catch (IOException | SAXException | ParserConfigurationException e) {
            fail();
            return null;
        }
    }

}

