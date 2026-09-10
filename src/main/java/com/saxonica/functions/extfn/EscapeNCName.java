package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.NameChecker;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.z.IntIterator;

/**
 * The {@code saxon:escape-NCName} function takes an arbitrary string and converts it to an NCName.
 * Characters that are not legal in an NCName are converted to the form {@code _xxxx_}. A
 * zero length string is converted to a single underscore. An underscore is converted to
 * a pair of underscores. The conversion can be reversed using {@code saxon:unescape-NCName};
 */

public class EscapeNCName extends SystemFunction {

    /**
     * Evaluate this function call at run-time
     *
     * @param context   The XPath dynamic evaluation context. Not used in this case
     * @param arguments The values of the arguments to the function call.
     * @return The escaped NCName
     * @throws XPathException if lazy evaluation of the supplied argument fails
     */

    @Override
    public StringValue call(XPathContext context, Sequence[] arguments) throws XPathException {
        UnicodeString input = arguments[0].head().getUnicodeStringValue();
        if (input.isEmpty()) {
            return new StringValue("_", BuiltInAtomicType.NCNAME);
        }
        UnicodeBuilder out = new UnicodeBuilder((int)input.estimatedLength());
        IntIterator iter = input.codePoints();
        if (iter.hasNext()) {
            // process first character (which always exists)
            int ch = iter.next();
            if (NameChecker.isNCNameStartChar(ch)) {
                if (ch == '_') {
                    out.append('_').append('_');
                } else {
                    out.append(ch);
                }
            } else {
                escape(ch, out);
            }
        }
        while (iter.hasNext()) {
            // process subsequent characters
            int ch = iter.next();
            if (NameChecker.isNCNameChar(ch)) {
                if (ch == '_') {
                    out.append('_').append('_');
                } else {
                    out.append(ch);
                }
            } else {
                escape(ch, out);
            }
        }
        return new StringValue(out.toUnicodeString(), BuiltInAtomicType.NCNAME);
    }

    private void escape(int codepoint, UnicodeBuilder sb) {
        sb.append('_');
        sb.append(Integer.toHexString(codepoint));
        sb.append('_');
    }


}

// Copyright (c) 2021-2023 Saxonica Limited
