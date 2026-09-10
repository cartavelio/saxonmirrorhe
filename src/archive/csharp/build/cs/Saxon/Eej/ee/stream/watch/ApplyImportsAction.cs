using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.watch {
    public class ApplyImportsAction : Saxon.Eej.ee.stream.watch.ApplyTemplatesAction {
        public ApplyImportsAction(Saxon.Hej.expr.Expression expression, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (expression, result, context) {
        }
        protected internal override Saxon.Hej.trans.rules.Rule getRule(Saxon.Eej.ee.stream.om.FleetingNode node) {
            Saxon.Hej.trans.rules.Rule currentRule = getLocalContext().getCurrentTemplateRule();
            int min = currentRule.getMinImportPrecedence();
            int max = (currentRule.getPrecedence()-1);
            return ((Saxon.Hej.trans.Mode)(getMode().getActor())).getRule(node, min, max, getContext());
        }
    
    }
}