package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.NameChecker;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.serialize.charcode.XMLCharacterData;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.z.IntIterator;

/**
 * The {@code saxon:unescape-NCName} function reverses the effect of {@code saxon:escape-NCName}:
 * it takes an NCName constructed using {@code saxon:escape-NCName} as input, and returns the
 * string from which it was constructed. A dynamic error is raised if the input contains an invalid
 * escape sequence.
 */

public class UnescapeNCName extends SystemFunction {

    /**
     * Evaluate this function call at run-time
     *
     * @param context   The XPath dynamic evaluation context. Not used in this case
     * @param arguments The values of the arguments to the function call.
     * @return The unescaped NCName
     * @throws XPathException if lazy evaluation of the supplied argument fails, or if the
     * input contains an invalid escape sequence
     */

    @Override
    public StringValue call(XPathContext context, Sequence[] arguments) throws XPathException {
        StringValue arg = (StringValue)arguments[0].head();
        UnicodeString input = arg.getUnicodeStringValue();
        final TypeHierarchy th = context.getConfiguration().getTypeHierarchy();
        if (!(BuiltInAtomicType.NCNAME.matches(arg, th) || NameChecker.isValidNCName(input.codePoints()))) {
            throw new XPathException("Input to saxon:unescape-NCName must be a valid NCName: supplied value is '" + input + "'", "FORG0001");
        }
        if (input.length() == 1 && input.codePointAt(0) == '_') {
            return new StringValue("");
        }
        UnicodeBuilder out = new UnicodeBuilder((int)input.estimatedLength());
        IntIterator iter = input.codePoints();
        while (iter.hasNext()) {
            int ch = iter.next();
            if (ch == '_') {
                StringBuilder hex = new StringBuilder(6);
                while (iter.hasNext()) {
                    int digit = iter.next();
                    if (digit == '_') {
                        if (hex.length() == 0) {
                            out.append('_');
                        } else {
                            int code = Integer.parseInt(hex.toString(), 16);
                            if (!XMLCharacterData.isValid11(code)) {
                                throw new XPathException("Invalid escaped XML character: " + hex, "FORG0001");
                            }
                            out.append(code);
                        }
                        break;
                    } else if (digit < 128 && "0123456789abcdefABCDEF".indexOf((char)digit) >= 0) {
                        hex.append((char)digit);
                    } else {
                        throw new XPathException("Invalid character '" + Integer.toHexString(digit) + " in escaped NCName");
                    }
                }
            } else {
                out.append(ch);
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

// Copyright (c) 2021-2022 Saxonica Limited
