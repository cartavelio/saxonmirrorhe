using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public sealed class DayTimeDurationValue : Saxon.Hej.value.DurationValue, Saxon.Hej.expr.sort.XPathComparable, Saxon.Hej.value.ContextFreeAtomicValue {
        public static Saxon.Hej.type.ConversionResult makeDayTimeDurationValue(Saxon.Hej.str.UnicodeString s) {
            Saxon.Hej.type.ConversionResult d = Saxon.Hej.value.DurationValue.makeDuration(s, false, true);
            if ((d is Saxon.Hej.type.ValidationFailure)) {
                return d;
            }
            Saxon.Hej.value.DurationValue dv = (Saxon.Hej.value.DurationValue)(d);
            return Saxon.Hej.type.Converter.DurationToDayTimeDuration.INSTANCE.convert(dv);
        }
        public DayTimeDurationValue(int sign, int days, int hours, int minutes, long seconds, int microseconds) : base ((sign>0), 0, 0, days, hours, minutes, seconds, microseconds, Saxon.Hej.type.BuiltInAtomicType.DAY_TIME_DURATION) {
        }
        public DayTimeDurationValue(int days, int hours, int minutes, long seconds, int nanoseconds) : base (0, 0, days, hours, minutes, seconds, nanoseconds, Saxon.Hej.type.BuiltInAtomicType.DAY_TIME_DURATION) {
        }
        public DayTimeDurationValue(int days, int hours, int minutes, long seconds, int nanoseconds, Saxon.Hej.type.AtomicType typeLabel) : base (0, 0, days, hours, minutes, seconds, nanoseconds, typeLabel) {
        }
        public override Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            return Saxon.Hej.value.DayTimeDurationValue.fromSeconds(getTotalSeconds(), typeLabel);
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.DAY_TIME_DURATION;
        }
        public override Saxon.Hej.str.UnicodeString getPrimitiveStringValue() {
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder(16);
            if (_negative) {
                sb.append('-');
            }
            int days = getDays();
            int hours = getHours();
            int minutes = getMinutes();
            int seconds = getSeconds();
            sb.append('P');
            if ((days != 0)) {
                sb.append((days+"D"));
            }
            if ((((((days == 0) || (hours != 0)) || (minutes != 0)) || (seconds != 0)) || (_nanoseconds != 0))) {
                sb.append('T');
            }
            if ((hours != 0)) {
                sb.append((hours+"H"));
            }
            if ((minutes != 0)) {
                sb.append((minutes+"M"));
            }
            if ((((seconds != 0) || (_nanoseconds != 0)) || ((((days == 0) && (minutes == 0)) && (hours == 0))))) {
                if ((_nanoseconds == 0)) {
                    sb.append((seconds+"S"));
                } else {
                    formatFractionalSeconds(sb, seconds, (((seconds*1_000_000_000L))+_nanoseconds));
                }
            }
            return sb.toUnicodeString();
        }
        public override double getLengthInSeconds() {
            double a = (_seconds+(((double)(_nanoseconds)/1_000_000_000)));
            return (_negative ? -(a) : a);
        }
        public long getLengthInMicroseconds() {
            if ((_seconds>(System.Int64.MaxValue/1_000_000L))) {
                throw new System.ArithmeticException("Value is too large to be expressed in microseconds");
            }
            long a = ((_seconds*1_000_000L)+((_nanoseconds/1000)));
            return (_negative ? -(a) : a);
        }
        public long getLengthInNanoseconds() {
            if ((_seconds>(System.Int64.MaxValue/1_000_000_000L))) {
                throw new System.ArithmeticException("Value is too large to be expressed in nanoseconds");
            }
            long a = ((_seconds*1_000_000_000L)+_nanoseconds);
            return (_negative ? -(a) : a);
        }
        public static Saxon.Hej.value.DayTimeDurationValue fromSeconds(Singulink.Numerics.BigDecimal seconds) {
            return fromSeconds(seconds, Saxon.Hej.type.BuiltInAtomicType.DAY_TIME_DURATION);
        }
        public static Saxon.Hej.value.DayTimeDurationValue fromSeconds(Singulink.Numerics.BigDecimal seconds, Saxon.Hej.type.AtomicType typeLabel) {
            System.Numerics.BigInteger wholeSeconds = ((System.Numerics.BigInteger)seconds);
            long wholeSecondsL = Saxon.Impl.Helpers.BigIntegerUtils.LongValueExact(wholeSeconds);
            Singulink.Numerics.BigDecimal fractionalPart = Saxon.Impl.Helpers.BigDecimalUtils.Remainder(seconds, Singulink.Numerics.BigDecimal.One);
            Singulink.Numerics.BigDecimal nanoseconds = (fractionalPart*Saxon.Hej.value.BigDecimalValue.BIG_DECIMAL_ONE_BILLION);
            int nanosecondsL = ((int)nanoseconds);
            return new Saxon.Hej.value.DayTimeDurationValue(0, 0, 0, wholeSecondsL, nanosecondsL, typeLabel);
        }
        public static Saxon.Hej.value.DayTimeDurationValue fromMilliseconds(long milliseconds) {
            int sign = System.Math.Sign(milliseconds);
            if ((sign<0)) {
                milliseconds = -(milliseconds);
            }
            try {
                return new Saxon.Hej.value.DayTimeDurationValue(sign, 0, 0, 0, (milliseconds/1000), ((int)(((milliseconds%1000)))*1000));
            } catch (Saxon.Ejava.lang.IllegalArgumentException err) {
                throw new Saxon.Hej.type.ValidationFailure("Duration exceeds limits").makeException();
            }
        }
        public static Saxon.Hej.value.DayTimeDurationValue fromMicroseconds(long microseconds) {
            int sign = System.Math.Sign(microseconds);
            if ((sign<0)) {
                microseconds = -(microseconds);
            }
            return new Saxon.Hej.value.DayTimeDurationValue(sign, 0, 0, 0, (microseconds/1_000_000), (int)(((microseconds%1_000_000))));
        }
        public static Saxon.Hej.value.DayTimeDurationValue fromNanoseconds(long nanoseconds) {
            return new Saxon.Hej.value.DayTimeDurationValue(0, 0, 0, (nanoseconds/1_000_000_000L), (int)(((nanoseconds%1_000_000_000L))));
        }
        public override Saxon.Hej.value.DurationValue multiply(long factor) {
            if ((((System.Math.Abs(factor)<0x7fff_ffff) && (System.Math.Abs(_seconds)<0x7fff_ffff)) && (_nanoseconds == 0))) {
                return new Saxon.Hej.value.DayTimeDurationValue(0, 0, 0, ((_seconds*factor)*((_negative ? -1 : 1))), 0);
            } else {
                return multiply(Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(factor));
            }
        }
        public override Saxon.Hej.value.DayTimeDurationValue multiply(double n) {
            if (System.Double.IsNaN(n)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot multiply a duration by NaN");
                err.setErrorCode("FOCA0005");
                throw err;
            }
            if (System.Double.IsInfinity(n)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot multiply a duration by infinity");
                err.setErrorCode("FODT0002");
                throw err;
            }
            Singulink.Numerics.BigDecimal factor = Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(n);
            return multiply(factor);
        }
        public override Saxon.Hej.value.DayTimeDurationValue multiply(Singulink.Numerics.BigDecimal factor) {
            Singulink.Numerics.BigDecimal secs = getTotalSeconds();
            Singulink.Numerics.BigDecimal product = (secs*factor);
            try {
                return fromSeconds(product);
            } catch (Exception err) when (err is Saxon.Ejava.lang.IllegalArgumentException || err is System.ArithmeticException) {
                if ((err.InnerException is Saxon.Hej.trans.XPathException)) {
                    throw (Saxon.Hej.trans.XPathException)(err.InnerException);
                } else {
                    Saxon.Hej.trans.XPathException err2 = new Saxon.Hej.trans.XPathException("Overflow when multiplying a duration by a number", err);
                    err2.setErrorCode("FODT0002");
                    throw err2;
                }
            }
        }
        public override Saxon.Hej.value.DurationValue divide(double n) {
            if (System.Double.IsNaN(n)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot divide a duration by NaN");
                err.setErrorCode("FOCA0005");
                throw err;
            }
            if ((n == 0)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot divide a duration by zero");
                err.setErrorCode("FODT0002");
                throw err;
            }
            Singulink.Numerics.BigDecimal secs = getTotalSeconds();
            Singulink.Numerics.BigDecimal product = Singulink.Numerics.BigDecimal.Divide(secs, Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(n), 18);
            try {
                return fromSeconds(product);
            } catch (Exception err) when (err is Saxon.Ejava.lang.IllegalArgumentException || err is System.ArithmeticException) {
                if ((err.InnerException is Saxon.Hej.trans.XPathException)) {
                    throw (Saxon.Hej.trans.XPathException)(err.InnerException);
                } else {
                    Saxon.Hej.trans.XPathException err2 = new Saxon.Hej.trans.XPathException("Overflow when dividing a duration by a number", err);
                    err2.setErrorCode("FODT0002");
                    throw err2;
                }
            }
        }
        public override Saxon.Hej.value.BigDecimalValue divide(Saxon.Hej.value.DurationValue other) {
            if ((other is Saxon.Hej.value.DayTimeDurationValue)) {
                Singulink.Numerics.BigDecimal v1 = getTotalSeconds();
                Singulink.Numerics.BigDecimal v2 = other.getTotalSeconds();
                if ((v2.Sign == 0)) {
                    throw new Saxon.Hej.trans.XPathException("Divide by zero (durations)", "FOAR0001");
                }
                return new Saxon.Hej.value.BigDecimalValue(Singulink.Numerics.BigDecimal.Divide(v1, v2, 20, Singulink.Numerics.RoundingMode.MidpointToEven));
            } else {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot divide two durations of different type");
                err.setErrorCode("XPTY0004");
                throw err;
            }
        }
        public override Saxon.Hej.value.DurationValue add(Saxon.Hej.value.DurationValue other) {
            if ((other is Saxon.Hej.value.DayTimeDurationValue)) {
                Saxon.Hej.value.DayTimeDurationValue d2 = (Saxon.Hej.value.DayTimeDurationValue)(other);
                if ((((((_seconds|d2._seconds))&0x7fff_ffff_0000_0000L)) != 0)) {
                    try {
                        Singulink.Numerics.BigDecimal v1 = getTotalSeconds();
                        Singulink.Numerics.BigDecimal v2 = other.getTotalSeconds();
                        return fromSeconds((v1+v2));
                    } catch (Saxon.Ejava.lang.IllegalArgumentException e) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Overflow when adding two durations");
                        err.setErrorCode("FODT0002");
                        throw err;
                    }
                } else {
                    return Saxon.Hej.value.DayTimeDurationValue.fromNanoseconds((getLengthInNanoseconds()+d2.getLengthInNanoseconds()));
                }
            } else {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot add two durations of different type");
                err.setErrorCode("XPTY0004");
                throw err;
            }
        }
        public override Saxon.Hej.value.DurationValue subtract(Saxon.Hej.value.DurationValue other) {
            if ((other is Saxon.Hej.value.DayTimeDurationValue)) {
                Saxon.Hej.value.DayTimeDurationValue d2 = (Saxon.Hej.value.DayTimeDurationValue)(other);
                if ((((((_seconds|d2._seconds))&0x7fff_ffff_0000_0000L)) != 0)) {
                    try {
                        Singulink.Numerics.BigDecimal v1 = getTotalSeconds();
                        Singulink.Numerics.BigDecimal v2 = other.getTotalSeconds();
                        return fromSeconds((v1-v2));
                    } catch (Saxon.Ejava.lang.IllegalArgumentException e) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Overflow when subtracting two durations");
                        err.setErrorCode("FODT0002");
                        throw err;
                    }
                } else {
                    return Saxon.Hej.value.DayTimeDurationValue.fromNanoseconds((getLengthInNanoseconds()-d2.getLengthInNanoseconds()));
                }
            } else {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot subtract two durations of different type");
                err.setErrorCode("XPTY0004");
                throw err;
            }
        }
        public override Saxon.Hej.value.DurationValue negate() {
            if (_negative) {
                return new Saxon.Hej.value.DayTimeDurationValue(0, 0, 0, _seconds, _nanoseconds);
            } else {
                return new Saxon.Hej.value.DayTimeDurationValue(0, 0, 0, -(_seconds), -(_nanoseconds));
            }
        }
        public override Saxon.Hej.expr.sort.XPathComparable getXPathComparable(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return this;
        }
        public Saxon.Hej.expr.sort.XPathComparable getXPathComparable() {
            return this;
        }
        public int compareTo(Saxon.Hej.expr.sort.XPathComparable other) {
            if ((other is Saxon.Hej.value.DayTimeDurationValue)) {
                Saxon.Ejava.util.Objects.requireNonNull(other);
                Saxon.Hej.value.DayTimeDurationValue dtd = (Saxon.Hej.value.DayTimeDurationValue)(other);
                if ((this._negative != dtd._negative)) {
                    return (this._negative ? -1 : +(1));
                } else {
                    if ((this._seconds != dtd._seconds)) {
                        return (this._seconds.CompareTo(dtd._seconds)*((this._negative ? -1 : +(1))));
                    } else {
                        return (this._nanoseconds.CompareTo(dtd._nanoseconds)*((this._negative ? -1 : +(1))));
                    }
                }
            } else {
                throw new System.InvalidCastException(("Cannot compare xs:dayTimeDuration to "+other));
            }
        }
        public override Saxon.Hej.expr.sort.AtomicMatchKey getXPathMatchKey(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return this;
        }
    
    }
}