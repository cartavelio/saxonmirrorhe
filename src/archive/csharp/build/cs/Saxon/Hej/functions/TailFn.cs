using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class TailFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return Saxon.Hej.om.SequenceTool.toLazySequence(Saxon.Hej.expr.TailIterator.make(arguments[0].iterate(), 2));
        }
        public override Saxon.Hej.expr.TailExpression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
            return new Saxon.Hej.expr.TailExpression(arguments[0], 2);
        }
    
    }
}