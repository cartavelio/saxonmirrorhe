package net.sf.saxon.dom;

import net.sf.saxon.event.Receiver;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.resource.ActiveSource;
import net.sf.saxon.trans.XPathException;
import org.w3c.dom.Node;

import javax.xml.transform.dom.DOMSource;

/**
 * An extension of {@link DOMSource} that implements the {@link ActiveSource} interface,
 * so it knows how to send itself to a {@link Receiver}
 */

public class ActiveDOMSource extends DOMSource implements ActiveSource {

    public ActiveDOMSource(Node node, String systemId) {
        super(node, systemId);
    }

    /**
     * Construct an ActiveDOMSource from a DOMSource
     * @param source the input source object, whose properties are copied.
     */
    public ActiveDOMSource(DOMSource source) {
        setNode(source.getNode());
        setSystemId((source.getSystemId()));
    }

    @Override
    public void deliver(Receiver receiver, ParseOptions options) throws XPathException {
        DOMObjectModel.sendDOMSource(this, receiver);
    }
}


