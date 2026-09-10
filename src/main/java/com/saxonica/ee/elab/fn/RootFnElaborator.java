package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.om.NodeInfo;

public class RootFnElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final ItemEvaluator arg0Eval = makeElaborator(fnc.getArg(0)).elaborateForItem();
        return context -> {
            NodeInfo focus = (NodeInfo)arg0Eval.eval(context);
            return focus == null ? null : focus.getRoot();
        };

    }

}

