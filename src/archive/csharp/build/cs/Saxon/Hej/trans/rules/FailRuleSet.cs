using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.rules {
    public class FailRuleSet : Saxon.Hej.trans.rules.BuiltInRuleSet {
        private static readonly Saxon.Hej.trans.rules.FailRuleSet THE_INSTANCE = new Saxon.Hej.trans.rules.FailRuleSet();
        public static Saxon.Hej.trans.rules.FailRuleSet getInstance() {
            return THE_INSTANCE;
        }
        private FailRuleSet()  {
        }
        public void process(Saxon.Hej.om.Item item, Saxon.Hej.expr.instruct.ParameterSet parameters, Saxon.Hej.expr.instruct.ParameterSet tunnelParams, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context, Saxon.Hej.s9api.Location locationId) {
            string id = (((item is Saxon.Hej.om.NodeInfo) ? ("the node "+Saxon.Hej.tree.util.Navigator.getPath((Saxon.Hej.om.NodeInfo)(item))) : ("the atomic value "+item.getUnicodeStringValue())));
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((("No user-defined template rule in "+context.getCurrentMode().getActor().getModeTitle(false))+" matches ")+id), "XTDE0555");
            err.setLocator(locationId.saveLocation());
            throw err;
        }
        public string getName() {
            return "fail";
        }
        public Saxon.Hej.trans.rules.BuiltInRules[] getActionForParentNodes(int nodeKind) {
            return new Saxon.Hej.trans.rules.BuiltInRules[]{Saxon.Hej.trans.rules.BuiltInRules.FAIL};
        }
    
    }
}