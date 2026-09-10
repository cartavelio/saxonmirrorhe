using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class CompileQuery : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.value.ObjectValue<Saxon.Hej.query.XQueryExpression> call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            string query = arguments[0].head().getStringValue();
            Saxon.Hej.query.StaticQueryContext sqc = context.getConfiguration().newStaticQueryContext();
            Saxon.Hej.query.XQueryExpression exp = sqc.compileQuery(query);
            return new Saxon.Hej.value.ObjectValue<Saxon.Hej.query.XQueryExpression>(exp);
        }
    
    }
}