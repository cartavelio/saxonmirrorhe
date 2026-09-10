using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.number {
    public abstract class AbstractNumberer : Saxon.Hej.lib.Numberer {
        private string country;
        private string language;
        public const int UPPER_CASE = 0;
        public const int LOWER_CASE = 1;
        public const int TITLE_CASE = 2;
        public System.Globalization.CultureInfo defaultedLocale() {
            return null;
        }
        public void setCountry(string country) {
            this.country = country;
        }
        public virtual void setLanguage(string language) {
            this.language = language;
        }
        public string getLanguage() {
            return language;
        }
        public string getCountry() {
            return country;
        }
        public string format(long number, Saxon.Hej.str.UnicodeString picture, int groupSize, string groupSeparator, string letterValue, string cardinal, string ordinal) {
            return format(number, picture, new Saxon.Hej.expr.number.RegularGroupFormatter(groupSize, groupSeparator, Saxon.Hej.str.EmptyUnicodeString.getInstance()), letterValue, cardinal, ordinal);
        }
        public string format(long number, Saxon.Hej.str.UnicodeString picture, Saxon.Hej.expr.number.NumericGroupFormatter numGroupFormatter, string letterValue, string cardinal, string ordinal) {
            if ((number<0)) {
                return (""+number);
            }
            if (((picture == null) || (picture.length() == 0))) {
                return (""+number);
            }
            int pictureLength = picture.length32();
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(16);
            int formchar = picture.codePointAt(0);
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(2);
            switch (formchar) {
              case '0':
              case '1':
                sb.append(toRadical(number, westernDigits, pictureLength, numGroupFormatter));
                if (((ordinal != null) && !(ordinal.Length==0))) {
                    sb.append(ordinalSuffix(ordinal, number));
                }
                break ;
              case 'A':
                if ((number == 0)) {
                    return "0";
                }
                return toAlphaSequence(number, latinUpper);
              case 'a':
                if ((number == 0)) {
                    return "0";
                }
                return toAlphaSequence(number, latinLower);
              case 'w':
              case 'W':
                int wordCase;
                if ((pictureLength == 1)) {
                    if ((formchar == 'W')) {
                        wordCase = UPPER_CASE;
                    } else {
                        wordCase = LOWER_CASE;
                    }
                } else {
                    wordCase = TITLE_CASE;
                }
                if (((ordinal != null) && !(ordinal.Length==0))) {
                    return toOrdinalWords(ordinal, number, wordCase);
                } else {
                    return toWords(cardinal, number, wordCase);
                }
              case 'i':
                if ((number == 0)) {
                    return "0";
                }
                if ((((letterValue == null) || letterValue.Length==0) || letterValue.Equals("traditional"))) {
                    return toRoman(number);
                } else {
                    alphaDefault(number, 'i', sb);
                }
                break ;
              case 'I':
                if ((number == 0)) {
                    return "0";
                }
                if ((((letterValue == null) || letterValue.Length==0) || letterValue.Equals("traditional"))) {
                    return toRoman(number).ToUpper();
                } else {
                    alphaDefault(number, 'I', sb);
                }
                break ;
              case '\u2460':
                if ((number == 0)) {
                    return (""+(char)(0x24EA));
                }
                if (((number>20) && (number<=35))) {
                    return (""+(char)((((0x3251+number)-21))));
                }
                if (((number>35) && (number<=50))) {
                    return (""+(char)((((0x32B1+number)-36))));
                }
                if ((number>50)) {
                    return (""+number);
                }
                return (""+(char)((((0x2460+number)-1))));
              case '\u2474':
                if (((number == 0) || (number>20))) {
                    return (""+number);
                }
                return (""+(char)((((0x2474+number)-1))));
              case '\u2488':
                if ((number == 0)) {
                    return ((""+(char)(0xD83C))+(char)(0xDD00));
                }
                if ((number>20)) {
                    return (""+number);
                }
                return (""+(char)((((0x2488+number)-1))));
              case '\u2776':
                if ((number == 0)) {
                    return (""+(char)(0x24FF));
                }
                if (((number>10) && (number<=20))) {
                    return (""+(char)((((0x24EB+number)-11))));
                }
                if ((number>20)) {
                    return (""+number);
                }
                return (""+(char)((((0x2776+number)-1))));
              case '\u2780':
                if ((number == 0)) {
                    return ((""+(char)(0xD83C))+(char)(0xDD0B));
                }
                if ((number>10)) {
                    return (""+number);
                }
                return (""+(char)((((0x2780+number)-1))));
              case '\u24F5':
                if (((number == 0) || (number>10))) {
                    return (""+number);
                }
                return (""+(char)((((0x24F5+number)-1))));
              case '\u278A':
                if ((number == 0)) {
                    return ((""+(char)(0xD83C))+(char)(0xDD0C));
                }
                if ((number>10)) {
                    return (""+number);
                }
                return (""+(char)((((0x278A+number)-1))));
              case '\u3220':
                if (((number == 0) || (number>10))) {
                    return (""+number);
                }
                return (""+(char)((((0x3220+number)-1))));
              case '\u3280':
                if (((number == 0) || (number>10))) {
                    return (""+number);
                }
                return (""+(char)((((0x3280+number)-1))));
              case 65799:
                if (((number == 0) || (number>10))) {
                    return (""+number);
                }
                fsb.appendCodePoint(((65799+(int)(number))-1));
                return fsb.ToString();
              case 69216:
                if (((number == 0) || (number>10))) {
                    return (""+number);
                }
                fsb.appendCodePoint(((69216+(int)(number))-1));
                return fsb.ToString();
              case 69714:
                if (((number == 0) || (number>10))) {
                    return (""+number);
                }
                fsb.appendCodePoint(((69714+(int)(number))-1));
                return fsb.ToString();
              case 119648:
                if (((number == 0) || (number>=10))) {
                    return (""+number);
                }
                fsb.appendCodePoint(((119648+(int)(number))-1));
                return fsb.ToString();
              case 127234:
                if ((number == 0)) {
                    fsb.appendCodePoint(127233);
                    return fsb.ToString();
                }
                if ((number>=10)) {
                    return (""+number);
                }
                fsb.appendCodePoint(((127234+(int)(number))-1));
                return fsb.ToString();
              case '\u0391':
                if ((number == 0)) {
                    return "0";
                }
                return toAlphaSequence(number, greekUpper);
              case '\u03b1':
                if ((number == 0)) {
                    return "0";
                }
                return toAlphaSequence(number, greekLower);
              case '\u0410':
                if ((number == 0)) {
                    return "0";
                }
                return toAlphaSequence(number, cyrillicUpper);
              case '\u0430':
                if ((number == 0)) {
                    return "0";
                }
                return toAlphaSequence(number, cyrillicLower);
              case '\u05d0':
                if ((number == 0)) {
                    return "0";
                }
                return toAlphaSequence(number, hebrew);
              case '\u3042':
                if ((number == 0)) {
                    return "0";
                }
                return toAlphaSequence(number, hiraganaA);
              case '\u30a2':
                if ((number == 0)) {
                    return "0";
                }
                return toAlphaSequence(number, katakanaA);
              case '\u3044':
                if ((number == 0)) {
                    return "0";
                }
                return toAlphaSequence(number, hiraganaI);
              case '\u30a4':
                if ((number == 0)) {
                    return "0";
                }
                return toAlphaSequence(number, katakanaI);
              case '\u4e00':
                return toJapanese(number);
              default:
                int digitValue = Saxon.Hej.expr.number.Alphanumeric.getDigitValue(formchar);
                if ((digitValue>=0)) {
                    int zero = (formchar-digitValue);
                    int[] digits = new int[10];
                    for (int z = 0; (z<=9); z++) {
                        digits[z] = (zero+z);
                    }
                    return toRadical(number, digits, pictureLength, numGroupFormatter);
                } else {
                    if ((((formchar<'\u1100') && System.Char.IsLetter((char)(formchar))) && (number>0))) {
                        alphaDefault(number, (char)(formchar), sb);
                    } else {
                        sb.append(toRadical(number, westernDigits, pictureLength, numGroupFormatter));
                        if (((ordinal != null) && !(ordinal.Length==0))) {
                            sb.append(ordinalSuffix(ordinal, number));
                        }
                    }
                    break ;
                }
            }
            return sb.ToString();
        }
        protected internal virtual string ordinalSuffix(string ordinalParam, long number) {
            return "";
        }
        protected internal static readonly int[] westernDigits = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        protected internal const string latinUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        protected internal const string latinLower = "abcdefghijklmnopqrstuvwxyz";
        protected internal const string greekUpper = (("\u0391\u0392\u0393\u0394\u0395\u0396\u0397\u0398\u0399\u039a"+"\u039b\u039c\u039d\u039e\u039f\u03a0\u03a1\u03a2\u03a3\u03a4")+"\u03a5\u03a6\u03a7\u03a8\u03a9");
        protected internal const string greekLower = (("\u03b1\u03b2\u03b3\u03b4\u03b5\u03b6\u03b7\u03b8\u03b9\u03ba"+"\u03bb\u03bc\u03bd\u03be\u03bf\u03c0\u03c1\u03c2\u03c3\u03c4")+"\u03c5\u03c6\u03c7\u03c8\u03c9");
        protected internal const string cyrillicUpper = (("\u0410\u0411\u0412\u0413\u0414\u0415\u0416\u0417\u0418"+"\u041a\u041b\u041c\u041d\u041e\u041f\u0420\u0421\u0421\u0423")+"\u0424\u0425\u0426\u0427\u0428\u0429\u042b\u042d\u042e\u042f");
        protected internal const string cyrillicLower = (("\u0430\u0431\u0432\u0433\u0434\u0435\u0436\u0437\u0438"+"\u043a\u043b\u043c\u043d\u043e\u043f\u0440\u0441\u0441\u0443")+"\u0444\u0445\u0446\u0447\u0448\u0449\u044b\u044d\u044e\u044f");
        protected internal const string hebrew = ("\u05d0\u05d1\u05d2\u05d3\u05d4\u05d5\u05d6\u05d7\u05d8\u05d9\u05db\u05dc"+"\u05de\u05e0\u05e1\u05e2\u05e4\u05e6\u05e7\u05e8\u05e9\u05ea");
        protected internal const string hiraganaA = (((("\u3042\u3044\u3046\u3048\u304a\u304b\u304d\u304f\u3051\u3053"+"\u3055\u3057\u3059\u305b\u305d\u305f\u3061\u3064\u3066\u3068")+"\u306a\u306b\u306c\u306d\u306e\u306f\u3072\u3075\u3078\u307b")+"\u307e\u307f\u3080\u3081\u3082\u3084\u3086\u3088\u3089\u308a")+"\u308b\u308c\u308d\u308f\u3092\u3093");
        protected internal const string katakanaA = (((("\u30a2\u30a4\u30a6\u30a8\u30aa\u30ab\u30ad\u30af\u30b1\u30b3"+"\u30b5\u30b7\u30b9\u30bb\u30bd\u30bf\u30c1\u30c4\u30c6\u30c8")+"\u30ca\u30cb\u30cc\u30cd\u30ce\u30cf\u30d2\u30d5\u30d8\u30db")+"\u30de\u30df\u30e0\u30e1\u30e2\u30e4\u30e6\u30e8\u30e9\u30ea")+"\u30eb\u30ec\u30ed\u30ef\u30f2\u30f3");
        protected internal const string hiraganaI = (((("\u3044\u308d\u306f\u306b\u307b\u3078\u3068\u3061\u308a\u306c"+"\u308b\u3092\u308f\u304b\u3088\u305f\u308c\u305d\u3064\u306d")+"\u306a\u3089\u3080\u3046\u3090\u306e\u304a\u304f\u3084\u307e")+"\u3051\u3075\u3053\u3048\u3066\u3042\u3055\u304d\u3086\u3081")+"\u307f\u3057\u3091\u3072\u3082\u305b\u3059");
        protected internal const string katakanaI = (((("\u30a4\u30ed\u30cf\u30cb\u30db\u30d8\u30c8\u30c1\u30ea\u30cc"+"\u30eb\u30f2\u30ef\u30ab\u30e8\u30bf\u30ec\u30bd\u30c4\u30cd")+"\u30ca\u30e9\u30e0\u30a6\u30f0\u30ce\u30aa\u30af\u30e4\u30de")+"\u30b1\u30d5\u30b3\u30a8\u30c6\u30a2\u30b5\u30ad\u30e6\u30e1")+"\u30df\u30b7\u30f1\u30d2\u30e2\u30bb\u30b9");
        protected internal void alphaDefault(long number, char formchar, Saxon.Ejava.lang.StringBuilder sb) {
            int min = formchar;
            int max = formchar;
            while (System.Char.IsLetterOrDigit((char)(((max+1))))) {
                max++;
            }
            sb.append(toAlpha(number, min, max));
        }
        protected internal string toAlpha(long number, int min, int max) {
            if ((number<=0)) {
                return (""+number);
            }
            int range = ((max-min)+1);
            char last = (char)(((((((number-1))%range))+min)));
            if ((number>range)) {
                return (toAlpha((((number-1))/range), min, max)+last);
            } else {
                return (""+last);
            }
        }
        protected internal string toAlphaSequence(long number, string alphabet) {
            if ((number<=0)) {
                return (""+number);
            }
            int range = alphabet.Length;
            char last = alphabet[(int)(((((number-1))%range)))];
            if ((number>range)) {
                return (toAlphaSequence((((number-1))/range), alphabet)+last);
            } else {
                return (""+last);
            }
        }
        private string toRadical(long number, int[] digits, int pictureLength, Saxon.Hej.expr.number.NumericGroupFormatter numGroupFormatter) {
            string temp = convertDigitSystem(number, digits, pictureLength);
            if ((numGroupFormatter == null)) {
                return temp;
            }
            return numGroupFormatter.format(temp);
        }
        public static string convertDigitSystem(long number, int[] digits, int requiredLength) {
            Saxon.Ejava.lang.StringBuilder temp = new Saxon.Ejava.lang.StringBuilder(16);
            int @base = digits.Length;
            Saxon.Ejava.lang.StringBuilder s = new Saxon.Ejava.lang.StringBuilder(16);
            long n = number;
            int count = 0;
            while ((n>0)) {
                int digit = digits[(int)(((n%@base)))];
                Saxon.Hej.str.StringTool.prependWideChar(s, digit);
                count++;
                n = (n/@base);
            }
            for (int i = 0; (i<((requiredLength-count))); i++) {
                temp.appendCodePoint(digits[0]);
            }
            temp.append(s);
            return temp.ToString();
        }
        public static string toRoman(long n) {
            if (((n<=0) || (n>9999))) {
                return (""+n);
            }
            return (((romanThousands[((int)(n)/1000)]+romanHundreds[((((int)(n)/100))%10)])+romanTens[((((int)(n)/10))%10)])+romanUnits[((int)(n)%10)]);
        }
        private static readonly string[] romanThousands = {"", "m", "mm", "mmm", "mmmm", "mmmmm", "mmmmmm", "mmmmmmm", "mmmmmmmm", "mmmmmmmmm"};
        private static readonly string[] romanHundreds = {"", "c", "cc", "ccc", "cd", "d", "dc", "dcc", "dccc", "cm"};
        private static readonly string[] romanTens = {"", "x", "xx", "xxx", "xl", "l", "lx", "lxx", "lxxx", "xc"};
        private static readonly string[] romanUnits = {"", "i", "ii", "iii", "iv", "v", "vi", "vii", "viii", "ix"};
        public string toJapanese(long number) {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(16);
            if ((number == 0)) {
                fsb.appendCodePoint(0x3007);
            } else {
                if ((number<=9999)) {
                    toJapanese((int)(number), fsb, false);
                } else {
                    fsb.append((""+number));
                }
            }
            return fsb.ToString();
        }
        private static void toJapanese(int nr, Saxon.Ejava.lang.StringBuilder fsb, bool isInitial) {
            if ((nr == 0)) {
            } else {
                if ((nr<=9)) {
                    if (!((((nr == 1) && isInitial)))) {
                        fsb.appendCodePoint(kanjiDigits[nr]);
                    }
                } else {
                    if ((nr == 10)) {
                        fsb.appendCodePoint(0x5341);
                    } else {
                        if ((nr<=99)) {
                            toJapanese((nr/10), fsb, true);
                            fsb.appendCodePoint(0x5341);
                            toJapanese((nr%10), fsb, false);
                        } else {
                            if ((nr<=999)) {
                                toJapanese((nr/100), fsb, true);
                                fsb.appendCodePoint(0x767e);
                                toJapanese((nr%100), fsb, false);
                            } else {
                                if ((nr<=9999)) {
                                    toJapanese((nr/1000), fsb, true);
                                    fsb.appendCodePoint(0x5343);
                                    toJapanese((nr%1000), fsb, false);
                                }
                            }
                        }
                    }
                }
            }
        }
        private static readonly int[] kanjiDigits = {0x3007, 0x4e00, 0x4e8c, 0x4e09, 0x56db, 0x4e94, 0x516d, 0x4e03, 0x516b, 0x4e5d};
        public abstract string toWords(string cardinal, long number);
        public string toWords(string cardinal, long number, int wordCase) {
            string s;
            if ((number == 0)) {
                s = zero();
            } else {
                s = toWords(cardinal, number);
            }
            switch (wordCase) {
              case UPPER_CASE:
                return s.ToUpper();
              case LOWER_CASE:
                return s.ToLower();
              default:
                return s;
            }
        }
        public string zero() {
            return "Zero";
        }
        public abstract string toOrdinalWords(string ordinalParam, long number, int wordCase);
        public abstract string monthName(int month, int minWidth, int maxWidth);
        public abstract string dayName(int day, int minWidth, int maxWidth);
        public string halfDayName(int minutes, int minWidth, int maxWidth) {
            string s;
            if ((((minutes == 0) && (maxWidth>=8)) && "gb".Equals(country))) {
                s = "Midnight";
            } else {
                if ((minutes<(12*60))) {
                    switch (maxWidth) {
                      case 1:
                        s = "A";
                        break ;
                      case 2:
                      case 3:
                        s = "Am";
                        break ;
                      default:
                        s = "A.M.";
                        break ;
                    }
                } else {
                    if ((((minutes == (12*60)) && (maxWidth>=8)) && "gb".Equals(country))) {
                        s = "Noon";
                    } else {
                        switch (maxWidth) {
                          case 1:
                            s = "P";
                            break ;
                          case 2:
                          case 3:
                            s = "Pm";
                            break ;
                          default:
                            s = "P.M.";
                            break ;
                        }
                    }
                }
            }
            return s;
        }
        public string getOrdinalSuffixForDateTime(string component) {
            return "yes";
        }
        public string getEraName(int year) {
            return ((year>0) ? "AD" : "BC");
        }
        public string getCalendarName(string code) {
            if (code.Equals("AD")) {
                return "Gregorian";
            } else {
                return code;
            }
        }
    
    }
}