using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class AtomicEqual : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.AtomicValue op1 = (Saxon.Hej.value.AtomicValue)(arguments[0].head());
            Saxon.Hej.value.AtomicValue op2 = (Saxon.Hej.value.AtomicValue)(arguments[1].head());
            return Saxon.Hej.value.BooleanValue.get(op1.asMapKey().Equals(op2.asMapKey()));
        }
    
    }
}