package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.query.StaticQueryContext;
import net.sf.saxon.query.XQueryExpression;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.ObjectValue;


/**
 * This class implements the saxon:compile-query() extension function
 */

public class CompileQuery extends SystemFunction {

    /**
     * Evaluate this function call at run-time
     *
     * @param context   The XPath dynamic evaluation context
     * @param arguments The values of the arguments to the function call.
     * @return the results of the function.
     * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs during evaluation of the function.
     */

    @Override
    public ObjectValue<XQueryExpression> call(XPathContext context, Sequence[] arguments) throws XPathException {
        String query = arguments[0].head().getStringValue();
        StaticQueryContext sqc = context.getConfiguration().newStaticQueryContext();
        //sqc.setLanguageVersion(31);
        XQueryExpression exp = sqc.compileQuery(query);
        return new ObjectValue<>(exp);
    }
}


// Copyright (c) 2009-2022 Saxonica Limited
