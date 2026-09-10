package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.lib.SubstringMatcher;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.StringConstants;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;

import java.util.HashSet;
import java.util.Set;

/**
 * Implements the fn:collation-available() function.
 */

public class CollationAvailable extends SystemFunction {

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
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        String collation = arguments[0].head().getStringValue();
        StringCollator collator = context.getConfiguration().getCollation(collation); // TODO: resolve against base URI?
        if (collator == null) {
            return BooleanValue.FALSE;
        }
        Set<String> usages = new HashSet<>();
        SequenceIterator iter = arguments[1].iterate();
        Item usage;
        while ((usage = iter.next()) != null) {
            usages.add(usage.getStringValue());
        }
        if (usages.isEmpty()) {
            usages.add("equality");
            usages.add("sort");
            usages.add("substring");
        }
        if (usages.contains("sort")) {
            try {
                collator.compareStrings(EmptyUnicodeString.getInstance(), StringConstants.SINGLE_SPACE);
            } catch (Exception e) {
                return BooleanValue.FALSE;
            }
        }
        if (usages.contains("substring") && !(collator instanceof SubstringMatcher)) {
            return BooleanValue.FALSE;
        }
        return BooleanValue.TRUE;
    }

    

}

// Copyright (c) 2020-2023 Saxonica Limited

