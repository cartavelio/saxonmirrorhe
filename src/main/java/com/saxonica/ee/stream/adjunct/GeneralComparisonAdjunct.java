package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.*;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.GeneralComparison;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.trans.XPathException;


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
        return new FeedMaker() {
            @Override
            public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
                int op = ((GeneralComparison)getExpression()).getSingletonOperator();
                if (op == Token.FEQ) {
                    return new GeneralComparisonEqualityFeed(getExpression(), streamedArg, out, context);
                } else if (op == Token.FNE) {
                    return new GeneralComparisonInequalityFeed(getExpression(), streamedArg, out, context);
                } else {
                    return new GeneralComparisonMinimaxFeed(getExpression(), streamedArg, out, context);
                }
            }
        };
    }
}

// Copyright (c) 2010-2020 Saxonica Limited