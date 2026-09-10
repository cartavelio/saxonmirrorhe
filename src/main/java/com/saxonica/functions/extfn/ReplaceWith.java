package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.RegexFunction;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.regex.RegularExpression;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.StringValue;


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
    public StringValue call(XPathContext context, Sequence[] arguments) throws XPathException {

        StringValue arg0 = (StringValue)arguments[0].head();
        CharSequence input = arg0 == null ? "" : arg0.getStringValueCS();

        if (arguments.length == 3) {
            Sequence[] extra = new Sequence[4];
            System.arraycopy(arguments, 0, extra, 0, 3);
            extra[3] = new StringValue("");
            arguments = extra;
        }
        Function arg2 = (Function) arguments[2].head();
        java.util.function.Function<CharSequence, CharSequence> fn = in -> {
                try {
                    Sequence result = arg2.call(context, new Sequence[]{new StringValue(in)});
                    return result.head().getStringValueCS();
                } catch (XPathException e) {
                    throw new AssertionError(e);
                }
            };

        RegularExpression re = getRegularExpression(arguments);
        CharSequence res = re.replaceWith(input, fn);
        return StringValue.makeStringValue(res);
    }

}

// Copyright (c) 2020 Saxonica Limited

