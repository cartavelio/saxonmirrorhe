package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.value.AnyURIValue;
import net.sf.saxon.value.Cardinality;

/**
 * Elaborator for simple string-valued properties of nodes such as name(), local-name(), namespace-uri(),
 * and generate-id()
 */

public class BaseUriFnElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final Expression arg = fnc.getArg(0);
        final boolean nullable = Cardinality.allowsZero(arg.getCardinality());
        final ItemEvaluator argEval = makeElaborator(arg).elaborateForItem();

        return context -> {
            NodeInfo node = (NodeInfo) argEval.eval(context);
            if (nullable && node == null) {
                return null;
            }
            String s = node.getBaseURI();
            if (s == null) {
                return null;
            }
            return new AnyURIValue(s);
        };

    }

}

