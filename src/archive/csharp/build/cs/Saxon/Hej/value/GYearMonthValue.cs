using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public class GYearMonthValue : Saxon.Hej.value.GDateValue {
        private static readonly Saxon.Ejava.util.regex.Pattern regex = Saxon.Ejava.util.regex.Pattern.compile("(-?[0-9]+-[0-9][0-9])(Z|[+-][0-9][0-9]:[0-9][0-9])?");
        private GYearMonthValue(Saxon.Hej.value.GDateValue.MutableGDateValue m) : base (m) {
        }
        public static Saxon.Hej.type.ConversionResult makeGYearMonthValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.lib.ConversionRules rules) {
            Saxon.Hej.str.UnicodeString trimmed = Saxon.Hej.value.Whitespace.trim(value);
            Saxon.Ejava.util.regex.Matcher m = regex.matcher(trimmed.ToString());
            if (!(m.matches())) {
                return new Saxon.Hej.type.ValidationFailure((("Cannot convert '"+value)+"' to a gYearMonth"));
            }
            Saxon.Hej.value.GDateValue.MutableGDateValue g = new Saxon.Hej.value.GDateValue.MutableGDateValue();
            string @base = m.group(1);
            string tz = m.group(2);
            string date = ((@base+"-01")+(((tz == null) ? "" : tz)));
            g.typeLabel = Saxon.Hej.type.BuiltInAtomicType.G_YEAR_MONTH;
            setLexicalValue(g, Saxon.Hej.str.BMPString.of(date), rules.isAllowYearZero());
            return ((g.error == null) ? new Saxon.Hej.value.GYearMonthValue(g) : g.error);
        }
        public GYearMonthValue(int year, byte month, int tz, bool xsd10) : this (new Saxon.Hej.value.GDateValue.MutableGDateValue(year, month, 1, xsd10, tz, Saxon.Hej.type.BuiltInAtomicType.G_YEAR_MONTH)) {
        }
        public GYearMonthValue(int year, byte month, int tz, Saxon.Hej.type.AtomicType type) : this (new Saxon.Hej.value.GDateValue.MutableGDateValue(year, month, 1, false, tz, type)) {
        }
        public override Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            Saxon.Hej.value.GDateValue.MutableGDateValue m = makeMutableCopy();
            m.typeLabel = typeLabel;
            return new Saxon.Hej.value.GYearMonthValue(m);
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.G_YEAR_MONTH;
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