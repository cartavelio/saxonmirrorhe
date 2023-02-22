using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class AtomicSortComparer : Saxon.Hej.expr.sort.AtomicComparer {
        private Saxon.Hej.lib.StringCollator collator;
        private readonly Saxon.Hej.expr.XPathContext context;
        private readonly int itemType;
        public static Saxon.Hej.expr.sort.AtomicComparer makeSortComparer(Saxon.Hej.lib.StringCollator collator, int itemType, Saxon.Hej.expr.XPathContext context) {
            switch (itemType) {
              case Saxon.Hej.om.StandardNames.XS_STRING:
              case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
              case Saxon.Hej.om.StandardNames.XS_ANY_URI:
                if ((collator is Saxon.Hej.expr.sort.CodepointCollator)) {
                    return Saxon.Hej.expr.sort.CodepointCollatingComparer.getInstance();
                } else {
                    return new Saxon.Hej.expr.sort.CollatingAtomicComparer(collator);
                }
              case Saxon.Hej.om.StandardNames.XS_INTEGER:
              case Saxon.Hej.om.StandardNames.XS_DECIMAL:
                return Saxon.Hej.expr.sort.DecimalSortComparer.getDecimalSortComparerInstance();
              case Saxon.Hej.om.StandardNames.XS_DOUBLE:
              case Saxon.Hej.om.StandardNames.XS_FLOAT:
              case Saxon.Hej.om.StandardNames.XS_NUMERIC:
                return Saxon.Hej.expr.sort.DoubleSortComparer.getInstance();
              case Saxon.Hej.om.StandardNames.XS_DATE_TIME:
              case Saxon.Hej.om.StandardNames.XS_DATE:
              case Saxon.Hej.om.StandardNames.XS_TIME:
                return new Saxon.Hej.expr.sort.CalendarValueComparer(context);
              default:
                return new Saxon.Hej.expr.sort.AtomicSortComparer(collator, itemType, context);
            }
        }
        protected internal AtomicSortComparer(Saxon.Hej.lib.StringCollator collator, int itemType, Saxon.Hej.expr.XPathContext context)  {
            this.collator = collator;
            if ((collator == null)) {
                this.collator = Saxon.Hej.expr.sort.CodepointCollator.getInstance();
            }
            this.context = context;
            this.itemType = itemType;
        }
        public Saxon.Hej.lib.StringCollator getCollator() {
            return collator;
        }
        public Saxon.Hej.expr.sort.AtomicComparer provideContext(Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.sort.AtomicSortComparer(collator, itemType, context);
        }
        public Saxon.Hej.lib.StringCollator getStringCollator() {
            return collator;
        }
        public int getItemType() {
            return itemType;
        }
        public int compareAtomicValues(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            if ((a == null)) {
                if ((b == null)) {
                    return 0;
                } else {
                    return -1;
                }
            } else {
                if ((b == null)) {
                    return +(1);
                }
            }
            if (a.isNaN()) {
                return (b.isNaN() ? 0 : -1);
            } else {
                if (b.isNaN()) {
                    return +(1);
                } else {
                    if (((a is Saxon.Hej.value.StringValue) && (b is Saxon.Hej.value.StringValue))) {
                        return collator.compareStrings(a.getUnicodeStringValue(), b.getUnicodeStringValue());
                    } else {
                        int implicitTimezone = context.getImplicitTimezone();
                        Saxon.Hej.expr.sort.XPathComparable ac = a.getXPathComparable(collator, implicitTimezone);
                        Saxon.Hej.expr.sort.XPathComparable bc = b.getXPathComparable(collator, implicitTimezone);
                        if (((ac == null) || (bc == null))) {
                            return compareNonComparables(a, b);
                        } else {
                            try {
                                return ac.compareTo(bc);
                            } catch (System.InvalidCastException e) {
                                string message = ((("Cannot compare "+a.getPrimitiveType().getDisplayName())+" with ")+b.getPrimitiveType().getDisplayName());
                                if ((a.isUntypedAtomic() || b.isUntypedAtomic())) {
                                    message += ". Further information: see http://saxonica.plan.io/issues/3450";
                                }
                                throw new System.InvalidCastException(message);
                            }
                        }
                    }
                }
            }
        }
        protected internal int compareNonComparables(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((((("Values are not comparable ("+Saxon.Hej.type.Type.displayTypeName(a))+", ")+Saxon.Hej.type.Type.displayTypeName(b))+')'), "XPTY0004");
            throw new Saxon.Hej.expr.sort.ComparisonException(err);
        }
        public bool comparesEqual(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            return (compareAtomicValues(a, b) == 0);
        }
        private class MatchKeyForNaN : Saxon.Hej.expr.sort.AtomicMatchKey {
            public Saxon.Hej.value.AtomicValue asAtomic() {
                return new Saxon.Hej.value.QNameValue("saxon", Saxon.Hej.om.NamespaceUri.of("http://saxon.sf.net/collation-key"), "NaN");
            }
            public int compareTo(Saxon.Hej.expr.sort.AtomicMatchKey o) {
                return Saxon.Hej.om.SequenceTool.INDETERMINATE_ORDERING;
            }
        
        }
        public static Saxon.Hej.expr.sort.AtomicMatchKey COLLATION_KEY_NaN = new Saxon.Hej.expr.sort.AtomicSortComparer.MatchKeyForNaN();
        public string save() {
            return ((("AtSC|"+itemType)+"|")+getCollator().getCollationURI());
        }
    
    }
}