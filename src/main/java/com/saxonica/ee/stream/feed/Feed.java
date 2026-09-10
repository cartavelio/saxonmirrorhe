package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.*;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;

/**
 * A Feed is a processing object that sits on a push pipeline to evaluate some expression. One operand of the
 * expression is a streamed operand, and this operand delivers a sequence either of composed items, or of
 * start/end element events (or a mixture of both). The calls to the Feed represent incremental events in the
 * evaluation of the streamed operand. The Feed is responsible for processing this stream of events and
 * generating a new stream of events representing the result of the expression, which are pushed to the
 * next step in the pipeline in the same way.
 *
 * <p>A "composed feed" consists entirely of items, supplied using the processItem interface. A "decomposed feed"
 * represents a tree using a sequence of startElement/endElement events. In general the code for a particular
 * expression should be prepared to accept either. However, in many cases (especially expressions that atomize
 * their operand), it is known that the input will always be composed, as a result of the action of an earlier
 * step in the pipeline.</p>
 *
 * <p>A Feed musy be serially reusable; that is, its state after exiting the close() method must be the same as
 * the state before entering the open() method.</p>
 */

public interface Feed {

    /**
     * Get the pipeline configuration
     * @return the pipeline configuration
     */

    PipelineConfiguration getPipelineConfiguration();

    /**
     * Open the feed. This is done at the point where evaluation of an expression begins.
     * @throws XPathException May be raised if a dynamic error occurs
     * @param terminator provides a method for early termination of the streamed processing.
     * The Feed should call the terminator.terminate() method, and close() the result, if it
     * has computed its result and requires no more input. After a call on terminate(), the
     * feed MAY be called to supply further input (which it should ignore), or it may never
     * be called again. The Feed should leave its state clean to allow serial reuse, via
     * another call on open().
     */

    void open(Terminator terminator) throws XPathException;

    /**
     * Signal that an element or document node has been found that . This method is called by the WatchManager while
     * processing the startElement or startDocument event that matches the selection.
     *
     *
     * @param node       the element or document node whose start event has been matched
     * @param locationId the location associated with the element or document node (may be
     *                   the location of the instruction that created it)
     * @return a Receiver to be notified of all events starting with the startElement/startDocument event
     *         for the matched element, and ending with the endElement event for that element; or null if this
     *         feature is not required.
     * @throws net.sf.saxon.trans.XPathException May be raised if a dynamic error occurs
     */

    /*@Nullable*/
    Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException;

    /**
     * Signal that the endElement event has occurred for the element whose startElement
     * event caused the Watch to be activated.
     *
     * @param locationId the location of the element
     * @throws net.sf.saxon.trans.XPathException May be raised if a constraint implemented by this
     *                        Watch is violated
     */

    void endSelectedParentNode(Location locationId) throws XPathException;

    /**
     * Signal that a node (other than a document or element node) that matches the selection
     * has been found, or that a grounded item is being passed to the next expression in the
     * evaluation pipeline.
     *
     * @param item       the node that was matched.
     * @throws net.sf.saxon.trans.XPathException May be raised if a dynamic error occurs
     */

    void processItem(Item<?> item) throws XPathException;

    /**
     * The close() method is called immediately before the Watch is destroyed, that is,
     * when the element whose declaration scopes the constraint implemented by this
     * Watch goes out of scope.
     * @throws net.sf.saxon.trans.XPathException May be raised if a dynamic error occurs
     */

    void close() throws XPathException;

    /**
     * Get a receiver to which start and end events for documents and elements can be
     * written. By default this will be the same as the receiver for the result feed,
     * because very few XSLT constructs (specifically, variables and functions) allow
     * the results of document and element constructors to be captured for further
     * processing. A construct that does need to process these events will intercept
     * them by providing its own receiver, typically a SequenceOutputter.
     * @return the receiver
     */

    Receiver getReceiver();

}

// Copyright (c) 2004-2013 Saxonica Limited. No warranty.
