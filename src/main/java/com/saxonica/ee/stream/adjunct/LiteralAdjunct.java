package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.pattern.NodeTestPattern;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ErrorType;

import java.util.List;

/**
 * Streamed evaluation of a literal (not difficult!)
 */
public class LiteralAdjunct extends StreamingAdjunct implements FeedMaker {

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        return PostureAndSweep.GROUNDED_AND_MOTIONLESS;
    }

    /**
     * Convert this expression to a streaming pattern (a pattern used internally to match nodes during
     * push processing of an event stream)
     *
     * @param config the Saxon configuration
     * @return the equivalent pattern if conversion succeeds; otherwise null
     */
    @Override
    public Pattern toStreamingPattern(Configuration config) {
        Literal exp = (Literal) getExpression();
        if (Literal.isEmptySequence(exp)) {
            return new NodeTestPattern(ErrorType.getInstance());
        } else {
            return super.toStreamingPattern(config);
        }
    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new LiteralItemFeed(getExpression(), out, context);
    }

    public class LiteralItemFeed extends ItemFeed {

        LiteralItemFeed(Expression exp, ItemFeed out, XPathContext context) {
            super(exp, out, context);
        }

        @Override
        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            Outputter out = getNextOutputter();
            if (!Literal.isEmptySequence(getExpression())) {
                GroundedValue value = ((Literal) getExpression()).getGroundedValue();
                if (value instanceof Item) {
                    out.append((Item) value);
                } else {
                    SequenceIterator iter = value.iterate();
                    processItems(iter, out);
                }
            }
        }

        @Override
        public void append(Item item) throws XPathException {
            // not called
        }
    }
}

// Copyright (c) 2018-2023 Saxonica Limited

