package com.saxonica.functions.extfn;

import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.om.*;
import net.sf.saxon.query.DynamicQueryContext;
import net.sf.saxon.query.StaticQueryContext;
import net.sf.saxon.query.XQueryExpression;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.QNameValue;
import net.sf.saxon.value.SequenceType;


/**
 * This class implements the saxon:xquery() extension function
 */

public class XQueryFn extends SystemFunction {

    /**
     * Evaluate this function call at run-time
     *
     * @param context   The XPath dynamic evaluation context
     * @param arguments The values of the arguments to the function call.
     * @return the results of the function.
     * @throws XPathException if a dynamic error occurs during evaluation of the function.
     */

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        String query = arguments[0].head().getStringValue();
        StaticQueryContext sqc = context.getConfiguration().newStaticQueryContext();
        //sqc.setLanguageVersion(31);
        XQueryExpression exp = sqc.compileQuery(query);
        CallableDelegate.Lambda callable = (cxt, args) -> {
            DynamicQueryContext dqc = new DynamicQueryContext(cxt.getConfiguration());
            Item arg0 = args[0].head();
            if (arg0 != null) {
                dqc.setContextItem(arg0);
            }
            MapItem paramMap = (MapItem)args[1].head();
            for (KeyValuePair kvp : paramMap.keyValuePairs()) {
                QNameValue key = (QNameValue)kvp.key;
                GroundedValue value = kvp.value;
                dqc.setParameter(key.getStructuredQName(), value);
            }
            SequenceIterator iter = exp.iterator(dqc);
            return new LazySequence(iter);
        };
        return new CallableFunction(callable, functionType)
                .withDescription("saxon:xquery");
    }

    private static final MapType paramMap = new MapType(BuiltInAtomicType.QNAME, SequenceType.ANY_SEQUENCE);
    public static final SpecificFunctionType functionType = new SpecificFunctionType(
            SequenceType.OPTIONAL_ITEM,
            SequenceType.one(paramMap),
            SequenceType.ANY_SEQUENCE);
}


// Copyright (c) 2009-2023 Saxonica Limited
