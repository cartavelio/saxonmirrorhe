package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.StreamingPatternMaker;
import com.saxonica.ee.stream.feed.TypedValueFeed;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchMaker;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SingletonAtomizer;
import net.sf.saxon.pattern.Pattern;

/**
 * Watch factory class for a singleton atomizer
 */
public class SingletonAtomizerAdjunct extends StreamingAdjunct {

    /*@Nullable*/
    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) {
        Expression select = ((SingletonAtomizer) getExpression()).getBaseExpression();
        final Pattern selection = StreamingPatternMaker.makeStreamingPattern(select, getConfiguration(), null);
        if (selection != null) {
            return (watchManager, out, context) -> {
                TypedValueFeed action = new TypedValueFeed(out, context);
                Trigger watch = new Trigger(selection, action, context);
                watch.setPipelineConfiguration(watchManager.getPipelineConfiguration());
                return watch;
            };
        }
        return null;
    }
}

// Copyright (c) 2010-2022 Saxonica Limited