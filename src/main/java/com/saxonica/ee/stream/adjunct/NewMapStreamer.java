package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.*;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.Block;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapFunctionSet;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.Item;
import net.sf.saxon.pattern.AnchorPattern;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;

import java.util.List;


/**
 * Streaming code to implement an xsl:map instruction or map constructor, where each
 * map entry is allowed to be consuming. Based on the code of ForkAdjunct
 */
public class NewMapStreamer extends StreamingAdjunct implements FeedMaker {

    private Inversion[] actionInversions;

    /**
     * Compute the posture and sweep of this expression as defined in the W3C streamability specifications.
     * This must be implemented in all subclasses that don't use the general streamability rules.
     *
     * @param contextInfo Information about the context item type and posture
     * @param reasons     the caller may supply a list, in which case the implementation may add to this
     */
    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        SystemFunctionCall exp = (SystemFunctionCall) getExpression();
        Expression block = exp.getArg(0);
        if (!(block instanceof Block)) {
            return super.computeStreamability(contextInfo, reasons);
        }
        // xsl:map and map constructors allow multiple consuming map entries
        Operand[] children = ((Block) block).getOperanda();
        if (children.length == 0) {
            return PostureAndSweep.GROUNDED_AND_MOTIONLESS;
        } else {
            Sweep sweep = Sweep.MOTIONLESS;
            for (Operand o : children) {
                Expression e = o.getChildExpression();
                if (!e.isCallOn(MapFunctionSet.MapEntry.class)) {
                    return super.computeStreamability(contextInfo, reasons);
                }
                PostureAndSweep ps = Streamability.getStreamability(e, contextInfo, reasons);
                if (ps.getPosture() == Posture.ROAMING || ps.getSweep() == Sweep.FREE_RANGING) {
                    Streamability.setPostureAndSweep(block, PostureAndSweep.ROAMING_AND_FREE_RANGING);
                    return PostureAndSweep.ROAMING_AND_FREE_RANGING;
                } else {
                    sweep = Sweep.wider(sweep, ps.getSweep());
                }
            }
            PostureAndSweep ps = new PostureAndSweep(Posture.GROUNDED, sweep);
            Streamability.setPostureAndSweep(block, ps);
            return ps;
        }
    }

    private Operand[] getChildren() {
        SystemFunctionCall exp = (SystemFunctionCall) getExpression();
        Expression arg = exp.getArg(0);
        return ((Block) arg).getOperanda();
    }


    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        SystemFunctionCall exp = (SystemFunctionCall) getExpression();
        Expression block = exp.getArg(0);
        if (!(block instanceof Block)) {
            return super.getWatchMaker(forGrouping);
        }
        final Operand[] actions = getChildren();
        if (actionInversions == null) {
            actionInversions = new Inversion[actions.length];
            for (int i = 0; i < actions.length; i++) {
                Expression action = actions[i].getChildExpression();
                if (Streamability.getSweep(action) == Sweep.CONSUMING) {
                    actionInversions[i] = Inversion.invertExpression(action, forGrouping);
                } else {
                    actionInversions[i] = null;
                }
            }
        }

        return (watchManager, out, context) ->
                new Trigger(AnchorPattern.getInstance(),
                            new NewMapForkingFeed(getExpression(), out, context, watchManager), context);

    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new NewMapSimpleFeed(getExpression(), out, context);
    }

    /**
     * A map forking feed handles the special case where multiple map entries can be created in parallel,
     * each being a consuming construct.
     */

    private class NewMapForkingFeed extends ItemFeed {

        private final WatchManager watchManager;
        private HashTrieMap resultMap;

        NewMapForkingFeed(Expression exp, ItemFeed result, XPathContext context, WatchManager watchManager) {
            super(exp, result, context);
            this.watchManager = watchManager;
        }

        /**
         * Start evaluating the map{} expression.
         *
         * @param terminator used to achieve early exit
         * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs
         */
        @Override
        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            resultMap = new HashTrieMap();
            final Operand[] actions = getChildren();
            for (int i = 0; i < actions.length; i++) {
                Expression action = actions[i].getChildExpression();
                if (Streamability.getSweep(action) == Sweep.CONSUMING) {
                    ItemFeed feed = new ItemFeed(getResult(), getContext()) {

                        // This feed handles a single entry in the map{} being constructed

                        @Override
                        public void open(Terminator terminator) {
                            // No action here
                        }

                        @Override
                        public void append(Item item) throws XPathException {
                            for (KeyValuePair pair : ((MapItem) item).keyValuePairs()) {
                                // there will only be one key/value pair, created by map:entry
                                if (resultMap.get(pair.key) != null) {
                                    XPathException err = new XPathException(
                                            "Duplicate key value in map: " + Err.depict(pair.key), "XTDE3365");
                                    dynamicError(err);
                                } else {
                                    resultMap = resultMap.addEntry(pair.key, pair.value);
                                }
                            }
                        }

                        @Override
                        public void close() {
                            // No action here;
                        }

                        @Override
                        public void setHasFailed() {
                            NewMapForkingFeed.this.setHasFailed();
                        }
                    };
//                    if (Streamability.getPosture(action) != Posture.GROUNDED) {
//                        // test si-fork-006
//                        feed = new CopyOfFeed(watchManager, feed, getContext());
//                    }
                    watchManager.addWatch(actionInversions[i].getWatch(watchManager, feed, getContext()), true);
                } else {
                    MapItem sm = (MapItem) action.evaluateItem(getContext());
                    for (KeyValuePair pair : sm.keyValuePairs()) {
                        // there will only be one key/value pair, created by map:entry
                        if (resultMap.get(pair.key) != null) {
                            XPathException err = new XPathException(
                                    "Duplicate key value in map: " + Err.depict(pair.key), "XTDE3365");
                            dynamicError(err);
                        } else {
                            resultMap = resultMap.addEntry(pair.key, pair.value);
                        }
                    }
                }
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
         * Finish evaluating the expression.
         *
         * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs
         */
        @Override
        public void close() throws XPathException {
            if (!hasFailed()) {
                Outputter out = getNextOutputter();
                out.append(resultMap);
                out.close();
            }
            resultMap = null;
        }

    }

    /**
     * A simple feed handles the case where there is only one consuming subexpression, often
     * a for-each loop of some kind delivering multiple map entries for nodes in document order.
     */

    private class NewMapSimpleFeed extends ItemFeed {

        HashTrieMap resultMap;

        NewMapSimpleFeed(Expression exp, ItemFeed result, XPathContext context) {
            super(exp, result, context);
        }


        /**
         * Start evaluating the expression. The default implementation does nothing.
         *
         * @param terminator used to achieve early exit
         * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs
         */
        @Override
        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            resultMap = new HashTrieMap();
        }

        /**
         * Supply one item towards the streamed input of the expression
         *
         * @param item the item to be supplied
         */
        @Override
        public void append(Item item) throws XPathException {
            if (item instanceof MapItem) {
                for (KeyValuePair pair : ((MapItem) item).keyValuePairs()) {
                    boolean existing = resultMap.initialPut(pair.key, pair.value);
                    if (existing) {
                        XPathException err = new XPathException("Duplicate key value in map: " + Err.depict(pair.key), "XTDE3365");
                        dynamicError(err);
                    }
                }
            } else {
                // Should be caught by type-checking?
                XPathException err = new XPathException("Argument to map constructor contains an item that is not a map", "XPTY0004");
                err.setIsTypeError(true);
                dynamicError(err);
            }

        }

        /**
         * Finish evaluating the expression. The default implementation does nothing.
         *
         * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs
         */
        @Override
        public void close() throws XPathException {
            //System.err.println("NMA close() " + toString());
            if (!hasFailed()) {
                Outputter out = getNextOutputter();
                out.append(resultMap);
                out.close();
            }
            resultMap = null;
        }

    }
}

// Copyright Saxonica 2014-2017.