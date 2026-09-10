package com.saxonica.ee.elab;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.Count;
import net.sf.saxon.value.Int64Value;

public abstract class CountElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        PullEvaluator puller = Elaborator.make(arg).getPullEvaluator();
        return context -> Int64Value.makeIntegerValue(Count.count(puller.eval(context)));
    }


}

