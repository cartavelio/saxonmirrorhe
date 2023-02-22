////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr;

import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;

/**
 * ItemMappingFunction is an interface that must be satisfied by an object passed to a
 * ItemMappingIterator. It represents an object which, given an Item, can return either
 * another Item, or null.
 *
 * <p>NOTE: Java allows a lambda expression to be used where an ItemMappingFunction
 * is needed, but C# does not (it's not possible in C# to have a class implementing
 * a delegate). So if a delegate is wanted, use {@link ItemMapper}. Note that this
 * class is deliberately NOT declared as a FunctionalInterface, to ensure that it
 * transpiles into a C# interface rather than a delegate.</p>
 */

public interface ItemMappingFunction {

    /**
     * Map one item to another item.
     *
     * @param item The input item to be mapped.
     * @return either the output item, or null.
     * @throws XPathException if a dynamic error occurs
     */

    /*@Nullable*/
    Item mapItem(Item item) throws XPathException;

}

