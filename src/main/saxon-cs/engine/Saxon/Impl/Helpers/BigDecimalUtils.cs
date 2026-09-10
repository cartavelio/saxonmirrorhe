using System;
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
