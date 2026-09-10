package com.saxonica.functions.extfn;

import com.saxonica.ee.pin.pin.LabeledMapItem;
import com.saxonica.ee.pin.pin.LabeledArrayItem;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;

/**
 * The fn:pin function, proposed for XPath 4.0.
 * <p>The function is applied to a map or array, and it returns a map or array, with the
 * same content, but with a different implementation such that all downward selections
 * from the map/array to contained maps and arrays maintain information about the
 * selection route, which can be used to get information about the containing maps
 * and arrays in a JSON-like tree.</p>
 */

public class PinFn extends SystemFunction {

    @Override
    public Item call(XPathContext context, Sequence[] arguments) throws XPathException {
        Item in = arguments[0].head();
        return makePinnedRoot(in);
    }

    public static FunctionItem makePinnedRoot(Item root) throws XPathException {
        if (root instanceof MapItem) {
            return LabeledMapItem.makePinnedMapItem((MapItem) root);
        } else if (root instanceof ArrayItem) {
            return LabeledArrayItem.makePinnedArrayItem((ArrayItem) root);
        } else {
            throw new XPathException("Argument to fn:pin() must be a single map or array")
                    .withErrorCode("XPTY0004")
                    .asTypeError();

        }
    }

}

// Copyright (c) 2024 Saxonica Limited