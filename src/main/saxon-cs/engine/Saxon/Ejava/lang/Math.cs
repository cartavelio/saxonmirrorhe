using System;
using SysMath = System.Math;
namespace Saxon.Ejava.lang {
    public class Math {
        public static int max(int x, int y) {
            return x > y ? x : y;
        }
        
        public static long max(long x, long y) {
            return x > y ? x : y;
        }
        
        public static double max(double x, double y) {
            return x > y ? x : y;
        }

        public static int min(int x, int y) {
            return x < y ? x : y;
        }
        
        public static long min(long x, long y) {
            return x < y ? x : y;
        }
        
        public static double min(double x, double y) {
            return x < y ? x : y;
        }

        public const double PI = SysMath.PI; 
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
        public static long round(double x) {
            return (long)SysMath.Round(x);
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
            return SysMath.Acos(x);
        }
        public static double atan(double x) {
            return SysMath.Atan(x);
        }
        public static double log(double x) {
            return SysMath.Log(x);
        }
        public static double log10(double x) {
            return SysMath.Log10(x);
        }
        public static double exp(double x) {
            return SysMath.Exp(x);
        }
        public static double sqrt(double x) {
            return SysMath.Sqrt(x);
        }
        public static double pow(double x, double y) {
            return SysMath.Pow(x, y);
        }

        public static long addExact(long a, long b) {
            try {
                return checked(a + b);
            }
            catch (OverflowException err) {
                throw new ArithmeticException(err.Message);
            }
        }
        
        public static int addExact(int a, int b) {
            try {
                return checked(a + b);
            }
            catch (OverflowException err) {
                throw new ArithmeticException(err.Message);
            }
        }
        
        public static long multiplyExact(long a, long b) {
            try {
                return checked(a * b);
            }
            catch (OverflowException err) {
                throw new ArithmeticException(err.Message);
            }
        }
        
        public static int multiplyExact(int a, int b) {
            try {
                return checked(a * b);
            }
            catch (OverflowException err) {
                throw new ArithmeticException(err.Message);
            }
        }


    }
}