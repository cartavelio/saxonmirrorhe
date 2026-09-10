using System;
using System.Globalization;
using System.Numerics;
using Saxon.Ejava.lang;
using BigInt = System.Numerics.BigInteger;

namespace Saxon.Impl.Helpers {
    public class BigIntegerUtils {

        public static readonly BigInt ZERO = ValueOf(0);
        public static BigInt ValueOf(long val) {
            return val;
        }

        public static BigInt ValueOf(string s) {
            try {
                return BigInteger.Parse(s);
            }
            catch (FormatException e) {
                throw new NumberFormatException("Invalid integer " + s);
            }
        }
        
        public static BigInt ValueOf(string s, int radix) {
            try {
                if (radix == 10) {
                    return ValueOf(s);
                }
                if (radix == 16) {
                    return BigInteger.Parse(s, NumberStyles.HexNumber);
                }

                BigInt result = 0;
                s = s.ToLower();
                for (int i = 0; i < s.Length; i++) {
                    int digit = "0123456789abcdefghijklmnopqrstuvwxyz".IndexOf(s[i]);
                    if (digit < 0 || digit >= radix) {
                        throw new NumberFormatException("Invalid digit '" + s[i] + "'");
                    }

                    result = result * radix + digit;
                }

                return result;
            }
            catch (FormatException e) {
                throw new NumberFormatException("Invalid integer " + s);
            }
        }

        public static BigInt[] DivideAndRemainder(BigInt numerator, BigInt denominator) {
            BigInt quotient = BigInteger.DivRem(numerator, denominator, out var remainder);
            return new BigInt[] {quotient, remainder};

        }
        
        public static BigInt Remainder(BigInt numerator, BigInt denominator) {
            return numerator % denominator;
        }
        
        public static BigInt Mod(BigInt numerator, BigInt denominator) {
            return numerator % denominator;
        }

        public static int Signum(BigInt value) {
            return value.Sign;
        }

        public static long LongValueExact(BigInt value) {
            if (value <= Int64.MaxValue && value >= Int64.MinValue) {
                return (long) value;
            }
            else {
                throw new ArithmeticException("BigInteger outwith range of Int64");
            }
        }
        
        public static long IntValueExact(BigInt value) {
            if (value <= Int32.MaxValue && value >= Int32.MinValue) {
                return (int) value;
            }
            else {
                throw new ArithmeticException("BigInteger outwith range of Int32");
            }
        }

        public static BigInteger FromBytes(byte[] bytes, int offset, int len, bool unsigned, bool bigEndian) {
            byte[] copy = new byte[len];
            Array.Copy(bytes, offset, copy, 0, len);
            return new System.Numerics.BigInteger(copy, unsigned, bigEndian);
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited