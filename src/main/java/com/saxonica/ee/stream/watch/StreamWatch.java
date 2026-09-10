package com.saxonica.ee.stream.watch;


import com.saxonica.ee.stream.Inversion;
import com.saxonica.ee.stream.feed.DecomposingFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.om.FleetingNode;
import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ManualIterator;
import net.sf.saxon.type.Type;
import net.sf.saxon.type.UType;

public class StreamWatch extends AbstractWatch {

    private final Inversion action;
    private final WatchManager watchManager;
    private final XPathContext context;

    public StreamWatch(Inversion action, WatchManager watchManager, XPathContext context) {
        this.action = action;
        this.watchManager = watchManager;
        this.context = context;
    }

    /**
     * Determine whether the watch matches a given node
     *
     *
     * @param node    the node to be tested
     * @param context the XPath dynamic context
     * @return true if the node is one of those the Watch is looking out for
     * @throws net.sf.saxon.trans.XPathException
     *          if a dynamic error occurs
     */
    @Override
    public boolean matchesNode(FleetingNode node, XPathContext context) throws XPathException {
        return node.getNodeKind() == Type.DOCUMENT;
    }

    /**
     * Ask whether the Watch is capable of matching nodes of a particular kind
     *
     * @param kind the node kind
     */
    @Override
    public boolean matchesNodesOfKind(UType kind) {
        return kind.subsumes(UType.DOCUMENT);
    }

    /**
     * Set the anchor node for the watch. This is the node to which the watch is scoped, typically the
     * context node for the streamable path expression that defines the root of the subtree within which nodes
     * are being sought.
     *
     * @param anchor The anchor node within which the watch is scoped
     */
    @Override
    public void setAnchorNode(FleetingParentNode anchor) {
    }

    /**
     * Get the anchor node for the watch. This is the node to which the watch is scoped, typically the
     * context node for the streamable path expression that defines the root of the subtree within which nodes
     * are being sought.
     *
     * @return The anchor node within which the watch is scoped
     */
    @Override
    public FleetingParentNode getAnchorNode() {
        return null;
    }

    /**
     * Signal that an element or document node has been found that matches the selection that this
     * Watch is looking for. This method is called by the WatchManager while
     * processing the startElement or startDocument event that matches the selection.
     *
     * @param node       the element or document node whose start event has been matched
     * @param locationId the location associated with the element or document node (may be
     *                   the location of the instruction that created it)
     * @return a Receiver to be notified of all events starting with the startElement/startDocument event
     *         for the matched element, and ending with the endElement event for that element; or null if this
     *         feature is not required.
     * @throws net.sf.saxon.trans.XPathException
     *          May be raised if a dynamic error occurs
     */
    @Override
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        Inversion inversion = action;
        ManualIterator iter = new ManualIterator(node);
        context.setCurrentIterator(iter);
        ItemFeed parentFeed = new DecomposingFeed(watchManager.getOutputter(), context);
        Watch watch = inversion.getWatch(watchManager, parentFeed, context);
        watch.setAnchorNode(node);
        watchManager.addWatch(watch, true);
        return null;
    }

    /**
     * Signal that the endElement event has occurred for the element whose startElement
     * event caused the Watch to be activated.
     *
     * @param locationId the location of the element
     * @throws net.sf.saxon.trans.XPathException
     *          May be raised if a constraint implemented by this
     *          Watch is violated
     */
    @Override
    public void endSelectedParentNode(Location locationId) throws XPathException {
    }
}

// Copyright (c) 2018-2023 Saxonica Limited