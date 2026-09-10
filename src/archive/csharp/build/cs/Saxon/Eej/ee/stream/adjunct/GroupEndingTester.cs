using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class GroupEndingTester : Saxon.Eej.ee.stream.adjunct.GroupBoundaryTester {
        private readonly Saxon.Hej.pattern.Pattern pattern;
        private bool foundEndingItem = true;
        public GroupEndingTester(Saxon.Hej.expr.instruct.ForEachGroup expression, Saxon.Hej.expr.XPathContext context) : base (expression, context) {
            pattern = (Saxon.Hej.pattern.Pattern)(expression.getGroupingKey());
        }
        public override bool notifyItem() {
            bool startNew = foundEndingItem;
            foundEndingItem = pattern.matches(getLocalContext().getContextItem(), getLocalContext());
            return startNew;
        }
    
    }
}