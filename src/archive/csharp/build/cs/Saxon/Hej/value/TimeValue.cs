using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public sealed class TimeValue : Saxon.Hej.value.CalendarValue, Saxon.Hej.expr.sort.XPathComparable {
        private readonly byte hour;
        private readonly byte minute;
        private readonly byte second;
        private readonly int nanosecond;
        public TimeValue(byte hour, byte minute, byte second, int microsecond, int tzMinutes) : base (Saxon.Hej.type.BuiltInAtomicType.TIME, tzMinutes) {
            this.hour = hour;
            this.minute = minute;
            this.second = second;
            this.nanosecond = (microsecond*1000);
        }
        public TimeValue(byte hour, byte minute, byte second, int nanosecond, int tzMinutes, Saxon.Hej.type.AtomicType typeLabel) : base (typeLabel, tzMinutes) {
            this.hour = hour;
            this.minute = minute;
            this.second = second;
            this.nanosecond = nanosecond;
        }
        public Saxon.Hej.value.TimeValue makeTimeValue(byte hour, byte minute, byte second, int nanosecond, int tz) {
            return new Saxon.Hej.value.TimeValue(hour, minute, second, nanosecond, tz, Saxon.Hej.type.BuiltInAtomicType.TIME);
        }
        public static Saxon.Hej.type.ConversionResult makeTimeValue(Saxon.Hej.str.UnicodeString s) {
            Saxon.Ejava.util.StringTokenizer tok = new Saxon.Ejava.util.StringTokenizer(Saxon.Hej.value.Whitespace.trim(s).ToString(), "-:.+Z", true);
            if (!(tok.hasMoreTokens())) {
                return badTime("too short", s);
            }
            string part = tok.nextToken();
            if ((part.Length != 2)) {
                return badTime("hour must be two digits", s);
            }
            int value = Saxon.Hej.value.DurationValue.simpleInteger(part);
            if ((value<0)) {
                return badTime("Non-numeric hour component", s);
            }
            byte hour = (byte)(value);
            if ((hour>24)) {
                return badTime("hour is out of range", s);
            }
            if (!(tok.hasMoreTokens())) {
                return badTime("too short", s);
            }
            if (!(":".Equals(tok.nextToken()))) {
                return badTime("wrong delimiter after hour", s);
            }
            if (!(tok.hasMoreTokens())) {
                return badTime("too short", s);
            }
            part = tok.nextToken();
            if ((part.Length != 2)) {
                return badTime("minute must be two digits", s);
            }
            value = Saxon.Hej.value.DurationValue.simpleInteger(part);
            if ((value<0)) {
                return badTime("Non-numeric minute component", s);
            }
            byte minute = (byte)(value);
            if ((minute>59)) {
                return badTime("minute is out of range", s);
            }
            if (((hour == 24) && (minute != 0))) {
                return badTime("If hour is 24, minute must be 00", s);
            }
            if (!(tok.hasMoreTokens())) {
                return badTime("too short", s);
            }
            if (!(":".Equals(tok.nextToken()))) {
                return badTime("wrong delimiter after minute", s);
            }
            if (!(tok.hasMoreTokens())) {
                return badTime("too short", s);
            }
            part = tok.nextToken();
            if ((part.Length != 2)) {
                return badTime("second must be two digits", s);
            }
            value = Saxon.Hej.value.DurationValue.simpleInteger(part);
            if ((value<0)) {
                return badTime("Non-numeric second component", s);
            }
            byte second = (byte)(value);
            if ((second>59)) {
                return badTime("second is out of range", s);
            }
            if (((hour == 24) && (second != 0))) {
                return badTime("If hour is 24, second must be 00", s);
            }
            int tz = NO_TIMEZONE;
            bool negativeTz = false;
            int state = 0;
            int nanosecond = 0;
            while (tok.hasMoreTokens()) {
                if ((state == 9)) {
                    return badTime("characters after the end", s);
                }
                string delim = tok.nextToken();
                if (".".Equals(delim)) {
                    if ((state != 0)) {
                        return badTime("decimal separator occurs twice", s);
                    }
                    if (!(tok.hasMoreTokens())) {
                        return badTime("decimal point must be followed by digits", s);
                    }
                    part = tok.nextToken();
                    if (((part.Length>9) && System.Text.RegularExpressions.Regex.IsMatch(part, "^[0-9]+$"))) {
                        part = part.Substring(0, 9 - 0);
                    }
                    value = Saxon.Hej.value.DurationValue.simpleInteger(part);
                    if ((value<0)) {
                        return badTime("Non-numeric fractional seconds component", s);
                    }
                    double fractionalSeconds = Saxon.Impl.Helpers.Utils.ParseDouble(('.'+part));
                    nanosecond = (int)((long)System.Math.Round((fractionalSeconds*1_000_000_000), System.MidpointRounding.AwayFromZero));
                    if (((hour == 24) && (nanosecond != 0))) {
                        return badTime("If hour is 24, fractional seconds must be 0", s);
                    }
                    state = 1;
                } else {
                    if ("Z".Equals(delim)) {
                        if ((state>1)) {
                            return badTime("Z cannot occur here", s);
                        }
                        tz = 0;
                        state = 9;
                    } else {
                        if (("+".Equals(delim) || "-".Equals(delim))) {
                            if ((state>1)) {
                                return badTime((delim+" cannot occur here"), s);
                            }
                            state = 2;
                            if (!(tok.hasMoreTokens())) {
                                return badTime("missing timezone", s);
                            }
                            part = tok.nextToken();
                            if ((part.Length != 2)) {
                                return badTime("timezone hour must be two digits", s);
                            }
                            value = Saxon.Hej.value.DurationValue.simpleInteger(part);
                            if ((value<0)) {
                                return badTime("Non-numeric timezone hour component", s);
                            }
                            tz = (value*60);
                            if ((tz>(14*60))) {
                                return badTime("timezone hour is out of range", s);
                            }
                            if ("-".Equals(delim)) {
                                negativeTz = true;
                            }
                        } else {
                            if (":".Equals(delim)) {
                                if ((state != 2)) {
                                    return badTime("colon cannot occur here", s);
                                }
                                state = 9;
                                part = tok.nextToken();
                                value = Saxon.Hej.value.DurationValue.simpleInteger(part);
                                if ((value<0)) {
                                    return badTime("Non-numeric timezone minute component", s);
                                }
                                int tzminute = value;
                                if ((part.Length != 2)) {
                                    return badTime("timezone minute must be two digits", s);
                                }
                                if ((tzminute>59)) {
                                    return badTime("timezone minute is out of range", s);
                                }
                                tz += tzminute;
                                if (negativeTz) {
                                    tz = -(tz);
                                }
                            } else {
                                return badTime("timezone format is incorrect", s);
                            }
                        }
                    }
                }
            }
            if (((state == 2) || (state == 3))) {
                return badTime("timezone incomplete", s);
            }
            if ((hour == 24)) {
                hour = 0;
            }
            return new Saxon.Hej.value.TimeValue(hour, minute, second, nanosecond, tz, Saxon.Hej.type.BuiltInAtomicType.TIME);
        }
        private static Saxon.Hej.type.ValidationFailure badTime(string msg, Saxon.Hej.str.UnicodeString value) {
            Saxon.Hej.type.ValidationFailure err = new Saxon.Hej.type.ValidationFailure((((("Invalid time "+Saxon.Hej.trans.Err.wrap(value, Saxon.Hej.trans.Err.VALUE))+" (")+msg)+")"));
            err.setErrorCode("FORG0001");
            return err;
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.TIME;
        }
        public byte getHour() {
            return hour;
        }
        public byte getMinute() {
            return minute;
        }
        public byte getSecond() {
            return second;
        }
        public int getMicrosecond() {
            return (nanosecond/1000);
        }
        public int getNanosecond() {
            return nanosecond;
        }
        public override Saxon.Hej.str.UnicodeString getPrimitiveStringValue() {
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder(16);
            appendTwoDigits(sb, hour);
            sb.append(':');
            appendTwoDigits(sb, minute);
            sb.append(':');
            appendTwoDigits(sb, second);
            if ((nanosecond != 0)) {
                sb.append('.');
                int ms = nanosecond;
                int div = 100_000_000;
                while ((ms>0)) {
                    int d = (ms/div);
                    sb.append((char)(((d+'0'))));
                    ms = (ms%div);
                    div /= 10;
                }
            }
            if (hasTimezone()) {
                appendTimezone(sb);
            }
            return sb.toUnicodeString();
        }
        public override Saxon.Hej.str.UnicodeString getCanonicalLexicalRepresentation() {
            if ((hasTimezone() && (getTimezoneInMinutes() != 0))) {
                return adjustTimezone(0).getUnicodeStringValue();
            } else {
                return this.getUnicodeStringValue();
            }
        }
        public override Saxon.Hej.value.DateTimeValue toDateTime() {
            return new Saxon.Hej.value.DateTimeValue(1972, (byte)(12), (byte)(31), hour, minute, second, nanosecond, getTimezoneInMinutes());
        }
        public override Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            return new Saxon.Hej.value.TimeValue(hour, minute, second, nanosecond, getTimezoneInMinutes(), typeLabel);
        }
        public override Saxon.Hej.value.TimeValue adjustTimezone(int timezone) {
            Saxon.Hej.value.DateTimeValue dt = toDateTime().adjustTimezone(timezone);
            return new Saxon.Hej.value.TimeValue(dt.getHour(), dt.getMinute(), dt.getSecond(), dt.getNanosecond(), dt.getTimezoneInMinutes(), Saxon.Hej.type.BuiltInAtomicType.TIME);
        }
        public override Saxon.Hej.value.AtomicValue getComponent(Saxon.Hej.functions.AccessorFn.Component component) {
            switch (component) {
              case Saxon.Hej.functions.AccessorFn.Component.HOURS:
                return Saxon.Hej.value.Int64Value.makeIntegerValue(hour);
              case Saxon.Hej.functions.AccessorFn.Component.MINUTES:
                return Saxon.Hej.value.Int64Value.makeIntegerValue(minute);
              case Saxon.Hej.functions.AccessorFn.Component.SECONDS:
                Singulink.Numerics.BigDecimal d = Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(nanosecond);
                d = Singulink.Numerics.BigDecimal.Divide(d, Saxon.Hej.value.BigDecimalValue.BIG_DECIMAL_ONE_BILLION, 6, Singulink.Numerics.RoundingMode.MidpointAwayFromZero);
                d = (d+Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(second));
                return new Saxon.Hej.value.BigDecimalValue(d);
              case Saxon.Hej.functions.AccessorFn.Component.WHOLE_SECONDS:
                return Saxon.Hej.value.Int64Value.makeIntegerValue(second);
              case Saxon.Hej.functions.AccessorFn.Component.MICROSECONDS:
                return new Saxon.Hej.value.Int64Value((nanosecond/1000));
              case Saxon.Hej.functions.AccessorFn.Component.NANOSECONDS:
                return new Saxon.Hej.value.Int64Value(nanosecond);
              case Saxon.Hej.functions.AccessorFn.Component.TIMEZONE:
                if (hasTimezone()) {
                    return Saxon.Hej.value.DayTimeDurationValue.fromMilliseconds((60000L*getTimezoneInMinutes()));
                } else {
                    return null;
                }
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown component for time: "+component));
            }
        }
        public override Saxon.Hej.expr.sort.XPathComparable getXPathComparable(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            if (hasTimezone()) {
                return this;
            } else {
                if ((implicitTimezone == MISSING_TIMEZONE)) {
                    throw new Saxon.Hej.trans.NoDynamicContextException("Unknown implicit timezone");
                } else {
                    return adjustTimezone(implicitTimezone);
                }
            }
        }
        public int compareTo(Saxon.Hej.expr.sort.XPathComparable other) {
            if ((other is Saxon.Hej.value.TimeValue)) {
                Saxon.Hej.value.TimeValue otherTime = (Saxon.Hej.value.TimeValue)(other);
                if ((getTimezoneInMinutes() == otherTime.getTimezoneInMinutes())) {
                    if ((hour != otherTime.hour)) {
                        return Saxon.Hej.value.IntegerValue.signum((hour-otherTime.hour));
                    } else {
                        if ((minute != otherTime.minute)) {
                            return Saxon.Hej.value.IntegerValue.signum((minute-otherTime.minute));
                        } else {
                            if ((second != otherTime.second)) {
                                return Saxon.Hej.value.IntegerValue.signum((second-otherTime.second));
                            } else {
                                if ((nanosecond != otherTime.nanosecond)) {
                                    return Saxon.Hej.value.IntegerValue.signum((nanosecond-otherTime.nanosecond));
                                } else {
                                    return 0;
                                }
                            }
                        }
                    }
                } else {
                    return toDateTime().compareTo(otherTime.toDateTime());
                }
            } else {
                throw new System.InvalidCastException(("Cannot compare xs:time to "+other));
            }
        }
        public override int compareTo(Saxon.Hej.value.CalendarValue other, int implicitTimezone) {
            if (!(((other is Saxon.Hej.value.TimeValue)))) {
                throw new System.InvalidCastException(("Time values are not comparable to "+other.GetType()));
            }
            Saxon.Hej.value.TimeValue otherTime = (Saxon.Hej.value.TimeValue)(other);
            if ((getTimezoneInMinutes() == otherTime.getTimezoneInMinutes())) {
                return compareTo(otherTime);
            } else {
                return toDateTime().compareTo(otherTime.toDateTime(), implicitTimezone);
            }
        }
        public Saxon.Hej.value.TimeValue.TimeComparable getSchemaComparable() {
            return new Saxon.Hej.value.TimeValue.TimeComparable(this);
        }
        public class TimeComparable : Saxon.Ejava.lang.Comparable<Saxon.Hej.value.TimeValue.TimeComparable> {
            private readonly Saxon.Hej.value.TimeValue value;
            public TimeComparable(Saxon.Hej.value.TimeValue value)  {
                this.value = value;
            }
            public Saxon.Hej.value.TimeValue asTimeValue() {
                return value;
            }
            public int compareTo(Saxon.Hej.value.TimeValue.TimeComparable o) {
                Saxon.Hej.value.DateTimeValue dt0 = asTimeValue().toDateTime();
                Saxon.Hej.value.DateTimeValue dt1 = o.asTimeValue().toDateTime();
                return dt0.getSchemaComparable().compareTo(dt1.getSchemaComparable());
            }
            public override bool Equals(System.Object o) {
                return ((o is Saxon.Hej.value.TimeValue.TimeComparable) && (compareTo((Saxon.Hej.value.TimeValue.TimeComparable)(o)) == 0));
            }
            public override int GetHashCode() {
                return value.toDateTime().getSchemaComparable().GetHashCode();
            }
        
        }
        public override bool Equals(System.Object other) {
            return ((other is Saxon.Hej.value.TimeValue) && (compareTo((Saxon.Hej.value.TimeValue)(other)) == 0));
        }
        public override int GetHashCode() {
            return Saxon.Hej.value.DateTimeValue.computeHashCode(1951, (byte)(10), (byte)(11), hour, minute, second, nanosecond, getTimezoneInMinutes());
        }
        public override Saxon.Hej.value.TimeValue add(Saxon.Hej.value.DurationValue duration) {
            if ((duration is Saxon.Hej.value.DayTimeDurationValue)) {
                Saxon.Hej.value.DateTimeValue dt = toDateTime().add(duration);
                return new Saxon.Hej.value.TimeValue(dt.getHour(), dt.getMinute(), dt.getSecond(), dt.getNanosecond(), getTimezoneInMinutes(), Saxon.Hej.type.BuiltInAtomicType.TIME);
            } else {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Time+Duration arithmetic is supported only for xs:dayTimeDuration");
                err.setErrorCode("XPTY0004");
                err.setIsTypeError(true);
                throw err;
            }
        }
        public override Saxon.Hej.value.DayTimeDurationValue subtract(Saxon.Hej.value.CalendarValue other, Saxon.Hej.expr.XPathContext context) {
            if (!(((other is Saxon.Hej.value.TimeValue)))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("First operand of '-' is a time, but the second is not");
                err.setIsTypeError(true);
                throw err;
            }
            return base.subtract(other, context);
        }
    
    }
}