////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.ma.json;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.serialize.charcode.UTF16CharacterSet;
import net.sf.saxon.str.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.z.IntPredicateProxy;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

/**
 * Default handler class for accepting the result from parsing JSON strings
 */
public class JsonHandler {

    public boolean escape;
    protected IntPredicateProxy charChecker;
    private XPathContext context;

    private FunctionItem fallbackFunction = null;
    public static final UnicodeString REPLACEMENT = new UnicodeChar(0xfffd);

    public void setContext(XPathContext context) {
        this.context = context;
    }

    public XPathContext getContext() {
        return context;
    }

    public Sequence getResult() throws XPathException {
        return null;
    }

    /**
     * Set the key to be written for the next entry in an object/map
     *
     * @param unEscaped the key for the entry (null implies no key) in unescaped form (backslashes,
     *                  if present, do not signal an escape sequence)
     * @param reEscaped the key for the entry (null implies no key) in reescaped form. In this form
     *                  special characters are represented as backslash-escaped sequences if the escape
     *                  option is yes; if escape=no, the reEscaped form is the same as the unEscaped form.
     * @return true if the key is already present in the map, false if it is not
     */
    public boolean setKey(UnicodeString unEscaped, UnicodeString reEscaped) {
        return false;
    }

    /**
     * Open a new array
     *
     * @throws XPathException if any error occurs
     */
    public void startArray() throws XPathException {}

    /**
     * Close the current array
     *
     * @throws XPathException if any error occurs
     */
    public void endArray() throws XPathException {}

    /**
     * Start a new object/map
     *
     * @throws XPathException if any error occurs
     */
    public void startMap() throws XPathException {}

    /**
     * Close the current object/map
     *
     * @throws XPathException if any error occurs
     */
    public void endMap() throws XPathException {}

    /**
     * Write a numeric value
     *
     * @param asString the raw string representation of the value
     * @param parsedValue the parsed representation of the value, typically an xs:double, but under user control
     * @throws XPathException if any error occurs
     */
    public void writeNumeric(String asString, Item parsedValue) throws XPathException {}

    /**
     * Write a string value
     *
     * @param val The string to be written (which may or may not contain JSON escape sequences, according to the
     * options that were set)
     * @throws XPathException if any error occurs
     */
    public void writeString(UnicodeString val) throws XPathException {}

    /**
     * Optionally apply escaping or unescaping to a value.
     * @param val the string to be escaped or unEscaped. This is supplied as a String rather than a UnicodeString
     *            because there are special rules for handling unpaired surrogates, which cannot be represented
     *            in a UnicodeString
     * @return the escaped or unescaped string
     * @throws XPathException if there are invalid characters or escape sequences
     */

    public UnicodeString reEscape(UnicodeString val) throws XPathException {
        UnicodeString escaped;
        if (escape) {
            escaped = JsonReceiver.escape(val, true, true, value ->
                    (value >= 0 && value <= 0x1F) ||
                    (value >= 0x7F && value <= 0x9F) ||
                    !charChecker.test(value) ||
                    (value == 0x5C));
        } else {
            escaped = handleInvalidCharacters(val);
        }
        return escaped;
    }

    /**
     * Write a boolean value
     * @param value the boolean value to be written
     * @throws XPathException if any error occurs
     */
    public void writeBoolean(boolean value) throws XPathException {}

    /**
     * Write a null value
     *
     * @throws XPathException if any error occurs
     */
    public void writeNull() throws XPathException {}

