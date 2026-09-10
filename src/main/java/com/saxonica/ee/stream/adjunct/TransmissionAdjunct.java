package com.saxonica.ee.stream.adjunct;


import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.UserFunctionParameter;
import net.sf.saxon.om.AxisInfo;
import net.sf.saxon.pattern.AncestorQualifiedPattern;
import net.sf.saxon.pattern.AnchorPattern;
import net.sf.saxon.pattern.NodeTestPattern;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;

/**
 * A subclass of StreamingAdjunct used for expressions that have a sequence-valued operand with usage
 * transmission. Such expressions may appear as a filter for striding or crawling nodes, in which case
 * they need to filter (or in some cases augment) the event stream rather than operating at the level of
 * composed items.
 */

public abstract class TransmissionAdjunct extends StreamingAdjunct implements FeedMaker {

    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        Posture posture = Streamability.getPosture(getExpression());
        if (Streamability.isIncrementalPosture(posture)) {
            return (watchManager, out, context) -> makeTransmissionFlow(watchManager, getExpression(), out, context);
        } else {
            return null;
        }
    }

    /**
     * Make a Trigger that implements this expression in streaming mode. This method is used
     * for an expression that is striding or crawling, and it delivers a Trigger connected to a sequence
     * of Feeds which together have the effect of delivering a sequence of events and/or items that
     * together represent the results of this instruction. This method MUST be implemented for every
     * expression that has an operand whose usage is transmission (and for every such operand).
     *
     * @param watchManager the WatchManager that fires this trigger
     * @param exp the expression
     * @param out the next action in the push pipeline for evaluation
     * @param context the XPath dynamic context
     * @return the Trigger (a Watch with a pattern)
     * @throws XPathException if a dynamic evaluation error occurs.
     */

    public Trigger makeTransmissionFlow(
            WatchManager watchManager, Expression exp, ItemFeed out, XPathContext context)
            throws XPathException {
        Expression select = null;
        for (Operand o : exp.operands()) {
            if (Streamability.getSweep(o.getChildExpression()) == Sweep.CONSUMING && o.hasSameFocus()) {
                select = o.getChildExpression();
                break;
            }
        }
        if (select == null) {
            // No consuming operand found, look for a potentially-consuming one, e.g. "."
            for (Operand o : exp.operands()) {
                if (o.hasSameFocus() && o.getUsage() == OperandUsage.TRANSMISSION) {
                    select = o.getChildExpression();
                    break;
                }
            }
        }
        Pattern selection = null;
        if (select == null && exp instanceof SimpleStepExpression) {
            // special-case for expressions such as $node/child::x within a streamable stylesheet function
            // TODO: generalize this
            SimpleStepExpression sse = (SimpleStepExpression)exp;
            Expression lhs = sse.getLhsExpression();
            if (lhs instanceof LocalVariableReference && ((LocalVariableReference)lhs).getBinding() instanceof UserFunctionParameter) {
                AxisExpression ae = (AxisExpression)sse.getRhsExpression();
                int axis = ae.getAxis();
                if (axis == AxisInfo.CHILD) {
                    selection = new AncestorQualifiedPattern(
                            new NodeTestPattern(ae.getNodeTest()),
                            AnchorPattern.getInstance(),
                            AxisInfo.PARENT);
                } else if (axis == AxisInfo.DESCENDANT) {
                    selection = new AncestorQualifiedPattern(
                            new NodeTestPattern(ae.getNodeTest()),
                            AnchorPattern.getInstance(),
                            AxisInfo.ANCESTOR);
                }
            }
        }
        if (select == null && selection == null) {
            throw new XPathException(
                "No transmission watch for " + exp.toShortString(), SaxonErrorCode.SXST0067, exp.getLocation());
        }
        if (select != null && selection == null) {
            selection = Streamability.toStreamingPattern(select, context.getConfiguration());
        }
        ItemFeed fw = makeItemFeed(watchManager, out, context);

        if (selection != null) {
            // Create a watch on the pattern corresponding to the consuming subexpression, and
            // connect this watch to the Feed corresponding to this expression
            return new Trigger(selection, fw, context);
        } else {
            StreamingAdjunct sa = StreamingAdjunct.makeStreamingAdjunct(getConfiguration(), select);
            if (sa instanceof TransmissionAdjunct) {
                return ((TransmissionAdjunct)sa).makeTransmissionFlow(watchManager, select, fw, context);
            }
        }
        throw new XPathException(
            "No transmission filter available for " + select.toShortString(), SaxonErrorCode.SXST0067, exp.getLocation());
    }


}

// Copyright (c) 2018-2023 Saxonica Limited