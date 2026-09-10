using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
#if EE
using ICU4N;
using ICU4N.Globalization;
#endif
using Saxon.Hej.expr.number;

// Note that we use #if EE in this class to separate the EE version that
// uses ICU4N from the HE version which does not.

namespace Saxon.Impl.Overrides {
    
    /// <summary>
    /// Handles localized formatting of dates, using .NET CultureInfo. Although a <c>LocalNumberer</c> in theory
    /// handles number formatting as well as date formatting, this particular implementation delegates
    /// all number formatting to its superclass, which only supports English.
    /// </summary>

    public class LocalNumberer : Numberer_en {

        private readonly CultureInfo culture;     // Standard .NET version

#if EE        
        private readonly UCultureInfo uCulture;   // ICU4N version
        private readonly Dictionary<string, string> ordinalParamIndex = new ();
        private CultureInfo defaultLocale = null;
#else
        private readonly Numberer_en numberer = new Numberer_en();
#endif

        public LocalNumberer(string language, string country) {
            try {
                culture = CultureInfo.GetCultureInfo(language, true);
#if EE
                uCulture = UCultureInfo.GetCultureInfoByIetfLanguageTag(language);
#endif
            }
            catch (System.Globalization.CultureNotFoundException) {
                culture = CultureInfo.GetCultureInfo("en");
                defaultLocale = culture;
#if EE
                uCulture = UCultureInfo.GetCultureInfoByIetfLanguageTag("en");
#endif
            }
        }

        public override System.Globalization.CultureInfo defaultedLocale()
        {
            return defaultLocale;
        }
        
        public override string monthName(int month, int minWidth, int maxWidth) {
            DateTimeFormatInfo format = culture.DateTimeFormat;
            string name = format.GetMonthName(month);
            if (name.Length > maxWidth) {
                name = format.GetAbbreviatedMonthName(month);
            }
            if (name.Length < minWidth) {
                name = name.PadRight(minWidth);
            }
            return name;
        }
       public override string dayName(int day, int minWidth, int maxWidth) {
           // day is (1=Monday, 7=Sunday)
           DateTimeFormatInfo format = culture.DateTimeFormat;
           DayOfWeek ddd = (DayOfWeek)(day % 7);
           string name = format.GetDayName(ddd);
           if (name.Length > maxWidth) {
               name = format.GetAbbreviatedDayName(ddd);
           }
           if (name.Length < minWidth) {
               name = name.PadRight(minWidth);
           }
           return name;
        }

#if EE
        public override string toWords(string cardinal, long number) {
            string ruleSetName = null;
            if (cardinal != "") {
                NumberFormatRules rules = uCulture.NumberFormat.SpellOut;
                if (rules.RuleSetNames.Contains(cardinal)) {
                    ruleSetName = cardinal;
                } else if (cardinal.Contains("ordinal", StringComparison.InvariantCulture)) {
                    ruleSetName = "%spellout-ordinal";
                }
                else {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Available formats are: ");
                    foreach (string name in rules.RuleSetNames) {
                        sb.Append(name).Append("; ");
                    }
                    Console.WriteLine(sb.ToString());
                }
            }
            return FormatNumberRuleBased.ToString(number, NumberPresentation.SpellOut, ruleSetName, uCulture); 
        }

        public override string toOrdinalWords(string ordinalParam, long number, int wc) {
            NumberFormatRules rules = uCulture.NumberFormat.SpellOut;
            string ruleSet = null;
            if (ordinalParam == "" || ordinalParam == "yes" || ordinalParam == "%spellout-ordinal") {
                ruleSet = "%spellout-ordinal";
            }
            else if (ordinalParam.StartsWith('%')) {
                ruleSet = ordinalParam;
            }
            else {
                if (ordinalParam.StartsWith("-") && ordinalParam.Length >= 2) {
                    // If the ordinalParam starts with "-" we treat it as a required suffix. We
                    // try formatting the number "2" with each possible ruleset, searching for a ruleset
                    // where the result ends with the required suffix. This tells us which ruleset to
                    // use. Once we've worked this out, we remember the association of suffix to ruleset
                    // for future use.
                    String suffix = ordinalParam.Substring(1);
                    foreach (string rs in rules.RuleSetNames) {
                        string two = 2.ToString(NumberPresentation.SpellOut, rs, uCulture);
                        if (two.EndsWith(suffix)) {
                            ruleSet = rs;
                            break;
                        }
                    }
                }
                else if (ordinalParam.StartsWith("2=") && ordinalParam.Length >= 3) {
                    // If the ordinalParam starts with "2=" then we treat it as containing the
                    // desired representation of the number 2, and again we use this information
                    // to find the appropriate ruleset which we can then use for all numbers
                    foreach (string rs in rules.RuleSetNames) {
                        string two = 2.ToString(NumberPresentation.SpellOut, rs, uCulture);
                        if (two == ordinalParam.Substring(2)) {
                            ruleSet = rs;
                            break;
                        }
                    }
                }
            }

            if (ruleSet == null || !rules.RuleSetNames.Contains(ruleSet)) {
                foreach (string rs in rules.RuleSetNames) {
                    string two = 2.ToString(NumberPresentation.SpellOut, rs, uCulture);
                    if (rs.Contains("ordinal")) {
                        ruleSet = rs;
                        break;
                    }
                }
            }

            if (ruleSet == null) {
                ruleSet = rules.DefaultRuleSetName;
            }
            
            if (ruleSet != null) {
                string word = number.ToString(NumberPresentation.SpellOut, ruleSet, uCulture);
                return wordCase(word, wc);
            }
            
            return number.ToString();
        }
#else
        public override string toWords(string cardinal, long number) {
            return numberer.toWords(cardinal, number);
        }

        public override string toOrdinalWords(string ordinalParam, long number, int wc) {
            return numberer.toOrdinalWords(ordinalParam, number, wc);
        }
#endif

        private String wordCase(string s, int wc) {
            return wc switch {
                UPPER_CASE => s.ToUpper(),
                LOWER_CASE => s.ToLower(),
                TITLE_CASE when s.Length > 1 && Char.IsUpper(s[0]) && Char.IsLower(s[1]) => s,
                TITLE_CASE => toTitleCase(s),
                _ => s
            };
        }



        public override string toTitleCase(string s) {
            return culture.TextInfo.ToTitleCase(s);
        }
    }
}
