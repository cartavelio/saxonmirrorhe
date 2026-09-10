package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.NumericValue;

/**
 * Elaborator for the functions floor(), ceiling(), round(), round-half-to-even(), and abs()
 */

public class RoundingFnElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final ItemEvaluator argEval = makeElaborator(fnc.getArg(0)).elaborateForItem();
        final boolean nullable = Cardinality.allowsZero(fnc.getArg(0).getCardinality());
        final String name = fnc.getFunctionName().getLocalPart();
        final ItemEvaluator scaleArg = fnc.getArity() == 2
                ? makeElaborator(fnc.getArg(1)).elaborateForItem()
                : null;
        switch (name) {
            case "abs":
                return context -> {
                    NumericValue result = (NumericValue) argEval.eval(context);
                    if (nullable && result == null) {
                        return null;
                    }
                    return result.abs();
                };
            case "floor":
                return context -> {
                    NumericValue result = (NumericValue) argEval.eval(context);
                    if (nullable && result == null) {
                        return null;
                    }
                    return result.floor();
                };
            case "ceiling":
                return context -> {
                    NumericValue result = (NumericValue) argEval.eval(context);
                    if (nullable && result == null) {
                        return null;
                    }
                    return result.ceiling();
                };
            case "round":
                return context -> {
                    NumericValue result = (NumericValue) argEval.eval(context);
                    if (nullable && result == null) {
                        return null;
                    }
                    int scale = 0;
                    if (scaleArg != null) {
                        scale = (int)((NumericValue)scaleArg.eval(context)).longValue();
                    }
                    return result.round(scale);
                };
            case "round-half-to-even":
                return context -> {
                    NumericValue result = (NumericValue) argEval.eval(context);
                    if (nullable && result == null) {
                        return null;
                    }
                    int scale = 0;
                    if (scaleArg != null) {
                        scale = (int) ((NumericValue) scaleArg.eval(context)).longValue();
                    }
                    return result.roundHalfToEven(scale);
                };
            default: throw new UnsupportedOperationException();

        }

    }


}

