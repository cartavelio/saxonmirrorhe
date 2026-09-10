package com.saxonica.ee.elab;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import com.saxonica.ee.elab.gen.Elaborator;
import net.sf.saxon.expr.CompareToConstant;
import net.sf.saxon.expr.CompareToStringConstant;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.str.UnicodeString;

public class CompareToStringConstantElaborator extends BooleanElaborator {

    public BooleanEvaluator getBooleanEvaluator() {
        CompareToStringConstant expression = (CompareToStringConstant) getExpression();
        Expression arg = expression.getBaseExpression();
        StringEvaluator argEval = Elaborator.make(arg).getStringEvaluator(null);
        int operator = expression.getComparisonOperator();
        UnicodeString comparand = expression.getComparand();

        return context -> {
            UnicodeString value = argEval.eval(context);
            if (value == null) {
                return false;
            }
            int c = value.compareTo(comparand);
            return CompareToConstant.interpretComparisonResult(operator, c);
        };

    }


}

