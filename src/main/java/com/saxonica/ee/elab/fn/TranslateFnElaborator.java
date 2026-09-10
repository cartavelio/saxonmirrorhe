package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.Translate;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.z.IntToIntMap;

public class TranslateFnElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final Translate fn = (Translate) fnc.getTargetFunction();
        final ItemEvaluator arg0Eval = makeElaborator(fnc.getArg(0)).elaborateForItem();
        final ItemEvaluator arg1Eval = makeElaborator(fnc.getArg(1)).elaborateForItem();
        final ItemEvaluator arg2Eval = makeElaborator(fnc.getArg(2)).elaborateForItem();
        final IntToIntMap staticMap = fn.getStaticMap();

        if (staticMap != null) {
            return context -> {
                StringValue s0 = (StringValue)arg0Eval.eval(context);
                if (s0 == null || s0.isEmpty()) {
                    return StringValue.EMPTY_STRING;
                }
                return Translate.translateUsingMap(s0, staticMap);
            };
        } else {
            return context -> {
                StringValue s0 = (StringValue) arg0Eval.eval(context);
                if (s0 == null || s0.isEmpty()) {
                    return StringValue.EMPTY_STRING;
                }
                StringValue s1 = (StringValue) arg1Eval.eval(context);
                StringValue s2 = (StringValue) arg2Eval.eval(context);
                return Translate.translate(s0, s1, s2);
            };
        }
    }

}

