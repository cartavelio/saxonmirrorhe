using SysDouble = System.Double;
namespace java.lang {
    public class Double {
        public const double NaN = SysDouble.NaN;
        public static bool isNaN(double x) {
            return double.IsNaN(x);
        }
        public static bool isInfinite(double x) {
            return double.IsInfinity(x);
        }

        public static int compare(double x, double y) {
            return x.CompareTo(y);
        }

        public static int GetHashCode(double x) {
            return x.GetHashCode();
        }

        public static long doubleToLongBits(double x) {
            return System.BitConverter.DoubleToInt64Bits(x);
        }

        public static double valueOf(double d) {
            return d;
        }

        public static double parseDouble(string input) {
            return SysDouble.Parse(input);
        }
    }
}