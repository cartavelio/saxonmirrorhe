using System;
using System.Globalization;
using Saxon.Hej.expr.number;

namespace Saxon.Impl.Overrides {
    
    /// <summary>
    /// Handles localized formatting of dates, using .NET CultureInfo. Although a <c>LocalNumberer</c> in theory
    /// handles number formatting as well as date formatting, this particular implementation delegates
    /// all number formatting to its superclass, which only supports English.
    /// </summary>

    public class LocalNumberer : Numberer_en {

        private readonly CultureInfo culture;

        public LocalNumberer(string language, string country) {
            try {
                culture = CultureInfo.GetCultureInfo(language);
            }
            catch (System.Globalization.CultureNotFoundException) {
                culture = CultureInfo.GetCultureInfo("en-US");
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

    }
}
