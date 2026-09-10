package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.value.DateTimeValue;
import net.sf.saxon.value.DateValue;
import net.sf.saxon.value.TimeValue;

public class DateTimeFnElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        SystemFunctionCall sfc = (SystemFunctionCall) getExpression();
        ItemEvaluator arg0eval = makeElaborator(sfc.getArg(0)).elaborateForItem();
        ItemEvaluator arg1eval = makeElaborator(sfc.getArg(1)).elaborateForItem();
        return context -> DateTimeValue.makeDateTimeValue((DateValue)arg0eval.eval(context), (TimeValue) arg1eval.eval(context));
    }

}

