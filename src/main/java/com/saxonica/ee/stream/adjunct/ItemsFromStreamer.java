package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.FilteringFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpInnerClass;
import net.sf.saxon.value.BooleanValue;

import static com.saxonica.ee.stream.feed.FilteringFeed.FilterResult.MATCHES;
import static com.saxonica.ee.stream.feed.FilteringFeed.FilterResult.SKIP;


/**
 * Generate streaming code for the saxon:items-from function. The streamability rules
 * are the same as for the fn:filter function.
 */
public class ItemsFromStreamer extends FilterFnStreamer {

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
    @CSharpInnerClass(outer = false, extra = {"Saxon.Hej.expr.XPathContext context", "Saxon.Hej.om.Function function"})
    public FilteringFeed.Filter makeFilter(final XPathContext context, ItemFeed out) throws XPathException {
        final Function function = (Function) ((SystemFunctionCall)getExpression()).getArg(1).evaluateItem(context);
        return new FilteringFeed.Filter() {
            boolean started = false;
            @Override
            public FilteringFeed.FilterResult matches(Item item, int position) throws XPathException {
                boolean match = started || ((BooleanValue) function.call(context, new Sequence[]{item}).head()).getBooleanValue();
                if (match) {
                    started = true;
                    return MATCHES;
                } else {
                    return SKIP;
                }
            }
        };
    }


}

// Copyright (c) 2018-2022 Saxonica Limited