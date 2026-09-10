using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class DecimalDivide : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.NumericValue d1 = (Saxon.Hej.value.NumericValue)(arguments[0].head());
            Saxon.Hej.value.NumericValue d2 = (Saxon.Hej.value.NumericValue)(arguments[1].head());
            if (((d1 == null) || (d2 == null))) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            int scale = (int)(((Saxon.Hej.value.IntegerValue)(arguments[2].head())).longValue());
            return divide(d1, d2, scale);
        }
        private Saxon.Hej.value.NumericValue divide(Saxon.Hej.value.NumericValue d1, Saxon.Hej.value.NumericValue d2, int scale) {
            return new Saxon.Hej.value.BigDecimalValue(Singulink.Numerics.BigDecimal.Divide(d1.getDecimalValue(), d2.getDecimalValue(), scale < 18 ? 18 :scale, Singulink.Numerics.RoundingMode.ToZero)).round(scale);
        }
    
    }
}