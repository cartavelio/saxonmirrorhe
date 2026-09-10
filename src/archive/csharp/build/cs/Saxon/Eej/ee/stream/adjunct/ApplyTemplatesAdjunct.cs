using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class ApplyTemplatesAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override void setExpression(Saxon.Hej.expr.Expression expression) {
            base.setExpression(expression);
        }
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            if ((reasons == null)) {
                reasons = new System.Collections.Generic.List<string>();
            }
            Saxon.Hej.expr.instruct.ApplyTemplates exp = (Saxon.Hej.expr.instruct.ApplyTemplates)(getExpression());
            Saxon.Eej.ee.stream.PostureAndSweep selectPS = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(exp.getSelectExpression(), contextInfo, reasons);
            if ((selectPS.getPosture() == Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                return /*134*/Saxon.Eej.ee.stream.Streamability.generalStreamabilityRules(exp, exp.operands(), contextInfo, reasons);
            }
            if ((exp.getSelectExpression() is Saxon.Hej.expr.sort.SortExpression)) {
                reasons.Add("Cannot use sorting in a streamable template");
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            Saxon.Hej.trans.Mode mode = exp.getMode();
            if (((!(exp.usesCurrentMode()) && (mode != null)) && !(mode.isDeclaredStreamable()))) {
                Saxon.Hej.om.StructuredQName modeName = mode.getModeName();
                string name = ((modeName == null) ? "(unnamed)" : modeName.getDisplayName());
                reasons.Add((("Cannot use non-streamable mode "+name)+" in a streamable template"));
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            if ((selectPS.getPosture() == Saxon.Eej.ee.stream.Posture.CLIMBING)) {
                reasons.Add(("In a streaming apply-templates instruction, the select expression must not have climbing posture "+"(for example, it cannot select ancestors)"));
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            if ((selectPS.getPosture() == Saxon.Eej.ee.stream.Posture.CRAWLING)) {
                reasons.Add(("In a streaming apply-templates instruction, the select expression must not have crawling posture "+"(for example, it cannot select descendants)"));
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            return /*134*/Saxon.Eej.ee.stream.Streamability.generalStreamabilityRules(exp, exp.operands(), contextInfo, reasons);
        }
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.expr.instruct.ApplyTemplates instruction = (Saxon.Hej.expr.instruct.ApplyTemplates)(getExpression());
            Saxon.Hej.expr.Expression select = instruction.getSelectExpression();
            if ((/*134*/Saxon.Eej.ee.stream.Streamability.getPosture(select) == Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                return null;
            }
            Saxon.Hej.pattern.Pattern selection = /*134*/Saxon.Eej.ee.stream.StreamingPatternMaker.makeStreamingPattern(select, getConfiguration(), null);
            Saxon.Eej.ee.stream.adjunct.StreamingAdjunct sa = /*134*/Saxon.Eej.ee.stream.adjunct.StreamingAdjunct.makeStreamingAdjunct(getConfiguration(), select);
            if (((selection != null) || (sa is Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct))) {
                return (watchManager, @out, context) => {
                    Saxon.Hej.expr.Component.M actualMode = instruction.getTargetMode(context);
                    Saxon.Eej.ee.stream.watch.ApplyTemplatesAction action = new Saxon.Eej.ee.stream.watch.ApplyTemplatesAction(getExpression(), @out, context);
                    action.setMode(actualMode);
                    action.setWatchManager(watchManager);
                    if ((selection != null)) {
                        return new Saxon.Eej.ee.stream.watch.Trigger(selection, action, context);
                    } else {
                        return ((Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct)(sa)).makeTransmissionFlow(watchManager, select, action, context);
                    }
                } ;
            }
            return null;
        }
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(((Saxon.Hej.expr.instruct.ApplyTemplates)(getExpression())).getSelectExpression()) != Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                int i = 0;
                foreach (Saxon.Hej.expr.Operand o in getExpression().operands()) {
                    if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(o.getChildExpression()) == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                        return getDefaultFeedMaker(i).makeItemFeed(watchManager, @out, context);
                    }
                    i++;
                }
            }
            return new Saxon.Eej.ee.stream.feed.ApplyTemplatesFeed((Saxon.Hej.expr.instruct.ApplyTemplates)(getExpression()), @out, context);
        }
    
    }
}