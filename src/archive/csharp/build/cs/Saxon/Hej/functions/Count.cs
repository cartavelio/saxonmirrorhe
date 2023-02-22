using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Count : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            return new Saxon.Hej.value.IntegerValue[]{Saxon.Hej.value.Int64Value.ZERO, Saxon.Hej.expr.Expression.MAX_SEQUENCE_LENGTH};
        }
        public static int count(Saxon.Hej.om.SequenceIterator iter) {
            if (Saxon.Hej.om.SequenceTool.supportsGetLength(iter)) {
                return Saxon.Hej.om.SequenceTool.getLength(iter);
            } else {
                int n = 0;
                while ((iter.next() != null)) {
                    n++;
                }
                return n;
            }
        }
        public static int steppingCount(Saxon.Hej.om.SequenceIterator iter) {
            int n = 0;
            while ((iter.next() != null)) {
                n++;
            }
            return n;
        }
        public override Saxon.Hej.value.IntegerValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Sequence arg = arguments[0];
            int size = ((arg is Saxon.Hej.om.GroundedValue) ? ((Saxon.Hej.om.GroundedValue)(arg)).getLength() : count(arg.iterate()));
            return Saxon.Hej.value.Int64Value.makeIntegerValue(size);
        }
        public override string getCompilerName() {
            return "CountCompiler";
        }
        public override string getStreamerName() {
            return "Count";
        }
    
    }
}