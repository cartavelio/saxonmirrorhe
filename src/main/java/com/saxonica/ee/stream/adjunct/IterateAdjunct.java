package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.*;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.IterateAction;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.IterateInstr;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;

import java.util.List;

/**
 * Implement streaming code for xsl:iterate
 */

public class IterateAdjunct extends TransmissionAdjunct implements FeedMaker {

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        IterateInstr exp = (IterateInstr)getExpression();
        PostureAndSweep initiallyPS = Streamability.getStreamability(exp.getInitiallyExp(), contextInfo, reasons);
        if (initiallyPS.getSweep() != Sweep.MOTIONLESS) {
            if (reasons != null) {
                reasons.add("The xsl:param initializer is not motionless");
            }
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }
        PostureAndSweep finallyPS = Streamability.getStreamability(exp.getOnCompletion(), contextInfo, reasons);
        if (finallyPS.getSweep() != Sweep.MOTIONLESS) {
            if (reasons != null) {
                reasons.add("The xsl:on-completion expression is not motionless");
            }
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }
        return ForEachAdjunct.getStreamability(exp, contextInfo, reasons);
    }



    /*@Nullable*/
    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        final Expression select = ((IterateInstr) getExpression()).getSelectExpression();
        Expression action = ((IterateInstr) getExpression()).getActionExpression();
        Configuration config = getConfiguration();

        // If the select expression is grounded (but consuming), return null; so we end up with a Feed rather than a Watch.
        if (Streamability.getPosture(select) == Posture.GROUNDED) {
            return null;
        }

        Inversion body = null;
        if (Streamability.getSweep(action) == Sweep.CONSUMING) {
            body = Inversion.invertExpression(action, forGrouping);
        }
        final Inversion actionInversion = body;

        final Pattern selection = StreamingPatternMaker.makeStreamingPattern(select, config, null);
        final StreamingAdjunct sa = StreamingAdjunct.makeStreamingAdjunct(config, select);
        return (watchManager, out, context) -> {
            IterateAction actor = new IterateAction(watchManager, getExpression(), out, context);
            actor.setInversion(actionInversion);
            if (selection != null) {
                return new Trigger(selection, actor, context);
            } else {
                return ((TransmissionAdjunct) sa).makeTransmissionFlow(watchManager, select, actor, context);
            }
        };

    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new IterateAction(watchManager, getExpression(), out, context);
    }

}

// Copyright (c) 2010-2022 Saxonica Limited