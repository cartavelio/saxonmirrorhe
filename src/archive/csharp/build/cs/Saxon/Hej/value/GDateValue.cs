using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public abstract class GDateValue : Saxon.Hej.value.CalendarValue {
        protected internal readonly int year;
        protected internal readonly byte month;
        protected internal readonly byte day;
        protected internal readonly bool hasNoYearZero;
        public GDateValue(int year, byte month, byte day, bool hasNoYearZero, int tzMinutes, Saxon.Hej.type.AtomicType typeLabel) : base (typeLabel, tzMinutes) {
            this.year = year;
            this.month = month;
            this.day = day;
            this.hasNoYearZero = hasNoYearZero;
        }
        protected internal class MutableGDateValue {
            public int year;
            public byte month;
            public byte day;
            public bool hasNoYearZero;
            public int tzMinutes = NO_TIMEZONE;
            public Saxon.Hej.type.AtomicType typeLabel = Saxon.Hej.type.BuiltInAtomicType.DATE_TIME;
            public Saxon.Hej.type.ValidationFailure error = null;
            public MutableGDateValue()  {
            }
            public MutableGDateValue(int year, int month, int day, bool hasNoYearZero, int tzMinutes, Saxon.Hej.type.AtomicType typeLabel)  {
                this.year = year;
                this.month = (byte)(month);
                this.day = (byte)(day);
                this.hasNoYearZero = hasNoYearZero;
                this.tzMinutes = tzMinutes;
                this.typeLabel = typeLabel;
            }
        
        }
        protected internal Saxon.Hej.value.GDateValue.MutableGDateValue makeMutableCopy() {
            Saxon.Hej.value.GDateValue.MutableGDateValue m = new Saxon.Hej.value.GDateValue.MutableGDateValue();
            m.year = year;
            m.month = month;
            m.day = day;
            m.hasNoYearZero = hasNoYearZero;
            m.tzMinutes = getTimezoneInMinutes();
            m.typeLabel = typeLabel;
            return m;
        }
        protected internal GDateValue(Saxon.Hej.value.GDateValue.MutableGDateValue m) : base (m.typeLabel, m.tzMinutes) {
            this.year = m.year;
            this.month = m.month;
            this.day = m.day;
            this.hasNoYearZero = m.hasNoYearZero;
        }
        protected internal static byte[] daysPerMonth = {31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
        protected internal static readonly short[] monthData = {306, 337, 0, 31, 61, 92, 122, 153, 184, 214, 245, 275};
        public int getYear() {
            return year;
        }
        public byte getMonth() {
            return month;
        }
        public byte getDay() {
            return day;
        }
        protected internal static void setLexicalValue(Saxon.Hej.value.GDateValue.MutableGDateValue m, Saxon.Hej.str.UnicodeString s, bool allowYearZero) {
            m.hasNoYearZero = !(allowYearZero);
            Saxon.Ejava.util.StringTokenizer tok = new Saxon.Ejava.util.StringTokenizer(Saxon.Hej.value.Whitespace.trim(s).ToString(), "-:+TZ", true);
            try {
                if (!(tok.hasMoreTokens())) {
                    m.error = badDate("Too short", s);
                    return ;
                }
                string part = tok.nextToken();
                int era = +(1);
                if ("+".Equals(part)) {
                    m.error = badDate("Date must not start with '+' sign", s);
                    return ;
                } else {
                    if ("-".Equals(part)) {
                        era = -1;
                        if (!(tok.hasMoreTokens())) {
                            m.error = badDate("No year after '-'", s);
                            return ;
                        }
                        part = (string)(tok.nextToken());
                    }
                }
                if ((part.Length<4)) {
                    m.error = badDate("Year is less than four digits", s);
                    return ;
                }
                if (((part.Length>4) && (part[0] == '0'))) {
                    m.error = badDate("When year exceeds 4 digits, leading zeroes are not allowed", s);
                    return ;
                }
                int value = Saxon.Hej.value.DurationValue.simpleInteger(part);
                if ((value<0)) {
                    if ((value == -1)) {
                        m.error = badDate("Non-numeric year component", s);
                    } else {
                        m.error = badDate("Year is outside the range that Saxon can handle", s, "FODT0001");
                    }
                    return ;
                }
                m.year = (value*era);
                if (((m.year == 0) && !(allowYearZero))) {
                    m.error = badDate("Year zero is not allowed", s);
                    return ;
                }
                if (((era<0) && !(allowYearZero))) {
                    m.year++;
                }
                if (!(tok.hasMoreTokens())) {
                    m.error = badDate("Too short", s);
                    return ;
                }
                if (!("-".Equals(tok.nextToken()))) {
                    m.error = badDate("Wrong delimiter after year", s);
                    return ;
                }
                if (!(tok.hasMoreTokens())) {
                    m.error = badDate("Too short", s);
                    return ;
                }
                part = tok.nextToken();
                if ((part.Length != 2)) {
                    m.error = badDate("Month must be two digits", s);
                    return ;
                }
                value = Saxon.Hej.value.DurationValue.simpleInteger(part);
                if ((value<0)) {
                    m.error = badDate("Non-numeric month component", s);
                    return ;
                }
                m.month = (byte)(value);
                if (((m.month<1) || (m.month>12))) {
                    m.error = badDate("Month is out of range", s);
                    return ;
                }
                if (!(tok.hasMoreTokens())) {
                    m.error = badDate("Too short", s);
                    return ;
                }
                if (!("-".Equals(tok.nextToken()))) {
                    m.error = badDate("Wrong delimiter after month", s);
                    return ;
                }
                if (!(tok.hasMoreTokens())) {
                    m.error = badDate("Too short", s);
                    return ;
                }
                part = (string)(tok.nextToken());
                if ((part.Length != 2)) {
                    m.error = badDate("Day must be two digits", s);
                    return ;
                }
                value = Saxon.Hej.value.DurationValue.simpleInteger(part);
                if ((value<0)) {
                    m.error = badDate("Non-numeric day component", s);
                    return ;
                }
                m.day = (byte)(value);
                if (((m.day<1) || (m.day>31))) {
                    m.error = badDate("Day is out of range", s);
                    return ;
                }
                int tzOffset;
                if (tok.hasMoreTokens()) {
                    string delim = tok.nextToken();
                    if ("T".Equals(delim)) {
                        m.error = badDate("Value includes time", s);
                        return ;
                    } else {
                        if ("Z".Equals(delim)) {
                            tzOffset = 0;
                            if (tok.hasMoreTokens()) {
                                m.error = badDate("Continues after 'Z'", s);
                                return ;
                            }
                            m.tzMinutes = tzOffset;
                        } else {
                            if (!(((!("+".Equals(delim)) && !("-".Equals(delim)))))) {
                                if (!(tok.hasMoreTokens())) {
                                    m.error = badDate("Missing timezone", s);
                                    return ;
                                }
                                part = (string)(tok.nextToken());
                                value = Saxon.Hej.value.DurationValue.simpleInteger(part);
                                if ((value<0)) {
                                    m.error = badDate("Non-numeric timezone hour component", s);
                                    return ;
                                }
                                int tzhour = value;
                                if ((part.Length != 2)) {
                                    m.error = badDate("Timezone hour must be two digits", s);
                                    return ;
                                }
                                if ((tzhour>14)) {
                                    m.error = badDate("Timezone hour is out of range", s);
                                    return ;
                                }
                                if (!(tok.hasMoreTokens())) {
                                    m.error = badDate("No minutes in timezone", s);
                                    return ;
                                }
                                if (!(":".Equals(tok.nextToken()))) {
                                    m.error = badDate("Wrong delimiter after timezone hour", s);
                                    return ;
                                }
                                if (!(tok.hasMoreTokens())) {
                                    m.error = badDate("No minutes in timezone", s);
                                    return ;
                                }
                                part = (string)(tok.nextToken());
                                value = Saxon.Hej.value.DurationValue.simpleInteger(part);
                                if ((value<0)) {
                                    m.error = badDate("Non-numeric timezone minute component", s);
                                    return ;
                                }
                                int tzminute = value;
                                if ((part.Length != 2)) {
                                    m.error = badDate("Timezone minute must be two digits", s);
                                    return ;
                                }
                                if ((tzminute>59)) {
                                    m.error = badDate("Timezone minute is out of range", s);
                                    return ;
                                }
                                if (tok.hasMoreTokens()) {
                                    m.error = badDate("Continues after timezone", s);
                                    return ;
                                }
                                tzOffset = ((tzhour*60)+tzminute);
                                if ("-".Equals(delim)) {
                                    tzOffset = -(tzOffset);
                                }
                                m.tzMinutes = tzOffset;
                            } else {
                                m.error = badDate("Timezone format is incorrect", s);
                                return ;
                            }
                        }
                    }
                }
                if (!(isValidDate(m.year, m.month, m.day))) {
                    m.error = badDate("Non-existent date", s);
                }
            } catch (Saxon.Ejava.lang.NumberFormatException err) {
                m.error = badDate("Non-numeric component", s);
            }
        }
        private static Saxon.Hej.type.ValidationFailure badDate(string msg, Saxon.Hej.str.UnicodeString value) {
            Saxon.Hej.type.ValidationFailure err = new Saxon.Hej.type.ValidationFailure((((("Invalid date "+Saxon.Hej.trans.Err.wrap(value, Saxon.Hej.trans.Err.VALUE))+" (")+msg)+")"));
            err.setErrorCode("FORG0001");
            return err;
        }
        private static Saxon.Hej.type.ValidationFailure badDate(string msg, Saxon.Hej.str.UnicodeString value, string errorCode) {
            Saxon.Hej.type.ValidationFailure err = new Saxon.Hej.type.ValidationFailure((((("Invalid date "+Saxon.Hej.trans.Err.wrap(value, Saxon.Hej.trans.Err.VALUE))+" (")+msg)+")"));
            err.setErrorCode(errorCode);
            return err;
        }
        public static bool isValidDate(int year, int month, int day) {
            return (((((month>0) && (month<=12)) && (day>0)) && (day<=daysPerMonth[(month-1)])) || (((month == 2) && (day == 29)) && isLeapYear(year)));
        }
        public static bool isLeapYear(int year) {
            return ((((year%4) == 0)) && !(((((year%100) == 0) && !((((year%400) == 0)))))));
        }
        public override void checkValidInJavascript() {
            if (((year<=0) || (year>9999))) {
                throw new Saxon.Hej.trans.XPathException("Year out of range for SaxonJS", "FODT0001");
            }
        }
        public override bool Equals(System.Object o) {
            if ((o is Saxon.Hej.value.GDateValue)) {
                Saxon.Hej.value.GDateValue gdv = (Saxon.Hej.value.GDateValue)(o);
                return ((getPrimitiveType() == gdv.getPrimitiveType()) && toDateTime().Equals(gdv.toDateTime()));
            } else {
                return false;
            }
        }
        public override int GetHashCode() {
            return Saxon.Hej.value.DateTimeValue.computeHashCode(year, month, day, (byte)(12), (byte)(0), (byte)(0), 0, getTimezoneInMinutes());
        }
        public override int compareTo(Saxon.Hej.value.CalendarValue other, int implicitTimezone) {
            if ((getPrimitiveType() != other.getPrimitiveType())) {
                throw new System.InvalidCastException("Cannot compare dates of different types");
            }
            Saxon.Hej.value.GDateValue v2 = (Saxon.Hej.value.GDateValue)(other);
            if ((getTimezoneInMinutes() == other.getTimezoneInMinutes())) {
                if ((year != v2.year)) {
                    return Saxon.Hej.value.IntegerValue.signum((year-v2.year));
                }
                if ((month != v2.month)) {
                    return Saxon.Hej.value.IntegerValue.signum((month-v2.month));
                }
                if ((day != v2.day)) {
                    return Saxon.Hej.value.IntegerValue.signum((day-v2.day));
                }
                return 0;
            }
            return toDateTime().compareTo(other.toDateTime(), implicitTimezone);
        }
        public override Saxon.Hej.value.DateTimeValue toDateTime() {
            return new Saxon.Hej.value.DateTimeValue(year, month, day, (byte)(0), (byte)(0), (byte)(0), 0, getTimezoneInMinutes(), hasNoYearZero);
        }
        public Saxon.Hej.value.GDateValue.GDateComparable getSchemaComparable() {
            return new Saxon.Hej.value.GDateValue.GDateComparable(this);
        }
        public override Saxon.Hej.expr.sort.XPathComparable getXPathComparable(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return null;
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
              case Saxon.Hej.functions.AccessorFn.Component.TIMEZONE:
                if (hasTimezone()) {
                    return Saxon.Hej.value.DayTimeDurationValue.fromMilliseconds((60000L*getTimezoneInMinutes()));
                } else {
                    return null;
                }
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown component for date: "+component));
            }
        }
        public class GDateComparable : Saxon.Ejava.lang.Comparable<Saxon.Hej.value.GDateValue.GDateComparable> {
            private readonly Saxon.Hej.value.GDateValue value;
            public GDateComparable(Saxon.Hej.value.GDateValue value)  {
                this.value = value;
            }
            public Saxon.Hej.value.GDateValue asGDateValue() {
                return value;
            }
            public int compareTo(Saxon.Hej.value.GDateValue.GDateComparable o) {
                if ((asGDateValue().getPrimitiveType() != o.asGDateValue().getPrimitiveType())) {
                    return Saxon.Hej.om.SequenceTool.INDETERMINATE_ORDERING;
                }
                Saxon.Hej.value.DateTimeValue dt0 = value.toDateTime();
                Saxon.Hej.value.DateTimeValue dt1 = o.value.toDateTime();
                return dt0.getSchemaComparable().compareTo(dt1.getSchemaComparable());
            }
            public override bool Equals(System.Object o) {
                return ((o is Saxon.Hej.value.GDateValue.GDateComparable) && (compareTo((Saxon.Hej.value.GDateValue.GDateComparable)(o)) == 0));
            }
            public override int GetHashCode() {
                return value.toDateTime().getSchemaComparable().GetHashCode();
            }
        
        }
    
    }
}