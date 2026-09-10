package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import net.sf.saxon.expr.ContextMappingFunction;
import net.sf.saxon.expr.ContextMappingIterator;
import net.sf.saxon.expr.SlashExpression;
import net.sf.saxon.expr.XPathContextMinor;
import net.sf.saxon.om.FocusIterator;

public class SlashExprElaborator extends Elaborator {

    // TODO: combine ForEach and SlashExpression

    @Override
    public PullEvaluator getPullEvaluator() {
        SlashExpression forEach = (SlashExpression) getExpression();
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
        SlashExpression forEach = (SlashExpression)getExpression();
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

