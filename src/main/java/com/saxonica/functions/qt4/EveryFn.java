package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
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
        SequenceIterator iter = arguments[0].iterate();
        if (arguments.length == 1) {
            for (Item item; (item = iter.next()) != null; ) {
                if (item instanceof BooleanValue) {
                    if (!((BooleanValue) item).getBooleanValue()) {
                        iter.close();
                        return BooleanValue.FALSE;
                    }
                } else {
                    throw new XPathException("Items supplied to fn:every#1 must be of type xs:boolean", "XPTY0004").asTypeError();
                }
            }
            return BooleanValue.TRUE;
        } else {
            FunctionItem predicate = (FunctionItem) arguments[1].head();
            switch (predicate.getArity()) {
                case 0: {
                    boolean condition = (((BooleanValue) dynamicCall(predicate, context).head()).getBooleanValue());
                    boolean empty = iter.next() == null;
                    iter.close();
                    return BooleanValue.get(condition || empty);
                }
                case 1: {
                    for (Item item; (item = iter.next()) != null; ) {
                        boolean condition = ((BooleanValue) dynamicCall(predicate, context, item).head()).getBooleanValue();
                        if (!condition) {
                            iter.close();
                            return BooleanValue.FALSE;
                        }
                    }
                    return BooleanValue.TRUE;
                }
                case 2:
                    int pos = 0;
                    for (Item item; (item = iter.next()) != null; ) {
                        boolean condition = ((BooleanValue) dynamicCall(
                                predicate, context, item, Int64Value.makeIntegerValue(++pos)).head()).getBooleanValue();
                        if (!condition) {
                            iter.close();
                            return BooleanValue.FALSE;
                        }
                    }
                    return BooleanValue.TRUE;
                default:
                    throw new XPathException("Unsupported arity " + predicate.getArity() + " in fn:every callback");
            }

        }
    }


}


// Copyright (c) 2023 Saxonica Limited.