using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface Numberer {
        void setCountry(string country);
        string getCountry();
        System.Globalization.CultureInfo defaultedLocale();
        string format(long number, Saxon.Hej.str.UnicodeString picture, int groupSize, string groupSeparator, string letterValue, string ordinal);
        string format(long number, Saxon.Hej.str.UnicodeString picture, Saxon.Hej.expr.number.NumericGroupFormatter numGrpFormatter, string letterValue, string ordinal);
        string monthName(int month, int minWidth, int maxWidth);
        string dayName(int day, int minWidth, int maxWidth);
        string halfDayName(int minutes, int minWidth, int maxWidth);
        string getOrdinalSuffixForDateTime(string component);
        string getEraName(int year);
        string getCalendarName(string code);
    
    }
}