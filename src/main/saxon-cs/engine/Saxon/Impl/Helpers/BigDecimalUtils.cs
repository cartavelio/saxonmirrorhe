using System;
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
            return a%b;
        }
        
        public static Singulink.Numerics.BigDecimal Idiv(Singulink.Numerics.BigDecimal a, Singulink.Numerics.BigDecimal b) {
            Singulink.Numerics.BigDecimal quot = a / b;
            return quot.Sign < 0 ? Singulink.Numerics.BigDecimal.Ceiling(quot) : Singulink.Numerics.BigDecimal.Floor(quot);
        }
        
        public static Singulink.Numerics.BigDecimal Round(Singulink.Numerics.BigDecimal a, int scale) {
            if (scale < 0) {
                BigDecimal factor = BigDecimal.Pow10(scale);
                return Round(a * factor, 0) / factor;
            }
            switch (a.Sign) {
                case -1:
                    // MidpointRoundingMode.ToPositiveInfinity and .ToZero are not supported,
                    // so this requires some contortion
                    return BigDecimal.Round(a, scale, MidpointRounding.AwayFromZero);
                case 0:
                    return a;
                default:
                    return BigDecimal.Round(a, scale, MidpointRounding.AwayFromZero);
            }
        }
    }
}
