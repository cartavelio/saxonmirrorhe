package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.Number_1;
import net.sf.saxon.value.AtomicValue;

public class NumberFnElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        ItemEvaluator argEval = Elaborator.make(fnc.getArg(0)).getItemEvaluator();
        return context -> Number_1.toNumber((AtomicValue)argEval.eval(context));
    }


}

