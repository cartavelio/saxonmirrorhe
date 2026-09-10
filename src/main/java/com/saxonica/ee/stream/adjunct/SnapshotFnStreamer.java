package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.StreamingPatternMaker;
import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.SnapshotFeed;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;

/**
 * Generate streaming code for the snapshot() function
 */
public class SnapshotFnStreamer extends ComposingAdjunct implements FeedMaker {




    @Override
    public Feed makeComposingAction(WatchManager watchManager, Feed out, XPathContext context) {
        return new SnapshotFeed(watchManager, out, context);
    }

    /*@Nullable*/
    public WatchMaker getWatchMaker(boolean forGrouping) {
        Expression select = ((SystemFunctionCall) getExpression()).getArg(0);
        final Pattern selection = StreamingPatternMaker.makeStreamingPattern(select, getConfiguration(), null);
        if (selection != null) {
            return new WatchMaker() {
                public Trigger makeWatch(WatchManager watchManager, Feed out, XPathContext context) {
                    SnapshotFeed actor = new SnapshotFeed(watchManager, out, context);
                    return new Trigger(selection, actor, context);
                }
            };
        }
        return null;
    }

    public Feed makeItemFeed(WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
        return new SnapshotFeed(watchManager, out, context);
    }

}

// Copyright (c) 2014-2015 Saxonica Limited.