package com.saxonica.ee.elab;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.Translate;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.z.IntToIntMap;

public class TranslateFnElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final Translate fn = (Translate) fnc.getTargetFunction();
        final ItemEvaluator arg0Eval = Elaborator.make(fnc.getArg(0)).getItemEvaluator();
        final ItemEvaluator arg1Eval = Elaborator.make(fnc.getArg(1)).getItemEvaluator();
        final ItemEvaluator arg2Eval = Elaborator.make(fnc.getArg(2)).getItemEvaluator();
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

