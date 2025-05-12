using ICU4N;
using ICU4N.Globalization;

namespace Saxon.Localize {
    public class IcuNumberer  : Saxon.Hej.expr.number.AbstractNumberer {

        private UCultureInfo culture;
        
        public IcuNumberer(string language, string country) {
            try {
                culture = new UCultureInfo(language);
            }
            catch (System.Globalization.CultureNotFoundException) {
                culture = new UCultureInfo("en-US");
            }
        }
        
        public override void setLanguage(string language) {
            base.setLanguage(language);
            culture = new UCultureInfo(language);
        }
        protected internal override string ordinalSuffix(string ordinalParam, long number) {
            int penult = ((int)(((number%100)))/10);
            int ult = (int)(((number%10)));
            if ((penult == 1)) {
                return "th";
            } else {
                if ((ult == 1)) {
                    return "st";
                } else {
                    if ((ult == 2)) {
                        return "nd";
                    } else {
                        if ((ult == 3)) {
                            return "rd";
                        } else {
                            return "th";
                        }
                    }
                }
            }
        }
        public override string toWords(string cardinal, long number) {
            return number.ToString(NumberPresentation.SpellOut, culture);
        }
        public override string toOrdinalWords(string ordinalParam, long number, int wordCase) {
            string s = number.ToString(NumberPresentation.SpellOut, culture);
            if ((wordCase == UPPER_CASE)) {
                return s.ToUpper();
            } else {
                if ((wordCase == LOWER_CASE)) {
                    return s.ToLower();
                } else {
                    return s;
                }
            }
        }
        public override string monthName(int month, int minWidth, int maxWidth) {
            string name = englishMonths[(month-1)];
            if ((maxWidth<3)) {
                maxWidth = 3;
            }
            if ((name.Length>maxWidth)) {
                name = name.Substring(0, maxWidth - 0);
            }
            Saxon.Ejava.lang.StringBuilder nameBuilder = new Saxon.Ejava.lang.StringBuilder(name);
            while ((nameBuilder.length()<minWidth)) {
                nameBuilder.append(' ');
            }
            name = nameBuilder.ToString();
            return name;
        }
        private static readonly string[] englishMonths = {"January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"};
        public override string dayName(int day, int minWidth, int maxWidth) {
            string name = englishDays[(day-1)];
            if ((maxWidth<2)) {
                maxWidth = 2;
            }
            if ((name.Length>maxWidth)) {
                name = englishDayAbbreviations[(day-1)];
                if ((name.Length>maxWidth)) {
                    name = name.Substring(0, maxWidth - 0);
                }
            }
            Saxon.Ejava.lang.StringBuilder nameBuilder = new Saxon.Ejava.lang.StringBuilder(name);
            while ((nameBuilder.length()<minWidth)) {
                nameBuilder.append(' ');
            }
            name = nameBuilder.ToString();
            if (((minWidth == 1) && (maxWidth == 2))) {
                name = name.Substring(0, minUniqueDayLength[(day-1)] - 0);
            }
            return name;
        }
        private static readonly string[] englishDays = {"Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"};
        private static readonly string[] englishDayAbbreviations = {"Mon", "Tues", "Weds", "Thurs", "Fri", "Sat", "Sun"};
        private static readonly int[] minUniqueDayLength = {1, 2, 1, 2, 1, 2, 2};
    
        
    }
}