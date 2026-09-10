using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class ShallowDescentFunctionCallAdjunct : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct {
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            if ((reasons == null)) {
                reasons = new System.Collections.Generic.List<string>();
            }
            Saxon.Hej.expr.UserFunctionCall exp = (Saxon.Hej.expr.UserFunctionCall)(getExpression());
            Saxon.Hej.expr.instruct.UserFunction target = exp.getFunction();
            Saxon.Eej.ee.stream.PostureAndSweep ps0 = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(exp.getArg(0), contextInfo, reasons);
            Saxon.Eej.ee.stream.Posture p0 = ps0.getPosture();
            if (((p0 != Saxon.Eej.ee.stream.Posture.STRIDING) && (p0 != Saxon.Eej.ee.stream.Posture.GROUNDED))) {
                reasons.Add("Posture of first argument to a shallow-descent function must be striding or grounded");
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            Saxon.Eej.ee.stream.PostureAndSweep ps1 = Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS;
            if ((exp.getOperanda().getNumberOfOperands()>1)) {
                Saxon.Hej.expr.parser.RebindingMap rebindings = new Saxon.Hej.expr.parser.RebindingMap();
                Saxon.Hej.expr.Expression[] children = new Saxon.Hej.expr.Expression[(exp.getOperanda().getNumberOfOperands()-1)];
                bool interesting = false;
                for (int i = 0; (i<children.Length); i++) {
                    Saxon.Hej.expr.Expression arg = exp.getArg((i+1));
                    Saxon.Eej.ee.stream.PostureAndSweep ps = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(arg, contextInfo, reasons);
                    if (!(ps.Equals(Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS))) {
                        interesting = true;
                    }
                    children[i] = arg.copy(rebindings);
                }
                if (interesting) {
                    Saxon.Hej.expr.instruct.Block block = new Saxon.Hej.expr.instruct.Block(children);
                    System.Collections.Generic.IList<Saxon.Hej.expr.Operand> operands = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>(children.Length);
                    for (int i = 0; (i<children.Length); i++) {
                        Saxon.Hej.expr.OperandRole original = exp.getOperanda().getOperand((i+1)).getOperandRole();
                        Saxon.Hej.expr.OperandUsage usage = Saxon.Hej.expr.Operand.typeDeterminedUsage(target.getArgumentType((i+1)).getPrimaryType());
                        Saxon.Hej.expr.OperandRole role = new Saxon.Hej.expr.OperandRole(original.getProperties(), usage);
                        Saxon.Hej.expr.Operand o2 = new Saxon.Hej.expr.Operand(children[i], block, role);
                        operands.Add(o2);
                    }
                    ps1 = /*134*/Saxon.Eej.ee.stream.Streamability.generalStreamabilityRules(block, operands, contextInfo, reasons);
                }
            }
            if ((ps1.getPosture() != Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                reasons.Add("Arguments after the first must be grounded");
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            if (((ps0.getSweep() == Saxon.Eej.ee.stream.Sweep.CONSUMING) && (ps1.getSweep() == Saxon.Eej.ee.stream.Sweep.CONSUMING))) {
                reasons.Add("More than one argument is consuming");
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            if (((ps0.getSweep() == Saxon.Eej.ee.stream.Sweep.FREE_RANGING) || (ps1.getSweep() == Saxon.Eej.ee.stream.Sweep.FREE_RANGING))) {
                reasons.Add("All arguments must be streamable");
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            if ((p0 == Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.GROUNDED, /*134*/Saxon.Eej.ee.stream.Streamability.wider(ps0.getSweep(), ps1.getSweep()));
            }
            if (target.getArgumentType(0).getPrimaryType().getUType().intersection(Saxon.Hej.type.UType.DOCUMENT.union(Saxon.Hej.type.UType.ELEMENT)).Equals(Saxon.Hej.type.UType.VOID)) {
                return new Saxon.Eej.ee.stream.PostureAndSweep(p0, ps0.getSweep());
            }
            if (exp.getArg(0).getItemType().getUType().intersection(Saxon.Hej.type.UType.DOCUMENT.union(Saxon.Hej.type.UType.ELEMENT)).Equals(Saxon.Hej.type.UType.VOID)) {
                return new Saxon.Eej.ee.stream.PostureAndSweep(p0, ps0.getSweep());
            }
            return new Saxon.Eej.ee.stream.PostureAndSweep(p0, Saxon.Eej.ee.stream.Sweep.CONSUMING);
        }
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.expr.UserFunctionCall instruction = (Saxon.Hej.expr.UserFunctionCall)(getExpression());
            Saxon.Hej.expr.Expression select = instruction.getArg(0);
            Saxon.Hej.pattern.Pattern selection = /*134*/Saxon.Eej.ee.stream.StreamingPatternMaker.makeStreamingPattern(select, getConfiguration(), null);
            if ((selection != null)) {
                return (watchManager, @out, context) => {
                    Saxon.Eej.ee.stream.adjunct.ShallowDescentFunctionCallAdjunct.ShallowDescentFunctionCallAction action = new Saxon.Eej.ee.stream.adjunct.ShallowDescentFunctionCallAdjunct.ShallowDescentFunctionCallAction(getExpression(), @out, context);
                    action.setWatchManager(watchManager);
                    return new Saxon.Eej.ee.stream.watch.Trigger(selection, action, context);
                } ;
            }
            return null;
        }
        private class ShallowDescentFunctionCallAction : Saxon.Eej.ee.stream.feed.ItemFeed {
            private Saxon.Hej.expr.XPathContextMajor localContext;
            private Saxon.Eej.ee.stream.watch.WatchManager watchManager;
            private Saxon.Eej.ee.stream.StreamableUserFunction targetFunction;
            private Saxon.Eej.ee.stream.watch.Watch innerWatch;
            private bool started;
            public ShallowDescentFunctionCallAction(Saxon.Hej.expr.Expression functionCall, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) : base (functionCall, @out, context) {
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
                start();
                innerWatch.setAnchorNode(node);
                watchManager.addWatch(innerWatch, false);
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
                watchManager.removeWatch(innerWatch);
            }
            public override void close() {
                start();
                innerWatch.close();
                base.close();
            }
        
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            Saxon.Eej.ee.stream.adjunct.ShallowDescentFunctionCallAdjunct.ShallowDescentFunctionCallAction action = new Saxon.Eej.ee.stream.adjunct.ShallowDescentFunctionCallAdjunct.ShallowDescentFunctionCallAction(getExpression(), @out, context);
            action.setWatchManager(watchManager);
            return action;
        }
    
    }
}