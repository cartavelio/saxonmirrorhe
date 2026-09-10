using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public class GYearMonthValue : Saxon.Hej.value.GDateValue {
        private static readonly Saxon.Ejava.util.regex.Pattern regex = Saxon.Ejava.util.regex.Pattern.compile("(-?[0-9]+-[0-9][0-9])(Z|[+-][0-9][0-9]:[0-9][0-9])?");
        private GYearMonthValue()  {
        }
        public static Saxon.Hej.type.ConversionResult makeGYearMonthValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.lib.ConversionRules rules) {
            Saxon.Hej.str.UnicodeString trimmed = Saxon.Hej.value.Whitespace.trim(value);
            Saxon.Ejava.util.regex.Matcher m = regex.matcher(trimmed.ToString());
            if (!(m.matches())) {
                return new Saxon.Hej.type.ValidationFailure((("Cannot convert '"+value)+"' to a gYearMonth"));
            }
            Saxon.Hej.value.GYearMonthValue g = new Saxon.Hej.value.GYearMonthValue();
            string @base = m.group(1);
            string tz = m.group(2);
            string date = ((@base+"-01")+(((tz == null) ? "" : tz)));
            g.typeLabel = Saxon.Hej.type.BuiltInAtomicType.G_YEAR_MONTH;
            return setLexicalValue(g, Saxon.Hej.str.BMPString.of(date), rules.isAllowYearZero());
        }
        public GYearMonthValue(int year, byte month, int tz, bool xsd10) : this (year, month, tz, Saxon.Hej.type.BuiltInAtomicType.G_YEAR_MONTH) {
            this.hasNoYearZero = xsd10;
        }
        public GYearMonthValue(int year, byte month, int tz, Saxon.Hej.type.AtomicType type)  {
            this.year = year;
            this.month = month;
            day = 1;
            setTimezoneInMinutes(tz);
            typeLabel = type;
        }
        public override Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            Saxon.Hej.value.GYearMonthValue v = new Saxon.Hej.value.GYearMonthValue(year, month, getTimezoneInMinutes(), hasNoYearZero);
            v.typeLabel = typeLabel;
            return v;
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.G_YEAR_MONTH;
        }
        public override Saxon.Hej.str.UnicodeString getPrimitiveStringValue() {
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder();
            int yr = year;
            if ((year<=0)) {
                yr = (-(yr)+((hasNoYearZero ? 1 : 0)));
                if ((yr != 0)) {
                    sb.append('-');
                }
            }
            appendString(sb, yr, (((yr>9999) ? ((yr+"")).Length : 4)));
            sb.append('-');
            appendTwoDigits(sb, month);
            if (hasTimezone()) {
                appendTimezone(sb);
            }
            return sb.toUnicodeString();
        }
        public override Saxon.Hej.value.CalendarValue add(Saxon.Hej.value.DurationValue duration) {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot add a duration to an xs:gYearMonth");
            err.setErrorCode("XPTY0004");
            throw err;
        }
        public override Saxon.Hej.value.CalendarValue adjustTimezone(int tz) {
            Saxon.Hej.value.DateTimeValue dt = toDateTime().adjustTimezone(tz);
            return new Saxon.Hej.value.GYearMonthValue(dt.getYear(), dt.getMonth(), dt.getTimezoneInMinutes(), hasNoYearZero);
        }
    
    }
}