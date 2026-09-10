package com.saxonica.functions.extfn;

import net.sf.saxon.event.Builder;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.Untyped;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.QNameValue;

/**
 * Implements the saxon:new-text() function. Creates a new text node.
 */

public class NewElement extends SystemFunction {

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
        AtomicValue nameArg = (AtomicValue)arguments[0].head();
        QNameValue name;
        if (nameArg instanceof QNameValue) {
            name = (QNameValue) arguments[0].head();
        } else {
            String s = nameArg.getStringValue();
            StructuredQName q = StructuredQName.fromLexicalQName(s, false, true, getRetainedStaticContext());
            name = new QNameValue(q, BuiltInAtomicType.QNAME);
        }
        cco.startElement(new FingerprintedQName(name.getStructuredQName(), context.getNamePool()),
                         Untyped.getInstance(),
                         Loc.NONE, 0);
        SequenceTool.supply(arguments[1].iterate(), (ItemConsumer<? super Item>) cco::append);
        cco.endElement();
        cco.close();
        return builder.getCurrentRoot();
    }


}

// Copyright (c) 2020-2022 Saxonica Limited

