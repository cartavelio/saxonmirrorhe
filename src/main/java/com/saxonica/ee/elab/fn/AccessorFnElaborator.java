package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.value.NumericValue;

/**
 * Elaborator for the functions floor(), ceiling(), round(), round-half-to-even(), and abs()
 */

public class RoundingFnElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final ItemEvaluator argEval = Elaborator.make(fnc.getArg(0)).getItemEvaluator();
        final String name = fnc.getFunctionName().getLocalPart();
        final ItemEvaluator scaleArg = fnc.getArity() == 2
                ? Elaborator.make(fnc.getArg(1)).getItemEvaluator()
                : null;
        switch (name) {
            case "abs":
                return context -> ((NumericValue)argEval.eval(context)).abs();
            case "floor":
                return context -> ((NumericValue) argEval.eval(context)).floor();
            case "ceiling":
                return context -> ((NumericValue) argEval.eval(context)).ceiling();
            case "round":
                return context -> {
                    int scale = 0;
                    if (scaleArg != null) {
                        scale = (int)((NumericValue)scaleArg.eval(context)).longValue();
                    }
                    return ((NumericValue) argEval.eval(context)).round(scale);
                };
            case "round-half-to-even":
                return context -> {
                    int scale = 0;
                    if (scaleArg != null) {
                        scale = (int) ((NumericValue) scaleArg.eval(context)).longValue();
                    }
                    return ((NumericValue) argEval.eval(context)).roundHalfToEven(scale);
                };
            default: throw new UnsupportedOperationException();

        }

    }


}

