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

// Copyright (c) 2023 Saxonica Limited.