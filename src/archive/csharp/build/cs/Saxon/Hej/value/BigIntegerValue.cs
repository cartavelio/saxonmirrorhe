using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public sealed class BigIntegerValue : Saxon.Hej.value.IntegerValue {
        private readonly System.Numerics.BigInteger value;
        private static readonly System.Numerics.BigInteger MAX_INT = Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(System.Int32.MaxValue);
        private static readonly System.Numerics.BigInteger MIN_INT = Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(System.Int32.MinValue);
        public static readonly System.Numerics.BigInteger MAX_LONG = Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(System.Int64.MaxValue);
        public static readonly System.Numerics.BigInteger MIN_LONG = Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(System.Int64.MinValue);
        public static readonly System.Numerics.BigInteger MAX_UNSIGNED_LONG = Saxon.Impl.Helpers.BigIntegerUtils.ValueOf("18446744073709551615");
        public static readonly Saxon.Hej.value.BigIntegerValue ZERO = new Saxon.Hej.value.BigIntegerValue(System.Numerics.BigInteger.Zero);
        public BigIntegerValue(System.Numerics.BigInteger value)  {
            this.value = value;
            typeLabel = Saxon.Hej.type.BuiltInAtomicType.INTEGER;
        }
        public BigIntegerValue(System.Numerics.BigInteger value, Saxon.Hej.type.AtomicType typeLabel)  {
            this.value = value;
            this.typeLabel = typeLabel;
        }
        public BigIntegerValue(long value)  {
            this.value = Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(value);
            typeLabel = Saxon.Hej.type.BuiltInAtomicType.INTEGER;
        }
        public override Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            if ((typeLabel.getPrimitiveType() == Saxon.Hej.om.StandardNames.XS_INTEGER)) {
                Saxon.Hej.value.BigIntegerValue v = new Saxon.Hej.value.BigIntegerValue(value);
                v.typeLabel = typeLabel;
                return v;
            } else {
                return new Saxon.Hej.value.BigDecimalValue(new Singulink.Numerics.BigDecimal(value, 0));
            }
        }
        public override Saxon.Hej.type.ValidationFailure convertToSubType(Saxon.Hej.type.BuiltInAtomicType type, bool validate) {
            if (!(validate)) {
                typeLabel = type;
                return null;
            }
            if (Saxon.Hej.value.IntegerValue.checkBigRange(value, type)) {
                typeLabel = type;
                return null;
            } else {
                Saxon.Hej.type.ValidationFailure err = new Saxon.Hej.type.ValidationFailure(("Integer value is out of range for subtype "+type.getDisplayName()));
                err.setErrorCode("FORG0001");
                return err;
            }
        }
        public override Saxon.Hej.type.ValidationFailure validateAgainstSubType(Saxon.Hej.type.BuiltInAtomicType type) {
            if (Saxon.Hej.value.IntegerValue.checkBigRange(value, type)) {
                typeLabel = type;
                return null;
            } else {
                Saxon.Hej.type.ValidationFailure err = new Saxon.Hej.type.ValidationFailure(("Integer value is out of range for subtype "+type.getDisplayName()));
                err.setErrorCode("FORG0001");
                return err;
            }
        }
        public override int GetHashCode() {
            if (((value.CompareTo(MIN_INT)>=0) && (value.CompareTo(MAX_INT)<=0))) {
                return ((int)value);
            } else {
                return (getDoubleValue()).GetHashCode();
            }
        }
        public override long longValue() {
            return ((long)value);
        }
        public override System.Numerics.BigInteger asBigInteger() {
            return value;
        }
        public bool isWithinLongRange() {
            return ((value.CompareTo(MIN_LONG)>=0) && (value.CompareTo(MAX_LONG)<=0));
        }
        public Singulink.Numerics.BigDecimal asDecimal() {
            return new Singulink.Numerics.BigDecimal(value, 0);
        }
        public override bool effectiveBooleanValue() {
            return (value.CompareTo(System.Numerics.BigInteger.Zero) != 0);
        }
        public override int compareTo(Saxon.Hej.expr.sort.XPathComparable other) {
            if ((other is Saxon.Hej.value.NumericValue)) {
                if ((other is Saxon.Hej.value.BigIntegerValue)) {
                    return value.CompareTo(((Saxon.Hej.value.BigIntegerValue)(other)).value);
                } else {
                    if ((other is Saxon.Hej.value.Int64Value)) {
                        return value.CompareTo(Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(((Saxon.Hej.value.Int64Value)(other)).longValue()));
                    } else {
                        if ((other is Saxon.Hej.value.BigDecimalValue)) {
                            return asDecimal().CompareTo(((Saxon.Hej.value.BigDecimalValue)(other)).getDecimalValue());
                        } else {
                            return base.compareTo(other);
                        }
                    }
                }
            } else {
                throw new System.InvalidCastException(("Cannot compare xs:integer to "+other));
            }
        }
        public override int compareTo(long other) {
            if ((other == 0)) {
                return Saxon.Impl.Helpers.BigIntegerUtils.Signum(value);
            }
            return value.CompareTo(Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(other));
        }
        public override Saxon.Hej.str.UnicodeString getPrimitiveStringValue() {
            return Saxon.Hej.str.BMPString.of(value.ToString());
        }
        public override double getDoubleValue() {
            return ((double)value);
        }
        public override Singulink.Numerics.BigDecimal getDecimalValue() {
            return new Singulink.Numerics.BigDecimal(value, 0);
        }
        public override float getFloatValue() {
            return (float)(getDoubleValue());
        }
        public override Saxon.Hej.value.NumericValue negate() {
            return new Saxon.Hej.value.BigIntegerValue(System.Numerics.BigInteger.Negate(value));
        }
        public override Saxon.Hej.value.NumericValue floor() {
            return this;
        }
        public override Saxon.Hej.value.NumericValue ceiling() {
            return this;
        }
        public override Saxon.Hej.value.NumericValue round(int scale) {
            if ((scale>=0)) {
                return this;
            } else {
                System.Numerics.BigInteger factor = System.Numerics.BigInteger.Pow(Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(10), -(scale));
                System.Numerics.BigInteger[] pair = Saxon.Impl.Helpers.BigIntegerUtils.DivideAndRemainder(value, factor);
                int up = pair[1].CompareTo(System.Numerics.BigInteger.Divide(factor, Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(2)));
                if ((up>=0)) {
                    pair[0] = System.Numerics.BigInteger.Add(pair[0], Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(1));
                }
                return makeIntegerValue(System.Numerics.BigInteger.Multiply(pair[0], factor));
            }
        }
        public override Saxon.Hej.value.NumericValue roundHalfToEven(int scale) {
            if ((scale>=0)) {
                return this;
            } else {
                System.Numerics.BigInteger factor = System.Numerics.BigInteger.Pow(Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(10), -(scale));
                System.Numerics.BigInteger[] pair = Saxon.Impl.Helpers.BigIntegerUtils.DivideAndRemainder(value, factor);
                int up = pair[1].CompareTo(System.Numerics.BigInteger.Divide(factor, Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(2)));
                if ((up>0)) {
                    pair[0] = System.Numerics.BigInteger.Add(pair[0], Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(1));
                } else {
                    if ((up == 0)) {
                        if ((Saxon.Impl.Helpers.BigIntegerUtils.Signum(Saxon.Impl.Helpers.BigIntegerUtils.Mod(pair[0], Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(2))) != 0)) {
                            pair[0] = System.Numerics.BigInteger.Add(pair[0], Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(1));
                        }
                    }
                }
                return makeIntegerValue(System.Numerics.BigInteger.Multiply(pair[0], factor));
            }
        }
        public override int signum() {
            return Saxon.Impl.Helpers.BigIntegerUtils.Signum(value);
        }
        public override Saxon.Hej.value.NumericValue abs() {
            if ((Saxon.Impl.Helpers.BigIntegerUtils.Signum(value)>=0)) {
                return this;
            } else {
                return new Saxon.Hej.value.BigIntegerValue(System.Numerics.BigInteger.Abs(value));
            }
        }
        public override bool isWholeNumber() {
            return true;
        }
        public override int asSubscript() {
            if (((value.CompareTo(System.Numerics.BigInteger.Zero)>0) && (value.CompareTo(MAX_INT)<=0))) {
                return (int)(longValue());
            } else {
                return -1;
            }
        }
        public override Saxon.Hej.value.IntegerValue plus(Saxon.Hej.value.IntegerValue other) {
            if ((other is Saxon.Hej.value.BigIntegerValue)) {
                return makeIntegerValue(System.Numerics.BigInteger.Add(value, ((Saxon.Hej.value.BigIntegerValue)(other)).value));
            } else {
                return makeIntegerValue(System.Numerics.BigInteger.Add(value, Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(((Saxon.Hej.value.Int64Value)(other)).longValue())));
            }
        }
        public override Saxon.Hej.value.IntegerValue minus(Saxon.Hej.value.IntegerValue other) {
            if ((other is Saxon.Hej.value.BigIntegerValue)) {
                return makeIntegerValue(System.Numerics.BigInteger.Subtract(value, ((Saxon.Hej.value.BigIntegerValue)(other)).value));
            } else {
                return makeIntegerValue(System.Numerics.BigInteger.Subtract(value, Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(((Saxon.Hej.value.Int64Value)(other)).longValue())));
            }
        }
        public override Saxon.Hej.value.IntegerValue times(Saxon.Hej.value.IntegerValue other) {
            if ((other is Saxon.Hej.value.BigIntegerValue)) {
                return makeIntegerValue(System.Numerics.BigInteger.Multiply(value, ((Saxon.Hej.value.BigIntegerValue)(other)).value));
            } else {
                return makeIntegerValue(System.Numerics.BigInteger.Multiply(value, Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(((Saxon.Hej.value.Int64Value)(other)).longValue())));
            }
        }
        public override Saxon.Hej.value.NumericValue div(Saxon.Hej.value.IntegerValue other) {
            System.Numerics.BigInteger oi;
            if ((other is Saxon.Hej.value.BigIntegerValue)) {
                oi = ((Saxon.Hej.value.BigIntegerValue)(other)).value;
            } else {
                oi = Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(other.longValue());
            }
            Saxon.Hej.value.BigDecimalValue a = new Saxon.Hej.value.BigDecimalValue(new Singulink.Numerics.BigDecimal(value, 0));
            Saxon.Hej.value.BigDecimalValue b = new Saxon.Hej.value.BigDecimalValue(new Singulink.Numerics.BigDecimal(oi, 0));
            return Saxon.Hej.expr.Calculator.decimalDivide(a, b);
        }
        public override Saxon.Hej.value.IntegerValue mod(Saxon.Hej.value.IntegerValue other) {
            if ((other.signum() == 0)) {
                throw new Saxon.Hej.trans.XPathException("Integer modulo zero", "FOAR0001");
            }
            if ((other is Saxon.Hej.value.BigIntegerValue)) {
                return makeIntegerValue(Saxon.Impl.Helpers.BigIntegerUtils.Remainder(value, ((Saxon.Hej.value.BigIntegerValue)(other)).value));
            } else {
                return makeIntegerValue(Saxon.Impl.Helpers.BigIntegerUtils.Remainder(value, Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(other.longValue())));
            }
        }
        public override Saxon.Hej.value.IntegerValue idiv(Saxon.Hej.value.IntegerValue other) {
            if ((other.signum() == 0)) {
                throw new Saxon.Hej.trans.XPathException("Integer division by zero", "FOAR0001");
            }
            System.Numerics.BigInteger oi;
            if ((other is Saxon.Hej.value.BigIntegerValue)) {
                oi = ((Saxon.Hej.value.BigIntegerValue)(other)).value;
            } else {
                oi = Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(other.longValue());
            }
            return makeIntegerValue(System.Numerics.BigInteger.Divide(value, oi));
        }
        public override Saxon.Hej.value.IntegerValue reduce() {
            if (((compareTo(System.Int64.MaxValue)<0) && (compareTo(System.Int64.MinValue)>0))) {
                Saxon.Hej.value.Int64Value iv = new Saxon.Hej.value.Int64Value(longValue());
                iv.setTypeLabel(typeLabel);
                return iv;
            }
            return this;
        }
    
    }
}