using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class ApplyImports : Saxon.Hej.expr.instruct.ApplyNextMatchingTemplate, Saxon.Hej.expr.instruct.ITemplateCall {
        public ApplyImports()  {
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_APPLY_IMPORTS;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.ApplyImports ai2 = new Saxon.Hej.expr.instruct.ApplyImports();
            ai2.setActualParams(Saxon.Hej.expr.instruct.WithParam.copy(ai2, getActualParams(), rebindings));
            ai2.setTunnelParams(Saxon.Hej.expr.instruct.WithParam.copy(ai2, getTunnelParams(), rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, ai2);
            return ai2;
        }
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Controller controller = context.getController();
            System.Diagnostics.Debug.Assert((controller != null));
            Saxon.Hej.expr.instruct.ParameterSet @params = assembleParams(context, getActualParams());
            Saxon.Hej.expr.instruct.ParameterSet tunnels = assembleTunnelParams(context, getTunnelParams());
            Saxon.Hej.trans.rules.Rule currentTemplateRule = context.getCurrentTemplateRule();
            if ((currentTemplateRule == null)) {
                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("There is no current template rule");
                e.setXPathContext(context);
                e.setErrorCode("XTDE0560");
                e.setLocation(getLocation());
                throw e;
            }
            int min = currentTemplateRule.getMinImportPrecedence();
            int max = (currentTemplateRule.getPrecedence()-1);
            Saxon.Hej.expr.Component.M modeComponent = context.getCurrentMode();
            if ((modeComponent == null)) {
                throw new Saxon.Ejava.lang.AssertionError("Current mode is null");
            }
            Saxon.Hej.om.Item currentItem = context.getCurrentIterator().current();
            Saxon.Hej.trans.Mode mode = modeComponent.getActor();
            Saxon.Hej.trans.rules.Rule rule = mode.getRule(currentItem, min, max, context);
            if ((rule == null)) {
                mode.getBuiltInRuleSet().process(currentItem, @params, tunnels, output, context, getLocation());
            } else {
                Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                Saxon.Hej.expr.instruct.TemplateRule nh = (Saxon.Hej.expr.instruct.TemplateRule)(rule.getAction());
                nh.initialize();
                c2.setOrigin(this);
                c2.setLocalParameters(@params);
                c2.setTunnelParameters(tunnels);
                c2.openStackFrame(nh.getStackFrameMap());
                c2.setCurrentTemplateRule(rule);
                c2.setCurrentComponent(modeComponent);
                c2.setCurrentMergeGroupIterator(null);
                if (mode.isModeTracing()) {
                    Saxon.Hej.trace.TemplateRuleTraceListener tracer = ((Saxon.Hej.trans.XsltController)(controller)).getTemplateRuleTraceListener();
                    tracer.enter("apply-imports", getLocation(), currentItem, nh);
                    nh.apply(output, c2);
                    tracer.leave();
                } else {
                    nh.apply(output, c2);
                }
            }
            return null;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("applyImports", this);
            @out.emitAttribute("flags", "i");
            if ((getActualParams().Length != 0)) {
                Saxon.Hej.expr.instruct.WithParam.exportParameters(getActualParams(), @out, false);
            }
            if ((getTunnelParams().Length != 0)) {
                Saxon.Hej.expr.instruct.WithParam.exportParameters(getTunnelParams(), @out, true);
            }
            @out.endElement();
        }
        public override string getStreamerName() {
            return "ApplyImports";
        }
    
    }
}