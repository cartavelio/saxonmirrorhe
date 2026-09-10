package com.saxonica.functions.extfn;

import com.saxonica.functions.qt4.pin.LabeledArrayItem;
import com.saxonica.functions.qt4.pin.LabeledItem;
import com.saxonica.functions.qt4.pin.LabeledMapItem;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.*;
import net.sf.saxon.om.*;
import net.sf.saxon.str.Twine8;
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

    private final static Shape HIT_SHAPE = new Shape(new Twine8("key"), new Twine8("value"), new Twine8("map"));

    private void processSequence(GroundedValue seq, AtomicValue key, FunctionItem tester, XPathContext context, List<Item> result) throws XPathException {
        GroundedValue in = ((Sequence) seq).materialize();
        SequenceIterator iter = in.iterate();
        for (Item item; (item = iter.next()) != null; ) {
            if (item instanceof ArrayItem) {
                ArrayItem array = (ArrayItem)item;
                if (!(array instanceof LabeledItem)) {
                    array = LabeledArrayItem.makePinnedArrayItem(array);
                }
                for (GroundedValue member : array.members()) {
                    processSequence(member, key, tester, context, result);
                }
            } else if (item instanceof MapItem) {
                MapItem map = (MapItem) item;
                if (!(map instanceof LabeledItem)) {
                    map = LabeledMapItem.makePinnedMapItem(map);
                }
                GroundedValue value = map.get(key);
                if (value != null) {
                    Sequence test = tester == null ? BooleanValue.TRUE : tester.call(context, new Sequence[]{value});
                    if (((BooleanValue)test.head()).getBooleanValue()) {
                        result.add(new ShapedMap(HIT_SHAPE, key, value, map));
                    }
                }
                for (KeyValuePair entry : map.keyValuePairs()) {
                    processSequence(entry.value, key, tester, context, result);
                }
            }
        }
    }

}

// Copyright (c) 2018-2023 Saxonica Limited