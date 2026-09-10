using System;
using System.Globalization;
using System.Text;
using System.Threading;
using Saxon.Ejava.text;
using Saxon.Ejava.util;
using Saxon.Hej;
using Saxon.Hej.expr.sort;
using Saxon.Hej.functions;
using Saxon.Hej.lib;
using Saxon.Hej.str;
using Saxon.Hej.trans;
using Saxon.Hej.type;
using Saxon.Hej.value;
using Saxon.Impl.Overrides;
using StringBuilder = Saxon.Ejava.lang.StringBuilder;

namespace Saxon.Impl.Helpers
{

    public class UcaCollatorUsingCSharp : SubstringMatcher {
        private string uri;
        private Properties properties;
        private Saxon.Hej.Configuration config;
        private System.StringComparer stringComparer;
        private bool ignoreCase = false;
        private System.Globalization.CultureInfo cultureInfo = CultureInfo.InvariantCulture;

        private readonly static string[] keywords = {
            "fallback", "lang", "version", "strength",
            "alternate", "backwards", "normalization", "maxVariable",
            "caseLevel", "caseFirst", "numeric", "reorder"
        };

        private static System.Collections.Generic.HashSet<string> keys = new (Arrays.asList(keywords));

        public UcaCollatorUsingCSharp(string urim, Configuration config)
        {
            this.config = config;
            stringComparer = System.StringComparer.CurrentCultureIgnoreCase; //new RuleBasedCollator();
            setProps(parseProps(urim));
        }

        private void setProps(Properties props)
        {
            this.properties = props;
            bool fallbackError = false;
            CompareOptions compareOptions = CompareOptions.None;
            string fallback = props.getProperty("fallback");
            if (fallback != null)
            {
                switch (fallback)
                {
                    case "yes":
                        break;
                    case "no":
                        if (config.getEditionCode().Equals("HE"))
                        {
                            error("fallback=no is not supported in Saxon-HE");
#if EE
                    } else if (!ICULibrary.isICUCollatorAvailable()) {
                        error("fallback=no is not available because ICU4N is not available");
#endif
                        }
                        else
                        {
                            error("fallback=no is not available except when ICU4N is in use");
                        }

                        break;
                    default:
                        error("fallback", "yes|no");
                        break;
                }
            }

            string lang = props.getProperty("lang");
            if (!string.IsNullOrEmpty(lang)) {
                ValidationFailure vf = Saxon.Hej.type.StringConverter.StringToLanguage.INSTANCE.validate(Saxon.Hej.str.StringView.tidy(lang));
                if (vf != null) {
                    error("lang", "a valid language code");
                }

                string language, country = "", variant = "";
                string[] parts = lang.Split("-");
                language = parts[0];
                if (parts.Length > 1) {
                    country = parts[1];
                }

                if (parts.Length > 2) {
                    variant = parts[2];
                }

                //TODO Locale loc = new Locale(language, country, variant);
                this.cultureInfo = makeLocale(language, country, variant);

                //TODO create RuleBaseCollator uca = (RuleBasedCollator) Collator.getInstance(loc);
            }

            string strengthAtt = props.getProperty("strength");
            if (strengthAtt != null && compareOptions == CompareOptions.None)
            {
                compareOptions = strengthAtt switch {
                    "primary" => System.Globalization.CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace |
                                 CompareOptions.IgnoreWidth,
                    "secondary" => CompareOptions.IgnoreCase | CompareOptions.IgnoreWidth,
                    "tertiary" => CompareOptions.IgnoreWidth,
                    "identical" => 0,
                    _ => 0 // throw new XPathException("strength must be primary, secondary, tertiary, or identical")
                };
                this.ignoreCase = strengthAtt switch {
                    "primary" => true,
                    "secondary" => true,
                    "tertiary" => false,
                    "identical" => false,
                    _ => false
                };
            }

            // Look for the properties ignore-case, ignore-modifiers, ignore-width

            string ignore = props.getProperty("ignore-case");
            if (ignore != null)
            {
                if (ignore == "yes")
                {
                    compareOptions |= CompareOptions.IgnoreCase;
                }
                else if (ignore == "no")
                {
                    // no-op
                }
                else
                {
                    throw new XPathException("ignore-case must be yes or no");
                }
            }

            ignore = props.getProperty("ignore-modifiers");
            if (ignore != null)
            {
                if (ignore == "yes")
                {
                    compareOptions |= CompareOptions.IgnoreNonSpace;
                }
                else if (ignore == "no")
                {
                    // no-op
                }
                else
                {
                    throw new XPathException("ignore-modifiers must be yes or no");
                }
            }

            ignore = props.getProperty("ignore-symbols");
            if (ignore != null)
            {
                if (ignore == "yes")
                {
                    compareOptions |= CompareOptions.IgnoreSymbols;
                }
                else if (ignore == "no")
                {
                    // no-op
                }
                else
                {
                    throw new XPathException("ignore-symbols must be yes or no");
                }
            }

            ignore = props.getProperty("ignore-width");
            if (ignore != null)
            {
                if (ignore == "yes")
                {
                    compareOptions |= CompareOptions.IgnoreWidth;
                }
                else if (ignore == "no")
                {
                    // no-op
                }
                else
                {
                    throw new XPathException("ignore-width must be yes or no");
                }
            }

            stringComparer = System.StringComparer.Create(cultureInfo, compareOptions);

            string normalization = props.getProperty("normalization");
            if (normalization != null)
            {
                if (normalization.Equals("yes"))
                {
                    //TODO uca.setDecomposition(Collator.CANONICAL_DECOMPOSITION);
                }
                else if (normalization.Equals("no"))
                {
                    //TODO uca.setDecomposition(Collator.NO_DECOMPOSITION);
                }
            }
        }

