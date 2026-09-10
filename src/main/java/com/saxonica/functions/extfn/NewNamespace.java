package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.str.StringTool;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.Type;

/**
 * Implements the saxon:new-namespace() function. Creates a new namespace node.
 */

public class NewNamespace extends SystemFunction {

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
        orphan.setNodeKind(Type.NAMESPACE);
        String name = arguments[0].head().getStringValue();
        if (!NameChecker.isValidNCName(StringTool.codePoints(name))) {
            throw new XPathException("Name of namespace node must be a valid NCName: " + name, "FORG0001");
        }
        orphan.setNodeName(new FingerprintedQName("", "", name));
        orphan.setStringValue(arguments[1].head().getUnicodeStringValue());
        return orphan;
    }


}

// Copyright (c) 2020-2022 Saxonica Limited

