package net.sf.saxon.tree.iter;

import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.transpile.CSharpSimpleEnum;

/**
 * Defines possible properties of a {@link SequenceIterator}
 */

@CSharpSimpleEnum(flags=true)
public enum SequenceIteratorProperty {

    /**
     * Property value: the iterator is "grounded". This means that (a) the
     * iterator must be an instance of {@link net.sf.saxon.tree.iter.GroundedIterator}, and (b) the
     * implementation of the materialize() method must be efficient (in particular,
     * it should not involve the creation of new objects)
     */

    GROUNDED,

    /**
     * Property value: the iterator knows the number of items that it will deliver.
     * This means that (a) the iterator must be an instance of {@link net.sf.saxon.expr.LastPositionFinder},
     * and (b) the implementation of the getLastPosition() method must be efficient (in particular,
     * it should take constant time, rather than time proportional to the length of the sequence)
     */

    LAST_POSITION_FINDER,

    /**
     * Property value: the iterator knows whether there are more items still to come. This means
     * that (a) the iterator must be an instance of {@link net.sf.saxon.tree.iter.LookaheadIterator}, and (b) the
     * implementation of the hasNext() method must be efficient (more efficient than the client doing
     * it)
     */

    LOOKAHEAD,

    /**
     * Property value: the iterator can deliver an atomized result. This means that the iterator
     * must be an instance of {@link AtomizingIterator}.
     */

    ATOMIZING

}

