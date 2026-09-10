
package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.List;
import java.util.Stack;

/**
 * This class is used as an abstract superclass for expressions where the consuming
 * operand is absorbed by the expression: for example, data(), string(), or copy-of().
 * It handles the fact that the input can be crawling, which means that selected nodes
 * can be nested. We therefore need to be collecting the result values of multiple nodes
 * during the same pass of the input. Hence we maintain a stack of values-under-construction.
 * Moreover, an inner value will be available before the outer value, but it follows the outer
 * value in document order, so it needs to be held pending until the outer value is available.
 * Hence we maintain a list of values that are ready to be output.
 */


public abstract class AbsorptionFeed extends GroundedItemFeed {

    private final Stack<PendingItem> valuesUnderConstructionStack = new Stack<>();
    List<Sequence> valuesReadyToBeOutput = new ArrayList<>(5);


    public AbsorptionFeed(ItemFeed result, XPathContext context) {
        super(result, context);
    }

    /**
     * Signal that an element has been found that matches the selection that this
     * Watch is looking for.
     * @param node the node that matches the selection
     * @param locationId the location of the instruction
     * @throws XPathException  if a dynamic error occurs
     * @return a receiver that collects the contents of the selected node
     */

    @Override
    public final Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        PendingItem item = makePendingItem(valuesReadyToBeOutput.size(), node);
        valuesUnderConstructionStack.push(item);
        valuesReadyToBeOutput.add(StringValue.EMPTY_STRING);     // to be replaced later
        return item.getGatherer();
    }

    /**
     * Create a pending item (appropriate to the subclass) containing the data needed
     * to compute the result value for one node in the streamed input sequence
     * @param sequenceNr the sequence number of this node within a set of nested nodes
     * @param node the streamed input node
     * @return the data needed to process this node
     * @throws XPathException if a dynamic error is detected at this stage
     */

    protected abstract PendingItem makePendingItem(int sequenceNr, FleetingParentNode node) throws XPathException;

    /**
     * Signal that the endElement event has occurred for the element whose startElement
     * event caused the Watch to be activated.
     *
     * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs
     * @param locationId the location of the node in the source, or of the instruction that created it
     */

    @Override
    public final void endSelectedParentNode(Location locationId) throws XPathException {
        if (!hasFailed()) {
            PendingItem item = valuesUnderConstructionStack.pop();
            int seq = item.getSequenceNr();

            valuesReadyToBeOutput.set(seq, item.deliver());

            if (valuesUnderConstructionStack.isEmpty()) {
                Outputter out = getNextOutputter();
                for (Sequence val : valuesReadyToBeOutput) {
                    processItems(val.iterate(), out);
                }
                valuesReadyToBeOutput.clear();
            }
        }
    }


    /**
     * Signal that a node (other than an element) that matches the selection
     * has been found. This may be a leaf node in the streamed input, or it may
     * be a grounded node or atomic value (of any kind) that has been mixed into the input sequence
     * for example using a comma operator.
     *
     * @param item       the node or other item that was matched.
     *
     */

    @Override
    public void append(Item item) throws XPathException {
        if (!hasFailed()) {
            Sequence value = processGroundedItem(item);
            if (!hasFailed()) {
                if (valuesUnderConstructionStack.isEmpty()) {
                    if (value instanceof Item) {
                        // common case, fast path
                        getNextOutputter().append((Item)value);
                    } else {
                        processItems(value.iterate(), getNextOutputter());
                    }
                } else {
                    // We are currently processing nested elements. The output will have to wait until the elements
                    // are all finished.
                    valuesReadyToBeOutput.add(value);
                }
            }
        }
    }

    /**
     * Process a grounded item in the input sequence: either a streamed leaf node such as an attribute
     * or text node, or an atomic value, or a non-streamed element or document node injected into the
     * sequence for example by use of the comma operator.
     * @param item the grounded item
     * @return the result of processing the grounded item
     * @throws XPathException if a dynamic error occurs
     */

    protected abstract Sequence processGroundedItem(Item item) throws XPathException;

    @Override
    public void setHasFailed() {
        if (!valuesUnderConstructionStack.isEmpty()) {
            PendingItem item = valuesUnderConstructionStack.peek();
            item.rollback();
        }
        super.setHasFailed();
    }


    /**
     * A pending item represents a tree under construction.
     * The sequence number represents its position relative to other trees under construction.
     * The builder is the builder being used to construct the tree.
     */

    public abstract static class PendingItem {

        int sequenceNr;
        public PendingItem(int sequenceNr) {
            this.sequenceNr = sequenceNr;
        }

        /**
         * Get the sequence number of this item within a set of items representing nested nodes
         * @return the sequence number
         */

        public int getSequenceNr() {
            return sequenceNr;
        }

        /**
         * Get a receiver which will be notified of all events in the streamed input between the start event
         * for this node and the corresponding end event. This Receiver gathers data into the pending item
         * so that the final result of processing the streamed node can be computed by the deliver() method
         * @return a receiver to be notified of all events relating to this streamed input node
         * @throws XPathException if a dynamic error is detected
         */

        public abstract Receiver getGatherer() throws XPathException;

        /**
         * On completion of this streamed node, that is, when the relevant end tag is reached,
         * compute the result of processing this node and return this result
         * @return the result of processing a streamed input node
         * @throws XPathException if the processing fails with a dynamic error
         */

        public abstract Sequence deliver() throws XPathException;

        /**
         * Reset the data relating to this node if a dynamic error occurs while processing the
         * input stream.
         */

        public abstract void rollback();
    }
}

// Copyright (c) 2009-2022 Saxonica Limited