using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class GenericAtomicComparer : Saxon.Hej.expr.sort.AtomicComparer {
        private Saxon.Hej.lib.StringCollator collator;
        private readonly Saxon.Hej.expr.XPathContext context;
        public GenericAtomicComparer(Saxon.Hej.lib.StringCollator collator, Saxon.Hej.expr.XPathContext conversionContext)  {
            this.collator = collator;
            if ((collator == null)) {
                this.collator = Saxon.Hej.expr.sort.CodepointCollator.getInstance();
            }
            context = conversionContext;
        }
        public static Saxon.Hej.expr.sort.AtomicComparer makeAtomicComparer(Saxon.Hej.type.BuiltInAtomicType type0, Saxon.Hej.type.BuiltInAtomicType type1, Saxon.Hej.lib.StringCollator collator, Saxon.Hej.expr.XPathContext context) {
            int fp0 = type0.getFingerprint();
            int fp1 = type1.getFingerprint();
            if ((fp0 == fp1)) {
                switch (fp0) {
                  case Saxon.Hej.om.StandardNames.XS_DATE_TIME:
                  case Saxon.Hej.om.StandardNames.XS_DATE:
                  case Saxon.Hej.om.StandardNames.XS_TIME:
                  case Saxon.Hej.om.StandardNames.XS_G_DAY:
                  case Saxon.Hej.om.StandardNames.XS_G_MONTH:
                  case Saxon.Hej.om.StandardNames.XS_G_YEAR:
                  case Saxon.Hej.om.StandardNames.XS_G_MONTH_DAY:
                  case Saxon.Hej.om.StandardNames.XS_G_YEAR_MONTH:
                    return new Saxon.Hej.expr.sort.CalendarValueComparer(context);
                  case Saxon.Hej.om.StandardNames.XS_BOOLEAN:
                  case Saxon.Hej.om.StandardNames.XS_INTEGER:
                  case Saxon.Hej.om.StandardNames.XS_DECIMAL:
                  case Saxon.Hej.om.StandardNames.XS_DOUBLE:
                  case Saxon.Hej.om.StandardNames.XS_FLOAT:
                  case Saxon.Hej.om.StandardNames.XS_DAY_TIME_DURATION:
                  case Saxon.Hej.om.StandardNames.XS_YEAR_MONTH_DURATION:
                  case Saxon.Hej.om.StandardNames.XS_BASE64_BINARY:
                  case Saxon.Hej.om.StandardNames.XS_HEX_BINARY:
                    return Saxon.Hej.expr.sort.ContextFreeAtomicComparer.getInstance();
                  case Saxon.Hej.om.StandardNames.XS_QNAME:
                  case Saxon.Hej.om.StandardNames.XS_NOTATION:
                    return Saxon.Hej.expr.sort.EqualityComparer.getInstance();
                }
            }
            if ((type0.isPrimitiveNumeric() && type1.isPrimitiveNumeric())) {
                return Saxon.Hej.expr.sort.ContextFreeAtomicComparer.getInstance();
            }
            if ((((((fp0 == Saxon.Hej.om.StandardNames.XS_STRING) || (fp0 == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC)) || (fp0 == Saxon.Hej.om.StandardNames.XS_ANY_URI))) && ((((fp1 == Saxon.Hej.om.StandardNames.XS_STRING) || (fp1 == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC)) || (fp1 == Saxon.Hej.om.StandardNames.XS_ANY_URI))))) {
                if ((collator is Saxon.Hej.expr.sort.CodepointCollator)) {
                    return Saxon.Hej.expr.sort.CodepointCollatingComparer.getInstance();
                } else {
                    return new Saxon.Hej.expr.sort.CollatingAtomicComparer(collator);
                }
            }
            return new Saxon.Hej.expr.sort.GenericAtomicComparer(collator, context);
        }
        public delegate bool AtomicComparisonFunction (Saxon.Hej.value.AtomicValue v0, Saxon.Hej.value.AtomicValue v1, Saxon.Hej.expr.XPathContext context);
        private static Saxon.Hej.expr.sort.GenericAtomicComparer.AtomicComparisonFunction getContextFreeComparisonFunction(int @operator) {
            return (a, b, context) => {
                int comp = ((Saxon.Hej.expr.sort.XPathComparable)(a)).compareTo((Saxon.Hej.expr.sort.XPathComparable)(b));
                return Saxon.Hej.expr.CompareToConstant.interpretComparisonResult(@operator, comp);
            } ;
        }
        private static Saxon.Hej.expr.sort.GenericAtomicComparer.AtomicComparisonFunction getFloatingPointComparisonFunction(int @operator) {
            return (a, b, context) => {
                if ((a.isNaN() || b.isNaN())) {
                    return (@operator == Saxon.Hej.expr.parser.Token.FNE);
                }
                int comp = ((Saxon.Hej.expr.sort.XPathComparable)(a)).compareTo((Saxon.Hej.expr.sort.XPathComparable)(b));
                return Saxon.Hej.expr.CompareToConstant.interpretComparisonResult(@operator, comp);
            } ;
        }
        private static int applyPromotion(Saxon.Hej.type.BuiltInAtomicType type) {
            if (type.isPrimitiveNumeric()) {
                return Saxon.Hej.om.StandardNames.XS_DOUBLE;
            }
            int fp = type.getFingerprint();
            if (((fp == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC) || (fp == Saxon.Hej.om.StandardNames.XS_ANY_URI))) {
                return Saxon.Hej.om.StandardNames.XS_STRING;
            } else {
                return fp;
            }
        }
        public static Saxon.Hej.expr.sort.GenericAtomicComparer.AtomicComparisonFunction makeAtomicComparisonFunction(Saxon.Hej.type.BuiltInAtomicType type0, Saxon.Hej.type.BuiltInAtomicType type1, Saxon.Hej.lib.StringCollator collator, int @operator, bool allowRecursion) {
            int fp0 = applyPromotion(type0);
            int fp1 = applyPromotion(type1);
            if ((fp0 == fp1)) {
                switch (fp0) {
                  case Saxon.Hej.om.StandardNames.XS_DATE_TIME:
                  case Saxon.Hej.om.StandardNames.XS_DATE:
                  case Saxon.Hej.om.StandardNames.XS_TIME:
                  case Saxon.Hej.om.StandardNames.XS_G_DAY:
                  case Saxon.Hej.om.StandardNames.XS_G_MONTH:
                  case Saxon.Hej.om.StandardNames.XS_G_YEAR:
                  case Saxon.Hej.om.StandardNames.XS_G_MONTH_DAY:
                  case Saxon.Hej.om.StandardNames.XS_G_YEAR_MONTH:
                    return (a, b, context) => {
                        int comp = ((Saxon.Hej.value.CalendarValue)(a)).compareTo((Saxon.Hej.value.CalendarValue)(b), context.getImplicitTimezone());
                        return Saxon.Hej.expr.CompareToConstant.interpretComparisonResult(@operator, comp);
                    } ;
                  case Saxon.Hej.om.StandardNames.XS_DOUBLE:
                  case Saxon.Hej.om.StandardNames.XS_FLOAT:
                    return getFloatingPointComparisonFunction(@operator);
                  case Saxon.Hej.om.StandardNames.XS_BOOLEAN:
                  case Saxon.Hej.om.StandardNames.XS_INTEGER:
                  case Saxon.Hej.om.StandardNames.XS_DECIMAL:
                  case Saxon.Hej.om.StandardNames.XS_DAY_TIME_DURATION:
                  case Saxon.Hej.om.StandardNames.XS_YEAR_MONTH_DURATION:
                  case Saxon.Hej.om.StandardNames.XS_BASE64_BINARY:
                  case Saxon.Hej.om.StandardNames.XS_HEX_BINARY:
                    return getContextFreeComparisonFunction(@operator);
                  case Saxon.Hej.om.StandardNames.XS_QNAME:
                  case Saxon.Hej.om.StandardNames.XS_NOTATION:
                    switch (@operator) {
                      case Saxon.Hej.expr.parser.Token.FEQ:
                        return (a, b, context) => a.Equals(b);
                      case Saxon.Hej.expr.parser.Token.FNE:
                        return (a, b, context) => !(a.Equals(b));
                      default:
                        return (a, b, context) => {
                            throw new Saxon.Hej.trans.XPathException((type0+" values cannot be compared for ordering"), "XPTY0004");
                        } ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    if (((collator is Saxon.Hej.expr.sort.CodepointCollator) && (@operator == Saxon.Hej.expr.parser.Token.FEQ))) {
                        return (a, b, context) => a.Equals(b);
                    }
                    if (((collator is Saxon.Hej.expr.sort.CodepointCollator) && (@operator == Saxon.Hej.expr.parser.Token.FNE))) {
                        return (a, b, context) => !(a.Equals(b));
                    } else {
                        return (a, b, context) => {
                            int comp = collator.compareStrings(a.getUnicodeStringValue(), b.getUnicodeStringValue());
                            return Saxon.Hej.expr.CompareToConstant.interpretComparisonResult(@operator, comp);
                        } ;
                    }
                }
            }
            if ((type0.isDurationType() && type1.isDurationType())) {
                switch (@operator) {
                  case Saxon.Hej.expr.parser.Token.FEQ:
                    return (a, b, context) => a.Equals(b);
                  case Saxon.Hej.expr.parser.Token.FNE:
                    return (a, b, context) => !(a.Equals(b));
                  default:
                    break ;
                }
            }
            if (allowRecursion) {
                Saxon.Hej.type.BuiltInAtomicType[] firstTimeTypes = new Saxon.Hej.type.BuiltInAtomicType[2];
                Saxon.Hej.expr.sort.GenericAtomicComparer.AtomicComparisonFunction[] firstTimeFunction = new Saxon.Hej.expr.sort.GenericAtomicComparer.AtomicComparisonFunction[1];
                return (a, b, context) => {
                    Saxon.Hej.type.BuiltInAtomicType at = a.getPrimitiveType();
                    Saxon.Hej.type.BuiltInAtomicType bt = b.getPrimitiveType();
                    lock (firstTimeFunction) {
                        if ((firstTimeFunction[0] == null)) {
                            Saxon.Hej.expr.sort.GenericAtomicComparer.AtomicComparisonFunction comparisonFunction = makeAtomicComparisonFunction(at, bt, collator, @operator, false);
                            firstTimeFunction[0] = comparisonFunction;
                            firstTimeTypes[0] = at;
                            firstTimeTypes[1] = bt;
                            return /*Delegate.compare()*/comparisonFunction(a, b, context);
                        } else {
                            if (((firstTimeTypes[0] == at) && (firstTimeTypes[1] == bt))) {
                                return /*Delegate.compare()*/firstTimeFunction[0](a, b, context);
                            } else {
                                Saxon.Hej.expr.sort.GenericAtomicComparer.AtomicComparisonFunction comparisonFunction = makeAtomicComparisonFunction(at, bt, collator, @operator, false);
                                return /*Delegate.compare()*/comparisonFunction(a, b, context);
                            }
                        }
                    }
                } ;
            } else {
                return (a, b, context) => {
                    throw new Saxon.Hej.trans.XPathException((((("Values are not comparable ("+Saxon.Hej.type.Type.displayTypeName(a))+", ")+Saxon.Hej.type.Type.displayTypeName(b))+')'), "XPTY0004", context);
                } ;
            }
        }
        public Saxon.Hej.lib.StringCollator getCollator() {
            return collator;
        }
        public virtual Saxon.Hej.expr.sort.AtomicComparer provideContext(Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.sort.GenericAtomicComparer(collator, context);
        }
        public Saxon.Hej.lib.StringCollator getStringCollator() {
            return collator;
        }
        public int compareAtomicValues(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            if ((a == null)) {
                return ((b == null) ? 0 : -1);
            } else {
                if ((b == null)) {
                    return +(1);
                }
            }
            if (((a is Saxon.Hej.value.StringValue) && (b is Saxon.Hej.value.StringValue))) {
                return collator.compareStrings(a.getUnicodeStringValue(), b.getUnicodeStringValue());
            } else {
                int implicitTimezone = context.getImplicitTimezone();
                Saxon.Hej.expr.sort.XPathComparable ac = a.getXPathComparable(collator, implicitTimezone);
                Saxon.Hej.expr.sort.XPathComparable bc = b.getXPathComparable(collator, implicitTimezone);
                if (((ac == null) || (bc == null))) {
                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException((((("Objects are not comparable ("+Saxon.Hej.type.Type.displayTypeName(a))+", ")+Saxon.Hej.type.Type.displayTypeName(b))+')'), "XPTY0004");
                    throw new Saxon.Hej.expr.sort.ComparisonException(e);
                } else {
                    return ac.compareTo(bc);
                }
            }
        }
        public virtual bool comparesEqual(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            if (((a is Saxon.Hej.value.StringValue) && (b is Saxon.Hej.value.StringValue))) {
                return collator.comparesEqual(a.getUnicodeStringValue(), b.getUnicodeStringValue());
            } else {
                if (((a is Saxon.Hej.value.CalendarValue) && (b is Saxon.Hej.value.CalendarValue))) {
                    return (((Saxon.Hej.value.CalendarValue)(a)).compareTo((Saxon.Hej.value.CalendarValue)(b), context.getImplicitTimezone()) == 0);
                } else {
                    int implicitTimezone = context.getImplicitTimezone();
                    Saxon.Hej.expr.sort.AtomicMatchKey ac = a.getXPathMatchKey(collator, implicitTimezone);
                    Saxon.Hej.expr.sort.AtomicMatchKey bc = b.getXPathMatchKey(collator, implicitTimezone);
                    return ac.Equals(bc);
                }
            }
        }
        public Saxon.Hej.expr.XPathContext getContext() {
            return context;
        }
        public virtual string save() {
            return ("GAC|"+collator.getCollationURI());
        }
        public override int GetHashCode() {
            return collator.GetHashCode();
        }
        public override bool Equals(System.Object obj) {
            return ((obj is Saxon.Hej.expr.sort.GenericAtomicComparer) && collator.Equals(((Saxon.Hej.expr.sort.GenericAtomicComparer)(obj)).collator));
        }
    
    }
}