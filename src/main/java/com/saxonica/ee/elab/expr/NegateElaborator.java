package com.saxonica.ee.elab;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.NegateExpression;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.DoubleValue;
import net.sf.saxon.value.NumericValue;

public class NegateElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
        final NegateExpression exp = (NegateExpression)getExpression();
        final ItemEvaluator argEval = Elaborator.make(exp.getBaseExpression()).getItemEvaluator();
        final boolean maybeEmpty = Cardinality.allowsZero(exp.getBaseExpression().getCardinality());
        final boolean backwardsCompatible = exp.isBackwardsCompatible();
        return context -> {
            NumericValue v1 = (NumericValue)argEval.eval(context);
            if (maybeEmpty && v1 == null) {
                return backwardsCompatible ? DoubleValue.NaN : null;
            }
            return v1.negate();
        };
    }

}

