package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.FilteringFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.NumericValue;

import static com.saxonica.ee.stream.feed.FilteringFeed.FilterResult.DONE;
import static com.saxonica.ee.stream.feed.FilteringFeed.FilterResult.MATCHES;
import static com.saxonica.ee.stream.feed.FilteringFeed.FilterResult.SKIP;


/**
 * Generate streaming code for a subsequence() function call. Also used for expressions such as tail() that compile
 * into a subsequence() call
 */
public class SubsequenceStreamer extends FilteringAdjunct implements FeedMaker {


    @Override
    public FeedMaker getFeedMaker(int arg) throws XPathException {
        return arg == 0 ? this : getDefaultFeedMaker(arg);
    }

    @Override
    public FilteringFeed.Filter makeFilter(XPathContext context, ItemFeed out) throws XPathException {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        SystemFunction fn = fnc.getTargetFunction();
        int first, last;
        NumericValue startD = (NumericValue) fnc.getArg(1).evaluateItem(context);
        first = (int)startD.round(0).longValue();
        if (fn.getArity() == 3) {
            NumericValue lenD = (NumericValue) fnc.getArg(2).evaluateItem(context);
            int len = (int)lenD.round(0).longValue();
            last = first + len - 1;
        } else {
            last = Integer.MAX_VALUE;
        }

        return new SubsequenceFilter(first, last);
    }


    public static class SubsequenceFilter implements FilteringFeed.Filter {

        private final int first;
        private final int last;

        public SubsequenceFilter(int first, int last) {
            this.first = first;
            this.last = last;
        }

        @Override
        public FilteringFeed.FilterResult matches(Item item, int position) throws XPathException {
            if (position < first) {
                return SKIP;
            } else if (position <= last) {
                return MATCHES;
            } else {
                return DONE;
            }
        }

    }

}

// Copyright (c) 2013-2022 Saxonica Limited