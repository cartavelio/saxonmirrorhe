////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.expr.sort.GenericAtomicComparer;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.StringItem;

/**
 * This class implements the XPath 2.0 fn:compare() function
 */

public class Compare extends CollatingFunctionFixed {

    private static Int64Value compare(StringItem s1, StringItem s2, AtomicComparer comparer) throws XPathException {
        if (s1 == null || s2 == null) {
            return null;
        }
        int result = comparer.compareAtomicValues(s1, s2);
        if (result < 0) {
            return Int64Value.MINUS_ONE;
        } else if (result > 0) {
            return Int64Value.PLUS_ONE;
        } else {
            return Int64Value.ZERO;
        }
    }

    @Override
    public Int64Value call(XPathContext context, Sequence[] arguments) throws XPathException {
        StringItem arg0 = (StringItem) arguments[0].head();
        StringItem arg1 = (StringItem) arguments[1].head();
        GenericAtomicComparer comparer = new GenericAtomicComparer(getStringCollator(), context);
        return compare(arg0, arg1, comparer);
    }

}

