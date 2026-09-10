package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FilteringFeed;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;


/**
 * Generate streaming code for a consuming xsl:on-empty instruction
 */
public class OnEmptyAdjunct extends FilteringAdjunct {

    /**
     * Make a filter for the event stream
     *
     * @param context the XPath dyamic context
     * @param out the next feed in the pipeline, which will always be the feed for the containing ConditionalBlock
     *            instruction
     * @return a filter that can be added to the event stream
     * @throws XPathException if a dynamic evaluation error occurs
     */
    @Override
    public FilteringFeed.Filter makeFilter(XPathContext context, Feed out) throws XPathException {
        final ConditionalBlockAdjunct.ConditionalBlockFeed nextFeed = (ConditionalBlockAdjunct.ConditionalBlockFeed)out;
        return new FilteringFeed.Filter() {
            int firstMatch = Integer.MAX_VALUE;
            public int matches(Item item, int position) throws XPathException {
                // We rely on the fact that on-empty instructions have been moved to the end, which means that
                // on the first call, we check with the receiving Feed whether it is empty so far; and on subsequent
                // calls, we pass the item on unconditionally
                if (position > firstMatch) {
                    return MATCHES;
                } else if (nextFeed.isEmpty()) {
                    firstMatch = position;
                    return MATCHES;
                } else {
                    return DONE;
                }
            }
        };
    }

}

// Copyright (c) 2018 Saxonica Limited.