using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class SwitchAdjunct : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct {
        private System.Collections.Generic.IDictionary<Saxon.Hej.expr.Operand,Saxon.Eej.ee.stream.Inversion> actionInversions;
        public override void setExpression(Saxon.Hej.expr.Expression expression) {
            base.setExpression(expression);
            actionInversions = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.Operand, Saxon.Eej.ee.stream.Inversion>(((Saxon.Eej.ee.optim.SwitchExpression)(getExpression())).getCaseMap().Count);
        }
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Eej.ee.optim.SwitchExpression exp = (Saxon.Eej.ee.optim.SwitchExpression)(getExpression());
            return (watchManager, @out, context) => {
                @out.open(new Saxon.Eej.ee.stream.watch.Terminator());
                @out = new Saxon.Eej.ee.stream.feed.NoOpenFeed(@out, context);
                Saxon.Hej.expr.Operand selected;
                Saxon.Hej.value.AtomicValue subjectValue = (Saxon.Hej.value.AtomicValue)(exp.getSubjectExpression().evaluateItem(context));
                if ((subjectValue == null)) {
                    selected = exp.getOtherwiseOperand();
                } else {
                    Saxon.Hej.expr.sort.AtomicMatchKey key = subjectValue.getXPathMatchKey(exp.getCollation(), context.getImplicitTimezone());
                    System.Collections.Generic.IDictionary<Saxon.Hej.expr.sort.AtomicMatchKey,int> caseMap = exp.getCaseMap();
                    if (caseMap.ContainsKey(key)) {
                        selected = exp.getAction(Saxon.Impl.Helpers.MapUtils.Get(caseMap, key));
                    } else {
                        selected = exp.getOtherwiseOperand();
                    }
                }
                Saxon.Hej.expr.Expression action = selected.getChildExpression();
                if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(action) == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                    Saxon.Eej.ee.stream.Inversion actionInversion = Saxon.Impl.Helpers.MapUtils.Get(actionInversions, selected);
                    if ((actionInversion == null)) {
                        actionInversion = /*134*/Saxon.Eej.ee.stream.Inversion.invertExpression(action, forGrouping);
                        Saxon.Impl.Helpers.MapUtils.Put(actionInversions, selected, actionInversion);
                    }
                    return actionInversion.getWatch(watchManager, @out, context);
                } else {
                    Saxon.Hej.om.SequenceTool.supply(action.iterate(context), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)(/* Unresolved method reference */@out.append));
                }
                return new Saxon.Eej.ee.stream.watch.Trigger(new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.type.ErrorType.getInstance()), new Saxon.Eej.ee.stream.feed.FilteringFeed(watchManager, @out, context, Saxon.Eej.ee.stream.feed.FilteringFeed.OpaqueFilter), context);
            } ;
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
    
    }
}