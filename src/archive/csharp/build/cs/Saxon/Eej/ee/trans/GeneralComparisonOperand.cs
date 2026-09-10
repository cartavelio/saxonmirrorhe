using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class GeneralComparisonOperand : Saxon.Eej.ee.trans.Characteristic {
        internal Saxon.Hej.expr.Expression exp;
        internal bool multiValued;
        internal Saxon.Hej.lib.StringCollator collator;
        public GeneralComparisonOperand(Saxon.Hej.lib.StringCollator collator, Saxon.Hej.expr.Expression exp)  {
            this.collator = collator;
            this.exp = exp;
            this.multiValued = Saxon.Hej.value.Cardinality.allowsMany(exp.getCardinality());
        }
        public override Saxon.Hej.z.IntSet evaluate(Saxon.Hej.expr.XPathContext context) {
            if (multiValued) {
                Saxon.Hej.om.SequenceIterator iter = exp.iterate(context);
                Saxon.Hej.z.IntHashSet keys = new Saxon.Hej.z.IntHashSet();
                Saxon.Hej.om.SequenceTool.supply(iter, (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((item => {
                    keys.add(collator.getCollationKey(item.getUnicodeStringValue()).GetHashCode());
                } )));
                return keys;
            } else {
                Saxon.Hej.om.Item item = exp.evaluateItem(context);
                if ((item == null)) {
                    return Saxon.Hej.z.IntEmptySet.getInstance();
                } else {
                    return new Saxon.Hej.z.IntSingletonSet(collator.getCollationKey(item.getUnicodeStringValue()).GetHashCode());
                }
            }
        }
        public override int GetHashCode() {
            return exp.GetHashCode();
        }
        public override bool Equals(System.Object obj) {
            return (((obj is Saxon.Eej.ee.trans.GeneralComparisonOperand) && exp.isEqual(((Saxon.Eej.ee.trans.GeneralComparisonOperand)(obj)).exp)) && collator.Equals(((Saxon.Eej.ee.trans.GeneralComparisonOperand)(obj)).collator));
        }
        public override string ToString() {
            return exp.ToString();
        }
    
    }
}