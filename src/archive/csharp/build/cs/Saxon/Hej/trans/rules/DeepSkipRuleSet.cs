using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.rules {
    public class DeepSkipRuleSet : Saxon.Hej.trans.rules.BuiltInRuleSet {
        private static readonly Saxon.Hej.trans.rules.DeepSkipRuleSet THE_INSTANCE = new Saxon.Hej.trans.rules.DeepSkipRuleSet();
        public static Saxon.Hej.trans.rules.DeepSkipRuleSet getInstance() {
            return THE_INSTANCE;
        }
        private DeepSkipRuleSet()  {
        }
        public void process(Saxon.Hej.om.Item item, Saxon.Hej.expr.instruct.ParameterSet parameters, Saxon.Hej.expr.instruct.ParameterSet tunnelParams, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context, Saxon.Hej.s9api.Location locationId) {
            if (((item is Saxon.Hej.om.NodeInfo) && (((Saxon.Hej.om.NodeInfo)(item)).getNodeKind() == Saxon.Hej.type.Type.DOCUMENT))) {
                Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                c2.setOrigin(this);
                c2.trackFocus(((Saxon.Hej.om.NodeInfo)(item)).iterateAxis(Saxon.Hej.om.AxisInfo.CHILD));
                c2.setCurrentComponent(c2.getCurrentMode());
                Saxon.Hej.expr.instruct.TailCall tc = c2.getCurrentMode().getActor().applyTemplates(parameters, tunnelParams, null, output, c2, locationId);
                while ((tc != null)) {
                    tc = tc.processLeavingTail();
                }
            }
        }
        public string getName() {
            return "deep-skip";
        }
        public Saxon.Hej.trans.rules.BuiltInRules[] getActionForParentNodes(int nodeKind) {
            if ((nodeKind == Saxon.Hej.type.Type.DOCUMENT)) {
                return new Saxon.Hej.trans.rules.BuiltInRules[]{Saxon.Hej.trans.rules.BuiltInRules.APPLY_TEMPLATES_TO_CHILDREN};
            } else {
                return new Saxon.Hej.trans.rules.BuiltInRules[]{Saxon.Hej.trans.rules.BuiltInRules.DEEP_SKIP};
            }
        }
    
    }
}