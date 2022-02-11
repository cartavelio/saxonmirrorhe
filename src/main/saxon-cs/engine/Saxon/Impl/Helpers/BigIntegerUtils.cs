using System;
using System.Numerics;
using BigInt = System.Numerics.BigInteger;

namespace Saxon.Impl.Helpers {
    public class BigIntegerUtils {

        public static readonly BigInt ZERO = ValueOf(0);
        public static BigInt ValueOf(long val) {
            return (BigInt) val;
        }

        public static BigInt ValueOf(string s) {
            // TODO: should be able to use Parse or TryParse, but I'm not finding these methods...
            throw new NotImplementedException();
        }

        public static BigInt[] DivideAndRemainder(BigInt numerator, BigInt denominator) {
            throw new NotImplementedException();
        }
        
        public static BigInt Remainder(BigInt numerator, BigInt denominator) {
            throw new NotImplementedException();
        }
        
        public static BigInt Mod(BigInt numerator, BigInt denominator) {
            throw new NotImplementedException();
        }

        public static int Signum(BigInt value) {
            if (value > ZERO) {
                return +1;
            } else if (value < ZERO) {
                return -1;
            }
            else {
                return 0;
            }
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
    }
}