package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.QNameFn;
import net.sf.saxon.value.StringValue;

public class QNameFnElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        SystemFunctionCall sfc = (SystemFunctionCall) getExpression();
        ItemEvaluator arg0eval = makeElaborator(sfc.getArg(0)).elaborateForItem();
        ItemEvaluator arg1eval = makeElaborator(sfc.getArg(1)).elaborateForItem();
        return context -> QNameFn.expandedQName((StringValue)arg0eval.eval(context), (StringValue)arg1eval.eval(context));
    }

}

