using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.number {
    public class NumberFormatter {
        private System.Collections.Generic.List<Saxon.Hej.str.UnicodeString> formatTokens;
        private System.Collections.Generic.List<Saxon.Hej.str.UnicodeString> punctuationTokens;
        private bool startsWithPunctuation;
        public void prepare(string format) {
            if (format.Length==0) {
                format = "1";
            }
            formatTokens = new System.Collections.Generic.List<Saxon.Hej.str.UnicodeString>(10);
            punctuationTokens = new System.Collections.Generic.List<Saxon.Hej.str.UnicodeString>(10);
            Saxon.Hej.str.UnicodeString uFormat = Saxon.Hej.str.StringView.tidy(format);
            int len = uFormat.length32();
            int i = 0;
            int t;
            bool first = true;
            startsWithPunctuation = true;
            while ((i<len)) {
                int c = uFormat.codePointAt(i);
                t = i;
                while (isLetterOrDigit(c)) {
                    i++;
                    if ((i == len)) {
                        break ;
                    }
                    c = uFormat.codePointAt(i);
                }
                if ((i>t)) {
                    Saxon.Hej.str.UnicodeString tok = uFormat.substring(t, i);
                    formatTokens.Add(tok);
                    if (first) {
                        punctuationTokens.Add(Saxon.Hej.str.BMPString.of("."));
                        startsWithPunctuation = false;
                        first = false;
                    }
                }
                if ((i == len)) {
                    break ;
                }
                t = i;
                c = uFormat.codePointAt(i);
                while (!(isLetterOrDigit(c))) {
                    first = false;
                    i++;
                    if ((i == len)) {
                        break ;
                    }
                    c = uFormat.codePointAt(i);
                }
                if ((i>t)) {
                    Saxon.Hej.str.UnicodeString sep = uFormat.substring(t, i);
                    punctuationTokens.Add(sep);
                }
            }
            if (formatTokens.Count==0) {
                formatTokens.Add(Saxon.Hej.str.BMPString.of("1"));
                if ((punctuationTokens.Count == 1)) {
                    punctuationTokens.Add(punctuationTokens[0]);
                }
            }
        }
        public static bool isLetterOrDigit(int c) {
            if ((c<=0x7F)) {
                return (((((c>=0x30) && (c<=0x39))) || (((c>=0x41) && (c<=0x5A)))) || (((c>=0x61) && (c<=0x7A))));
            } else {
                return alphanumeric.test(c);
            }
        }
        private static readonly Saxon.Hej.z.IntPredicateProxy alphanumeric = Saxon.Hej.z.IntUnionPredicate.makeUnion(Saxon.Hej.regex.charclass.Categories.getCategory("N"), (Saxon.Hej.regex.charclass.Categories.getCategory("L")));
        public Saxon.Hej.str.UnicodeString format(System.Collections.Generic.IList<System.Object> numbers, int groupSize, string groupSeparator, string letterValue, string ordinal, Saxon.Hej.lib.Numberer numberer) {
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder(32);
            int num = 0;
            int tok = 0;
            if (startsWithPunctuation) {
                sb.accept(punctuationTokens[tok]);
            }
            while ((num<numbers.Count)) {
                if ((num>0)) {
                    if (((tok == 0) && startsWithPunctuation)) {
                        sb.append(".");
                    } else {
                        sb.accept(punctuationTokens[tok]);
                    }
                }
                System.Object o = numbers[num++];
                string s;
                if ((o is System.Int64)) {
                    long nr = (System.Int64)(o);
                    Saxon.Hej.expr.number.RegularGroupFormatter rgf = new Saxon.Hej.expr.number.RegularGroupFormatter(groupSize, groupSeparator, Saxon.Hej.str.EmptyUnicodeString.getInstance());
                    s = numberer.format(nr, formatTokens[tok], rgf, letterValue, "", ordinal);
                } else {
                    if ((o is System.Numerics.BigInteger)) {
                        Saxon.Hej.expr.number.RegularGroupFormatter rgf = new Saxon.Hej.expr.number.RegularGroupFormatter(groupSize, groupSeparator, Saxon.Hej.str.EmptyUnicodeString.getInstance());
                        s = rgf.format(o.ToString());
                        s = translateDigits(s, formatTokens[tok]);
                    } else {
                        s = o.ToString();
                    }
                }
                sb.append(s);
                tok++;
                if ((tok == formatTokens.Count)) {
                    tok--;
                }
            }
            if ((punctuationTokens.Count>formatTokens.Count)) {
                sb.accept(punctuationTokens[(punctuationTokens.Count-1)]);
            }
            return sb.toUnicodeString();
        }
        private string translateDigits(string @in, Saxon.Hej.str.UnicodeString picture) {
            if ((picture.length() == 0)) {
                return @in;
            }
            int formchar = picture.codePointAt(0);
            int digitValue = Saxon.Hej.expr.number.Alphanumeric.getDigitValue(formchar);
            if ((digitValue>=0)) {
                int zero = (formchar-digitValue);
                if ((zero == (int)('0'))) {
                    return @in;
                }
                int[] digits = new int[10];
                for (int z = 0; (z<=9); z++) {
                    digits[z] = (zero+z);
                }
                Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(128);
                for (int i = 0; (i<@in.Length); i++) {
                    char c = @in[i];
                    if (((c>='0') && (c<='9'))) {
                        sb.appendCodePoint(digits[(c-'0')]);
                    } else {
                        sb.append(c);
                    }
                }
                return sb.ToString();
            } else {
                return @in;
            }
        }
    
    }
}