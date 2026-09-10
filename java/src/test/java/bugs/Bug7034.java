package bugs;

import net.sf.saxon.Controller;
import net.sf.saxon.event.NamespaceReducer;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.s9api.*;
import net.sf.saxon.serialize.SerializationProperties;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Assertions;import org.junit.jupiter.api.Test;

import java.io.File;import java.util.ArrayList;
import static org.junit.jupiter.api.Assertions.fail;

public class Bug7034 {
    ArrayList<Integer> columnNumbers = new ArrayList<>();

    @Test
    void testLineNumbers() {
        try {
            Processor processor = new Processor();
            processor.getUnderlyingConfiguration().setLineNumbering(true);
            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode doc = builder.build(new File("src/test/resources/sample-two.xsl"));

            Controller controller = new Controller(processor.getUnderlyingConfiguration());
            PipelineConfiguration pipe = controller.makePipelineConfiguration();
            XdmDestination destination = new XdmDestination();
            Receiver receiver = destination.getReceiver(pipe, new SerializationProperties());
            receiver = new NamespaceReducer(receiver);
            receiver.setPipelineConfiguration(pipe);
            if (doc.getBaseURI() != null) {
                receiver.setSystemId(doc.getBaseURI().toString());
            }

            receiver.open();
            receiver.startDocument(0);
            receiver.append(doc.getUnderlyingNode());
            receiver.endDocument();

            XdmNode copied = destination.getXdmNode();

            columnNumbers.clear();
            checkLineNumbers(copied, "");

            boolean pass = false;
            Integer colno = null;
            for (int col : columnNumbers) {
                if (colno == null) {
                    colno = col;
                } else {
                    pass = pass || colno != col;
                }
                if (pass) {
                    break;
                }
            }

            Assertions.assertTrue(pass);

        } catch (SaxonApiException | XPathException sae) {
            fail();
        }
    }

    void checkLineNumbers(XdmNode node, String indent) {
        if (node.getNodeKind() == XdmNodeKind.DOCUMENT) {
            for (XdmNode child : node.children()) {
                checkLineNumbers(child, indent + "  ");
            }
        }
        if (node.getNodeKind() == XdmNodeKind.ELEMENT) {
            columnNumbers.add(node.getColumnNumber());
            //System.err.println(indent + node.getNodeName().toString() + " :: " + node.getLineNumber() + "," + node.getColumnNumber());
            for (XdmNode child : node.children()) {
                checkLineNumbers(child, indent + "  ");
            }
        }
    }
}
