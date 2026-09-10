package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.Int64Value;

/**
 * This class implements the function fn:every(), which is a proposed function for XPath 4.0
 */

public class EveryFn extends SystemFunction {

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as SequenceIterators
     * @return the result of the evaluation, in the form of a SequenceIterator
     * @throws XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        FunctionItem predicate = null;
        if (arguments.length > 1) {
            predicate = (FunctionItem) arguments[1].head();
        }
        SequenceIterator iter = arguments[0].iterate();
        if (predicate == null) {
            for (Item item; (item = iter.next()) != null; ) {
                boolean ebv = ExpressionTool.effectiveBooleanValue(item);
                if (!ebv) {
                    iter.close();
                    return BooleanValue.FALSE;
                }
            }
            return BooleanValue.TRUE;
        } else {
            int pos = 0;
            for (Item item; (item = iter.next()) != null; ) {
                boolean condition = ExpressionTool.effectiveBooleanValue(
                        dynamicCall(predicate, context, item, Int64Value.makeIntegerValue(++pos)).iterate());
                if (!condition) {
                    iter.close();
                    return BooleanValue.FALSE;
                }
            }
            return BooleanValue.TRUE;
        }
    }


}


// Copyright (c) 2023 Saxonica Limited.