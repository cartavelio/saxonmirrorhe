////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.tree.jiter;

import java.util.Iterator;

/**
 * A Java Iterator which applies a filter predicate to each item in an input sequence
 * @param <T> the type of the input items
 */


public class FilteringJavaIterator<T> implements Iterator<T> {

    private final Iterator<T> input;
    private final java.util.function.Predicate<T> filter;
    private T nextItem;

    /**
     * Create a mapping iterator
     * @param in the input sequence
     * @param filter the filter function to be applied to each item in the input sequence to
     *               decide whether it belongs in the result sequence.
     */

    public FilteringJavaIterator(Iterator<T> in, java.util.function.Predicate<T> filter) {
        this.input = in;
        this.filter = filter;
        advance();
    }

    @Override
    public boolean hasNext() {
        return nextItem != null;
    }

    @Override
    public T next() {
        T result = nextItem;
        if (result != null) {
            advance();
        }
        return result;
    }

    private void advance() {
        while (input.hasNext()) {
            T candidate = input.next();
            if (filter.test(candidate)) {
                nextItem = candidate;
                return;
            }
        }
        nextItem = null;
    }
    //#if CSHARP==false
    //C# emulation of java.util.Iterator omits remove() method
    @Override
    public void remove() {
        input.remove();
    }
    //#endif
}

