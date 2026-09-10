package com.saxonica.ee.stream;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.GroupIterator;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ManualIterator;
import net.sf.saxon.value.SequenceExtent;

import java.util.ArrayList;
import java.util.List;

/**
 * The class ManualGroupIterator represents an iterator that is never repositioned, but can
 * be used to represent the focus in the dynamic context. It is used as a snapshot copy of a real
 * grouping iterator for use when asynchronous threads are forked, as in xsl:result-document; the
 * purpose of taking a snapshot is to ensure that changes to the base iterator in one thread do not
 * affect the focus of another thread.
 */
public class ManualGroupIterator extends ManualIterator implements GroupIterator {

    private AtomicSequence currentGroupingKey;
    private List<Item> currentGroupItems = new ArrayList<>();

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
    
    public void setCurrentGroup(List<Item> group) {
        this.currentGroupItems = group;
    }

    public void appendToCurrentGroup(Item item) {
        this.currentGroupItems.clear(); // we only need the last item in the group...
        this.currentGroupItems.add(item);
    }

    public void startNewCurrentGroup(Item item) {
        this.currentGroupItems.clear();
        this.currentGroupItems.add(item);
    }

    @Override
    public GroundedValue currentGroup() throws XPathException {
        return SequenceExtent.makeSequenceExtent(currentGroupItems);
    }

    public boolean currentGroupContains(Item item) {
        return currentGroupItems.contains(item);
    }
}


// Copyright (c) 2018-2022 Saxonica Limited
