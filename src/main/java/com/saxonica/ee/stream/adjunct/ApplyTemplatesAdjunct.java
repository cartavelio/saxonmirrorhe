package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.*;
import com.saxonica.ee.stream.feed.ApplyTemplatesFeed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.ApplyTemplatesAction;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.expr.Component;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ApplyTemplates;
import net.sf.saxon.expr.sort.SortExpression;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.Mode;
import net.sf.saxon.trans.XPathException;

import java.util.ArrayList;
import java.util.List;


/**
 * Handle streaming for the xsl:apply-templates instruction
 */

public class ApplyTemplatesAdjunct extends StreamingAdjunct implements FeedMaker {

    /**
     * Set the expression being processed by this StreamingAdjunct
     *
     * @param expression the expression being processed
     */
    @Override
    public void setExpression(Expression expression) {
        super.setExpression(expression);
    }

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        if (reasons == null) {
            reasons = new ArrayList<>();
        }

        ApplyTemplates exp = (ApplyTemplates)getExpression();

        // See XSLT 3.0 19.8.4.5

        // Rule 1
        PostureAndSweep selectPS = Streamability.getStreamability(exp.getSelectExpression(), contextInfo, reasons);
        if (selectPS.getPosture() == Posture.GROUNDED) {
            return Streamability.generalStreamabilityRules(exp, exp.operands(), contextInfo, reasons);
        }

        // Rule 2
        if (exp.getSelectExpression() instanceof SortExpression) {
            reasons.add("Cannot use sorting in a streamable template");
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }

        // Rule 3
        Mode mode = exp.getMode();
        if (!exp.usesCurrentMode() && mode != null && !mode.isDeclaredStreamable()) {
            StructuredQName modeName = mode.getModeName();
            String name = modeName == null ? "(unnamed)" : modeName.getDisplayName();
            reasons.add("Cannot use non-streamable mode " + name + " in a streamable template");
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }

        // Rule 4
        if (selectPS.getPosture() == Posture.CLIMBING) {
            reasons.add("In a streaming apply-templates instruction, the select expression must not have climbing posture "
                                    + "(for example, it cannot select ancestors)");
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }
        if (selectPS.getPosture() == Posture.CRAWLING) {
            reasons.add("In a streaming apply-templates instruction, the select expression must not have crawling posture "
                                    + "(for example, it cannot select descendants)");
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }

        // Rule 5
        return Streamability.generalStreamabilityRules(exp, exp.operands(), contextInfo, reasons);

    }


    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        final ApplyTemplates instruction = (ApplyTemplates)getExpression();
        final Expression select = instruction.getSelectExpression();

        // If the select expression is grounded (but consuming), return null; so we end up with a ItemFeed rather than a Watch.
        if (Streamability.getPosture(select) == Posture.GROUNDED) {
            return null;
        }

        final Pattern selection = StreamingPatternMaker.makeStreamingPattern(select, getConfiguration(), null);
        final StreamingAdjunct sa = StreamingAdjunct.makeStreamingAdjunct(getConfiguration(), select);

        if (selection != null || sa instanceof TransmissionAdjunct) {
            return (watchManager, out, context) -> {
                Component.M actualMode = instruction.getTargetMode(context);
                ApplyTemplatesAction action = new ApplyTemplatesAction(getExpression(), out, context);
                action.setMode(actualMode);
                action.setWatchManager(watchManager);
                if (selection != null) {
                    return new Trigger(selection, action, context);
                } else {
                    return ((TransmissionAdjunct) sa).makeTransmissionFlow(watchManager, select, action, context);
                }
            };
        }
        return null;
    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        if (Streamability.getSweep(((ApplyTemplates) getExpression()).getSelectExpression()) != Sweep.CONSUMING) {
            int i = 0;
            for (Operand o : getExpression().operands()) {
                if (Streamability.getSweep(o.getChildExpression()) == Sweep.CONSUMING) {
                    return getDefaultFeedMaker(i).makeItemFeed(watchManager, out, context);
                }
                i++;
            }
        }
        return new ApplyTemplatesFeed((ApplyTemplates) getExpression(), out, context);
    }


}

// Copyright (c) 2009-2023 Saxonica Limited