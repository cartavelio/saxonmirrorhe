package com.saxonica.ee.elab;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.value.Int64Value;

public abstract class StringLengthElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        StringEvaluator argEval = Elaborator.make(arg).getStringEvaluator(EmptyUnicodeString.getInstance());
        return context -> Int64Value.makeIntegerValue(argEval.eval(context).length());
    }


}

