package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.gen.StringElaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.GenerateId_1;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.StringView;
import net.sf.saxon.value.Cardinality;

/**
 * Elaborator for simple string-valued properties of nodes such as name(), local-name(), namespace-uri(),
 * and generate-id()
 */

public class NodePropertyElaborator extends StringElaborator {

    public StringEvaluator elaborateForString(boolean zeroLengthIfAbsent) {
        final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        final Expression arg = fnc.getArg(0);
        final boolean nullable = Cardinality.allowsZero(arg.getCardinality());
        final ItemEvaluator argEval = makeElaborator(arg).elaborateForItem();
        switch (fnc.getFunctionName().getLocalPart()) {
            case "generate-id":
                return context -> {
                    NodeInfo node = (NodeInfo) argEval.eval(context);
                    if (nullable && node == null) {
                        return EmptyUnicodeString.getInstance();
                    }
                    return GenerateId_1.generateId(node).getUnicodeStringValue();
                };
            case "local-name":
                return context -> {
                    NodeInfo node = (NodeInfo) argEval.eval(context);
                    if (nullable && node == null) {
                        return EmptyUnicodeString.getInstance();
                    }
                    return node == null ? EmptyUnicodeString.getInstance() : StringView.of(node.getLocalPart());
                };
            case "name":
                return context -> {
                    NodeInfo node = (NodeInfo) argEval.eval(context);
                    if (nullable && node == null) {
                        return EmptyUnicodeString.getInstance();
                    }
                    return StringView.of(node.getDisplayName());
                };
            case "namespace-uri":
                return context -> {
                    NodeInfo node = (NodeInfo) argEval.eval(context);
                    if (nullable && node == null) {
                        return EmptyUnicodeString.getInstance();
                    }
                    return StringView.of(node.getURI());
                };
            default:
                throw new UnsupportedOperationException("Unknown accessor function");

        }
    }

}

