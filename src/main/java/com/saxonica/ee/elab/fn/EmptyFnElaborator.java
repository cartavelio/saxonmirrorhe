package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;

public class EmptyFnElaborator extends BooleanElaborator {

    public BooleanEvaluator elaborateForBoolean() {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        PullEvaluator puller = makeElaborator(arg).elaborateForPull();
        return context -> {
            SequenceIterator iter = puller.eval(context);
            Item nextItem = iter.next();
            iter.close();
            return nextItem == null;
        };
    }


}

