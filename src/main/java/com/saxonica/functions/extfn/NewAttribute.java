package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.*;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.QNameValue;

/**
 * Implements the saxon:new-attribute() function. Creates a new processing instruction node.
 */

public class NewAttribute extends SystemFunction {

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
        orphan.setNodeKind(Type.ATTRIBUTE);
        AtomicValue nameArg = (AtomicValue) arguments[0].head();
        StructuredQName name;
        if (nameArg instanceof QNameValue) {
            name = ((QNameValue) arguments[0].head()).getStructuredQName();
        } else {
            String s = nameArg.getStringValue();
            name = StructuredQName.fromLexicalQName(s, false, true, getRetainedStaticContext());
        }
        String prefix = name.getPrefix();
        String local = name.getLocalPart();
        String uri = name.getURI();
        if (local.equals("xmlns") && uri.isEmpty()) {
            throw new XPathException("Invalid attribute name: xmlns", "XQDY0044");
        }
        if (uri.isEmpty() && !prefix.isEmpty()) {
            prefix = "";
        }
        if (prefix.equals("xmlns")) {
            prefix = "";
        }
        if (prefix.isEmpty() && !uri.isEmpty()) {
            prefix = context.getNamePool().suggestPrefixForURI(uri);
            if (prefix == null) {
                prefix = "ns0";
            }
        }
        if (uri.equals(NamespaceConstant.XML) != "xml".equals(prefix)) {
            throw new XPathException("The xml prefix and namespace can only be used together", "XTDE0835");
        }
        name = new StructuredQName(prefix, uri, local);
        orphan.setNodeName(new FingerprintedQName(name, context.getNamePool()));
        Item first = arguments[1].head();
        orphan.setStringValue(first == null ? (UnicodeString) EmptyUnicodeString.getInstance() : first.getUnicodeStringValue());
        return orphan;
    }


}

// Copyright (c) 2020-2022 Saxonica Limited

