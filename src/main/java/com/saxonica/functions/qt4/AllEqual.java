////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.CollatingFunctionFixed;
import net.sf.saxon.functions.Count;
import net.sf.saxon.functions.DistinctValues;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;

/**
 * The proposed XPath 4.0 all-equal() function, with the collation argument already known
 */

public class AllEqual extends CollatingFunctionFixed {

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        StringCollator collator = getStringCollator();
        DistinctValues.DistinctIterator iter =
                new DistinctValues.DistinctIterator(arguments[0].iterate(), collator, context);
        int count = Count.count(iter);
        return BooleanValue.get(count <= 1);
    }

}

