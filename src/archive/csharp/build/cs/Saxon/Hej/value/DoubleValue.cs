using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public sealed class DoubleValue : Saxon.Hej.value.NumericValue {
        public static readonly Saxon.Hej.value.DoubleValue ZERO = new Saxon.Hej.value.DoubleValue(0.0);
        public static readonly Saxon.Hej.value.DoubleValue NEGATIVE_ZERO = new Saxon.Hej.value.DoubleValue(-(0.0));
        public static readonly Saxon.Hej.value.DoubleValue ONE = new Saxon.Hej.value.DoubleValue(1.0);
        public static readonly Saxon.Hej.value.DoubleValue NaN = new Saxon.Hej.value.DoubleValue(System.Double.NaN);
        private readonly double value;
        public DoubleValue(double value) : base (Saxon.Hej.type.BuiltInAtomicType.DOUBLE) {
            this.value = value;
        }
        public DoubleValue(double value, Saxon.Hej.type.AtomicType typeLabel) : base (typeLabel) {
            this.value = value;
        }
        public static Saxon.Hej.value.DoubleValue makeDoubleValue(double value) {
            return new Saxon.Hej.value.DoubleValue(value);
        }
        public override Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            return new Saxon.Hej.value.DoubleValue(value, typeLabel);
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
        }
        public override double getDoubleValue() {
            return value;
        }
        public override float getFloatValue() {
            return (float)(value);
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
                return (value).GetHashCode();
            }
        }
        public override bool isNaN() {
            return System.Double.IsNaN(value);
        }
        public override bool effectiveBooleanValue() {
            return ((value != 0.0) && !(System.Double.IsNaN(value)));
        }
        public override Saxon.Hej.str.UnicodeString getPrimitiveStringValue() {
            return doubleToString(value);
        }
        public override Saxon.Hej.str.UnicodeString getCanonicalLexicalRepresentation() {
            return Saxon.Hej.value.FloatingPointConverter.convertDouble(value, true);
        }
        public static Saxon.Hej.str.UnicodeString doubleToString(double value) {
            return Saxon.Hej.value.FloatingPointConverter.convertDouble(value, false);
        }
        public override Saxon.Hej.value.NumericValue negate() {
            return new Saxon.Hej.value.DoubleValue(-(value));
        }
        public override Saxon.Hej.value.NumericValue floor() {
            return new Saxon.Hej.value.DoubleValue(System.Math.Floor(value));
        }
        public override Saxon.Hej.value.NumericValue ceiling() {
            return new Saxon.Hej.value.DoubleValue(System.Math.Ceiling(value));
        }
        public override Saxon.Hej.value.NumericValue round(int scale) {
            return new Saxon.Hej.value.DoubleValue(Saxon.Impl.Helpers.Utils.roundDouble(value, scale));
        }
        public override Saxon.Hej.value.NumericValue roundHalfToEven(int scale) {
            if (System.Double.IsNaN(value)) {
                return this;
            }
            if (System.Double.IsInfinity(value)) {
                return this;
            }
            if ((value == 0.0)) {
                return this;
            }
            double factor = System.Math.Pow(10, (scale+1));
            double d = System.Math.Abs((value*factor));
            if (System.Double.IsInfinity(d)) {
                Singulink.Numerics.BigDecimal dec = Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(value);
                return new Saxon.Hej.value.DoubleValue(((double)dec));
            }
            double rem = (d%10);
            if ((rem>5)) {
                d += (10-rem);
            } else {
                if ((rem<5)) {
                    d -= rem;
                } else {
                    if ((((d%20)) == 15)) {
                        d += 5;
                    } else {
                        d -= 5;
                    }
                }
            }
            d /= factor;
            if ((value<0)) {
                d = -(d);
            }
            return new Saxon.Hej.value.DoubleValue(d);
        }
        public override int signum() {
            if (System.Double.IsNaN(value)) {
                return 0;
            }
            return ((value>0) ? 1 : ((value == 0) ? 0 : -1));
        }
        public override bool isNegativeZero() {
            return value == 0.0 && double.IsNegativeInfinity(1.0 / value);
        }
        public override bool isWholeNumber() {
            return ((value == System.Math.Floor(value)) && !(System.Double.IsInfinity(value)));
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
                return new Saxon.Hej.value.DoubleValue(System.Math.Abs(value));
            }
        }
        public override int compareTo(long other) {
            double otherDouble = (double)(other);
            if ((value == otherDouble)) {
                return 0;
            }
            return ((value<otherDouble) ? -1 : +(1));
        }
        public override Saxon.Hej.expr.sort.AtomicMatchKey asMapKey() {
            if (isNaN()) {
                return Saxon.Hej.expr.sort.AtomicSortComparer.COLLATION_KEY_NaN;
            } else {
                if (System.Double.IsInfinity(value)) {
                    return this;
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
            return ((v is Saxon.Hej.value.DoubleValue) && Saxon.Hej.expr.sort.DoubleSortComparer.getInstance().comparesEqual(this, v));
        }
    
    }
}