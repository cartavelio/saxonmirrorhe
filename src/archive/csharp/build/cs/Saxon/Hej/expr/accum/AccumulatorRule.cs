using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.accum {
    public class AccumulatorRule : Saxon.Hej.trans.rules.RuleTarget {
        private readonly Saxon.Hej.expr.Expression newValueExpression;
        private readonly Saxon.Hej.expr.instruct.SlotManager stackFrameMap;
        private readonly bool postDescent;
        private bool capturing;
        public AccumulatorRule(Saxon.Hej.expr.Expression newValueExpression, Saxon.Hej.expr.instruct.SlotManager stackFrameMap, bool postDescent)  {
            this.newValueExpression = newValueExpression;
            this.stackFrameMap = stackFrameMap;
            this.postDescent = postDescent;
        }
        public Saxon.Hej.expr.Expression getNewValueExpression() {
            return newValueExpression;
        }
        public void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            newValueExpression.export(@out);
        }
        public Saxon.Hej.expr.instruct.SlotManager getStackFrameMap() {
            return stackFrameMap;
        }
        public void registerRule(Saxon.Hej.trans.rules.Rule rule) {
        }
        public void setCapturing(bool capturing) {
            this.capturing = capturing;
        }
        public bool isCapturing() {
            return capturing;
        }
        public bool isPostDescent() {
            return postDescent;
        }
    
    }
}