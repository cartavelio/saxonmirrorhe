package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FilteringFeed;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;


/**
 * Generate streaming code for the fn:filter function. Only streamable if the operand is grounded
 */
public class FilterFnStreamer extends FilteringAdjunct {

    /**
     * Make a filter for the event stream
     *
     * @param context the XPath dyamic context
     * @param out
     * @return a filter that can be added to the event stream
     * @throws net.sf.saxon.trans.XPathException
     *          if a dynamic evaluation error occurs
     */
    @Override
    public FilteringFeed.Filter makeFilter(final XPathContext context, Feed out) throws XPathException {
        final Function function = (Function) ((SystemFunctionCall)getExpression()).getArg(1).evaluateItem(context);
        final Sequence[] args = SequenceTool.makeSequenceArray(1);
        return new FilteringFeed.Filter() {
            public int matches(Item item, int position) throws XPathException {
                args[0] = item;
                return ((BooleanValue) function.call(context, args).head()).getBooleanValue() ? MATCHES : SKIP;
            }
        };
    }


}

// Copyright (c) 2014 Saxonica Limited.