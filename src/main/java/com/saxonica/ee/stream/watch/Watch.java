package com.saxonica.ee.stream.watch;

import com.saxonica.ee.stream.om.FleetingNode;
import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.UType;

/**
 * A Watch represents a class that is interested in looking at nodes passing down a push
 * pipeline. Originally this was for the purpose of evaluating uniqueness and key constraints while
 * validating an instance document against a schema; now it is used for many expressions that need
 * to be evaluated in streaming mode.
 * <p>The Watch is responsible for deciding which nodes it is interested in. There is a subclass
 * {@link PatternWatch} in which this is decided by matching an XSLT-like pattern, but not all
 * watches make the decision in this way.</p>
 */

public interface Watch /*extends Feed*/ {

    /**
     * Determine whether the watch matches a given node
     *
     * @param node the node to be tested
     * @param context the XPath dynamic context
     * @return true if the node is one of those the Watch is looking out for
     * @throws XPathException if a dynamic error occurs
     */

    boolean matchesNode(FleetingNode node, XPathContext context) throws XPathException;

    /**
     * Ask whether the Watch is capable of matching nodes of a particular kind
     * @param kind the node kind as a UType
     * @return true if the watch is capabl of matching
     */

    boolean matchesNodesOfKind(UType kind);

    /**
     * Set the pipeline configuration in use
     *
     * @param pipe the pipeline Configuration in use
     */

    void setPipelineConfiguration(PipelineConfiguration pipe);

    /**
     * Get the pipeline configuration in use
     *
     * @return the pipeline configuration
     */

    PipelineConfiguration getPipelineConfiguration();

    /**
     * Set a NamespaceResolver for use by the Watch
     *
     * @param resolver the namespace resolver to be used
     */

    void setNamespaceResolver(NamespaceResolver resolver);

    /**
     * Get the namespace resolver in use by the Watch
     *
     * @return the namespace resolver
     */

    NamespaceResolver getNamespaceResolver();

    /**
     * Set the anchor node for the watch. This is the node to which the watch is scoped, typically the
     * context node for the streamable path expression that defines the root of the subtree within which nodes
     * are being sought.
     *
     * @param anchor The anchor node within which the watch is scoped
     */

    void setAnchorNode(FleetingParentNode anchor);

    /**
     * Get the anchor node for the watch. This is the node to which the watch is scoped, typically the
     * context node for the streamable path expression that defines the root of the subtree within which nodes
     * are being sought.
     *
     * @return The anchor node within which the watch is scoped
     */

    FleetingParentNode getAnchorNode();

    /**
     * Open the watch. This is done at the point where it first starts watching for nodes
     * that match the selection.
     * @throws XPathException May be raised if a dynamic error occurs
     * @param terminator action to be taken on failure
     */

    void open(Terminator terminator) throws XPathException;

    /**
     * Signal that an element or document node has been found that matches the selection that this
     * Watch is looking for. This method is called by the WatchManager while
     * processing the startElement or startDocument event that matches the selection.
     *
     *
     * @param node       the element or document node whose start event has been matched
     * @param locationId the location associated with the element or document node (may be
     *                   the location of the instruction that created it)
     * @return a Receiver to be notified of all events starting with the startElement/startDocument event
     *         for the matched element, and ending with the endElement event for that element; or null if this
     *         feature is not required.
     * @throws XPathException May be raised if a dynamic error occurs
     */

    /*@Nullable*/
    Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException;

    /**
     * Signal that the endElement event has occurred for the element whose startElement
     * event caused the Watch to be activated.
     *
     * @param locationId the location of the element
     * @throws XPathException May be raised if a constraint implemented by this
     *                        Watch is violated
     */

    void endSelectedParentNode(Location locationId) throws XPathException;

    /**
     * Signal that a node (other than a document or element node) that matches the selection
     * has been found.
     *
     *
     *
     * @param item       the node that was matched.
     * @throws XPathException May be raised if a dynamic error occurs
     */

    void processItem(Item item) throws XPathException;

    /**
     * The close() method is called immediately before the Watch is destroyed, that is,
     * when the element whose declaration scopes the constraint implemented by this
     * Watch goes out of scope.
     * @throws XPathException May be raised if a dynamic error occurs
     */

    void close() throws XPathException;

}

// Copyright (c) 2004-2022 Saxonica Limited No warranty.
