package com.saxonica.serialize;

import net.sf.saxon.serialize.XMLEmitter;
import net.sf.saxon.str.TwineBuilder;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;

import java.io.IOException;

/**
 * The XQueryEmitter is designed to serialize an XQuery that was originally embedded in an
 * XML document. It is a variant of the XMLEmitter, and differs in that the operators &lt;, &gt;, &lt;=, &gt;=, &lt;&lt;, and &gt;&gt;
 * are output without escaping. They are recognized by virtue of the fact that they appear in text or attribute
 * content between curly braces but not in quotes.
 */

public class XQueryEmitter extends XMLEmitter {

    /**
     * Write contents of array to current writer, after escaping special characters.
     * This method converts the XML special characters (such as &lt; and &amp;) into their
     * predefined entities.
     * @param chars       The character sequence containing the string
     * @param inAttribute Set to true if the text is in an attribute value
     */

    @Override
    protected void writeEscape(UnicodeString chars, final boolean inAttribute) throws IOException, XPathException {
        boolean inBraces = false;
        chars = chars.tidy();
        TwineBuilder tb = TwineBuilder.make(chars.length32() + 20);
        for (int i = 0; i < chars.length(); i++) {
            int c = chars.codePointAt(i);
            if (!inBraces && c == '{' && chars.codePointAt(i + 1) != '{') {
                inBraces = true;
                tb = tb.append((char) 0);   // switch disable-output-escaping on
            } else if (inBraces && c == '}') {
                inBraces = false;
                tb = tb.append((char) 0);   // switch disable-output-escaping off
            } else if (inBraces && c == '"') {
                tb = tb.append((char) 0);
                i++;
                do {
                    tb = tb.append(c);
                    c = chars.codePointAt(i++);
                } while (c != '"');
                tb = tb.append((char) 0);
                i--;
            } else if (inBraces && c == '\'') {
                tb = tb.append((char) 0);
                i++;
                do {
                    tb = tb.append(c);
                    c = chars.codePointAt(i++);
                } while (c != '\'');
                tb = tb.append((char) 0);
                i--;
            }
            tb = tb.append(c);
        }
        super.writeEscape(tb.toUnicodeString(), inAttribute);
    }
}

// Copyright (c) 2011-2023 Saxonica Limited