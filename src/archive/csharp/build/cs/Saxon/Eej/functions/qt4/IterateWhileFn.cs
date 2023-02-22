using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class IterateWhileFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.GroundedValue input = arguments[0].materialize();
            Saxon.Hej.om.FunctionItem predicate = (Saxon.Hej.om.FunctionItem)(arguments[1].head());
            Saxon.Hej.om.FunctionItem action = (Saxon.Hej.om.FunctionItem)(arguments[2].head());
            while (true) {
                bool condition = ((Saxon.Hej.value.BooleanValue)(dynamicCall(predicate, context, input).head())).getBooleanValue();
                if (condition) {
                    input = dynamicCall(action, context, input).materialize();
                } else {
                    return input;
                }
            }
        }
    
    }
}