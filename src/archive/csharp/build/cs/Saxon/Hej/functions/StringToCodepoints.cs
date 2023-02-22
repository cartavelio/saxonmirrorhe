using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class StringToCodepoints : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            return new Saxon.Hej.value.IntegerValue[]{Saxon.Hej.value.Int64Value.PLUS_ONE, Saxon.Hej.value.Int64Value.makeIntegerValue(1114111)};
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue val = (Saxon.Hej.value.StringValue)(arguments[0].head());
            if ((val == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            return Saxon.Hej.om.SequenceTool.toLazySequence(val.iterateCharacters());
        }
    
    }
}