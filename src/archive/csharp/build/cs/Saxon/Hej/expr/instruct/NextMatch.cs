using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class NextMatch : Saxon.Hej.expr.instruct.ApplyNextMatchingTemplate {
        internal bool useTailRecursion;
        public NextMatch(bool useTailRecursion) : base () {
            this.useTailRecursion = useTailRecursion;
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_NEXT_MATCH;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.NextMatch nm2 = new Saxon.Hej.expr.instruct.NextMatch(useTailRecursion);
            nm2.setActualParams(Saxon.Hej.expr.instruct.WithParam.copy(nm2, getActualParams(), rebindings));
            nm2.setTunnelParams(Saxon.Hej.expr.instruct.WithParam.copy(nm2, getTunnelParams(), rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, nm2);
            return nm2;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("nextMatch", this);
            string flags = "i";
            if (useTailRecursion) {
                flags = "t";
            }
            @out.emitAttribute("flags", flags);
            if ((getActualParams().Length != 0)) {
                Saxon.Hej.expr.instruct.WithParam.exportParameters(getActualParams(), @out, false);
            }
            if ((getTunnelParams().Length != 0)) {
                Saxon.Hej.expr.instruct.WithParam.exportParameters(getTunnelParams(), @out, true);
            }
            @out.endElement();
        }
        private class NextMatchPackage : Saxon.Hej.expr.instruct.TailCall {
            private readonly Saxon.Hej.expr.instruct.NextMatch instruction;
            private readonly Saxon.Hej.trans.rules.Rule rule;
            private readonly Saxon.Hej.expr.instruct.ParameterSet @params;
            private readonly Saxon.Hej.expr.instruct.ParameterSet tunnelParams;
            private readonly Saxon.Hej.@event.Outputter output;
            private readonly Saxon.Hej.expr.XPathContext evaluationContext;
            public NextMatchPackage(Saxon.Hej.expr.instruct.NextMatch instruction, Saxon.Hej.trans.rules.Rule rule, Saxon.Hej.expr.instruct.ParameterSet @params, Saxon.Hej.expr.instruct.ParameterSet tunnelParams, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext evaluationContext)  {
                this.instruction = instruction;
                this.rule = rule;
                this.@params = @params;
                this.tunnelParams = tunnelParams;
                this.output = output;
                this.evaluationContext = evaluationContext;
            }
            public Saxon.Hej.expr.instruct.TailCall processLeavingTail() {
                Saxon.Hej.expr.instruct.TemplateRule nh = (Saxon.Hej.expr.instruct.TemplateRule)(rule.getAction());
                nh.initialize();
                Saxon.Hej.expr.XPathContextMajor c2 = evaluationContext.newContext();
                c2.setOrigin(instruction);
                c2.setLocalParameters(@params);
                c2.setTunnelParameters(tunnelParams);
                c2.openStackFrame(nh.getStackFrameMap());
                c2.setCurrentTemplateRule(rule);
                c2.setCurrentComponent(evaluationContext.getCurrentComponent());
                c2.setCurrentMergeGroupIterator(null);
                Saxon.Hej.trans.Mode mode = evaluationContext.getCurrentMode().getActor();
                if (mode.isModeTracing()) {
                    Saxon.Hej.trace.TemplateRuleTraceListener tracer = ((Saxon.Hej.trans.XsltController)(evaluationContext.getController())).getTemplateRuleTraceListener();
                    tracer.enter("next-match", instruction.getLocation(), evaluationContext.getContextItem(), nh);
                    Saxon.Hej.expr.instruct.TailCall tc = nh.applyLeavingTail(output, c2);
                    tracer.leave();
                    return tc;
                } else {
                    return nh.applyLeavingTail(output, c2);
                }
            }
        
        }
        public override string getStreamerName() {
            return "NextMatch";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.NextMatch.NextMatchElaborator();
        }
        private class NextMatchElaborator : Saxon.Hej.expr.elab.PushElaborator {
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.instruct.NextMatch expr = (Saxon.Hej.expr.instruct.NextMatch)(getExpression());
                Saxon.Hej.s9api.Location loc = expr.getLocation();
                return (output, context) => {
                    Saxon.Hej.Controller controller = context.getController();
                    System.Diagnostics.Debug.Assert((controller != null));
                    Saxon.Hej.expr.instruct.ParameterSet @params = assembleParams(context, expr.getActualParams());
                    Saxon.Hej.expr.instruct.ParameterSet tunnels = assembleTunnelParams(context, expr.getTunnelParams());
                    Saxon.Hej.trans.rules.Rule currentRule = context.getCurrentTemplateRule();
                    if ((currentRule == null)) {
                        Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("There is no current template rule", "XTDE0560");
                        e.setXPathContext(context);
                        e.setLocation(loc);
                        throw e;
                    }
                    Saxon.Hej.expr.Component.M modeComponent = context.getCurrentMode();
                    if ((modeComponent == null)) {
                        throw new Saxon.Ejava.lang.AssertionError("Current mode is null");
                    }
                    Saxon.Hej.trans.Mode mode = modeComponent.getActor();
                    Saxon.Hej.om.Item currentItem = context.getCurrentIterator().current();
                    Saxon.Hej.trans.rules.Rule rule = mode.getNextMatchRule(currentItem, currentRule, context);
                    if ((rule == null)) {
                        mode.getBuiltInRuleSet().process(currentItem, @params, tunnels, output, context, loc);
                    } else {
                        if (expr.useTailRecursion) {
                            Saxon.Ejava.util.Arrays.fill(context.getStackFrame().getStackFrameValues(), null);
                            ((Saxon.Hej.expr.XPathContextMajor)(context)).setCurrentComponent(modeComponent);
                            return new Saxon.Hej.expr.instruct.NextMatch.NextMatchPackage(expr, rule, @params, tunnels, output, context);
                        } else {
                            Saxon.Hej.expr.instruct.TemplateRule nh = (Saxon.Hej.expr.instruct.TemplateRule)(rule.getAction());
                            nh.initialize();
                            Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                            c2.setOrigin(expr);
                            c2.openStackFrame(nh.getStackFrameMap());
                            c2.setLocalParameters(@params);
                            c2.setTunnelParameters(tunnels);
                            c2.setCurrentTemplateRule(rule);
                            c2.setCurrentComponent(modeComponent);
                            c2.setCurrentMergeGroupIterator(null);
                            if (mode.isModeTracing()) {
                                Saxon.Hej.trace.TemplateRuleTraceListener tracer = ((Saxon.Hej.trans.XsltController)(controller)).getTemplateRuleTraceListener();
                                tracer.enter("next-match", loc, currentItem, nh);
                                nh.apply(output, c2);
                                tracer.leave();
                            } else {
                                nh.apply(output, c2);
                            }
                        }
                    }
                    return null;
                } ;
            }
        
        }
    
    }
}