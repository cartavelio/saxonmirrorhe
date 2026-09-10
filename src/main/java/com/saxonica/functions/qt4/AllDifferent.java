package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.CollatingFunctionFixed;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;

/**
 * The proposed XPath 4.0 all-different() function, with the collation argument already known
 */

public class AllDifferent extends CollatingFunctionFixed {

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        StringCollator collator = getStringCollator();
        DuplicateValues.DuplicatesIterator iter =
                new DuplicateValues.DuplicatesIterator(arguments[0].iterate(), collator, context);
        if (iter.next() != null) {
            iter.close();
            return BooleanValue.FALSE;
        }
        return BooleanValue.TRUE;
    }


}

// Copyright (c) 2023 Saxonica Limited.

