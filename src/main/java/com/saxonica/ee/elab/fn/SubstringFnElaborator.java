package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.Substring;
import net.sf.saxon.value.NumericValue;
import net.sf.saxon.value.StringValue;

public class SubstringFnElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final ItemEvaluator arg0Eval = Elaborator.make(fnc.getArg(0)).getItemEvaluator();
        final ItemEvaluator arg1Eval = Elaborator.make(fnc.getArg(1)).getItemEvaluator();

        if (fnc.getArity() == 2) {
            return context -> {
                StringValue sv = (StringValue)arg0Eval.eval(context);
                NumericValue start = (NumericValue)arg1Eval.eval(context);
                return Substring.substring(sv, start);
            };
        } else {
            final ItemEvaluator arg2Eval = Elaborator.make(fnc.getArg(1)).getItemEvaluator();
            return context -> {
                StringValue sv = (StringValue) arg0Eval.eval(context);
                NumericValue start = (NumericValue) arg1Eval.eval(context);
                NumericValue len = (NumericValue) arg2Eval.eval(context);
                return Substring.substring(sv, start, len);
            };
        }
    }

}