    /**
     * Deal with invalid characters in the JSON string
     * @param input the JSON string
     * @throws XPathException if any error occurs
     */
    protected UnicodeString handleInvalidCharacters(UnicodeString input) throws XPathException {
        IntPredicateProxy charChecker = context.getConfiguration().getValidCharacterChecker();
        List<int[]> substitutions = new ArrayList<>();
        for (int i = 0; i < input.length(); i++) {
            int ch = input.codePointAt(i);
            if (UTF16CharacterSet.isHighSurrogate(ch)) {
                if (i + 1 >= input.length() || !UTF16CharacterSet.isLowSurrogate(input.codePointAt(i + 1))) {
                    substitutions.add(new int[]{i, 1});
                    //substitute(input, i, 1, context);
                }
            } else if (UTF16CharacterSet.isLowSurrogate(ch)) {
                if (i == 0 || !UTF16CharacterSet.isHighSurrogate(input.codePointAt(i - 1))) {
                    substitutions.add(new int[]{i, 1});
                    //substitute(input, i, 1, context);
                } else {
                    int pair = UTF16CharacterSet.combinePair((char)input.codePointAt(i - 1), (char)ch);
                    if (!charChecker.test(pair)) {
                        substitutions.add(new int[]{i - 1, 2});
                        //substitute(input, i - 1, 2, context);
                    }
                }
            } else {
                if (!charChecker.test(ch)) {
                    substitutions.add(new int[]{i, 1});
                    //substitute(input, i, 1, context);
                }
            }
        }
        if (substitutions.isEmpty()) {
            return input;
        }
        UnicodeBuilder builder = new UnicodeBuilder(input.length32() * 2);
        int pos = 0;
        for (int[] substitution : substitutions) {
            builder.append(input.substring(pos, substitution[0]));
            substitute(input, builder, substitution[0], substitution[1], context);
            pos = substitution[0] + substitution[1];
        }
        builder.append(input.substring(pos));
        return builder.toUnicodeString();
    }

    protected void markAsEscaped(UnicodeString escaped, boolean isKey) throws XPathException {
        // do nothing in this class
    }

    /**
     * Replace a character or two characters within a string buffer, either by executing the replacement function,
     * or using the default Unicode replacement character
     *
     * @param buffer the string buffer, which is modified by this call
     * @param offset the position of the characters to be replaced
     * @param count the number of characters to be replaced
     * @param context the XPath context
     * @throws XPathException if the callback function throws an exception
     */
    private void substitute(UnicodeString input, UnicodeBuilder buffer, int offset, int count, XPathContext context) throws XPathException {
        TwineBuilder escaped = TwineBuilder.make(count*6);
        for (int j=0; j<count; j++) {
            escaped = escaped.append('\\').append('u');
            StringBuilder hex = new StringBuilder(Integer.toHexString(input.codePointAt(offset + j)));
            while (hex.length() < 4) {
                hex.insert(0, "0");
            }
            escaped = escaped.append(hex.toString().toUpperCase());
        }
        UnicodeString replacement = replace(escaped.toUnicodeString(), context);
        buffer.append(replacement);
    }

    /**
     * Replace an illegal XML character, either by executing the replacement function,
     * or using the default Unicode replacement character
     *
     * @param s       the string representation of the illegal character
     * @param context the XPath context
     * @return the replacement string
     * @throws XPathException if the callback function throws an exception
     */
    private UnicodeString replace(UnicodeString s, XPathContext context) throws XPathException {
        if (fallbackFunction != null) {
            Sequence[] args = new Sequence[1];
            args[0] = new StringValue(s);
            Sequence result = SystemFunction.dynamicCall(fallbackFunction, context, args).head();
            Item first = result.head();
            return first == null ? EmptyUnicodeString.getInstance() : first.getUnicodeStringValue();
        } else {
            return REPLACEMENT;
        }
    }

    public void setFallbackFunction(Map<String, GroundedValue> options, XPathContext context) throws XPathException {
        GroundedValue val = options.get("fallback");
        if (val != null) {
            Item fn = val.head();
            if (fn instanceof FunctionItem) {
                fallbackFunction = (FunctionItem) fn;
                if (fallbackFunction.getArity() != 1) {
                    throw new XPathException("Fallback function must have arity=1", "FOJS0005");
                }
                SpecificFunctionType required = new SpecificFunctionType(
                        SequenceType.SINGLE_STRING, SequenceType.ANY_SEQUENCE);
                if (!required.matches(fallbackFunction, context.getConfiguration().getTypeHierarchy())) {
                    throw new XPathException("Fallback function does not match the required type", "XPTY0004");
                }
            } else {
                throw new XPathException("Value of option 'fallback' is not a function", "XPTY0004");
            }
        }
    }

    public FunctionItem getFallbackFunction() {
        return fallbackFunction;
    }
}

// Copyright (c) 2018-2023 Saxonica Limited
