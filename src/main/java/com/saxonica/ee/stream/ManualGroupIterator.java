package com.saxonica.ee.stream;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.GroupIterator;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ManualIterator;

/**
 * The class ManualGroupIterator represents an iterator that is never repositioned, but can
 * be used to represent the focus in the dynamic context. It is used as a snapshot copy of a real
 * grouping iterator for use when asynchronous threads are forked, as in xsl:result-document; the
 * purpose of taking a snapshot is to ensure that changes to the base iterator in one thread do not
 * affect the focus of another thread. It is also used when streaming with xsl:fork, which involves
 * pseudo-parallel processing of groups.
 */
public class ManualGroupIterator extends ManualIterator implements GroupIterator {

    // The current grouping key, for grouping methods that use a current grouping key
    private AtomicSequence currentGroupingKey;
    // The contents of the current group
    private GroundedValue currentGroupItems;

    /**
     * Create a manual grouping iterator
     *
     * @param item     the context item
     * @param position the context position
     */

    public ManualGroupIterator(Item item, int position) {
        super(item, position);
    }


    @Override
    public ManualGroupIterator getSnapShot(XPathContext context) {
        return this;
    }

    public void setCurrentGroupingKey(AtomicSequence key) {
        //System.err.println("MGI " + this + " SET KEY = " + key);
        currentGroupingKey = key;
    }

    @Override
    public AtomicSequence getCurrentGroupingKey() {
        //System.err.println("MGI " + this + " GET KEY = " + currentGroupingKey);
        return currentGroupingKey;
    }
    
    public void setCurrentGroup(GroundedValue group) {
        this.currentGroupItems = group;
    }

    public void appendToCurrentGroup(Item item) {
        this.currentGroupItems = item;
    }

    public void startNewCurrentGroup(Item item) {
        this.currentGroupItems = item;
    }

    @Override
    public GroundedValue currentGroup() throws XPathException {
        return currentGroupItems;
    }

    public boolean currentGroupContains(NodeInfo node) throws XPathException {
        return currentGroupItems.containsNode(node);
    }
}


// Copyright (c) 2018-2023 Saxonica Limited
