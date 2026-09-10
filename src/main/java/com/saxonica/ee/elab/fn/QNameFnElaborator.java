package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.QNameFn;
import net.sf.saxon.value.StringValue;

public class QNameFnElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
        SystemFunctionCall sfc = (SystemFunctionCall) getExpression();
        ItemEvaluator arg0eval = Elaborator.make(sfc.getArg(0)).getItemEvaluator();
        ItemEvaluator arg1eval = Elaborator.make(sfc.getArg(1)).getItemEvaluator();
        return context -> QNameFn.expandedQName((StringValue)arg0eval.eval(context), (StringValue)arg1eval.eval(context));
    }

}

