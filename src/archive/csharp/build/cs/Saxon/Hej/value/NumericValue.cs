using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public abstract class NumericValue : Saxon.Hej.value.AtomicValue, Saxon.Hej.expr.sort.XPathComparable, Saxon.Hej.expr.sort.AtomicMatchKey, Saxon.Hej.value.ContextFreeAtomicValue {
        public NumericValue(Saxon.Hej.type.AtomicType typeLabel) : base (typeLabel) {
        }
        public static Saxon.Hej.value.NumericValue parseNumber(string @in) {
            if (((@in.IndexOf('e')>=0) || (@in.IndexOf('E')>=0))) {
                try {
                    return new Saxon.Hej.value.DoubleValue(Saxon.Impl.Helpers.Utils.ParseDouble(@in));
                } catch (Saxon.Ejava.lang.NumberFormatException e) {
                    return Saxon.Hej.value.DoubleValue.NaN;
                }
            } else {
                if ((@in.IndexOf('.')>=0)) {
                    Saxon.Hej.type.ConversionResult v = Saxon.Hej.value.BigDecimalValue.makeDecimalValue(@in, true);
                    if ((v is Saxon.Hej.type.ValidationFailure)) {
                        return Saxon.Hej.value.DoubleValue.NaN;
                    } else {
                        return (Saxon.Hej.value.NumericValue)(v);
                    }
                } else {
                    Saxon.Hej.type.ConversionResult v = Saxon.Hej.value.Int64Value.stringToInteger(@in);
                    if ((v is Saxon.Hej.type.ValidationFailure)) {
                        return Saxon.Hej.value.DoubleValue.NaN;
                    } else {
                        return (Saxon.Hej.value.NumericValue)(v);
                    }
                }
            }
        }
        public abstract double getDoubleValue();
        public abstract float getFloatValue();
        public abstract Singulink.Numerics.BigDecimal getDecimalValue();
        public abstract override bool effectiveBooleanValue();
        public static bool isInteger(Saxon.Hej.value.AtomicValue value) {
            return (value is Saxon.Hej.value.IntegerValue);
        }
        public abstract long longValue();
        public abstract Saxon.Hej.value.NumericValue negate();
        public abstract Saxon.Hej.value.NumericValue floor();
        public abstract Saxon.Hej.value.NumericValue ceiling();
        public abstract Saxon.Hej.value.NumericValue round(int scale);
        public abstract Saxon.Hej.value.NumericValue roundHalfToEven(int scale);
        public abstract int signum();
        public virtual bool isNegativeZero() {
            return false;
        }
        public abstract bool isWholeNumber();
        public abstract int asSubscript();
        public abstract Saxon.Hej.value.NumericValue abs();
        public override Saxon.Hej.expr.sort.AtomicMatchKey getXPathMatchKey(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return this;
        }
        public override Saxon.Hej.expr.sort.XPathComparable getXPathComparable(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return this;
        }
        public Saxon.Hej.expr.sort.XPathComparable getXPathComparable() {
            return this;
        }
        public virtual int compareTo(Saxon.Hej.expr.sort.XPathComparable other) {
            if ((other is Saxon.Hej.value.NumericValue)) {
                double a = getDoubleValue();
                double b = ((Saxon.Hej.value.NumericValue)(other)).getDoubleValue();
                if ((a == b)) {
                    return 0;
                }
                if ((a<b)) {
                    return -1;
                }
                return +(1);
            } else {
                throw new System.InvalidCastException(("Cannot compare numeric value to "+other.ToString()));
            }
        }
        public abstract int compareTo(long other);
        public override bool Equals(System.Object other) {
            return ((other is Saxon.Hej.value.NumericValue) && (compareTo((Saxon.Hej.value.NumericValue)(other)) == 0));
        }
        public abstract override int GetHashCode();
        public override string show() {
            return getStringValue();
        }
    
    }
}