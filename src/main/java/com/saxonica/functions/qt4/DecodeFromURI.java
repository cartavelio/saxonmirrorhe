package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.serialize.charcode.XMLCharacterData;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.z.IntIterator;

import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;

/**
 * Implements the fn:decode-from-uri function.
 */

public class DecodeFromURI extends SystemFunction {
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
    public StringValue call(XPathContext context, Sequence[] arguments) throws XPathException {
        if (arguments[0].head() == null) {
            return new StringValue("");
        }

        String value = arguments[0].head().getStringValue();
        if (value.isEmpty()) {
            return new StringValue("");
        }

        return new StringValue(DecodeFromURI.decode(value));
    }

    private static boolean isHex(char ch) {
        return (ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F');
    }

    public static String decode(String value) {
        List<Byte> output = new ArrayList<>();
        byte[] octets = value.getBytes(StandardCharsets.UTF_8);
        int idx = 0;
        while (idx < octets.length) {
            switch ((char)octets[idx]) {
                case '+':
                    output.add((byte) 0x20);
                    idx++;
                    break;

                case '%':
                    if (idx+2 >= octets.length) {
                        output.add((byte) 0xEF);
                        output.add((byte) 0xBF);
                        output.add((byte) 0xBD); // 0xFFFD in UTF-8
                        idx = octets.length;
                        break;
                    }

                    char tens = Character.toUpperCase((char) (octets[idx+1] & 0xFF));
                    char ones = Character.toUpperCase((char) (octets[idx+2] & 0xFF));
                    idx += 3;

                    if (DecodeFromURI.isHex(tens) && DecodeFromURI.isHex(ones)) {
                        int hex = Integer.parseInt("" + tens + ones, 16);
                        output.add((byte) hex);
                    } else {
                        output.add((byte) 0xEF);
                        output.add((byte) 0xBF);
                        output.add((byte) 0xBD); // 0xFFFD in UTF-8
                    }

                    break;

                default:
                    output.add(octets[idx]);
                    idx++;
                    break;
            }
        }

        byte[] utf8 = new byte[output.size()];
        for (idx = 0; idx < output.size(); idx++) {
            utf8[idx] = output.get(idx);
        }

        UnicodeString result = StringView.of(new String(utf8, StandardCharsets.UTF_8));
        StringBuilder sb = new StringBuilder();

        IntIterator iter = result.codePoints();
        while (iter.hasNext()) {
            int cp = iter.next();
            if (XMLCharacterData.isValid10(cp)) {
                sb.appendCodePoint(cp);
            } else {
                sb.appendCodePoint(0xFFFD);
            }
        }

        return sb.toString();
    }
}

// Copyright (c) 2023 Saxonica Limited

