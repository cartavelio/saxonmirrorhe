package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.watch.Terminator;
import net.sf.saxon.expr.instruct.NextIteration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.WithParam;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.SequenceExtent;

import java.util.ArrayList;
import java.util.List;

/**
 * This class is used when one of the xsl:with-param elements within an xsl:next-iteration
 * of an xsl:iterate consumes the streamed input
 */


public class NextIterationFeed extends ItemFeed {

    private final int streamedArgument;
    private final List<Item> inputValue = new ArrayList<>(4);
    Sequence[] newValues;

    public NextIterationFeed(Expression inst, ItemFeed result, XPathContext context, int arg) throws XPathException {
        super(inst, result, context);
        WithParam[] actualParams = ((NextIteration) getExpression()).getParameters();
        newValues = SequenceTool.makeSequenceArray(actualParams.length);
        streamedArgument = arg;
    }

    @Override
    public void open(Terminator terminator) throws XPathException {
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               getResultFeed().open(terminator);
        WithParam[] actualParams = ((NextIteration) getExpression()).getParameters();
        XPathContext context = getContext();
        if (actualParams.length > 1) {
            // Don't overwrite the old parameter values until all the parameters have been evaluated
            for (int i = 0; i < actualParams.length; i++) {
                if (i != streamedArgument) {
                    WithParam wp = actualParams[i];
                    newValues[i] = wp.getSelectValue(context);
                }
            }
        }
    }

    /**
     * Supply an item to the feed
     *
     * @param item the item being fed
     * @throws net.sf.saxon.trans.XPathException
     *          to report any failure
     */

    @Override
    public void append(Item item) throws XPathException {
        inputValue.add(item);
    }

    /**
     * Signal the end of the sequence of items
     *
     * @throws net.sf.saxon.trans.XPathException
     *          to report any failure
     */

    @Override
    public void close() throws XPathException {
        WithParam[] actualParams = ((NextIteration) getExpression()).getParameters();
        WithParam wp = actualParams[streamedArgument];
        XPathContext context = getContext();
        context.setLocalVariable(wp.getSlotNumber(), SequenceExtent.makeSequenceExtent(inputValue));
        for (int i = 0; i < actualParams.length; i++) {
            if (i != streamedArgument) {
                wp = actualParams[i];
                context.setLocalVariable(wp.getSlotNumber(), newValues[i]);
            }
        }
        getNextOutputter().close();
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
