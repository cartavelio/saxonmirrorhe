package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.instruct.Choose;
import net.sf.saxon.tree.iter.EmptyIterator;

/**
 * Elaborator for a "Choose" expression (which may be xsl:if, xsl:choose, or an XPath conditional expression).
 *
 * <p>Provides "push" and "pull" implementations, as well as a singleton implementation.</p>
 */

public class ChooseExprElaborator extends PullElaborator {

    public PullEvaluator elaborateForPull() {
        final Choose expr = (Choose) getExpression();
        final int count = expr.getNumberOfConditions();
        final BooleanEvaluator[] conditions = new BooleanEvaluator[count];
        final PullEvaluator[] actions = new PullEvaluator[count];
        for (int i = 0; i < count; i++) {
            conditions[i] = makeElaborator(expr.getCondition(i)).elaborateForBoolean();
            actions[i] = makeElaborator(expr.getAction(i)).elaborateForPull();
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
    public ItemEvaluator elaborateForItem() {
        final Choose expr = (Choose) getExpression();
        final int count = expr.getNumberOfConditions();
        final BooleanEvaluator[] conditions = new BooleanEvaluator[count];
        final ItemEvaluator[] actions = new ItemEvaluator[count];
        for (int i = 0; i < count; i++) {
            conditions[i] = makeElaborator(expr.getCondition(i)).elaborateForBoolean();
            actions[i] = makeElaborator(expr.getAction(i)).elaborateForItem();
        }
        return (context) -> {
            for (int i = 0; i < count; i++) {
                if (conditions[i].eval(context)) {
                    return actions[i].eval(context);
                }
            }
            return null;
        };
    }

    @Override
    public PushEvaluator elaborateForPush() {
        final Choose expr = (Choose) getExpression();
        final int count = expr.getNumberOfConditions();
        final BooleanEvaluator[] conditions = new BooleanEvaluator[count];
        final PushEvaluator[] actions = new PushEvaluator[count];
        for (int i = 0; i < count; i++) {
            conditions[i] = makeElaborator(expr.getCondition(i)).elaborateForBoolean();
            actions[i] = makeElaborator(expr.getAction(i)).elaborateForPush();
        }
        return (out, context) -> {
            for (int i = 0; i < count; i++) {
                if (conditions[i].eval(context)) {
                    return actions[i].process(out, context);
                }
            }
            return null;
        };
    }
}

