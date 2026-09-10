using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public sealed class Int64Value : Saxon.Hej.value.IntegerValue {
        public static readonly Saxon.Hej.value.Int64Value MINUS_ONE = new Saxon.Hej.value.Int64Value(-1);
        public static readonly Saxon.Hej.value.Int64Value ZERO = new Saxon.Hej.value.Int64Value(0);
        public static readonly Saxon.Hej.value.Int64Value PLUS_ONE = new Saxon.Hej.value.Int64Value(+(1));
        public static readonly Saxon.Hej.value.Int64Value MAX_LONG = new Saxon.Hej.value.Int64Value(System.Int64.MaxValue);
        public static readonly Saxon.Hej.value.Int64Value MIN_LONG = new Saxon.Hej.value.Int64Value(System.Int64.MinValue);
        private readonly long value;
        private static readonly Saxon.Hej.value.Int64Value[] SMALL_INTEGERS = {new Saxon.Hej.value.Int64Value(0), new Saxon.Hej.value.Int64Value(1), new Saxon.Hej.value.Int64Value(2), new Saxon.Hej.value.Int64Value(3), new Saxon.Hej.value.Int64Value(4), new Saxon.Hej.value.Int64Value(5), new Saxon.Hej.value.Int64Value(6), new Saxon.Hej.value.Int64Value(7), new Saxon.Hej.value.Int64Value(8), new Saxon.Hej.value.Int64Value(9), new Saxon.Hej.value.Int64Value(10), new Saxon.Hej.value.Int64Value(11), new Saxon.Hej.value.Int64Value(12), new Saxon.Hej.value.Int64Value(13), new Saxon.Hej.value.Int64Value(14), new Saxon.Hej.value.Int64Value(15), new Saxon.Hej.value.Int64Value(16), new Saxon.Hej.value.Int64Value(17), new Saxon.Hej.value.Int64Value(18), new Saxon.Hej.value.Int64Value(19), new Saxon.Hej.value.Int64Value(20)};
        public Int64Value(long value) : base (Saxon.Hej.type.BuiltInAtomicType.INTEGER) {
            this.value = value;
        }
        public Int64Value(long value, Saxon.Hej.type.AtomicType typeLabel) : base (typeLabel) {
            this.value = value;
        }
        public Int64Value(long val, Saxon.Hej.type.BuiltInAtomicType typeLabel, bool check) : base (typeLabel) {
            value = val;
            if ((check && !(checkRange(value, typeLabel)))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((("Integer value "+val)+" is out of range for the requested type ")+typeLabel.getDescription()));
                err.setErrorCode("XPTY0004");
                err.setIsTypeError(true);
                throw err;
            }
        }
        public static Saxon.Hej.value.Int64Value makeIntegerValue(long value) {
            if (((value<=20) && (value>=0))) {
                return SMALL_INTEGERS[(int)(value)];
            } else {
                return new Saxon.Hej.value.Int64Value(value);
            }
        }
        public static Saxon.Hej.value.Int64Value makeDerived(long val, Saxon.Hej.type.AtomicType type) {
            return new Saxon.Hej.value.Int64Value(val, type);
        }
        public static Saxon.Hej.value.Int64Value signum(long val) {
            if ((val == 0)) {
                return ZERO;
            } else {
                return ((val<0) ? MINUS_ONE : PLUS_ONE);
            }
        }
        public override int asSubscript() {
            if (((value>0) && (value<=System.Int32.MaxValue))) {
                return (int)(value);
            } else {
                return -1;
            }
        }
        public override Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            if ((typeLabel.getPrimitiveType() == Saxon.Hej.om.StandardNames.XS_INTEGER)) {
                return new Saxon.Hej.value.Int64Value(value, typeLabel);
            } else {
                return new Saxon.Hej.value.BigDecimalValue(value).copyAsSubType(typeLabel);
            }
        }
        public override Saxon.Hej.type.ValidationFailure validateAgainstSubType(Saxon.Hej.type.BuiltInAtomicType type) {
            if (checkRange(value, type)) {
                return null;
            } else {
                Saxon.Hej.type.ValidationFailure err = new Saxon.Hej.type.ValidationFailure(((("Value "+value)+" cannot be converted to integer subtype ")+type.getDescription()));
                err.setErrorCode("FORG0001");
                return err;
            }
        }
        public override int GetHashCode() {
            if (((value>System.Int32.MinValue) && (value<System.Int32.MaxValue))) {
                return (int)(value);
            } else {
                return (getDoubleValue()).GetHashCode();
            }
        }
        public override long longValue() {
            return value;
        }
        public override bool effectiveBooleanValue() {
            return (value != 0);
        }
        public override int compareTo(Saxon.Hej.expr.sort.XPathComparable other) {
            if ((other is Saxon.Hej.value.NumericValue)) {
                if ((other is Saxon.Hej.value.Int64Value)) {
                    return value.CompareTo(((Saxon.Hej.value.Int64Value)(other)).value);
                } else {
                    if ((other is Saxon.Hej.value.BigIntegerValue)) {
                        return Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(value).CompareTo(((Saxon.Hej.value.BigIntegerValue)(other)).asBigInteger());
                    } else {
                        if ((other is Saxon.Hej.value.BigDecimalValue)) {
                            return Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(value).CompareTo(((Saxon.Hej.value.BigDecimalValue)(other)).getDecimalValue());
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
            return value.CompareTo(other);
        }
        public override Saxon.Hej.str.UnicodeString getPrimitiveStringValue() {
            if ((value == System.Int64.MinValue)) {
                return Saxon.Hej.str.StringConstants.MIN_LONG;
            }
            int size = (((value<0)) ? (stringSize(-(value))+1) : stringSize(value));
            byte[] buf = new byte[size];
            getDigits(value, size, buf);
            return new Saxon.Hej.str.Twine8(buf);
        }
        private static void getDigits(long i, int index, byte[] buf) {
            long q;
            int r;
            int charPos = index;
            byte sign = 0;
            if ((i<0)) {
                sign = (byte)('-');
                i = -(i);
            }
            while ((i>System.Int32.MaxValue)) {
                q = (i/100);
                r = (int)(((i-(((((q<<6))+((q<<5)))+((q<<2)))))));
                i = q;
                buf[--charPos] = DIGIT_ONES[r];
                buf[--charPos] = DIGIT_TENS[r];
            }
            int q2;
            int i2 = (int)(i);
            while ((i2>=65536)) {
                q2 = (i2/100);
                r = (i2-(((((q2<<6))+((q2<<5)))+((q2<<2)))));
                i2 = q2;
                buf[--charPos] = DIGIT_ONES[r];
                buf[--charPos] = DIGIT_TENS[r];
            }
            do {
                q2 = Saxon.Impl.Helpers.Utils.unsignedRightShift(((i2*52429)), ((16+3)));
                r = (i2-((((q2<<3))+((q2<<1)))));
                buf[--charPos] = DIGITS[r];
                i2 = q2;
            } while ((i2 != 0));
            if ((sign != 0)) {
                buf[--charPos] = sign;
            }
        }
        private readonly static byte[] DIGITS = Saxon.Hej.str.StringConstants.bytes("0123456789");
        private readonly static byte[] DIGIT_TENS = Saxon.Hej.str.StringConstants.bytes(((((((((("0000000000"+"1111111111")+"2222222222")+"3333333333")+"4444444444")+"5555555555")+"6666666666")+"7777777777")+"8888888888")+"9999999999"));
        private readonly static byte[] DIGIT_ONES = Saxon.Hej.str.StringConstants.bytes(((((((((("0123456789"+"0123456789")+"0123456789")+"0123456789")+"0123456789")+"0123456789")+"0123456789")+"0123456789")+"0123456789")+"0123456789"));
        private static int stringSize(long x) {
            for (int w = 0; (w<18); w++) {
                if ((x<powersOfTen[w])) {
                    return (w+1);
                }
            }
            return 19;
        }
        private static readonly long[] powersOfTen = new long[]{10L, 100L, 1000L, 10000L, 100000L, 1_000_000L, 10_000_000L, 100_000_000L, 1_000_000_000L, 10_000_000_000L, 100_000_000_000L, 1_000_000_000_000L, 10_000_000_000_000L, 100_000_000_000_000L, 1_000_000_000_000_000L, 10_000_000_000_000_000L, 100_000_000_000_000_000L, 1_000_000_000_000_000_000L};
        public override double getDoubleValue() {
            return (double)(value);
        }
        public override float getFloatValue() {
            return (float)(value);
        }
        public override Singulink.Numerics.BigDecimal getDecimalValue() {
            return Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(value);
        }
        public override Saxon.Hej.value.NumericValue negate() {
            if ((value == System.Int64.MinValue)) {
                return Saxon.Hej.value.BigIntegerValue.makeIntegerValue(Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(value)).negate();
            } else {
                return new Saxon.Hej.value.Int64Value(-(value));
            }
        }
        public override Saxon.Hej.value.NumericValue floor() {
            return this;
        }
        public override Saxon.Hej.value.NumericValue ceiling() {
            return this;
        }
        public override Saxon.Hej.value.NumericValue round(int scale) {
            if (((scale>=0) || (value == 0))) {
                return this;
            } else {
                if ((scale<-15)) {
                    return new Saxon.Hej.value.BigIntegerValue(value).round(scale);
                }
                long absolute = System.Math.Abs(value);
                long factor = 1;
                for (long i = 1; (i<=-(scale)); i++) {
                    factor *= 10;
                }
                long modulus = (absolute%factor);
                long rval = (absolute-modulus);
                long d = (modulus*2);
                if ((value>0)) {
                    if ((d>=factor)) {
                        rval += factor;
                    }
                } else {
                    if ((d>factor)) {
                        rval += factor;
                    }
                    rval = -(rval);
                }
                return new Saxon.Hej.value.Int64Value(rval);
            }
        }
        public override Saxon.Hej.value.NumericValue roundHalfToEven(int scale) {
            if ((scale>=0)) {
                return this;
            } else {
                if ((scale<-15)) {
                    return new Saxon.Hej.value.BigIntegerValue(value).roundHalfToEven(scale);
                }
                long absolute = System.Math.Abs(value);
                long factor = 1;
                for (long i = 1; (i<=-(scale)); i++) {
                    factor *= 10;
                }
                long modulus = (absolute%factor);
                long rval = (absolute-modulus);
                long d = (modulus*2);
                if ((d>factor)) {
                    rval += factor;
                } else {
                    if ((d<factor)) {
                    } else {
                        if (((rval%((2*factor))) == 0)) {
                        } else {
                            rval += factor;
                        }
                    }
                }
                if ((value<0)) {
                    rval = -(rval);
                }
                return new Saxon.Hej.value.Int64Value(rval);
            }
        }
        public override int signum() {
            if ((value>0)) {
                return +(1);
            }
            if ((value == 0)) {
                return 0;
            }
            return -1;
        }
        public override Saxon.Hej.value.NumericValue abs() {
            if ((value>0)) {
                return this;
            } else {
                if ((value == System.Int64.MinValue)) {
                    return new Saxon.Hej.value.BigIntegerValue(Saxon.Impl.Helpers.BigIntegerUtils.ValueOf("9223372036854775808"));
                } else {
                    return makeIntegerValue(-(value));
                }
            }
        }
        public override Saxon.Hej.value.IntegerValue plus(Saxon.Hej.value.IntegerValue other) {
            if ((other is Saxon.Hej.value.Int64Value)) {
                long topa = (((value>>60))&unchecked((int)0xf));
                if (((topa != 0) && (topa != unchecked((int)0xf)))) {
                    return new Saxon.Hej.value.BigIntegerValue(value).plus(new Saxon.Hej.value.BigIntegerValue(((Saxon.Hej.value.Int64Value)(other)).value));
                }
                long topb = (((((Saxon.Hej.value.Int64Value)(other)).value>>60))&unchecked((int)0xf));
                if (((topb != 0) && (topb != unchecked((int)0xf)))) {
                    return new Saxon.Hej.value.BigIntegerValue(value).plus(new Saxon.Hej.value.BigIntegerValue(((Saxon.Hej.value.Int64Value)(other)).value));
                }
                return makeIntegerValue((value+((Saxon.Hej.value.Int64Value)(other)).value));
            } else {
                return new Saxon.Hej.value.BigIntegerValue(value).plus(other);
            }
        }
        public override Saxon.Hej.value.IntegerValue minus(Saxon.Hej.value.IntegerValue other) {
            if ((other is Saxon.Hej.value.Int64Value)) {
                long topa = (((value>>60))&unchecked((int)0xf));
                if (((topa != 0) && (topa != unchecked((int)0xf)))) {
                    return new Saxon.Hej.value.BigIntegerValue(value).minus(new Saxon.Hej.value.BigIntegerValue(((Saxon.Hej.value.Int64Value)(other)).value));
                }
                long topb = (((((Saxon.Hej.value.Int64Value)(other)).value>>60))&unchecked((int)0xf));
                if (((topb != 0) && (topb != unchecked((int)0xf)))) {
                    return new Saxon.Hej.value.BigIntegerValue(value).minus(new Saxon.Hej.value.BigIntegerValue(((Saxon.Hej.value.Int64Value)(other)).value));
                }
                return makeIntegerValue((value-((Saxon.Hej.value.Int64Value)(other)).value));
            } else {
                return new Saxon.Hej.value.BigIntegerValue(value).minus(other);
            }
        }
        public override Saxon.Hej.value.IntegerValue times(Saxon.Hej.value.IntegerValue other) {
            if ((other is Saxon.Hej.value.Int64Value)) {
                if ((isLong() || ((Saxon.Hej.value.Int64Value)(other)).isLong())) {
                    return new Saxon.Hej.value.BigIntegerValue(value).times(new Saxon.Hej.value.BigIntegerValue(((Saxon.Hej.value.Int64Value)(other)).value));
                } else {
                    return makeIntegerValue((value*((Saxon.Hej.value.Int64Value)(other)).value));
                }
            } else {
                return new Saxon.Hej.value.BigIntegerValue(value).times(other);
            }
        }
        public override Saxon.Hej.value.NumericValue div(Saxon.Hej.value.IntegerValue other) {
            if ((other is Saxon.Hej.value.Int64Value)) {
                long quotient = ((Saxon.Hej.value.Int64Value)(other)).value;
                if ((quotient == 0)) {
                    throw new Saxon.Hej.trans.XPathException("Integer division by zero", "FOAR0001");
                }
                if ((isLong() || ((Saxon.Hej.value.Int64Value)(other)).isLong())) {
                    return new Saxon.Hej.value.BigIntegerValue(value).div(new Saxon.Hej.value.BigIntegerValue(quotient));
                }
                if (((value%quotient) == 0)) {
                    return makeIntegerValue((value/quotient));
                } else {
                    return Saxon.Hej.expr.Calculator.decimalDivide(new Saxon.Hej.value.BigDecimalValue(value), new Saxon.Hej.value.BigDecimalValue(quotient));
                }
            } else {
                return new Saxon.Hej.value.BigIntegerValue(value).div(other);
            }
        }
        public override Saxon.Hej.value.IntegerValue mod(Saxon.Hej.value.IntegerValue other) {
            if ((other is Saxon.Hej.value.Int64Value)) {
                long quotient = ((Saxon.Hej.value.Int64Value)(other)).value;
                if ((quotient == 0)) {
                    throw new Saxon.Hej.trans.XPathException("Integer modulo zero", "FOAR0001");
                }
                if ((isLong() || ((Saxon.Hej.value.Int64Value)(other)).isLong())) {
                    return new Saxon.Hej.value.BigIntegerValue(value).mod(new Saxon.Hej.value.BigIntegerValue(((Saxon.Hej.value.Int64Value)(other)).value));
                } else {
                    return makeIntegerValue((value%quotient));
                }
            } else {
                return new Saxon.Hej.value.BigIntegerValue(value).mod(other);
            }
        }
        public override Saxon.Hej.value.IntegerValue idiv(Saxon.Hej.value.IntegerValue other) {
            if ((other.signum() == 0)) {
                throw new Saxon.Hej.trans.XPathException("Integer division by zero", "FOAR0001");
            }
            if ((other is Saxon.Hej.value.Int64Value)) {
                if ((isLong() || ((Saxon.Hej.value.Int64Value)(other)).isLong())) {
                    return new Saxon.Hej.value.BigIntegerValue(value).idiv(new Saxon.Hej.value.BigIntegerValue(((Saxon.Hej.value.Int64Value)(other)).value));
                }
                return makeIntegerValue((value/((Saxon.Hej.value.Int64Value)(other)).value));
            } else {
                return new Saxon.Hej.value.BigIntegerValue(value).idiv(other);
            }
        }
        private bool isLong() {
            long top = (value>>31);
            return (top != 0);
        }
        public override System.Numerics.BigInteger asBigInteger() {
            return Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(value);
        }
    
    }
}