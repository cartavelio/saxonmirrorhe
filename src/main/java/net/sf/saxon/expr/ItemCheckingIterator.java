////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr;

import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.LookaheadIterator;
import net.sf.saxon.tree.iter.SequenceIteratorProperty;

import java.util.EnumSet;
import java.util.function.Consumer;

/**
 * ItemCheckingIterator applies a supplied function to each item in a sequence.
 * The iterator returns the same items as the original, but has side effects (such
 * as throwing an error) determined by the supplied callback function
 */

public class ItemCheckingIterator
        implements SequenceIterator, LookaheadIterator, LastPositionFinder {

    private final SequenceIterator base;
    private final Consumer<Item> action;


    /**
     * Construct an ItemCheckingIterator that will apply a specified DummyItemMappingFunction to
     * each Item returned by the base iterator.
     *
     * @param base   the base iterator
     * @param action the mapping function to be applied.
     */

    public ItemCheckingIterator(SequenceIterator base, Consumer<Item> action) {
        this.base = base;
        this.action = action;
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
        // Call the supplied checking function
        action.accept(nextSource);
        return nextSource;
    }

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
     * Get properties of this iterator
     *
     * @return the properties of this iterator. This will be some combination of
     *         properties such as {@link SequenceIteratorProperty#GROUNDED},
     *         {@link SequenceIteratorProperty#LAST_POSITION_FINDER},
     *         and {@link SequenceIteratorProperty#LOOKAHEAD}. It is always
     *         acceptable to return the value zero, indicating that there are no known special properties.
     *         It is acceptable for the properties of the iterator to change depending on its state.
     */

    @Override
    public EnumSet<SequenceIteratorProperty> getProperties() {
        return base.getProperties();
    }
}

