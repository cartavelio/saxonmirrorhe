package com.saxonica.functions.qt4.pin;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.EmptyMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;

/**
 * The fn:label function, proposed for XPath 4.0
 * <p>The function is applied to an item that has a label, typically
 * because it was selected by downwards selection within a tree where pedigrees
 * are maintained. It returns the label as a map.</p>
 */

public class LabelFn extends SystemFunction {

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Item in = arguments[0].head();
        MapItem label = in.getLabel();
        return label == null ? EmptyMap.getInstance() : label;
    }

}

// Copyright (c) 2018-2023 Saxonica Limited