using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class EquivalenceComparer : Saxon.Hej.expr.sort.GenericAtomicComparer {
        protected internal EquivalenceComparer(Saxon.Hej.lib.StringCollator collator, Saxon.Hej.expr.XPathContext context) : base (collator, context) {
        }
        public override Saxon.Hej.expr.EquivalenceComparer provideContext(Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.EquivalenceComparer(getStringCollator(), context);
        }
        public override bool comparesEqual(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            if (((a is Saxon.Hej.value.StringValue) && (b is Saxon.Hej.value.StringValue))) {
                return getStringCollator().comparesEqual(a.getUnicodeStringValue(), b.getUnicodeStringValue());
            } else {
                if (((a is Saxon.Hej.value.CalendarValue) && (b is Saxon.Hej.value.CalendarValue))) {
                    return (((Saxon.Hej.value.CalendarValue)(a)).compareTo((Saxon.Hej.value.CalendarValue)(b), getContext().getImplicitTimezone()) == 0);
                } else {
                    if ((a.isNaN() && b.isNaN())) {
                        return true;
                    } else {
                        int implicitTimezone = getContext().getImplicitTimezone();
                        System.Object ac = a.getXPathMatchKey(getStringCollator(), implicitTimezone);
                        System.Object bc = b.getXPathMatchKey(getStringCollator(), implicitTimezone);
                        return ac.Equals(bc);
                    }
                }
            }
        }
        public override string save() {
            return ("EQUIV|"+base.save());
        }
    
    }
}