package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.str.StringTool;
import net.sf.saxon.str.StringView;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.StringValue;

/**
 * Implements the saxon:new-processing-instruction() function. Creates a new processing instruction node.
 */

public class NewProcessingInstruction extends SystemFunction {

    /**
     * Evaluate this function call
     *
     * @param context   The XPath dynamic evaluation context
     * @param arguments The values of the arguments to the function call. Each argument value (which is in general
     *                  a sequence) is supplied in the form of a sequence.
     * @return the results of the function.
     * @throws XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                                           code will add information about the error location.
     */

    @Override
    public NodeInfo call(XPathContext context, Sequence[] arguments) throws XPathException {
        Orphan orphan = new Orphan(context.getConfiguration());
        orphan.setNodeKind(Type.PROCESSING_INSTRUCTION);
        String name = arguments[0].head().getStringValue();
        if (!NameChecker.isValidNCName(StringTool.codePoints(name))) {
            throw new XPathException("Name of processing instruction must be a valid NCName: " + name, "XTDE0890");
        }
        if (name.equalsIgnoreCase("xml")) {
            throw new XPathException("Name of processing instruction must must not match [Xx][Mm][Ll]: " + name, "XTDE0890");
        }
        StringValue first = (StringValue)arguments[1].head();
        String value = first == null ? "" : first.getStringValue();
        value = value.replace("?>", "? >");
        orphan.setNodeName(new FingerprintedQName("", NamespaceUri.NULL, name));
        orphan.setStringValue(StringView.tidy(value));
        return orphan;
    }


}

// Copyright (c) 2020-2023 Saxonica Limited

