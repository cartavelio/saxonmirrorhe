using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public sealed class FloatValue : Saxon.Hej.value.NumericValue {
        public static readonly Saxon.Hej.value.FloatValue ZERO = new Saxon.Hej.value.FloatValue((float)(0.0));
        public static readonly Saxon.Hej.value.FloatValue NEGATIVE_ZERO = new Saxon.Hej.value.FloatValue((float)(-(0.0)));
        public static readonly Saxon.Hej.value.FloatValue ONE = new Saxon.Hej.value.FloatValue((float)(1.0));
        public static readonly Saxon.Hej.value.FloatValue NaN = new Saxon.Hej.value.FloatValue(System.Single.NaN);
        private readonly float value;
        public FloatValue(float value) : base (Saxon.Hej.type.BuiltInAtomicType.FLOAT) {
            this.value = value;
        }
        public static Saxon.Hej.value.FloatValue makeFloatValue(float value) {
            return new Saxon.Hej.value.FloatValue(value);
        }
        public FloatValue(float value, Saxon.Hej.type.AtomicType typeLabel) : base (typeLabel) {
            this.value = value;
        }
        public override Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            return new Saxon.Hej.value.FloatValue(value, typeLabel);
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.FLOAT;
        }
        public override float getFloatValue() {
            return value;
        }
        public override double getDoubleValue() {
            return value;
        }
        public override Singulink.Numerics.BigDecimal getDecimalValue() {
            try {
                return Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(value);
            } catch (Saxon.Ejava.lang.NumberFormatException e) {
                throw new Saxon.Hej.type.ValidationException(e);
            }
        }
        public override long longValue() {
            if ((System.Math.Abs(value)>(double)(System.Int64.MaxValue))) {
                return ((value<0) ? -(System.Int64.MaxValue) : +(System.Int64.MaxValue));
            }
            return (long)(value);
        }
        public override int GetHashCode() {
            if (((value>System.Int32.MinValue) && (value<System.Int32.MaxValue))) {
                return (int)(value);
            } else {
                return (getDoubleValue()).GetHashCode();
            }
        }
        public override bool isNaN() {
            return System.Single.IsNaN(value);
        }
        public override bool effectiveBooleanValue() {
            return (((value != 0.0) && !(System.Single.IsNaN(value))));
        }
        public override Saxon.Hej.str.UnicodeString getPrimitiveStringValue() {
            return floatToString(value);
        }
        public override Saxon.Hej.str.UnicodeString getCanonicalLexicalRepresentation() {
            Saxon.Hej.str.UnicodeBuilder fsb = new Saxon.Hej.str.UnicodeBuilder(32);
            return Saxon.Hej.value.FloatingPointConverter.appendFloat(fsb, value, true);
        }
        public static Saxon.Hej.str.UnicodeString floatToString(float value) {
            return Saxon.Hej.value.FloatingPointConverter.appendFloat(new Saxon.Hej.str.UnicodeBuilder(), value, false);
        }
        public override Saxon.Hej.value.NumericValue negate() {
            return new Saxon.Hej.value.FloatValue(-(value));
        }
        public override Saxon.Hej.value.NumericValue floor() {
            return new Saxon.Hej.value.FloatValue((float)(System.Math.Floor(value)));
        }
        public override Saxon.Hej.value.NumericValue ceiling() {
            return new Saxon.Hej.value.FloatValue((float)(System.Math.Ceiling(value)));
        }
        public override Saxon.Hej.value.NumericValue round(int scale) {
            return new Saxon.Hej.value.FloatValue(Saxon.Impl.Helpers.Utils.roundFloat(value, scale));
        }
        public override Saxon.Hej.value.NumericValue roundHalfToEven(int scale) {
            Saxon.Hej.value.DoubleValue d = new Saxon.Hej.value.DoubleValue(getDoubleValue());
            d = (Saxon.Hej.value.DoubleValue)(d.roundHalfToEven(scale));
            return new Saxon.Hej.value.FloatValue(d.getFloatValue());
        }
        public override int signum() {
            if (System.Single.IsNaN(value)) {
                return 0;
            }
            return compareTo(0);
        }
        public override bool isNegativeZero() {
            return value == 0.0f && float.IsNegativeInfinity(1.0f / value);
        }
        public override bool isWholeNumber() {
            return ((value == System.Math.Floor(value)) && !(System.Single.IsInfinity(value)));
        }
        public override int asSubscript() {
            if (((isWholeNumber() && (value>0)) && (value<=System.Int32.MaxValue))) {
                return (int)(value);
            } else {
                return -1;
            }
        }
        public override Saxon.Hej.value.NumericValue abs() {
            if ((value>0.0)) {
                return this;
            } else {
                return new Saxon.Hej.value.FloatValue(System.Math.Abs(value));
            }
        }
        public override int compareTo(Saxon.Hej.expr.sort.XPathComparable other) {
            if ((other is Saxon.Hej.value.NumericValue)) {
                if ((other is Saxon.Hej.value.FloatValue)) {
                    float otherFloat = ((Saxon.Hej.value.FloatValue)(other)).value;
                    if ((value == otherFloat)) {
                        return 0;
                    } else {
                        if ((value<otherFloat)) {
                            return -1;
                        } else {
                            return +(1);
                        }
                    }
                }
                if ((other is Saxon.Hej.value.DoubleValue)) {
                    return base.compareTo(other);
                }
                return compareTo(Saxon.Hej.type.Converter.NumericToFloat.INSTANCE.convert((Saxon.Hej.value.NumericValue)(other)));
            } else {
                throw new System.InvalidCastException(("Cannot compare xs:float to "+other));
            }
        }
        public override int compareTo(long other) {
            float otherFloat = (float)(other);
            if ((value == otherFloat)) {
                return 0;
            }
            return ((value<otherFloat) ? -1 : +(1));
        }
        public override Saxon.Hej.expr.sort.AtomicMatchKey asMapKey() {
            if (isNaN()) {
                return Saxon.Hej.expr.sort.AtomicSortComparer.COLLATION_KEY_NaN;
            } else {
                if (System.Double.IsInfinity(value)) {
                    return new Saxon.Hej.value.DoubleValue(value);
                } else {
                    try {
                        return new Saxon.Hej.value.BigDecimalValue(value);
                    } catch (Saxon.Hej.type.ValidationException e) {
                        throw new Saxon.Ejava.lang.AssertionError(e);
                    }
                }
            }
        }
        public override bool isIdentical(Saxon.Hej.value.AtomicValue v) {
            return ((v is Saxon.Hej.value.FloatValue) && Saxon.Hej.expr.sort.DoubleSortComparer.getInstance().comparesEqual(this, v));
        }
        public override Saxon.Hej.value.AtomicValue asAtomic() {
            return this;
        }
    
    }
}