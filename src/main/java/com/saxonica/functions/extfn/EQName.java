package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.QNameValue;

/**
 * This class implements the saxon:EQName extension function. The function accepts
 * a string which may be in any of three formats: (a) an NCName, representing a name
 * in no namespace, (b) a lexical QName, in which case the prefix is interpreted
 * relative to the in-scope namespaces in the static context, or (c) a URIQualifiedName
 * in the form Q{uri}local. The result is an xs:QName instance.
 */

public class EQName extends SystemFunction {

    /**
     * Evaluate the function call
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as SequenceIterators
     * @return the result of the evaluation, in the form of a SequenceIterator
     * @throws XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Item first = arguments[0].head();
        if (first == null) {
            return EmptySequence.getInstance();
        }
        String arg = first.getStringValue();
        StructuredQName qname =
                StructuredQName.fromLexicalQName(arg, false, true, getRetainedStaticContext());
        return new QNameValue(qname, BuiltInAtomicType.QNAME);
    }
}

// Copyright (c) 2020-2022 Saxonica Limited

