using System;
using System.Numerics;
using java.math;
using Singulink.Numerics;

namespace Saxon.Impl.Helpers {
    public class BigDecimalUtils {
        public static Singulink.Numerics.BigDecimal ValueOf(long val) {
            return (Singulink.Numerics.BigDecimal)val;
        }

        public static Singulink.Numerics.BigDecimal ValueOf(double val) {
            return Singulink.Numerics.BigDecimal.FromDouble(val, true);
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
        
        public static Singulink.Numerics.BigDecimal Idiv(Singulink.Numerics.BigDecimal a, Singulink.Numerics.BigDecimal b) {
            Singulink.Numerics.BigDecimal quot = a / b;
            return quot.Sign < 0 ? Singulink.Numerics.BigDecimal.Ceiling(quot) : Singulink.Numerics.BigDecimal.Floor(quot);
        }
        
        public static Singulink.Numerics.BigDecimal Round(Singulink.Numerics.BigDecimal a, int scale) {
            // if (scale < 0) {
            //     BigDecimal factor = BigDecimal.Pow10(scale);
            //     return Round(a * factor, 0) / factor;
            // }
            switch (a.Sign) {
                case -1:
                    // MidpointRoundingMode.ToPositiveInfinity does not do what the name suggests;
                    // it is a "directed rounding" mode that also affects non-midpoint values
                    // Choose a positive shift that is (a) big enough to make the value positive, and 
                    // (b) a multiple of 10^N where N is ge -scale. Then add the shift, perform the
                    // rounding away from zero, then subtract the shift.
                    BigDecimal shift = -BigDecimal.Round(a) + (scale > 0 ? 100 : BigInteger.Pow(10, -scale));
                    return BigDecimal.Round(a + shift, scale, MidpointRounding.AwayFromZero) - shift;
                case 0:
                    return a;
                default:
                    return BigDecimal.Round(a, scale, MidpointRounding.AwayFromZero);
            }
        }
    }
}
