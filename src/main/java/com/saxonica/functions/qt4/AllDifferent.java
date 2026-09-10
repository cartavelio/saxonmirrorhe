package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.CollatingFunctionFixed;
import net.sf.saxon.functions.DistinctValues;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;

import java.util.ArrayList;
import java.util.List;

/**
 * The proposed XPath 4.0 all-different() function, with the collation argument already known
 */

public class AllDifferent extends CollatingFunctionFixed {

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

// Copyright (c) 2023 Saxonica Limited.

