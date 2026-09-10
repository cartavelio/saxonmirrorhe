using System;
using java.lang;
using Math = System.Math;

namespace Saxon.Impl.Helpers {
    public class Utils {
        public static int unsignedRightShift(int i, int shift) {
            return (int) ((uint) i >> shift);
        }
        public static long unsignedRightShift(long i, int shift) {
            return (long) ((ulong) i >> shift);
        }

        public static string intToHexString(int i) {
            return i.ToString("X");
        }
        
        public static string longToHexString(long i) {
            return i.ToString("X");
        }
        
        // parse an integer literal whose radix is not known statically
        public static int ParseInt(string input, int radix) {
            if (input.StartsWith("0x")) {
                throw new NumberFormatException("Leading 0x not allowed in number");
            }
            if (radix == 2 || radix == 8 || radix == 10 || radix == 16) {
                return System.Convert.ToInt32(input, radix);
            }

            throw new NotSupportedException("Unsupported number radix " + radix);
        }
        
 
        // Test "instanceof" a generic type. For example,
        // IsInstanceOfGenericType(new List<string>(), typeof(List<>))
        // returns true
        static bool IsInstanceOfGenericType(object instance, Type genericType)
        {
            Type type = instance.GetType();
            while (type != null)
            {
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == genericType)
                {
                    return true;
                }
                type = type.BaseType;
            }
            return false;
        }

        public static double roundDouble(double value, int scale) {
            if (System.Double.IsNaN(value) || System.Double.IsInfinity(value) || value == 0.0) {
                return value;
            }
            if (scale == 0 && value < System.Int64.MaxValue) {
                if (value > 0.0) {
                    return System.Math.Round(value, System.MidpointRounding.AwayFromZero);
                } else if (value > -0.5) {
                    return -0.0;
                } else if (value > -100_000_000) {
                    // add a sufficiently large displacement, then round away from zero, then subtract the displacement
                    return System.Math.Round(value + 100_000_001, System.MidpointRounding.AwayFromZero) - 100_000_001;
                }
            }
            // In other cases, do it by hand...   
            double factor = System.Math.Pow(10, (scale+1));
            double d = System.Math.Abs((value*factor));
            if (System.Double.IsInfinity(d)) {
                Singulink.Numerics.BigDecimal dec = Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(value);
                return (double)dec;
            }
            double rem = (d%10);
            if ((rem>=5)) {
                d += (10-rem);
            } else {
                if ((rem<5)) {
                    d -= rem;
                }
            }
            d /= factor;
            if ((value<0)) {
                d = -(d);
            }
            return d;
        }

        public static float roundFloat(float value, int scale) {
            return (float) roundDouble(value, scale);
        }



    }
}