        private static System.Globalization.CultureInfo makeLocale(string language, string country, string variant) {
            StringBuilder sb = new (language);
            if (!string.IsNullOrEmpty(country)) {
                sb.append("-").append(country);
            }
            if (!string.IsNullOrEmpty(variant)) {
                sb.append("-").append(variant);
            }
            return new System.Globalization.CultureInfo(sb.ToString());
        }


        private Properties parseProps(string uri) {
            System.Uri uuri;
            try {
                uuri = new System.Uri(uri, System.UriKind.RelativeOrAbsolute);
            }
            catch (System.UriFormatException err) {
                throw new XPathException(err);
            }

            System.Collections.Generic.List<string> unknownKeys = new System.Collections.Generic.List<string>();
            Properties props = new ();
            string query = AnyURIValue.decode(uuri.Query);
            if (!string.IsNullOrEmpty(query)) {
                if (query.StartsWith("?")) {
                    query = query.Substring(1);
                }

                foreach (string s in System.Text.RegularExpressions.Regex.Split(query, ";")) {
                    string[] tokens = System.Text.RegularExpressions.Regex.Split(s, "=");
                    if (!(keys.Contains(tokens[0]))) {
                        unknownKeys.Add(tokens[0]);
                    }

                    string value = ((tokens.Length >= 2) ? tokens[1] : "");
                    props.setProperty(tokens[0], value);
                }
            }

            string fallback = props.getProperty("fallback");
            if ((((fallback != null) && fallback.Equals("no")) && unknownKeys.Count != 0))
            {
                StringBuilder message =
                    new StringBuilder(((unknownKeys.Count > 1) ? "unknown parameters:" : "unknown parameter:"));
                foreach (string u in unknownKeys) {
                    message.append(u).append(" ");
                }

                error(message.ToString());
            }

            return props;
        }

        public Properties getProperties() {
            return properties;
        }

        public string getCollationURI() {
            return uri;
        }

        public int compareStrings(UnicodeString s1, UnicodeString s2) {
            return stringComparer.Compare(s1.ToString(), s2.ToString());
        }


        public bool comparesEqual(UnicodeString s1, UnicodeString s2) {
            return stringComparer.Compare(s1.ToString(), s2.ToString()) == 0;
        }

        public bool isEqualToEmpty(UnicodeString s1) {
            return s1.isEmpty();
        }

        public AtomicMatchKey getCollationKey(UnicodeString s) {
            byte[] key = cultureInfo.CompareInfo.GetSortKey(s.ToString(), 
                ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None).KeyData;
            return new Base64BinaryValue(key);
        }

        private void error(string message) {
            throw new XPathException("Error in UCA Collation URI " + uri + ": " + message, "FOCH0002");
        }

        private void error(string field, string allowed) {
            error("value of " + field + " must be " + allowed);
        }

        private void error(string field, string allowed, string requested) {
            error("value of " + field + " must be " + allowed + ", requested was:" + requested);
        }

        public bool contains(UnicodeString s1, UnicodeString s2) {
            CultureInfo saved = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            bool result = s1.ToString().Contains(
                s2.ToString(), 
                this.ignoreCase ?  StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
            Thread.CurrentThread.CurrentCulture = saved;
            return result;
        }

        public bool startsWith(UnicodeString s1, UnicodeString s2) {
            CultureInfo saved = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            bool result = s1.ToString().StartsWith(
                s2.ToString(), 
                this.ignoreCase ?  StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
            Thread.CurrentThread.CurrentCulture = saved;
            return result;
        }

        public bool endsWith(UnicodeString s1, UnicodeString s2) {
            CultureInfo saved = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            bool result = s1.ToString().EndsWith(
                s2.ToString(), 
                this.ignoreCase ?  StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
            Thread.CurrentThread.CurrentCulture = saved;
            return result;
        }

        public UnicodeString substringBefore(UnicodeString s1, UnicodeString s2) {
            string g1 = s1.ToString();
            string g2 = s2.ToString();
            int bb = IndexOf(g1, g2);
            if (bb >= 0) {
                return s1.substring(0, bb);
            } 
            return EmptyUnicodeString.getInstance();
        }

        public UnicodeString substringAfter(UnicodeString s1, UnicodeString s2) {
            string g1 = s1.ToString();
            string g2 = s2.ToString();
            int bb = IndexOf(g1, g2);
            if (bb >= 0) {
                return s1.substring(bb + g2.Length);
            } 
            return EmptyUnicodeString.getInstance();
        }


        private int IndexOf(string s1, string s2) {
            CultureInfo saved = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            int result = s1.ToString().IndexOf(
                s2.ToString(), 
                this.ignoreCase ?  StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
            Thread.CurrentThread.CurrentCulture = saved;
            return result;
        }
    }
}