package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.StreamingPatternMaker;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.FilteringFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.feed.SimpleContentFeed;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.AdjacentTextNodeMerger;
import net.sf.saxon.expr.EmptyTextNodeRemover;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;

/**
 * Streamed evaluation of an EmptyTextNodeRemover expression.
 */

public class EmptyTextNodeRemoverAdjunct extends StreamingAdjunct implements FeedMaker {

    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) {
        final EmptyTextNodeRemover scc = (EmptyTextNodeRemover) getExpression();
        if (Streamability.getSweep(scc) == Sweep.CONSUMING) {
            final Expression select = scc.getBaseExpression();
            final Pattern selection = StreamingPatternMaker.makeStreamingPattern(select, getConfiguration(), null);
            if (selection != null) {
                return (watchManager, out, context) -> {
                    SimpleContentFeed action = new SimpleContentFeed(out);
                    Trigger watch = new Trigger(selection, action, context);
                    watch.setPipelineConfiguration(watchManager.getPipelineConfiguration());
                    return watch;
                };
            }
            return null;
        } else {
            return null;
        }
    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
          return new FilteringFeed(watchManager, out, context,
                                   new FilteringFeed.FilterLambda((item, position) ->
                                           (!AdjacentTextNodeMerger.isTextNode(item) || !item.getUnicodeStringValue().isEmpty()) ? FilteringFeed.FilterResult.MATCHES : FilteringFeed.FilterResult.SKIP));
    }
}

// Copyright (c) 2010-2022 Saxonica Limited
