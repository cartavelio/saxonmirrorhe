using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trace {
    public class TraceCodeInjector : Saxon.Hej.expr.parser.CodeInjector {
        public Saxon.Hej.expr.Expression inject(Saxon.Hej.expr.Expression exp) {
            if ((!(((exp is Saxon.Hej.expr.instruct.TraceExpression))) && isApplicable(exp))) {
                return new Saxon.Hej.expr.instruct.TraceExpression(exp);
            } else {
                return exp;
            }
        }
        protected internal virtual bool isApplicable(Saxon.Hej.expr.Expression exp) {
            return false;
        }
        public virtual void process(Saxon.Hej.trace.TraceableComponent component) {
            Saxon.Hej.expr.Expression newBody = Saxon.Hej.expr.parser.ExpressionTool.injectCode(component.getBody(), this);
            component.setBody(newBody);
            Saxon.Hej.expr.instruct.ComponentTracer trace = new Saxon.Hej.expr.instruct.ComponentTracer(component);
            component.setBody(trace);
        }
        public Saxon.Hej.expr.flwor.Clause injectClause(Saxon.Hej.expr.flwor.FLWORExpression expression, Saxon.Hej.expr.flwor.Clause clause) {
            return new Saxon.Hej.expr.flwor.TraceClause(expression, clause);
        }
    
    }
}