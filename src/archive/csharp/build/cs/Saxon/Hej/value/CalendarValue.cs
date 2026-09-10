using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public abstract class CalendarValue : Saxon.Hej.value.AtomicValue, Saxon.Hej.expr.sort.AtomicMatchKey {
        private readonly int tzMinutes;
        public static readonly int NO_TIMEZONE = System.Int32.MinValue;
        public static readonly int MISSING_TIMEZONE = System.Int32.MaxValue;
        public static Saxon.Hej.type.ConversionResult makeCalendarValue(Saxon.Hej.str.UnicodeString s, Saxon.Hej.lib.ConversionRules rules) {
            Saxon.Hej.type.ConversionResult cr = Saxon.Hej.value.DateTimeValue.makeDateTimeValue(s, rules);
            Saxon.Hej.type.ConversionResult firstError = cr;
            if ((cr is Saxon.Hej.type.ValidationFailure)) {
                cr = Saxon.Hej.value.DateValue.makeDateValue(s, rules);
            }
            if ((cr is Saxon.Hej.type.ValidationFailure)) {
                cr = Saxon.Hej.value.TimeValue.makeTimeValue(s);
            }
            if ((cr is Saxon.Hej.type.ValidationFailure)) {
                cr = Saxon.Hej.value.GYearValue.makeGYearValue(s, rules);
            }
            if ((cr is Saxon.Hej.type.ValidationFailure)) {
                cr = Saxon.Hej.value.GYearMonthValue.makeGYearMonthValue(s, rules);
            }
            if ((cr is Saxon.Hej.type.ValidationFailure)) {
                cr = Saxon.Hej.value.GMonthValue.makeGMonthValue(s);
            }
            if ((cr is Saxon.Hej.type.ValidationFailure)) {
                cr = Saxon.Hej.value.GMonthDayValue.makeGMonthDayValue(s);
            }
            if ((cr is Saxon.Hej.type.ValidationFailure)) {
                cr = Saxon.Hej.value.GDayValue.makeGDayValue(s);
            }
            if ((cr is Saxon.Hej.type.ValidationFailure)) {
                return firstError;
            }
            return cr;
        }
        public CalendarValue(Saxon.Hej.type.AtomicType typeLabel) : base (typeLabel) {
            this.tzMinutes = NO_TIMEZONE;
        }
        public CalendarValue(Saxon.Hej.type.AtomicType typeLabel, int tzMinutes) : base (typeLabel) {
            this.tzMinutes = tzMinutes;
        }
        public bool hasTimezone() {
            return (tzMinutes != NO_TIMEZONE);
        }
        public abstract Saxon.Hej.value.DateTimeValue toDateTime();
        public int getTimezoneInMinutes() {
            return tzMinutes;
        }
        public abstract Saxon.Hej.value.CalendarValue add(Saxon.Hej.value.DurationValue duration);
        public virtual Saxon.Hej.value.DayTimeDurationValue subtract(Saxon.Hej.value.CalendarValue other, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.value.DateTimeValue dt1 = toDateTime();
            Saxon.Hej.value.DateTimeValue dt2 = other.toDateTime();
            if ((dt1.getTimezoneInMinutes() != dt2.getTimezoneInMinutes())) {
                int tz = Saxon.Hej.value.CalendarValue.NO_TIMEZONE;
                if (((context == null) || ((tz = context.getImplicitTimezone()) == Saxon.Hej.value.CalendarValue.MISSING_TIMEZONE))) {
                    throw new Saxon.Hej.trans.NoDynamicContextException("Implicit timezone required");
                }
                dt1 = dt1.adjustToUTC(tz);
                dt2 = dt2.adjustToUTC(tz);
            }
            Singulink.Numerics.BigDecimal d1 = dt1.toJulianInstant();
            Singulink.Numerics.BigDecimal d2 = dt2.toJulianInstant();
            Singulink.Numerics.BigDecimal difference = (d1-d2);
            return Saxon.Hej.value.DayTimeDurationValue.fromSeconds(difference);
        }
        public Saxon.Hej.value.CalendarValue removeTimezone() {
            return adjustTimezone(NO_TIMEZONE);
        }
        public abstract Saxon.Hej.value.CalendarValue adjustTimezone(int tz);
        public Saxon.Hej.value.CalendarValue adjustTimezone(Saxon.Hej.value.DayTimeDurationValue tz) {
            long microseconds = tz.getLengthInMicroseconds();
            if (((microseconds%60000000) != 0)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Timezone is not an integral number of minutes");
                err.setErrorCode("FODT0003");
                throw err;
            }
            int tzminutes = (int)(((microseconds/60000000)));
            if ((System.Math.Abs(tzminutes)>(14*60))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Timezone out of range (-14:00 to +14:00)");
                err.setErrorCode("FODT0003");
                throw err;
            }
            return adjustTimezone(tzminutes);
        }
        public override Saxon.Hej.expr.sort.AtomicMatchKey getXPathMatchKey(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            if (hasTimezone()) {
                return this;
            }
            if ((implicitTimezone == MISSING_TIMEZONE)) {
                throw new Saxon.Hej.trans.NoDynamicContextException("Unknown implicit timezone");
            }
            return (hasTimezone() ? this : adjustTimezone(implicitTimezone));
        }
        public override Saxon.Hej.expr.sort.AtomicMatchKey asMapKey() {
            return new Saxon.Hej.value.CalendarValue.CalendarValueMapKey(this);
        }
        public override Saxon.Hej.expr.sort.XPathComparable getXPathComparable(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return null;
        }
        public abstract int compareTo(Saxon.Hej.value.CalendarValue other, int implicitTimezone);
        public override bool isIdentical(Saxon.Hej.value.AtomicValue v) {
            return (base.isIdentical(v) && (tzMinutes == ((Saxon.Hej.value.CalendarValue)(v)).tzMinutes));
        }
        public override int identityHashCode() {
            return (GetHashCode()^tzMinutes);
        }
        public void appendTimezone(Saxon.Hej.str.UnicodeBuilder sb) {
            if (hasTimezone()) {
                appendTimezone(getTimezoneInMinutes(), sb);
            }
        }
        public static void appendTimezone(int tz, Saxon.Hej.str.UnicodeBuilder sb) {
            if ((tz == 0)) {
                sb.append('Z');
            } else {
                sb.append(((tz>0) ? "+" : "-"));
                tz = System.Math.Abs(tz);
                appendTwoDigits(sb, (tz/60));
                sb.append(':');
                appendTwoDigits(sb, (tz%60));
            }
        }
        protected internal static void appendString(Saxon.Hej.str.UnicodeBuilder sb, int value, int size) {
            string s = ("000000000"+value);
            sb.append(s.Substring((s.Length-size)));
        }
        protected internal static void appendTwoDigits(Saxon.Hej.str.UnicodeBuilder sb, int value) {
            sb.append((char)((((value/10)+'0'))));
            sb.append((char)((((value%10)+'0'))));
        }
        private class CalendarValueMapKey : Saxon.Hej.expr.sort.AtomicMatchKey {
            private readonly Saxon.Hej.value.CalendarValue value;
            public CalendarValueMapKey(Saxon.Hej.value.CalendarValue value)  {
                this.value = value;
            }
            public Saxon.Hej.value.AtomicValue asAtomic() {
                return value;
            }
            public override bool Equals(System.Object obj) {
                if ((obj is Saxon.Hej.value.CalendarValue.CalendarValueMapKey)) {
                    Saxon.Hej.value.CalendarValue a = value;
                    Saxon.Hej.value.CalendarValue b = ((Saxon.Hej.value.CalendarValue.CalendarValueMapKey)(obj)).value;
                    if ((a.hasTimezone() == b.hasTimezone())) {
                        if (a.hasTimezone()) {
                            return a.adjustTimezone(b.tzMinutes).isIdentical(b);
                        } else {
                            return a.isIdentical(b);
                        }
                    } else {
                        return false;
                    }
                } else {
                    return false;
                }
            }
            public override int GetHashCode() {
                return asAtomic().GetHashCode();
            }
        
        }
    
    }
}