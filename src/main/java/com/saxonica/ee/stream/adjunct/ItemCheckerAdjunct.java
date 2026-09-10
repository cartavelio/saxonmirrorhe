package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemCheckingFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.expr.ItemChecker;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.pattern.DocumentNodeTest;
import net.sf.saxon.trans.XPathException;

import java.util.List;

/**
 * Streaming code for an item checker
 */
public class ItemCheckerAdjunct extends TransmissionAdjunct implements FeedMaker {

    /**
     * Get the sweep and posture of this expression as defined in the W3C streamability specifications.
     * This provides an assessment of stylesheet code against the W3C criteria for guaranteed
     * streamability, and is implemented to allow these criteria to be tested. It is not the
     * case that all expression that emerge as streamable from this analysis are currently
     * capable of being streamed by Saxon
     * @param contextInfo      information about the context item type and posture
     * @param reasons          the caller may supply a list, in which case the implementation may add to this
     */
    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        ItemChecker expr = (ItemChecker) getExpression();
        PostureAndSweep base = Streamability.getStreamability(expr.getBaseExpression(), contextInfo, reasons);
        if (base.getPosture() != Posture.ROAMING && base.getSweep() != Sweep.FREE_RANGING
                && expr.getRequiredType() instanceof DocumentNodeTest) {
            reasons.add("The node-test " + expr.getRequiredType() + " requires look-ahead");
            return new PostureAndSweep(Posture.ROAMING, Sweep.FREE_RANGING);
        }
        return base;
    }


    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new ItemCheckingFeed(getExpression(), out, context);
    }
}

// Copyright (c) 2010-2023 Saxonica Limited