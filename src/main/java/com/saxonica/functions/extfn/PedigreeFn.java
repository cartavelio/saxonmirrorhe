package com.saxonica.functions.extfn;

import com.saxonica.xsltextn.pedigree.Pedigree;
import com.saxonica.xsltextn.pedigree.PedigreeValue;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.Int64Value;

/**
 * The saxon:pedigree function, new in Saxon 9.9.
 * <p>The function is applied to a map or array that has a pedigree, typically
 * because it was selected by downwards selection within a tree where pedigrees
 * are maintained. It returns information about the pedigree of the map or array,
 * that is, information about its position within the containing tree.</p>
 * <p>This information is returned in the form of a map that contains the following
 * entries.</p>
 * <p><code>container</code> - the containing map or array, or an empty sequence if the
 * supplied map or array is the root of the pedigree tree</p>
 * <p><code>index</code> - if the container is an array, this entry is an integer holding
 * the 1-based index of the supplied map or array within the containing array</p>
 * <p><code>key</code> - if the container is a map, this entry is an atomic value holding
 * the key of the entry in this map that contains the supplied map or array.</p>
 */

public class PedigreeFn extends SystemFunction {

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Item in = arguments[0].head();
        if (in instanceof PedigreeValue) {
            DictionaryMap result = new DictionaryMap();
            Pedigree pedigree = ((PedigreeValue) in).getPedigree();
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