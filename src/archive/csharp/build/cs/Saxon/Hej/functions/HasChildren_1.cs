using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class HasChildren_1 : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.NodeInfo arg = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
            if ((arg == null)) {
                return Saxon.Hej.value.BooleanValue.FALSE;
            }
            return Saxon.Hej.value.BooleanValue.get(arg.hasChildNodes());
        }
    
    }
}