package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.GenerateId_1;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.AnyURIValue;
import net.sf.saxon.value.QNameValue;
import net.sf.saxon.value.StringValue;

public class NodePropertyElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        Expression arg = fnc.getArg(0);
        ItemEvaluator argEval = Elaborator.make(arg).getItemEvaluator();
        switch (fnc.getFunctionName().getLocalPart()) {
            case "base-uri":
                return context -> {
                    NodeInfo node = (NodeInfo) argEval.eval(context);
                    String s = node.getBaseURI();
                    if (s == null) {
                        return null;
                    }
                    return new AnyURIValue(s);
                };
            case "generate-id":
                return context -> {
                    NodeInfo node = (NodeInfo) argEval.eval(context);
                    return GenerateId_1.generateId(node);
                };
            case "local-name":
                return context -> {
                    NodeInfo node = (NodeInfo) argEval.eval(context);
                    return StringValue.makeStringValue(node.getLocalPart());
                };
            case "name":
                return context -> {
                    NodeInfo node = (NodeInfo) argEval.eval(context);
                    return StringValue.makeStringValue(node.getDisplayName());
                };
            case "namespace-uri":
                return context -> {
                    NodeInfo node = (NodeInfo) argEval.eval(context);
                    return StringValue.makeStringValue(node.getURI());
                };
            case "node-name":
                return context -> {
                    NodeInfo node = (NodeInfo) argEval.eval(context);
                    return new QNameValue(node.getPrefix(), node.getURI(), node.getLocalPart(), BuiltInAtomicType.QNAME);
                };
            default:
                throw new UnsupportedOperationException("Unknown accessor function");

        }
    }

}

