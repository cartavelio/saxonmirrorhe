package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.AccessorFn;
import net.sf.saxon.value.AtomicValue;

/**
 * Elaborator for accessor functions such as hours-from-date-Time, minutes-from-duration
 */

public class AccessorFnElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final AccessorFn fn = (AccessorFn) fnc.getTargetFunction();
        final AccessorFn.Component component = fn.getComponentId();
        final ItemEvaluator argEval = Elaborator.make(fnc.getArg(0)).getItemEvaluator();
        return context -> ((AtomicValue) argEval.eval(context)).getComponent(component);
    }


}

