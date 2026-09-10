package com.saxonica.functions.qt4;

import com.ibm.icu.text.BreakIterator;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.LazySequence;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.StringValue;

/**
 * Implements the fn:graphemes() function.
 * This splits a supplied string into a sequence of single-grapheme strings.
 * Relies on ICU4J support.
 */

public class GraphemesFn extends SystemFunction {

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
        StringValue s = (StringValue)arguments[0].head();
        if (s == null || s.isEmpty()) {
            return EmptySequence.getInstance();
        }
        BreakIterator breakIterator = BreakIterator.getCharacterInstance();
        String input = s.getStringValue();
        breakIterator.setText(input);
        SequenceIterator graphemes = new GraphemeIterator(input, breakIterator);
        return new LazySequence(graphemes);
    }

    private static class GraphemeIterator implements SequenceIterator {

        private final BreakIterator breakIterator;
        private final String input;
        private int start;

        public GraphemeIterator(String input, BreakIterator breakIterator) {
            this.breakIterator = breakIterator;
            this.input = input;
            this.start = breakIterator.first();
        }

        public StringValue next() {
            int end = breakIterator.next();
            if (end == BreakIterator.DONE) {
                return null;
            }
            StringValue result = new StringValue(input.substring(start, end));
            start = end;
            return result;
        }
    }


}

// Copyright (c) 2020-2023 Saxonica Limited

