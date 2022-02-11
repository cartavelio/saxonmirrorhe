using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public class GMonthDayValue : Saxon.Hej.value.GDateValue {
        private static readonly Saxon.Ejava.util.regex.Pattern regex = Saxon.Ejava.util.regex.Pattern.compile("--([0-9][0-9]-[0-9][0-9])(Z|[+-][0-9][0-9]:[0-9][0-9])?");
        private GMonthDayValue()  {
        }
        public static Saxon.Hej.type.ConversionResult makeGMonthDayValue(Saxon.Hej.str.UnicodeString value) {
            Saxon.Hej.str.UnicodeString trimmed = Saxon.Hej.value.Whitespace.trim(value);
            Saxon.Ejava.util.regex.Matcher m = regex.matcher(trimmed.ToString());
            if (!(m.matches())) {
                return new Saxon.Hej.type.ValidationFailure((("Cannot convert '"+value)+"' to a gMonthDay"));
            }
            Saxon.Hej.value.GMonthDayValue g = new Saxon.Hej.value.GMonthDayValue();
            string @base = m.group(1);
            string tz = m.group(2);
            string date = (("2000-"+@base)+(((tz == null) ? "" : tz)));
            g.typeLabel = Saxon.Hej.type.BuiltInAtomicType.G_MONTH_DAY;
            return setLexicalValue(g, Saxon.Hej.str.BMPString.of(date), true);
        }
        public GMonthDayValue(byte month, byte day, int tz) : this (month, day, tz, Saxon.Hej.type.BuiltInAtomicType.G_MONTH_DAY) {
        }
        public GMonthDayValue(byte month, byte day, int tz, Saxon.Hej.type.AtomicType type)  {
            this.year = 2000;
            this.month = month;
            this.day = day;
            setTimezoneInMinutes(tz);
            this.typeLabel = type;
        }
        public override Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            Saxon.Hej.value.GMonthDayValue v = new Saxon.Hej.value.GMonthDayValue(month, day, getTimezoneInMinutes());
            v.typeLabel = typeLabel;
            return v;
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.G_MONTH_DAY;
        }
        public override Saxon.Hej.str.UnicodeString getPrimitiveStringValue() {
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder();
            sb.appendLatin("--");
            appendTwoDigits(sb, month);
            sb.append('-');
            appendTwoDigits(sb, day);
            if (hasTimezone()) {
                appendTimezone(sb);
            }
            return sb.toUnicodeString();
        }
        public override Saxon.Hej.value.CalendarValue add(Saxon.Hej.value.DurationValue duration) {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot add a duration to an xs:gMonthDay");
            err.setErrorCode("XPTY0004");
            throw err;
        }
        public override Saxon.Hej.value.CalendarValue adjustTimezone(int tz) {
            Saxon.Hej.value.DateTimeValue dt = toDateTime().adjustTimezone(tz);
            return new Saxon.Hej.value.GMonthDayValue(dt.getMonth(), dt.getDay(), dt.getTimezoneInMinutes());
        }
    
    }
}