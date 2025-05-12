package internaltests;

import net.sf.saxon.Configuration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.StreamWriterToReceiver;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;

import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamWriter;
import javax.xml.transform.stream.StreamResult;
import java.io.StringWriter;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 *
 */
public class ReceiverTests {

    @Test
    public void testStreamWriterToReceiver() {
        // Simple test to fix a bug in 9.3
        try {
            StringWriter out = new StringWriter();
            Configuration config = new Configuration();
            Receiver receiver = config.getSerializerFactory().getReceiver(new StreamResult(out));
            XMLStreamWriter sw = new StreamWriterToReceiver(receiver);
            sw.writeStartDocument();
            sw.writeEmptyElement("doc");
            sw.writeEndDocument();
            assertTrue(out.toString().contains("<doc/>"));
        } catch (XMLStreamException | XPathException e) {
            e.printStackTrace();
            fail();
        }

    }
}




