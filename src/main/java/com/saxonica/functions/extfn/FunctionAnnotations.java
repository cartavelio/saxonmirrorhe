package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.TupleItemType;
import net.sf.saxon.om.AtomicArray;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.query.Annotation;
import net.sf.saxon.query.AnnotationList;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.QNameValue;
import net.sf.saxon.value.SequenceExtent;
import net.sf.saxon.value.SequenceType;

import java.util.ArrayList;
import java.util.List;

/**
 * Implementation of the saxon:function-annotations() extension function. Introduced in 9.8. Returns information
 * about the function annotations on a function value.
 */

public class FunctionAnnotations extends SystemFunction {


    private static List<String> returnTypeNames = new ArrayList<>();
    private static List<SequenceType> returnTypeTypes = new ArrayList<>();


    static {
        returnTypeNames.add("name");
        returnTypeTypes.add(SequenceType.SINGLE_QNAME);
        returnTypeNames.add("params");
        returnTypeTypes.add(SequenceType.ATOMIC_SEQUENCE);
    }

    public final static TupleItemType RETURN_TYPE = new TupleItemType(returnTypeNames, returnTypeTypes);


    @Override
    public Sequence<?> call(XPathContext context, Sequence[] arguments) throws XPathException {
        Function fn = (Function) arguments[0].head();
        AnnotationList annotations = fn.getAnnotations();

        List<MapItem> result = new ArrayList<>();
        for (Annotation ann : annotations) {
            DictionaryMap map = new DictionaryMap();
            map.initialPut("name", new QNameValue(ann.getAnnotationQName(), BuiltInAtomicType.QNAME));
            map.initialPut("params", new AtomicArray(ann.getAnnotationParameters()));
            result.add(map);
        }
        return SequenceExtent.makeSequenceExtent(result);
    }

}

// Copyright (c) 2012 Saxonica Limited.