using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Subsequence_2 : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            return arguments[0].getSpecialProperties();
        }
        public override int getCardinality(Saxon.Hej.expr.Expression[] arguments) {
            return (arguments[0].getCardinality()|Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE);
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return Saxon.Hej.om.SequenceTool.toLazySequence(subSequence(arguments[0].iterate(), (Saxon.Hej.value.NumericValue)(arguments[1].head())));
        }
        public static Saxon.Hej.om.SequenceIterator subSequence(Saxon.Hej.om.SequenceIterator seq, Saxon.Hej.value.NumericValue startVal) {
            long lstart;
            if ((startVal is Saxon.Hej.value.Int64Value)) {
                lstart = startVal.longValue();
                if ((lstart<=1)) {
                    return seq;
                }
            } else {
                if (startVal.isNaN()) {
                    return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
                } else {
                    startVal = startVal.round(0);
                    if ((startVal.compareTo(Saxon.Hej.value.Int64Value.PLUS_ONE)<=0)) {
                        return seq;
                    } else {
                        if ((startVal.compareTo(Saxon.Hej.value.Int64Value.MAX_LONG)>0)) {
                            return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
                        } else {
                            lstart = startVal.longValue();
                        }
                    }
                }
            }
            if ((lstart>System.Int32.MaxValue)) {
                return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
            }
            return Saxon.Hej.expr.TailIterator.make(seq, (int)(lstart));
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
            try {
                if ((Saxon.Hej.expr.Literal.isAtomic(arguments[1]) && !(((arguments[0] is Saxon.Hej.expr.ErrorExpression))))) {
                    Saxon.Hej.value.NumericValue start = (Saxon.Hej.value.NumericValue)(((Saxon.Hej.expr.Literal)(arguments[1])).getGroundedValue());
                    start = start.round(0);
                    long intStart = start.longValue();
                    if ((intStart>System.Int32.MaxValue)) {
                        return base.makeFunctionCall(arguments);
                    }
                    if ((intStart<=0)) {
                        return arguments[0];
                    }
                    return new Saxon.Hej.expr.TailExpression(arguments[0], (int)(intStart));
                }
            } catch (System.Exception e) {
            }
            return base.makeFunctionCall(arguments);
        }
        public override string getStreamerName() {
            return "Subsequence";
        }
    
    }
}