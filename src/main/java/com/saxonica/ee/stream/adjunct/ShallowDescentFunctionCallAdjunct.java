package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.*;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.*;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.Block;
import net.sf.saxon.expr.instruct.UserFunction;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.UType;

import java.util.ArrayList;
import java.util.List;


/**
 * Handle streaming for calls to user-defined functions with streamability="shallow-descent"
 */

public class ShallowDescentFunctionCallAdjunct extends TransmissionAdjunct {

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        if (reasons == null) {
            reasons = new ArrayList<>();
        }

        UserFunctionCall exp = (UserFunctionCall)getExpression();
        UserFunction target = exp.getFunction();

        // Get the type-adjusted posture and sweep of the first argument expression.
        // This works because we have already type-adjusted the expression in the type-checking phase

        // Rule 2
        PostureAndSweep ps0 = Streamability.getStreamability(exp.getArg(0), contextInfo, reasons);

        // Rule 3
        Posture p0 = ps0.getPosture();
        if (p0 != Posture.STRIDING && p0 != Posture.GROUNDED) {
            reasons.add("Posture of first argument to a shallow-descent function must be striding or grounded");
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }

        // Rule 4
        PostureAndSweep ps1 = PostureAndSweep.GROUNDED_AND_MOTIONLESS;
        if (exp.getOperanda().getNumberOfOperands() > 1) {
            RebindingMap rebindings = new RebindingMap();
            Expression[] children = new Expression[exp.getOperanda().getNumberOfOperands() - 1];
            boolean interesting = false;
            for (int i = 0; i < children.length; i++) {
                Expression arg = exp.getArg(i + 1);
                PostureAndSweep ps = Streamability.getStreamability(arg, contextInfo, reasons);
                if (!ps.equals(PostureAndSweep.GROUNDED_AND_MOTIONLESS)) {
                    interesting = true;
                }
                children[i] = arg.copy(rebindings);
            }
            if (interesting) {
                // Skip this (for performance) if all remaining arguments are grounded and motionless
                Block block = new Block(children);
                List<Operand> operands = new ArrayList<>(children.length);
                for (int i = 0; i < children.length; i++) {
                    OperandRole original = exp.getOperanda().getOperand(i + 1).getOperandRole();
                    OperandUsage usage = Operand.typeDeterminedUsage(target.getArgumentType(i + 1).getPrimaryType());
                    OperandRole role = new OperandRole(original.getProperties(), usage);
                    Operand o2 = new Operand(children[i], block, role);
                    operands.add(o2);
                }
                ps1 = Streamability.generalStreamabilityRules(block, operands, contextInfo, reasons);
            }
        }

        // Rule 5
        if (ps1.getPosture() != Posture.GROUNDED) {
            reasons.add("Arguments after the first must be grounded");
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }

        // Rule 6
        if (ps0.getSweep() == Sweep.CONSUMING && ps1.getSweep() == Sweep.CONSUMING) {
            reasons.add("More than one argument is consuming");
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }

        if (ps0.getSweep() == Sweep.FREE_RANGING || ps1.getSweep() == Sweep.FREE_RANGING) {
            reasons.add("All arguments must be streamable");
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }

        // Rule 7
        if (p0 == Posture.GROUNDED) {
            return new PostureAndSweep(Posture.GROUNDED, Streamability.wider(ps0.getSweep(), ps1.getSweep()));
        }

        // Rule 8a
        if (target.getArgumentType(0).getPrimaryType().getUType().intersection(
                UType.DOCUMENT.union(UType.ELEMENT)).equals(UType.VOID)) {
            return new PostureAndSweep(p0, ps0.getSweep());
        }

        // Rule 8b
        if (exp.getArg(0).getItemType().getUType().intersection(
                UType.DOCUMENT.union(UType.ELEMENT)).equals(UType.VOID)) {
            return new PostureAndSweep(p0, ps0.getSweep());
        }

        // Rule 8c
        return new PostureAndSweep(p0, Sweep.CONSUMING);

    }

    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        final UserFunctionCall instruction = (UserFunctionCall) getExpression();
        final Expression select = instruction.getArg(0);

        final Pattern selection = StreamingPatternMaker.makeStreamingPattern(select, getConfiguration(), null);

        if (selection != null) {
            return (watchManager, out, context) -> {
                ShallowDescentFunctionCallAction action = new ShallowDescentFunctionCallAction(getExpression(), out, context);
                action.setWatchManager(watchManager);
                return new Trigger(selection, action, context);
            };
        }
        return null;
    }

    private static class ShallowDescentFunctionCallAction extends ItemFeed {

        private XPathContextMajor localContext;
        private WatchManager watchManager;
        private StreamableUserFunction targetFunction;
        private Watch innerWatch;
        private boolean started;

        public ShallowDescentFunctionCallAction(Expression functionCall, ItemFeed out, XPathContext context) {
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
        ShallowDescentFunctionCallAction action = new ShallowDescentFunctionCallAction(getExpression(), out, context);
        action.setWatchManager(watchManager);
        return action;
    }


}

// Copyright (c) 2018-2023 Saxonica Limited