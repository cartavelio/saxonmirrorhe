using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class Slice : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            if ((arguments[1] is Saxon.Hej.value.IntegerRange)) {
                Saxon.Hej.value.IntegerRange range = (Saxon.Hej.value.IntegerRange)(arguments[1]);
                if ((((range.getStep() == 1L) && (System.Math.Abs(range.getStart())<System.Int32.MaxValue)) && (System.Math.Abs(range.getEnd())<System.Int32.MaxValue))) {
                    int start = (int)(range.getStart());
                    if ((start>0)) {
                        return new Saxon.Hej.om.LazySequence(Saxon.Hej.expr.SubsequenceIterator.make(arguments[0].iterate(), start, ((start-1)+(int)(range.getEnd()))));
                    }
                }
            }
            System.Collections.Generic.IList<Saxon.Hej.om.Item> selected = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
            Saxon.Hej.om.GroundedValue input = arguments[0].materialize();
            int length = input.getLength();
            Saxon.Hej.om.Item index;
            Saxon.Hej.om.SequenceIterator indices = arguments[1].iterate();
            while (((index = indices.next()) != null)) {
                Saxon.Hej.value.IntegerValue value = (Saxon.Hej.value.IntegerValue)(index);
                int sign = value.signum();
                if ((sign == 0)) {
                    continue ;
                }
                int i = value.abs().asSubscript();
                if (((i<0) || (i>length))) {
                    continue ;
                }
                if ((sign<0)) {
                    selected.Add(input.itemAt((length-i)));
                } else {
                    selected.Add(input.itemAt((i-1)));
                }
            }
            return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(selected);
        }
    
    }
}