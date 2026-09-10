package com.saxonica.ee.stream.watch;

import com.saxonica.ee.stream.om.FleetingNode;
import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.accum.Accumulator;
import net.sf.saxon.om.Item;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.pattern.NodeTestPattern;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Type;

import java.util.Map;
import java.util.Stack;

/**
 * This watch managers a set of accumulators in streaming mode. Unlike other watches,
 * it is called to process every node in the stream; it then checks whether any accumulator
 * has any rule that matches this node, and if so invokes the corresponding AccumulatorWatch
 * to update the value of the associated Accumulator
 *
 * <p>The complicating factor (see bug 4430) is that accumulators may invoke each other,
 * and we do not know this statically. We therefore start the processing of a node/event by
 * marking all accumulators unavailable. We then evaluate the accumulators in essentially
 * random order. If one of them invokes another and finds it unavailable, it is then evaluated
 * by a recursive call (with a check for cycles), and removed from the list of accumulators
 * that are unavailable.</p>
 */


public class MultiAccumulatorWatch extends PatternWatch {

    private final Stack<FleetingNode> nodeStack = new Stack<>();
    private final Map<Accumulator, AccumulatorWatch> accumulatorMap;


    public MultiAccumulatorWatch(Map<Accumulator, AccumulatorWatch> accumulatorMap) {
        this.accumulatorMap = accumulatorMap;
        setSelection(new NodeTestPattern(AnyNodeTest.getInstance()));
    }

    public Map<Accumulator, AccumulatorWatch> getAccumulatorMap() {
        return accumulatorMap;
    }

    @Override
    public void open(Terminator terminator) throws XPathException {
        for (AccumulatorWatch watch : accumulatorMap.values()) {
            watch.open(terminator);
            watch.setNodeStack(nodeStack);
        }
    }

    public void setWatchManager(WatchManager watchManager) {
        //
    }

    /**
     * Signal that an element has been found that matches the selection that this
     * Watch is looking for. This method is called by the WatchManager while
     * processing the startElement event that matches the selection. For this purpose,
     * any attributes selected by the last step in the selection are ignored
     *
     * @throws net.sf.saxon.type.ValidationException May be raised if the constraint implemented by this
     *                                               Watch is violated @param locationId the location of the node in the source, or of the instruction that created it
     * @return always null
     */

    @Override
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        //System.err.println("AccumulatorWatch.startSelectedParentNode()");
        nodeStack.push(node);
        for (AccumulatorWatch watch : accumulatorMap.values()) {
            watch.startSelectedParentNode(node, locationId);
        }
        return null;
    }

    /**
     * Signal that a node (other than an element) that matches the selection
     * has been found.
     *
     * @param item the node that was matched.
     */

    @Override
    public void processItem(Item item) throws XPathException {
        //System.err.println("AccumulatorWatch.processItem()");
        FleetingNode node = (FleetingNode) item;
        if (node.getNodeKind() == Type.ATTRIBUTE) {
            return;
        }
        for (AccumulatorWatch watch : accumulatorMap.values()) {
            watch.processItem(node);
        }
    }

    /**
     * Signal that the endElement event has occurred for the element whose startElement
     * event caused the Watch to be activated.
     *
     * @param locationId the location of the parsing event
     * @throws net.sf.saxon.type.ValidationException May be raised if the constraint implemented by this
     *                                               Watch is violated @param locationId the location of the node in the source, or of the instruction that created it
     */

    @Override
    public void endSelectedParentNode(Location locationId) throws XPathException {
        //System.err.println("AccumulatorWatch.endSelectedParentNode()");
        for (AccumulatorWatch watch : accumulatorMap.values()) {
            watch.endSelectedParentNode(locationId);
        }
        nodeStack.pop();
    }

    @Override
    public void close() throws XPathException {
        //
    }


}

// Copyright (c) 2012-2023 Saxonica Limited