package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.ForExpression;
import net.sf.saxon.expr.ItemMappingIterator;
import net.sf.saxon.expr.MappingIterator;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.value.Cardinality;

public class ForExprElaborator extends PullElaborator {

    public PullEvaluator getPullEvaluator() {
        final ForExpression expr = (ForExpression) getExpression();
        final PullEvaluator selectEval = Elaborator.make(expr.getSequence()).getPullEvaluator();
        final int actionCardinality = expr.getAction().getCardinality();
        final int slot = expr.getLocalSlotNumber();
        if (Cardinality.allowsMany(actionCardinality)) {
            final PullEvaluator actionEval = Elaborator.make(expr.getAction()).getPullEvaluator();
            return context -> {
                SequenceIterator base = selectEval.eval(context);
                return new MappingIterator(base, item -> {
                    context.setLocalVariable(slot, item);
                    return actionEval.eval(context);
                });
            };
        } else {
            final ItemEvaluator actionEval = Elaborator.make(expr.getAction()).getItemEvaluator();
            return context -> {
                SequenceIterator base = selectEval.eval(context);
                return new ItemMappingIterator(base, item -> {
                    context.setLocalVariable(slot, item);
                    return actionEval.eval(context);
                });
            };
        }
    }

    @Override
    public PushEvaluator getPushEvaluator() {
        final ForExpression expr = (ForExpression) getExpression();
        final PullEvaluator selectEval = Elaborator.make(expr.getSequence()).getPullEvaluator();
        final PushEvaluator actionEval = Elaborator.make(expr.getAction()).getPushEvaluator();
        final int slot = expr.getLocalSlotNumber();
        return (out, context) -> {
            SequenceIterator base = selectEval.eval(context);
            Item item;
            while ((item = base.next()) != null) {
                context.setLocalVariable(slot, item);
                actionEval.process(out, context);
            }
        };
    }
}

