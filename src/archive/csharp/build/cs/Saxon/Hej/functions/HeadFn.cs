using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class HeadFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
            return Saxon.Hej.expr.FirstItemExpression.makeFirstItemExpression(arguments[0]);
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Item head = arguments[0].head();
            return Saxon.Hej.om.SequenceTool.itemOrEmpty(head);
        }
    
    }
}