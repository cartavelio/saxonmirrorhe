package com.saxonica.ee.stream.feed;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMinor;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.SequenceExtent;

import java.util.ArrayList;
import java.util.List;

/**
 * A BufferingFeed is used to wrap an expression that does not have intrinsic capability
 * to accept streamed input. The expression must have been modified to read its input from
 * a SuppliedParameterReference referring to the dynamic context stack. This feed buffers up
 * the streamed input, and on its close() method calls the wrapped expression having placed
 * the buffered input onto the context stack, from where the expression will now read it.
 */


public class BufferingFeed extends ItemFeed {

    private List<Item<?>> inputValue = new ArrayList<>(4);

    public BufferingFeed(Expression expr, Feed result, XPathContext context) {
        super(expr, result, context);
    }

    /**
     * Supply an item to the feed
     *
     * @param item the item being fed
     * @throws net.sf.saxon.trans.XPathException
     *          to report any failure
     */

    public void processItem(Item<?> item) throws XPathException {
        inputValue.add(item);
        if (inputValue.size() == 2) {
            XPathContext c = getContext();
            c.getController().getErrorListener().warning(
                    new XPathException("Execution of " + getExpression().toShortString() + " is not fully streamed", SaxonErrorCode.SXST0067, getExpression().getLocation())
            );
        }
    }

    /**
     * Signal the end of the sequence of items
     *
     * @throws net.sf.saxon.trans.XPathException
     *          to report any failure
     */

    public void close() throws XPathException {
        ((XPathContextMinor)getContext()).makeStackFrameMutable();
        getContext().getStackFrame().pushDynamicValue(SequenceExtent.makeSequenceExtent(inputValue));
        inputValue = new ArrayList<>(4);
        SequenceIterator<?> iter = getExpression().iterate(getContext());
        iter.forEachOrFail(item -> getResult().processItem(item));
        getResult().close();
    }

}

// Copyright (c) 2009-2014 Saxonica Limited