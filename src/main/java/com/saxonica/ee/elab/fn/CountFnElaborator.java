package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.Count;
import net.sf.saxon.value.Int64Value;

public class CountFnElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        PullEvaluator puller = makeElaborator(arg).elaborateForPull();
        return context -> Int64Value.makeIntegerValue(Count.count(puller.eval(context)));
    }


}

