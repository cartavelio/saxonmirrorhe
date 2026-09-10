package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.QNameValue;

/**
 * Elaborator for the fn:node-name() function
 */

public class NodeNameFnElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        ItemEvaluator argEval = makeElaborator(arg).elaborateForItem();

        return context -> {
            NodeInfo node = (NodeInfo) argEval.eval(context);
            if (node == null || node.getLocalPart().isEmpty()) {
                return null;
            }
            return new QNameValue(node.getPrefix(), node.getNamespaceUri(), node.getLocalPart(), BuiltInAtomicType.QNAME);
        };

    }

}

