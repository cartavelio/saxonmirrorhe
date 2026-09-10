package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.RegexFunction;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.regex.RegularExpression;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.StringItem;


/**
 * This class implements the saxon:replace-with() function for replacing
 * substrings that match a regular expression by means of a callback to a user-supplied function
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
    public StringItem call(XPathContext context, Sequence[] arguments) throws XPathException {

        StringItem arg0 = (StringItem)arguments[0].head();
        StringItem input = arg0 == null ? StringItem.EMPTY_STRING : arg0;

        if (arguments.length == 3) {
            Sequence[] extra = new Sequence[4];
            System.arraycopy(arguments, 0, extra, 0, 3);
            extra[3] = StringItem.EMPTY_STRING;
            arguments = extra;
        }
        Function arg2 = (Function) arguments[2].head();
        java.util.function.Function<UnicodeString, UnicodeString> fn = in -> {
                try {
                    Sequence result = arg2.call(context, new Sequence[]{new StringItem(in)});
                    return result.head().getUnicodeStringValue();
                } catch (XPathException e) {
                    throw new AssertionError(e);
                }
            };

        RegularExpression re = getRegularExpression(arguments);
        return new StringItem(re.replaceWith(input.getUnicodeStringValue(), fn));
    }

}

// Copyright (c) 2020 Saxonica Limited

