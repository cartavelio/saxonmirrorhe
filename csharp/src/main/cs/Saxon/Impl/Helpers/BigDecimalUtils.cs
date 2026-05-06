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
    
        public static Singulink.Numerics.BigDecimal ExactValueOf(double val) {
            return Singulink.Numerics.BigDecimal.FromDouble(val, Singulink.Numerics.FloatConversion.Exact);
        }
        
        public static Singulink.Numerics.BigDecimal StripTrailingZeros(Singulink.Numerics.BigDecimal val) {
            return val;
        }
        
        public static Singulink.Numerics.BigDecimal Remainder(Singulink.Numerics.BigDecimal a, Singulink.Numerics.BigDecimal b) {
            // The remainder is given by this.subtract(this.divideToIntegralValue(divisor).multiply(divisor)).
            // Note that this is not the modulo operation (the result can be negative).
            return a - Idiv(a, b)*b;
        }
        
        public static Singulink.Numerics.BigDecimal Idiv(Singulink.Numerics.BigDecimal a, Singulink.Numerics.BigDecimal b) {
            Singulink.Numerics.BigDecimal quot = a / b;
            return quot.Sign < 0 ? Singulink.Numerics.BigDecimal.Ceiling(quot) : Singulink.Numerics.BigDecimal.Floor(quot);
        }
        
        public static Singulink.Numerics.BigDecimal Round(Singulink.Numerics.BigDecimal a, int scale) {
            // if (scale < 0) {
            //     BigDecimal factor = BigDecimal.Pow10(scale);
            //     return Round(a * factor, 0) / factor;
            // }
            return a.Sign switch {
                // case -1:
                //     // MidpointRoundingMode.ToPositiveInfinity does not do what the name suggests;
                //     // it is a "directed rounding" mode that also affects non-midpoint values
                //     // Choose a positive shift that is (a) big enough to make the value positive, and 
                //     // (b) a multiple of 10^N where N is ge -scale. Then add the shift, perform the
                //     // rounding away from zero, and finally subtract the shift.
                //     int digits = BigDecimal.Round(a).Precision;
                //     BigDecimal shift = BigInteger.Pow(10, digits);
                //     return BigDecimal.Round(a + shift, scale, Singulink.Numerics.RoundingMode.MidpointToPositiveInfinity) - shift;
                0 => a,
                _ => BigDecimal.Round(a, scale, Singulink.Numerics.RoundingMode.MidpointToPositiveInfinity)
            };
        }
        
        public static Singulink.Numerics.BigDecimal Round(Singulink.Numerics.BigDecimal a, int scale,
            Saxon.Hej.functions.Round.RoundingRule roundingRule) {
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
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited
