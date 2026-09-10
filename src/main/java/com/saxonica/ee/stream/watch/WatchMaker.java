package com.saxonica.ee.stream.watch;

import com.saxonica.ee.stream.feed.ItemFeed;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;

/**
 * A WatchMaker is a factory class for creating a {@link com.saxonica.ee.stream.watch.Watch}.
 */
@FunctionalInterface
public interface WatchMaker {

    /**
     * Make a Trigger: this comprises (a) a pattern to match the nodes that should be processed
     * during a streaming pass of the source document, and (b) the processing of those nodes
     * by a WatchAction or perhaps a sequence of WatchActions in a pipeline. If a pipeline is needed,
     * this method is responsible for constructing the entire pipeline
     *
     * @param watchManager the WatchManager
     * @param out the destination for events representing the output of this pipeline
     * @param context the dynamic evaluation context
     * @return the new Trigger
     * @throws XPathException if any dynamic error occurs
     */

    Trigger makeWatch(WatchManager watchManager, ItemFeed out, XPathContext context)
            throws XPathException;
}

//
// Copyright (c) 2009-2022 Saxonica Limited
//

