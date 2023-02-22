////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;

import java.util.ArrayList;
import java.util.List;

/**
 * The proposed XPath 4.0 unique() function, with the collation argument already known
 */

public class UniqueFn extends CollatingFunctionFixed {

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        StringCollator collator = getStringCollator();
        DistinctValues.DistinctIterator iter =
                new DistinctValues.DistinctIterator(arguments[0].iterate(), collator, context);
        List<String> listener = new ArrayList<>(1);
        iter.notifyDuplicates(() -> listener.add("gotcha!"));
        while (iter.next() != null) {
            if (!listener.isEmpty()) {
                iter.close();
                return BooleanValue.FALSE;
            }
        }
        return BooleanValue.get(listener.isEmpty());
    }





}

