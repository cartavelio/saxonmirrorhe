package com.saxonica.functions.extfn;

import com.saxonica.xsltextn.pedigree.PedigreeArrayItem;
import com.saxonica.xsltextn.pedigree.PedigreeMapItem;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;

/**
 * The saxon:with-pedigree function, new in Saxon 9.9.
 * <p>The function is applied to a map or array, and it returns a map or array, with the
 * same content, but with a different implementation such that all downward selections
 * from the map/array to contained maps and arrays maintain information about the
 * selection route, which can be used to get information about the containing maps
 * and arrays in a JSON-like tree.</p>
 */

public class WithPedigree extends SystemFunction {

    @Override
    public Item call(XPathContext context, Sequence[] arguments) throws XPathException {
        Item in = arguments[0].head();
        Item out = makeRootWithPedigree(in);
        return out == null ? in : out;
    }

    public static net.sf.saxon.om.Function makeRootWithPedigree(Item root) {
        if (root instanceof MapItem) {
            return new PedigreeMapItem((MapItem) root);
        } else if (root instanceof ArrayItem) {
            return new PedigreeArrayItem((ArrayItem) root);
        } else {
            return null;
        }
    }

}

// Copyright (c) 2018-2022 Saxonica Limited