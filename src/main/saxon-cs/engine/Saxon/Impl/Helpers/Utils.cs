using System;
using System.Globalization;
using Saxon.Ejava.lang;
using Math = System.Math;

namespace Saxon.Impl.Helpers {
    public class Utils {
        public static int unsignedRightShift(int i, int shift) {
            return (int) ((uint) i >> shift);
        }
        
        public static int RotateRight(int i, int distance) {
            return unsignedRightShift(i, distance) | (i << -distance);
        }
        
        public static long unsignedRightShift(long i, int shift) {
            return (long) ((ulong) i >> shift);
        }

        public static string intToHexString(int i) {
            return i.ToString("x");
        }
        
        public static string longToHexString(long i) {
            return i.ToString("x");
        }
        
        // parse an integer literal in base 10
        public static int ParseInt(string input) {
            if (input.StartsWith("0x")) {
                throw new NumberFormatException("Leading 0x not allowed in number");
            }

            int number;
            bool success = Int32.TryParse(input, out number);
            if (success) {
                return number;
            }
            else {
                throw new NumberFormatException("Invalid integer '" + input + "'");
            }
        }
        
        // parse an integer literal whose radix is not known statically
        public static int ParseInt(string input, int radix) {
            if (input.StartsWith("0x")) {
                throw new NumberFormatException("Leading 0x not allowed in number");
            }
            if (radix == 2 || radix == 8 || radix == 10 || radix == 16) {
                try {
                    return Convert.ToInt32(input, radix);
                }
                catch (System.Exception e) when (e is FormatException or ArgumentException or OverflowException) {
                    throw new NumberFormatException(e.Message);
                }
            }

            throw new NotSupportedException("Unsupported number radix " + radix);
        }


        public static double roundDouble(double value, int scale) {
            if (System.Double.IsNaN(value) || System.Double.IsInfinity(value) || value == 0.0) {
                return value;
            }
            if (scale == 0 && value < System.Int64.MaxValue) {
                if (value > 0.0) {
                    return System.Math.Round(value, System.MidpointRounding.AwayFromZero);
                } else if (value >= -0.5) {
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

        /// <summary>
        /// Convert an array of 8 bytes containing an IEEE representation of a double, to a double
        /// </summary>
        /// <param name="bytes">An array of at least 8 bytes</param>
        /// <param name="start">Offset of the first of the 8 bytes to be considered</param>
        /// <param name="bigendian">true if the most significant byte comes first</param>
        /// <returns></returns>
        public static double bytesToDouble(byte[] bytes, int start, bool bigendian) {
            if (bigendian == BitConverter.IsLittleEndian) {
                byte[] copy = new byte[8];
                System.Array.Copy(bytes, start, copy, 0, 8);
                // that is, if the actual endianness is different from what the BitConverter expects
                Array.Reverse(copy);
                return BitConverter.ToDouble(copy);
            }
            else {
                return BitConverter.ToDouble(bytes, start);
            }
        }
        
        /// <summary>
        /// Convert an array of 4 bytes containing an IEEE representation of a float, to a float
        /// </summary>
        /// <param name="bytes">An array of at least 4 bytes</param>
        /// <param name="start">Offset of the first of the 4 bytes to be considered</param>
        /// <param name="bigendian">true if the most significant byte comes first</param>
        /// <returns></returns>
        public static float bytesToFloat(byte[] bytes, int start, bool bigendian) {
            if (bigendian == BitConverter.IsLittleEndian) {
                byte[] copy = new byte[4];
                System.Array.Copy(bytes, start, copy, 0, 4);
                // that is, if the actual endianness is different from what the BitConverter expects
                Array.Reverse(copy);
                return BitConverter.ToSingle(copy);
            }
            else {
                return BitConverter.ToSingle(bytes, start);
            }
        }
        
        /// <summary>
        /// Parse a string as a double - emulation of java.lang.Double.parseDouble()
        /// </summary>

        public static double ParseDouble(string str) {
            try {
                return System.Double.Parse(str, CultureInfo.InvariantCulture);
            }
            catch (FormatException e) {
                throw new NumberFormatException(e.Message);
            }
        }

    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited