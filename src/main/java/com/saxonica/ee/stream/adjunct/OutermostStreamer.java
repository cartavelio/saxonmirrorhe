package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.FilteringFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.Navigator;

import java.util.List;

import static com.saxonica.ee.stream.feed.FilteringFeed.FilterResult.MATCHES;
import static com.saxonica.ee.stream.feed.FilteringFeed.FilterResult.SKIP;


/**
 * Streaming code for an outermost() function call.
 */
public class OutermostStreamer extends FilteringAdjunct implements FeedMaker {

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

    @Override
    public FilteringFeed.Filter makeFilter(XPathContext context, ItemFeed out) throws XPathException {
        return new OutermostFilter();
    }

    /**
     * OutermostFilter is a filtering feed which matches a node only if it is not a
     * descendant of the most recently selected node
     */

    private static class OutermostFilter implements FilteringFeed.Filter {

        private NodeInfo previous = null;

        public OutermostFilter() {
        }

        @Override
        public FilteringFeed.FilterResult matches(Item item, int position) {
            if (previous==null ||
                    !Navigator.isAncestorOrSelf(previous, (NodeInfo)item)) {
                previous = (NodeInfo)item;
                return MATCHES;
            } else {
                return SKIP;
            }
        }
    }

}

// Copyright (c) 2013-2022 Saxonica Limited