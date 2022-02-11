using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class ApplyTemplatesFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        private readonly Saxon.Hej.expr.XPathContext localContext;
        private readonly Saxon.Hej.tree.iter.ManualIterator manual;
        private readonly Saxon.Hej.expr.Component.M targetMode;
        public ApplyTemplatesFeed(Saxon.Hej.expr.instruct.ApplyTemplates expr, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (expr, result, context) {
            localContext = context.newMinorContext();
            manual = new Saxon.Hej.tree.iter.ManualIterator();
            localContext.setCurrentIterator(manual);
            targetMode = expr.getTargetMode(context);
        }
        public override void append(Saxon.Hej.om.Item item) {
            manual.setContextItem(item);
            manual.incrementPosition();
            Saxon.Hej.expr.instruct.ParameterSet actualParams = Saxon.Hej.expr.instruct.Instruction.assembleParams(localContext, ((Saxon.Hej.expr.instruct.ApplyTemplates)(getExpression())).getActualParams());
            Saxon.Hej.expr.instruct.ParameterSet tunnelParams = Saxon.Hej.expr.instruct.Instruction.assembleTunnelParams(localContext, ((Saxon.Hej.expr.instruct.ApplyTemplates)(getExpression())).getTunnelParams());
            Saxon.Hej.expr.XPathContextMajor c2 = localContext.newContext();
            c2.setCurrentIterator(manual);
            c2.setCurrentMode(targetMode);
            c2.setCurrentComponent(targetMode);
            c2.setLocalParameters(actualParams);
            c2.setTunnelParameters(tunnelParams);
            c2.setCurrentMergeGroupIterator(null);
            Saxon.Hej.@event.SequenceWriter writer = new Saxon.Eej.ee.stream.StreamingSequenceWriter(getPipelineConfiguration(), getNextOutputter());
            Saxon.Hej.@event.Outputter output = new Saxon.Hej.@event.ComplexContentOutputter(writer);
            Saxon.Hej.trans.Mode mode = targetMode.getActor();
            Saxon.Hej.trans.rules.Rule rule = mode.getRule(item, c2);
            if ((rule == null)) {
                mode.getBuiltInRuleSet().process(item, actualParams, tunnelParams, output, c2, Saxon.Hej.expr.parser.Loc.NONE);
            } else {
                Saxon.Hej.expr.instruct.TemplateRule template = (Saxon.Hej.expr.instruct.TemplateRule)(rule.getAction());
                template.initialize();
                c2.openStackFrame(template.getStackFrameMap());
                c2.setCurrentTemplateRule(rule);
                template.apply(output, c2);
            }
        }
    
    }
}