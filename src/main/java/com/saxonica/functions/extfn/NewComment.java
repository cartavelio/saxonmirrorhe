package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.Comment;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.Type;

/**
 * Implements the saxon:new-comment() function. Creates a new comment node.
 */

public class NewComment extends SystemFunction {

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
        orphan.setNodeKind(Type.COMMENT);
        Item first = arguments[0].head();
        String value = first == null ? "" : first.getStringValue();
        value = Comment.checkContentXSLT(value);
        orphan.setStringValue(value);
        return orphan;
    }


}

// Copyright (c) 2020 Saxonica Limited

