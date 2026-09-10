using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class IsNaN : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return Saxon.Hej.value.BooleanValue.get(((Saxon.Hej.value.AtomicValue)(arguments[0].head())).isNaN());
        }
    
    }
}