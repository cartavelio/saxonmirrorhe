package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Inversion;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.feed.FeedMaker;
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
import net.sf.saxon.expr.elab.BooleanEvaluator;
import net.sf.saxon.expr.instruct.Choose;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.ItemConsumer;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.pattern.NodeTestPattern;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ErrorType;
import net.sf.saxon.value.BooleanValue;


/**
 * Streaming code to implement a conditional ("if/choose") expression
 */
public class ChooseAdjunct extends TransmissionAdjunct {

    int consumingCondition = -2;
    Inversion invertedCondition;
    Inversion[] actionInversions;

    @Override
    public void setExpression(Expression expression) {
        super.setExpression(expression);
        actionInversions = new Inversion[((Choose)getExpression()).size()];
    }



    private int getConsumingCondition() throws XPathException {
        if (consumingCondition == -2) {
            consumingCondition = -1;
            int cond = 0;
            for (Operand o : ((Choose) getExpression()).conditions()) {
                if (Streamability.getSweep(o.getChildExpression()) == Sweep.CONSUMING) {
                    consumingCondition = cond;
                    invertedCondition = Inversion.invertExpression(o.getChildExpression(), false);
                    break;
                }
                cond++;
            }
        }
        return consumingCondition;
    }

    @Override
    public WatchMaker getWatchMaker(final boolean forGrouping) throws XPathException {

        final Choose exp = (Choose) getExpression();

        if (getConsumingCondition() < 0) {
            return (watchManager, out, context) -> {
                BooleanEvaluator[] booleanEvaluators = exp.getConditionEvaluators();
                // first do the open() actions of the parent expression. This is needed in case the parent
                // expression initializes variables used in the conditions of the xsl:choose
                out.open(new Terminator());
                // Now ensure that the open doesn't get executed a second time
                out = new NoOpenFeed(out, context);
                for (int i=0; i<exp.size(); i++) {
                    if (booleanEvaluators[i].eval(context)) {
                        Expression action = exp.getAction(i);
                        if (Streamability.getSweep(action) == Sweep.CONSUMING) {
                            Inversion actionInversion = actionInversions[i];
                            if (actionInversion == null) {
                                actionInversion = Inversion.invertExpression(action, forGrouping);
                                actionInversions[i] = actionInversion;
                            }
                            return actionInversion.getWatch(watchManager, out, context);

                        } else {
                            SequenceTool.supply(action.iterate(context), (ItemConsumer<? super Item>) out::append);
                            break;
                        }
                    }
                }
                // if no condition matches, or if a matching condition has been processed without reference to the
                // source document, then return a Watch that never fires
                return new Trigger(new NodeTestPattern(ErrorType.getInstance()),
                                   new FilteringFeed(watchManager, out, context, FilteringFeed.OpaqueFilter),
                                   context);
            };
        } else {
            return null; // For a consuming condition, we use a feed rather than a watch
        }
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

    @Override
    public FeedMaker getFeedMaker(int arg) throws XPathException {
        if (getConsumingCondition() < 0) {
            throw new XPathException("Streaming xsl:choose: expecting a watch");
            //return new FeedMakerForMotionlessCondition();
        } else {
            return new FeedMakerForConsumingCondition(this, getConsumingCondition());
        }
    }


    private static class FeedMakerForConsumingCondition implements FeedMaker {

        final ChooseAdjunct chooseAdjunct;
        final Choose expression;
        final int consumingCondition;

        public FeedMakerForConsumingCondition(ChooseAdjunct adjunct, int consumingCondition) {
            this.chooseAdjunct = adjunct;
            this.expression = (Choose) adjunct.getExpression();
            this.consumingCondition = consumingCondition;
        }

        @Override
        public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
            ItemFeed action = new ItemFeedForConsumingCondition(expression, consumingCondition, out, context);
            // Now construct an input feed that computes the EBV. May be unnecessary since
            // the optimizer will have already inserted a call of boolean() in the expression tree.
            return new BooleanFnFeed.Positive(watchManager, expression, action, context);
        }
    }

    private static class ItemFeedForConsumingCondition extends ItemFeed {

        private final int consumingCondition;
        private boolean matched = false;
        BooleanEvaluator[] booleanEvaluators;

        public ItemFeedForConsumingCondition(Expression expression, int consumingCondition,
                                             ItemFeed out, XPathContext context) {
            super(expression, out, context);
            this.consumingCondition = consumingCondition;
        }

        /**
         * Start evaluating the expression.
         *
         * @param terminator used to achieve early exit
         * @throws XPathException if a dynamic error occurs
         */
        @Override
        public void open(Terminator terminator) throws XPathException {
            Choose expression = (Choose)getExpression();
            booleanEvaluators = expression.getConditionEvaluators();
            super.open(terminator);
            // evaluate all conditions prior to the consuming condition
            for (int i = 0; i < consumingCondition; i++) {
                boolean truth = booleanEvaluators[i].eval(getContext());
                if (truth) {
                    SequenceIterator iter = expression.getAction(i).iterate(getContext());
                    processItems(iter, getNextOutputter());
                    matched = true;
                    break;
                }
            }
        }

        @Override
        public void append(Item item) throws XPathException {
            boolean truth = ((BooleanValue) item).getBooleanValue();
            Choose expression = (Choose) getExpression();
            Expression selectedAction = null;
            if (!matched) {
                if (truth) {
                    selectedAction = expression.getAction(consumingCondition);
                } else {
                    for (int i = 1; i < expression.size(); i++) {
                        if (booleanEvaluators[i].eval(getContext())) {
                            selectedAction = expression.getAction(i);
                            break;
                        }
                    }
                }
            }
            if (selectedAction != null) {
                SequenceIterator iter = selectedAction.iterate(getContext());
                processItems(iter, getNextOutputter());
            }
        }


    }

}

// Copyright 2010-2022 Saxonica Limited.