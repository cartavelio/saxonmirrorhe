using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Subsequence_3 : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            return arguments[0].getSpecialProperties();
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return Saxon.Hej.om.SequenceTool.toLazySequence(subSequence(arguments[0].iterate(), (Saxon.Hej.value.NumericValue)(arguments[1].head()), (Saxon.Hej.value.NumericValue)(arguments[2].head()), context));
        }
        public static Saxon.Hej.om.SequenceIterator subSequence(Saxon.Hej.om.SequenceIterator seq, Saxon.Hej.value.NumericValue startVal, Saxon.Hej.value.NumericValue lengthVal, Saxon.Hej.expr.XPathContext context) {
            if (((startVal is Saxon.Hej.value.Int64Value) && (lengthVal is Saxon.Hej.value.Int64Value))) {
                long lstart = startVal.longValue();
                if ((lstart>System.Int32.MaxValue)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
                }
                long llength = lengthVal.longValue();
                if ((llength>System.Int32.MaxValue)) {
                    llength = System.Int32.MaxValue;
                }
                if ((llength<1)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
                }
                long lend = ((lstart+llength)-1);
                if ((lend<1)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
                }
                int start = ((lstart<1) ? 1 : (int)(lstart));
                return Saxon.Hej.expr.SubsequenceIterator.make(seq, start, (int)(lend));
            } else {
                if (startVal.isNaN()) {
                    return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
                }
                if ((startVal.compareTo(Saxon.Hej.value.Int64Value.MAX_LONG)>0)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
                }
                startVal = startVal.round(0);
                if (lengthVal.isNaN()) {
                    return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
                }
                lengthVal = lengthVal.round(0);
                if ((lengthVal.compareTo(Saxon.Hej.value.Int64Value.ZERO)<=0)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
                }
                Saxon.Hej.value.NumericValue rend = (Saxon.Hej.value.NumericValue)(Saxon.Hej.expr.ArithmeticExpression.compute(startVal, Saxon.Hej.expr.Calculator.PLUS, lengthVal, context));
                if (rend.isNaN()) {
                    return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
                }
                rend = (Saxon.Hej.value.NumericValue)(Saxon.Hej.expr.ArithmeticExpression.compute(rend, Saxon.Hej.expr.Calculator.MINUS, Saxon.Hej.value.Int64Value.PLUS_ONE, context));
                if ((rend.compareTo(Saxon.Hej.value.Int64Value.ZERO)<=0)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
                }
                long lstart;
                if ((startVal.compareTo(Saxon.Hej.value.Int64Value.PLUS_ONE)<=0)) {
                    lstart = 1;
                } else {
                    lstart = startVal.longValue();
                }
                if ((lstart>System.Int32.MaxValue)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
                }
                long lend;
                if ((rend.compareTo(Saxon.Hej.value.Int64Value.MAX_LONG)>=0)) {
                    lend = System.Int32.MaxValue;
                } else {
                    lend = rend.longValue();
                }
                return Saxon.Hej.expr.SubsequenceIterator.make(seq, (int)(lstart), (int)(lend));
            }
        }
        public override string getStreamerName() {
            return "Subsequence";
        }
    
    }
}