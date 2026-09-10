package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.Substring;
import net.sf.saxon.value.NumericValue;
import net.sf.saxon.value.StringValue;

public class SubstringFnElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final ItemEvaluator arg0Eval = makeElaborator(fnc.getArg(0)).elaborateForItem();
        final ItemEvaluator arg1Eval = makeElaborator(fnc.getArg(1)).elaborateForItem();

        if (fnc.getArity() == 2) {
            return context -> {
                StringValue sv = (StringValue)arg0Eval.eval(context);
                NumericValue start = (NumericValue)arg1Eval.eval(context);
                return Substring.substring(sv, start);
            };
        } else {
            final ItemEvaluator arg2Eval = makeElaborator(fnc.getArg(1)).elaborateForItem();
            return context -> {
                StringValue sv = (StringValue) arg0Eval.eval(context);
                NumericValue start = (NumericValue) arg1Eval.eval(context);
                NumericValue len = (NumericValue) arg2Eval.eval(context);
                return Substring.substring(sv, start, len);
            };
        }
    }

}

