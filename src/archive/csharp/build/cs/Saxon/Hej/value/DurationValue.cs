using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public class DurationValue : Saxon.Hej.value.AtomicValue, Saxon.Hej.expr.sort.AtomicMatchKey {
        protected internal readonly bool _negative;
        protected internal readonly int _months;
        protected internal readonly long _seconds;
        protected internal readonly int _nanoseconds;
        public DurationValue(bool positive, int years, int months, int days, int hours, int minutes, long seconds, int microseconds) : this (positive, years, months, days, hours, minutes, seconds, microseconds, Saxon.Hej.type.BuiltInAtomicType.DURATION) {
        }
        public DurationValue(bool positive, int years, int months, int days, int hours, int minutes, long seconds, int microseconds, Saxon.Hej.type.AtomicType typeLabel) : base (typeLabel) {
            if ((((((((years<0) || (months<0)) || (days<0)) || (hours<0)) || (minutes<0)) || (seconds<0)) || (microseconds<0))) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Negative component value");
            }
            if (((((double)(years)*12)+(double)(months))>System.Int32.MaxValue)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Duration months limit exceeded");
            }
            if (((((((double)(days)*(((24*60)*60)))+((double)(hours)*((60*60))))+((double)(minutes)*60))+(double)(seconds))>System.Int64.MaxValue)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Duration seconds limit exceeded");
            }
            this._months = ((years*12)+months);
            long h = ((days*24L)+hours);
            long m = ((h*60L)+minutes);
            this._seconds = ((m*60L)+seconds);
            this._nanoseconds = (microseconds*1000);
            this._negative = isNegativeDuration(!(positive));
        }
        public DurationValue(int years, int months, int days, int hours, int minutes, long seconds, int nanoseconds, Saxon.Hej.type.AtomicType typeLabel) : base (typeLabel) {
            bool somePositive = (((((((years>0) || (months>0)) || (days>0)) || (hours>0)) || (minutes>0)) || (seconds>0)) || (nanoseconds>0));
            bool someNegative = (((((((years<0) || (months<0)) || (days<0)) || (hours<0)) || (minutes<0)) || (seconds<0)) || (nanoseconds<0));
            if ((somePositive && someNegative)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Some component values are positive and some negative");
            }
            if (someNegative) {
                years = -(years);
                months = -(months);
                days = -(days);
                hours = -(hours);
                minutes = -(minutes);
                seconds = -(seconds);
                nanoseconds = -(nanoseconds);
            }
            if (((((double)(years)*12)+(double)(months))>System.Int32.MaxValue)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Duration months limit exceeded");
            }
            if (((((((double)(days)*(((24*60)*60)))+((double)(hours)*((60*60))))+((double)(minutes)*60))+(double)(seconds))>System.Int64.MaxValue)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Duration seconds limit exceeded");
            }
            this._months = ((years*12)+months);
            long h = ((days*24L)+hours);
            long m = ((h*60L)+minutes);
            this._seconds = ((m*60L)+seconds);
            this._nanoseconds = nanoseconds;
            this._negative = someNegative;
        }
        protected internal static void formatFractionalSeconds(Saxon.Hej.str.UnicodeBuilder sb, int seconds, long nanosecs) {
            string mss = (nanosecs+"");
            if ((seconds == 0)) {
                mss = ("0000000000"+mss);
                mss = mss.Substring((mss.Length-10));
            }
            sb.append(mss.Substring(0, (mss.Length-9) - 0));
            sb.append('.');
            int lastSigDigit = (mss.Length-1);
            while ((mss[lastSigDigit] == '0')) {
                lastSigDigit--;
            }
            sb.append(mss.Substring((mss.Length-9), (lastSigDigit+1) - (mss.Length-9)));
            sb.append('S');
        }
        protected internal bool isNegativeDuration(bool nonPositive) {
            if ((((_months == 0) && (_seconds == 0L)) && (_nanoseconds == 0))) {
                return false;
            } else {
                return nonPositive;
            }
        }
        public static Saxon.Hej.type.ConversionResult makeDuration(Saxon.Hej.str.UnicodeString s) {
            return makeDuration(s, true, true);
        }
        protected internal static Saxon.Hej.type.ConversionResult makeDuration(Saxon.Hej.str.UnicodeString s, bool allowYM, bool allowDT) {
            int years = 0, months = 0, days = 0, hours = 0, minutes = 0, seconds = 0, nanoseconds = 0;
            bool negative = false;
            Saxon.Ejava.util.StringTokenizer tok = new Saxon.Ejava.util.StringTokenizer(Saxon.Hej.value.Whitespace.trim(s).ToString(), "-+.PYMDTHS", true);
            int components = 0;
            if (!(tok.hasMoreTokens())) {
                return badDuration("empty string", s);
            }
            string part = (string)(tok.nextToken());
            if ("+".Equals(part)) {
                return badDuration("+ sign not allowed in a duration", s);
            } else {
                if ("-".Equals(part)) {
                    negative = true;
                    part = (string)(tok.nextToken());
                }
            }
            if (!("P".Equals(part))) {
                return badDuration("missing 'P'", s);
            }
            int state = 0;
            while (tok.hasMoreTokens()) {
                part = (string)(tok.nextToken());
                if ("T".Equals(part)) {
                    state = 4;
                    if (!(tok.hasMoreTokens())) {
                        return badDuration("T must be followed by time components", s);
                    }
                    part = (string)(tok.nextToken());
                }
                int value = simpleInteger(part);
                if ((value<0)) {
                    if ((value == -2)) {
                        return badDuration("component of duration exceeds Saxon limits", s, "FODT0002");
                    } else {
                        return badDuration("invalid or non-numeric component", s);
                    }
                }
                if (!(tok.hasMoreTokens())) {
                    return badDuration("missing unit letter at end", s);
                }
                char delim = ((string)(tok.nextToken()))[0];
                switch (delim) {
                  case 'Y':
                    if ((state>0)) {
                        return badDuration("Y is out of sequence", s);
                    }
                    if (!(allowYM)) {
                        return badDuration("Year component is not allowed in dayTimeDuration", s);
                    }
                    years = value;
                    state = 1;
                    components++;
                    break ;
                  case 'M':
                    if (((state == 4) || (state == 5))) {
                        if (!(allowDT)) {
                            return badDuration("Minute component is not allowed in yearMonthDuration", s);
                        }
                        minutes = value;
                        state = 6;
                        components++;
                        break ;
                    } else {
                        if (((state == 0) || (state == 1))) {
                            if (!(allowYM)) {
                                return badDuration("Month component is not allowed in dayTimeDuration", s);
                            }
                            months = value;
                            state = 2;
                            components++;
                            break ;
                        } else {
                            return badDuration("M is out of sequence", s);
                        }
                    }
                  case 'D':
                    if ((state>2)) {
                        return badDuration("D is out of sequence", s);
                    }
                    if (!(allowDT)) {
                        return badDuration("Day component is not allowed in yearMonthDuration", s);
                    }
                    days = value;
                    state = 3;
                    components++;
                    break ;
                  case 'H':
                    if ((state != 4)) {
                        return badDuration("H is out of sequence", s);
                    }
                    if (!(allowDT)) {
                        return badDuration("Hour component is not allowed in yearMonthDuration", s);
                    }
                    hours = value;
                    state = 5;
                    components++;
                    break ;
                  case '.':
                    if (((state<4) || (state>6))) {
                        return badDuration("misplaced decimal point", s);
                    }
                    seconds = value;
                    state = 7;
                    break ;
                  case 'S':
                    if (((state<4) || (state>7))) {
                        return badDuration("S is out of sequence", s);
                    }
                    if (!(allowDT)) {
                        return badDuration("Seconds component is not allowed in yearMonthDuration", s);
                    }
                    if ((state == 7)) {
                        Saxon.Ejava.lang.StringBuilder frac = new Saxon.Ejava.lang.StringBuilder(part);
                        while ((frac.length()<9)) {
                            frac.append("0");
                        }
                        part = frac.ToString();
                        if ((part.Length>9)) {
                            part = part.Substring(0, 9 - 0);
                        }
                        value = simpleInteger(part);
                        if ((value<0)) {
                            return badDuration("non-numeric fractional seconds", s);
                        }
                        nanoseconds = value;
                    } else {
                        seconds = value;
                    }
                    state = 8;
                    components++;
                    break ;
                  default:
                    return badDuration(("misplaced "+delim), s);
                }
            }
            if ((components == 0)) {
                return badDuration("Duration specifies no components", s);
            }
            if (negative) {
                years = -(years);
                months = -(months);
                days = -(days);
                hours = -(hours);
                minutes = -(minutes);
                seconds = -(seconds);
                nanoseconds = -(nanoseconds);
            }
            try {
                return new Saxon.Hej.value.DurationValue(years, months, days, hours, minutes, seconds, nanoseconds, Saxon.Hej.type.BuiltInAtomicType.DURATION);
            } catch (Saxon.Ejava.lang.IllegalArgumentException err) {
                return new Saxon.Hej.type.ValidationFailure(err.Message);
            }
        }
        protected internal static Saxon.Hej.type.ValidationFailure badDuration(string msg, Saxon.Hej.str.UnicodeString s) {
            Saxon.Hej.type.ValidationFailure err = new Saxon.Hej.type.ValidationFailure((((("Invalid duration value '"+s)+"' (")+msg)+')'));
            err.setErrorCode("FORG0001");
            return err;
        }
        protected internal static Saxon.Hej.type.ValidationFailure badDuration(string msg, Saxon.Hej.str.UnicodeString s, string errorCode) {
            Saxon.Hej.type.ValidationFailure err = new Saxon.Hej.type.ValidationFailure((((("Invalid duration value '"+s)+"' (")+msg)+')'));
            err.setErrorCode(errorCode);
            return err;
        }
        protected internal static int simpleInteger(string s) {
            long result = 0;
            int len = s.Length;
            if ((len == 0)) {
                return -1;
            }
            for (int i = 0; (i<len); i++) {
                char c = s[i];
                if (((c>='0') && (c<='9'))) {
                    result = ((result*10)+((c-'0')));
                    if ((result>System.Int32.MaxValue)) {
                        return -2;
                    }
                } else {
                    return -1;
                }
            }
            return (int)(result);
        }
        public override Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            if (_negative) {
                return new Saxon.Hej.value.DurationValue(0, -(_months), 0, 0, 0, -(_seconds), -(_nanoseconds), typeLabel);
            } else {
                return new Saxon.Hej.value.DurationValue(0, _months, 0, 0, 0, _seconds, _nanoseconds, typeLabel);
            }
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.DURATION;
        }
        public int signum() {
            if (_negative) {
                return -1;
            }
            if ((((_months == 0) && (_seconds == 0L)) && (_nanoseconds == 0))) {
                return 0;
            }
            return +(1);
        }
        public int getYears() {
            return (_months/12);
        }
        public int getMonths() {
            return (_months%12);
        }
        public int getDays() {
            return (int)(((_seconds/(((24L*60L)*60L)))));
        }
        public int getHours() {
            return (int)((((_seconds%(((24L*60L)*60L)))/((60L*60L)))));
        }
        public int getMinutes() {
            return (int)((((_seconds%((60L*60L)))/60L)));
        }
        public int getSeconds() {
            return (int)(((_seconds%60L)));
        }
        public int getMicroseconds() {
            return (_nanoseconds/1000);
        }
        public int getNanoseconds() {
            return _nanoseconds;
        }
        public int getTotalMonths() {
            return (_negative ? -(_months) : _months);
        }
        public Singulink.Numerics.BigDecimal getTotalSeconds() {
            Singulink.Numerics.BigDecimal dec = Saxon.Impl.Helpers.BigDecimalUtils.ValueOf((_negative ? -(_seconds) : _seconds));
            if ((_nanoseconds != 0)) {
                dec = (dec+new Singulink.Numerics.BigDecimal(Saxon.Impl.Helpers.BigIntegerUtils.ValueOf((_negative ? -(_nanoseconds) : _nanoseconds)), -(9)));
            }
            return dec;
        }
        public override Saxon.Hej.str.UnicodeString getPrimitiveStringValue() {
            if ((((_months == 0) && (_seconds == 0L)) && (_nanoseconds == 0))) {
                return Saxon.Hej.str.BMPString.of("PT0S");
            }
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder(16);
            if (_negative) {
                sb.append('-');
            }
            int years = getYears();
            int months = getMonths();
            int days = getDays();
            int hours = getHours();
            int minutes = getMinutes();
            int seconds = getSeconds();
            sb.append("P");
            if ((years != 0)) {
                sb.append((years+"Y"));
            }
            if ((months != 0)) {
                sb.append((months+"M"));
            }
            if ((days != 0)) {
                sb.append((days+"D"));
            }
            if (((((hours != 0) || (minutes != 0)) || (seconds != 0)) || (_nanoseconds != 0))) {
                sb.append("T");
            }
            if ((hours != 0)) {
                sb.append((hours+"H"));
            }
            if ((minutes != 0)) {
                sb.append((minutes+"M"));
            }
            if (((seconds != 0) || (_nanoseconds != 0))) {
                if (((seconds != 0) && (_nanoseconds == 0))) {
                    sb.append((seconds+"S"));
                } else {
                    formatFractionalSeconds(sb, seconds, (((seconds*1_000_000_000L))+_nanoseconds));
                }
            }
            return sb.toUnicodeString();
        }
        public virtual double getLengthInSeconds() {
            double a = ((((((_months*((365.242199/12.0)))*24)*60)*60)+_seconds)+(((double)(_nanoseconds)/1_000_000_000)));
            return (_negative ? -(a) : a);
        }
        public override Saxon.Hej.value.AtomicValue getComponent(Saxon.Hej.functions.AccessorFn.Component component) {
            switch (component) {
              case Saxon.Hej.functions.AccessorFn.Component.YEAR:
                return Saxon.Hej.value.Int64Value.makeIntegerValue((_negative ? -(getYears()) : getYears()));
              case Saxon.Hej.functions.AccessorFn.Component.MONTH:
                return Saxon.Hej.value.Int64Value.makeIntegerValue((_negative ? -(getMonths()) : getMonths()));
              case Saxon.Hej.functions.AccessorFn.Component.DAY:
                return Saxon.Hej.value.Int64Value.makeIntegerValue((_negative ? -(getDays()) : getDays()));
              case Saxon.Hej.functions.AccessorFn.Component.HOURS:
                return Saxon.Hej.value.Int64Value.makeIntegerValue((_negative ? -(getHours()) : getHours()));
              case Saxon.Hej.functions.AccessorFn.Component.MINUTES:
                return Saxon.Hej.value.Int64Value.makeIntegerValue((_negative ? -(getMinutes()) : getMinutes()));
              case Saxon.Hej.functions.AccessorFn.Component.SECONDS:
                Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(16);
                string ms = ("000000000"+_nanoseconds);
                ms = ms.Substring((ms.Length-9));
                sb.append((((((_negative ? "-" : ""))+getSeconds())+'.')+ms));
                return Saxon.Hej.value.BigDecimalValue.parse(sb.ToString());
              case Saxon.Hej.functions.AccessorFn.Component.WHOLE_SECONDS:
                return Saxon.Hej.value.Int64Value.makeIntegerValue((_negative ? -(_seconds) : _seconds));
              case Saxon.Hej.functions.AccessorFn.Component.MICROSECONDS:
                return new Saxon.Hej.value.Int64Value((((_negative ? -(_nanoseconds) : _nanoseconds))/1000));
              case Saxon.Hej.functions.AccessorFn.Component.NANOSECONDS:
                return new Saxon.Hej.value.Int64Value((_negative ? -(_nanoseconds) : _nanoseconds));
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown component for duration: "+component));
            }
        }
        public override Saxon.Hej.expr.sort.AtomicMatchKey getXPathMatchKey(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return this;
        }
        public override Saxon.Hej.expr.sort.XPathComparable getXPathComparable(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return null;
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Hej.value.DurationValue)) {
                Saxon.Hej.value.DurationValue d1 = this;
                Saxon.Hej.value.DurationValue d2 = (Saxon.Hej.value.DurationValue)(other);
                return ((((d1._negative == d2._negative) && (d1._months == d2._months)) && (d1._seconds == d2._seconds)) && (d1._nanoseconds == d2._nanoseconds));
            } else {
                return false;
            }
        }
        public override int GetHashCode() {
            return (getLengthInSeconds()).GetHashCode();
        }
        public virtual Saxon.Hej.value.DurationValue add(Saxon.Hej.value.DurationValue other) {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Only subtypes of xs:duration can be added");
            err.setErrorCode("XPTY0004");
            err.setIsTypeError(true);
            throw err;
        }
        public virtual Saxon.Hej.value.DurationValue subtract(Saxon.Hej.value.DurationValue other) {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Only subtypes of xs:duration can be subtracted");
            err.setErrorCode("XPTY0004");
            err.setIsTypeError(true);
            throw err;
        }
        public virtual Saxon.Hej.value.DurationValue negate() {
            if (_negative) {
                return new Saxon.Hej.value.DurationValue(0, _months, 0, 0, 0, _seconds, _nanoseconds, typeLabel);
            } else {
                return new Saxon.Hej.value.DurationValue(0, -(_months), 0, 0, 0, -(_seconds), -(_nanoseconds), typeLabel);
            }
        }
        public virtual Saxon.Hej.value.DurationValue multiply(long factor) {
            return multiply((double)(factor));
        }
        public virtual Saxon.Hej.value.DurationValue multiply(double factor) {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Only subtypes of xs:duration can be multiplied by a number");
            err.setErrorCode("XPTY0004");
            err.setIsTypeError(true);
            throw err;
        }
        public virtual Saxon.Hej.value.DurationValue multiply(Singulink.Numerics.BigDecimal factor) {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Only subtypes of xs:duration can be multiplied by a number");
            err.setErrorCode("XPTY0004");
            err.setIsTypeError(true);
            throw err;
        }
        public virtual Saxon.Hej.value.DurationValue divide(double factor) {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Only subtypes of xs:duration can be divided by a number");
            err.setErrorCode("XPTY0004");
            err.setIsTypeError(true);
            throw err;
        }
        public virtual Saxon.Hej.value.BigDecimalValue divide(Saxon.Hej.value.DurationValue other) {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Only subtypes of xs:duration can be divided by another duration");
            err.setErrorCode("XPTY0004");
            err.setIsTypeError(true);
            throw err;
        }
        public Saxon.Hej.value.DurationValue.DurationComparable getSchemaComparable() {
            int m = this._months;
            long s = this._seconds;
            int n = this._nanoseconds;
            if (this._negative) {
                s = -(s);
                m = -(m);
                n = -(n);
            }
            return new Saxon.Hej.value.DurationValue.DurationComparable(m, s, n);
        }
        public class DurationComparable : Saxon.Ejava.lang.Comparable<Saxon.Hej.value.DurationValue.DurationComparable> {
            private readonly int months;
            private readonly long seconds;
            private readonly int nanoseconds;
            public DurationComparable(int m, long s, int nanos)  {
                months = m;
                seconds = s;
                nanoseconds = nanos;
            }
            public int compareTo(Saxon.Hej.value.DurationValue.DurationComparable other) {
                if ((months == other.months)) {
                    if ((seconds == other.seconds)) {
                        return nanoseconds.CompareTo(other.nanoseconds);
                    } else {
                        return seconds.CompareTo(other.seconds);
                    }
                } else {
                    double oneDay = ((24e0*60e0)*60e0);
                    double min0 = ((monthsToDaysMinimum(months)*oneDay)+seconds);
                    double max0 = ((monthsToDaysMaximum(months)*oneDay)+seconds);
                    double min1 = ((monthsToDaysMinimum(other.months)*oneDay)+other.seconds);
                    double max1 = ((monthsToDaysMaximum(other.months)*oneDay)+other.seconds);
                    if ((max0<min1)) {
                        return -1;
                    } else {
                        if ((min0>max1)) {
                            return +(1);
                        } else {
                            return Saxon.Hej.om.SequenceTool.INDETERMINATE_ORDERING;
                        }
                    }
                }
            }
            public override bool Equals(System.Object o) {
                return ((o is Saxon.Hej.value.DurationValue.DurationComparable) && (compareTo((Saxon.Hej.value.DurationValue.DurationComparable)(o)) == 0));
            }
            public override int GetHashCode() {
                return (months^(int)(seconds));
            }
            private int monthsToDaysMinimum(int months) {
                if ((months<0)) {
                    return -(monthsToDaysMaximum(-(months)));
                }
                if ((months<12)) {
                    int[] shortest = {0, 28, 59, 89, 120, 150, 181, 212, 242, 273, 303, 334};
                    return shortest[months];
                } else {
                    int years = (months/12);
                    int remainingMonths = (months%12);
                    int yearDays = (((((years*365)+((years%4)))-((years%100)))+((years%400)))-1);
                    return (yearDays+monthsToDaysMinimum(remainingMonths));
                }
            }
            private int monthsToDaysMaximum(int months) {
                if ((months<0)) {
                    return -(monthsToDaysMinimum(-(months)));
                }
                if ((months<12)) {
                    int[] longest = {0, 31, 62, 92, 123, 153, 184, 215, 245, 276, 306, 337};
                    return longest[months];
                } else {
                    int years = (months/12);
                    int remainingMonths = (months%12);
                    int yearDays = (((((years*365)+((years%4)))-((years%100)))+((years%400)))+1);
                    return (yearDays+monthsToDaysMaximum(remainingMonths));
                }
            }
        
        }
    
    }
}