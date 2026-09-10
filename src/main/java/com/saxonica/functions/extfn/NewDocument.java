package com.saxonica.functions.extfn;

import net.sf.saxon.event.Builder;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.trans.XPathException;

/**
 * Implements the saxon:new-text() function. Creates a new text node.
 */

public class NewDocument extends SystemFunction {

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
        Builder builder = context.getController().makeBuilder();
        builder.setBaseURI(getStaticBaseUriString());
        ComplexContentOutputter cco = new ComplexContentOutputter(builder);
        cco.setSystemId(getStaticBaseUriString());
        cco.setHostLanguage(HostLanguage.XSLT);
        cco.open();
        cco.startDocument(0);
        SequenceTool.supply(arguments[0].iterate(), (ItemConsumer<? super Item>) cco::append);
        cco.endDocument();
        cco.close();
        return builder.getCurrentRoot();
    }


}

// Copyright (c) 2020-2023 Saxonica Limited

