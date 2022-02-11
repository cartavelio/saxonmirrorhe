using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class IterateAdjunct : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.instruct.IterateInstr exp = (Saxon.Hej.expr.instruct.IterateInstr)(getExpression());
            Saxon.Eej.ee.stream.PostureAndSweep initiallyPS = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(exp.getInitiallyExp(), contextInfo, reasons);
            if ((initiallyPS.getSweep() != Saxon.Eej.ee.stream.Sweep.MOTIONLESS)) {
                if ((reasons != null)) {
                    reasons.Add("The xsl:param initializer is not motionless");
                }
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            Saxon.Eej.ee.stream.PostureAndSweep finallyPS = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(exp.getOnCompletion(), contextInfo, reasons);
            if ((finallyPS.getSweep() != Saxon.Eej.ee.stream.Sweep.MOTIONLESS)) {
                if ((reasons != null)) {
                    reasons.Add("The xsl:on-completion expression is not motionless");
                }
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            return /*134*/Saxon.Eej.ee.stream.adjunct.ForEachAdjunct.getStreamability(exp, contextInfo, reasons);
        }
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.expr.Expression select = ((Saxon.Hej.expr.instruct.IterateInstr)(getExpression())).getSelectExpression();
            Saxon.Hej.expr.Expression action = ((Saxon.Hej.expr.instruct.IterateInstr)(getExpression())).getActionExpression();
            Saxon.Hej.Configuration config = getConfiguration();
            if ((/*134*/Saxon.Eej.ee.stream.Streamability.getPosture(select) == Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                return null;
            }
            Saxon.Eej.ee.stream.Inversion body = null;
            if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(action) == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                body = /*134*/Saxon.Eej.ee.stream.Inversion.invertExpression(action, forGrouping);
            }
            Saxon.Eej.ee.stream.Inversion actionInversion = body;
            Saxon.Hej.pattern.Pattern selection = /*134*/Saxon.Eej.ee.stream.StreamingPatternMaker.makeStreamingPattern(select, config, null);
            Saxon.Eej.ee.stream.adjunct.StreamingAdjunct sa = /*134*/Saxon.Eej.ee.stream.adjunct.StreamingAdjunct.makeStreamingAdjunct(config, select);
            return (watchManager, @out, context) => {
                Saxon.Eej.ee.stream.watch.IterateAction actor = new Saxon.Eej.ee.stream.watch.IterateAction(watchManager, getExpression(), @out, context);
                actor.setInversion(actionInversion);
                if ((selection != null)) {
                    return new Saxon.Eej.ee.stream.watch.Trigger(selection, actor, context);
                } else {
                    return ((Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct)(sa)).makeTransmissionFlow(watchManager, select, actor, context);
                }
            } ;
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.watch.IterateAction(watchManager, getExpression(), @out, context);
        }
    
    }
}