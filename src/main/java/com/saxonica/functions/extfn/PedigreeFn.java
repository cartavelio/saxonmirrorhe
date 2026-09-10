package com.saxonica.functions.extfn;

import com.saxonica.xsltextn.pedigree.Pedigree;
import net.sf.saxon.ma.pin.LabeledItem;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.Int64Value;

/**
 * The fn:label function, proposed for XPath 4.0
 * <p>The function is applied to an item that has a label, typically
 * because it was selected by downwards selection within a tree where pedigrees
 * are maintained. It returns the label as a map.</p>
 */

public class PedigreeFn extends SystemFunction {

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Item in = arguments[0].head();
        if (in instanceof LabeledItem) {
            DictionaryMap result = new DictionaryMap();
            Pedigree pedigree = null; // TODO ((LabeledItem) in).getPedigree();
            if (pedigree.getContainer() != null) {
                result.initialPut("container", (Item) pedigree.getContainer());
            }
            if (pedigree.getIndex() != -1) {
                result.initialPut("index", Int64Value.makeIntegerValue(pedigree.getIndex()+1));
            }
            if (pedigree.getKey() != null) {
                result.initialPut("key", pedigree.getKey());
            }
            return result;
        } else {
            return EmptySequence.getInstance();
        }
    }

}

// Copyright (c) 2018-2023 Saxonica Limited