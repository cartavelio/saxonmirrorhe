using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public class DateValue : Saxon.Hej.value.GDateValue, Saxon.Hej.expr.sort.XPathComparable {
        private DateValue(Saxon.Hej.value.GDateValue.MutableGDateValue m) : base (m) {
        }
        public DateValue(int year, byte month, byte day) : this (new Saxon.Hej.value.GDateValue.MutableGDateValue(year, month, day, true, NO_TIMEZONE, Saxon.Hej.type.BuiltInAtomicType.DATE)) {
        }
        public DateValue(int year, byte month, byte day, bool xsd10) : this (new Saxon.Hej.value.GDateValue.MutableGDateValue(year, month, day, xsd10, NO_TIMEZONE, Saxon.Hej.type.BuiltInAtomicType.DATE)) {
        }
        public DateValue(int year, byte month, byte day, int tz, bool xsd10) : this (new Saxon.Hej.value.GDateValue.MutableGDateValue(year, month, day, xsd10, tz, Saxon.Hej.type.BuiltInAtomicType.DATE)) {
        }
        public DateValue(int year, byte month, byte day, int tz, Saxon.Hej.type.AtomicType type) : this (new Saxon.Hej.value.GDateValue.MutableGDateValue(year, month, day, false, tz, type)) {
        }
        public DateValue(Saxon.Hej.str.UnicodeString s) : this (s, Saxon.Hej.lib.ConversionRules.DEFAULT) {
        }
        public DateValue(Saxon.Hej.str.UnicodeString s, Saxon.Hej.lib.ConversionRules rules) : this (fromUnicodeString(s, rules)) {
        }
        private static Saxon.Hej.value.GDateValue.MutableGDateValue fromUnicodeString(Saxon.Hej.str.UnicodeString s, Saxon.Hej.lib.ConversionRules rules) {
            Saxon.Hej.value.GDateValue.MutableGDateValue m = new Saxon.Hej.value.GDateValue.MutableGDateValue();
            setLexicalValue(m, s, rules.isAllowYearZero());
            if ((m.error == null)) {
                return m;
            } else {
                throw m.error.makeException();
            }
        }
        public static Saxon.Hej.type.ConversionResult makeDateValue(Saxon.Hej.str.UnicodeString @in, Saxon.Hej.lib.ConversionRules rules) {
            Saxon.Hej.value.GDateValue.MutableGDateValue g = new Saxon.Hej.value.GDateValue.MutableGDateValue();
            g.typeLabel = Saxon.Hej.type.BuiltInAtomicType.DATE;
            setLexicalValue(g, @in, rules.isAllowYearZero());
            return ((g.error == null) ? new Saxon.Hej.value.DateValue(g) : g.error);
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.DATE;
        }
        public static Saxon.Hej.value.DateValue tomorrow(int year, byte month, byte day) {
            if (Saxon.Hej.value.DateValue.isValidDate(year, month, (day+1))) {
                return new Saxon.Hej.value.DateValue(year, month, (byte)(((day+1))), true);
            } else {
                if ((month<12)) {
                    return new Saxon.Hej.value.DateValue(year, (byte)(((month+1))), (byte)(1), true);
                } else {
                    return new Saxon.Hej.value.DateValue((year+1), (byte)(1), (byte)(1), true);
                }
            }
        }
        public static Saxon.Hej.value.DateValue yesterday(int year, byte month, byte day) {
            if ((day>1)) {
                return new Saxon.Hej.value.DateValue(year, month, (byte)(((day-1))), true);
            } else {
                if ((month>1)) {
                    if (((month == 3) && isLeapYear(year))) {
                        return new Saxon.Hej.value.DateValue(year, (byte)(2), (byte)(29), true);
                    } else {
                        return new Saxon.Hej.value.DateValue(year, (byte)(((month-1))), daysPerMonth[(month-2)], true);
                    }
                } else {
                    return new Saxon.Hej.value.DateValue((year-1), (byte)(12), (byte)(31), true);
                }
            }
        }
        public override Saxon.Hej.str.UnicodeString getPrimitiveStringValue() {
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder(16);
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
            if (hasTimezone()) {
                appendTimezone(sb);
            }
            return sb.toUnicodeString();
        }
        public override Saxon.Hej.str.UnicodeString getCanonicalLexicalRepresentation() {
            Saxon.Hej.value.DateValue target = this;
            if (hasTimezone()) {
                if ((getTimezoneInMinutes()>(12*60))) {
                    target = adjustTimezone((getTimezoneInMinutes()-(24*60)));
                } else {
                    if ((getTimezoneInMinutes()<=(-12*60))) {
                        target = adjustTimezone((getTimezoneInMinutes()+(24*60)));
                    }
                }
            }
            return target.getUnicodeStringValue();
        }
        public override Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            Saxon.Hej.value.GDateValue.MutableGDateValue m = makeMutableCopy();
            m.typeLabel = typeLabel;
            return new Saxon.Hej.value.DateValue(m);
        }
        public override Saxon.Hej.value.DateValue adjustTimezone(int timezone) {
            Saxon.Hej.value.DateTimeValue dt = toDateTime().adjustTimezone(timezone);
            return new Saxon.Hej.value.DateValue(dt.getYear(), dt.getMonth(), dt.getDay(), dt.getTimezoneInMinutes(), hasNoYearZero);
        }
        public override Saxon.Hej.value.DateValue add(Saxon.Hej.value.DurationValue duration) {
            if ((duration is Saxon.Hej.value.DayTimeDurationValue)) {
                long microseconds = ((Saxon.Hej.value.DayTimeDurationValue)(duration)).getLengthInMicroseconds();
                bool negative = (microseconds<0);
                microseconds = System.Math.Abs(microseconds);
                int days = (int)(System.Math.Floor(((double)(microseconds)/((((1000000L*60L)*60L)*24L)))));
                bool partDay = (((microseconds%((((1000000L*60L)*60L)*24L))))>0);
                int julian = getJulianDayNumber();
                Saxon.Hej.value.GDateValue.MutableGDateValue d = mutableDateFromJulianDayNumber((julian+((negative ? -(days) : days))));
                if (partDay) {
                    if (negative) {
                        d = yesterday(d.year, d.month, d.day).makeMutableCopy();
                    }
                }
                d.tzMinutes = getTimezoneInMinutes();
                d.hasNoYearZero = this.hasNoYearZero;
                return new Saxon.Hej.value.DateValue(d);
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
                    while (!(isValidDate(y, m, d))) {
                        d -= 1;
                    }
                    return new Saxon.Hej.value.DateValue(y, (byte)(m), (byte)(d), getTimezoneInMinutes(), hasNoYearZero);
                } else {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Date arithmetic is not available for xs:duration, only for its subtypes");
                    err.setIsTypeError(true);
                    err.setErrorCode("XPTY0004");
                    throw err;
                }
            }
        }
        public override Saxon.Hej.value.DayTimeDurationValue subtract(Saxon.Hej.value.CalendarValue other, Saxon.Hej.expr.XPathContext context) {
            if (!(((other is Saxon.Hej.value.DateValue)))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("First operand of '-' is a date, but the second is not");
                err.setIsTypeError(true);
                err.setErrorCode("XPTY0004");
                throw err;
            }
            return base.subtract(other, context);
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
            if ((v2 is Saxon.Hej.value.DateValue)) {
                try {
                    return compareTo((Saxon.Hej.value.DateValue)(v2), MISSING_TIMEZONE);
                } catch (System.Exception err) {
                    throw new System.InvalidCastException("Date comparison requires access to implicit timezone");
                }
            } else {
                throw new System.InvalidCastException(("Cannot compare xs:date to "+v2.ToString()));
            }
        }
        public static int getJulianDayNumber(int year, int month, int day) {
            int z = (year-(((month<3) ? 1 : 0)));
            short f = monthData[(month-1)];
            if ((z>=0)) {
                return ((((((day+f)+(365*z))+(z/4))-(z/100))+(z/400))+1721118);
            } else {
                z += 12000;
                int j = ((((((day+f)+(365*z))+(z/4))-(z/100))+(z/400))+1721118);
                return (j-(((((365*12000)+(12000/4))-(12000/100))+(12000/400))));
            }
        }
        public int getJulianDayNumber() {
            return getJulianDayNumber(year, month, day);
        }
        public static Saxon.Hej.value.DateValue dateFromJulianDayNumber(int julianDayNumber) {
            return new Saxon.Hej.value.DateValue(mutableDateFromJulianDayNumber(julianDayNumber));
        }
        private static Saxon.Hej.value.GDateValue.MutableGDateValue mutableDateFromJulianDayNumber(int julianDayNumber) {
            if ((julianDayNumber>=0)) {
                int L = ((julianDayNumber+68569)+1);
                int n = (((4*L))/146097);
                L = (L-((((146097*n)+3))/4));
                int i = (((4000*((L+1))))/1461001);
                L = ((L-(((1461*i))/4))+31);
                int j = (((80*L))/2447);
                int d = (L-(((2447*j))/80));
                L = (j/11);
                int m = ((j+2)-((12*L)));
                int y = (((100*((n-49)))+i)+L);
                return new Saxon.Hej.value.GDateValue.MutableGDateValue(y, m, d, true, NO_TIMEZONE, Saxon.Hej.type.BuiltInAtomicType.DATE);
            } else {
                Saxon.Hej.value.GDateValue.MutableGDateValue dt = mutableDateFromJulianDayNumber(((((julianDayNumber+(365*12000))+(12000/4))-(12000/100))+(12000/400)));
                dt.year -= 12000;
                return dt;
            }
        }
        public static int getDayWithinYear(int year, int month, int day) {
            int j = getJulianDayNumber(year, month, day);
            int k = getJulianDayNumber(year, 1, 1);
            return ((j-k)+1);
        }
        public static int getDayOfWeek(int year, int month, int day) {
            int d = getJulianDayNumber(year, month, day);
            d -= 2378500;
            while ((d<=0)) {
                d += 70000000;
            }
            return ((((d-1))%7)+1);
        }
        public static int getWeekNumber(int year, int month, int day) {
            return System.Globalization.ISOWeek.GetWeekOfYear(new System.DateTime(year, month, day));
        }
        public static int getWeekNumberWithinMonth(int year, int month, int day) {
            int firstDay = getDayOfWeek(year, month, 1);
            if (((firstDay>4) && (((firstDay+day))<=8))) {
                Saxon.Hej.value.DateValue lastDayPrevMonth = yesterday(year, (byte)(month), (byte)(1));
                return getWeekNumberWithinMonth(lastDayPrevMonth.year, lastDayPrevMonth.month, lastDayPrevMonth.day);
            }
            int inc = ((firstDay<5) ? 1 : 0);
            return ((((((day+firstDay)-2))/7))+inc);
        }
    
    }
}