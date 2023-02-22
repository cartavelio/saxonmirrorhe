package net.sf.saxon.pull;

import net.sf.saxon.event.Receiver;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.ActiveSource;
import net.sf.saxon.trans.XPathException;

import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;
import javax.xml.transform.stax.StAXSource;

/**
 * An extension of StAXSource that makes the source active: that is, able
 * to deliver itself to a Saxon {@link Receiver}.
 *
 * <p>Saxon can only handle a StAXSource that wraps an XMLStreamReader (not an
 * XMLEventReader)</p>
 */

public class ActiveStAXSource extends StAXSource implements ActiveSource {

    public ActiveStAXSource(XMLStreamReader reader) {
        super(reader);
    }

    public static ActiveStAXSource fromStAXSource(StAXSource source) throws XMLStreamException {
        if (source.getXMLStreamReader() != null) {
            return new ActiveStAXSource(source.getXMLStreamReader());
        } else {
            throw new XMLStreamException("Saxon can only handle a StAXSource that wraps an XMLStreamReader");
        }
    }

    @Override
    public void deliver(Receiver receiver, ParseOptions options) throws XPathException {
        XMLStreamReader reader = getXMLStreamReader();
        if (reader == null) {
            throw new XPathException("Saxon can only handle a StAXSource that wraps an XMLStreamReader");
        }
        StaxBridge bridge = new StaxBridge();
        bridge.setXMLStreamReader(reader);
        new PullSource(bridge).deliver(receiver, options);
    }
}


