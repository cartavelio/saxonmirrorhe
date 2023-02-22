using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public sealed class BigDecimalValue : Saxon.Hej.value.DecimalValue {
        public const int DIVIDE_PRECISION = 18;
        private readonly Singulink.Numerics.BigDecimal value;
        private double doubleValue = System.Double.NaN;
        public static readonly Singulink.Numerics.BigDecimal BIG_DECIMAL_ONE_MILLION = Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(1_000_000);
        public static readonly Singulink.Numerics.BigDecimal BIG_DECIMAL_ONE_BILLION = Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(1_000_000_000);
        public static readonly Saxon.Hej.value.BigDecimalValue ZERO = new Saxon.Hej.value.BigDecimalValue(Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(0));
        public static readonly Saxon.Hej.value.BigDecimalValue ONE = new Saxon.Hej.value.BigDecimalValue(Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(1));
        public static readonly Saxon.Hej.value.BigDecimalValue TWO = new Saxon.Hej.value.BigDecimalValue(Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(2));
        public static readonly Saxon.Hej.value.BigDecimalValue THREE = new Saxon.Hej.value.BigDecimalValue(Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(3));
        public static readonly Singulink.Numerics.BigDecimal MAX_INT = Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(System.Int32.MaxValue);
        public BigDecimalValue(Singulink.Numerics.BigDecimal value)  {
            this.value = value;
            typeLabel = Saxon.Hej.type.BuiltInAtomicType.DECIMAL;
        }
        private static readonly Saxon.Ejava.util.regex.Pattern decimalPattern = Saxon.Ejava.util.regex.Pattern.compile("(\\-|\\+)?((\\.[0-9]+)|([0-9]+(\\.[0-9]*)?))");
        public static Saxon.Hej.type.ConversionResult makeDecimalValue(string @in, bool validate) {
            try {
                return parse(@in);
            } catch (Saxon.Ejava.lang.NumberFormatException err) {
                Saxon.Hej.type.ValidationFailure e = new Saxon.Hej.type.ValidationFailure(((("Cannot convert string "+Saxon.Hej.trans.Err.wrap(@in, Saxon.Hej.trans.Err.VALUE))+" to xs:decimal: ")+err.Message));
                e.setErrorCode("FORG0001");
                return e;
            }
        }
        public static Saxon.Hej.value.BigDecimalValue parse(string @in) {
            Saxon.Ejava.lang.StringBuilder digits = new Saxon.Ejava.lang.StringBuilder(@in.Length);
            int scale = 0;
            int state = 0;
            bool foundDigit = false;
            int len = @in.Length;
            for (int i = 0; (i<len); i++) {
                char c = @in[i];
                switch (c) {
                  case ' ':
                  case '\t':
                  case '\r':
                  case '\n':
                    if ((state != 0)) {
                        state = 5;
                    }
                    break ;
                  case '+':
                    if ((state != 0)) {
                        throw new Saxon.Ejava.lang.NumberFormatException("unexpected sign");
                    }
                    state = 1;
                    break ;
                  case '-':
                    if ((state != 0)) {
                        throw new Saxon.Ejava.lang.NumberFormatException("unexpected sign");
                    }
                    state = 1;
                    digits.append(c);
                    break ;
                  case '0':
                  case '1':
                  case '2':
                  case '3':
                  case '4':
                  case '5':
                  case '6':
                  case '7':
                  case '8':
                  case '9':
                    if ((state == 0)) {
                        state = 1;
                    } else {
                        if ((state>=3)) {
                            scale++;
                        }
                    }
                    if ((state == 5)) {
                        throw new Saxon.Ejava.lang.NumberFormatException("contains embedded whitespace");
                    }
                    digits.append(c);
                    foundDigit = true;
                    break ;
                  case '.':
                    if ((state == 5)) {
                        throw new Saxon.Ejava.lang.NumberFormatException("contains embedded whitespace");
                    }
                    if ((state>=3)) {
                        throw new Saxon.Ejava.lang.NumberFormatException("more than one decimal point");
                    }
                    state = 3;
                    break ;
                  default:
                    throw new Saxon.Ejava.lang.NumberFormatException((("invalid character '"+c)+"'"));
                }
            }
            if (!(foundDigit)) {
                throw new Saxon.Ejava.lang.NumberFormatException("no digits in value");
            }
            while ((scale>0)) {
                if ((digits.charAt((digits.length()-1)) == '0')) {
                    digits.setLength((digits.length()-1));
                    scale--;
                } else {
                    break ;
                }
            }
            if (((digits.length() == 0) || (((digits.length() == 1) && (digits.charAt(0) == '-'))))) {
                return Saxon.Hej.value.BigDecimalValue.ZERO;
            }
            System.Numerics.BigInteger bigInt = Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(digits.ToString());
            Singulink.Numerics.BigDecimal bigDec = new Singulink.Numerics.BigDecimal(bigInt, -(scale));
            return new Saxon.Hej.value.BigDecimalValue(bigDec);
        }
        public static bool castableAsDecimal(string @in) {
            string trimmed = Saxon.Hej.value.Whitespace.trim(@in).ToString();
            return decimalPattern.matcher(trimmed).matches();
        }
        public BigDecimalValue(double @in)  {
            try {
                Singulink.Numerics.BigDecimal d = Singulink.Numerics.BigDecimal.FromDouble(@in, Singulink.Numerics.FloatConversion.Roundtrip);
                value = d;
            } catch (System.Exception err) {
                Saxon.Hej.type.ValidationFailure e = new Saxon.Hej.type.ValidationFailure((("Cannot convert double "+Saxon.Hej.trans.Err.wrap((@in+""), Saxon.Hej.trans.Err.VALUE))+" to decimal"));
                e.setErrorCode("FOCA0002");
                throw e.makeException();
            }
            typeLabel = Saxon.Hej.type.BuiltInAtomicType.DECIMAL;
        }
        public BigDecimalValue(long @in)  {
            value = Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(@in);
            typeLabel = Saxon.Hej.type.BuiltInAtomicType.DECIMAL;
        }
        public override Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            if ((typeLabel.getPrimitiveItemType() == Saxon.Hej.type.BuiltInAtomicType.INTEGER)) {
                return Saxon.Hej.value.IntegerValue.makeIntegerValue(((System.Numerics.BigInteger)value)).copyAsSubType(typeLabel);
            } else {
                Saxon.Hej.value.BigDecimalValue v = new Saxon.Hej.value.BigDecimalValue(value);
                v.typeLabel = typeLabel;
                return v;
            }
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.DECIMAL;
        }
        public override double getDoubleValue() {
            if (System.Double.IsNaN(doubleValue)) {
                doubleValue = ((double)value);
            }
            return doubleValue;
        }
        public override float getFloatValue() {
            return (float)(((double)value));
        }
        public override long longValue() {
            return (long)(((double)value));
        }
        public override Singulink.Numerics.BigDecimal getDecimalValue() {
            return value;
        }
        public override int GetHashCode() {
            Singulink.Numerics.BigDecimal round = value;
            long longVal = ((long)round);
            if (((longVal>System.Int32.MinValue) && (longVal<System.Int32.MaxValue))) {
                return (int)(longVal);
            } else {
                return (getDoubleValue()).GetHashCode();
            }
        }
        public override bool effectiveBooleanValue() {
            return (value.Sign != 0);
        }
        public override Saxon.Hej.str.UnicodeString getCanonicalLexicalRepresentation() {
            Saxon.Hej.str.UnicodeString s = this.getUnicodeStringValue().tidy();
            if ((s.indexOf('.')<0)) {
                s = s.concat(Saxon.Hej.str.StringConstants.POINT_ZERO);
            }
            return s;
        }
        public override Saxon.Hej.str.UnicodeString getPrimitiveStringValue() {
            return Saxon.Hej.str.BMPString.of(decimalToString(value, new Saxon.Ejava.lang.StringBuilder(16)).ToString());
        }
        public static Saxon.Ejava.lang.StringBuilder decimalToString(Singulink.Numerics.BigDecimal value, Saxon.Ejava.lang.StringBuilder fsb) {
            return fsb.append(value.ToString()); /*maybe imprecise?*/
        }
        public override Saxon.Hej.value.NumericValue negate() {
            return new Saxon.Hej.value.BigDecimalValue(-value);
        }
        public override Saxon.Hej.value.NumericValue floor() {
            return new BigDecimalValue(Singulink.Numerics.BigDecimal.Floor(value));
        }
        public override Saxon.Hej.value.NumericValue ceiling() {
            return new BigDecimalValue(Singulink.Numerics.BigDecimal.Ceiling(value));
        }
        public override Saxon.Hej.value.NumericValue round(int scale) {
            return new BigDecimalValue(Saxon.Impl.Helpers.BigDecimalUtils.Round(value, scale));
        }
        public override Saxon.Hej.value.NumericValue roundHalfToEven(int scale) {
            return new BigDecimalValue(Singulink.Numerics.BigDecimal.Round(value, scale, Singulink.Numerics.RoundingMode.MidpointToEven));
        }
        public override int signum() {
            return value.Sign;
        }
        public override bool isWholeNumber() {
            return value.DecimalPlaces == 0;
        }
        public override int asSubscript() {
            if (((isWholeNumber() && (value.CompareTo(Singulink.Numerics.BigDecimal.Zero)>0)) && (value.CompareTo(MAX_INT)<=0))) {
                try {
                    return (int)(longValue());
                } catch (Saxon.Hej.trans.XPathException e) {
                    return -1;
                }
            } else {
                return -1;
            }
        }
        public override Saxon.Hej.value.NumericValue abs() {
            if ((value.Sign>0)) {
                return this;
            } else {
                return new Saxon.Hej.value.BigDecimalValue(-value);
            }
        }
        public override Saxon.Hej.expr.sort.XPathComparable getXPathComparable(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return this;
        }
        public override int compareTo(Saxon.Hej.expr.sort.XPathComparable other) {
            if ((other is Saxon.Hej.value.NumericValue)) {
                if (Saxon.Hej.value.NumericValue.isInteger(((Saxon.Hej.value.NumericValue)(other)))) {
                    try {
                        return value.CompareTo(((Saxon.Hej.value.NumericValue)(other)).getDecimalValue());
                    } catch (Saxon.Hej.trans.XPathException err) {
                        throw new Saxon.Ejava.lang.AssertionError("Conversion of integer to decimal should never fail");
                    }
                } else {
                    if ((other is Saxon.Hej.value.BigDecimalValue)) {
                        return value.CompareTo(((Saxon.Hej.value.BigDecimalValue)(other)).value);
                    } else {
                        if ((other is Saxon.Hej.value.FloatValue)) {
                            return -(other.compareTo(this));
                        } else {
                            return base.compareTo(other);
                        }
                    }
                }
            } else {
                throw new System.InvalidCastException(("Cannot compare xs:decimal to "+other.ToString()));
            }
        }
        public override int compareTo(long other) {
            if ((other == 0)) {
                return value.Sign;
            }
            return value.CompareTo(Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(other));
        }
        public override bool isIdentical(Saxon.Hej.value.AtomicValue v) {
            return (((v is Saxon.Hej.value.DecimalValue)) && Equals(v));
        }
    
    }
}