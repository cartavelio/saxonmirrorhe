using System;
using net.sf.saxon.functions;
using SysMath = System.Math;
namespace java.lang {
    public class Math {
        public static int max(int x, int y) {
            return x > y ? x : y;
        }

        public static int min(int x, int y) {
            return x < y ? x : y;
        }

        public static long abs(long x) {
            return x >= 0 ? x : -x;
        }

        public static int abs(int x) {
            return x >= 0 ? x : -x;
        }

        public static double abs(double x) {
            return x >= 0 ? x : -x;
        }
        
        public static float abs(float x) {
            return x >= 0 ? x : -x;
        }

        public static double floor(double x) {
            return SysMath.Floor(x);
        }
        
        public static double ceil(double x) {
            return SysMath.Ceiling(x);
        }
        public static double round(double x) {
            return SysMath.Round(x);
        }

        public static double sin(double x) {
            return SysMath.Sin(x);
        }
        
        public static double cos(double x) {
            return SysMath.Cos(x);
        }
        public static double tan(double x) {
            return SysMath.Tan(x);
        }
        public static double asin(double x) {
            return SysMath.Asin(x);
        }
        public static double acos(double x) {
            return SysMath.Atan(x);
        }
    }
}