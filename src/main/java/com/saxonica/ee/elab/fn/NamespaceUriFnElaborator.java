package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.value.AnyURIValue;
import net.sf.saxon.value.Cardinality;

/**
 * Elaborator for the namespace-uri() function
 */

public class NamespaceUriFnElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final Expression arg = fnc.getArg(0);
        final boolean nullable = Cardinality.allowsZero(arg.getCardinality());
        final ItemEvaluator argEval = makeElaborator(arg).elaborateForItem();

        return context -> {
            NodeInfo node = (NodeInfo) argEval.eval(context);
            String uri = nullable && node == null ? "" : node.getURI();
            return new AnyURIValue(uri);
        };

    }

}

