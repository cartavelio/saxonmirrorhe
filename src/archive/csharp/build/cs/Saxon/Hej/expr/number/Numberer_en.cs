using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.number {
    public class Numberer_en : Saxon.Hej.expr.number.AbstractNumberer {
        private string tensUnitsSeparatorCardinal = " ";
        private string tensUnitsSeparatorOrdinal = "-";
        public void setTensUnitsSeparatorCardinal(string separator) {
            tensUnitsSeparatorCardinal = separator;
        }
        public void setTensUnitsSeparatorOrdinal(string separator) {
            tensUnitsSeparatorOrdinal = separator;
        }
        public override void setLanguage(string language) {
            base.setLanguage(language);
            if (language.EndsWith("-x-hyphen")) {
                setTensUnitsSeparatorOrdinal("-");
                setTensUnitsSeparatorCardinal("-");
            } else {
                if (language.EndsWith("-x-nohyphen")) {
                    setTensUnitsSeparatorOrdinal(" ");
                    setTensUnitsSeparatorCardinal(" ");
                }
            }
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
            if ((number>=1000000000)) {
                long rem = (number%1000000000);
                return ((toWords(cardinal, (number/1000000000))+" Billion")+(((rem == 0) ? "" : ((((rem<100) ? " and " : " "))+toWords(cardinal, rem)))));
            } else {
                if ((number>=1000000)) {
                    long rem = (number%1000000);
                    return ((toWords(cardinal, (number/1000000))+" Million")+(((rem == 0) ? "" : ((((rem<100) ? " and " : " "))+toWords(cardinal, rem)))));
                } else {
                    if ((number>=1000)) {
                        long rem = (number%1000);
                        return ((toWords(cardinal, (number/1000))+" Thousand")+(((rem == 0) ? "" : ((((rem<100) ? " and " : " "))+toWords(cardinal, rem)))));
                    } else {
                        if ((number>=100)) {
                            long rem = (number%100);
                            return ((toWords(cardinal, (number/100))+" Hundred")+(((rem == 0) ? "" : (" and "+toWords(cardinal, rem)))));
                        } else {
                            if ((number<20)) {
                                return englishUnits[(int)(number)];
                            }
                            int rem = (int)(((number%10)));
                            return (englishTens[((int)(number)/10)]+(((rem == 0) ? "" : (tensUnitsSeparatorCardinal+englishUnits[rem]))));
                        }
                    }
                }
            }
        }
        public override string toOrdinalWords(string ordinalParam, long number, int wordCase) {
            string s;
            if ((number>=1000000000)) {
                long rem = (number%1000000000);
                s = ((toWords(ordinalParam, (number/1000000000))+" Billion")+(((rem == 0) ? "th" : ((((rem<100) ? " and " : " "))+toOrdinalWords(ordinalParam, rem, wordCase)))));
            } else {
                if ((number>=1000000)) {
                    long rem = (number%1000000);
                    s = ((toWords(ordinalParam, (number/1000000))+" Million")+(((rem == 0) ? "th" : ((((rem<100) ? " and " : " "))+toOrdinalWords(ordinalParam, rem, wordCase)))));
                } else {
                    if ((number>=1000)) {
                        long rem = (number%1000);
                        s = ((toWords(ordinalParam, (number/1000))+" Thousand")+(((rem == 0) ? "th" : ((((rem<100) ? " and " : " "))+toOrdinalWords(ordinalParam, rem, wordCase)))));
                    } else {
                        if ((number>=100)) {
                            long rem = (number%100);
                            s = ((toWords(ordinalParam, (number/100))+" Hundred")+(((rem == 0) ? "th" : (" and "+toOrdinalWords(ordinalParam, rem, wordCase)))));
                        } else {
                            if ((number<20)) {
                                s = englishOrdinalUnits[(int)(number)];
                            } else {
                                int rem = (int)(((number%10)));
                                if ((rem == 0)) {
                                    s = englishOrdinalTens[((int)(number)/10)];
                                } else {
                                    s = ((englishTens[((int)(number)/10)]+tensUnitsSeparatorOrdinal)+englishOrdinalUnits[rem]);
                                }
                            }
                        }
                    }
                }
            }
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
        private static readonly string[] englishUnits = {"Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"};
        private static readonly string[] englishTens = {"", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"};
        private static readonly string[] englishOrdinalUnits = {"Zeroth", "First", "Second", "Third", "Fourth", "Fifth", "Sixth", "Seventh", "Eighth", "Ninth", "Tenth", "Eleventh", "Twelfth", "Thirteenth", "Fourteenth", "Fifteenth", "Sixteenth", "Seventeenth", "Eighteenth", "Nineteenth"};
        private static readonly string[] englishOrdinalTens = {"", "Tenth", "Twentieth", "Thirtieth", "Fortieth", "Fiftieth", "Sixtieth", "Seventieth", "Eightieth", "Ninetieth"};
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