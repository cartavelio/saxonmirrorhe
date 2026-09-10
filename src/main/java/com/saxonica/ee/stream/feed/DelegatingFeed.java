package com.saxonica.ee.stream.feed;

import net.sf.saxon.event.ProxyOutputter;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;

/**
 * A DelegatingFeed is an ItemFeed that delegates most of its work to a supplied Outputter.
 * This allows standard Outputter code (for example, the WherePopulatedReceiver) to be used
 * in a streaming pipeline.
 */

public abstract class DelegatingFeed extends ItemFeed {

    public DelegatingFeed(ProxyOutputter proxy, XPathContext context) {
        super(proxy, context, true);
    }

    /**
     * Get the result, that is, the ItemFeed to which the items in the result
     * of the expression should be supplied
     *
     * @return the Outputter that consumes the results of this expression
     */

    /*@Nullable*/
    @Override
    public ItemFeed getResultFeed() {
        ProxyOutputter proxy = (ProxyOutputter)getNextOutputter();
        return (ItemFeed) proxy.getNextOutputter();
    }

    @Override
    public void append(Item item) throws XPathException {
        ProxyOutputter proxy = (ProxyOutputter) getNextOutputter();
        proxy.append(item);
    }
}

// Copyright (c) 2020-2023 Saxonica Limited

