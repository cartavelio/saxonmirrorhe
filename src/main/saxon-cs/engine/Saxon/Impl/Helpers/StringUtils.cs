using System;

namespace Saxon.Impl.Helpers {
    public class StringUtils {

        public static readonly System.Text.Encoding LATIN_1 = System.Text.Encoding.GetEncoding("iso-8859-1");
     
        //Java str.getChars(begin, end, array, dest) => C# str.CopyTo(begin, array, dest, count)
        public static void stringToCharArray(string str, int begin, int end, char[] buff, int dest) {
            str.CopyTo(begin, buff, dest, end - begin);
        }

        public static bool stringEqualsIgnoreCase(string a, string b) {
            return String.Compare(a, b, true) == 0;
        }
        
 

    }
}