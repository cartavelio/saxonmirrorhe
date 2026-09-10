using System;
using java.lang;

namespace java.math {
    public class MathContext {
        private const int DEFAULT_DIGITS = 9;
        private readonly RoundingMode DEFAULT_ROUNDINGMODE = RoundingMode.HALF_UP;
        private const int MIN_DIGITS = 0;


        public static readonly MathContext UNLIMITED =
            new MathContext(0, RoundingMode.HALF_UP);

        public static readonly MathContext DECIMAL32 =
            new MathContext(7, RoundingMode.HALF_EVEN);

        public static readonly MathContext DECIMAL64 =
            new MathContext(16, RoundingMode.HALF_EVEN);

        public static readonly MathContext DECIMAL128 =
            new MathContext(34, RoundingMode.HALF_EVEN);

        public int precision;


        public RoundingMode roundingMode;

        public MathContext(int setPrecision) {
            this(setPrecision, DEFAULT_ROUNDINGMODE);
            return;
        }

        public MathContext(int setPrecision,
            RoundingMode setRoundingMode) {
            if (setPrecision < MIN_DIGITS)
                throw new IllegalArgumentException("Digits < 0");
            if (setRoundingMode == null)
                throw new NullReferenceException("null RoundingMode");

            precision = setPrecision;
            roundingMode = setRoundingMode;
            return;
        }

        public MathContext(string val) {
            bool bad = false;
            int setPrecision;
            if (val == null)
                throw new NullReferenceException("null String");
            try {
                // any error here is a string format problem
                if (!val.StartsWith("precision=")) throw new RuntimeException();
                int fence = val.IndexOf(' '); // could be -1
                int off = 10; // where value starts
                setPrecision = Integer.parseInt(val.Substring(10, fence));

                if (!val.StartsWith("roundingMode=", fence + 1))
                    throw new RuntimeException();
                off = fence + 1 + 13;
                string str = val.Substring(off, val.length());
                roundingMode = RoundingMode.valueOf(str);
            }
            catch (RuntimeException re) {
                throw new IllegalArgumentException("bad string format");
            }

            if (setPrecision < MIN_DIGITS)
                throw new IllegalArgumentException("Digits < 0");
            // the other parameters cannot be invalid if we got here
            precision = setPrecision;
        }

        public int getPrecision() {
            return precision;
        }


        public RoundingMode getRoundingMode() {
            return roundingMode;
        }
        
        public bool equals(object x) {
            MathContext mc;
            if (!(x is MathContext))
                return false;
            mc = (MathContext) x;
            return mc.precision == this.precision
                   && mc.roundingMode == this.roundingMode; // no need for .equals()
        }

        public int hashCode() {
            return this.precision + roundingMode.GetHashCode() * 59;
        }


        public string ToString() {
            return "precision=" + precision + " " +
                   "roundingMode=" + roundingMode.ToString();
        }
        
    }


}