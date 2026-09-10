package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.ObjectValue;

/**
 * This class implements the saxon:eval() extension function
 */

public class Eval extends Evaluate {

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        XPathContextMajor c2 = context.newCleanContext();
        Item head = arguments[0].head();
        ObjectValue<PreparedExpression> oval = (ObjectValue<Evaluate.PreparedExpression>)head;
        Evaluate.PreparedExpression pexpr = oval.getObject();
        c2.setOrigin(pexpr);
        c2.openStackFrame(pexpr.stackFrameMap);
        for (int i = 1; i < arguments.length; i++) {
            int slot = pexpr.variables[i - 1].getLocalSlotNumber();
            c2.setLocalVariable(slot, arguments[i].materialize());
        }
        c2.setCurrentIterator(context.getCurrentIterator());
        return SequenceTool.toLazySequence(pexpr.expression.iterate(c2));
    }

}

// Copyright (c) 2009-2022 Saxonica Limited
