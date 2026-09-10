package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.adjunct.TryCatchAdjunct;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.QuitParsingException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ValidationException;

/**
 * An ItemFeed evaluates an expression in "composed push" mode. Its streaming input
 * is supplied via a sequence of append() calls that supply individual (composed) items
 * in the input sequence, and its output is (in most cases) delivered via a sequence of
 * append() calls to the feed representing the parent expression. The exception is
 * the FeedToReceiver class which appends the supplied items to a Receiver.
 * <p>Every kind of expression can be evaluated using an ItemFeed. The ItemFeed is obtained
 * by calling getFeedMaker() on the ExpressionCompiler corresponding to the class of expression.
 * The expression must have been previously marked to indicate which of its inputs is being
 * supplied in streaming mode.</p>
 */

public abstract class ItemFeed implements Feed {

    private Terminator terminator;
    private Expression expression;
    private Feed result;
    private XPathContext context;
    private PipelineConfiguration pipe;
    private boolean failed = false;

    public ItemFeed(Feed result, XPathContext context) {
        this.result = result;
        this.context = context;
        if (result != null) {
            this.pipe = result.getPipelineConfiguration();
        } else {
            throw new NullPointerException();
        }
    }

    public ItemFeed(Expression exp, Feed result, XPathContext context) {
        this.expression = exp;
        this.result = result;
        this.context = context;
        if (result != null) {
            this.pipe = result.getPipelineConfiguration();
        } else {
            throw new NullPointerException();
        }
    }

    /**
     * Convenience method to process all the items selected by an iterator and push them to the next
     * feed in the push pipeline
     * @param iter iterator over the selected items
     * @param result the next feed in the pipeline
     * @throws net.sf.saxon.trans.XPathException if any dynamic error occurs
     */

    public static void processItems(SequenceIterator<?> iter, Feed result) throws XPathException {
        try {
            iter.forEachOrFail(result::processItem);
        } catch (QuitParsingException quit) {
            throw quit;
        } catch (XPathException e) {
            if (result instanceof ItemFeed) {
                ((ItemFeed) result).dynamicError(e);
            } else {
                // TODO: need the same logic on other kinds of feed
                throw e;
            }
        }
    }

    /**
     * Set the expression being evaluated by this ItemFeed
     * @param exp the relevant expression
     */

    public void setExpression(Expression exp) {
        this.expression = exp;
    }

    /**
     * Get the expression being evaluated by this ItemFeed. May be null if not known or not relevant
     * @return the expression being evaluated
     */

    public Expression getExpression() {
        return expression;
    }

    /**
     * Provide a callback that can be used to request early termination of the
     * streaming of this input document
     * @param terminator the class that can be called to request termination
     */

    public void setTerminator(Terminator terminator) {
        this.terminator = terminator;
    }

    /**
     * Mark this feed as having failed (that is, as having detected and reported
     * a dynamic error). Once the feed is marked as having failed, it should not
     * attempt to process any further input.
     */

    public void setHasFailed() {
        failed = true;
    }

    /**
     * Get the Terminator which can be used to request early termination of the Feed
     * @return the Terminator that was passed to the open() call, or null if open() has not been called
     */

    public Terminator getTerminator() {
        return terminator;
    }

    /**
     * Get the result, that is, the ItemFeed to which the items in the result
     * of the expression should be supplied
     *
     * @return the ItemFeed that consumes the results of this expression
     */

    /*@Nullable*/
    public Feed getResult() {
        return result;
    }

    /**
     * Set the pipeline configuration
     *
     * @param pipe the pipeline configuration
     */

    public void setPipelineConfiguration(PipelineConfiguration pipe) {
        this.pipe = pipe;
    }

    /**
     * Get the pipeline configuration
     *
     * @return the pipeline configuration
     */

    public PipelineConfiguration getPipelineConfiguration() {
        return pipe;
    }


    /**
     * Get the dynamic evaluation context
     *
     * @return the dynamic evaluation context
     */

    public XPathContext getContext() {
        return context;
    }

