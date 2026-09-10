package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.PullElaborator;
import com.saxonica.ee.optim.SwitchExpression;
import net.sf.saxon.expr.ContextMappingIterator;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.value.AtomicValue;

import java.util.ArrayList;
import java.util.List;

/**
 * Elaborator for a "Switch" expression (typically an {@code xsl:choose} that tests one expression against a list
 * of constants: not exactly equivalent to an XQuery 3.1 switch expression).
 *
 * <p>Provides "push" and "pull" implementations, as well as a singleton implementation.</p>
 */

public class SwitchExprElaborator extends PullElaborator {

    public PullEvaluator elaborateForPull() {
        final SwitchExpression expr = (SwitchExpression) getExpression();
        final ItemEvaluator subjectEvaluator = Elaborator.makeElaborator(expr.getSubjectExpression()).elaborateForItem();
        final List<PullEvaluator> actionEvaluators = new ArrayList<>(expr.getActions().size());
        for (Operand o : expr.getActions()) {
            actionEvaluators.add(Elaborator.makeElaborator(o.getChildExpression()).elaborateForPull());
        }
        final PullEvaluator otherwiseEvaluator = Elaborator.makeElaborator(expr.getOtherwiseExpression()).elaborateForPull();
        return context -> {
            AtomicValue subjectValue = (AtomicValue) subjectEvaluator.eval(context);
            if (subjectValue == null) {
                return otherwiseEvaluator.eval(context);
            }
            AtomicMatchKey key = subjectValue.getXPathMatchKey(expr.getCollation(), context.getImplicitTimezone());
            int selection = expr.getIndex(key);
            if (selection == -1) {
                return otherwiseEvaluator.eval(context);
            } else {
                return actionEvaluators.get(selection).eval(context);
            }
        };
    }

    @Override
    public ItemEvaluator elaborateForItem() {
        final SwitchExpression expr = (SwitchExpression) getExpression();
        final ItemEvaluator subjectEvaluator = Elaborator.makeElaborator(expr.getSubjectExpression()).elaborateForItem();
        final List<ItemEvaluator> actionEvaluators = new ArrayList<>(expr.getActions().size());
        for (Operand o : expr.getActions()) {
            actionEvaluators.add(Elaborator.makeElaborator(o.getChildExpression()).elaborateForItem());
        }
        final ItemEvaluator otherwiseEvaluator = Elaborator.makeElaborator(expr.getOtherwiseExpression()).elaborateForItem();
        return context -> {
            AtomicValue subjectValue = (AtomicValue) subjectEvaluator.eval(context);
            if (subjectValue == null) {
                return otherwiseEvaluator.eval(context);
            }
            AtomicMatchKey key = subjectValue.getXPathMatchKey(expr.getCollation(), context.getImplicitTimezone());
            int selection = expr.getIndex(key);
            if (selection == -1) {
                return otherwiseEvaluator.eval(context);
            } else {
                return actionEvaluators.get(selection).eval(context);
            }
        };
    }

    @Override
    public PushEvaluator elaborateForPush() {
        final SwitchExpression expr = (SwitchExpression) getExpression();
        final ItemEvaluator subjectEvaluator = Elaborator.makeElaborator(expr.getSubjectExpression()).elaborateForItem();
        final List<PushEvaluator> actionEvaluators = new ArrayList<>(expr.getActions().size());
        for (Operand o : expr.getActions()) {
            actionEvaluators.add(Elaborator.makeElaborator(o.getChildExpression()).elaborateForPush());
        }
        final PushEvaluator otherwiseEvaluator = Elaborator.makeElaborator(expr.getOtherwiseExpression()).elaborateForPush();
        return (out, context) -> {
            AtomicValue subjectValue = (AtomicValue) subjectEvaluator.eval(context);
            if (subjectValue == null) {
                return otherwiseEvaluator.process(out, context);
            }
            AtomicMatchKey key = subjectValue.getXPathMatchKey(expr.getCollation(), context.getImplicitTimezone());
            int selection = expr.getIndex(key);
            if (selection == -1) {
                return otherwiseEvaluator.process(out, context);
            } else {
                return actionEvaluators.get(selection).process(out, context);
            }
        };
    }
}

