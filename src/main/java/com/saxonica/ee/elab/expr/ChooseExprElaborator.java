package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.instruct.Choose;
import net.sf.saxon.tree.iter.EmptyIterator;

public class ChooseExprElaborator extends PullElaborator {

    public PullEvaluator getPullEvaluator() {
        final Choose expr = (Choose) getExpression();
        final int count = expr.getNumberOfConditions();
        final BooleanEvaluator[] conditions = new BooleanEvaluator[count];
        final PullEvaluator[] actions = new PullEvaluator[count];
        for (int i = 0; i < count; i++) {
            conditions[i] = Elaborator.make(expr.getCondition(i)).getBooleanEvaluator();
            actions[i] = Elaborator.make(expr.getAction(i)).getPullEvaluator();
        }
        return context -> {
            for (int i = 0; i < count; i++) {
                if (conditions[i].eval(context)) {
                    return actions[i].eval(context);
                }
            }
            return EmptyIterator.getInstance();
        };
    }

    @Override
    public ItemEvaluator getItemEvaluator() {
        final Choose expr = (Choose) getExpression();
        final int count = expr.getNumberOfConditions();
        final BooleanEvaluator[] conditions = new BooleanEvaluator[count];
        final ItemEvaluator[] actions = new ItemEvaluator[count];
        for (int i = 0; i < count; i++) {
            conditions[i] = Elaborator.make(expr.getCondition(i)).getBooleanEvaluator();
            actions[i] = Elaborator.make(expr.getAction(i)).getItemEvaluator();
        }
        return (context) -> {
            for (int i = 0; i < count; i++) {
                if (conditions[i].eval(context)) {
                    actions[i].eval(context);
                    break;
                }
            }
            return null;
        };
    }

    @Override
    public PushEvaluator getPushEvaluator() {
        final Choose expr = (Choose) getExpression();
        final int count = expr.getNumberOfConditions();
        final BooleanEvaluator[] conditions = new BooleanEvaluator[count];
        final PushEvaluator[] actions = new PushEvaluator[count];
        for (int i = 0; i < count; i++) {
            conditions[i] = Elaborator.make(expr.getCondition(i)).getBooleanEvaluator();
            actions[i] = Elaborator.make(expr.getAction(i)).getPushEvaluator();
        }
        return (out, context) -> {
            for (int i = 0; i < count; i++) {
                if (conditions[i].eval(context)) {
                    actions[i].process(out, context);
                    break;
                }
            }
        };
    }
}

