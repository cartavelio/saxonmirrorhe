using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class FormatInteger : Saxon.Hej.functions.SystemFunction, Saxon.Hej.functions.StatefulSystemFunction {
        private static readonly Saxon.Hej.regex.RegularExpression badDecimalHashPattern = Saxon.Hej.regex.ARegularExpression.compile("(([\\dXx]+|\\w+)#+.*)|(#+[^\\dXx]+)", "");
        private static readonly Saxon.Hej.regex.RegularExpression modifierPattern = Saxon.Hej.regex.ARegularExpression.compile("([co](\\(.*\\))?)?[at]?", "");
        private static readonly Saxon.Hej.regex.RegularExpression decimalDigitPattern = Saxon.Hej.regex.ARegularExpression.compile("^((\\p{Nd}|#|[^\\p{N}\\p{L}])+?)$", "");
        private static readonly Saxon.Hej.regex.RegularExpression nonDecimalDigitPattern = Saxon.Hej.regex.ARegularExpression.compile("^(([Xx#]|[^\\p{N}\\p{L}])+?)$", "");
        public const string preface = "In the picture string for format-integer, ";
        private System.Func<Saxon.Hej.value.IntegerValue,string> formatter = null;
        public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
            bool opt = true;
            if (!(((arguments[1] is Saxon.Hej.expr.Literal)))) {
                opt = false;
            }
            if (((arguments.Length == 3) && !(((arguments[2] is Saxon.Hej.expr.Literal))))) {
                opt = false;
            }
            if (!(opt)) {
                return base.makeOptimizedFunctionCall(visitor, contextInfo, arguments);
            }
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            string language = ((arguments.Length == 3) ? ((Saxon.Hej.expr.StringLiteral)(arguments[2])).getGroundedValue().getStringValue() : config.getDefaultLanguage());
            Saxon.Hej.lib.Numberer numb = config.makeNumberer(language, null);
            bool allow40 = (visitor.getStaticContext().getPackageData().getHostLanguageVersion()>=40);
            formatter = makeFormatter(numb, ((Saxon.Hej.expr.StringLiteral)(arguments[1])).getGroundedValue().getStringValue(), allow40);
            return base.makeOptimizedFunctionCall(visitor, contextInfo, arguments);
        }
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return formatInteger((Saxon.Hej.value.IntegerValue)(arguments[0].head()), (Saxon.Hej.value.StringValue)(arguments[1].head()), ((arguments.Length == 2) ? null : (Saxon.Hej.value.StringValue)(arguments[2].head())), context);
        }
        private Saxon.Hej.value.StringValue formatInteger(Saxon.Hej.value.IntegerValue num, Saxon.Hej.value.StringValue picture, Saxon.Hej.value.StringValue language, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Configuration config = context.getConfiguration();
            bool allow40 = (getRetainedStaticContext().getPackageData().getHostLanguageVersion()>=40);
            if ((num == null)) {
                return Saxon.Hej.value.StringValue.EMPTY_STRING;
            }
            System.Func<Saxon.Hej.value.IntegerValue,string> localFormatter = formatter;
            if ((localFormatter == null)) {
                string languageVal;
                if ((language != null)) {
                    languageVal = language.getStringValue();
                } else {
                    languageVal = config.getDefaultLanguage();
                }
                Saxon.Hej.lib.Numberer numb = config.makeNumberer(languageVal, null);
                localFormatter = makeFormatter(numb, picture.getStringValue(), allow40);
            }
            try {
                return new Saxon.Hej.value.StringValue(localFormatter(num));
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }
        private System.Func<Saxon.Hej.value.IntegerValue,string> makeFormatter(Saxon.Hej.lib.Numberer numb, string pic, bool allow40) {
            if (pic.Length==0) {
                throw new Saxon.Hej.trans.XPathException((preface+"the picture cannot be empty"), "FODF1310");
            }
            bool hasExplicitRadix = false;
            int radix = 10;
            if ((allow40 && System.Text.RegularExpressions.Regex.IsMatch(pic, "^([2-9]|[12][0-9]|3[0-6])\\^.*[xX].*$"))) {
                int hat = pic.IndexOf('^');
                radix = Saxon.Impl.Helpers.Utils.ParseInt(pic.Substring(0, hat - 0));
                hasExplicitRadix = true;
                pic = pic.Substring((hat+1));
            }
            string primaryToken;
            string modifier;
            string parenthetical;
            int lastSemicolon = pic.LastIndexOf(';');
            if ((lastSemicolon>=0)) {
                primaryToken = pic.Substring(0, lastSemicolon - 0);
                if (primaryToken.Length==0) {
                    throw new Saxon.Hej.trans.XPathException((preface+"the primary format token cannot be empty"), "FODF1310");
                }
                modifier = ((lastSemicolon<(pic.Length-1)) ? pic.Substring((lastSemicolon+1)) : "");
                if (!(modifierPattern.matches(Saxon.Hej.str.StringView.tidy(modifier)))) {
                    throw new Saxon.Hej.trans.XPathException((preface+"the modifier is invalid"), "FODF1310");
                }
            } else {
                primaryToken = pic;
                modifier = "";
            }
            bool cardinal = modifier.StartsWith("c");
            bool ordinal = modifier.StartsWith("o");
            bool alphabetic = modifier.EndsWith("a");
            int leftParen = modifier.IndexOf('(');
            int rightParen = modifier.LastIndexOf(')');
            parenthetical = ((leftParen<0) ? "" : modifier.Substring((leftParen+1), rightParen - (leftParen+1)));
            string letterValue = (alphabetic ? "alphabetic" : "traditional");
            string ordinalValue = (ordinal ? ("".Equals(parenthetical) ? "yes" : parenthetical) : "");
            string cardinalValue = (cardinal ? ("".Equals(parenthetical) ? "yes" : parenthetical) : "");
            Saxon.Hej.str.UnicodeString primary = Saxon.Hej.str.StringView.tidy(primaryToken);
            Saxon.Hej.regex.charclass.Categories.Category isDecimalDigit = Saxon.Hej.regex.charclass.Categories.getCategory("Nd");
            bool isDecimalDigitPattern = false;
            if (hasExplicitRadix) {
                if (!(nonDecimalDigitPattern.matches(primary))) {
                    throw new Saxon.Hej.trans.XPathException(((((preface+"the primary format token with radix ")+radix)+" does not ")+"meet the rules for a non-decimal digit pattern"), "FODF1310");
                }
                letterValue = ((((primary.indexOf('X')>=0) ? "X" : "x"))+radix);
            } else {
                Saxon.Hej.z.IntIterator iter = primary.codePoints();
                while (iter.hasNext()) {
                    if (isDecimalDigit.test(iter.next())) {
                        isDecimalDigitPattern = true;
                        break ;
                    }
                }
                if ((isDecimalDigitPattern && !(decimalDigitPattern.matches(primary)))) {
                    throw new Saxon.Hej.trans.XPathException(((preface+"the primary format token contains a decimal digit but does not ")+"meet the rules for a decimal digit pattern"), "FODF1310");
                }
            }
            if ((isDecimalDigitPattern || hasExplicitRadix)) {
                Saxon.Hej.expr.number.NumericGroupFormatter picGroupFormat = getPicSeparators(primary, hasExplicitRadix);
                Saxon.Hej.str.UnicodeString adjustedPicture = picGroupFormat.getAdjustedPicture();
                string finalLetterValue = letterValue;
                return num => {
                    try {
                        string s = numb.format(num.abs().longValue(), adjustedPicture, picGroupFormat, finalLetterValue, "", ordinalValue);
                        return ((num.signum()<0) ? (("-"+s)) : s);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw new Saxon.Hej.trans.UncheckedXPathException(e);
                    }
                } ;
            } else {
                Saxon.Hej.str.UnicodeString token = Saxon.Hej.str.StringView.tidy(primaryToken);
                string finalLetterValue = letterValue;
                return num => {
                    try {
                        string s = numb.format(num.abs().longValue(), token, null, finalLetterValue, cardinalValue, ordinalValue);
                        return ((num.signum()<0) ? (("-"+s)) : s);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw new Saxon.Hej.trans.UncheckedXPathException(e);
                    }
                } ;
            }
        }
        public static Saxon.Hej.expr.number.NumericGroupFormatter getPicSeparators(Saxon.Hej.str.UnicodeString picExpanded, bool hasExplicitRadix) {
            Saxon.Hej.z.IntSet groupingPositions = new Saxon.Hej.z.IntHashSet(5);
            System.Collections.Generic.IList<int> separatorList = new System.Collections.Generic.List<int>();
            int groupingPosition = 0;
            int firstGroupingPos = 0;
            int lastGroupingPos = 0;
            bool regularCheck = true;
            int zeroDigit = -1;
            if (badDecimalHashPattern.matches(picExpanded)) {
                throw new Saxon.Hej.trans.XPathException((preface+"the picture is not valid (it uses '#' where disallowed)"), "FODF1310");
            }
            for (long i = (picExpanded.length()-1); (i>=0); i--) {
                int codePoint = picExpanded.codePointAt(i);
                switch (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(codePoint)) {
                  case System.Globalization.UnicodeCategory.DecimalDigitNumber:
                    if ((zeroDigit == -1)) {
                        zeroDigit = Saxon.Hej.expr.number.Alphanumeric.getDigitFamily(codePoint);
                    } else {
                        if ((zeroDigit != Saxon.Hej.expr.number.Alphanumeric.getDigitFamily(codePoint))) {
                            throw new Saxon.Hej.trans.XPathException((preface+"the picture mixes digits from different digit families"), "FODF1310");
                        }
                    }
                    groupingPosition++;
                    break ;
                  case System.Globalization.UnicodeCategory.UppercaseLetter:
                  case System.Globalization.UnicodeCategory.LowercaseLetter:
                    if (!(hasExplicitRadix)) {
                        break ;
                    }
                    if (((codePoint == 'x') || (codePoint == 'X'))) {
                        if ((zeroDigit == -1)) {
                            zeroDigit = codePoint;
                        } else {
                            if ((zeroDigit != codePoint)) {
                                throw new Saxon.Hej.trans.XPathException((preface+"the picture mixes upper-case and lower-case non-decimal digits"), "FODF1310");
                            }
                        }
                    } else {
                        throw new Saxon.Hej.trans.XPathException((preface+"non-decimal digits must be indicated by 'x' or 'X'"), "FODF1310");
                    }
                    groupingPosition++;
                    break ;
                  case System.Globalization.UnicodeCategory.LetterNumber:
                  case System.Globalization.UnicodeCategory.OtherNumber:
                  case System.Globalization.UnicodeCategory.ModifierLetter:
                  case System.Globalization.UnicodeCategory.OtherLetter:
                    break ;
                  default:
                    if ((i == (picExpanded.length()-1))) {
                        throw new Saxon.Hej.trans.XPathException((preface+"the picture cannot end with a separator"), "FODF1310");
                    }
                    if ((codePoint == '#')) {
                        groupingPosition++;
                        if ((i != 0)) {
                            switch (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(picExpanded.codePointAt((i-1)))) {
                              case System.Globalization.UnicodeCategory.DecimalDigitNumber:
                              case System.Globalization.UnicodeCategory.LetterNumber:
                              case System.Globalization.UnicodeCategory.OtherNumber:
                              case System.Globalization.UnicodeCategory.UppercaseLetter:
                              case System.Globalization.UnicodeCategory.LowercaseLetter:
                              case System.Globalization.UnicodeCategory.ModifierLetter:
                              case System.Globalization.UnicodeCategory.OtherLetter:
                                throw new Saxon.Hej.trans.XPathException((preface+"the picture cannot contain alphanumeric character(s) before character '#'"), "FODF1310");
                            }
                        }
                        break ;
                    } else {
                        bool added = groupingPositions.add(groupingPosition);
                        if (!(added)) {
                            throw new Saxon.Hej.trans.XPathException((preface+"the picture contains consecutive separators"), "FODF1310");
                        }
                        separatorList.Add(codePoint);
                        if ((groupingPositions.size() == 1)) {
                            firstGroupingPos = groupingPosition;
                        } else {
                            if ((groupingPosition != (firstGroupingPos*groupingPositions.size()))) {
                                regularCheck = false;
                            }
                            if ((separatorList[0] != codePoint)) {
                                regularCheck = false;
                            }
                        }
                        if ((i == 0)) {
                            throw new Saxon.Hej.trans.XPathException((preface+"the picture cannot begin with a separator"), "FODF1310");
                        }
                        lastGroupingPos = groupingPosition;
                    }
                    break ;
                }
            }
            if ((regularCheck && (groupingPositions.size()>=1))) {
                if ((((picExpanded.length()-lastGroupingPos)-groupingPositions.size())>firstGroupingPos)) {
                    regularCheck = false;
                }
            }
            Saxon.Hej.str.UnicodeString adjustedPic = extractSeparators(picExpanded, groupingPositions);
            if (groupingPositions.isEmpty()) {
                return new Saxon.Hej.expr.number.RegularGroupFormatter(0, "", adjustedPic);
            }
            if (regularCheck) {
                if (separatorList.Count==0) {
                    return new Saxon.Hej.expr.number.RegularGroupFormatter(0, "", adjustedPic);
                } else {
                    Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(4);
                    sb.appendCodePoint(separatorList[0]);
                    return new Saxon.Hej.expr.number.RegularGroupFormatter(firstGroupingPos, sb.ToString(), adjustedPic);
                }
            } else {
                return new Saxon.Hej.expr.number.IrregularGroupFormatter(groupingPositions, separatorList, adjustedPic);
            }
        }
        private static Saxon.Hej.str.UnicodeString extractSeparators(Saxon.Hej.str.UnicodeString arr, Saxon.Hej.z.IntSet excludePositions) {
            Saxon.Hej.str.UnicodeBuilder ub = new Saxon.Hej.str.UnicodeBuilder(arr.length32());
            Saxon.Hej.z.IntIterator iter = arr.codePoints();
            while (iter.hasNext()) {
                int c = iter.next();
                if (Saxon.Hej.expr.number.NumberFormatter.isLetterOrDigit(c)) {
                    ub.append(c);
                }
            }
            return ub.toUnicodeString();
        }
        public Saxon.Hej.functions.SystemFunction copy() {
            Saxon.Hej.functions.FormatInteger fi2 = (Saxon.Hej.functions.FormatInteger)(Saxon.Hej.functions.SystemFunction.makeFunction(getFunctionName().getLocalPart(), getRetainedStaticContext(), getArity()));
            fi2.formatter = formatter;
            return fi2;
        }
    
    }
}