using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Compare : Saxon.Hej.functions.CollatingFunctionFixed {
        private static Saxon.Hej.value.Int64Value compare(Saxon.Hej.value.StringValue s1, Saxon.Hej.value.StringValue s2, Saxon.Hej.expr.sort.AtomicComparer comparer) {
            if (((s1 == null) || (s2 == null))) {
                return null;
            }
            int result = comparer.compareAtomicValues(s1, s2);
            if ((result<0)) {
                return Saxon.Hej.value.Int64Value.MINUS_ONE;
            } else {
                if ((result>0)) {
                    return Saxon.Hej.value.Int64Value.PLUS_ONE;
                } else {
                    return Saxon.Hej.value.Int64Value.ZERO;
                }
            }
        }
        public override Saxon.Hej.om.GroundedValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue arg0 = (Saxon.Hej.value.StringValue)(arguments[0].head());
            Saxon.Hej.value.StringValue arg1 = (Saxon.Hej.value.StringValue)(arguments[1].head());
            Saxon.Hej.expr.sort.GenericAtomicComparer comparer = new Saxon.Hej.expr.sort.GenericAtomicComparer(getStringCollator(), context);
            return Saxon.Hej.om.SequenceTool.itemOrEmpty(compare(arg0, arg1, comparer));
        }
    
    }
}