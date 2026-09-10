package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.ValueComparison;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.Cardinality;

/**
 * Elaborator for a value comparison (such as {@code A eq B}), including the case
 * where a general comparison is reduced to a value comparison by the optimiser
 */

public class ValueComparisonElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        final ValueComparison exp = (ValueComparison)getExpression();
        final ItemEvaluator p0 = makeElaborator(exp.getLhsExpression()).elaborateForItem();
        final ItemEvaluator p1 = makeElaborator(exp.getRhsExpression()).elaborateForItem();
        final AtomicComparer comparer = exp.getAtomicComparer();
        final BooleanValue resultWhenEmpty = exp.getResultWhenEmpty();
        final int operator = exp.getOperator();
        final boolean needsRuntimeCheck = exp.needsRuntimeComparabilityCheck();
        final boolean nullable0 = Cardinality.allowsZero(exp.getLhsExpression().getCardinality());
        final boolean nullable1 = Cardinality.allowsZero(exp.getRhsExpression().getCardinality());

        return context -> {
            AtomicValue v0 = (AtomicValue)p0.eval(context);
            if (nullable0 && v0 == null) {
                return resultWhenEmpty;  // normally false
            }
            AtomicValue v1 = (AtomicValue)p1.eval(context);
            if (nullable1 && v1 == null) {
                return resultWhenEmpty;  // normally false
            }
            return BooleanValue.get(
                    ValueComparison.compare(v0, operator, v1, comparer.provideContext(context), needsRuntimeCheck));
        };

    }



}

