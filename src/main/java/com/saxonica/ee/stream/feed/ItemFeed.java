package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.adjunct.TryCatchAdjunct;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.ProxyOutputter;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.ItemConsumer;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.QuitParsingException;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ValidationException;

/**
 * An ItemFeed evaluates an expression in "composed push" mode. Its streaming input
 * is supplied via a sequence of event calls (start element, end element, etc) that
 * represent the value of the streamed operand of the expression; other operands
 * are evaluated in pull mode in the usual way.
 *
 * <p>Expressions are thus evaluated "bottom up": instead of a parent expression
 * triggering the evaluation of its children, the appearance of data needed by a child
 * expression causes evaluation of the child expression, which then triggers evaluation
 * of its parent, and so on.</p>
 *
 * <p>One consequence of this is that special mechanisms are needed for error handling.
 * In particular, it's not possible for a child expression to notify its parent expression
 * of dynamic errors simply by throwing a Java exception: the error information needs
 * to be passed up the pipeline in the same way as regular results.</p>
 *
 * <p>Some expressions (inspection expressions) are only interested in top-level events:
 * for example {@code count()}, {@code exists}, and {@code instance of} only need to know
 * about a {@code startElement} event, they are not interested in the content of the element,
 * only in its existence. Other expressions (absorption expression) need access to the content
 * of the element: in most cases, the typed value of the element, but in some cases (such as
 * {@code copy-of()} or {@code snapshot()} a finer-grained representation of the content.
 * In such cases the {@link #startSelectedParentNode(FleetingParentNode, Location)} method
 * (which is notified of a top-level event in the input stream) returns a {@link Receiver},
 * which is then notified of all the contained events up to the corresponding {@code endElement}.</p>
 *
 * <p>Some expressions (transmission expression) pass data on to their parent expressions directly,
 * and in these cases the decision whether to ask for full details of the content is delegated
 * to the parent expression, or beyond.</p>
 *
 * <p>An expression whose result is known before all the input is available (for example,
 * {@code exists()} can call a supplied {@link Terminator} to indicate that no more input is
 * required. In some cases (where the expression would otherwise read an entire document)
 * this enables parsing of the streamed input file to be abandoned.</p>
 *
 * <p>The events passed up the pipeline will usually correspond to nodes in the streamed source
 * document, or to atomic values computed from those nodes. In unusual edge cases, however, a
 * consuming expression may generate new element or document nodes, which are still delivered
 * incrementally because their content is established by streaming the source document. Events
 * representing these constructed elements are passed up the pipeline as if they came from
 * the source document. (Note, because this situation is unusual and somewhat contrived, it
 * probably has incomplete test coverage and may not be fully implemented for all kinds of
 * expression).</p>
 *
 * <p>The start of an element may be represented either as a single event containing all the
 * attributes and namespaces, or as a sequence of separate events (start element, attributes,
 * namespaces, then {@code startContent}). Events coming directly from parsing the streamed
 * input will use the more efficient "bundled" form, but events representing constructed nodes
 * may be "unbundled" because attributes and namespaces can be constructed incrementally as the
 * streamed input is processed. An {@code ItemFeed} for a parent expression should therefore
 * be prepared to accept either form.</p>
 */

public abstract class ItemFeed extends ProxyOutputter {

    private Terminator terminator;
    private Expression expression;
    private final XPathContext context;
    private boolean failed = false;

    public ItemFeed(ItemFeed result, XPathContext context) {
        super(result);
        this.context = context;
    }

    public ItemFeed(Expression exp, ItemFeed result, XPathContext context) {
        super(result);
        this.expression = exp;
        this.context = context;
    }

    /**
     * Special constructor for the last ItemFeed in the pipeline, which feeds into an Outputter
     * rather than into another ItemFeed. Note that the getResult() method in this situation
     * will throw an exception.
     * @param result the resulting outputter
     * @param context the dynamic evaluation context
     * @param thisIsAnExceptionalCase value is ignored; the parameter is there simply to flag
     *                                this constructor as a special case
     */
    public ItemFeed(Outputter result, XPathContext context, boolean thisIsAnExceptionalCase) {
        super(result);
        this.context = context;
    }

    /**
     * Convenience method to process all the items selected by an iterator and push them to the next
     * feed in the push pipeline
     * @param iter iterator over the selected items
     * @param result the next feed in the pipeline
     * @throws net.sf.saxon.trans.XPathException if any dynamic error occurs
     */

    public static void processItems(SequenceIterator iter, Outputter result) throws XPathException {
        try {
            SequenceTool.supply(iter, (ItemConsumer<? super Item>) result::append);
        } catch (UncheckedXPathException unxe) {
            XPathException xe = unxe.getXPathException();
            if (xe instanceof QuitParsingException) {
                throw xe;
            } else if (result instanceof ItemFeed) {
                // Try to pass the error up the pipeline to the next try/catch
                ((ItemFeed) result).dynamicError(xe);
            } else {
                throw xe;
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
     * @return the Outputter that consumes the results of this expression
     */

    /*@Nullable*/
    public ItemFeed getResultFeed() {
        return (ItemFeed)getNextOutputter();
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
        getResultFeed().open(terminator);
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
//        throw new UnsupportedOperationException();
        append(node);
        return null;
    }

    /**
     * Supply one item towards the streamed input of the expression
     *
     * @param item the item to be supplied
     */

    @Override
    public abstract void append(Item item) throws XPathException;

    /**
     * Supply one item towards the streamed input of the expression. This implementation
     * calls {@link #append(Item)}. Subclasses do not have to cater for
     * this event
     *
     * @param item the item to be supplied
     */

    @Override
    public final void append(Item item, Location locationId, int properties) throws XPathException {
        append(item);
    }

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
     * Finish evaluating the expression.
     *
     * @throws XPathException if a dynamic error occurs
     */

    @Override
    public void close() throws XPathException {
        if (!hasFailed()) {
            try {
                getNextOutputter().close();
            } catch (XPathException e) {
                if (expression != null) {
                    e.maybeSetLocation(expression.getLocation());
                }
                throw e;
            }
        }
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
        ItemFeed next = getResultFeed();
        while (true) {
            if (next instanceof TryCatchAdjunct.TryCatchFeed) {
                next.dynamicError(error);
                return;
            } else {
                next.setHasFailed();
                Outputter result = next.getNextOutputter();
                if (result instanceof ItemFeed) {
                    next = ((ItemFeed)result);
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
    }

    public boolean hasFailed() {
        return failed;
    }

//    /**
//     * Notify the start of an element, supplying all attributes and namespaces
//     *
//     * @param elemName   the name of the element.
//     * @param type       the type annotation of the element.
//     * @param attributes the attributes of this element
//     * @param namespaces the in-scope namespaces of this element: generally this is all the in-scope
//     *                   namespaces, without relying on inheriting namespaces from parent elements
//     * @param location   an object providing information about the module, line, and column where the node originated
//     * @param properties bit-significant properties of the element node. If there are no relevant
//     *                   properties, zero is supplied. The definitions of the bits are in class {@link ReceiverOption}
//     * @throws XPathException if an error occurs
//     */
//    @Override
//    public void startElement(NodeName elemName, SchemaType type, AttributeMap attributes, NamespaceMap namespaces, Location location, int properties) throws XPathException {
//        startElement(elemName, type, location, properties);
//        for (AttributeInfo att: attributes) {
//            attribute(att.getNodeName(), att.getType(), att.getValue(), att.getLocation(),att.getProperties());
//        }
//        startContent();
//    }
}

// Copyright (c) 2009-2022 Saxonica Limited
