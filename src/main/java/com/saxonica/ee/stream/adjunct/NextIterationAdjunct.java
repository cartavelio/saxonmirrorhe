package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.FeedMakerDelegate;
import com.saxonica.ee.stream.feed.NextIterationFeed;

/**
 * Compile a xsl:next-intruction instruction to streaming code.
 */

public class NextIterationAdjunct extends StreamingAdjunct {

    @Override
    public FeedMaker getFeedMaker(final int arg) {
        return new FeedMakerDelegate((watchManager, out, context) ->
                                             new NextIterationFeed(getExpression(), out, context, arg));
    }

}

// Copyright (c) 2010-2022 Saxonica Limited