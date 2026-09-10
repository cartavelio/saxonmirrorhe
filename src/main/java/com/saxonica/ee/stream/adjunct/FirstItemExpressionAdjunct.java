package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.FilteringFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;

import java.util.List;


/**
 * Generate streaming code for a first item expression (when not part of a Watch).
 */
public class FirstItemExpressionAdjunct extends FilteringAdjunct implements FeedMaker {

    /**
     * Get the posture and sweep of this expression as defined in the W3C streamability specifications.
     * This provides an assessment of stylesheet code against the W3C criteria for guaranteed
     * streamability, and is implemented to allow these criteria to be tested. It is not the
     * case that all expression that emerge as streamable from this analysis are currently
     * capable of being streamed by Saxon
     * @param contextInfo     Information about the context item type and posture
     * @param reasons         the caller may supply a list, in which case the implementation may add to this
     */
    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        PostureAndSweep ps = super.computeStreamability(contextInfo, reasons);
        if (ps.getPosture() == Posture.CRAWLING) {
            return new PostureAndSweep(Posture.STRIDING, ps.getSweep());
        } else {
            return ps;
        }
    }

    /**
     * Make a filter for the event stream
     *
     * @param context the XPath dynamic context
     * @param out destination for the output of this filter
     * @return a filter that can be added to the event stream
     * @throws net.sf.saxon.trans.XPathException
     *          if a dynamic evaluation error occurs
     */
    @Override
    public FilteringFeed.Filter makeFilter(XPathContext context, ItemFeed out) throws XPathException {
        return new FilteringFeed.FilterLambda((item, position) ->
                position == 1 ? FilteringFeed.FilterResult.MATCHES : FilteringFeed.FilterResult.DONE);
    }

}

// Copyright (c) 2012-2023 Saxonica Limited