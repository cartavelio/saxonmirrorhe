package com.saxonica.ee.stream.adjunct;


import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.feed.FilteringFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.VennExpression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.expr.sort.GlobalOrderComparer;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.pattern.ExceptPattern;
import net.sf.saxon.pattern.IntersectPattern;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.pattern.UnionPattern;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.PrependSequenceIterator;

import java.util.List;

public class VennExpressionAdjunct extends TransmissionAdjunct {

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
        if (ps0.equals(PostureAndSweep.CLIMBING_AND_MOTIONLESS) && ps1.equals(PostureAndSweep.CLIMBING_AND_MOTIONLESS)) {
            return ps0;
        }

        // Rule 4
        Posture p0 = ps0.getPosture();
        Posture p1 = ps1.getPosture();
        if ((p0 == Posture.STRIDING || p0 == Posture.CRAWLING) && (p1 == Posture.STRIDING || p1 == Posture.CRAWLING)) {
            return new PostureAndSweep(Posture.CRAWLING, Streamability.wider(ps0.getSweep(), ps1.getSweep()));
        }

        // Rule 5
        if (reasons != null) {
            reasons.add("Operands of " + Token.tokens[exp.getOperator()] + " expression on line "
                                + exp.getLocation().getLineNumber() + " have mixed posture");
        }

        return PostureAndSweep.ROAMING_AND_FREE_RANGING;

    }

    @Override
    public ItemFeed makeItemFeed(final WatchManager watchManager, ItemFeed out, final XPathContext context) throws XPathException {
        final VennExpression exp = (VennExpression) getExpression();

        Sweep s0 = Streamability.getSweep(exp.getLhsExpression());
        Sweep s1 = Streamability.getSweep(exp.getRhsExpression());

        boolean a0consuming = s0 == Sweep.CONSUMING;

        int operator = exp.getOperator();
        if (operator == Token.UNION) {
            if (a0consuming) {
                return new UnionFeed(exp, exp.getRhsExpression(), out, context);
            } else {
                return new UnionFeed(exp, exp.getLhsExpression(), out, context);
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
     * Feed for a union expression. Either of the arguments (but not both) can be consuming.
     */

    private static class UnionFeed extends ItemFeed {

        Expression nonConsumingOperand;
        SequenceIterator nonConsumingIterator;
        boolean atStart = true;
        boolean emitNonConsumingOperandAtEnd = true;

        public UnionFeed(Expression exp, Expression nonConsumingOperand, ItemFeed result, XPathContext context) {
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