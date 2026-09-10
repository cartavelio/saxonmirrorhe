using System;
#if EE
using Saxon.Eej.expr.sort;
#endif
using Saxon.Hej;
using Saxon.Hej.lib;

namespace Saxon.Impl.Overrides {
    // SaxonCS override for com.saxonica.config.ICULibrary
    public class ICULibrary {
        
        public static bool isICUCollatorAvailable() {
            return true;
        }
        
        public static bool hasNumberer() {
            return false;
        }

        public static StringCollator makeICUCollator(string uri, Configuration config) {
#if EE                
                return new UcaCollatorUsingIcu(uri);
#else
            throw new NotSupportedException("Not supported in HE");
#endif            
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

// Copyright (c) 2021 - 2023 Saxonica Limited