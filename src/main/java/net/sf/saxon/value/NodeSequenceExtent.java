////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.value;

import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.ReverseList;
import net.sf.saxon.om.ZeroOrMore;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.NodeListIterator;
import net.sf.saxon.tree.iter.UnfailingIterator;

import java.util.Arrays;
import java.util.Iterator;
import java.util.List;


/**
 * A sequence value implemented extensionally. That is, this class represents a sequence
 * by allocating memory to each item in the sequence.
 */

public class NodeSequenceExtent extends ZeroOrMore<NodeInfo> {

    /**
     * Construct an sequence from an array of items. Note, the array of items is used as is,
     * which means the caller must not subsequently change its contents.
     *
     * @param items the array of items to be included in the sequence
     */

    public NodeSequenceExtent(NodeInfo[] items) {
        super(Arrays.asList(items));
    }

    /**
     * Construct a SequenceExtent as a view of another SequenceExtent
     *
     * @param ext    The existing SequenceExtent
     * @param start  zero-based offset of the first item in the existing SequenceExtent
     *               that is to be included in the new SequenceExtent
     * @param length The number of items in the new SequenceExtent
     */

//    public SequenceExtent(SequenceExtent ext, int start, int length) {
//        value = ext.value.subList(start, start+length);
//    }

    /**
     * Construct a SequenceExtent from a List. The members of the list must all
     * be Items
     *
     * @param list the list of items to be included in the sequence
     */

    public NodeSequenceExtent(List<NodeInfo> list) {
        super(list);
    }

    /**
     * Return an iterator over this sequence.
     *
     * @return the required SequenceIterator, positioned at the start of the
     *         sequence
     */

    /*@NotNull*/
    @Override
    public NodeListIterator iterate() {
        return new NodeListIterator(getValue());
    }

    /**
     * Return an enumeration of this sequence in reverse order (used for reverse axes)
     *
     * @return an AxisIterator that processes the items in reverse order
     */

    /*@NotNull*/
    public UnfailingIterator reverseIterate() {
        return new NodeListIterator(new ReverseList<>(getValue()));
    }

    /**
     * Get the effective boolean value
     */

    @Override
    public boolean effectiveBooleanValue() throws XPathException {
        return getLength() > 0;
    }


    /**
     * Get the contents of this value in the form of a Java {@link Iterable},
     * so that it can be used in a for-each expression
     * @return an Iterable containing the same sequence of items
     */

    @Override
    public Iterable<NodeInfo> asIterable() {
        return getValue();
    }


    /**
     * Get an iterator (a Java {@link Iterator}) over the items in this sequence.
     * @return an iterator over the items in this sequence.
     */

    //@Override
    public Iterator<NodeInfo> iterator() {
        return getValue().iterator();
    }
}

