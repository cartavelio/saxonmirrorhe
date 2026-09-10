package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.IContextAccessorFunction;
import net.sf.saxon.functions.StatefulSystemFunction;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.SameNameTest;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.iter.PrependAxisIterator;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.Int64Value;

/**
 * This class implements the function fn:element-number(), which is a proposed function for XPath 4.0
 */

public class ElementNumber extends SystemFunction implements IContextAccessorFunction, StatefulSystemFunction {

    private NodeInfo contextNode;

    /**
     * Bind context information to appear as part of the function's closure. If this method
     * has been called, the supplied context will be used in preference to the
     * context at the point where the function is actually called.
     *
     * @param context the context to which the function applies. Must not be null.
     */
    @Override
    public FunctionItem bindContext(XPathContext context) throws XPathException {
        contextNode = (NodeInfo)context.getContextItem();
        return this;
    }

    @Override
    public SystemFunction copy() {
        ElementNumber en2 = (ElementNumber) SystemFunction.makeFunction(getFunctionName().getLocalPart(), getRetainedStaticContext(), getArity());
        en2.contextNode = contextNode;
        return en2;
    }

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
        NodeInfo target = null;
        if (arguments.length > 0) {
            target = (NodeInfo) arguments[0].head();
        }
        if (target == null) {
            target = contextNode;  // TODO: needs error checking
        }
        NodeInfo root = null;
        if (arguments.length > 1) {
            root = (NodeInfo)arguments[1].head();
        }
        if (root == null) {
            root = target.getRoot();
        }
        FunctionItem predicate = null;
        if (arguments.length > 2) {
            predicate = (FunctionItem) arguments[2].head();
        }
        int count = 0;
        NodeInfo item;
        if (predicate == null) {
            AxisIterator iter = target.iterateAxis(AxisInfo.PRECEDING_OR_ANCESTOR, new SameNameTest(target));
            count = 1;
            while ((item = iter.next()) != null) {
                if (Navigator.isAncestorOrSelf(root, item)) {
                    count++;
                } else {
                    break;
                }
            }
        } else {
            AxisIterator iter = target.iterateAxis(AxisInfo.PRECEDING_OR_ANCESTOR, new SameNameTest(target));
            iter = new PrependAxisIterator(target, iter);
            while ((item = iter.next()) != null) {
                if (Navigator.isAncestorOrSelf(root, item)) {
                    BooleanValue condition =
                            ((BooleanValue) dynamicCall(predicate, context, item).head());
                    if (condition != null && condition.getBooleanValue()) {
                        count++;
                    }
                } else {
                    break;
                }

            }
        }
        return Int64Value.makeIntegerValue(count);
    }


}


// Copyright (c) 2023 Saxonica Limited.