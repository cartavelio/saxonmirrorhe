using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public sealed class RoundHalfToEven : Saxon.Hej.functions.SystemFunction {
        public override int getCardinality(Saxon.Hej.expr.Expression[] arguments) {
            return arguments[0].getCardinality();
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.NumericValue val0 = (Saxon.Hej.value.NumericValue)(arguments[0].head());
            if ((val0 == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            int scale = 0;
            if ((arguments.Length == 2)) {
                Saxon.Hej.value.NumericValue scaleVal = (Saxon.Hej.value.NumericValue)(arguments[1].head());
                if ((scaleVal.compareTo(System.Int32.MaxValue)>0)) {
                    return val0;
                } else {
                    if ((scaleVal.compareTo(System.Int32.MinValue)<0)) {
                        scale = System.Int32.MinValue;
                    } else {
                        scale = (int)(scaleVal.longValue());
                    }
                }
            }
            return val0.roundHalfToEven(scale);
        }
        public override string getCompilerName() {
            return "RoundingCompiler";
        }
    
    }
}