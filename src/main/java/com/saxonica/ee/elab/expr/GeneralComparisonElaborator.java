package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import net.sf.saxon.expr.GeneralComparison;
import net.sf.saxon.value.AtomicValue;

/**
 * Elaborator for a general comparison expression such as (A = B).
 */

public class GeneralComparisonElaborator extends BooleanElaborator {

    public BooleanEvaluator elaborateForBoolean() {
        final GeneralComparison exp = (GeneralComparison)getExpression();
        GeneralComparison.ComparisonCardinality cardinality = exp.getComparisonCardinality();

        switch (cardinality) {
            case ONE_TO_ONE: {
                final ItemEvaluator p0 = makeElaborator(exp.getLhsExpression()).elaborateForItem();
                final ItemEvaluator p1 = makeElaborator(exp.getRhsExpression()).elaborateForItem();
                return context -> exp.evaluateOneToOne((AtomicValue)p0.eval(context), (AtomicValue)p1.eval(context), context);
            }
            case MANY_TO_ONE: {
                final PullEvaluator p0 = makeElaborator(exp.getLhsExpression()).elaborateForPull();
                final ItemEvaluator p1 = makeElaborator(exp.getRhsExpression()).elaborateForItem();
                return context -> exp.evaluateManyToOne(p0.eval(context), (AtomicValue) p1.eval(context), context);
            }
            case MANY_TO_MANY: {
                final PullEvaluator p0 = makeElaborator(exp.getLhsExpression()).elaborateForPull();
                final PullEvaluator p1 = makeElaborator(exp.getRhsExpression()).elaborateForPull();
                return context -> exp.evaluateManyToMany(p0.eval(context), p1.eval(context), context);
            }
            default:
                throw new UnsupportedOperationException();
        }

    }



}

