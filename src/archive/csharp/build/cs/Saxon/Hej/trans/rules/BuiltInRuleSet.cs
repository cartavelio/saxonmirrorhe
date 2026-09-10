using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.rules {
    public interface BuiltInRuleSet : Saxon.Hej.expr.ContextOriginator {
        void process(Saxon.Hej.om.Item item, Saxon.Hej.expr.instruct.ParameterSet parameters, Saxon.Hej.expr.instruct.ParameterSet tunnelParams, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context, Saxon.Hej.s9api.Location locationId);
        string getName();
        Saxon.Hej.trans.rules.BuiltInRules[] getActionForParentNodes(int nodeKind);
    
    }
}