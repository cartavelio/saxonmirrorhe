package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.om.Sequence;

/**
 * Elaborator for a system function call, used in cases where the specific function call has no custom support
 */

public class SystemFunctionCallElaborator extends PullElaborator {

    public PullEvaluator elaborateForPull() {
        final SystemFunctionCall expr = (SystemFunctionCall) getExpression();
        return context -> {
            Sequence[] arguments = expr.evaluateArguments(context);
            Sequence result = expr.call(context, arguments);
            return result.iterate();
        };
    }

    @Override
    public ItemEvaluator elaborateForItem() {
        final SystemFunctionCall expr = (SystemFunctionCall) getExpression();
        return context -> {
            Sequence[] arguments = expr.evaluateArguments(context);
            Sequence result = expr.call(context, arguments);
            return result.head();
        };
    }
}

