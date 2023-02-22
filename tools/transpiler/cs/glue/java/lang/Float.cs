using SysFloat = System.Single;
namespace java.lang {
    public class Float {
        public const float NaN = SysFloat.NaN;
        public static bool isNaN(float x) {
            return float.IsNaN(x);
        }
        public static bool isInfinite(float x) {
            return float.IsInfinity(x);
        }

        public static int compare(float x, float y) {
            return x.CompareTo(y);
        }

        public static int GetHashCode(float x) {
            return x.GetHashCode();
        }
    }
}