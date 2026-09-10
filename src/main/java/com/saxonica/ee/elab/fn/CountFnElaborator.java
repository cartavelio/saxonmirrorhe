package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.Count;
import net.sf.saxon.value.Int64Value;

public class CountElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        PullEvaluator puller = Elaborator.make(arg).getPullEvaluator();
        return context -> Int64Value.makeIntegerValue(Count.count(puller.eval(context)));
    }


}

