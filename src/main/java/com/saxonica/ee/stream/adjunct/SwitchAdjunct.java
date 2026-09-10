package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.optim.SwitchExpression;
import com.saxonica.ee.stream.Inversion;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.feed.FilteringFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.feed.NoOpenFeed;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.ItemConsumer;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.pattern.NodeTestPattern;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ErrorType;
import net.sf.saxon.value.AtomicValue;

import java.util.HashMap;
import java.util.Map;


/**
 * Streaming code to implement a switch expression, which results from optimizing a choose where all the
 * branches have similar test conditions. Unlike a choose, the conditions cannot be consuming: this is
 * because the conditions must be similar, and if one is consuming then several will be consuming.
 */
public class SwitchAdjunct extends TransmissionAdjunct {

    private Map<Operand, Inversion> actionInversions;

    @Override
    public void setExpression(Expression expression) {
        super.setExpression(expression);
        actionInversions = new HashMap<>(((SwitchExpression) getExpression()).getCaseMap().size());
    }

    @Override
    public WatchMaker getWatchMaker(final boolean forGrouping) throws XPathException {

        final SwitchExpression exp = (SwitchExpression) getExpression();


        return (watchManager, out, context) -> {
            // first do the open() actions of the parent expression. This is needed in case the parent
            // expression initializes variables used in the conditions of the xsl:choose
            out.open(new Terminator());
            // Now ensure that the open doesn't get executed a second time
            out = new NoOpenFeed(out, context);


            Operand selected;
            AtomicValue subjectValue = (AtomicValue) exp.getSubjectExpression().evaluateItem(context);
            if (subjectValue == null) {
                selected = exp.getOtherwiseOperand();
            } else {
                AtomicMatchKey key = subjectValue.getXPathMatchKey(exp.getCollation(), context.getImplicitTimezone());
                Map<AtomicMatchKey, Integer> caseMap = exp.getCaseMap();
                if (caseMap.containsKey(key)) {
                    selected = exp.getAction(caseMap.get(key));
                } else {
                    selected = exp.getOtherwiseOperand();
                }
            }
            Expression action = selected.getChildExpression();
            if (Streamability.getSweep(action) == Sweep.CONSUMING) {
                Inversion actionInversion = actionInversions.get(selected);
                if (actionInversion == null) {
                    actionInversion = Inversion.invertExpression(action, forGrouping);
                    actionInversions.put(selected, actionInversion);
                }
                return actionInversion.getWatch(watchManager, out, context);

            } else {
                SequenceTool.supply(action.iterate(context), (ItemConsumer<? super Item>) out::append);
            }

            // if no condition matches, or if a matching condition has been processed without reference to the
            // source document, then return a Watch that never fires
            return new Trigger(new NodeTestPattern(ErrorType.getInstance()),
                               new FilteringFeed(watchManager, out, context, FilteringFeed.OpaqueFilter),
                               context);
        };
    }

    @Override
    public Trigger makeTransmissionFlow(WatchManager watchManager, Expression exp, ItemFeed out, XPathContext context) throws XPathException {
        WatchMaker wm = getWatchMaker(false);
        if (wm != null) {
            return wm.makeWatch(watchManager, out, context);
        } else {
            // This implies the condition is consuming, but in that case the action cannot return streamed nodes,
            // So we shouldn't get here.
            throw new XPathException("Choose expression too complex for streaming", "XTSE3430");
        }
    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return getFeedMaker(-1).makeItemFeed(watchManager, out, context);
    }


}

// Copyright 2018-2022 Saxonica Limited.