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

//    /**
//     * Make an expression that either calls this function, or that is equivalent to a call
//     * on this function
//     *
//     * @param arguments the supplied arguments to the function call
//     * @return either a function call on this function, or an expression that delivers
//     * the same result
//     */
//    @Override
//    public Expression makeFunctionCall(Expression... arguments) {
//        // For the arity-1 function, add identity#1 as the value of the second argument; but bypass this
//        // if we know the values are booleans, in which case no coercion will be needed.
//        if (arguments.length == 1 && arguments[0].getItemType() != BuiltInAtomicType.BOOLEAN) {
//            return SystemFunction.makeFunction40("every", getRetainedStaticContext(), 2)
//                    .makeFunctionCall(arguments[0],
//                                      Literal.makeLiteral(SystemFunction.makeFunction40("identity", getRetainedStaticContext(), 1)));
//        } else {
//            return super.makeFunctionCall(arguments);
//        }
//    }


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
                boolean ebv = item.effectiveBooleanValue();
                if (!ebv) {
                    iter.close();
                    return BooleanValue.FALSE;
                }
            }
            return BooleanValue.TRUE;
        } else {
            int pos = 0;
            for (Item item; (item = iter.next()) != null; ) {
                boolean condition =
                        ((BooleanValue)dynamicCall(predicate, context, item, Int64Value.makeIntegerValue(++pos)).head()).getBooleanValue();
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