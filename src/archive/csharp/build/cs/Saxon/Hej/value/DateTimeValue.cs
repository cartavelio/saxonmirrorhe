using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    #pragma warning disable 659
    public sealed class DateTimeValue : Saxon.Hej.value.CalendarValue, Saxon.Hej.expr.sort.XPathComparable {
    // Injecting C# code
        
        public static Saxon.Hej.value.DateTimeValue fromDateTime(System.DateTime dt) {
            Saxon.Hej.value.DateTimeValue dtv = new (dt.Year, (byte)dt.Month, (byte)dt.Day, (byte)dt.Hour, (byte)dt.Minute, (byte)dt.Second, dt.Millisecond * 1000000, 0);
            return (Saxon.Hej.value.DateTimeValue)dtv.copyAsSubType(Saxon.Hej.type.BuiltInAtomicType.DATE_TIME_STAMP);
        }
         public static Saxon.Hej.value.DateTimeValue fromDateTimeOffset(System.DateTimeOffset dt) {
            Saxon.Hej.value.DateTimeValue dtv = new (dt.Year, (byte)dt.Month, (byte)dt.Day, (byte)dt.Hour, (byte)dt.Minute, (byte)dt.Second, dt.Millisecond * 1000000, (int)dt.Offset.TotalMinutes);
            return (Saxon.Hej.value.DateTimeValue)dtv.copyAsSubType(Saxon.Hej.type.BuiltInAtomicType.DATE_TIME_STAMP);
        }
    // End of injected C# code
        private readonly int year;
        private readonly byte month;
        private readonly byte day;
        private readonly byte hour;
        private readonly byte minute;
        private readonly byte second;
        private readonly int nanosecond;
        private readonly bool hasNoYearZero;
        public DateTimeValue(int year, byte month, byte day, byte hour, byte minute, byte second, int nanosecond, bool hasNoYearZero, int tzMinutes, Saxon.Hej.type.AtomicType typeLabel) : base (typeLabel, tzMinutes) {
            this.year = year;
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.second = second;
            this.nanosecond = nanosecond;
            this.hasNoYearZero = hasNoYearZero;
        }
        private class MutableDateTimeValue {
            public int year;
            public byte month;
            public byte day;
            public byte hour;
            public byte minute;
            public byte second;
            public int nanosecond;
            public bool hasNoYearZero;
            public int tzMinutes = NO_TIMEZONE;
            public Saxon.Hej.type.AtomicType typeLabel = Saxon.Hej.type.BuiltInAtomicType.DATE_TIME;
        
        }
        private Saxon.Hej.value.DateTimeValue.MutableDateTimeValue makeMutableCopy() {
            Saxon.Hej.value.DateTimeValue.MutableDateTimeValue m = new Saxon.Hej.value.DateTimeValue.MutableDateTimeValue();
            m.year = year;
            m.month = month;
            m.day = day;
            m.hour = hour;
            m.minute = minute;
            m.second = second;
            m.nanosecond = nanosecond;
            m.hasNoYearZero = hasNoYearZero;
            m.tzMinutes = getTimezoneInMinutes();
            m.typeLabel = typeLabel;
            return m;
        }
        private static Saxon.Hej.value.DateTimeValue fromMutableCopy(Saxon.Hej.value.DateTimeValue.MutableDateTimeValue m) {
            return new Saxon.Hej.value.DateTimeValue(m.year, m.month, m.day, m.hour, m.minute, m.second, m.nanosecond, m.hasNoYearZero, m.tzMinutes, m.typeLabel);
        }
        public static Saxon.Hej.value.DateTimeValue getCurrentDateTime(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Controller c;
            if (((context == null) || ((c = context.getController()) == null))) {
                return now();
            } else {
                return c.getCurrentDateTime();
            }
        }
        public static Saxon.Hej.value.DateTimeValue now() {
            return fromDateTimeOffset(System.DateTimeOffset.Now);
        }
        public static Saxon.Hej.value.DateTimeValue fromJavaTime(long time) {
            return EPOCH.add(Saxon.Hej.value.DayTimeDurationValue.fromMilliseconds(time));
        }
        public static Saxon.Hej.value.DateTimeValue fromJavaInstant(long seconds, int nano) {
            return EPOCH.add(Saxon.Hej.value.DayTimeDurationValue.fromSeconds(Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(seconds)).add(Saxon.Hej.value.DayTimeDurationValue.fromNanoseconds(nano)));
        }
        public static readonly Saxon.Hej.value.DateTimeValue EPOCH = new Saxon.Hej.value.DateTimeValue(1970, (byte)(1), (byte)(1), (byte)(0), (byte)(0), (byte)(0), 0, 0, true);
        public static Saxon.Hej.value.DateTimeValue makeDateTimeValue(Saxon.Hej.value.DateValue date, Saxon.Hej.value.TimeValue time) {
            if (((date == null) || (time == null))) {
                return null;
            }
            int tz1 = date.getTimezoneInMinutes();
            int tz2 = time.getTimezoneInMinutes();
            if ((((tz1 != NO_TIMEZONE) && (tz2 != NO_TIMEZONE)) && (tz1 != tz2))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Supplied date and time are in different timezones");
                err.setErrorCode("FORG0008");
                throw err;
            }
            Saxon.Hej.value.DateTimeValue.MutableDateTimeValue v = date.toDateTime().makeMutableCopy();
            v.hour = time.getHour();
            v.minute = time.getMinute();
            v.second = time.getSecond();
            v.nanosecond = time.getNanosecond();
            v.tzMinutes = System.Math.Max(tz1, tz2);
            v.typeLabel = Saxon.Hej.type.BuiltInAtomicType.DATE_TIME;
            v.hasNoYearZero = date.hasNoYearZero;
            return fromMutableCopy(v);
        }
        public static Saxon.Hej.type.ConversionResult makeDateTimeValue(Saxon.Hej.str.UnicodeString s, Saxon.Hej.lib.ConversionRules rules) {
            Saxon.Hej.value.DateTimeValue.MutableDateTimeValue dt = new Saxon.Hej.value.DateTimeValue.MutableDateTimeValue();
            dt.hasNoYearZero = !(rules.isAllowYearZero());
            Saxon.Ejava.util.StringTokenizer tok = new Saxon.Ejava.util.StringTokenizer(Saxon.Hej.value.Whitespace.trim(s).ToString(), "-:.+TZ", true);
            if (!(tok.hasMoreTokens())) {
                return badDate("too short", s);
            }
            string part = tok.nextToken();
            int era = +(1);
            if ("+".Equals(part)) {
                return badDate("Date must not start with '+' sign", s);
            } else {
                if ("-".Equals(part)) {
                    era = -1;
                    if (!(tok.hasMoreTokens())) {
                        return badDate("No year after '-'", s);
                    }
                    part = tok.nextToken();
                }
            }
            int value = Saxon.Hej.value.DurationValue.simpleInteger(part);
            if ((value<0)) {
                if ((value == -1)) {
                    return badDate("Non-numeric year component", s);
                } else {
                    return badDate("Year is outside the range that Saxon can handle", s, "FODT0001");
                }
            }
            dt.year = (value*era);
            if ((part.Length<4)) {
                return badDate("Year is less than four digits", s);
            }
            if (((part.Length>4) && (part[0] == '0'))) {
                return badDate("When year exceeds 4 digits, leading zeroes are not allowed", s);
            }
            if (((dt.year == 0) && !(rules.isAllowYearZero()))) {
                return badDate("Year zero is not allowed", s);
            }
            if (((era<0) && !(rules.isAllowYearZero()))) {
                dt.year++;
            }
            if (!(tok.hasMoreTokens())) {
                return badDate("Too short", s);
            }
            if (!("-".Equals(tok.nextToken()))) {
                return badDate("Wrong delimiter after year", s);
            }
            if (!(tok.hasMoreTokens())) {
                return badDate("Too short", s);
            }
            part = tok.nextToken();
            if ((part.Length != 2)) {
                return badDate("Month must be two digits", s);
            }
            value = Saxon.Hej.value.DurationValue.simpleInteger(part);
            if ((value<0)) {
                return badDate("Non-numeric month component", s);
            }
            dt.month = (byte)(value);
            if (((dt.month<1) || (dt.month>12))) {
                return badDate("Month is out of range", s);
            }
            if (!(tok.hasMoreTokens())) {
                return badDate("Too short", s);
            }
            if (!("-".Equals(tok.nextToken()))) {
                return badDate("Wrong delimiter after month", s);
            }
            if (!(tok.hasMoreTokens())) {
                return badDate("Too short", s);
            }
            part = (string)(tok.nextToken());
            if ((part.Length != 2)) {
                return badDate("Day must be two digits", s);
            }
            value = Saxon.Hej.value.DurationValue.simpleInteger(part);
            if ((value<0)) {
                return badDate("Non-numeric day component", s);
            }
            dt.day = (byte)(value);
            if (((dt.day<1) || (dt.day>31))) {
                return badDate("Day is out of range", s);
            }
            if (!(tok.hasMoreTokens())) {
                return badDate("Too short", s);
            }
            if (!("T".Equals(tok.nextToken()))) {
                return badDate("Wrong delimiter after day", s);
            }
            if (!(tok.hasMoreTokens())) {
                return badDate("Too short", s);
            }
            part = tok.nextToken();
            if ((part.Length != 2)) {
                return badDate("Hour must be two digits", s);
            }
            value = Saxon.Hej.value.DurationValue.simpleInteger(part);
            if ((value<0)) {
                return badDate("Non-numeric hour component", s);
            }
            dt.hour = (byte)(value);
            if ((dt.hour>24)) {
                return badDate("Hour is out of range", s);
            }
            if (!(tok.hasMoreTokens())) {
                return badDate("Too short", s);
            }
            if (!(":".Equals(tok.nextToken()))) {
                return badDate("Wrong delimiter after hour", s);
            }
            if (!(tok.hasMoreTokens())) {
                return badDate("Too short", s);
            }
            part = tok.nextToken();
            if ((part.Length != 2)) {
                return badDate("Minute must be two digits", s);
            }
            value = Saxon.Hej.value.DurationValue.simpleInteger(part);
            if ((value<0)) {
                return badDate("Non-numeric minute component", s);
            }
            dt.minute = (byte)(value);
            if ((dt.minute>59)) {
                return badDate("Minute is out of range", s);
            }
            if (((dt.hour == 24) && (dt.minute != 0))) {
                return badDate("If hour is 24, minute must be 00", s);
            }
            if (!(tok.hasMoreTokens())) {
                return badDate("Too short", s);
            }
            if (!(":".Equals(tok.nextToken()))) {
                return badDate("Wrong delimiter after minute", s);
            }
            if (!(tok.hasMoreTokens())) {
                return badDate("Too short", s);
            }
            part = tok.nextToken();
            if ((part.Length != 2)) {
                return badDate("Second must be two digits", s);
            }
            value = Saxon.Hej.value.DurationValue.simpleInteger(part);
            if ((value<0)) {
                return badDate("Non-numeric second component", s);
            }
            dt.second = (byte)(value);
            if ((dt.second>59)) {
                return badDate("Second is out of range", s);
            }
            if (((dt.hour == 24) && (dt.second != 0))) {
                return badDate("If hour is 24, second must be 00", s);
            }
            int tz = 0;
            bool negativeTz = false;
            int state = 0;
            while (tok.hasMoreTokens()) {
                if ((state == 9)) {
                    return badDate("Characters after the end", s);
                }
                string delim = (string)(tok.nextToken());
                if (".".Equals(delim)) {
                    if ((state != 0)) {
                        return badDate("Decimal separator occurs twice", s);
                    }
                    if (!(tok.hasMoreTokens())) {
                        return badDate("Decimal point must be followed by digits", s);
                    }
                    part = tok.nextToken();
                    if (((part.Length>9) && System.Text.RegularExpressions.Regex.IsMatch(part, "^[0-9]+$"))) {
                        part = part.Substring(0, 9 - 0);
                    }
                    value = Saxon.Hej.value.DurationValue.simpleInteger(part);
                    if ((value<0)) {
                        return badDate("Non-numeric fractional seconds component", s);
                    }
                    double fractionalSeconds = Saxon.Impl.Helpers.Utils.ParseDouble(('.'+part));
                    int nanoSeconds = (int)((long)System.Math.Round((fractionalSeconds*1_000_000_000), System.MidpointRounding.AwayFromZero));
                    if ((nanoSeconds == 1_000_000_000)) {
                        nanoSeconds--;
                    }
                    dt.nanosecond = nanoSeconds;
                    if (((dt.hour == 24) && (dt.nanosecond != 0))) {
                        return badDate("If hour is 24, fractional seconds must be 0", s);
                    }
                    state = 1;
                } else {
                    if ("Z".Equals(delim)) {
                        if ((state>1)) {
                            return badDate("Z cannot occur here", s);
                        }
                        tz = 0;
                        state = 9;
                        dt.tzMinutes = 0;
                    } else {
                        if (("+".Equals(delim) || "-".Equals(delim))) {
                            if ((state>1)) {
                                return badDate((delim+" cannot occur here"), s);
                            }
                            state = 2;
                            if (!(tok.hasMoreTokens())) {
                                return badDate("Missing timezone", s);
                            }
                            part = tok.nextToken();
                            if ((part.Length != 2)) {
                                return badDate("Timezone hour must be two digits", s);
                            }
                            value = Saxon.Hej.value.DurationValue.simpleInteger(part);
                            if ((value<0)) {
                                return badDate("Non-numeric timezone hour component", s);
                            }
                            tz = value;
                            if ((tz>14)) {
                                return badDate("Timezone is out of range (-14:00 to +14:00)", s);
                            }
                            tz *= 60;
                            if ("-".Equals(delim)) {
                                negativeTz = true;
                            }
                        } else {
                            if (":".Equals(delim)) {
                                if ((state != 2)) {
                                    return badDate("Misplaced ':'", s);
                                }
                                state = 9;
                                part = tok.nextToken();
                                value = Saxon.Hej.value.DurationValue.simpleInteger(part);
                                if ((value<0)) {
                                    return badDate("Non-numeric timezone minute component", s);
                                }
                                int tzminute = value;
                                if ((part.Length != 2)) {
                                    return badDate("Timezone minute must be two digits", s);
                                }
                                if ((tzminute>59)) {
                                    return badDate("Timezone minute is out of range", s);
                                }
                                if (((System.Math.Abs(tz) == (14*60)) && (tzminute != 0))) {
                                    return badDate("Timezone is out of range (-14:00 to +14:00)", s);
                                }
                                tz += tzminute;
                                if (negativeTz) {
                                    tz = -(tz);
                                }
                                dt.tzMinutes = tz;
                            } else {
                                return badDate("Timezone format is incorrect", s);
                            }
                        }
                    }
                }
            }
            if (((state == 2) || (state == 3))) {
                return badDate("Timezone incomplete", s);
            }
            bool midnight = false;
            if ((dt.hour == 24)) {
                dt.hour = 0;
                midnight = true;
            }
            if (!(Saxon.Hej.value.DateValue.isValidDate(dt.year, dt.month, dt.day))) {
                return badDate("Non-existent date", s);
            }
            if (midnight) {
                Saxon.Hej.value.DateValue t = Saxon.Hej.value.DateValue.tomorrow(dt.year, dt.month, dt.day);
                dt.year = t.getYear();
                dt.month = t.getMonth();
                dt.day = t.getDay();
            }
            dt.typeLabel = Saxon.Hej.type.BuiltInAtomicType.DATE_TIME;
            return fromMutableCopy(dt);
        }
        public static Saxon.Hej.value.DateTimeValue parse(Saxon.Hej.str.UnicodeString s) {
            Saxon.Hej.type.ConversionResult result = makeDateTimeValue(s, Saxon.Hej.lib.ConversionRules.DEFAULT);
            if ((result is Saxon.Hej.type.ValidationFailure)) {
                throw new System.FormatException(((Saxon.Hej.type.ValidationFailure)(result)).getMessage() + " : " + s.ToString());
            } else {
                return (Saxon.Hej.value.DateTimeValue)(result);
            }
        }
        private static Saxon.Hej.type.ValidationFailure badDate(string msg, Saxon.Hej.str.UnicodeString value) {
            Saxon.Hej.type.ValidationFailure err = new Saxon.Hej.type.ValidationFailure((((("Invalid dateTime value "+Saxon.Hej.trans.Err.wrap(value, Saxon.Hej.trans.Err.VALUE))+" (")+msg)+")"));
            err.setErrorCode("FORG0001");
            return err;
        }
        private static Saxon.Hej.type.ValidationFailure badDate(string msg, Saxon.Hej.str.UnicodeString value, string errorCode) {
            Saxon.Hej.type.ValidationFailure err = new Saxon.Hej.type.ValidationFailure((((("Invalid dateTime value "+Saxon.Hej.trans.Err.wrap(value, Saxon.Hej.trans.Err.VALUE))+" (")+msg)+")"));
            err.setErrorCode(errorCode);
            return err;
        }
        public DateTimeValue(int year, byte month, byte day, byte hour, byte minute, byte second, int nanosecond, int tz) : this (year, month, day, hour, minute, second, nanosecond, false, tz, Saxon.Hej.type.BuiltInAtomicType.DATE_TIME) {
        }
        public DateTimeValue(int year, byte month, byte day, byte hour, byte minute, byte second, int microsecond, int tz, bool hasNoYearZero) : this (year, month, day, hour, minute, second, (microsecond*1000), hasNoYearZero, tz, Saxon.Hej.type.BuiltInAtomicType.DATE_TIME) {
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.DATE_TIME;
        }
        public int getYear() {
            return year;
        }
        public byte getMonth() {
            return month;
        }
        public byte getDay() {
            return day;
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
        public override Saxon.Hej.value.DateTimeValue toDateTime() {
            return this;
        }
        public bool isXsd10Rules() {
            return hasNoYearZero;
        }
        public override void checkValidInJavascript() {
            if (((year<=0) || (year>9999))) {
                throw new Saxon.Hej.trans.XPathException("Year out of range for SaxonJS", "FODT0001");
            }
        }
        public Saxon.Hej.value.DateTimeValue adjustToUTC(int implicitTimezone) {
            if (hasTimezone()) {
                return adjustTimezone(0);
            } else {
                if (((implicitTimezone == Saxon.Hej.value.CalendarValue.MISSING_TIMEZONE) || (implicitTimezone == Saxon.Hej.value.CalendarValue.NO_TIMEZONE))) {
                    throw new Saxon.Hej.trans.NoDynamicContextException("DateTime operation needs access to implicit timezone");
                }
                Saxon.Hej.value.DateTimeValue.MutableDateTimeValue m = makeMutableCopy();
                m.tzMinutes = implicitTimezone;
                return fromMutableCopy(m).adjustTimezone(0);
            }
        }
        public Singulink.Numerics.BigDecimal toJulianInstant() {
            int julianDay = Saxon.Hej.value.DateValue.getJulianDayNumber(year, month, day);
            long julianSecond = (((julianDay*24L)*60L)*60L);
            julianSecond += ((((((hour*60L)+minute))*60L))+second);
            Singulink.Numerics.BigDecimal j = Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(julianSecond);
            if ((nanosecond == 0)) {
                return j;
            } else {
                return (j+Singulink.Numerics.BigDecimal.Divide(Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(nanosecond), Saxon.Hej.value.BigDecimalValue.BIG_DECIMAL_ONE_BILLION, 9, Singulink.Numerics.RoundingMode.MidpointToEven));
            }
        }
        public static Saxon.Hej.value.DateTimeValue fromJulianInstant(Singulink.Numerics.BigDecimal instant) {
            System.Numerics.BigInteger julianSecond = ((System.Numerics.BigInteger)instant);
            Singulink.Numerics.BigDecimal nanoseconds = ((instant-new Singulink.Numerics.BigDecimal(julianSecond, 0))*Saxon.Hej.value.BigDecimalValue.BIG_DECIMAL_ONE_BILLION);
            long js = ((long)julianSecond);
            long jd = (js/(((24L*60L)*60L)));
            Saxon.Hej.value.DateValue date = Saxon.Hej.value.DateValue.dateFromJulianDayNumber((int)(jd));
            js = (js%(((24L*60L)*60L)));
            byte hour = (byte)(((js/((60L*60L)))));
            js = (js%((60L*60L)));
            byte minute = (byte)(((js/60L)));
            js = (js%60L);
            return new Saxon.Hej.value.DateTimeValue(date.getYear(), date.getMonth(), date.getDay(), hour, minute, (byte)(js), ((int)nanoseconds), 0);
        }
        public long randomSeed() {
            return ((((long)minute)*360 + ((long)second))*60) * 1000000000L + (long)nanosecond;
        }
        public override Saxon.Hej.str.UnicodeString getPrimitiveStringValue() {
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder(32);
            int yr = year;
            if ((year<=0)) {
                yr = (-(yr)+((hasNoYearZero ? 1 : 0)));
                if ((yr != 0)) {
                    sb.append('-');
                }
            }
            appendString(sb, yr, ((yr>9999) ? ((yr+"")).Length : 4));
            sb.append('-');
            appendTwoDigits(sb, month);
            sb.append('-');
            appendTwoDigits(sb, day);
            sb.append('T');
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
        public Saxon.Hej.value.DateValue toDateValue() {
            return new Saxon.Hej.value.DateValue(year, month, day, getTimezoneInMinutes(), hasNoYearZero);
        }
        public Saxon.Hej.value.TimeValue toTimeValue() {
            return new Saxon.Hej.value.TimeValue(hour, minute, second, nanosecond, getTimezoneInMinutes(), Saxon.Hej.type.BuiltInAtomicType.TIME);
        }
        public override Saxon.Hej.str.UnicodeString getCanonicalLexicalRepresentation() {
            if ((hasTimezone() && (getTimezoneInMinutes() != 0))) {
                return adjustTimezone(0).getUnicodeStringValue();
            } else {
                return this.getUnicodeStringValue();
            }
        }
        public override Saxon.Hej.value.DateTimeValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            Saxon.Hej.value.DateTimeValue.MutableDateTimeValue v = makeMutableCopy();
            v.typeLabel = typeLabel;
            return fromMutableCopy(v);
        }
        public override Saxon.Hej.value.DateTimeValue adjustTimezone(int timezone) {
            if ((!(hasTimezone()) || (timezone == NO_TIMEZONE))) {
                Saxon.Hej.value.DateTimeValue.MutableDateTimeValue m = makeMutableCopy();
                m.tzMinutes = timezone;
                return fromMutableCopy(m);
            }
            int oldtz = getTimezoneInMinutes();
            if ((oldtz == timezone)) {
                return this;
            }
            int tz = (timezone-oldtz);
            int h = hour;
            int mi = minute;
            mi += tz;
            if (((mi<0) || (mi>59))) {
                h += (int)(System.Math.Floor((mi/60.0)));
                mi = (((mi+(60*24)))%60);
            }
            if (((h>=0) && (h<24))) {
                return new Saxon.Hej.value.DateTimeValue(year, month, day, (byte)(h), (byte)(mi), second, nanosecond, hasNoYearZero, timezone, Saxon.Hej.type.BuiltInAtomicType.DATE_TIME);
            }
            Saxon.Hej.value.DateTimeValue dt = this;
            while ((h<0)) {
                h += 24;
                Saxon.Hej.value.DateValue t = Saxon.Hej.value.DateValue.yesterday(dt.getYear(), dt.getMonth(), dt.getDay());
                dt = new Saxon.Hej.value.DateTimeValue(t.getYear(), t.getMonth(), t.getDay(), (byte)(h), (byte)(mi), second, nanosecond, hasNoYearZero, timezone, Saxon.Hej.type.BuiltInAtomicType.DATE_TIME);
            }
            if ((h>23)) {
                h -= 24;
                Saxon.Hej.value.DateValue t = Saxon.Hej.value.DateValue.tomorrow(year, month, day);
                dt = new Saxon.Hej.value.DateTimeValue(t.getYear(), t.getMonth(), t.getDay(), (byte)(h), (byte)(mi), second, nanosecond, hasNoYearZero, timezone, Saxon.Hej.type.BuiltInAtomicType.DATE_TIME);
            }
            return dt;
        }
        public override Saxon.Hej.value.DateTimeValue add(Saxon.Hej.value.DurationValue duration) {
            if ((duration is Saxon.Hej.value.DayTimeDurationValue)) {
                Singulink.Numerics.BigDecimal seconds = duration.getTotalSeconds();
                Singulink.Numerics.BigDecimal julian = toJulianInstant();
                julian = (julian+seconds);
                Saxon.Hej.value.DateTimeValue.MutableDateTimeValue dt = fromJulianInstant(julian).makeMutableCopy();
                dt.tzMinutes = getTimezoneInMinutes();
                dt.hasNoYearZero = this.hasNoYearZero;
                return fromMutableCopy(dt);
            } else {
                if ((duration is Saxon.Hej.value.YearMonthDurationValue)) {
                    int months = ((Saxon.Hej.value.YearMonthDurationValue)(duration)).getLengthInMonths();
                    int m = (((month-1))+months);
                    int y = (year+(m/12));
                    m = (m%12);
                    if ((m<0)) {
                        m += 12;
                        y -= 1;
                    }
                    m++;
                    int d = day;
                    while (!(Saxon.Hej.value.DateValue.isValidDate(y, m, d))) {
                        d -= 1;
                    }
                    return new Saxon.Hej.value.DateTimeValue(y, (byte)(m), (byte)(d), hour, minute, second, nanosecond, hasNoYearZero, getTimezoneInMinutes(), Saxon.Hej.type.BuiltInAtomicType.DATE_TIME);
                } else {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("DateTime arithmetic is not supported on xs:duration, only on its subtypes");
                    err.setErrorCode("XPTY0004");
                    err.setIsTypeError(true);
                    throw err;
                }
            }
        }
        public override Saxon.Hej.value.DayTimeDurationValue subtract(Saxon.Hej.value.CalendarValue other, Saxon.Hej.expr.XPathContext context) {
            if (!(((other is Saxon.Hej.value.DateTimeValue)))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("First operand of '-' is a dateTime, but the second is not");
                err.setErrorCode("XPTY0004");
                err.setIsTypeError(true);
                throw err;
            }
            return base.subtract(other, context);
        }
        public Singulink.Numerics.BigDecimal secondsSinceEpoch() {
            try {
                Saxon.Hej.value.DateTimeValue dtv = adjustToUTC(0);
                Singulink.Numerics.BigDecimal d1 = dtv.toJulianInstant();
                Singulink.Numerics.BigDecimal d2 = EPOCH.toJulianInstant();
                return (d1-d2);
            } catch (Saxon.Hej.trans.NoDynamicContextException e) {
                throw new Saxon.Ejava.lang.AssertionError(e);
            }
        }
        public override Saxon.Hej.value.AtomicValue getComponent(Saxon.Hej.functions.AccessorFn.Component component) {
            switch (component) {
              case Saxon.Hej.functions.AccessorFn.Component.YEAR_ALLOWING_ZERO:
                return Saxon.Hej.value.Int64Value.makeIntegerValue(year);
              case Saxon.Hej.functions.AccessorFn.Component.YEAR:
                return Saxon.Hej.value.Int64Value.makeIntegerValue((((year>0) || !(hasNoYearZero)) ? year : (year-1)));
              case Saxon.Hej.functions.AccessorFn.Component.MONTH:
                return Saxon.Hej.value.Int64Value.makeIntegerValue(month);
              case Saxon.Hej.functions.AccessorFn.Component.DAY:
                return Saxon.Hej.value.Int64Value.makeIntegerValue(day);
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
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown component for dateTime: "+component));
            }
        }
        public override int compareTo(Saxon.Hej.value.CalendarValue other, int implicitTimezone) {
            if (!(((other is Saxon.Hej.value.DateTimeValue)))) {
                throw new System.InvalidCastException(("DateTime values are not comparable to "+other.GetType()));
            }
            Saxon.Hej.value.DateTimeValue v2 = (Saxon.Hej.value.DateTimeValue)(other);
            if ((getTimezoneInMinutes() == v2.getTimezoneInMinutes())) {
                if ((year != v2.year)) {
                    return Saxon.Hej.value.IntegerValue.signum((year-v2.year));
                }
                if ((month != v2.month)) {
                    return Saxon.Hej.value.IntegerValue.signum((month-v2.month));
                }
                if ((day != v2.day)) {
                    return Saxon.Hej.value.IntegerValue.signum((day-v2.day));
                }
                if ((hour != v2.hour)) {
                    return Saxon.Hej.value.IntegerValue.signum((hour-v2.hour));
                }
                if ((minute != v2.minute)) {
                    return Saxon.Hej.value.IntegerValue.signum((minute-v2.minute));
                }
                if ((second != v2.second)) {
                    return Saxon.Hej.value.IntegerValue.signum((second-v2.second));
                }
                if ((nanosecond != v2.nanosecond)) {
                    return Saxon.Hej.value.IntegerValue.signum((nanosecond-v2.nanosecond));
                }
                return 0;
            }
            return adjustToUTC(implicitTimezone).compareTo(v2.adjustToUTC(implicitTimezone), implicitTimezone);
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
        public int compareTo(Saxon.Hej.expr.sort.XPathComparable v2) {
            if ((v2 is Saxon.Hej.value.DateTimeValue)) {
                try {
                    return compareTo((Saxon.Hej.value.DateTimeValue)(v2), MISSING_TIMEZONE);
                } catch (System.Exception err) {
                    throw new System.InvalidCastException("DateTime comparison requires access to implicit timezone");
                }
            } else {
                throw new System.InvalidCastException(("Cannot compare xs:dateTime with "+v2.ToString()));
            }
        }
        public Saxon.Hej.value.DateTimeValue.DateTimeComparable getSchemaComparable() {
            return new Saxon.Hej.value.DateTimeValue.DateTimeComparable(this);
        }
        public class DateTimeComparable : Saxon.Ejava.lang.Comparable<Saxon.Hej.value.DateTimeValue.DateTimeComparable> {
            private readonly Saxon.Hej.value.DateTimeValue value;
            public DateTimeComparable(Saxon.Hej.value.DateTimeValue value)  {
                this.value = value;
            }
            public int compareTo(Saxon.Hej.value.DateTimeValue.DateTimeComparable o) {
                Saxon.Hej.value.DateTimeValue dt0 = value;
                Saxon.Hej.value.DateTimeValue dt1 = o.value;
                if (dt0.hasTimezone()) {
                    if (dt1.hasTimezone()) {
                        dt0 = dt0.adjustTimezone(0);
                        dt1 = dt1.adjustTimezone(0);
                        return dt0.compareTo(dt1);
                    } else {
                        Saxon.Hej.value.DateTimeValue dt1max = dt1.adjustTimezone((14*60));
                        if ((dt0.compareTo(dt1max)<0)) {
                            return -1;
                        }
                        Saxon.Hej.value.DateTimeValue dt1min = dt1.adjustTimezone((-14*60));
                        if ((dt0.compareTo(dt1min)>0)) {
                            return +(1);
                        }
                        return Saxon.Hej.om.SequenceTool.INDETERMINATE_ORDERING;
                    }
                } else {
                    if (dt1.hasTimezone()) {
                        Saxon.Hej.value.DateTimeValue dt0min = dt0.adjustTimezone((-14*60));
                        if ((dt0min.compareTo(dt1)<0)) {
                            return -1;
                        }
                        Saxon.Hej.value.DateTimeValue dt0max = dt0.adjustTimezone((14*60));
                        if ((dt0max.compareTo(dt1)>0)) {
                            return +(1);
                        }
                        return Saxon.Hej.om.SequenceTool.INDETERMINATE_ORDERING;
                    } else {
                        dt0 = dt0.adjustTimezone(0);
                        dt1 = dt1.adjustTimezone(0);
                        return dt0.compareTo(dt1);
                    }
                }
            }
            public override bool Equals(System.Object o) {
                return (((o is Saxon.Hej.value.DateTimeValue.DateTimeComparable) && (value.hasTimezone() == ((Saxon.Hej.value.DateTimeValue.DateTimeComparable)(o)).value.hasTimezone())) && (compareTo((Saxon.Hej.value.DateTimeValue.DateTimeComparable)(o)) == 0));
            }
            public override int GetHashCode() {
                Saxon.Hej.value.DateTimeValue dt0 = value.adjustTimezone(0);
                return (((((((dt0.year<<20))^((dt0.month<<16)))^((dt0.day<<11)))^((dt0.hour<<7)))^((dt0.minute<<2)))^(((dt0.second*1_000_000_000)+dt0.nanosecond)));
            }
        
        }
        public override bool Equals(System.Object o) {
            return ((o is Saxon.Hej.value.DateTimeValue) && (compareTo((Saxon.Hej.value.DateTimeValue)(o)) == 0));
        }
        public override int GetHashCode() {
            return computeHashCode(year, month, day, hour, minute, second, nanosecond, getTimezoneInMinutes());
        }
        public static int computeHashCode(int year, byte month, byte day, byte hour, byte minute, byte second, int nanosecond, int tzMinutes) {
            int tz = ((tzMinutes == Saxon.Hej.value.CalendarValue.NO_TIMEZONE) ? 0 : -(tzMinutes));
            int h = hour;
            int mi = minute;
            mi += tz;
            if (((mi<0) || (mi>59))) {
                h += (int)(System.Math.Floor((mi/60.0)));
                mi = (((mi+(60*24)))%60);
            }
            while ((h<0)) {
                h += 24;
                Saxon.Hej.value.DateValue t = Saxon.Hej.value.DateValue.yesterday(year, month, day);
                year = t.getYear();
                month = t.getMonth();
                day = t.getDay();
            }
            while ((h>23)) {
                h -= 24;
                Saxon.Hej.value.DateValue t = Saxon.Hej.value.DateValue.tomorrow(year, month, day);
                year = t.getYear();
                month = t.getMonth();
                day = t.getDay();
            }
            return ((((((((year<<4))^((month<<28)))^((day<<23)))^((h<<18)))^((mi<<13)))^second)^nanosecond);
        }
    
    }
}