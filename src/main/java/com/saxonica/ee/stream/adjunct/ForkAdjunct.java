package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.*;
import com.saxonica.ee.stream.feed.CopyOfFeed;
import com.saxonica.ee.stream.feed.DecomposingFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.Fork;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.AnchorPattern;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;

import java.util.List;


/**
 * Streaming code to implement an xsl:fork expression
 *
 * <p>The general strategy is to create an inversion of the expression in each prong of the fork, and
 * to create a separate watch for each one. These watches create output which is buffered, and on completion
 * the outputs of each watch are assembled in the right order.</p>
 *
 * <p>The implementation is NOT multithreaded: although the prongs of the fork execute conceptually
 * in parallel, in that they all act as listeners responding to the same sequence of events, they
 * actually all run in the same thread.</p>
 */
public class ForkAdjunct extends StreamingAdjunct {

    private Inversion[] actionInversions;

    /**
     * Compute the posture and sweep of this expression as defined in the W3C streamability specifications.
     * This must be implemented in all subclasses that don't use the general streamability rules.
     * @param contextInfo     Information about the context item type and posture
     * @param reasons         the caller may supply a list, in which case the implementation may add to this
     */
    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        Fork exp = (Fork) getExpression();
        if (exp.getSize() == 0) {
            return PostureAndSweep.GROUNDED_AND_MOTIONLESS;
        } else {
            PostureAndSweep eccentric = null;
            for (Operand o : exp.operands()) {
                PostureAndSweep ps = Streamability.getStreamability(o.getChildExpression(), contextInfo, reasons);
                if (ps.getPosture() == Posture.ROAMING || ps.getSweep() == Sweep.FREE_RANGING) {
                    return PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }

                if (ps.getPosture() != Posture.GROUNDED && ps.getSweep() != Sweep.MOTIONLESS) {
                    if (eccentric != null) {
                        reasons.add("At most one branch of the xsl:fork may return streamed nodes");
                        return PostureAndSweep.ROAMING_AND_FREE_RANGING;
                        //eccentric = null;
                    } else {
                        eccentric = ps;
                    }
                } else if (ps.getPosture() != Posture.GROUNDED) {
                    return PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }
            }
            if (eccentric != null) {
                return eccentric;
            } else {
                return new PostureAndSweep(Posture.GROUNDED, Sweep.CONSUMING);
            }
        }
    }


    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        final Fork fork = (Fork) getExpression();
        if (actionInversions == null) {
            actionInversions = new Inversion[fork.getSize()];
            int i = 0;
            for (Operand o : fork.operands()) {
                if (Streamability.getSweep(o.getChildExpression()) == Sweep.CONSUMING) {
                    actionInversions[i] = Inversion.invertExpression(o.getChildExpression(), forGrouping);
                } else {
                    actionInversions[i] = null;
                }
                i++;
            }
        }

        return (watchManager, out, context) ->
                new Trigger(AnchorPattern.getInstance(),
                            new ForkFeed(getExpression(), actionInversions, out, context, watchManager), context);

    }

    private static class ForkFeed extends ItemFeed {

        private WatchManager watchManager;
        private FeedToEventBuffer[] writers;
        private Inversion[] actionInversions;

        public ForkFeed(Expression exp, Inversion[] actionInversions, ItemFeed result,
                        XPathContext context, WatchManager watchManager) {
            super(exp, result, context);
            this.actionInversions = actionInversions;
            this.watchManager = watchManager;
        }


        /**
         * Start evaluating the expression. The default implementation does nothing.
         *
         * @param terminator callback for use on early exit
         * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs
         */
        @Override
        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            final Fork fork = (Fork) getExpression();
            writers = new FeedToEventBuffer[fork.getSize()];
            int i = 0;
            for (Operand o : fork.operands()) {
                // TODO: PERF: no need to buffer the result of the first prong
                final FeedToEventBuffer writer = new FeedToEventBuffer(getNextOutputter(), this, getContext());
                writer.setPipelineConfiguration(getPipelineConfiguration());
                writers[i] = writer;
                if (Streamability.getSweep(o.getChildExpression()) == Sweep.CONSUMING) {
                    ItemFeed feed = new DecomposingFeed(writer, getContext());
                    if (Streamability.getPosture(o.getChildExpression()) != Posture.GROUNDED) {
                        // test si-fork-006
                        feed = new CopyOfFeed(watchManager, feed, getContext());
                    }
                    watchManager.addWatch(actionInversions[i].getWatch(watchManager, feed, getContext()), true);
                } else {
                    o.getChildExpression().process(writer, getContext());
                }
                i++;
            }
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
         * Supply one item towards the streamed input of the expression
         *
         * @param item the item to be supplied
         */
        @Override
        public void append(Item item) throws XPathException {
            throw new UnsupportedOperationException();
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
            // No further action needed: the events are handled by the feeds for the individual prongs
        }

        /**
         * Finish evaluating the expression. The default implementation does nothing.
         *
         * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs
         */
        @Override
        public void close() throws XPathException {
            if (!hasFailed()) {
                Outputter out = getNextOutputter();
                for (FeedToEventBuffer writer : writers) {
                    writer.replay(out);
                }
                out.close();
            }
            writers = null;
        }

    }

}

// Copyright (c) 2014-2023 Saxonica Limited.