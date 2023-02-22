package net.sf.saxon.om;

import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.Sender;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.lib.ActiveSource;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.trans.XPathException;

public class NodeSource implements ActiveSource {

    private NodeInfo node;
    private String systemId;

    public NodeSource(NodeInfo node) {
        this.node = node;
        this.systemId = node.getSystemId();
    }

    /**
     * Deliver the content of the source to a supplied Receiver.
     * <p>For many (but not all) implementations of {@code Source}, this method consumes
     * the source and can therefore only be called once.</p>
     *
     * @param receiver the receiver to which events representing the parsed XML document will be sent
     * @param options  options for parsing the source
     * @throws XPathException if parsing fails for any reason. The detailed diagnostics will
     *                        have been sent to the error reporter.
     */
    @Override
    public void deliver(Receiver receiver, ParseOptions options) throws XPathException {
        Sender.sendDocumentInfo(node, receiver, new Loc(getSystemId(), -1, -1));
    }

    /**
     * Set the system identifier for this Source.
     *
     * <p>The system identifier is optional if the source does not
     * get its data from a URL, but it may still be useful to provide one.
     * The application can use a system identifier, for example, to resolve
     * relative URIs and to include in error messages and warnings.</p>
     *
     * @param systemId The system identifier as a URL string.
     */
    @Override
    public void setSystemId(String systemId) {
        this.systemId = systemId;
    }

    /**
     * Get the system identifier that was set with setSystemId.
     *
     * @return The system identifier that was set with setSystemId, or null
     * if setSystemId was not called.
     */
    @Override
    public String getSystemId() {
        if (systemId == null) {
            return node.getSystemId();
        } else {
            return this.systemId;
        }
    }

    public NodeInfo getNode() {
        return node;
    }
};


