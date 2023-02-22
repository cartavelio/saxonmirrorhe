using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.rules {
    public class RuleSetWithWarnings : Saxon.Hej.trans.rules.BuiltInRuleSet {
        private readonly Saxon.Hej.trans.rules.BuiltInRuleSet baseRuleSet;
        public RuleSetWithWarnings(Saxon.Hej.trans.rules.BuiltInRuleSet baseRuleSet)  {
            this.baseRuleSet = baseRuleSet;
        }
        public Saxon.Hej.trans.rules.BuiltInRuleSet getBaseRuleSet() {
            return baseRuleSet;
        }
        public void process(Saxon.Hej.om.Item item, Saxon.Hej.expr.instruct.ParameterSet parameters, Saxon.Hej.expr.instruct.ParameterSet tunnelParams, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context, Saxon.Hej.s9api.Location locationId) {
            outputWarning(item, context);
            baseRuleSet.process(item, parameters, tunnelParams, output, context, locationId);
        }
        public string getName() {
            return (baseRuleSet+" with warnings");
        }
        public void outputWarning(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            string id = ((item is Saxon.Hej.om.NodeInfo) ? ("the node "+Saxon.Hej.tree.util.Navigator.getPath((Saxon.Hej.om.NodeInfo)(item))) : ("the atomic value "+item.getUnicodeStringValue()));
            Saxon.Hej.trans.XmlProcessingIncident warning = ((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident(("No user-defined template rule matches "+id), "XTDE0555").asWarning());
            context.getController().getErrorReporter().report(warning);
        }
        public Saxon.Hej.trans.rules.BuiltInRules[] getActionForParentNodes(int nodeKind) {
            return baseRuleSet.getActionForParentNodes(nodeKind);
        }
    
    }
}