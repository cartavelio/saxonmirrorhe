using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trace {
    public class XSLTTraceCodeInjector : Saxon.Hej.trace.TraceCodeInjector {
        protected internal override bool isApplicable(Saxon.Hej.expr.Expression exp) {
            return ((exp.isInstruction() || (exp is Saxon.Hej.expr.LetExpression)) || (exp is Saxon.Hej.expr.instruct.FixedAttribute));
        }
    
    }
}