package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.FilteringFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpInnerClass;
import net.sf.saxon.value.BooleanValue;

import static com.saxonica.ee.stream.feed.FilteringFeed.FilterResult.DONE;
import static com.saxonica.ee.stream.feed.FilteringFeed.FilterResult.MATCHES;


/**
 * Generate streaming code for the saxon:items-ending-with function. The streamability rules
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
    @CSharpInnerClass(outer = false, extra = {"Saxon.Hej.expr.XPathContext context", "Saxon.Hej.om.FunctionItem function"})
    public FilteringFeed.Filter makeFilter(final XPathContext context, ItemFeed out) throws XPathException {
        final FunctionItem function = (FunctionItem) ((SystemFunctionCall)getExpression()).getArg(1).evaluateItem(context);
        return new FilteringFeed.Filter() {
            boolean found = false;
            @Override
            public FilteringFeed.FilterResult matches(Item item, int position) throws XPathException {
                if (found) {
                    return DONE;
                }
                boolean match = ((BooleanValue) function.call(context, new Sequence[]{item}).head()).getBooleanValue();
                if (match) {
                    found = true;
                }
                return MATCHES;
            }
        };
    }


}

// Copyright (c) 2018-2022 Saxonica Limited