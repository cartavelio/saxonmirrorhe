using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.rules {
    public class ShallowSkipRuleSet : Saxon.Hej.trans.rules.BuiltInRuleSet {
        private static readonly Saxon.Hej.trans.rules.ShallowSkipRuleSet THE_INSTANCE = new Saxon.Hej.trans.rules.ShallowSkipRuleSet();
        public static Saxon.Hej.trans.rules.ShallowSkipRuleSet getInstance() {
            return THE_INSTANCE;
        }
        protected internal ShallowSkipRuleSet()  {
        }
        public virtual void process(Saxon.Hej.om.Item item, Saxon.Hej.expr.instruct.ParameterSet parameters, Saxon.Hej.expr.instruct.ParameterSet tunnelParams, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context, Saxon.Hej.s9api.Location locationId) {
            if ((item is Saxon.Hej.om.NodeInfo)) {
                Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                switch (node.getNodeKind()) {
                  case Saxon.Hej.type.Type.ELEMENT:
                    {
                        Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                        c2.setOrigin(this);
                        c2.trackFocus(node.iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE));
                        c2.setCurrentComponent(c2.getCurrentMode());
                        Saxon.Hej.expr.instruct.TailCall tc = c2.getCurrentMode().getActor().applyTemplates(parameters, tunnelParams, null, output, c2, locationId);
                        while ((tc != null)) {
                            tc = tc.processLeavingTail();
                        }
                        goto case Saxon.Hej.type.Type.DOCUMENT;
                    }
                  case Saxon.Hej.type.Type.DOCUMENT:
                    {
                        Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                        c2.setOrigin(this);
                        c2.trackFocus(node.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD));
                        c2.setCurrentComponent(c2.getCurrentMode());
                        Saxon.Hej.expr.instruct.TailCall tc = c2.getCurrentMode().getActor().applyTemplates(parameters, tunnelParams, null, output, c2, locationId);
                        while ((tc != null)) {
                            tc = tc.processLeavingTail();
                        }
                        return ;
                    }
                  case Saxon.Hej.type.Type.TEXT:
                  case Saxon.Hej.type.Type.ATTRIBUTE:
                  case Saxon.Hej.type.Type.COMMENT:
                  case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                  case Saxon.Hej.type.Type.NAMESPACE:
                    break;
                }
            } else {
                if ((item is Saxon.Hej.ma.arrays.ArrayItem)) {
                    Saxon.Hej.om.Sequence seq = Saxon.Hej.ma.arrays.ArrayFunctionSet.ArrayToSequence.toSequence((Saxon.Hej.ma.arrays.ArrayItem)(item));
                    Saxon.Hej.om.SequenceIterator members = seq.iterate();
                    Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                    c2.setOrigin(this);
                    c2.trackFocus(members);
                    c2.setCurrentComponent(c2.getCurrentMode());
                    Saxon.Hej.expr.instruct.TailCall tc = c2.getCurrentMode().getActor().applyTemplates(parameters, tunnelParams, null, output, c2, locationId);
                    while ((tc != null)) {
                        tc = tc.processLeavingTail();
                    }
                } else {
                }
            }
        }
        public virtual string getName() {
            return "shallow-skip";
        }
        public Saxon.Hej.trans.rules.BuiltInRules[] getActionForParentNodes(int nodeKind) {
            return new Saxon.Hej.trans.rules.BuiltInRules[]{Saxon.Hej.trans.rules.BuiltInRules.APPLY_TEMPLATES_TO_ATTRIBUTES, Saxon.Hej.trans.rules.BuiltInRules.APPLY_TEMPLATES_TO_CHILDREN};
        }
    
    }
}