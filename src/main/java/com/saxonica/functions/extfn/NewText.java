package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.Type;

/**
 * Implements the saxon:new-text() function. Creates a new text node.
 */

public class NewText extends SystemFunction {

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
        orphan.setNodeKind(Type.TEXT);
        Item first = arguments[0].head();
        orphan.setStringValue(first == null ? (UnicodeString) EmptyUnicodeString.getInstance() : first.getUnicodeStringValue());
        return orphan;
    }


}

// Copyright (c) 2020-2023 Saxonica Limited

