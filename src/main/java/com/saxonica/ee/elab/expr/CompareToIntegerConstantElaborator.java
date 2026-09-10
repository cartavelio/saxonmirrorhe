package com.saxonica.ee.elab;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import com.saxonica.ee.elab.gen.Elaborator;
import net.sf.saxon.expr.CompareToConstant;
import net.sf.saxon.expr.CompareToIntegerConstant;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.NumericValue;

import java.math.BigDecimal;

public class CompareToIntegerConstantElaborator extends BooleanElaborator {

    public BooleanEvaluator getBooleanEvaluator() {
        CompareToIntegerConstant expression = (CompareToIntegerConstant) getExpression();
        Expression arg = expression.getBaseExpression();
        ItemEvaluator argEval = Elaborator.make(arg).getItemEvaluator();
        int operator = expression.getComparisonOperator();
        long comparand = expression.getComparand();

        TypeHierarchy th = getConfiguration().getTypeHierarchy();
        ItemType operandType = arg.getItemType();

        boolean isDouble = th.isSubType(operandType, BuiltInAtomicType.DOUBLE);
        boolean isFloat = th.isSubType(operandType, BuiltInAtomicType.FLOAT);
        boolean isInteger = th.isSubType(operandType, BuiltInAtomicType.INTEGER);
        boolean isDecimal = (!isInteger) && th.isSubType(operandType, BuiltInAtomicType.DECIMAL);

        if (isInteger) {
            return context -> {
                IntegerValue val = (IntegerValue) argEval.eval(context);
                return CompareToConstant.interpretComparisonResult(operator, val.compareTo(comparand));
            };

        }

        if (isDouble || isFloat) {
            // convert constant to double in advance
            final double constant = (double) comparand;
            return context -> {
                double val = ((NumericValue) argEval.eval(context)).getDoubleValue();
                if (Double.isNaN(val)) {
                    return operator == Token.FNE;
                }
                return CompareToConstant.interpretComparisonResult(operator, Double.compare(val, constant));
            };
        }

        if (isDecimal) {
            // convert constant to BigDecimal in advance
            final BigDecimal deci = new BigDecimal(comparand);
            return context -> {
                BigDecimal val = ((NumericValue) argEval.eval(context)).getDecimalValue();
                return CompareToConstant.interpretComparisonResult(operator, val.compareTo(deci));
            };
        }

        // Otherwise, the type is not statically known or is known to be untyped

        return context -> {
            NumericValue num = ((NumericValue) argEval.eval(context));
            if (num.isNaN()) {
                return operator == Token.FNE;
            }
            int c = num.compareTo(comparand);
            return CompareToConstant.interpretComparisonResult(operator, c);
        };

    }


}

