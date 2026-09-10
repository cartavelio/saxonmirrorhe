package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import net.sf.saxon.expr.CompareToConstant;
import net.sf.saxon.expr.CompareToStringConstant;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.Cardinality;

/**
 * Elaborator for a "compare to string constant" expression
 */

public class CompareToStringConstantElaborator extends BooleanElaborator {

    public BooleanEvaluator elaborateForBoolean() {
        final CompareToStringConstant expression = (CompareToStringConstant) getExpression();
        final Expression arg = expression.getBaseExpression();
        final StringEvaluator argEval = makeElaborator(arg).elaborateForString(false);
        final boolean nullable = Cardinality.allowsZero(expression.getCardinality());
        final int operator = expression.getComparisonOperator();
        final UnicodeString comparand = expression.getComparand();

        return context -> {
            UnicodeString value = argEval.eval(context);
            if (nullable && value == null) {
                return false;
            }
            int c = value.compareTo(comparand);
            return CompareToConstant.interpretComparisonResult(operator, c);
        };

    }


}

