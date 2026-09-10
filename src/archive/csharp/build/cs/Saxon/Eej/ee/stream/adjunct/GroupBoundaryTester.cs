using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public abstract class GroupBoundaryTester {
        private readonly Saxon.Hej.expr.XPathContext localContext;
        private Saxon.Hej.expr.instruct.ForEachGroup controllingExpression;
        public GroupBoundaryTester(Saxon.Hej.expr.instruct.ForEachGroup expression, Saxon.Hej.expr.XPathContext context)  {
            this.controllingExpression = expression;
            this.localContext = context;
        }
        protected internal Saxon.Hej.expr.XPathContext getLocalContext() {
            return localContext;
        }
        public void setControllingExpression(Saxon.Hej.expr.instruct.ForEachGroup controllingExpression) {
            this.controllingExpression = controllingExpression;
        }
        public Saxon.Hej.expr.instruct.ForEachGroup getControllingExpression() {
            return controllingExpression;
        }
        public abstract bool notifyItem();
    
    }
}