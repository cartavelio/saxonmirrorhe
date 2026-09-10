using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public abstract class IntegerValue : Saxon.Hej.value.DecimalValue {
        private const long NO_LIMIT = -9999;
        private const long MAX_UNSIGNED_LONG = -9998;
        private static readonly long[] ranges = {Saxon.Hej.om.StandardNames.XS_INTEGER, NO_LIMIT, NO_LIMIT, Saxon.Hej.om.StandardNames.XS_LONG, System.Int64.MinValue, System.Int64.MaxValue, Saxon.Hej.om.StandardNames.XS_INT, System.Int32.MinValue, System.Int32.MaxValue, Saxon.Hej.om.StandardNames.XS_SHORT, System.Int16.MinValue, System.Int16.MaxValue, Saxon.Hej.om.StandardNames.XS_BYTE, -128, 127, Saxon.Hej.om.StandardNames.XS_NON_NEGATIVE_INTEGER, 0, NO_LIMIT, Saxon.Hej.om.StandardNames.XS_POSITIVE_INTEGER, 1, NO_LIMIT, Saxon.Hej.om.StandardNames.XS_NON_POSITIVE_INTEGER, NO_LIMIT, 0, Saxon.Hej.om.StandardNames.XS_NEGATIVE_INTEGER, NO_LIMIT, -1, Saxon.Hej.om.StandardNames.XS_UNSIGNED_LONG, 0, MAX_UNSIGNED_LONG, Saxon.Hej.om.StandardNames.XS_UNSIGNED_INT, 0, 4294967295L, Saxon.Hej.om.StandardNames.XS_UNSIGNED_SHORT, 0, 65535, Saxon.Hej.om.StandardNames.XS_UNSIGNED_BYTE, 0, 255};
        public IntegerValue(Saxon.Hej.type.AtomicType typeLabel) : base (typeLabel) {
        }
        public static Saxon.Hej.value.IntegerValue makeIntegerValue(System.Numerics.BigInteger value) {
            if (((value.CompareTo(Saxon.Hej.value.BigIntegerValue.MAX_LONG)>0) || (value.CompareTo(Saxon.Hej.value.BigIntegerValue.MIN_LONG)<0))) {
                return new Saxon.Hej.value.BigIntegerValue(value);
            } else {
                return Saxon.Hej.value.Int64Value.makeIntegerValue(((long)value));
            }
        }
        public static Saxon.Hej.type.ConversionResult fromDouble(double value) {
            if (System.Double.IsNaN(value)) {
                Saxon.Hej.type.ValidationFailure err = new Saxon.Hej.type.ValidationFailure("Cannot convert double NaN to an integer");
                err.setErrorCode("FOCA0002");
                return err;
            }
            if (System.Double.IsInfinity(value)) {
                Saxon.Hej.type.ValidationFailure err = new Saxon.Hej.type.ValidationFailure("Cannot convert double INF to an integer");
                err.setErrorCode("FOCA0002");
                return err;
            }
            if (((value>System.Int64.MaxValue) || (value<System.Int64.MinValue))) {
                if ((value == System.Math.Floor(value))) {
                    return new Saxon.Hej.value.BigIntegerValue(((System.Numerics.BigInteger)Saxon.Hej.functions.FormatNumber.adjustToDecimal(value, 2)));
                } else {
                    return new Saxon.Hej.value.BigIntegerValue(((System.Numerics.BigInteger)Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(value)));
                }
            }
            return Saxon.Hej.value.Int64Value.makeIntegerValue((long)(value));
        }
        public abstract Saxon.Hej.type.ValidationFailure validateAgainstSubType(Saxon.Hej.type.BuiltInAtomicType type);
        public static bool checkRange(long value, Saxon.Hej.type.BuiltInAtomicType type) {
            int fp = type.getFingerprint();
            for (int i = 0; (i<ranges.Length); i += 3) {
                if ((ranges[i] == fp)) {
                    long min = ranges[(i+1)];
                    if (((min != NO_LIMIT) && (value<min))) {
                        return false;
                    }
                    long max = ranges[(i+2)];
                    return (((max == NO_LIMIT) || (max == MAX_UNSIGNED_LONG)) || (value<=max));
                }
            }
            throw new Saxon.Ejava.lang.IllegalArgumentException(("No range information found for integer subtype "+type.getDescription()));
        }
        public static Saxon.Hej.value.IntegerValue getMinInclusive(Saxon.Hej.type.BuiltInAtomicType type) {
            int fp = type.getFingerprint();
            for (int i = 0; (i<ranges.Length); i += 3) {
                if ((ranges[i] == fp)) {
                    long min = ranges[(i+1)];
                    if ((min == NO_LIMIT)) {
                        return null;
                    } else {
                        return Saxon.Hej.value.Int64Value.makeIntegerValue(min);
                    }
                }
            }
            return null;
        }
        public static Saxon.Hej.value.IntegerValue getMaxInclusive(Saxon.Hej.type.BuiltInAtomicType type) {
            int fp = type.getFingerprint();
            for (int i = 0; (i<ranges.Length); i += 3) {
                if ((ranges[i] == fp)) {
                    long max = ranges[(i+2)];
                    if ((max == NO_LIMIT)) {
                        return null;
                    } else {
                        if ((max == MAX_UNSIGNED_LONG)) {
                            return Saxon.Hej.value.IntegerValue.makeIntegerValue(Saxon.Hej.value.BigIntegerValue.MAX_UNSIGNED_LONG);
                        } else {
                            return Saxon.Hej.value.Int64Value.makeIntegerValue(max);
                        }
                    }
                }
            }
            return null;
        }
        public static bool checkBigRange(System.Numerics.BigInteger big, Saxon.Hej.type.BuiltInAtomicType type) {
            for (int i = 0; (i<ranges.Length); i += 3) {
                if ((ranges[i] == type.getFingerprint())) {
                    long min = ranges[(i+1)];
                    if (((min != NO_LIMIT) && (Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(min).CompareTo(big)>0))) {
                        return false;
                    }
                    long max = ranges[(i+2)];
                    if ((max == NO_LIMIT)) {
                        return true;
                    } else {
                        if ((max == MAX_UNSIGNED_LONG)) {
                            return (Saxon.Hej.value.BigIntegerValue.MAX_UNSIGNED_LONG.CompareTo(big)>=0);
                        } else {
                            return (Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(max).CompareTo(big)>=0);
                        }
                    }
                }
            }
            throw new Saxon.Ejava.lang.IllegalArgumentException(("No range information found for integer subtype "+type.getDescription()));
        }
        public static Saxon.Hej.type.ConversionResult stringToInteger(string s) {
            int len = s.Length;
            int start = 0;
            int last = (len-1);
            while (((start<len) && (s[start]<=0x20))) {
                start++;
            }
            while (((last>start) && (s[last]<=0x20))) {
                last--;
            }
            if ((start>last)) {
                return new Saxon.Hej.type.ValidationFailure("Cannot convert zero-length string to an integer");
            }
            if (((last-start)<16)) {
                bool negative = false;
                long value = 0;
                int i = start;
                if ((s[i] == '+')) {
                    i++;
                } else {
                    if ((s[i] == '-')) {
                        negative = true;
                        i++;
                    }
                }
                if ((i>last)) {
                    return new Saxon.Hej.type.ValidationFailure((("Cannot convert string "+Saxon.Hej.trans.Err.wrap(s, Saxon.Hej.trans.Err.VALUE))+" to integer: no digits after the sign"));
                }
                while ((i<=last)) {
                    int d = s[i++];
                    if (((d>='0') && (d<='9'))) {
                        value = ((10*value)+((d-'0')));
                    } else {
                        return new Saxon.Hej.type.ValidationFailure((("Cannot convert string "+Saxon.Hej.trans.Err.wrap(s, Saxon.Hej.trans.Err.VALUE))+" to an integer"));
                    }
                }
                return Saxon.Hej.value.Int64Value.makeIntegerValue((negative ? -(value) : value));
            } else {
                try {
                    if (((start>0) || (last<(len-1)))) {
                        s = s.Substring(start, (last+1) - start);
                    }
                    if ((s[0] == '+')) {
                        s = s.Substring(1);
                    }
                    if ((s.Length<16)) {
                        return new Saxon.Hej.value.Int64Value(System.Int64.Parse(s));
                    } else {
                        return new Saxon.Hej.value.BigIntegerValue(Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(s));
                    }
                } catch (Saxon.Ejava.lang.NumberFormatException err) {
                    return new Saxon.Hej.type.ValidationFailure((("Cannot convert string "+Saxon.Hej.trans.Err.wrap(s, Saxon.Hej.trans.Err.VALUE))+" to an integer"));
                }
            }
        }
        public static Saxon.Hej.type.ValidationFailure castableAsInteger(Saxon.Hej.str.UnicodeString input) {
            Saxon.Hej.z.IntIterator iter = input.codePoints();
            int state = 0;
            while (iter.hasNext()) {
                int c = iter.next();
                switch (state) {
                  case 0:
                    if (Saxon.Hej.value.Whitespace.isWhite(c)) {
                        state = 0;
                    } else {
                        if (((c == '+') || (c == '-'))) {
                            state = 1;
                        } else {
                            if (((c>='0') && (c<='9'))) {
                                state = 2;
                            } else {
                                return new Saxon.Hej.type.ValidationFailure((("Cannot convert string "+Saxon.Hej.trans.Err.wrap(input, Saxon.Hej.trans.Err.VALUE))+" to an integer: contains a character that is not a digit"));
                            }
                        }
                    }
                    break ;
                  case 1:
                    if (((c>='0') && (c<='9'))) {
                        state = 2;
                    } else {
                        return new Saxon.Hej.type.ValidationFailure(((("Cannot convert string "+Saxon.Hej.trans.Err.wrap(input, Saxon.Hej.trans.Err.VALUE))+" to an integer: expected a digit, found ")+c));
                    }
                    break ;
                  case 2:
                    if (((c>='0') && (c<='9'))) {
                        state = 2;
                    } else {
                        if (Saxon.Hej.value.Whitespace.isWhite(c)) {
                            state = 3;
                        } else {
                            return new Saxon.Hej.type.ValidationFailure(((("Cannot convert string "+Saxon.Hej.trans.Err.wrap(input, Saxon.Hej.trans.Err.VALUE))+" to an integer: expected a digit, found ")+c));
                        }
                    }
                    break ;
                  case 3:
                    if (Saxon.Hej.value.Whitespace.isWhite(c)) {
                        state = 3;
                    } else {
                        return new Saxon.Hej.type.ValidationFailure((((("Cannot convert string "+Saxon.Hej.trans.Err.wrap(input, Saxon.Hej.trans.Err.VALUE))+" to an integer: found ")+c)+" after final whitespace"));
                    }
                    break ;
                  default:
                    throw new Saxon.Ejava.lang.IllegalStateException();
                }
            }
            if (((state == 0) || (state == 1))) {
                return new Saxon.Hej.type.ValidationFailure((("Cannot convert string "+Saxon.Hej.trans.Err.wrap(input, Saxon.Hej.trans.Err.VALUE))+" to an integer: no digits found"));
            }
            return null;
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.INTEGER;
        }
        public abstract override Singulink.Numerics.BigDecimal getDecimalValue();
        public override bool isWholeNumber() {
            return true;
        }
        public abstract Saxon.Hej.value.IntegerValue plus(Saxon.Hej.value.IntegerValue other);
        public abstract Saxon.Hej.value.IntegerValue minus(Saxon.Hej.value.IntegerValue other);
        public abstract Saxon.Hej.value.IntegerValue times(Saxon.Hej.value.IntegerValue other);
        public abstract Saxon.Hej.value.NumericValue div(Saxon.Hej.value.IntegerValue other);
        public Saxon.Hej.value.NumericValue div(Saxon.Hej.value.IntegerValue other, Saxon.Hej.s9api.Location locator) {
            try {
                return div(other);
            } catch (Saxon.Hej.trans.XPathException err) {
                err.maybeSetLocation(locator);
                throw;
            }
        }
        public abstract Saxon.Hej.value.IntegerValue mod(Saxon.Hej.value.IntegerValue other);
        public Saxon.Hej.value.IntegerValue mod(Saxon.Hej.value.IntegerValue other, Saxon.Hej.s9api.Location locator) {
            try {
                return mod(other);
            } catch (Saxon.Hej.trans.XPathException err) {
                err.maybeSetLocation(locator);
                throw;
            }
        }
        public abstract Saxon.Hej.value.IntegerValue idiv(Saxon.Hej.value.IntegerValue other);
        public Saxon.Hej.value.IntegerValue idiv(Saxon.Hej.value.IntegerValue other, Saxon.Hej.s9api.Location locator) {
            try {
                return idiv(other);
            } catch (Saxon.Hej.trans.XPathException err) {
                err.maybeSetLocation(locator);
                throw;
            }
        }
        public abstract System.Numerics.BigInteger asBigInteger();
        protected internal static int signum(int i) {
            return (((i>>31))|(Saxon.Impl.Helpers.Utils.unsignedRightShift(-(i), 31)));
        }
        public override bool isIdentical(Saxon.Hej.value.AtomicValue v) {
            return (((v is Saxon.Hej.value.IntegerValue)) && Equals(v));
        }
    
    }
}