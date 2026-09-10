package com.saxonica.functions.qt4;

import net.sf.saxon.expr.Atomizer;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.str.ToUpper;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.str.UnicodeChar;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.z.IntIterator;

/**
 * Implements the fn:collation() function.
 */

public class CollationFn extends SystemFunction {

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
        MapItem s = (MapItem)arguments[0].head();
        UnicodeBuilder sb = new UnicodeBuilder();
        sb.append("http://www.w3.org/2013/collation/UCA");
        boolean first = true;
        for (KeyValuePair kvp : s.keyValuePairs()) {
            if (first) {
                sb.append('?');
                first = false;
            } else {
                sb.append(';');
            }
            sb.append(camelCase(kvp.key.getUnicodeStringValue()))
                    .append('=');
            GroundedValue value = kvp.value;
            if (!(value instanceof AtomicValue)) {
                // TODO - use the standard options-handling machinery?
                value = Atomizer.atomize(value);
            }
            if (value instanceof BooleanValue) {
                sb.append(((BooleanValue)value).getBooleanValue() ? "yes" : "no");
            } else {
                sb.append(value.getUnicodeStringValue());
            }
        }
        return new StringValue(sb.toUnicodeString());
    }

    private static UnicodeString camelCase(UnicodeString in) {
        long hyphen = in.indexOf('-');
        if (hyphen < 0) {
            return in;
        }
        UnicodeBuilder ub = new UnicodeBuilder((int)in.length());
        IntIterator iter = in.codePoints();
        while (iter.hasNext()) {
            int cp = iter.next();
            if (cp == '-' && iter.hasNext()) {
                cp = ToUpper.toUpper(new UnicodeChar(iter.next())).codePointAt(0);
            }
            ub.append(cp);
        }
        return ub.toUnicodeString();
    }


}

// Copyright (c) 2020-2023 Saxonica Limited

