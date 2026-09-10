using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public abstract class GDateValue : Saxon.Hej.value.CalendarValue {
        protected internal int year;
        protected internal byte month;
        protected internal byte day;
        protected internal bool hasNoYearZero;
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
        protected internal static Saxon.Hej.type.ConversionResult setLexicalValue(Saxon.Hej.value.GDateValue d, Saxon.Hej.str.UnicodeString s, bool allowYearZero) {
            d.hasNoYearZero = !(allowYearZero);
            Saxon.Ejava.util.StringTokenizer tok = new Saxon.Ejava.util.StringTokenizer(Saxon.Hej.value.Whitespace.trim(s).ToString(), "-:+TZ", true);
            try {
                if (!(tok.hasMoreTokens())) {
                    return badDate("Too short", s);
                }
                string part = (string)(tok.nextToken());
                int era = +(1);
                if ("+".Equals(part)) {
                    return badDate("Date must not start with '+' sign", s);
                } else {
                    if ("-".Equals(part)) {
                        era = -1;
                        if (!(tok.hasMoreTokens())) {
                            return badDate("No year after '-'", s);
                        }
                        part = (string)(tok.nextToken());
                    }
                }
                if ((part.Length<4)) {
                    return badDate("Year is less than four digits", s);
                }
                if (((part.Length>4) && (part[0] == '0'))) {
                    return badDate("When year exceeds 4 digits, leading zeroes are not allowed", s);
                }
                int value = Saxon.Hej.value.DurationValue.simpleInteger(part);
                if ((value<0)) {
                    if ((value == -1)) {
                        return badDate("Non-numeric year component", s);
                    } else {
                        return badDate("Year is outside the range that Saxon can handle", s, "FODT0001");
                    }
                }
                d.year = (value*era);
                if (((d.year == 0) && !(allowYearZero))) {
                    return badDate("Year zero is not allowed", s);
                }
                if (((era<0) && !(allowYearZero))) {
                    d.year++;
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
                part = (string)(tok.nextToken());
                if ((part.Length != 2)) {
                    return badDate("Month must be two digits", s);
                }
                value = Saxon.Hej.value.DurationValue.simpleInteger(part);
                if ((value<0)) {
                    return badDate("Non-numeric month component", s);
                }
                d.month = (byte)(value);
                if (((d.month<1) || (d.month>12))) {
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
                d.day = (byte)(value);
                if (((d.day<1) || (d.day>31))) {
                    return badDate("Day is out of range", s);
                }
                int tzOffset;
                if (tok.hasMoreTokens()) {
                    string delim = (string)(tok.nextToken());
                    if ("T".Equals(delim)) {
                        return badDate("Value includes time", s);
                    } else {
                        if ("Z".Equals(delim)) {
                            tzOffset = 0;
                            if (tok.hasMoreTokens()) {
                                return badDate("Continues after 'Z'", s);
                            }
                            d.setTimezoneInMinutes(tzOffset);
                        } else {
                            if (!(((!("+".Equals(delim)) && !("-".Equals(delim)))))) {
                                if (!(tok.hasMoreTokens())) {
                                    return badDate("Missing timezone", s);
                                }
                                part = (string)(tok.nextToken());
                                value = Saxon.Hej.value.DurationValue.simpleInteger(part);
                                if ((value<0)) {
                                    return badDate("Non-numeric timezone hour component", s);
                                }
                                int tzhour = value;
                                if ((part.Length != 2)) {
                                    return badDate("Timezone hour must be two digits", s);
                                }
                                if ((tzhour>14)) {
                                    return badDate("Timezone hour is out of range", s);
                                }
                                if (!(tok.hasMoreTokens())) {
                                    return badDate("No minutes in timezone", s);
                                }
                                if (!(":".Equals(tok.nextToken()))) {
                                    return badDate("Wrong delimiter after timezone hour", s);
                                }
                                if (!(tok.hasMoreTokens())) {
                                    return badDate("No minutes in timezone", s);
                                }
                                part = (string)(tok.nextToken());
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
                                if (tok.hasMoreTokens()) {
                                    return badDate("Continues after timezone", s);
                                }
                                tzOffset = ((tzhour*60)+tzminute);
                                if ("-".Equals(delim)) {
                                    tzOffset = -(tzOffset);
                                }
                                d.setTimezoneInMinutes(tzOffset);
                            } else {
                                return badDate("Timezone format is incorrect", s);
                            }
                        }
                    }
                }
                if (!(isValidDate(d.year, d.month, d.day))) {
                    return badDate("Non-existent date", s);
                }
            } catch (Saxon.Ejava.lang.NumberFormatException err) {
                return badDate("Non-numeric component", s);
            }
            return d;
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
                throw new Saxon.Hej.trans.XPathException("Year out of range for Saxon-JS", "FODT0001");
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