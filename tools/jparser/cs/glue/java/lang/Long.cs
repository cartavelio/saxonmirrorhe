using SysInt64 = System.Int64;
namespace java.lang {
    public class Long {
        public const long MIN_VALUE = SysInt64.MinValue;
        public const long MAX_VALUE = SysInt64.MaxValue;
        
        public static string ToString(long num) {
            return num.ToString();
        }

        public static long parseLong(string str) {
            return SysInt64.Parse(str); // TODO: use TryParse to avoid exceptions?
        }

        public static int compare(long x, long y) {
            return x.CompareTo(y);
        }

        public static int signum(long x) {
            return compare(x, 0);
        }
    }
}