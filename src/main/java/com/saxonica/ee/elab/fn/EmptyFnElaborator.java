package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import com.saxonica.ee.elab.gen.Elaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;

public class EmptyFnElaborator extends BooleanElaborator {

    public BooleanEvaluator getBooleanEvaluator() {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        PullEvaluator puller = Elaborator.make(arg).getPullEvaluator();
        return context -> {
            SequenceIterator iter = puller.eval(context);
            Item nextItem = iter.next();
            iter.close();
            return nextItem == null;
        };
    }


}

