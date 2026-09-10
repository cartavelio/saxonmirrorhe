using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public sealed class YearMonthDurationValue : Saxon.Hej.value.DurationValue, Saxon.Hej.expr.sort.XPathComparable, Saxon.Hej.value.ContextFreeAtomicValue {
        public YearMonthDurationValue(int months, Saxon.Hej.type.AtomicType typeLabel) : base (0, months, 0, 0, 0, 0, 0, typeLabel) {
        }
        public static Saxon.Hej.type.ConversionResult makeYearMonthDurationValue(Saxon.Hej.str.UnicodeString s) {
            Saxon.Hej.type.ConversionResult d = Saxon.Hej.value.DurationValue.makeDuration(s, true, false);
            if ((d is Saxon.Hej.type.ValidationFailure)) {
                return d;
            }
            Saxon.Hej.value.DurationValue dv = (Saxon.Hej.value.DurationValue)(d);
            return Saxon.Hej.value.YearMonthDurationValue.fromMonths(((((dv.getYears()*12)+dv.getMonths()))*dv.signum()));
        }
        public override Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            return new Saxon.Hej.value.YearMonthDurationValue(getLengthInMonths(), typeLabel);
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.YEAR_MONTH_DURATION;
        }
        public override Saxon.Hej.str.UnicodeString getPrimitiveStringValue() {
            int y = getYears();
            int m = getMonths();
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder(16);
            if (_negative) {
                sb.append('-');
            }
            sb.append('P');
            if ((y != 0)) {
                sb.append((y+"Y"));
            }
            if (((m != 0) || (y == 0))) {
                sb.append((m+"M"));
            }
            return sb.toUnicodeString();
        }
        public int getLengthInMonths() {
            return (_months*((_negative ? -1 : +(1))));
        }
        public static Saxon.Hej.value.YearMonthDurationValue fromMonths(int months) {
            return new Saxon.Hej.value.YearMonthDurationValue(months, Saxon.Hej.type.BuiltInAtomicType.YEAR_MONTH_DURATION);
        }
        public override Saxon.Hej.value.YearMonthDurationValue multiply(long factor) {
            if (((System.Math.Abs(factor)<30_000) && (System.Math.Abs(_months)<30_000))) {
                return Saxon.Hej.value.YearMonthDurationValue.fromMonths(((int)(factor)*getLengthInMonths()));
            } else {
                return multiply((double)(factor));
            }
        }
        public override Saxon.Hej.value.YearMonthDurationValue multiply(double n) {
            if (System.Double.IsNaN(n)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot multiply a duration by NaN");
                err.setErrorCode("FOCA0005");
                throw err;
            }
            double m = getLengthInMonths();
            double product = (n*m);
            if (((System.Double.IsInfinity(product) || (product>System.Int32.MaxValue)) || (product<System.Int32.MinValue))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Overflow when multiplying a duration by a number");
                err.setErrorCode("FODT0002");
                throw err;
            }
            return fromMonths((int)(new Saxon.Hej.value.DoubleValue(product).round(0).longValue()));
        }
        public override Saxon.Hej.value.YearMonthDurationValue multiply(Singulink.Numerics.BigDecimal n) {
            int m = getLengthInMonths();
            Singulink.Numerics.BigDecimal product = (n*Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(m));
            if ((Singulink.Numerics.BigDecimal.Abs(product).CompareTo(Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(System.Int32.MaxValue))>0)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Overflow when multiplying a duration by a number");
                err.setErrorCode("FODT0002");
                throw err;
            }
            return fromMonths((int)(new Saxon.Hej.value.BigDecimalValue(product).round(0).longValue()));
        }
        public override Saxon.Hej.value.DurationValue divide(double n) {
            if (System.Double.IsNaN(n)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot divide a duration by NaN");
                err.setErrorCode("FOCA0005");
                throw err;
            }
            double m = getLengthInMonths();
            double product = (m/n);
            if (((System.Double.IsInfinity(product) || (product>System.Int32.MaxValue)) || (product<System.Int32.MinValue))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Overflow when dividing a duration by a number");
                err.setErrorCode("FODT0002");
                throw err;
            }
            return fromMonths((int)(new Saxon.Hej.value.DoubleValue(product).round(0).longValue()));
        }
        public override Saxon.Hej.value.BigDecimalValue divide(Saxon.Hej.value.DurationValue other) {
            if ((other is Saxon.Hej.value.YearMonthDurationValue)) {
                Singulink.Numerics.BigDecimal v1 = Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(getLengthInMonths());
                Singulink.Numerics.BigDecimal v2 = Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(((Saxon.Hej.value.YearMonthDurationValue)(other)).getLengthInMonths());
                if ((v2.Sign == 0)) {
                    throw new Saxon.Hej.trans.XPathException("Divide by zero (durations)", "FOAR0001");
                }
                return new Saxon.Hej.value.BigDecimalValue(divideBigDecimal(v1, v2));
            } else {
                throw new Saxon.Hej.trans.XPathException("Cannot divide two durations of different type", "XPTY0004");
            }
        }
        private Singulink.Numerics.BigDecimal divideBigDecimal(Singulink.Numerics.BigDecimal v1, Singulink.Numerics.BigDecimal v2) {
            return Singulink.Numerics.BigDecimal.Divide(v1, v2, 20, Singulink.Numerics.RoundingMode.MidpointToEven);
        }
        public override Saxon.Hej.value.DurationValue add(Saxon.Hej.value.DurationValue other) {
            if ((other is Saxon.Hej.value.YearMonthDurationValue)) {
                return fromMonths((getLengthInMonths()+((Saxon.Hej.value.YearMonthDurationValue)(other)).getLengthInMonths()));
            } else {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot add two durations of different type");
                err.setErrorCode("XPTY0004");
                throw err;
            }
        }
        public override Saxon.Hej.value.DurationValue subtract(Saxon.Hej.value.DurationValue other) {
            if ((other is Saxon.Hej.value.YearMonthDurationValue)) {
                return fromMonths((getLengthInMonths()-((Saxon.Hej.value.YearMonthDurationValue)(other)).getLengthInMonths()));
            } else {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot subtract two durations of different type");
                err.setErrorCode("XPTY0004");
                throw err;
            }
        }
        public override Saxon.Hej.value.DurationValue negate() {
            return fromMonths(-(getLengthInMonths()));
        }
        public int compareTo(Saxon.Hej.expr.sort.XPathComparable other) {
            if ((other is Saxon.Hej.value.YearMonthDurationValue)) {
                return getLengthInMonths().CompareTo(((Saxon.Hej.value.YearMonthDurationValue)(other)).getLengthInMonths());
            } else {
                throw new System.InvalidCastException(("Cannot compare xs:yearMonthDuration with "+other));
            }
        }
        public override Saxon.Hej.expr.sort.XPathComparable getXPathComparable(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return this;
        }
        public Saxon.Hej.expr.sort.XPathComparable getXPathComparable() {
            return this;
        }
        public override Saxon.Hej.expr.sort.AtomicMatchKey getXPathMatchKey(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return this;
        }
    
    }
}