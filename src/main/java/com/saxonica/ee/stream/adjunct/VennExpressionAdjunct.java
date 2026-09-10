package com.saxonica.ee.stream.adjunct;


import com.saxonica.ee.stream.*;
import com.saxonica.ee.stream.feed.FilteringFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.feed.NoCloseFeed;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.UnionEnumeration;
import net.sf.saxon.expr.VennExpression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.expr.sort.GlobalOrderComparer;
import net.sf.saxon.expr.sort.LocalOrderComparer;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.pattern.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.NodeListIterator;
import net.sf.saxon.tree.iter.PrependSequenceIterator;

import java.util.LinkedList;
import java.util.List;
import java.util.Stack;

/**
 * Class to handle streaming of union, intersect, and except expressions
 */

public class VennExpressionAdjunct extends TransmissionAdjunct {

    private Inversion lhsInversion;
    private Inversion rhsInversion;

    /**
     * Convert this expression to a streaming pattern (a pattern used internally to match nodes during
     * push processing of an event stream)
     *
     * @param config the Saxon configuration
     * @return the equivalent pattern if conversion succeeds; otherwise null
     */
    @Override
    public Pattern toStreamingPattern(Configuration config) {
        VennExpression exp = (VennExpression) getExpression();
        Expression e0 = exp.getLhsExpression();
        Expression e1 = exp.getRhsExpression();
        int operator = exp.getOperator();

        Pattern p0 = makeStreamingPattern(e0, config);
        if (p0 == null) {
            return null;
        }
        Pattern p1 = makeStreamingPattern(e1, config);
        if (p1 == null) {
            return null;
        }

        if (operator == Token.UNION) {
            return new UnionPattern(p0, p1);
        } else if (operator == Token.EXCEPT) {
            return new ExceptPattern(p0, p1);
        } else {
            return new IntersectPattern(p0, p1);
        }
    }

    private static Pattern makeStreamingPattern(Expression e, Configuration config) {
        if (Streamability.getPostureAndSweepIfKnown(e) != null && Streamability.getPosture(e) == Posture.GROUNDED) {
            return null;
        }
        return Streamability.toStreamingPattern(e, config);
    }


    /**
     * Get the "sweep" of this expression as defined in the W3C streamability specifications.
     * This provides an assessment of stylesheet code against the W3C criteria for guaranteed
     * streamability, and is implemented to allow these criteria to be tested. It is not the
     * case that all expression that emerge as streamable from this analysis are currently
     * capable of being streamed by Saxon
     *
     * @param contextInfo static information about the context item
     * @param reasons     the caller may supply a list, in which case the implementation may add to this
     */
    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        // See 19.8.7.4

        VennExpression exp = (VennExpression) getExpression();

        PostureAndSweep ps0 = Streamability.getStreamability(exp.getLhsExpression(), contextInfo, reasons);
        PostureAndSweep ps1 = Streamability.getStreamability(exp.getRhsExpression(), contextInfo, reasons);

        Posture p0 = ps0.getPosture();
        Posture p1 = ps1.getPosture();

        // Rule 1
        if (ps0.getSweep() == Sweep.FREE_RANGING || ps1.getSweep() == Sweep.FREE_RANGING) {
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }
        // Rule 2
        if (ps0.equals(PostureAndSweep.GROUNDED_AND_MOTIONLESS)) {
            return ps1;
        }
        if (ps1.equals(PostureAndSweep.GROUNDED_AND_MOTIONLESS)) {
            return ps0;
        }

        // Rule 3
        if (p0.equals(Posture.CLIMBING) && p1.equals(Posture.CLIMBING)) {
            return new PostureAndSweep(p0, Streamability.wider(ps0.getSweep(), ps1.getSweep()));
        }

        // Rule 4

        if ((p0 == Posture.STRIDING || p0 == Posture.CRAWLING) && (p1 == Posture.STRIDING || p1 == Posture.CRAWLING)) {
            return new PostureAndSweep(Posture.CRAWLING, Streamability.wider(ps0.getSweep(), ps1.getSweep()));
        }

