package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.ContextMappingIterator;
import net.sf.saxon.expr.LetExpression;
import net.sf.saxon.expr.parser.Evaluator;
import net.sf.saxon.om.Sequence;

/**
 * Elaborator for a let expression (either {@code let $x := SELECT return ACTION} in XPath, or the
 * equivalent sequence constructor using local {@code xsl:variable} declarations in XSLT).
 *
 * <p>The elaborator allows evaluation in pull, push, or singleton mode.</p>
 */

public class LetExprElaborator extends PullElaborator {

    // TODO: evaluate nested let expressions in a loop to reduce stack consumption

    @Override
    public PullEvaluator elaborateForPull() {
        final LetExpression expr = (LetExpression) getExpression();
        final Evaluator.ElaboratedEvaluator selectEval = expr.getEvaluator().elaborate(expr.getSequence());
        final PullEvaluator actionPull = makeElaborator(expr.getAction()).elaborateForPull();
        final int slot = expr.getLocalSlotNumber();
        return context -> {
            Sequence value = selectEval.eval(context);
            context.setLocalVariable(slot, value);
            return actionPull.eval(context);
        };
    }

    @Override
    public PushEvaluator elaborateForPush() {
        final LetExpression expr = (LetExpression) getExpression();
        final Evaluator.ElaboratedEvaluator selectEval = expr.getEvaluator().elaborate(expr.getSequence());
        final PushEvaluator actionPush = makeElaborator(expr.getAction()).elaborateForPush();
        final int slot = expr.getLocalSlotNumber();
        return (out, context) -> {
            Sequence value = selectEval.eval(context);
            context.setLocalVariable(slot, value);
            return actionPush.process(out, context);
        };
    }

    @Override
    public ItemEvaluator elaborateForItem() {
        final LetExpression expr = (LetExpression) getExpression();
        final Evaluator.ElaboratedEvaluator selectEval = expr.getEvaluator().elaborate(expr.getSequence());
        final ItemEvaluator actionEval = makeElaborator(expr.getAction()).elaborateForItem();
        final int slot = expr.getLocalSlotNumber();
        return context -> {
            Sequence value = selectEval.eval(context);
            context.setLocalVariable(slot, value);
            return actionEval.eval(context);
        };
    }
}

