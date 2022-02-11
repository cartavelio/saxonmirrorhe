using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class AbsorbingFunctionCallAdjunct : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct {
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            if ((reasons == null)) {
                reasons = new System.Collections.Generic.List<string>();
            }
            Saxon.Hej.expr.UserFunctionCall exp = (Saxon.Hej.expr.UserFunctionCall)(getExpression());
            Saxon.Hej.expr.instruct.UserFunction target = exp.getFunction();
            Saxon.Hej.expr.Operand[] operanda = exp.getOperanda().copy();
            operanda[0].setUsage(Saxon.Hej.expr.OperandUsage.TRANSMISSION);
            for (int i = 1; (i<operanda.Length); i++) {
                operanda[i].setUsage(Saxon.Hej.expr.Operand.typeDeterminedUsage(target.getArgumentType(i).getPrimaryType()));
            }
            Saxon.Eej.ee.stream.PostureAndSweep first = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(exp.getArg(0), contextInfo, reasons);
            if ((first.getPosture() == Saxon.Eej.ee.stream.Posture.CRAWLING)) {
                reasons.Add("The first argument to a function with streamability=absorbing must not have crawling posture");
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            } else {
                operanda[0].setUsage(Saxon.Hej.expr.OperandUsage.ABSORPTION);
                return /*134*/Saxon.Eej.ee.stream.Streamability.generalStreamabilityRules(exp, exp.operands(), contextInfo, reasons);
            }
        }
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.expr.UserFunctionCall instruction = (Saxon.Hej.expr.UserFunctionCall)(getExpression());
            Saxon.Hej.expr.Expression select = instruction.getArg(0);
            Saxon.Hej.pattern.Pattern selection = /*134*/Saxon.Eej.ee.stream.StreamingPatternMaker.makeStreamingPattern(select, getConfiguration(), null);
            if ((selection != null)) {
                return (watchManager, @out, context) => {
                    Saxon.Eej.ee.stream.adjunct.AbsorbingFunctionCallAdjunct.AbsorbingFunctionCallAction action = new Saxon.Eej.ee.stream.adjunct.AbsorbingFunctionCallAdjunct.AbsorbingFunctionCallAction(getExpression(), @out, context);
                    action.setWatchManager(watchManager);
                    return new Saxon.Eej.ee.stream.watch.Trigger(selection, action, context);
                } ;
            }
            return null;
        }
        private class AbsorbingFunctionCallAction : Saxon.Eej.ee.stream.feed.ItemFeed {
            private Saxon.Hej.expr.XPathContextMajor localContext;
            private Saxon.Eej.ee.stream.watch.WatchManager watchManager;
            private Saxon.Eej.ee.stream.StreamableUserFunction targetFunction;
            private Saxon.Eej.ee.stream.watch.Watch innerWatch;
            private bool started;
            public AbsorbingFunctionCallAction(Saxon.Hej.expr.Expression functionCall, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) : base (functionCall, @out, context) {
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                Saxon.Hej.expr.UserFunctionCall exp = (Saxon.Hej.expr.UserFunctionCall)(getExpression());
                Saxon.Hej.expr.Component target = exp.getTargetComponent(getContext());
                targetFunction = (Saxon.Eej.ee.stream.StreamableUserFunction)(target.getActor());
                targetFunction.makeInversion();
                localContext = getContext().newContext();
                localContext.setCurrentComponent(target);
                Saxon.Hej.om.Sequence[] args = exp.evaluateArguments(getContext(), true);
                localContext.setStackFrame(targetFunction.getStackFrameMap(), args);
                started = false;
            }
            public void setWatchManager(Saxon.Eej.ee.stream.watch.WatchManager watchManager) {
                this.watchManager = watchManager;
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                Saxon.Hej.expr.XPathContext context = localContext;
                context.setLocalVariable(0, node);
                bool first = !(started);
                start();
                innerWatch.setAnchorNode(node);
                if (first) {
                    watchManager.addWatch(innerWatch, false);
                }
                return null;
            }
            private void start() {
                if (!(started)) {
                    started = true;
                    Saxon.Eej.ee.stream.ComponentInversion inversion = targetFunction.getInversion();
                    innerWatch = inversion.getWatch(watchManager, getResultFeed(), localContext);
                    innerWatch.open(new Saxon.Eej.ee.stream.watch.Terminator());
                }
            }
            public override void append(Saxon.Hej.om.Item item) {
                if (!(hasFailed())) {
                    Saxon.Hej.expr.XPathContext context = localContext;
                    context.setLocalVariable(0, item);
                    start();
                    ((Saxon.Eej.ee.stream.watch.Trigger)(innerWatch)).getAction().append(item);
                }
            }
            public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
            }
            public override void close() {
                start();
                innerWatch.close();
                watchManager.removeWatch(innerWatch);
            }
        
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            Saxon.Eej.ee.stream.adjunct.AbsorbingFunctionCallAdjunct.AbsorbingFunctionCallAction feed = new Saxon.Eej.ee.stream.adjunct.AbsorbingFunctionCallAdjunct.AbsorbingFunctionCallAction(getExpression(), @out, context);
            feed.setWatchManager(watchManager);
            return feed;
        }
    
    }
}