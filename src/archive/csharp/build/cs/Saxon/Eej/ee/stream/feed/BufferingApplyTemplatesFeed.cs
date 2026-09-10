using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class BufferingApplyTemplatesFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        private readonly System.Collections.Generic.IList<Saxon.Hej.om.Item> inputValue = new System.Collections.Generic.List<Saxon.Hej.om.Item>(4);
        private readonly Saxon.Hej.expr.Component.M targetMode;
        public BufferingApplyTemplatesFeed(Saxon.Hej.expr.instruct.ApplyTemplates expr, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (expr, result, context) {
            targetMode = expr.getTargetMode(context);
        }
        public override void append(Saxon.Hej.om.Item item) {
            inputValue.Add(item);
        }
        public override void close() {
            Saxon.Hej.expr.XPathContextMajor c2 = getContext().newContext();
            c2.trackFocus(new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item>(inputValue));
            Saxon.Hej.expr.instruct.ParameterSet actualParams = Saxon.Hej.expr.instruct.Instruction.assembleParams(getContext(), ((Saxon.Hej.expr.instruct.ApplyTemplates)(getExpression())).getActualParams());
            Saxon.Hej.expr.instruct.ParameterSet tunnelParams = Saxon.Hej.expr.instruct.Instruction.assembleTunnelParams(getContext(), ((Saxon.Hej.expr.instruct.ApplyTemplates)(getExpression())).getTunnelParams());
            c2.setCurrentMode(targetMode);
            c2.setCurrentComponent(targetMode);
            c2.setLocalParameters(actualParams);
            c2.setTunnelParameters(tunnelParams);
            c2.setCurrentMergeGroupIterator(null);
            Saxon.Hej.@event.SequenceWriter writer = new Saxon.Eej.ee.stream.StreamingSequenceWriter(getPipelineConfiguration(), getNextOutputter());
            Saxon.Hej.@event.Outputter output = new Saxon.Hej.@event.ComplexContentOutputter(writer);
            Saxon.Hej.trans.Mode mode = targetMode.getActor();
            Saxon.Hej.om.FocusIterator iter = c2.getCurrentIterator();
            for (Saxon.Hej.om.Item item; ((item = iter.next()) != null); ) {
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
}