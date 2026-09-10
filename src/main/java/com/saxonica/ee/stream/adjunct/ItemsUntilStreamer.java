package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.FilteringFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;


/**
 * Generate streaming code for the saxon:items-after function. The streamability rules
 * are the same as for the fn:filter function.
 */
public class ItemsUntilStreamer extends FilterFnStreamer {

    /**
     * Make a filter for the event stream
     *
     * @param context the XPath dyamic context
     * @param out the destination for the filtered events
     * @return a filter that can be added to the event stream
     * @throws XPathException
     *          if a dynamic evaluation error occurs
     */
    @Override
    public FilteringFeed.Filter makeFilter(final XPathContext context, ItemFeed out) throws XPathException {
        final Function function = (Function) ((SystemFunctionCall)getExpression()).getArg(1).evaluateItem(context);
        final Sequence[] args = SequenceTool.makeSequenceArray(1);
        return new FilteringFeed.Filter() {
            boolean found = false;
            @Override
            public int matches(Item item, int position) throws XPathException {
                args[0] = item;
                if (found) {
                    return DONE;
                }
                boolean match = ((BooleanValue) function.call(context, args).head()).getBooleanValue();
                if (match) {
                    found = true;
                }
                return MATCHES;
            }
        };
    }


}

// Copyright (c) 2018-2020 Saxonica Limited