package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.value.DateTimeValue;
import net.sf.saxon.value.DateValue;
import net.sf.saxon.value.TimeValue;

public class DateTimeFnElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
        SystemFunctionCall sfc = (SystemFunctionCall) getExpression();
        ItemEvaluator arg0eval = Elaborator.make(sfc.getArg(0)).getItemEvaluator();
        ItemEvaluator arg1eval = Elaborator.make(sfc.getArg(1)).getItemEvaluator();
        return context -> DateTimeValue.makeDateTimeValue((DateValue)arg0eval.eval(context), (TimeValue) arg1eval.eval(context));
    }

}

