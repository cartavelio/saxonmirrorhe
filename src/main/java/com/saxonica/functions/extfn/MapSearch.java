package com.saxonica.functions.extfn;

import com.saxonica.xsltextn.pedigree.PedigreeArrayItem;
import com.saxonica.xsltextn.pedigree.PedigreeMapItem;
import com.saxonica.xsltextn.pedigree.PedigreeValue;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.SequenceExtent;

import java.util.ArrayList;
import java.util.List;

/**
 * The saxon:map-search function, new in Saxon 9.9
 */

public class MapSearch extends SystemFunction {

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        List<Item> result = new ArrayList<>();
        AtomicValue key = (AtomicValue)arguments[1].head();
        FunctionItem tester = arguments.length > 2 ? (FunctionItem) arguments[2].head() : null;
        processSequence(arguments[0].materialize(), key, tester, context, result);
        return SequenceExtent.makeSequenceExtent(result);
    }

    private void processSequence(GroundedValue seq, AtomicValue key, FunctionItem tester, XPathContext context, List<Item> result) throws XPathException {
        GroundedValue in = ((Sequence) seq).materialize();
        SequenceIterator iter = in.iterate();
        for (Item item; (item = iter.next()) != null; ) {
            if (item instanceof ArrayItem) {
                ArrayItem array = (ArrayItem)item;
                if (!(array instanceof PedigreeValue)) {
                    array = new PedigreeArrayItem(array);
                }
                for (GroundedValue member : array.members()) {
                    processSequence(member, key, tester, context, result);
                }
            } else if (item instanceof MapItem) {
                MapItem map = (MapItem) item;
                if (!(map instanceof PedigreeValue)) {
                    map = new PedigreeMapItem(map);
                }
                GroundedValue value = map.get(key);
                if (value != null) {
                    Sequence test = tester == null ? BooleanValue.TRUE : tester.call(context, new Sequence[]{value});
                    if (((BooleanValue)test.head()).getBooleanValue()) {
                        DictionaryMap hit = new DictionaryMap();
                        hit.initialPut("key", key);
                        hit.initialPut("value", value);
                        hit.initialPut("map", map);
                        result.add(hit);
                    }
                }
                for (KeyValuePair entry : map.keyValuePairs()) {
                    processSequence(entry.value, key, tester, context, result);
                }
            }
        }
    }

}

// Copyright (c) 2018-2022 Saxonica Limited