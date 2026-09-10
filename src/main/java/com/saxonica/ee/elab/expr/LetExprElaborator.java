package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.LetExpression;
import net.sf.saxon.expr.parser.Evaluator;
import net.sf.saxon.om.Sequence;

public class LetExprElaborator extends PullElaborator {

    @Override
    public PullEvaluator getPullEvaluator() {
        final LetExpression expr = (LetExpression) getExpression();
        final Evaluator.ElaboratedEvaluator selectEval = expr.getEvaluator().elaborate(expr.getSequence());
        final PullEvaluator actionPull = Elaborator.make(expr.getAction()).getPullEvaluator();
        final int slot = expr.getLocalSlotNumber();
        return context -> {
            Sequence value = selectEval.eval(context);
            context.setLocalVariable(slot, value);
            return actionPull.eval(context);
        };
    }

    @Override
    public PushEvaluator getPushEvaluator() {
        final LetExpression expr = (LetExpression) getExpression();
        final Evaluator.ElaboratedEvaluator selectEval = expr.getEvaluator().elaborate(expr.getSequence());
        final PushEvaluator actionPush = Elaborator.make(expr.getAction()).getPushEvaluator();
        final int slot = expr.getLocalSlotNumber();
        return (out, context) -> {
            Sequence value = selectEval.eval(context);
            context.setLocalVariable(slot, value);
            actionPush.process(out, context);
        };
    }

    @Override
    public ItemEvaluator getItemEvaluator() {
        final LetExpression expr = (LetExpression) getExpression();
        final Evaluator.ElaboratedEvaluator selectEval = expr.getEvaluator().elaborate(expr.getSequence());
        final ItemEvaluator actionEval = Elaborator.make(expr.getAction()).getItemEvaluator();
        final int slot = expr.getLocalSlotNumber();
        return context -> {
            Sequence value = selectEval.eval(context);
            context.setLocalVariable(slot, value);
            return actionEval.eval(context);
        };
    }
}

