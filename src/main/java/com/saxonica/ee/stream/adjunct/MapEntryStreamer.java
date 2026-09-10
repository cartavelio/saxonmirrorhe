package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.SequenceExtent;

import java.util.ArrayList;
import java.util.List;


/**
 * Streaming code to implement an xsl:map instruction or map constructor, where each
 * map entry is allowed to be consuming. Based on the code of ForkAdjunct
 */
public class MapEntryStreamer extends StreamingAdjunct implements FeedMaker {

    public FeedMaker getFeedMaker(int arg) throws XPathException {
        return arg==1 ? this : getDefaultFeedMaker(arg);
    }

    public Feed makeItemFeed(WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
        return new MapEntryFeed(getExpression(), out, context);
    }


    private class MapEntryFeed extends ItemFeed {

        List<Item> value;

        public MapEntryFeed(Expression exp, Feed result, XPathContext context) {
            super(exp, result, context);
        }


        /**
         * Start evaluating the expression. The default implementation does nothing.
         *
         * @param terminator
         * @throws net.sf.saxon.trans.XPathException
         *          if a dynamic error occurs
         */
        @Override
        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            value = new ArrayList<>();
        }

        /**
         * Supply one item towards the streamed input of the expression
         *
         * @param item the item to be supplied
         */
        @Override
        public void processItem(Item<?> item) throws XPathException {
            value.add(item);
        }

        /**
         * Finish evaluating the expression.
         *
         * @throws net.sf.saxon.trans.XPathException
         *          if a dynamic error occurs
         */
        @Override
        public void close() throws XPathException {
            AtomicValue key = (AtomicValue) ((SystemFunctionCall) getExpression()).getArg(0).evaluateItem(getContext());
            GroundedValue seq = SequenceExtent.makeSequenceExtent(value);
            HashTrieMap map = HashTrieMap.singleton(key, seq);
            getResult().processItem(map);
            getResult().close();
        }

    }
}

// Copyright Saxonica 2014.