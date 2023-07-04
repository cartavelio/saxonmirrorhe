using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class Slice : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return slice(arguments);
        }
        public static Saxon.Hej.om.GroundedValue slice(Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.GroundedValue input = arguments[0].materialize();
            int length = input.getLength();
            if ((length == 0)) {
                return input;
            }
            Saxon.Hej.value.IntegerValue start = (Saxon.Hej.value.IntegerValue)(arguments[1].head());
            Saxon.Hej.value.IntegerValue end = (Saxon.Hej.value.IntegerValue)(arguments[2].head());
            Saxon.Hej.value.IntegerValue step = (Saxon.Hej.value.IntegerValue)(arguments[3].head());
            int s;
            if ((start == null)) {
                s = 1;
            } else {
                int startVal = (int)(start.longValue());
                if ((startVal == 0)) {
                    s = 1;
                } else {
                    if ((startVal<0)) {
                        s = ((length+startVal)+1);
                    } else {
                        s = startVal;
                    }
                }
            }
            int e;
            if ((end == null)) {
                e = length;
            } else {
                int endVal = (int)(end.longValue());
                if ((endVal == 0)) {
                    e = length;
                } else {
                    if ((endVal<0)) {
                        e = ((length+endVal)+1);
                    } else {
                        e = endVal;
                    }
                }
            }
            int st = 1;
            int stepVal = ((step == null) ? 0 : (int)(step.longValue()));
            if ((stepVal == 0)) {
                st = ((e>=s) ? +(1) : -1);
            } else {
                st = stepVal;
            }
            if ((st<0)) {
                Saxon.Hej.value.SequenceExtent reversed = Saxon.Hej.value.SequenceExtent.from(Saxon.Hej.functions.Reverse.getReverseIterator(input.iterate()));
                return positiveSlice(reversed, ((length-s)+1), ((length-e)+1), -(st));
            } else {
                return positiveSlice(input, s, e, st);
            }
        }
        private static Saxon.Hej.om.GroundedValue positiveSlice(Saxon.Hej.om.GroundedValue input, int start, int end, int step) {
            System.Collections.Generic.IList<Saxon.Hej.om.Item> result = new System.Collections.Generic.List<Saxon.Hej.om.Item>(input.getLength());
            for (int i = 0; (i<input.getLength()); i++) {
                int pos = (i+1);
                if ((((pos>=start) && (pos<=end)) && ((((pos-start))%step) == 0))) {
                    result.Add(input.itemAt(i));
                }
            }
            return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(result);
        }
    
    }
}