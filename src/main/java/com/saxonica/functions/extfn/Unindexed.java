package com.saxonica.functions.extfn;

import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.ContextItemStaticInfo;
import net.sf.saxon.expr.parser.ExpressionVisitor;
import net.sf.saxon.expr.sort.DocumentSorter;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;

/**
 * The extension function saxon:unindexed returns the value of its argument unchanged. A static call
 * on saxon:unindexed will generally be optimized away; but if the argument is a filter expression,
 * then it disables the use of indexes to optimize that filter expression. This is useful in cases
 * where indexing would achieve no benefit.
 */

public class Unindexed extends SystemFunction {

    @Override
    public Expression makeOptimizedFunctionCall(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo, Expression... arguments) throws XPathException {
        Expression arg = arguments[0];
        while (arg instanceof UnaryExpression) {
            if (arg instanceof ItemChecker || arg instanceof CardinalityChecker || arg instanceof DocumentSorter || arg instanceof HomogeneityChecker) {
                arg = ((UnaryExpression)arg).getBaseExpression();
            }
        }
        if (arg instanceof FilterExpression) {
            ((FilterExpression)arg).disableIndexing();
        } else if (arg instanceof SlashExpression) {
            ((SlashExpression) arg).disableIndexing();
        }
        return arguments[0];
    }

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        return arguments[0];
    }

}

// Copyright (c) 2012-2023 Saxonica Limited