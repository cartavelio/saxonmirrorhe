using System;

namespace com.saxonica.glue {
    public class Utils {
        public static int unsignedRightShift(int i, int shift) {
            return (int) ((uint) i >> shift);
        }
        
        //Java str.getChars(begin, end, array, dest) => C# str.CopyTo(begin, array, dest, count)
        public static void stringToCharArray(string str, int begin, int end, char[] buff, int dest) {
            str.CopyTo(begin, buff, dest, end - begin);
        }

        public static bool stringEqualsIgnoreCase(string a, string b) {
            return String.Compare(a, b, true) == 0;
        } 
    }
}