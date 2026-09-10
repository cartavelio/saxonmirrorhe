package com.saxonica.ee.elab.insn;

import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.ContextMappingFunction;
import net.sf.saxon.expr.ContextMappingIterator;
import net.sf.saxon.expr.XPathContextMinor;
import net.sf.saxon.expr.instruct.ForEach;
import net.sf.saxon.om.FocusIterator;

public class ForEachElaborator extends PullElaborator {

    @Override
    public PullEvaluator elaborateForPull() {
        ForEach forEach = (ForEach) getExpression();
        PullEvaluator select = makeElaborator(forEach.getSelectExpression()).elaborateForPull();
        PullEvaluator action = makeElaborator(forEach.getActionExpression()).elaborateForPull();
        ContextMappingFunction mapper = context -> action.eval(context);
        return context -> {
            XPathContextMinor c2 = context.newMinorContext();
            c2.trackFocus(select.eval(context));
            return new ContextMappingIterator(mapper, c2);
        };
    }

    @Override
    public PushEvaluator elaborateForPush() {
        ForEach forEach = (ForEach)getExpression();
        PullEvaluator select = makeElaborator(forEach.getSelectExpression()).elaborateForPull();
        PushEvaluator action = makeElaborator(forEach.getActionExpression()).elaborateForPush();
        return (out, context) -> {
            XPathContextMinor c2 = context.newMinorContext();
            FocusIterator iter = c2.trackFocus(select.eval(context));
            while (iter.next() != null) {
                action.process(out, c2);
            }
        };

    }
}

