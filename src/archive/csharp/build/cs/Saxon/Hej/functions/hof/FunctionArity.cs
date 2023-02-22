using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    public class FunctionArity : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            return new Saxon.Hej.value.IntegerValue[]{Saxon.Hej.value.Int64Value.ZERO, Saxon.Hej.value.Int64Value.makeIntegerValue(65535)};
        }
        public override Saxon.Hej.value.IntegerValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Function f = (Saxon.Hej.om.Function)(arguments[0].head());
            return Saxon.Hej.value.Int64Value.makeIntegerValue(f.getArity());
        }
    
    }
}