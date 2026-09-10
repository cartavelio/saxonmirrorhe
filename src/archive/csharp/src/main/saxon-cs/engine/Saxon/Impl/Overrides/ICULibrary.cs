using System;
using Saxon.Eej.expr.sort;
using Saxon.Hej;
using Saxon.Hej.lib;

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

        public static String getLanguageTag(Saxon.Ejava.util.Locale locale) {
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

// Copyright (c) 2021 Saxonica Limited