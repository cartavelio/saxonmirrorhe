package com.saxonica.ee.stream.watch;

import com.saxonica.ee.stream.om.FleetingNode;
import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.UType;

/**
 * A Watch represents a class that is interested in looking at nodes for the purpose
 * of evaluating uniqueness and key constraints, or for XSLT streaming; specifically,
 * an implementation of Watch that retains a Pattern which is evaluated to see whether
 * a particular node matches the Watch.
 */

public abstract class PatternWatch extends AbstractWatch {

    private Pattern selection;
    private FleetingParentNode anchor;


    public PatternWatch() {
    }

    /**
     * Determine whether the watch matches a given node
     *
     * @param node    the node to be tested
     * @param context the XPath dynamic context
     */
    @Override
    public boolean matchesNode(FleetingNode node, XPathContext context) throws XPathException {
        return getSelection().matchesBeneathAnchor(node, getAnchorNode(), context);
    }

    /**
     * Ask whether the Watch is capable of matching nodes of a particular kind
     *
     * @param kind the node kind as a UType
     */
    @Override
    public boolean matchesNodesOfKind(UType kind) {
        return getSelection().getUType().overlaps(kind);
    }

    /**
     * Set the selection (that is, the streamable XPath expression) that defines
     * the nodes that this Watch is looking for
     *
     * @param selection the streamable XPath expression
     */

    public void setSelection(Pattern selection) {
        this.selection = selection;
    }

    /**
     * Get the selection (that is, the restricted XPath expression) that defines
     * the nodes that this Watch is looking for
     *
     * @return the selection
     */

    public Pattern getSelection() {
        return selection;
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
        this.anchor = anchor;
    }

    /**
     * Get the anchor node for the watch. This is the node to which the watch is scoped, typically the
     * context node for the streamable path expression that defines the root of the subtree within which nodes
     * are being sought.
     *
     * @return The anchor node within which the watch is scoped
     */

    @Override
    public final FleetingParentNode getAnchorNode() {
        return anchor;
    }

    /**
     * Open the watch. This is done at the point where it first starts watching for nodes
     * that match the selection.
     * @param terminator action to be taken on failure
     */

    @Override
    public void open(Terminator terminator) throws XPathException {
        //
    }

    /**
     * Signal that a node (other than an element) that matches the selection
     * has been found.
     *
     * @param item       the node that was matched.
     *
     */

    @Override
    public void processItem(Item item) throws XPathException {
        //
    }

    /**
     * The close() method is called immediately before the Watch is destroyed, that is,
     * when the element whose declaration scopes the constraint implemented by this
     * Watch goes out of scope. The default implementation does nothing.
     */

    @Override
    public void close() throws XPathException {
        //
    }


}

// Copyright (c) 2018-2022 Saxonica Limited No warranty.
