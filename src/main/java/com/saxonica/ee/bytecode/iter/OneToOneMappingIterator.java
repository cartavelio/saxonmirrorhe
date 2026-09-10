package com.saxonica.ee.bytecode.iter;

import net.sf.saxon.expr.LastPositionFinder;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.EnumSetTool;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.LookaheadIterator;

import java.util.EnumSet;

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
    public boolean hasNext() {
        // Must only be called if this is a lookahead iterator, which will only be true if the base iterator
        // is a lookahead iterator and one-to-one is true
        return ((LookaheadIterator) base).hasNext();
    }

    @Override
    public Item next() throws XPathException {
        Item nextSource = base.next();
        if (nextSource == null) {
            return null;
        }
        // Call the supplied mapping function
        return map(nextSource);
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

    @Override
    public int getLength() throws XPathException {
        // Must only be called if this is a last-position-finder iterator, which will only be true if the base iterator
        // is a last-position-finder iterator and one-to-one is true
        return ((LastPositionFinder) base).getLength();
    }

    /**
     * Get properties of this iterator, as a bit-significant integer.
     *
     * @return the properties of this iterator. This will be some combination of
     *         properties such as {@link net.sf.saxon.om.SequenceIterator.Property#GROUNDED},
     *         {@link net.sf.saxon.om.SequenceIterator.Property#LAST_POSITION_FINDER},
     *         and {@link net.sf.saxon.om.SequenceIterator.Property#LOOKAHEAD}. It is always
     *         acceptable to return the value zero, indicating that there are no known special properties.
     *         It is acceptable for the properties of the iterator to change depending on its state.
     */

    @Override
    public EnumSet<Property> getProperties() {
        return EnumSetTool.intersect(
                base.getProperties(),
                EnumSet.of(Property.LAST_POSITION_FINDER, Property.LOOKAHEAD));
    }
}

// Copyright (c) 2011-2020 Saxonica Limited


