using java.math;

namespace com.saxonica.glue {
    public class BigDecimalUtils {
        public static BigDecimal ValueOf(long val) {
            return new BigDecimal(val);
        }

        public static BigDecimal ValueOf(double val) {
            return ValueOf(val.ToString());
        }

        public static BigDecimal ValueOf(string s) {
            return BigDecimal.Parse(s);
        }

        public static int Signum(BigDecimal val) {
            return val.compareTo(BigDecimal.Zero);
        }
    }
}
}