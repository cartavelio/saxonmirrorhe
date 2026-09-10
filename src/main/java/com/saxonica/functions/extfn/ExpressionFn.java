package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.ObjectValue;

/**
 * This class implements the saxon:expression() extension function.
 */

public class ExpressionFn extends Evaluate {

    @Override
    public ObjectValue<PreparedExpression> call(XPathContext context, Sequence[] arguments) throws XPathException {
        XPathContextMajor c2 = context.newCleanContext();
        String exprText = arguments[0].head().getStringValue();
        NodeInfo namespaceOrigin = null;
        if (arguments.length > 1) {
            namespaceOrigin = (NodeInfo) arguments[1].head();
        }
        PreparedExpression pexpr = prepareExpression(exprText, namespaceOrigin, getStaticBaseUriString(), "saxon:expression", c2);
        return new ObjectValue<>(pexpr);
    }

}

// Copyright (c) 2009-2022 Saxonica Limited
