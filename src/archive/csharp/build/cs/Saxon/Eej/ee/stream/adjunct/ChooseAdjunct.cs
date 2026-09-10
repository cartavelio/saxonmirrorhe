using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class ChooseAdjunct : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct {
        internal int consumingCondition = -2;
        internal Saxon.Eej.ee.stream.Inversion invertedCondition;
        internal Saxon.Eej.ee.stream.Inversion[] actionInversions;
        public override void setExpression(Saxon.Hej.expr.Expression expression) {
            base.setExpression(expression);
            actionInversions = new Saxon.Eej.ee.stream.Inversion[((Saxon.Hej.expr.instruct.Choose)(getExpression())).size()];
        }
        private int getConsumingCondition() {
            if ((consumingCondition == -2)) {
                consumingCondition = -1;
                int cond = 0;
                foreach (Saxon.Hej.expr.Operand o in ((Saxon.Hej.expr.instruct.Choose)(getExpression())).conditions()) {
                    if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(o.getChildExpression()) == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                        consumingCondition = cond;
                        invertedCondition = /*134*/Saxon.Eej.ee.stream.Inversion.invertExpression(o.getChildExpression(), false);
                        break ;
                    }
                    cond++;
                }
            }
            return consumingCondition;
        }
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.expr.instruct.Choose exp = (Saxon.Hej.expr.instruct.Choose)(getExpression());
            if ((getConsumingCondition()<0)) {
                return (watchManager, @out, context) => {
                    @out.open(new Saxon.Eej.ee.stream.watch.Terminator());
                    @out = new Saxon.Eej.ee.stream.feed.NoOpenFeed(@out, context);
                    for (int i = 0; (i<exp.size()); i++) {
                        if (exp.getCondition(i).effectiveBooleanValue(context)) {
                            Saxon.Hej.expr.Expression action = exp.getAction(i);
                            if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(action) == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                                Saxon.Eej.ee.stream.Inversion actionInversion = actionInversions[i];
                                if ((actionInversion == null)) {
                                    actionInversion = /*134*/Saxon.Eej.ee.stream.Inversion.invertExpression(action, forGrouping);
                                    actionInversions[i] = actionInversion;
                                }
                                return actionInversion.getWatch(watchManager, @out, context);
                            } else {
                                Saxon.Hej.om.SequenceTool.supply(action.iterate(context), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)(it => @out.append(it) /* converted from instance method reference */));
                                break ;
                            }
                        }
                    }
                    return new Saxon.Eej.ee.stream.watch.Trigger(new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.type.ErrorType.getInstance()), new Saxon.Eej.ee.stream.feed.FilteringFeed(@out, context, Saxon.Eej.ee.stream.feed.FilteringFeed.OpaqueFilter), context);
                } ;
            } else {
                return null;
            }
        }
        public override Saxon.Eej.ee.stream.watch.Trigger makeTransmissionFlow(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            Saxon.Eej.ee.stream.watch.WatchMaker wm = getWatchMaker(false);
            if ((wm != null)) {
                return /*Delegate.makeWatch()*/wm(watchManager, @out, context);
            } else {
                throw new Saxon.Hej.trans.XPathException("Choose expression too complex for streaming", "XTSE3430");
            }
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return getFeedMaker(-1).makeItemFeed(watchManager, @out, context);
        }
        public override Saxon.Eej.ee.stream.feed.FeedMaker getFeedMaker(int arg) {
            if ((getConsumingCondition()<0)) {
                throw new Saxon.Hej.trans.XPathException("Streaming xsl:choose: expecting a watch");
            } else {
                return new Saxon.Eej.ee.stream.adjunct.ChooseAdjunct.FeedMakerForConsumingCondition(this, getConsumingCondition());
            }
        }
        private class FeedMakerForConsumingCondition : Saxon.Eej.ee.stream.feed.FeedMaker {
            readonly Saxon.Eej.ee.stream.adjunct.ChooseAdjunct chooseAdjunct;
            readonly Saxon.Hej.expr.instruct.Choose expression;
            readonly int consumingCondition;
            public FeedMakerForConsumingCondition(Saxon.Eej.ee.stream.adjunct.ChooseAdjunct adjunct, int consumingCondition)  {
                this.chooseAdjunct = adjunct;
                this.expression = (Saxon.Hej.expr.instruct.Choose)(adjunct.getExpression());
                this.consumingCondition = consumingCondition;
            }
            public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
                Saxon.Eej.ee.stream.feed.ItemFeed action = new Saxon.Eej.ee.stream.adjunct.ChooseAdjunct.ItemFeedForConsumingCondition(expression, consumingCondition, @out, context);
                return new Saxon.Eej.ee.stream.adjunct.BooleanFnFeed.Positive(expression, action, context);
            }
        
        }
        private class ItemFeedForConsumingCondition : Saxon.Eej.ee.stream.feed.ItemFeed {
            private readonly int consumingCondition;
            private bool matched = false;
            public ItemFeedForConsumingCondition(Saxon.Hej.expr.Expression expression, int consumingCondition, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) : base (expression, @out, context) {
                this.consumingCondition = consumingCondition;
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                Saxon.Hej.expr.instruct.Choose expression = (Saxon.Hej.expr.instruct.Choose)(getExpression());
                for (int i = 0; (i<consumingCondition); i++) {
                    bool truth = expression.getCondition(i).effectiveBooleanValue(getContext());
                    if (truth) {
                        Saxon.Hej.om.SequenceIterator iter = expression.getAction(i).iterate(getContext());
                        processItems(iter, getNextOutputter());
                        matched = true;
                        break ;
                    }
                }
            }
            public override void append(Saxon.Hej.om.Item item) {
                bool truth = ((Saxon.Hej.value.BooleanValue)(item)).getBooleanValue();
                Saxon.Hej.expr.instruct.Choose expression = (Saxon.Hej.expr.instruct.Choose)(getExpression());
                Saxon.Hej.expr.Expression selectedAction = null;
                if (!(matched)) {
                    if (truth) {
                        selectedAction = expression.getAction(consumingCondition);
                    } else {
                        for (int i = 1; (i<expression.size()); i++) {
                            if (expression.getCondition(i).effectiveBooleanValue(getContext())) {
                                selectedAction = expression.getAction(i);
                                break ;
                            }
                        }
                    }
                }
                if ((selectedAction != null)) {
                    Saxon.Hej.om.SequenceIterator iter = selectedAction.iterate(getContext());
                    processItems(iter, getNextOutputter());
                }
            }
        
        }
    
    }
}