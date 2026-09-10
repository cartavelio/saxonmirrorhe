package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.AccessorFn;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;

/**
 * Elaborator for accessor functions such as hours-from-date-Time, minutes-from-duration
 */

public class AccessorFnElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final AccessorFn fn = (AccessorFn) fnc.getTargetFunction();
        final AccessorFn.Component component = fn.getComponentId();
        final ItemEvaluator argEval = makeElaborator(fnc.getArg(0)).elaborateForItem();
        final boolean nullable = Cardinality.allowsZero(fnc.getArg((0)).getCardinality());
        return context -> {
            AtomicValue base = ((AtomicValue) argEval.eval(context));
            if (nullable && base == null) {
                return null;
            }
            return base.getComponent(component);
        };
    }


}

