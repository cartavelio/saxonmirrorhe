package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.om.NodeInfo;

public class RootFnElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final ItemEvaluator arg0Eval = Elaborator.make(fnc.getArg(0)).getItemEvaluator();
        return context -> {
            NodeInfo focus = (NodeInfo)arg0Eval.eval(context);
            return focus == null ? null : focus.getRoot();
        };

    }

}

