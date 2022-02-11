using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class ConstantFunction : Saxon.Hej.functions.SystemFunction {
        public Saxon.Hej.om.GroundedValue value;
        public ConstantFunction(Saxon.Hej.om.GroundedValue value)  {
            this.value = value;
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return value;
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
            return Saxon.Hej.expr.Literal.makeLiteral(value);
        }
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            if ((value is Saxon.Hej.value.IntegerValue)) {
                return new Saxon.Hej.value.IntegerValue[]{(Saxon.Hej.value.IntegerValue)(value), (Saxon.Hej.value.IntegerValue)(value)};
            } else {
                return null;
            }
        }
        public class True : Saxon.Hej.functions.ConstantFunction {
            public True() : base (Saxon.Hej.value.BooleanValue.TRUE) {
            }
        
        }
        public class False : Saxon.Hej.functions.ConstantFunction {
            public False() : base (Saxon.Hej.value.BooleanValue.FALSE) {
            }
        
        }
    
    }
}