        // Rule 5
        if (reasons != null) {
            reasons.add("Operands of " + Token.tokens[exp.getOperator()] + " expression on line "
                                + exp.getLocation().getLineNumber() + " have mixed posture (" + p0 + " | " + p1 + ")");
        }

        return PostureAndSweep.ROAMING_AND_FREE_RANGING;

    }


    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        final VennExpression exp = (VennExpression) getExpression();
        Sweep s0 = Streamability.getSweep(exp.getLhsExpression());
        Sweep s1 = Streamability.getSweep(exp.getRhsExpression());

        boolean bothConsuming = s0 == Sweep.CONSUMING && s1 == Sweep.CONSUMING;
        if (bothConsuming) {

            // If both operands are consuming, we typically create a watch that passes its results into
            // a VennTwoWayFeed. The VennTwoWayFeed, when opened, creates a watch for each operand, directing
            // each one to pipe its output into a CombiningUnionFeed (for the union case), which has the
            // responsibility of mergeing the two streams

            if (lhsInversion == null) {
                lhsInversion = Inversion.invertExpression(exp.getLhsExpression(), forGrouping);
                rhsInversion = Inversion.invertExpression(exp.getRhsExpression(), forGrouping);
            }

            return (watchManager, out, context) ->
                new Trigger(AnchorPattern.getInstance(),
                                   new VennTwoWayFeed(exp, this, out, context, watchManager), context);


        } else {
            return super.getWatchMaker(forGrouping);
        }

    }

    /**
     * A LabellingFeed passes events through to its destination feed with an added label, so that
     * the receiving feed can receive inputs from multiple sources and distinguish which item came
     * from which input source. To achieve this it exploits the {@link #append(Item, Location, int)} method
     * which is otherwise unused on streaming feeds.
     *
     * <p>The LabellingFeed also suppresses the close event, because otherwise the recipient would
     * get multiple requests to close.</p>
     */

    private static class LabellingFeed extends NoCloseFeed {

        private final int label;

        /**
         * Create a labelling feed
         * @param resultFeed the destination for events. This will typically be a feed that
         *                   takes input from multiple sources and uses the label to distinguish them
         * @param context  the XPath evaluation context
         * @param label    the label to be used to identify events emanating from this feed
         */
        public LabellingFeed(ItemFeed resultFeed, XPathContext context, int label) {
            super(resultFeed, context);
            this.label = label;
        }

        @Override
        public void append(Item item) throws XPathException {
            // Append an item, adding the label to identify its origin
            getResultFeed().append(item, Loc.NONE, label);
        }
    }

    /**
     * A VennTwoWayFeed suppresses all events except open() and close(). At open() time, it constructs
     * and registers two new watches, one for each operand, directing these to send their output
     * (via a labelling feed) to a combining feed that combines the two streams according to the rules
     * of the union, intersect, or except operator. At close() time, it closes the combining feed.
     */

    private static class VennTwoWayFeed extends ItemFeed {

        private final VennExpressionAdjunct streamer;
        private final WatchManager watchManager;
        private CombiningUnionFeed combiner;

        VennTwoWayFeed(VennExpression exp, VennExpressionAdjunct streamer, ItemFeed result, XPathContext context, WatchManager watchManager) {
            super(exp, result, context);
            this.streamer = streamer;
            this.watchManager = watchManager;
        }

        /**
         * Start evaluating the union expression.
         *
         * @param terminator used to achieve early exit
         * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs
         */
        @Override
        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            VennExpression exp = ((VennExpression) getExpression());
            switch (exp.getOperator()) {
                case Token.UNION:
                    combiner = new CombiningUnionFeed(exp, getResultFeed(), getContext());
                    break;
                default:
                    combiner = null;


            }
            watchManager.addWatch(streamer.lhsInversion.getWatch(
                    watchManager, new LabellingFeed(combiner, getContext(), 0), getContext()), true);
            watchManager.addWatch(streamer.rhsInversion.getWatch(
                    watchManager, new LabellingFeed(combiner, getContext(), 1), getContext()), true);
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
         * for the matched element, and ending with the endElement event for that element; or null if this
         * feature is not required.
         * @throws net.sf.saxon.trans.XPathException May be raised if a dynamic error occurs
         */
        @Override
        public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
            return null;
        }

        /**
         * Signal that the endElement event has occurred for the element whose startElement
         * event caused the Watch to be activated.
         *
         * @param locationId the location of the element
         * @throws XPathException May be raised if a constraint implemented by this
         *                        Watch is violated
         */
        @Override
        public void endSelectedParentNode(Location locationId) throws XPathException {

        }

        /**
         * Supply one item towards the streamed input of the expression
         *
         * @param item the item to be supplied
         */
        @Override
        public void append(Item item) throws XPathException {

        }

        /**
         * Finish evaluating the expression.
         *
         * @throws XPathException if a dynamic error occurs
         */
        @Override
        public void close() throws XPathException {
            combiner.close();
        }
    }

    /**
     * A CombiningUnionFeed accepts events from two input sources, where the events carry an identifying
     * label to indicate which source they came from. It combines events from the two sources into a
     * single output stream according to the rules of the union operator.
     *
     * <p>The complication is that although each of the input streams is individually in document order,
     * an event from one stream may need to be buffered until the other stream catches up. This happens
     * when either or both streams selects nodes using the parent or ancestor axis (which is rare, but
     * needs to be handled). Both streams are therefore buffered, and a node is released from the buffer
     * only when we can be sure than no earlier node can possibly arrive on the other stream.</p>
     */

    private static class CombiningUnionFeed extends ItemFeed {

        private NodeInfo previousNode = null;
        private final Stack<Boolean> duplicates = new Stack<>();
        private final LinkedList<NodeInfo> lhsBuffer = new LinkedList<>();
        private final LinkedList<NodeInfo> rhsBuffer = new LinkedList<>();

        CombiningUnionFeed(VennExpression exp, ItemFeed result, XPathContext context) {
            super(exp, result, context);
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
         * for the matched element, and ending with the endElement event for that element; or null if this
         * feature is not required.
         * @throws net.sf.saxon.trans.XPathException May be raised if a dynamic error occurs
         */
        @Override
        public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
            //System.err.println("CombiningUnionFeed.startSelectedParent " + node.getDisplayName());
            boolean duplicate = previousNode != null && node.isSameNodeInfo(previousNode);
            if (!duplicate) {
                getResultFeed().startSelectedParentNode(node, locationId);
                previousNode = node;
            }
            duplicates.push(duplicate);
            return null;
        }

        /**
         * Signal that the endElement event has occurred for the element whose startElement
         * event caused the Watch to be activated.
         *
         * @param locationId the location of the element
         * @throws XPathException May be raised if a constraint implemented by this
         *                        Watch is violated
         */
        @Override
        public void endSelectedParentNode(Location locationId) throws XPathException {
            //System.err.println("CombiningUnionFeed.endSelectedParent ");
            boolean duplicate = duplicates.pop();
            if (!duplicate) {
                getResultFeed().endSelectedParentNode(locationId);
            }
        }

        /**
         * Supply one item towards the streamed input of the expression
         *
         * @param item the item to be supplied
         */
        @Override
        public void append(Item item) throws XPathException {
            throw new UnsupportedOperationException();
        }

        @Override
        public void append(Item item, Location loc, int label) throws XPathException {
            // The principle here is that we can flush a node from buffer A if it it is earlier
            // in document order than the last node in buffer B.
//            System.err.println("CombiningUnionFeed.appendItem " + ((NodeInfo) item).getDisplayName()
//                                       + " " + genId((NodeInfo) item) + " label=" + label);
            LinkedList<NodeInfo> thisBuffer = label == 0 ? lhsBuffer : rhsBuffer;
            LinkedList<NodeInfo> thatBuffer = label == 0 ? rhsBuffer : lhsBuffer;

            while (!thatBuffer.isEmpty()) {
                NodeInfo next = thatBuffer.peekFirst();
                int c = next.compareOrder((NodeInfo)item);
                if (c < 0) {
                    getResultFeed().append(thatBuffer.pollFirst());
                } else if (c == 0) {
                    thatBuffer.pollFirst();
                } else {
                    break;
                }
            }

            thisBuffer.add((NodeInfo)item);

        }

        /**
         * Finish evaluating the expression.
         *
         * @throws XPathException if a dynamic error occurs
         */
        @Override
        public void close() throws XPathException {
            //System.err.println("CombiningUnionFeed.close() ");
            // Flush out any remaining buffered nodes, using the pull-mode union iterator to merge them
            NodeListIterator iter0 = new NodeListIterator(lhsBuffer);
            NodeListIterator iter1 = new NodeListIterator(rhsBuffer);
            UnionEnumeration union = new UnionEnumeration(iter0, iter1, LocalOrderComparer.getInstance());
            processItems(union, getResultFeed());
            super.close();
        }

    }

    @Override
    public ItemFeed makeItemFeed(final WatchManager watchManager, ItemFeed out, final XPathContext context) throws XPathException {
        final VennExpression exp = (VennExpression) getExpression();

        Sweep s0 = Streamability.getSweep(exp.getLhsExpression());
        Sweep s1 = Streamability.getSweep(exp.getRhsExpression());

        boolean bothConsuming = s0 == Sweep.CONSUMING && s1 == Sweep.CONSUMING;
        boolean a0consuming = s0 == Sweep.CONSUMING;

        int operator = exp.getOperator();
        if (operator == Token.UNION) {
            if (bothConsuming) {
                if (lhsInversion == null) {
                    lhsInversion = Inversion.invertExpression(exp.getLhsExpression(), false);
                    rhsInversion = Inversion.invertExpression(exp.getRhsExpression(), false);
                }
                return new VennTwoWayFeed(exp, this, out, context, watchManager);
            } else if (a0consuming) {
                return new SemiStreamedUnionFeed(exp, exp.getRhsExpression(), out, context);
            } else {
                return new SemiStreamedUnionFeed(exp, exp.getLhsExpression(), out, context);
            }
        } else if (operator == Token.INTERSECT) {
            return new FilteringFeed(watchManager, out, context, FilteringFeed.OpaqueFilter);

        } else if (operator == Token.EXCEPT) {
            if (a0consuming) {
                // If the LHS expression is streamed, we deliver its contents and ignore the RHS
                // expression, which cannot deliver any streamed nodes
                return new FilteringFeed(watchManager, out, context, FilteringFeed.TransparentFilter);
            } else {
                // If the RHS expression is streamed, we deliver the contents of the LHS operand, and ignore the RHS
                // expression, which cannot deliver any unstreamed nodes
                return new FirstOperandOnlyFeed(watchManager, out, context, exp.getLhsExpression());
//                return new FilteringFeed(out, context, FilteringFeed.OpaqueFilter) {
//                    @Override
//                    public void open(Terminator terminator) throws XPathException {
//                        super.open(terminator);
//                        processItems(exp.getLhsExpression().iterate(getContext()), getNextOutputter());
//                    }
//                };
            }
        } else {
            throw new UnsupportedOperationException();
        }

    }

    /**
     * This item feed delivers all the results from the first (unstreamed) operand, and none of the
     * results from the second (streamed) operand. This is on the basis that it's not possible for
     * any nodes to be in both results.
     */
    private static class FirstOperandOnlyFeed extends FilteringFeed {

        Expression lhs;

        public FirstOperandOnlyFeed(WatchManager watchManager, ItemFeed out, XPathContext context, Expression lhs) {
            super(watchManager, out, context, FilteringFeed.OpaqueFilter);
            this.lhs = lhs;
        }

        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            processItems(lhs.iterate(getContext()), getNextOutputter());
        }
    }

    /**
     * Feed for a union expression in which one of the arguments (but not both) is streamed.
     */

    private static class SemiStreamedUnionFeed extends ItemFeed {

        Expression nonConsumingOperand;
        SequenceIterator nonConsumingIterator;
        boolean atStart = true;
        boolean emitNonConsumingOperandAtEnd = true;

        public SemiStreamedUnionFeed(Expression exp, Expression nonConsumingOperand, ItemFeed result, XPathContext context) {
            super(exp, result, context);
            this.nonConsumingOperand = nonConsumingOperand;

        }

        /**
         * Open the watch. This is done at the point where it first starts watching for nodes
         * that match the selection.
         *
         * @param terminator used for early exit
         */

        @Override
        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            nonConsumingIterator = nonConsumingOperand.iterate(getContext());
        }

        /**
         * The close() method is called immediately before the Watch is destroyed, that is,
         * when the element whose declaration scopes the constraint implemented by this
         * Watch goes out of scope. The default implementation does nothing.
         */

        @Override
        public void close() throws XPathException {
            if (emitNonConsumingOperandAtEnd) {
                processItems(nonConsumingIterator, getNextOutputter());
            }
            getNextOutputter().close();
        }

        /**
         * Signal that a node (other than a document or element node) that matches the selection
         * has been found, or that a grounded item is being passed to the next expression in the
         * evaluation pipeline.
         *
         * @param item the node that was matched.
         * @throws net.sf.saxon.trans.XPathException May be raised if a dynamic error occurs
         */
        @Override
        public void append(Item item) throws XPathException {
            if (atStart) {
                NodeInfo first = (NodeInfo) nonConsumingIterator.next();
                if (first != null) {
                    if (GlobalOrderComparer.getInstance().compare((NodeInfo) item, first) < 0) {
                        nonConsumingIterator = new PrependSequenceIterator(first, nonConsumingIterator);
                        emitNonConsumingOperandAtEnd = true;
                    } else {
                        getNextOutputter().append(first);
                        processItems(nonConsumingIterator, getNextOutputter());
                        emitNonConsumingOperandAtEnd = false;
                    }
                } else {
                    emitNonConsumingOperandAtEnd = false;
                }
            }
            atStart = false;
            getNextOutputter().append(item);
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
         * for the matched element, and ending with the endElement event for that element; or null if this
         * feature is not required.
         * @throws net.sf.saxon.trans.XPathException May be raised if a dynamic error occurs
         */
        @Override
        public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
            if (atStart) {
                NodeInfo first = (NodeInfo) nonConsumingIterator.next();
                if (first != null) {
                    if (GlobalOrderComparer.getInstance().compare(node, first) < 0) {
                        nonConsumingIterator = new PrependSequenceIterator(first, nonConsumingIterator);
                        emitNonConsumingOperandAtEnd = true;
                    } else {
                        getNextOutputter().append(first);
                        processItems(nonConsumingIterator, getNextOutputter());
                        emitNonConsumingOperandAtEnd = false;
                    }
                } else {
                    emitNonConsumingOperandAtEnd = false;
                }
            }
            atStart = false;
            return getResultFeed().startSelectedParentNode(node, locationId);
        }

        /**
         * Signal that the endElement event has occurred for the element whose startElement
         * event caused the Watch to be activated.
         *
         * @param locationId the location of the element
         * @throws net.sf.saxon.trans.XPathException May be raised if a constraint implemented by this
         *                                           Watch is violated
         */
        @Override
        public void endSelectedParentNode(Location locationId) throws XPathException {
            getResultFeed().endSelectedParentNode(locationId);
        }
    }


}

// Copyright (c) 2018-2022 Saxonica Limited