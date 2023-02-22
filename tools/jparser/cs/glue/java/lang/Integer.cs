using System;
using SysInt32 = System.Int32;
namespace java.lang {
    public static class Integer {
        public const int MAX_VALUE = SysInt32.MaxValue;
        public const int MIN_VALUE = SysInt32.MinValue;
        public static string ToString(int num) {
            return num.ToString();
        }

        public static string toHexString(int num) {
            return num.ToString("X");
        }

        public static int min(int x, int y) {
            return x < y ? x : y;
        }
        public static int max(int x, int y) {
            return x > y ? x : y;
        }

        public static int compare(int x, int y) {
            return x.CompareTo(y);
        }

        public static int signum(int x) {
            return x == 0 ? 0 : x < 0 ? -1 : 1;
        }
        
        public static int parseInt(string str) {
            return SysInt32.Parse(str); // TODO: use TryParse to avoid exceptions?
        }
        
        public static int parseInt(string str, int radix) {
            throw new NotImplementedException();
        }

        public static int valueOf(int v) {
            return v;
        }
    }
}