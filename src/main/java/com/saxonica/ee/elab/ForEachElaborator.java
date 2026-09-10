package com.saxonica.ee.elab;

import net.sf.saxon.expr.ContextMappingFunction;
import net.sf.saxon.expr.ContextMappingIterator;
import net.sf.saxon.expr.XPathContextMinor;
import net.sf.saxon.expr.instruct.ForEach;
import net.sf.saxon.om.FocusIterator;

public class ForEachElaborator extends Elaborator {

    @Override
    public PullEvaluator getPullEvaluator() {
        ForEach forEach = (ForEach) getExpression();
        PullEvaluator select = Elaborator.make(forEach.getSelectExpression()).getPullEvaluator();
        PullEvaluator action = Elaborator.make(forEach.getActionExpression()).getPullEvaluator();
        ContextMappingFunction mapper = action::eval;
        return context -> {
            XPathContextMinor c2 = context.newMinorContext();
            c2.trackFocus(select.eval(context));
            return new ContextMappingIterator(mapper, c2);
        };
    }

    @Override
    public PushEvaluator getPushEvaluator() {
        ForEach forEach = (ForEach)getExpression();
        PullEvaluator select = Elaborator.make(forEach.getSelectExpression()).getPullEvaluator();
        PushEvaluator action = Elaborator.make(forEach.getActionExpression()).getPushEvaluator();
        return (out, context) -> {
            XPathContextMinor c2 = context.newMinorContext();
            FocusIterator iter = c2.trackFocus(select.eval(context));
            while (iter.next() != null) {
                action.process(out, c2);
            }
        };

    }
}

