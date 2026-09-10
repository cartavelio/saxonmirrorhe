using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class NumericComparisonOperand : Saxon.Eej.ee.trans.Characteristic {
        internal Saxon.Hej.expr.Expression exp;
        public NumericComparisonOperand(Saxon.Hej.expr.Expression exp)  {
            this.exp = exp;
        }
        public override Saxon.Hej.value.NumericValue evaluate(Saxon.Hej.expr.XPathContext context) {
            return (Saxon.Hej.value.NumericValue)(exp.evaluateItem(context));
        }
        public override int GetHashCode() {
            return exp.GetHashCode();
        }
        public override bool Equals(System.Object obj) {
            return ((obj is Saxon.Eej.ee.trans.NumericComparisonOperand) && exp.isEqual(((Saxon.Eej.ee.trans.NumericComparisonOperand)(obj)).exp));
        }
        public override string ToString() {
            return exp.ToString();
        }
    
    }
}