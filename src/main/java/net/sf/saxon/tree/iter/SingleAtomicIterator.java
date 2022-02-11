////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.tree.iter;

import net.sf.saxon.expr.LastPositionFinder;
import net.sf.saxon.value.AtomicValue;


/**
 * SingletonIterator: an iterator over a sequence of zero or one values
 */

public class SingleAtomicIterator
        extends SingletonIterator
        implements AtomicIterator,
        ReversibleIterator, LastPositionFinder,
        GroundedIterator, LookaheadIterator {

    /**
     * Private constructor: external classes should use the factory method
     *
     * @param value the item to iterate over
     */

    public SingleAtomicIterator(AtomicValue value) {
        super(value);
    }


    /*@NotNull*/
    @Override
    public SingleAtomicIterator getReverseIterator() {
        return new SingleAtomicIterator((AtomicValue)getValue());
    }

    @Override
    public AtomicValue next() {
        return (AtomicValue)super.next();
    }
}

