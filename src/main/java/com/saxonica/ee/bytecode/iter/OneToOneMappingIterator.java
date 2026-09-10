package com.saxonica.ee.bytecode.iter;

import net.sf.saxon.expr.LastPositionFinder;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.LookaheadIterator;

/**
 * Abstract iterator that performs a one-to-one mapping from the items in the input sequence to the
 * items in the output sequence; the mapping operation is defined by the map() action implemented in the
 * subclass.
 */
public abstract class OneToOneMappingIterator implements SequenceIterator, LookaheadIterator, LastPositionFinder {
    private SequenceIterator base;
    protected XPathContext context;

    public OneToOneMappingIterator() {
    }

    public void setSequence(SequenceIterator base, XPathContext context) {
        this.base = base;
        this.context = context;
    }

    @Override
    public boolean supportsHasNext() {
        return (base instanceof LookaheadIterator) && ((LookaheadIterator)base).supportsHasNext();
    }

    @Override
    public boolean hasNext() {
        // Must only be called if this is a lookahead iterator, which will only be true if the base iterator
        // is a lookahead iterator and one-to-one is true
        return ((LookaheadIterator) base).hasNext();
    }

    @Override
    public Item next() {
        Item nextSource = base.next();
        if (nextSource == null) {
            return null;
        }
        // Call the supplied mapping function
        try {
            return map(nextSource);
        } catch (XPathException e) {
            throw new UncheckedXPathException(e);
        }
    }

    /**
     * Abstract method to be supplied in subclasses to map an item from the input sequence
     * to an item in the output sequence
     *
     * @param baseItem the item in the input (base) sequence
     * @return the corresponding item in the output sequence. Must not be null
     * @throws XPathException if the mapping function fails
     */

    protected abstract Item map(Item baseItem) throws XPathException;

    @Override
    public void close() {
        base.close();
    }

    public boolean supportsGetLength() {
        return SequenceTool.supportsGetLength(base);
    }

    @Override
    public int getLength() {
        return ((LastPositionFinder) base).getLength();
    }

}

// Copyright (c) 2011-2020 Saxonica Limited