    /**
     * Start evaluating the expression. The default implementation does nothing.
     *
     * @throws XPathException if a dynamic error occurs
     * @param terminator used to achieve early exit
     */

    public void open(Terminator terminator) throws XPathException {
        this.terminator = terminator;
        result.open(terminator);
    }

    /**
     * Signal that an element or document node has been found that matches the selection that this
     * Watch is looking for. This method is called by the WatchManager while
     * processing the startElement or startDocument event that matches the selection.
     *
     * @param node       the element or document node whose start event has been matched
     * @param locationId the location associated with the element or document node (may be
     *                   the location of the instruction that created it)
     * @return a Receiver to be notified of all events starting with the startElement/startDocument event
     *         for the matched element, and ending with the endElement event for that element; or null if this
     *         feature is not required.
     * @throws net.sf.saxon.trans.XPathException
     *          May be raised if a dynamic error occurs
     */
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        processItem(node);
        return null;
    }

    /**
     * Supply one item towards the streamed input of the expression
     *
     * @param item the item to be supplied
     */

    public abstract void processItem(Item<?> item) throws XPathException;

    /**
     * Signal that the endElement event has occurred for the element whose startElement
     * event caused the Watch to be activated.
     *
     * @param locationId the location of the element
     * @throws net.sf.saxon.trans.XPathException
     *          May be raised if a constraint implemented by this
     *          Watch is violated
     */
    public void endSelectedParentNode(Location locationId) throws XPathException {
        // no action
    }

    /**
     * Finish evaluating the expression. The default implementation does nothing.
     *
     * @throws XPathException if a dynamic error occurs
     */

    public void close() throws XPathException {
        if (!hasFailed()) {
            try {
                result.close();
            } catch (XPathException e) {
                if (expression != null) {
                    e.maybeSetLocation(expression.getLocation());
                }
                throw e;
            }
        }
    }

    /**
     * Get a receiver to which start and end events for documents and elements can be
     * written.
     */
    @Override
    public Receiver getReceiver() {
        Receiver next = getResult().getReceiver();
        return new ProxyReceiver(next) {
            /**
             * Append an arbitrary item (node or atomic value) to the output
             *
             * @param item           the item to be appended
             * @param locationId     the location of the calling instruction, for diagnostics
             * @param copyNamespaces if the item is an element node, this indicates whether its namespaces
             *                       need to be copied. Values are {@link ReceiverOptions#ALL_NAMESPACES},
             *                       {@link ReceiverOptions#LOCAL_NAMESPACES}; the default (0) means
             *                       no namespaces
             */
            @Override
            public void append(Item item, Location locationId, int copyNamespaces) throws XPathException {
                processItem(item);
            }
        };
    }

    /**
     * Report a dynamic error. This requires searching up the "feed" pipeline to see
     * if there is a try/catch expression interested in being notified of this error;
     * if there is none, the error is thrown as an exception
     * @param error the exception representing the dynamic error
     * @throws XPathException if there is no try/catch expression wanting to catch this error
     */

    public void dynamicError(XPathException error) throws XPathException {
        setHasFailed();
        if (expression != null) {
            error.maybeSetLocation(expression.getLocation());
        }
        Feed next = getResult();
        //noinspection InfiniteLoopStatement
        while (true) {
            if (next instanceof TryCatchAdjunct.TryCatchFeed) {
                ((TryCatchAdjunct.TryCatchFeed)next).dynamicError(error);
                return;
            } else if (next instanceof ItemFeed) {
                ((ItemFeed)next).setHasFailed();
                next = ((ItemFeed) next).getResult();
            } else {
                if (error instanceof ValidationException) {
                    // Make it fatal: processing might continue after a ValidationException and we can't handle that
                    XPathException xe = new XPathException(error);
                    xe.setErrorCodeQName(error.getErrorCodeQName());
                    throw xe;
                }
                throw error;
            }
        }
    }

    public boolean hasFailed() {
        return failed;
    }
}

// Copyright (c) 2009-2014 Saxonica Limited.
