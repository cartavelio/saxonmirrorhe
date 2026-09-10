using System.Globalization;
using System.Text;
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

    public class UcaCollatorUsingCSharp : SubstringMatcher
    {
        private string uri;

        private Properties properties;

        private Saxon.Hej.Configuration config;

        private System.StringComparer uca;

        private int strength;

        private static string[] keywords =
        {
            "fallback", "lang", "version", "strength",
            "alternate", "backwards", "normalization", "maxVariable",
            "caseLevel", "caseFirst", "numeric", "reorder"
        };

        private static System.Collections.Generic.HashSet<string> keys = new System.Collections.Generic.HashSet<string>(Arrays.asList(keywords));

        public UcaCollatorUsingCSharp(string urim, Configuration config)
        {
            this.uri = uri;
            this.config = config;
            uca = System.StringComparer.CurrentCultureIgnoreCase; //new RuleBasedCollator();
            setProps(parseProps(urim));
            strength = 0;
        }

        /**
        * Set the strength of the collation (primary, secondary, tertiary, etc)
        * @param newStrength the strength
        */

        public void setStrength(int newStrength)
        {
            strength = newStrength;
        }

        private void setProps(Properties props)
        {
            this.properties = props;
            System.Globalization.CultureInfo loc = null;
            bool fallbackError = false;
            CompareOptions options = CompareOptions.None;
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
                        error("fallback=no is not available because ICU4J is not available");
#endif
                        }
                        else
                        {
                            error("fallback=no is not available with JDK-based collations");
                        }

                        break;
                    default:
                        error("fallback", "yes|no");
                        break;
                }
            }

            string lang = props.getProperty("lang");
            if (lang != null && lang.Length != 0)
            {
                ValidationFailure vf = Saxon.Hej.type.StringConverter.StringToLanguage.INSTANCE.validate(Saxon.Hej.str.StringView.tidy(lang));
                if (vf != null)
                {
                    error("lang", "a valid language code");
                }

                string language, country = "", variant = "";
                string[] parts = lang.Split("-");
                language = parts[0];
                if (parts.Length > 1)
                {
                    country = parts[1];
                }

                if (parts.Length > 2)
                {
                    variant = parts[2];
                }

                //TODO Locale loc = new Locale(language, country, variant);
                loc = makeLocale(language, country, variant);

                //TODO create RuleBaseCollator uca = (RuleBasedCollator) Collator.getInstance(loc);
            }

            string strengthAtt = props.getProperty("strength");
            if (strengthAtt != null && options == CompareOptions.None)
            {
                options = strengthAtt switch
                {
                    "primary" => System.Globalization.CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace |
                                 CompareOptions.IgnoreWidth,
                    "secondary" => CompareOptions.IgnoreCase | CompareOptions.IgnoreWidth,
                    "tertiary" => CompareOptions.IgnoreWidth,
                    "identical" => 0,
                    _ => throw new XPathException("strength must be primary, secondary, tertiary, or identical")
                };
            }

            // Look for the properties ignore-case, ignore-modifiers, ignore-width

            string ignore = props.getProperty("ignore-case");
            if (ignore != null)
            {
                if (ignore == "yes")
                {
                    options |= CompareOptions.IgnoreCase;
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
                    options |= CompareOptions.IgnoreNonSpace;
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
                    options |= CompareOptions.IgnoreSymbols;
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
                    options |= CompareOptions.IgnoreWidth;
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

            if (loc != null)
            {
                uca = System.StringComparer.Create(loc, options);
            }
            else
            {
                uca = System.StringComparer.Create(CultureInfo.CurrentCulture, options);
            }

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

        private static System.Globalization.CultureInfo makeLocale(string language, string country, string variant)
        {
            return makeCSharpLocale(language, country, variant);
        }

        private static System.Globalization.CultureInfo makeCSharpLocale(string language, string country,
            string variant)
        {
            StringBuilder sb = new StringBuilder(language);
            if (((country != null) && !(country.Length == 0)))
            {
                sb.append("-").append(country);
            }

            if (((variant != null) && !(variant.Length == 0)))
            {
                sb.append("-").append(variant);
            }

            return new System.Globalization.CultureInfo(sb.ToString());
        }


        private Properties parseProps(string uri)
        {
            System.Uri uuri;
            try
            {
                uuri = new System.Uri(uri, System.UriKind.RelativeOrAbsolute);
            }
            catch (System.UriFormatException err)
            {
                throw new XPathException(err);
            }

            System.Collections.Generic.List<string> unknownKeys = new System.Collections.Generic.List<string>();
            Properties props = new Properties();
            string query = AnyURIValue.decode(uuri.Query);
            if (((query != null) && !(query.Length == 0)))
            {
                if (query.StartsWith("?"))
                {
                    query = query.Substring(1);
                }

                foreach (string s in System.Text.RegularExpressions.Regex.Split(query, ";"))
                {
                    string[] tokens = System.Text.RegularExpressions.Regex.Split(s, "=");
                    if (!(keys.Contains(tokens[0])))
                    {
                        unknownKeys.Add(tokens[0]);
                    }

                    string value = ((tokens.Length >= 2) ? tokens[1] : "");
                    props.setProperty(tokens[0], value);
                }
            }

            string fallback = props.getProperty("fallback");
            if ((((fallback != null) && fallback.Equals("no")) && !(unknownKeys.Count == 0)))
            {
                StringBuilder message =
                    new StringBuilder(((unknownKeys.Count > 1) ? "unknown parameters:" : "unknown parameter:"));
                foreach (string u in unknownKeys)
                {
                    message.append(u).append(" ");
                }

                error(message.ToString());
            }

            return props;
        }

        public Properties getProperties()
        {
            return properties;
        }

        public string getCollationURI()
        {
            return uri;
        }

        private abstract class Strength
        {

            abstract public int compare(int ce1, int ce2);


        }


        private class Primary : Strength
        {
            public override int compare(int ce1, int ce2)
            {
                return ce1.CompareTo(ce2);
            }
        }

        private class Secondary : Strength
        {
            public override int compare(int ce1, int ce2)
            {
                int c1 = ce1.CompareTo(ce2);
                if (c1 == 0)
                {
                    return
                        0; //Integer.compare(CollationElementIterator.secondaryOrder(ce1), CollationElementIterator.secondaryOrder(ce2));
                }
                else
                {
                    return c1;
                }
            }

        }

        private class Tertiary : Strength
        {
            public override int compare(int ce1, int ce2)
            {
                throw new System.NotImplementedException();
            }


        }

        private class Identical : Strength
        {
            public override int compare(int ce1, int ce2)
            {
                throw new System.NotImplementedException();
            }

        }

        public int compareStrings(UnicodeString s1, UnicodeString s2)
        {
            return uca.Compare(s1.ToString(), s2.ToString());
        }


        public bool comparesEqual(UnicodeString s1, UnicodeString s2)
        {
            return uca.Compare(s1.ToString(), s2.ToString()) == 0;
        }

        public bool isEqualToEmpty(UnicodeString s1)
        {
            return s1.isEmpty();
        }

        public AtomicMatchKey getCollationKey(UnicodeString s)
        {
            CollationKey ck = new CollationKeyUsingCSharp(s.ToString());
            return new CollationMatchKey(ck);
        }

        private void error(string message)
        {
            throw new XPathException("Error in UCA Collation URI " + uri + ": " + message, "FOCH0002");
        }

        private void error(string field, string allowed)
        {
            error("value of " + field + " must be " + allowed);
        }

        private void error(string field, string allowed, string requested)
        {
            error("value of " + field + " must be " + allowed + ", requested was:" + requested);
        }

        public bool contains(UnicodeString s1, UnicodeString s2)
        {
            return s1.ToString().Contains(s2.ToString());
        }

        public bool startsWith(UnicodeString s1, UnicodeString s2)
        {
            return s1.ToString().StartsWith(s2.ToString());
        }

        public bool endsWith(UnicodeString s1, UnicodeString s2)
        {
            return s1.ToString().EndsWith(s2.ToString());
        }

        public UnicodeString substringBefore(UnicodeString s1, UnicodeString s2)
        {
            string g1 = s1.ToString();
            string g2 = s2.ToString();
            int bb = g1.IndexOf(g2);
            if (bb > 0)
            {
                return s1.substring(0, bb);
            }
            else
            {
                return EmptyUnicodeString.getInstance();
            }
        }

        public UnicodeString substringAfter(UnicodeString s1, UnicodeString s2)
        {
            string g1 = s1.ToString();
            string g2 = s2.ToString();
            int bb = g1.IndexOf(g2);
            if (bb > 0)
            {
                return s1.substring(bb);
            }
            else
            {
                return EmptyUnicodeString.getInstance();
            }
        }
    }
}