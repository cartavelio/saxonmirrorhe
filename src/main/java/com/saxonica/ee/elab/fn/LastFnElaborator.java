package com.saxonica.ee.elab;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.PositionAndLast;
import net.sf.saxon.om.FocusIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Int64Value;

public class LastFnElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
        SystemFunctionCall sfc = (SystemFunctionCall) getExpression();
        PositionAndLast.Last fn = (PositionAndLast.Last) sfc.getTargetFunction();
        if (fn.isContextPossiblyUndefined()) {
            return context -> {
                FocusIterator focus = context.getCurrentIterator();
                if (focus == null) {
                    XPathException e = new XPathException("The context item is absent, so last() is undefined");
                    e.setXPathContext(context);
                    e.setLocation(sfc.getLocation());
                    e.setErrorCode("XPDY0002");
                    throw e;
                }
                return Int64Value.makeIntegerValue(context.getLast());
            };
        } else {
            return context -> Int64Value.makeIntegerValue(context.getLast());
        }
    }

}

