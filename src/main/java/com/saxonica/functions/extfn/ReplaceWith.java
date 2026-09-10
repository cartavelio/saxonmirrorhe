package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.RegexFunction;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.regex.RegularExpression;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.StringValue;


/**
 * This class implements the saxon:replace-with() function for replacing
 * substrings that match a regular expression by means of a callback to a user-supplied function
 *
 * <p>The function is obsolescent: use {@code fn:replace} with the {@code action} parameter in preference.</p>
 */

public class ReplaceWith extends RegexFunction  {

    @Override
    protected boolean allowRegexMatchingEmptyString() {
        return false;
    }

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as SequenceIterators
     * @return the result of the evaluation, in the form of a SequenceIterator
     * @throws XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public StringValue call(XPathContext context, Sequence[] arguments) throws XPathException {

        StringValue arg0 = (StringValue)arguments[0].head();
        StringValue input = arg0 == null ? StringValue.EMPTY_STRING : arg0;

        if (arguments.length == 3) {
            Sequence[] extra = new Sequence[4];
            System.arraycopy(arguments, 0, extra, 0, 3);
            extra[3] = StringValue.EMPTY_STRING;
            arguments = extra;
        }
        FunctionItem arg2 = (FunctionItem) arguments[2].head();
        java.util.function.BiFunction<UnicodeString, UnicodeString[], UnicodeString> fn = (in, groups) -> {
                try {
                    Sequence result = arg2.call(context, new Sequence[]{new StringValue(in)});
                    return result.head().getUnicodeStringValue();
                } catch (XPathException e) {
                    throw new AssertionError(e);
                }
            };

        RegularExpression re = getRegularExpression(arguments, 1, 2);
        return new StringValue(re.replaceWith(input.getUnicodeStringValue(), fn));
    }

}

// Copyright (c) 2020-2023 Saxonica Limited

