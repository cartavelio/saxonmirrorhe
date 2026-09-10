package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.*;
import net.sf.saxon.expr.GeneralComparison;
import net.sf.saxon.expr.parser.Token;


/**
 * Watch factory class for a general comparison
 */
public class GeneralComparisonAdjunct extends StreamingAdjunct {

    /**
     * @param streamedArg 0 if the LHS operand is streamed, 1 if the RHS operand is streamed
     * @return a feed factory
     */
    @Override
    public FeedMaker getFeedMaker(final int streamedArg) {
        return new FeedMakerDelegate((watchManager, out, context) -> {
            int op = ((GeneralComparison)getExpression()).getSingletonOperator();
            if (op == Token.FEQ) {
                return new GeneralComparisonEqualityFeed(watchManager, getExpression(), streamedArg, out, context);
            } else if (op == Token.FNE) {
                return new GeneralComparisonInequalityFeed(watchManager, getExpression(), streamedArg, out, context);
            } else {
                return new GeneralComparisonMinimaxFeed(watchManager, getExpression(), streamedArg, out, context);
            }
        });
    }
}

// Copyright (c) 2010-2022 Saxonica Limited