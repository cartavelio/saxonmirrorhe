using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.expr.sort {
    public class UcaCollatorUsingIcu : Saxon.Hej.lib.SubstringMatcher {
        private readonly string uri;
        private ICU4N.Text.RuleBasedCollator uca;
        private Saxon.Eej.expr.sort.UcaCollatorUsingIcu.Strength strengthLevel;
        public int[] maxIgnorable;
        private static readonly string[] keywords = {"fallback", "lang", "version", "strength", "alternate", "backwards", "normalization", "maxVariable", "caseLevel", "caseFirst", "numeric", "reorder"};
        private static readonly System.Collections.Generic.ISet<string> keys = new System.Collections.Generic.HashSet<string>(Saxon.Ejava.util.Arrays.asList(keywords));
        public static string getUCAVersion() {
            ICU4N.Util.VersionInfo v = ICU4N.Text.Collator.GetInstance().GetUCAVersion();
            return ((((((v.Major+".")+v.Minor)+".")+v.Milli)+".")+v.Micro);
        }
        public UcaCollatorUsingIcu(string uri)  {
            this.uri = uri;
            uca = (ICU4N.Text.RuleBasedCollator)(ICU4N.Text.Collator.GetInstance());
            setProps(parseProps(uri));
        }
        public ICU4N.Text.RuleBasedCollator getRuleBasedCollator() {
            return uca;
        }
        private void error(string field, string allowed) {
            error(((("value of "+field)+" must be ")+allowed));
        }
        private void error(string field, string allowed, string requested) {
            error(((((("value of "+field)+" must be ")+allowed)+", requested was:")+requested));
        }
        private void error(string message) {
            throw new Saxon.Hej.trans.XPathException(((("Error in UCA Collation URI "+uri)+": ")+message), "FOCH0002");
        }
        public Saxon.Ejava.text.CollationKey getCollationKey(string source) {
            return new Saxon.Eej.expr.sort.UcaCollationKeyUsingIcu(source, uca.GetCollationKey(source).ToByteArray());
        }
        public override int GetHashCode() {
            return uca.GetHashCode();
        }
        public void setProps(Saxon.Ejava.util.Properties props) {
            bool fallbackError = false;
            string fallback = props.getProperty("fallback");
            if ((fallback != null)) {
                switch (fallback) {
                  case "yes":
                    break ;
                  case "no":
                    fallbackError = true;
                    break ;
                  default:
                    error("fallback", "yes|no");
                    break ;
                }
            }
            string lang = props.getProperty("lang");
            if (((lang != null) && !(lang.Length==0))) {
                Saxon.Hej.type.ValidationFailure vf = Saxon.Hej.type.StringConverter.StringToLanguage.INSTANCE.validate(Saxon.Hej.str.StringView.tidy(lang));
                if ((vf != null)) {
                    error("lang", "a valid language code");
                }
                string language, country = "", variant = "";
                string[] parts = System.Text.RegularExpressions.Regex.Split(lang, "-");
                language = parts[0];
                if ((parts.Length>1)) {
                    country = parts[1];
                }
                if ((parts.Length>2)) {
                    variant = parts[2];
                }
                System.Globalization.CultureInfo loc = makeLocale(language, country, variant);
                uca = (ICU4N.Text.RuleBasedCollator)(ICU4N.Text.Collator.GetInstance(loc));
            }
            string version = props.getProperty("version");
            if ((version != null)) {
                try {
                    ICU4N.Util.VersionInfo v = ICU4N.Util.VersionInfo.GetInstance(version);
                    ICU4N.Util.VersionInfo current = uca.GetUCAVersion();
                    if (((v.CompareTo(current)>0) && fallbackError)) {
                        error(((("requesting UCA version "+v.ToString())+", implementation supports UCA version ")+current.ToString()));
                    }
                } catch (System.Exception e) {
                    if (fallbackError) {
                        error(("version number format incorrect:"+version));
                    }
                }
            }
            string strength = props.getProperty("strength");
            if ((strength != null)) {
                switch (strength) {
                  case "primary":
                  case "1":
                    setStrength((int)ICU4N.Text.CollationStrength.Primary);
                    break ;
                  case "secondary":
                  case "2":
                    setStrength((int)ICU4N.Text.CollationStrength.Secondary);
                    break ;
                  case "tertiary":
                  case "3":
                    setStrength((int)ICU4N.Text.CollationStrength.Tertiary);
                    break ;
                  case "quaternary":
                  case "4":
                    setStrength((int)ICU4N.Text.CollationStrength.Quaternary);
                    break ;
                  case "identical":
                  case "5":
                    setStrength((int)ICU4N.Text.CollationStrength.Identical);
                    break ;
                  default:
                    if (fallbackError) {
                        error("strength", "primary|secondary|tertiary|quaternary|identical|1|2|3|4|5", strength);
                    }
                    break ;
                }
            }
            string maxVariable = props.getProperty("maxVariable");
            if ((maxVariable != null)) {
                if (maxVariable.Equals("space")) {
                    uca.MaxVariable = (ICU4N.Text.ReorderCodes.Space);
                } else {
                    if (maxVariable.Equals("punct")) {
                        uca.MaxVariable = (ICU4N.Text.ReorderCodes.Punctuation);
                    } else {
                        if (maxVariable.Equals("symbol")) {
                            uca.MaxVariable = (ICU4N.Text.ReorderCodes.Symbol);
                        } else {
                            if (maxVariable.Equals("currency")) {
                                uca.MaxVariable = (ICU4N.Text.ReorderCodes.Currency);
                            } else {
                                if (fallbackError) {
                                    error("maxVariable", "space|punct|symbol|currency");
                                }
                            }
                        }
                    }
                }
            }
            string alternate = props.getProperty("alternate");
            if ((alternate != null)) {
                switch (alternate) {
                  case "non-ignorable":
                    uca.IsAlternateHandlingShifted = (false);
                    break ;
                  case "shifted":
                    uca.IsAlternateHandlingShifted = (true);
                    break ;
                  case "blanked":
                    uca.IsAlternateHandlingShifted = (true);
                    if ("quaternary".Equals(props.getProperty("strength"))) {
                        setStrength((int)ICU4N.Text.CollationStrength.Tertiary);
                    }
                    break ;
                  default:
                    if (fallbackError) {
                        error("alternate", "non-ignorable|shifted|blanked", alternate);
                    }
                    break ;
                }
            }
            string backwards = props.getProperty("backwards");
            if ((backwards != null)) {
                switch (backwards) {
                  case "yes":
                    uca.IsFrenchCollation = (true);
                    break ;
                  case "no":
                    uca.IsFrenchCollation = (false);
                    break ;
                  default:
                    if (fallbackError) {
                        error("backwards", "yes|no");
                    }
                    break ;
                }
            }
            string normalization = props.getProperty("normalization");
            if ((normalization != null)) {
                switch (normalization) {
                  case "yes":
                    uca.Decomposition = (ICU4N.Text.NormalizationMode.CanonicalDecomposition);
                    break ;
                  case "no":
                    uca.Decomposition = (ICU4N.Text.NormalizationMode.NoDecomposition);
                    break ;
                  default:
                    if (fallbackError) {
                        error("normalization", "yes|no");
                    }
                    break ;
                }
            }
            string caseLevel = props.getProperty("caseLevel");
            if ((caseLevel != null)) {
                switch (caseLevel) {
                  case "yes":
                    uca.IsCaseLevel = (true);
                    break ;
                  case "no":
                    uca.IsCaseLevel = (false);
                    break ;
                  default:
                    if (fallbackError) {
                        error("caseLevel", "yes|no");
                    }
                    break ;
                }
            }
            string caseFirst = props.getProperty("caseFirst");
            if ((caseFirst != null)) {
                switch (caseFirst) {
                  case "upper":
                    uca.IsUpperCaseFirst = (true);
                    break ;
                  case "lower":
                    uca.IsLowerCaseFirst = (true);
                    break ;
                  default:
                    if (fallbackError) {
                        error("caseFirst", "upper|lower");
                    }
                    break ;
                }
            }
            string numeric = props.getProperty("numeric");
            if ((numeric != null)) {
                switch (numeric) {
                  case "yes":
                    uca.IsNumericCollation = (true);
                    break ;
                  case "no":
                    uca.IsNumericCollation = (false);
                    break ;
                  default:
                    if (fallbackError) {
                        error("numeric", "yes|no");
                    }
                    break ;
                }
            }
            string reorder = props.getProperty("reorder");
            if ((reorder != null)) {
                System.Collections.Generic.List<int> codesInt = new System.Collections.Generic.List<int>();
                foreach (string param in System.Text.RegularExpressions.Regex.Split(reorder, ",")) {
                    if (param.Equals("space")) {
                        codesInt.Add(ICU4N.Text.ReorderCodes.Space);
                    } else {
                        if (param.Equals("punct")) {
                            codesInt.Add(ICU4N.Text.ReorderCodes.Punctuation);
                        } else {
                            if (param.Equals("symbol")) {
                                codesInt.Add(ICU4N.Text.ReorderCodes.Symbol);
                            } else {
                                if (param.Equals("currency")) {
                                    codesInt.Add(ICU4N.Text.ReorderCodes.Currency);
                                } else {
                                    if (param.Equals("digit")) {
                                        codesInt.Add(ICU4N.Text.ReorderCodes.Digit);
                                    } else {
                                        if ((param.Length == 4)) {
                                            int[] script = ICU4N.Globalization.UScript.GetCode(param);
                                            if ((script != null)) {
                                                codesInt.Add(script[0]);
                                            } else {
                                                if (fallbackError) {
                                                    error("reorder-code", "space|punct|symbol|currency|digit or 4-character script");
                                                }
                                            }
                                        } else {
                                            if (fallbackError) {
                                                error("reorder-code", "space|punct|symbol|currency|digit or 4-character script");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                int[] codes = new int[codesInt.Count];
                int i = 0;
                foreach (int n in codesInt) {
                    codes[i++] = n;
                }
                uca.SetReorderCodes(codes);
            }
            #pragma warning disable 618
            int m = uca.VariableTop;
            #pragma warning restore 618
            maxIgnorable = new int[]{m, m, m, m, 0};
        }
        private static System.Globalization.CultureInfo makeLocale(string language, string country, string variant) {
            return makeCSharpLocale(language, country, variant);
        }
        private static System.Globalization.CultureInfo makeCSharpLocale(string language, string country, string variant) {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(language);
            if (((country != null) && !(country.Length==0))) {
                sb.append("-").append(country);
            }
            if (((variant != null) && !(variant.Length==0))) {
                sb.append("-").append(variant);
            }
            return new System.Globalization.CultureInfo(sb.ToString());
        }
        private Saxon.Ejava.util.Properties parseProps(string uri) {
            System.Uri uuri;
            try {
                uuri = new System.Uri(uri, System.UriKind.RelativeOrAbsolute);
            } catch (System.UriFormatException err) {
                throw new Saxon.Hej.trans.XPathException(err);
            }
            System.Collections.Generic.List<string> unknownKeys = new System.Collections.Generic.List<string>();
            Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
            string query = Saxon.Hej.value.AnyURIValue.decode(uuri.Query);
            if (((query != null) && !(query.Length==0))) {
                if (query.StartsWith("?")) {
                    query = query.Substring(1);
                }
                foreach (string s in System.Text.RegularExpressions.Regex.Split(query, ";")) {
                    string[] tokens = System.Text.RegularExpressions.Regex.Split(s, "=");
                    if (!(keys.Contains(tokens[0]))) {
                        unknownKeys.Add(tokens[0]);
                    }
                    props.setProperty(tokens[0], tokens[1]);
                }
            }
            string fallback = props.getProperty("fallback");
            if ((((fallback != null) && fallback.Equals("no")) && !(unknownKeys.Count==0))) {
                string message = ((unknownKeys.Count>1) ? "unknown parameters:" : "unknown parameter:");
                foreach (string u in unknownKeys) {
                    message += (u+" ");
                }
                error(message);
            }
            return props;
        }
        public void setStrength(int newStrength) {
            uca.Strength = (ICU4N.Text.CollationStrength)(newStrength);
        }
        public int getStrength() {
            return (int)uca.Strength;
        }
        public bool comparesEqual(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            return (uca.Compare(s1.ToString(), s2.ToString()) == 0);
        }
        public string getCollationURI() {
            return uri;
        }
        public int compareStrings(Saxon.Hej.str.UnicodeString o1, Saxon.Hej.str.UnicodeString o2) {
            return uca.Compare(o1.ToString(), o2.ToString());
        }
        public Saxon.Hej.expr.sort.AtomicMatchKey getCollationKey(Saxon.Hej.str.UnicodeString s) {
            Saxon.Ejava.text.CollationKey ck = getCollationKey(s.ToString());
            return new Saxon.Hej.expr.sort.CollationMatchKey(ck);
        }
        public bool contains(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            ICU4N.Text.RuleBasedCollator collator = getRuleBasedCollator();
            ICU4N.Text.CollationElementIterator iter1 = collator.GetCollationElementIterator(s1.ToString());
            ICU4N.Text.CollationElementIterator iter2 = collator.GetCollationElementIterator(s2.ToString());
            return collationContains(iter1, iter2, null, false);
        }
        public bool endsWith(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            ICU4N.Text.RuleBasedCollator collator = getRuleBasedCollator();
            ICU4N.Text.CollationElementIterator iter1 = collator.GetCollationElementIterator(s1.ToString());
            ICU4N.Text.CollationElementIterator iter2 = collator.GetCollationElementIterator(s2.ToString());
            return collationContains(iter1, iter2, null, true);
        }
        public bool startsWith(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            ICU4N.Text.RuleBasedCollator collator = getRuleBasedCollator();
            ICU4N.Text.CollationElementIterator iter1 = collator.GetCollationElementIterator(s1.ToString());
            ICU4N.Text.CollationElementIterator iter2 = collator.GetCollationElementIterator(s2.ToString());
            return collationStartsWith(iter1, iter2);
        }
        public Saxon.Hej.str.UnicodeString substringAfter(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            ICU4N.Text.RuleBasedCollator collator = getRuleBasedCollator();
            string g1 = s1.ToString();
            ICU4N.Text.CollationElementIterator iter1 = collator.GetCollationElementIterator(g1);
            ICU4N.Text.CollationElementIterator iter2 = collator.GetCollationElementIterator(s2.ToString());
            int[] ia = new int[2];
            bool ba = collationContains(iter1, iter2, ia, false);
            if (ba) {
                return s1.substring(ia[1]);
            } else {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
        }
        public Saxon.Hej.str.UnicodeString substringBefore(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            ICU4N.Text.RuleBasedCollator collator = getRuleBasedCollator();
            string g1 = s1.ToString();
            ICU4N.Text.CollationElementIterator iter1 = collator.GetCollationElementIterator(g1);
            ICU4N.Text.CollationElementIterator iter2 = collator.GetCollationElementIterator(s2.ToString());
            int[] ib = new int[2];
            bool bb = collationContains(iter1, iter2, ib, false);
            if (bb) {
                return s1.substring(0, ib[0]);
            } else {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
        }
        private bool collationStartsWith(ICU4N.Text.CollationElementIterator s0, ICU4N.Text.CollationElementIterator s1) {
            makeStrengthObject();
            while (true) {
                int e0, e1;
                do {
                    e1 = s1.Next();
                } while (strengthLevel.isIgnored(e1));
                if ((e1 == ICU4N.Text.CollationElementIterator.NullOrder)) {
                    return true;
                }
                do {
                    e0 = s0.Next();
                } while (strengthLevel.isIgnored(e0));
                if ((e0 == ICU4N.Text.CollationElementIterator.NullOrder)) {
                    return false;
                }
                if ((strengthLevel.compareCollationElements(e0, e1) != 0)) {
                    return false;
                }
            }
        }
        private string show(int ce) {
            return (((((""+ICU4N.Text.CollationElementIterator.PrimaryOrder(ce))+"/")+ICU4N.Text.CollationElementIterator.SecondaryOrder(ce))+"/")+ICU4N.Text.CollationElementIterator.TertiaryOrder(ce));
        }
        private void makeStrengthObject() {
            if ((strengthLevel == null)) {
                switch (getStrength()) {
                  case (int)ICU4N.Text.CollationStrength.Primary:
                    strengthLevel = new Saxon.Eej.expr.sort.UcaCollatorUsingIcu.Primary(this);
                    break ;
                  case (int)ICU4N.Text.CollationStrength.Secondary:
                    strengthLevel = new Saxon.Eej.expr.sort.UcaCollatorUsingIcu.Secondary(this);
                    break ;
                  case (int)ICU4N.Text.CollationStrength.Tertiary:
                    strengthLevel = new Saxon.Eej.expr.sort.UcaCollatorUsingIcu.Tertiary(this);
                    break ;
                  default:
                    strengthLevel = new Saxon.Eej.expr.sort.UcaCollatorUsingIcu.Identical(this);
                    break ;
                }
            }
        }
        private bool collationContains(ICU4N.Text.CollationElementIterator s0, ICU4N.Text.CollationElementIterator s1, int[] offsets, bool matchAtEnd) {
            makeStrengthObject();
            int e0, e1;
            do {
                e1 = s1.Next();
            } while (strengthLevel.isIgnored(e1));
            if ((e1 == ICU4N.Text.CollationElementIterator.NullOrder)) {
                return true;
            }
            e0 = ICU4N.Text.CollationElementIterator.NullOrder;
            while (true) {
                while ((strengthLevel.compareCollationElements(e0, e1) != 0)) {
                    do {
                        e0 = s0.Next();
                    } while (strengthLevel.isIgnored(e0));
                    if ((e0 == ICU4N.Text.CollationElementIterator.NullOrder)) {
                        return false;
                    }
                }
                int start = s0.GetOffset();
                if (collationStartsWith(s0, s1)) {
                    if (matchAtEnd) {
                        do {
                            e0 = s0.Next();
                        } while (strengthLevel.isIgnored(e0));
                        if ((e0 == ICU4N.Text.CollationElementIterator.NullOrder)) {
                            return true;
                        }
                    } else {
                        if ((offsets != null)) {
                            offsets[0] = (start-1);
                            offsets[1] = s0.GetOffset();
                        }
                        return true;
                    }
                }
                s0.SetOffset(start);
                if ((s0.GetOffset() != start)) {
                    s0.Next();
                }
                s1.Reset();
                e0 = -1;
                do {
                    e1 = s1.Next();
                } while (strengthLevel.isIgnored(e1));
            }
        }
        private abstract class Strength {
            protected internal Saxon.Eej.expr.sort.UcaCollatorUsingIcu collator;
            public Strength(Saxon.Eej.expr.sort.UcaCollatorUsingIcu thisCollator)  {
                this.collator = thisCollator;
            }
            abstract public int compareCollationElements(int ce1, int ce2);
            abstract public bool isIgnored(int ce);
        
        }
        private class Primary : Saxon.Eej.expr.sort.UcaCollatorUsingIcu.Strength {
            public Primary(Saxon.Eej.expr.sort.UcaCollatorUsingIcu collator) : base (collator) {
            }
            public override int compareCollationElements(int ce1, int ce2) {
                return ICU4N.Text.CollationElementIterator.PrimaryOrder(ce1).CompareTo(ICU4N.Text.CollationElementIterator.PrimaryOrder(ce2));
            }
            public override bool isIgnored(int ce) {
                return ((ce != ICU4N.Text.CollationElementIterator.NullOrder) && (((ICU4N.Text.CollationElementIterator.PrimaryOrder(ce) == ICU4N.Text.CollationElementIterator.Ingorable) || (ce<collator.maxIgnorable[0]))));
            }
        
        }
        private class Secondary : Saxon.Eej.expr.sort.UcaCollatorUsingIcu.Strength {
            public Secondary(Saxon.Eej.expr.sort.UcaCollatorUsingIcu collator) : base (collator) {
            }
            public override int compareCollationElements(int ce1, int ce2) {
                int c1 = ICU4N.Text.CollationElementIterator.PrimaryOrder(ce1).CompareTo(ICU4N.Text.CollationElementIterator.PrimaryOrder(ce2));
                if ((c1 == 0)) {
                    return ICU4N.Text.CollationElementIterator.SecondaryOrder(ce1).CompareTo(ICU4N.Text.CollationElementIterator.SecondaryOrder(ce2));
                } else {
                    return c1;
                }
            }
            public override bool isIgnored(int ce) {
                return false;
            }
        
        }
        private class Tertiary : Saxon.Eej.expr.sort.UcaCollatorUsingIcu.Strength {
            public Tertiary(Saxon.Eej.expr.sort.UcaCollatorUsingIcu collator) : base (collator) {
            }
            public override int compareCollationElements(int ce1, int ce2) {
                int c1 = ICU4N.Text.CollationElementIterator.PrimaryOrder(ce1).CompareTo(ICU4N.Text.CollationElementIterator.PrimaryOrder(ce2));
                if ((c1 == 0)) {
                    int c2 = ICU4N.Text.CollationElementIterator.SecondaryOrder(ce1).CompareTo(ICU4N.Text.CollationElementIterator.SecondaryOrder(ce2));
                    if ((c2 == 0)) {
                        return ICU4N.Text.CollationElementIterator.TertiaryOrder(ce1).CompareTo(ICU4N.Text.CollationElementIterator.TertiaryOrder(ce2));
                    } else {
                        return c2;
                    }
                } else {
                    return c1;
                }
            }
            public override bool isIgnored(int ce) {
                return (((ce != ICU4N.Text.CollationElementIterator.NullOrder) && (ICU4N.Text.CollationElementIterator.SecondaryOrder(ce)<6)) && (ICU4N.Text.CollationElementIterator.PrimaryOrder(ce)<ICU4N.Text.CollationElementIterator.PrimaryOrder(collator.maxIgnorable[0])));
            }
        
        }
        private class Identical : Saxon.Eej.expr.sort.UcaCollatorUsingIcu.Strength {
            public Identical(Saxon.Eej.expr.sort.UcaCollatorUsingIcu collator) : base (collator) {
            }
            public override int compareCollationElements(int ce1, int ce2) {
                return ce1.CompareTo(ce2);
            }
            public override bool isIgnored(int ce) {
                return false;
            }
        
        }
    
    }
}