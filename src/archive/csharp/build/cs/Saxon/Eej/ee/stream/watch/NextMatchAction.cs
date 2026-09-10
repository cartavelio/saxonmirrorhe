using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.watch {
    public class NextMatchAction : Saxon.Eej.ee.stream.watch.ApplyTemplatesAction {
        public NextMatchAction(Saxon.Hej.expr.Expression expression, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (expression, result, context) {
        }
        protected internal override Saxon.Hej.trans.rules.Rule getRule(Saxon.Eej.ee.stream.om.FleetingNode node) {
            return ((Saxon.Hej.trans.Mode)(getMode().getActor())).getNextMatchRule(node, getLocalContext().getCurrentTemplateRule(), getContext());
        }
    
    }
}