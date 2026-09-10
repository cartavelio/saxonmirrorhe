package com.saxonica.ee.stream;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.pattern.PatternMaker;

import java.util.List;

/**
 * Factory class to make a streaming pattern from an expression. This is a variant of the class
 * {@link PatternMaker} which handles the special rules for the streaming case.
 */
public class StreamingPatternMaker {

    /**
     * Convert an expression that is capable of being evaluated as a filter on an XML event stream
     * into a Pattern object which is able to act as the filter
     *
     * @param expression       the expression to be converted, which must already have been simplified
     *                         and type-checked
     * @param config           the Saxon configurtaion
     * @param reasonForFailure a list which will be populated with messages giving reasons why the
     *                         expression cannot be converted
     * @return the Pattern object if conversion succeeds; otherwise null
     */

    public static Pattern makeStreamingPattern(Expression expression, Configuration config, List<String> reasonForFailure) {
        Pattern result = Streamability.toStreamingPattern(expression, config);
        if (result == null) {
            if (reasonForFailure != null && reasonForFailure.isEmpty()) {
                reasonForFailure.add("Cannot convert the " + expression.getExpressionName() + " expression to a streamed selection");
            }
            return null;
        } else {
            return result;
        }
    }
}

//
// Copyright (c) 2012-2022 Saxonica Limited
//



