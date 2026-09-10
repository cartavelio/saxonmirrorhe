package com.saxonica.functions.qt4;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.zeno.ZenoChain;
import net.sf.saxon.ma.zeno.ZenoSequence;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.Type;

/**
 * The proposed XPath 4.0 intersperse() function
 */


public class Intersperse extends SystemFunction {

    /**
     * Evaluate the expression as a general function call
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments
     * @return the result of the evaluation
     * @throws XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        SequenceIterator input = arguments[0].iterate();
        GroundedValue separator = arguments[1].materialize();
        // TODO: lazy evaluation as an iterator
        ZenoChain<Item> result = new ZenoChain<>();
        Item first = input.next();
        if (first != null) {
            result = result.add(first);
            for (Item it; (it = input.next()) != null;) {
                result = result.addAll(separator.asIterable());
                result = result.add(it);
            }
        }
        return new ZenoSequence(result);
    }

    @Override
    public Expression makeFunctionCall(Expression[] arguments) {
        return new SystemFunctionCall(this, arguments) {
            @Override
            public ItemType getItemType() {
                return Type.getCommonSuperType(getArg(0).getItemType(), getArg(1).getItemType());
            }
        };
    }

    // TODO: streaming implementation
}

// Copyright (c) 2023 Saxonica Limited.

