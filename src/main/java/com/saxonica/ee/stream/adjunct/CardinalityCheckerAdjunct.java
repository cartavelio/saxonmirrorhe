package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.feed.CardinalityCheckingFeed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.expr.CardinalityChecker;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;

import java.util.List;

/**
 * Generate streaming code for a cardinality checker
 */
public class CardinalityCheckerAdjunct extends TransmissionAdjunct implements FeedMaker {

    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        return null;
    }

    /**
     * Get the sweep and posture of this expression as defined in the W3C streamability specifications.
     * This provides an assessment of stylesheet code against the W3C criteria for guaranteed
     * streamability, and is implemented to allow these criteria to be tested. It is not the
     * case that all expression that emerge as streamable from this analysis are currently
     * capable of being streamed by Saxon
     * @param contextInfo  static information about the context item
     * @param reasons          the caller may supply a list, in which case the implementation may add to this
     */
    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        return Streamability.getStreamability(((CardinalityChecker)getExpression()).getBaseExpression(), contextInfo, reasons);
    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new CardinalityCheckingFeed(getExpression(), out, context);
    }

}

// Copyright (c) 2006-2022 Saxonica Limited


