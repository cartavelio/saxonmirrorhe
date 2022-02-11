using Sys = System;
namespace java.lang {
    public class Character {
        public static bool isLetterOrDigit(Sys.Char c) {
            return Sys.Char.IsLetterOrDigit(c);
        }
        public static bool isLetter(Sys.Char c) {
            return Sys.Char.IsLetter(c);
        }
        
        public static bool isDigit(Sys.Char c) {
            return Sys.Char.IsDigit(c);
        }
    }
}