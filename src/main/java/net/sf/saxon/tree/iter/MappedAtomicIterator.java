////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.tree.iter;

import net.sf.saxon.value.AtomicValue;

import java.util.Iterator;
import java.util.function.Function;


/**
 * A SequenceIterator is used to iterate over a sequence. An AtomicIterator
 * is a SequenceIterator that returns atomic values and throws no checked exceptions.
 */

public class MappedAtomicIterator<T> implements AtomicIterator {

    private final Iterator<T> baseIterator;
    private final Function<T, AtomicValue> mapper;

    public MappedAtomicIterator(Iterator<T> baseIterator, Function<T, AtomicValue> mapper) {
        this.baseIterator = baseIterator;
        this.mapper = mapper;
    }

    /**
     * Get the next atomic value in the sequence.
     *
     * @return the next Item. If there are no more items, return null.
     */

    @Override
    public AtomicValue next() {
        if (baseIterator.hasNext()) {
            return mapper.apply(baseIterator.next());
        } else {
            return null;
        }
    }


}

