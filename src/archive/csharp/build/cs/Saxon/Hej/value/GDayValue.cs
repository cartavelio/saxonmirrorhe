using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public class GDayValue : Saxon.Hej.value.GDateValue {
        private static readonly Saxon.Ejava.util.regex.Pattern regex = Saxon.Ejava.util.regex.Pattern.compile("---([0-9][0-9])(Z|[+-][0-9][0-9]:[0-9][0-9])?");
        protected internal GDayValue(Saxon.Hej.value.GDateValue.MutableGDateValue m) : base (m) {
        }
        public static Saxon.Hej.type.ConversionResult makeGDayValue(Saxon.Hej.str.UnicodeString value) {
            Saxon.Hej.str.UnicodeString trimmed = Saxon.Hej.value.Whitespace.trim(value);
            Saxon.Ejava.util.regex.Matcher m = regex.matcher(trimmed.ToString());
            if (!(m.matches())) {
                return new Saxon.Hej.type.ValidationFailure((("Cannot convert '"+value)+"' to a gDay"));
            }
            Saxon.Hej.value.GDateValue.MutableGDateValue g = new Saxon.Hej.value.GDateValue.MutableGDateValue();
            string @base = m.group(1);
            string tz = m.group(2);
            string date = (("2000-01-"+@base)+(((tz == null) ? "" : tz)));
            g.typeLabel = Saxon.Hej.type.BuiltInAtomicType.G_DAY;
            setLexicalValue(g, Saxon.Hej.str.BMPString.of(date), true);
            return ((g.error == null) ? new Saxon.Hej.value.GDayValue(g) : g.error);
        }
        public GDayValue(byte day, int tz) : this (day, tz, Saxon.Hej.type.BuiltInAtomicType.G_DAY) {
        }
        public GDayValue(byte day, int tz, Saxon.Hej.type.AtomicType type) : this (new Saxon.Hej.value.GDateValue.MutableGDateValue(2000, 1, day, false, tz, type)) {
        }
        public override Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            Saxon.Hej.value.GDateValue.MutableGDateValue m = makeMutableCopy();
            m.typeLabel = typeLabel;
            return new Saxon.Hej.value.GDayValue(m);
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.G_DAY;
        }
        public override Saxon.Hej.str.UnicodeString getPrimitiveStringValue() {
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder(16);
            sb.appendLatin("---");
            appendTwoDigits(sb, day);
            if (hasTimezone()) {
                appendTimezone(sb);
            }
            return sb.toUnicodeString();
        }
        public override Saxon.Hej.value.CalendarValue add(Saxon.Hej.value.DurationValue duration) {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot add a duration to an xs:gDay");
            err.setErrorCode("XPTY0004");
            throw err;
        }
        public override Saxon.Hej.value.CalendarValue adjustTimezone(int tz) {
            Saxon.Hej.value.DateTimeValue dt = (Saxon.Hej.value.DateTimeValue)(toDateTime().adjustTimezone(tz));
            return new Saxon.Hej.value.GDayValue(dt.getDay(), dt.getTimezoneInMinutes());
        }
    
    }
}