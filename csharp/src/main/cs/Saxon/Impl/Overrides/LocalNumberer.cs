using System;
using System.Globalization;
using System.Linq;
using ICU4N;
using ICU4N.Globalization;
using Saxon.Hej.expr.number;

namespace Saxon.Impl.Overrides {
    
    /// <summary>
    /// Handles localized formatting of dates, using .NET CultureInfo. Although a <c>LocalNumberer</c> in theory
    /// handles number formatting as well as date formatting, this particular implementation delegates
    /// all number formatting to its superclass, which only supports English.
    /// </summary>

    public class LocalNumberer : Numberer_en {

        private readonly CultureInfo culture;     // Standard .NET version
        private readonly UCultureInfo uCulture;   // ICU4N version

        public LocalNumberer(string language, string country) {
            try {
                culture = CultureInfo.GetCultureInfo(language);
                uCulture = UCultureInfo.GetCultureInfoByIetfLanguageTag(language);
            }
            catch (System.Globalization.CultureNotFoundException) {
                culture = CultureInfo.GetCultureInfo("en-US");
                uCulture = UCultureInfo.GetCultureInfoByIetfLanguageTag("en-US");
            }
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

        public override string toWords(string cardinal, long number) {
            string ruleSetName = null;
            if (cardinal != "") {
                NumberFormatRules rules = uCulture.NumberFormat.SpellOut;
                if (rules.RuleSetNames.Contains(cardinal)) {
                    ruleSetName = cardinal;
                } else if (cardinal.Contains("ordinal", StringComparison.InvariantCulture)) {
                    ruleSetName = "%spellout-ordinal";
                }
            }
            return FormatNumberRuleBased.ToString(number, NumberPresentation.SpellOut, ruleSetName, uCulture); 
        }

        public override string toOrdinalWords(string ordinalParam, long number, int wordCase) {
            NumberFormatRules rules = uCulture.NumberFormat.SpellOut;
            return FormatNumberRuleBased.ToString(number, NumberPresentation.Ordinal, null, uCulture); 
        }


        public override string toTitleCase(string s) {
            return culture.TextInfo.ToTitleCase(s);
        }
    }
}
