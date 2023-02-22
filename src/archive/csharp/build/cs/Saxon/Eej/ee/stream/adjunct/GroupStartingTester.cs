using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class GroupStartingTester : Saxon.Eej.ee.stream.adjunct.GroupBoundaryTester {
        private int pos = 0;
        private readonly Saxon.Hej.pattern.Pattern pattern;
        public GroupStartingTester(Saxon.Hej.expr.instruct.ForEachGroup expression, Saxon.Hej.expr.XPathContext context) : base (expression, context) {
            pattern = (Saxon.Hej.pattern.Pattern)(expression.getGroupingKey());
        }
        public override bool notifyItem() {
            return ((pos++ == 0) || pattern.matches(getLocalContext().getContextItem(), getLocalContext()));
        }
    
    }
}