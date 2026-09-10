using System;
using System.Xml.XPath;
using Saxon.Hej.functions;
using Saxon.Hej.ma.map;
using Saxon.Hej.value;
using Singulink.Numerics;

namespace Saxon.Impl.Helpers {
    public class BigDecimalUtils {
        public static Singulink.Numerics.BigDecimal ValueOf(long val) {
            return (Singulink.Numerics.BigDecimal)val;
        }

        public static Singulink.Numerics.BigDecimal ValueOf(double val) {
            return Singulink.Numerics.BigDecimal.FromDouble(val, Singulink.Numerics.FloatConversion.Roundtrip);
        }

        public static Singulink.Numerics.BigDecimal ValueOf(string s) {
            return Singulink.Numerics.BigDecimal.Parse(s);
        }
        
        
        public static Singulink.Numerics.BigDecimal StripTrailingZeros(Singulink.Numerics.BigDecimal val) {
            return val;
        }
        
        public static Singulink.Numerics.BigDecimal Remainder(Singulink.Numerics.BigDecimal a, Singulink.Numerics.BigDecimal b) {
            // The remainder is given by this.subtract(this.divideToIntegralValue(divisor).multiply(divisor)).
            // Note that this is not the modulo operation (the result can be negative).
            return a - Idiv(a, b)*b;
        }
        
        public static Singulink.Numerics.BigDecimal[] DivideAndRemainder(Singulink.Numerics.BigDecimal a, Singulink.Numerics.BigDecimal b) {
            BigDecimal quot = Idiv(a, b);
            return new BigDecimal[]{quot, a - quot*b};
        }
        
        public static MapItem DivideDecimals(DecimalValue a, DecimalValue b, int requiredDecimalPlaces) {
            // The Singulink Divide method allows us to specify the total number of digits in the result, whereas we want
            // to specify the number of digits after the decimal point. So we do a full-precision division and then round
            // the result.
            BigDecimal aDec = a.getDecimalValue();
            BigDecimal bDec = b.getDecimalValue();
            BigDecimal preciseQuotient;
            try {
                preciseQuotient = BigDecimal.Divide(
                    aDec, bDec, Math.Max(requiredDecimalPlaces, 50), RoundingMode.ToZero);
            }
            catch (System.DivideByZeroException e) {
                throw new Saxon.Hej.trans.XPathException("Division by zero", "FOAR0001");
            }
            BigDecimal roundedQuotient = Round(preciseQuotient, requiredDecimalPlaces);
            BigDecimal remainder = aDec - roundedQuotient * bDec;
            return new ShapedMap(quotientAndRemainder,
                new BigDecimalValue(roundedQuotient),
                new BigDecimalValue(remainder));

        }
        
        private static Saxon.Hej.ma.map.Shape quotientAndRemainder = 
            new Saxon.Hej.ma.map.Shape(new Saxon.Hej.str.Twine8("quotient"), new Saxon.Hej.str.Twine8("remainder"));

        
        public static Singulink.Numerics.BigDecimal Idiv(Singulink.Numerics.BigDecimal a, Singulink.Numerics.BigDecimal b) {
            Singulink.Numerics.BigDecimal quot = a / b;
            return quot.Sign < 0 ? Singulink.Numerics.BigDecimal.Ceiling(quot) : Singulink.Numerics.BigDecimal.Floor(quot);
        }
        
        public static Singulink.Numerics.BigDecimal Round(Singulink.Numerics.BigDecimal a, int scale) {
            return a.Sign switch {
                0 => a,
                _ => BigDecimal.Round(a, scale, Singulink.Numerics.RoundingMode.MidpointToPositiveInfinity)
            };
        }

        public static Singulink.Numerics.BigDecimal Round(Singulink.Numerics.BigDecimal a, int scale,
            Round.RoundingRule roundingRule) {
            #pragma warning disable 8524
            return roundingRule switch {
                Hej.functions.Round.RoundingRule.FLOOR =>
                    BigDecimal.Round(a, scale, Singulink.Numerics.RoundingMode.ToNegativeInfinity),
                Hej.functions.Round.RoundingRule.CEILING =>
                    BigDecimal.Round(a, scale, Singulink.Numerics.RoundingMode.ToPositiveInfinity),
                Hej.functions.Round.RoundingRule.TOWARD_ZERO =>
                    BigDecimal.Round(a, scale, Singulink.Numerics.RoundingMode.ToZero),
                Hej.functions.Round.RoundingRule.AWAY_FROM_ZERO =>
                    BigDecimal.Round(a, scale, Singulink.Numerics.RoundingMode.AwayFromZero),
                Hej.functions.Round.RoundingRule.HALF_TO_FLOOR =>
                    BigDecimal.Round(a, scale, Singulink.Numerics.RoundingMode.MidpointToNegativeInfinity),
                Hej.functions.Round.RoundingRule.HALF_TO_CEILING =>
                    BigDecimal.Round(a, scale, Singulink.Numerics.RoundingMode.MidpointToPositiveInfinity),
                Hej.functions.Round.RoundingRule.HALF_TOWARD_ZERO =>
                    BigDecimal.Round(a, scale, Singulink.Numerics.RoundingMode.MidpointToZero),
                Hej.functions.Round.RoundingRule.HALF_AWAY_FROM_ZERO =>
                    BigDecimal.Round(a, scale, Singulink.Numerics.RoundingMode.MidpointAwayFromZero),
                Hej.functions.Round.RoundingRule.HALF_TO_EVEN =>
                    BigDecimal.Round(a, scale, Singulink.Numerics.RoundingMode.MidpointToEven)
            };
            #pragma warning restore 8524
        }

        public static int IntValueExact(Singulink.Numerics.BigDecimal value) {
            if (value <= Int32.MaxValue && value >= Int32.MinValue
                                        && Remainder(value, BigDecimal.One).IsZero) {
                return (int) value;
            }
            else {
                throw new ArithmeticException("BigDecimal outwith range of Int32");
            }
        }
        
        public static long LongValueExact(Singulink.Numerics.BigDecimal value) {
            if (value <= Int64.MaxValue && value >= Int64.MinValue 
                                        && Remainder(value, BigDecimal.One).IsZero) {
                return (long) value;
            }
            else {
                throw new ArithmeticException("BigDecimal outwith range of Int32");
            }
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited
