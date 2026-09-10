package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.om.FocusIterator;

/**
 * Elaborator for a slash expression. (This actually corresponds to the "!" operator, not to "/")
 */

public class SlashExprElaborator extends PullElaborator {

    // TODO: combine ForEach and SlashExpression

    @Override
    public PullEvaluator elaborateForPull() {
        SlashExpression forEach = (SlashExpression) getExpression();
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
        SlashExpression forEach = (SlashExpression)getExpression();
        PullEvaluator select = makeElaborator(forEach.getSelectExpression()).elaborateForPull();
        PushEvaluator action = makeElaborator(forEach.getActionExpression()).elaborateForPush();
        return (out, context) -> {
            XPathContextMinor c2 = context.newMinorContext();
            FocusIterator iter = c2.trackFocus(select.eval(context));
            TailCall tc = null;
            while (iter.next() != null) {
                Expression.dispatchTailCall(tc);
                tc = action.process(out, c2);
            }
            return tc;
        };

    }
}

