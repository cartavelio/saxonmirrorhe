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
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Affinity;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.TypeHierarchy;

import java.util.ArrayList;
import java.util.List;


/**
 * Handle streaming for calls to user-defined functions with streamability="absorbing"
 */

public class DeepDescentFunctionCallAdjunct extends TransmissionAdjunct {

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        if (reasons == null) {
            reasons = new ArrayList<>();
        }

        UserFunctionCall exp = (UserFunctionCall)getExpression();
        UserFunction target = exp.getFunction();


        Operand[] operanda = exp.getOperanda().copy();
        operanda[0].setUsage(OperandUsage.TRANSMISSION);
        for (int i = 1; i < operanda.length; i++) {
            operanda[i].setUsage(Operand.typeDeterminedUsage(target.getArgumentType(i).getPrimaryType()));
        }

        // Get the type-adjusted posture and sweep of the first argument expression.
        // This works because we have already type-adjusted the expression in the type-checking phase
        PostureAndSweep ps0 = Streamability.getStreamability(exp.getArg(0), contextInfo, reasons);
        Posture p0 = ps0.getPosture();
        if (p0 != Posture.STRIDING && p0 != Posture.GROUNDED) {
            reasons.add("Posture of first argument to a deep-descent function must be striding or grounded");
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }
        // For the moment, require other arguments to be grounded and motionless
        // TODO: check the spec here
        for (int a = 1; a < exp.getArity(); a++) {
            Expression arg = exp.getArg(a);
            if (Streamability.getStreamability(arg, contextInfo, reasons) != PostureAndSweep.GROUNDED_AND_MOTIONLESS) {
                reasons.add("Posture of the second and subsequent arguments to a deep-descent function must (for now) be grounded and motionless");
                return PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
        }
        if (p0 == Posture.GROUNDED) {
            return new PostureAndSweep(Posture.GROUNDED, ps0.getSweep());
        } else {
            PostureAndSweep bodyPS = Streamability.getStreamability(target.getBody(), contextInfo, reasons);
            if (bodyPS.getPosture() != Posture.CRAWLING) {
                reasons.add("The body of the deep-descent function must have crawling posture");
                return PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            if (bodyPS.getSweep() == Sweep.FREE_RANGING) {
                reasons.add("The body of the deep-descent function must have motionless or consuming sweep");
                return PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            TypeHierarchy th = getConfiguration().getTypeHierarchy();
            ItemType declared = target.getArgumentType(0).getPrimaryType();
            boolean declaredTypeAllowsParentNodes =
                    th.relationship(declared, NodeKindTest.DOCUMENT) != Affinity.DISJOINT ||
                            th.relationship(declared, NodeKindTest.ELEMENT) != Affinity.DISJOINT;
            ItemType supplied = exp.getArg(0).getItemType();
            boolean suppliedTypeAllowsParentNodes =
                    th.relationship(supplied, NodeKindTest.DOCUMENT) != Affinity.DISJOINT ||
                            th.relationship(supplied, NodeKindTest.ELEMENT) != Affinity.DISJOINT;
            if (declaredTypeAllowsParentNodes && suppliedTypeAllowsParentNodes) {
                return new PostureAndSweep(p0, Sweep.CONSUMING);
            } else {
                return ps0;
            }
        }
    }

    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        final UserFunctionCall instruction = (UserFunctionCall) getExpression();
        final Expression select = instruction.getArg(0);

        final Pattern selection = StreamingPatternMaker.makeStreamingPattern(select, getConfiguration(), null);

        if (selection != null) {
            return (watchManager, out, context) -> {
                DeepDescentFunctionCallAction action = new DeepDescentFunctionCallAction(getExpression(), out, context);
                action.setWatchManager(watchManager);
                return new Trigger(selection, action, context);
            };
        }
        return null;
    }

    private static class DeepDescentFunctionCallAction extends ItemFeed {

        private XPathContextMajor localContext;
        private WatchManager watchManager;
        private StreamableUserFunction targetFunction;
        private Watch innerWatch;
        private boolean started;

        DeepDescentFunctionCallAction(Expression functionCall, ItemFeed out, XPathContext context) {
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

            start();

            innerWatch.setAnchorNode(node);
            watchManager.addWatch(innerWatch, false);

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
            watchManager.removeWatch(innerWatch);
        }

        @Override
        public void close() throws XPathException {
            start();
            innerWatch.close();
            super.close();
        }


    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, final ItemFeed out, final XPathContext context) throws XPathException {
        DeepDescentFunctionCallAction action = new DeepDescentFunctionCallAction(getExpression(), out, context);
        action.setWatchManager(watchManager);
        return action;
    }


}

// Copyright (c) 2018-2023 Saxonica Limited