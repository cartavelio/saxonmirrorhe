using System;
using com.saxonica.expr.sort;
using net.sf.saxon;
using net.sf.saxon.lib;

namespace Saxon.Impl.Overrides {
    // Saxon-CS override for com.saxonica.config.ICULibrary
    public class ICULibrary {
        
        public static bool isICUCollatorAvailable() {
            return true;
        }
        
        public static bool hasNumberer() {
            return false;
        }

        public static StringCollator makeICUCollator(string uri, Configuration config) {
            return new UcaCollatorUsingIcu(uri);
        }

        public static String getLanguageTag(java.util.Locale locale) {
            string rep = locale.ToString();
            int firstUnderscore = rep.IndexOf('_');
            if (firstUnderscore < 0) {
                return rep;
            }

            int secondUnderscore = rep.IndexOf('_', firstUnderscore + 1);
            if (secondUnderscore < 0) {
                return rep.Replace('_', '-');
            }

            return rep.Substring(0, secondUnderscore).Replace('_', '-');
        }
    }
}