package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.UnicodeStringEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.Int64Value;

public class StringLengthFnElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        UnicodeStringEvaluator argEval = makeElaborator(arg).elaborateForUnicodeString(true);
        return context -> {
            UnicodeString str = argEval.eval(context);
            return Int64Value.makeIntegerValue(str.length());
        };
    }

}

