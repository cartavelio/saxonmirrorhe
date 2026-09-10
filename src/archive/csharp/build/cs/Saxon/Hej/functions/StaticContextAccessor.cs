using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public abstract class StaticContextAccessor : Saxon.Hej.functions.SystemFunction {
        public abstract Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.expr.parser.RetainedStaticContext rsc);
        public override Saxon.Hej.value.AtomicValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return evaluate(getRetainedStaticContext());
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
            return Saxon.Hej.expr.Literal.makeLiteral(evaluate(getRetainedStaticContext()));
        }
        public class DefaultCollation : Saxon.Hej.functions.StaticContextAccessor {
            public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.expr.parser.RetainedStaticContext rsc) {
                return new Saxon.Hej.value.StringValue(rsc.getDefaultCollationName());
            }
        
        }
    
    }
}