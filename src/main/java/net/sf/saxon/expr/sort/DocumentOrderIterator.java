////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr.sort;

import net.sf.saxon.om.*;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.GroundedIterator;
import net.sf.saxon.tree.iter.NodeListIterator;
import net.sf.saxon.value.SequenceExtent;

import java.util.ArrayList;
import java.util.Comparator;

/**
 * DocumentOrderIterator takes as input an iteration of nodes in any order, and
 * returns as output an iteration of the same nodes in document order, eliminating
 * any duplicates. An error occurs if the input sequence contains items that are
 * not nodes.
 */

public final class DocumentOrderIterator implements SequenceIterator, GroundedIterator {

    private final SequenceIterator iterator;
    private final ArrayList<NodeInfo> sequence; // explicit type ArrayList used so C# List.Sort() is available
    private NodeInfo current = null;

    /**
     * Iterate over a sequence in document order.
     * @param base the input sequence to be sorted
     * @param comparer the comparer used for comparing node positions
     * @throws XPathException if the input sequence cannot be evaluated, or if it contains an item
     * that is not a node
     */

    public DocumentOrderIterator(SequenceIterator base, Comparator<? super NodeInfo> comparer)  {

        int len = SequenceTool.supportsGetLength(base) ? SequenceTool.getLength(base) : 50;
        sequence = new ArrayList<>(len);
        SequenceTool.supply(base, (ItemConsumer<? super Item>) item -> {
            if (item instanceof NodeInfo) {
                sequence.add((NodeInfo) item);
            } else {
                throw new XPathException("Item in input for sorting is not a node: " + Err.depict(item), "XPTY0004");
            }
        });

        //System.err.println("SORT into document order: sequence length = " + sequence.size());
        if (sequence.size() > 1) {
            sequence.sort(comparer);
        }
        iterator = new NodeListIterator(sequence);
    }

    // Implement the SequenceIterator as a wrapper around the underlying iterator
    // over the sequenceExtent, but looking ahead to remove duplicates.

    @Override
    public NodeInfo next() {
        while (true) {
            NodeInfo next = (NodeInfo)iterator.next();
            if (next == null) {
                current = null;
                return null;
            }
            if (!next.equals(current)) {
                current = next;
                return current;
            }
        }
    }

    /**
     * Ask if the iterator is actually grounded. This method must be called before calling
     * {@link #materialize()} or {@link #getResidue()}, because the iterator might
     * be grounded under some conditions and not others (usually when it delegates
     * to another iterator)
     *
     * @return true if this iterator is grounded
     */
    @Override
    public boolean isActuallyGrounded() {
        return true;
    }

    /**
     * Return a GroundedValue containing all the remaining items in the sequence returned by this
     * SequenceIterator, starting at the current position. This should be an "in-memory" value, not a
     * Closure. This method does not change the state of the iterator (in particular, it does not
     * consume the iterator).
     *
     * @return the corresponding Value
     */
    @Override
    public GroundedValue getResidue() {
        return materialize();
    }

    /**
     * Create a GroundedValue (a sequence materialized in memory) containing all the values delivered
     * by this SequenceIterator. The method must only be used when the SequenceIterator is positioned
     * at the start. If it is not positioned at the start, then it is implementation-dependant whether
     * the returned sequence contains all the nodes delivered by the SequenceIterator from the beginning,
     * or only those delivered starting at the current position.
     * <p>It is implementation-dependant whether this method consumes the SequenceIterator. (More specifically,
     * in the current implementation: if the iterator is backed by a {@link GroundedValue}, then that
     * value is returned and the iterator is not consumed; otherwise, the iterator is consumed and the
     * method returns the remaining items after the current position only).</p>
     *
     * @return a sequence containing all the items delivered by this SequenceIterator.
     * @throws UncheckedXPathException if reading the SequenceIterator throws an error
     */
    @Override
    public GroundedValue materialize() {
        return new SequenceExtent.Of<NodeInfo>(sequence);
    }
}

