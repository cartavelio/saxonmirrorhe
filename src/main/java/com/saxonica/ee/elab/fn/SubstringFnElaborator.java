package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.Substring;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.NumericValue;
import net.sf.saxon.value.StringValue;

public class SubstringFnElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final ItemEvaluator arg0Eval = makeElaborator(fnc.getArg(0)).elaborateForItem();
        final ItemEvaluator arg1Eval = makeElaborator(fnc.getArg(1)).elaborateForItem();
        final boolean nullable = Cardinality.allowsZero(fnc.getArg(0).getCardinality());
        if (fnc.getArity() == 2) {
            return context -> {
                StringValue sv = (StringValue)arg0Eval.eval(context);
                if (nullable && sv == null) {
                    return StringValue.EMPTY_STRING;
                }
                NumericValue start = (NumericValue)arg1Eval.eval(context);
                return Substring.substring(sv, start);
            };
        } else {
            final ItemEvaluator arg2Eval = makeElaborator(fnc.getArg(2)).elaborateForItem();
            return context -> {
                StringValue sv = (StringValue) arg0Eval.eval(context);
                if (nullable && sv == null) {
                    return StringValue.EMPTY_STRING;
                }
                NumericValue start = (NumericValue) arg1Eval.eval(context);
                NumericValue len = (NumericValue) arg2Eval.eval(context);
                return Substring.substring(sv, start, len);
            };
        }
    }

}

