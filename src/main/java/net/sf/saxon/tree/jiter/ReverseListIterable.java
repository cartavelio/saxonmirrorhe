////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
package net.sf.saxon.tree.jiter;

import net.sf.saxon.transpile.CSharpReplaceBody;

import java.util.Iterator;
import java.util.List;
import java.util.ListIterator;

/**
 * Provides an iterable view of a List that iterates in reverse order.
 * @param <T> the type of the items in the list
 */

public class ReverseListIterable<T> implements Iterable<T> {

    private final List<T> list;

    /**
     * Construct a reverse iterator over a list
     * @param list the list over which the reverse iterator will iterate
     */
    @CSharpReplaceBody(code="this.list = list.Reverse();")
    public ReverseListIterable(List<T> list) {
        this.list = list;
    }

    //#if CSHARP==false

    /**
     * Returns an iterator over elements in the list, starting at the end
     * @return a reverse-order Iterator.
     */

    @Override
    public Iterator<T> iterator() {
        return new Iterator<T>() {
            final ListIterator<T> listIterator = list.listIterator(list.size());

            public boolean hasNext() {
                return listIterator.hasPrevious();
            }

            public T next() {
                return listIterator.previous();
            }
        };
    }

    //#endif
}


