package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.*;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.*;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.UserFunction;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;

import java.util.ArrayList;
import java.util.List;


/**
 * Handle streaming for calls to user-defined functions with streamability="absorbing"
 */

public class AbsorbingFunctionCallAdjunct extends TransmissionAdjunct {

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        if (reasons == null) {
            reasons = new ArrayList<String>();
        }

        UserFunctionCall exp = (UserFunctionCall)getExpression();
        UserFunction target = exp.getFunction();


        Operand[] operanda = exp.getOperanda().copy();
        operanda[0].setUsage(OperandUsage.TRANSMISSION);
        for (int i = 1; i < operanda.length; i++) {
            operanda[i].setUsage(Operand.typeDeterminedUsage(target.getArgumentType(i).getPrimaryType()));
        }

        PostureAndSweep first = Streamability.getStreamability(exp.getArg(0), contextInfo, reasons);
        if (first.getPosture() == Posture.CRAWLING) {
            reasons.add("The first argument to a function with streamability=absorbing must not have crawling posture");
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        } else {
            operanda[0].setUsage(OperandUsage.ABSORPTION);
            return Streamability.generalStreamabilityRules(exp, exp.operands(), contextInfo, reasons);
        }

    }

    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        final UserFunctionCall instruction = (UserFunctionCall) getExpression();
        final Expression select = instruction.getArg(0);

        final Pattern selection = StreamingPatternMaker.makeStreamingPattern(select, getConfiguration(), null);

        if (selection != null) {
            return (watchManager, out, context) -> {
                AbsorbingFunctionCallAction action = new AbsorbingFunctionCallAction(getExpression(), out, context);
                action.setWatchManager(watchManager);
                return new Trigger(selection, action, context);
            };
        }
        return null;
    }

    private static class AbsorbingFunctionCallAction extends ItemFeed {

        private XPathContextMajor localContext;
        private WatchManager watchManager;
        private StreamableUserFunction targetFunction;
        private Watch innerWatch;
        private boolean started;

        public AbsorbingFunctionCallAction(Expression functionCall, ItemFeed out, XPathContext context) {
            super(functionCall, out, context);
        }

        @Override
        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            UserFunctionCall exp = (UserFunctionCall) getExpression();
            Component target = exp.getTargetComponent(getContext());
            targetFunction = (StreamableUserFunction)target.getActor();
            targetFunction.makeInversion(); // actually happens on first call only
            localContext = getContext().newContext();
            localContext.setCurrentComponent(target);

            Sequence[] args = exp.evaluateArguments(getContext(), true);
            localContext.setStackFrame(targetFunction.getStackFrameMap(), args);
            started = false;

        }


        public void setWatchManager(WatchManager watchManager) {
            this.watchManager = watchManager;
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
         * @throws XPathException May be raised if a dynamic error occurs
         */

        @Override
        public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {

            XPathContext context = localContext;
            context.setLocalVariable(0, node);

            boolean first = !started;
            start();

            innerWatch.setAnchorNode(node);
            if (first) {
                watchManager.addWatch(innerWatch, false);
            }

            return null;
        }


        private void start() throws XPathException {
            if (!started) {
                started = true;
                ComponentInversion inversion = targetFunction.getInversion();
                innerWatch = inversion.getWatch(watchManager, getResultFeed(), localContext);
                innerWatch.open(new Terminator());
            }
        }


        /**
         * Signal that a item (other than a streamed element) that matches the selection
         * has been found.
         *
         * @param item the item that was matched.
         */

        @Override
        public void append(Item item) throws XPathException {
            if (!hasFailed()) {
                XPathContext context = localContext;
                context.setLocalVariable(0, item);

                start();

                ((Trigger) innerWatch).getAction().append(item);
            }
        }

        /**
         * Signal that the endElement event has occurred for the element whose startElement
         * event caused the Watch to be activated.
         *
         * @param locationId the location of the node in the source, or of the instruction that created it
         * @throws net.sf.saxon.type.ValidationException May be raised if the constraint implemented by this
         *                                               Watch is violated @param locationId the location of the node in the source, or of the instruction that created it
         */

        @Override
        public void endSelectedParentNode(Location locationId) throws XPathException {
            //watchManager.removeWatch(innerWatch);
        }

        @Override
        public void close() throws XPathException {
            start();
            innerWatch.close();
            watchManager.removeWatch(innerWatch);
            //super.close();
        }


    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, final ItemFeed out, final XPathContext context) throws XPathException {
        AbsorbingFunctionCallAction feed = new AbsorbingFunctionCallAction(getExpression(), out, context);
        feed.setWatchManager(watchManager);
        return feed;
    }


}

// Copyright (c) 2018-2022 Saxonica Limited