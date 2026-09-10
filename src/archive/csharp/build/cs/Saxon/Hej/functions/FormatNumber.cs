using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class FormatNumber : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable, Saxon.Hej.functions.StatefulSystemFunction {
        private Saxon.Hej.om.StructuredQName decimalFormatName;
        private string picture;
        private Saxon.Hej.trans.DecimalSymbols decimalSymbols;
        private Saxon.Hej.functions.FormatNumber.SubPicture[] subPictures;
        public override Saxon.Hej.expr.Expression fixArguments(params Saxon.Hej.expr.Expression[]  arguments) {
            if (((arguments[1] is Saxon.Hej.expr.Literal) && (((arguments.Length == 2) || (arguments[2] is Saxon.Hej.expr.StringLiteral))))) {
                Saxon.Hej.trans.DecimalFormatManager dfm = getRetainedStaticContext().getDecimalFormatManager();
                System.Diagnostics.Debug.Assert((dfm != null));
                picture = ((Saxon.Hej.expr.StringLiteral)(arguments[1])).stringify();
                if (((arguments.Length == 3) && !(Saxon.Hej.expr.Literal.isEmptySequence(arguments[2])))) {
                    try {
                        string lexicalName = ((Saxon.Hej.expr.StringLiteral)(arguments[2])).stringify();
                        decimalFormatName = Saxon.Hej.om.StructuredQName.fromLexicalQName(lexicalName, false, true, getRetainedStaticContext());
                    } catch (Saxon.Hej.trans.XPathException e) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Invalid decimal format name. "+e.Message));
                        err.setErrorCode("FODF1280");
                        throw err;
                    }
                }
                if ((decimalFormatName == null)) {
                    decimalSymbols = dfm.getDefaultDecimalFormat();
                } else {
                    decimalSymbols = dfm.getNamedDecimalFormat(decimalFormatName);
                    if ((decimalSymbols == null)) {
                        throw new Saxon.Hej.trans.XPathException((("Decimal format "+decimalFormatName.getDisplayName())+" has not been defined"), "FODF1280");
                    }
                }
                subPictures = getSubPictures(picture, decimalSymbols);
            }
            return null;
        }
        private static Saxon.Hej.functions.FormatNumber.SubPicture[] getSubPictures(string picture, Saxon.Hej.trans.DecimalSymbols dfs) {
            int[] picture4 = Saxon.Hej.str.StringTool.expand(Saxon.Hej.str.StringView.of(picture));
            Saxon.Hej.functions.FormatNumber.SubPicture[] pics = new Saxon.Hej.functions.FormatNumber.SubPicture[2];
            if ((picture4.Length == 0)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("format-number() picture is zero-length");
                err.setErrorCode("FODF1310");
                throw err;
            }
            int sep = -1;
            for (int c = 0; (c<picture4.Length); c++) {
                if ((picture4[c] == dfs.getPatternSeparator())) {
                    if ((c == 0)) {
                        grumble("first subpicture is zero-length");
                    } else {
                        if ((sep>=0)) {
                            grumble("more than one pattern separator");
                        } else {
                            if ((sep == (picture4.Length-1))) {
                                grumble("second subpicture is zero-length");
                            }
                        }
                    }
                    sep = c;
                }
            }
            if ((sep<0)) {
                pics[0] = makeSubPicture(picture4, dfs);
                pics[1] = null;
            } else {
                int[] pic0 = new int[sep];
                Saxon.Ejava.lang.System.arraycopy(picture4, 0, pic0, 0, sep);
                int[] pic1 = new int[((picture4.Length-sep)-1)];
                Saxon.Ejava.lang.System.arraycopy(picture4, (sep+1), pic1, 0, ((picture4.Length-sep)-1));
                pics[0] = makeSubPicture(pic0, dfs);
                pics[1] = makeSubPicture(pic1, dfs);
            }
            return pics;
        }
        protected internal static Saxon.Hej.functions.FormatNumber.SubPicture makeSubPicture(int[] details, Saxon.Hej.trans.DecimalSymbols dfs) {
            return new Saxon.Impl.Overrides.FormatNumberSubPicture(details, dfs);
        }
        private static string formatNumber(Saxon.Hej.value.NumericValue number, Saxon.Hej.functions.FormatNumber.SubPicture[] subPictures, Saxon.Hej.trans.DecimalSymbols dfs) {
            Saxon.Hej.value.NumericValue absN = number;
            Saxon.Hej.functions.FormatNumber.SubPicture pic;
            string minusSign = "";
            int signum = number.signum();
            if (((signum == 0) && number.isNegativeZero())) {
                signum = -1;
            }
            if ((signum<0)) {
                absN = number.negate();
                if ((subPictures[1] == null)) {
                    pic = subPictures[0];
                    minusSign = stringFromCodepoint(dfs.getMinusSign());
                } else {
                    pic = subPictures[1];
                }
            } else {
                pic = subPictures[0];
            }
            return pic.format(absN, dfs, minusSign);
        }
        private static string stringFromCodepoint(int codepoint) {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder();
            sb.appendCodePoint(codepoint);
            return sb.ToString();
        }
        private static void grumble(string s) {
            throw new Saxon.Hej.trans.XPathException(("format-number picture: "+s), "FODF1310");
        }
        public static Singulink.Numerics.BigDecimal adjustToDecimal(double value, int precision) {
            return Singulink.Numerics.BigDecimal.FromDouble(value, Singulink.Numerics.FloatConversion.Truncate);
        }
        public class SubPicture {
            protected internal int minWholePartSize = 0;
            protected internal int maxWholePartSize = 0;
            protected internal int minFractionPartSize = 0;
            protected internal int maxFractionPartSize = 0;
            protected internal int minExponentSize = 0;
            protected internal int scalingFactor = 0;
            protected internal bool isPercent = false;
            protected internal bool isPerMille = false;
            protected internal string prefix = "";
            protected internal string suffix = "";
            protected internal int[] wholePartGroupingPositions = null;
            protected internal int[] fractionalPartGroupingPositions = null;
            protected internal bool regular;
            protected internal bool is31 = false;
            public SubPicture(int[] pic, Saxon.Hej.trans.DecimalSymbols dfs)  {
                is31 = true;
                int percentSign = dfs.getPercent();
                int perMilleSign = dfs.getPerMille();
                int decimalSeparator = dfs.getDecimalSeparator();
                int groupingSeparator = dfs.getGroupingSeparator();
                int digitSign = dfs.getDigit();
                int zeroDigit = dfs.getZeroDigit();
                int exponentSeparator = dfs.getExponentSeparator();
                Saxon.Ejava.lang.StringBuilder prefixBuilder = new Saxon.Ejava.lang.StringBuilder(8);
                Saxon.Ejava.lang.StringBuilder suffixBuilder = new Saxon.Ejava.lang.StringBuilder(8);
                System.Collections.Generic.IList<int> wholePartPositions = null;
                System.Collections.Generic.IList<int> fractionalPartPositions = null;
                bool foundDigit = false;
                bool foundDecimalSeparator = false;
                bool foundExponentSeparator = false;
                bool foundExponentSeparator2 = false;
                foreach (int ch in pic) {
                    if ((((ch == digitSign) || (ch == zeroDigit)) || isInDigitFamily(ch, zeroDigit))) {
                        foundDigit = true;
                        break ;
                    }
                }
                if (!(foundDigit)) {
                    grumble("subpicture contains no digit or zero-digit sign");
                }
                int phase = 0;
                foreach (int c in pic) {
                    if (((c == percentSign) || (c == perMilleSign))) {
                        if ((isPercent || isPerMille)) {
                            grumble("Cannot have more than one percent or per-mille character in a sub-picture");
                        }
                        isPercent = (c == percentSign);
                        isPerMille = (c == perMilleSign);
                        switch (phase) {
                          case 0:
                            prefixBuilder.appendCodePoint(c);
                            break ;
                          case 1:
                          case 2:
                          case 3:
                          case 4:
                          case 5:
                            if (foundExponentSeparator) {
                                grumble("Cannot have exponent-separator as well as percent or per-mille character in a sub-picture");
                            }
                            goto case 6;
                          case 6:
                            phase = 6;
                            suffixBuilder.appendCodePoint(c);
                            break ;
                        }
                    } else {
                        if ((c == digitSign)) {
                            switch (phase) {
                              case 0:
                              case 1:
                                phase = 1;
                                maxWholePartSize++;
                                break ;
                              case 2:
                                grumble("Digit sign must not appear after a zero-digit sign in the integer part of a sub-picture");
                                break ;
                              case 3:
                              case 4:
                                phase = 4;
                                maxFractionPartSize++;
                                break ;
                              case 5:
                                grumble("Digit sign must not appear in the exponent part of a sub-picture");
                                break ;
                              case 6:
                                if (foundExponentSeparator2) {
                                    grumble("There must only be one exponent separator in a sub-picture");
                                } else {
                                    grumble("Passive character must not appear between active characters in a sub-picture");
                                }
                                break ;
                            }
                        } else {
                            if (((c == zeroDigit) || isInDigitFamily(c, zeroDigit))) {
                                switch (phase) {
                                  case 0:
                                  case 1:
                                  case 2:
                                    phase = 2;
                                    minWholePartSize++;
                                    maxWholePartSize++;
                                    break ;
                                  case 3:
                                    minFractionPartSize++;
                                    maxFractionPartSize++;
                                    break ;
                                  case 4:
                                    grumble("Zero digit sign must not appear after a digit sign in the fractional part of a sub-picture");
                                    break ;
                                  case 5:
                                    minExponentSize++;
                                    break ;
                                  case 6:
                                    if (foundExponentSeparator2) {
                                        grumble("There must only be one exponent separator in a sub-picture");
                                    } else {
                                        grumble("Passive character must not appear between active characters in a sub-picture");
                                    }
                                    break ;
                                }
                            } else {
                                if ((c == decimalSeparator)) {
                                    if (foundDecimalSeparator) {
                                        grumble("There must only be one decimal separator in a sub-picture");
                                    }
                                    switch (phase) {
                                      case 0:
                                      case 1:
                                      case 2:
                                        phase = 3;
                                        foundDecimalSeparator = true;
                                        break ;
                                      case 3:
                                      case 4:
                                      case 5:
                                        if (foundExponentSeparator) {
                                            grumble("Decimal separator must not appear in the exponent part of a sub-picture");
                                        }
                                        break ;
                                      case 6:
                                        grumble("Decimal separator cannot come after a character in the suffix");
                                        break ;
                                    }
                                } else {
                                    if ((c == groupingSeparator)) {
                                        switch (phase) {
                                          case 0:
                                          case 1:
                                          case 2:
                                            if ((wholePartPositions == null)) {
                                                wholePartPositions = new System.Collections.Generic.List<int>(3);
                                            }
                                            if (wholePartPositions.Contains(maxWholePartSize)) {
                                                grumble("Sub-picture cannot contain adjacent grouping separators");
                                            }
                                            wholePartPositions.Add(maxWholePartSize);
                                            break ;
                                          case 3:
                                          case 4:
                                            if ((maxFractionPartSize == 0)) {
                                                grumble("Grouping separator cannot be adjacent to decimal separator");
                                            }
                                            if ((fractionalPartPositions == null)) {
                                                fractionalPartPositions = new System.Collections.Generic.List<int>(3);
                                            }
                                            if (fractionalPartPositions.Contains(maxFractionPartSize)) {
                                                grumble("Sub-picture cannot contain adjacent grouping separators");
                                            }
                                            fractionalPartPositions.Add(maxFractionPartSize);
                                            break ;
                                          case 5:
                                            if (foundExponentSeparator) {
                                                grumble("Grouping separator must not appear in the exponent part of a sub-picture");
                                            }
                                            break ;
                                          case 6:
                                            grumble("Grouping separator found in suffix of sub-picture");
                                            break ;
                                        }
                                    } else {
                                        if ((c == exponentSeparator)) {
                                            switch (phase) {
                                              case 0:
                                                prefixBuilder.appendCodePoint(c);
                                                break ;
                                              case 1:
                                              case 2:
                                              case 3:
                                              case 4:
                                                phase = 5;
                                                foundExponentSeparator = true;
                                                break ;
                                              case 5:
                                                if (foundExponentSeparator) {
                                                    foundExponentSeparator2 = true;
                                                    phase = 6;
                                                    suffixBuilder.appendCodePoint(exponentSeparator);
                                                }
                                                break ;
                                              case 6:
                                                suffixBuilder.appendCodePoint(c);
                                                break ;
                                            }
                                        } else {
                                            switch (phase) {
                                              case 0:
                                                prefixBuilder.appendCodePoint(c);
                                                break ;
                                              case 1:
                                              case 2:
                                              case 3:
                                              case 4:
                                              case 5:
                                                if (((minExponentSize == 0) && foundExponentSeparator)) {
                                                    phase = 6;
                                                    suffixBuilder.appendCodePoint(exponentSeparator);
                                                    suffixBuilder.appendCodePoint(c);
                                                    break ;
                                                }
                                                goto case 6;
                                              case 6:
                                                phase = 6;
                                                suffixBuilder.appendCodePoint(c);
                                                break ;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                prefix = prefixBuilder.ToString();
                suffix = suffixBuilder.ToString();
                scalingFactor = minWholePartSize;
                if (((maxWholePartSize == 0) && (maxFractionPartSize == 0))) {
                    grumble("Mantissa contains no digit or zero-digit sign");
                }
                if (((minWholePartSize == 0) && (maxFractionPartSize == 0))) {
                    if ((minExponentSize != 0)) {
                        minFractionPartSize = 1;
                        maxFractionPartSize = 1;
                    } else {
                        minWholePartSize = 1;
                    }
                }
                if ((((minExponentSize != 0) && (minWholePartSize == 0)) && (maxWholePartSize != 0))) {
                    minWholePartSize = 1;
                }
                if (((minWholePartSize == 0) && (minFractionPartSize == 0))) {
                    minFractionPartSize = 1;
                }
                if ((wholePartPositions != null)) {
                    int n = wholePartPositions.Count;
                    wholePartGroupingPositions = new int[n];
                    for (int i = 0; (i<n); i++) {
                        wholePartGroupingPositions[i] = (maxWholePartSize-wholePartPositions[((n-i)-1)]);
                    }
                    if ((n == 1)) {
                        regular = ((wholePartGroupingPositions[0]*2)>=maxWholePartSize);
                    } else {
                        if ((n>1)) {
                            regular = true;
                            int first = wholePartGroupingPositions[0];
                            for (int i = 1; (i<n); i++) {
                                if ((wholePartGroupingPositions[i] != (((i+1))*first))) {
                                    regular = false;
                                    break ;
                                }
                            }
                            if ((regular && (((maxWholePartSize-wholePartGroupingPositions[(n-1)])>first)))) {
                                regular = false;
                            }
                            if (regular) {
                                wholePartGroupingPositions = new int[1];
                                wholePartGroupingPositions[0] = first;
                            }
                        }
                    }
                    if ((wholePartGroupingPositions[0] == 0)) {
                        grumble("Cannot have a grouping separator at the end of the integer part");
                    }
                }
                if ((fractionalPartPositions != null)) {
                    int n = fractionalPartPositions.Count;
                    fractionalPartGroupingPositions = new int[n];
                    for (int i = 0; (i<n); i++) {
                        fractionalPartGroupingPositions[i] = fractionalPartPositions[i];
                    }
                }
            }
            public string format(Saxon.Hej.value.NumericValue value, Saxon.Hej.trans.DecimalSymbols dfs, string minusSign) {
                if (value.isNaN()) {
                    return dfs.getNaN();
                }
                int multiplier = 1;
                if (isPercent) {
                    multiplier = 100;
                } else {
                    if (isPerMille) {
                        multiplier = 1000;
                    }
                }
                if ((multiplier != 1)) {
                    try {
                        value = (Saxon.Hej.value.NumericValue)(Saxon.Hej.expr.ArithmeticExpression.compute(value, Saxon.Hej.expr.Calculator.TIMES, new Saxon.Hej.value.Int64Value(multiplier), null));
                    } catch (Saxon.Hej.trans.XPathException e) {
                        value = new Saxon.Hej.value.DoubleValue(System.Double.PositiveInfinity);
                    }
                }
                if (((((value is Saxon.Hej.value.DoubleValue) || (value is Saxon.Hej.value.FloatValue))) && System.Double.IsInfinity(value.getDoubleValue()))) {
                    return (((minusSign+prefix)+dfs.getInfinity())+suffix);
                }
                Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(16);
                if (((value is Saxon.Hej.value.DoubleValue) || (value is Saxon.Hej.value.FloatValue))) {
                    Singulink.Numerics.BigDecimal dec = adjustToDecimal(value.getDoubleValue(), 2);
                    formatDecimal(dec, sb);
                } else {
                    if ((value is Saxon.Hej.value.IntegerValue)) {
                        if ((minExponentSize != 0)) {
                            formatDecimal(((Saxon.Hej.value.IntegerValue)(value)).getDecimalValue(), sb);
                        } else {
                            formatInteger(value, sb);
                        }
                    } else {
                        if ((value is Saxon.Hej.value.BigDecimalValue)) {
                            formatDecimal(((Saxon.Hej.value.BigDecimalValue)(value)).getDecimalValue(), sb);
                        }
                    }
                }
                string raw = sb.ToString();
                int[] ib = Saxon.Hej.str.StringTool.expand(Saxon.Hej.str.StringView.of(raw));
                int ibused = ib.Length;
                int point = raw.IndexOf('.');
                if ((point == -1)) {
                    point = raw.Length;
                } else {
                    ib[point] = dfs.getDecimalSeparator();
                    if ((maxFractionPartSize == 0)) {
                        ibused--;
                    }
                }
                if ((dfs.getZeroDigit() != '0')) {
                    int newZero = dfs.getZeroDigit();
                    for (int i = 0; (i<ibused); i++) {
                        int c = ib[i];
                        if (((c>='0') && (c<='9'))) {
                            ib[i] = ((c-'0')+newZero);
                        }
                    }
                }
                if ((dfs.getExponentSeparator() != 'e')) {
                    int expS = raw.IndexOf('e');
                    if ((expS != -1)) {
                        ib[expS] = dfs.getExponentSeparator();
                    }
                }
                if ((wholePartGroupingPositions != null)) {
                    if (regular) {
                        int g = wholePartGroupingPositions[0];
                        int p = (point-g);
                        while ((p>0)) {
                            ib = insert(ib, ibused++, dfs.getGroupingSeparator(), p);
                            p -= g;
                        }
                    } else {
                        foreach (int wholePartGroupingPosition in wholePartGroupingPositions) {
                            int p = (point-wholePartGroupingPosition);
                            if ((p>0)) {
                                ib = insert(ib, ibused++, dfs.getGroupingSeparator(), p);
                            }
                        }
                    }
                }
                if ((fractionalPartGroupingPositions != null)) {
                    for (int i = 0; (i<fractionalPartGroupingPositions.Length); i++) {
                        int p = (((point+1)+fractionalPartGroupingPositions[i])+i);
                        if ((p<ibused)) {
                            ib = insert(ib, ibused++, dfs.getGroupingSeparator(), p);
                        } else {
                            break ;
                        }
                    }
                }
                Saxon.Ejava.lang.StringBuilder res = new Saxon.Ejava.lang.StringBuilder((((prefix.Length+minusSign.Length)+suffix.Length)+ibused));
                res.append(minusSign);
                res.append(prefix);
                for (int i = 0; (i<ibused); i++) {
                    res.appendCodePoint(ib[i]);
                }
                res.append(suffix);
                return res.ToString();
            }
            protected virtual  /* REPLACING private */ void formatDecimal(Singulink.Numerics.BigDecimal dval, Saxon.Ejava.lang.StringBuilder fsb) {
            }
            private void formatInteger(Saxon.Hej.value.NumericValue value, Saxon.Ejava.lang.StringBuilder sb) {
                if (!((((minWholePartSize == 0) && (value.compareTo(0) == 0))))) {
                    sb.append(value.getUnicodeStringValue());
                    int leadingZeroes = (minWholePartSize-sb.length());
                    if ((leadingZeroes>0)) {
                        Saxon.Hej.str.StringTool.prependRepeated(sb, '0', leadingZeroes);
                    }
                }
                if ((minFractionPartSize != 0)) {
                    sb.append('.');
                    for (int i = 0; (i<minFractionPartSize); i++) {
                        sb.append('0');
                    }
                }
            }
        
        }
        private static int[] insert(int[] array, int used, int value, int position) {
            if (((used+1)>array.Length)) {
                array = Saxon.Ejava.util.Arrays.copyOf(array, (used+10));
            }
            Saxon.Ejava.lang.System.arraycopy(array, position, array, (position+1), (used-position));
            array[position] = value;
            return array;
        }
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            int numArgs = arguments.Length;
            Saxon.Hej.trans.DecimalFormatManager dfm = getRetainedStaticContext().getDecimalFormatManager();
            Saxon.Hej.trans.DecimalSymbols dfs;
            Saxon.Hej.value.AtomicValue av0 = (Saxon.Hej.value.AtomicValue)(arguments[0].head());
            if ((av0 == null)) {
                av0 = Saxon.Hej.value.DoubleValue.NaN;
            }
            Saxon.Hej.value.NumericValue number = (Saxon.Hej.value.NumericValue)(av0);
            if ((picture != null)) {
                string result = formatNumber(number, subPictures, decimalSymbols);
                return new Saxon.Hej.value.StringValue(result);
            } else {
                if ((numArgs == 2)) {
                    dfs = dfm.getDefaultDecimalFormat();
                } else {
                    Saxon.Hej.om.Item arg2 = arguments[2].head();
                    if ((arg2 == null)) {
                        dfs = dfm.getDefaultDecimalFormat();
                    } else {
                        string lexicalName = arg2.getUnicodeStringValue().ToString();
                        dfs = getNamedDecimalFormat(dfm, lexicalName);
                    }
                }
                string format = arguments[1].head().getUnicodeStringValue().ToString();
                Saxon.Hej.functions.FormatNumber.SubPicture[] pics = getSubPictures(format, dfs);
                return new Saxon.Hej.value.StringValue(formatNumber(number, pics, dfs));
            }
        }
        protected internal Saxon.Hej.trans.DecimalSymbols getNamedDecimalFormat(Saxon.Hej.trans.DecimalFormatManager dfm, string lexicalName) {
            Saxon.Hej.trans.DecimalSymbols dfs;
            Saxon.Hej.om.StructuredQName qName;
            try {
                qName = Saxon.Hej.om.StructuredQName.fromLexicalQName(lexicalName, false, true, getRetainedStaticContext());
            } catch (Saxon.Hej.trans.XPathException e) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Invalid decimal format name. "+e.Message));
                err.setErrorCode("FODF1280");
                throw err;
            }
            dfs = dfm.getNamedDecimalFormat(qName);
            if ((dfs == null)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("format-number function: decimal-format '"+lexicalName)+"' is not defined"));
                err.setErrorCode("FODF1280");
                throw err;
            }
            return dfs;
        }
        private static bool isInDigitFamily(int ch, int zeroDigit) {
            return ((ch>=zeroDigit) && (ch<(zeroDigit+10)));
        }
        public static string formatExponential(Saxon.Hej.value.DoubleValue value) {
            try {
                Saxon.Hej.trans.DecimalSymbols dfs = new Saxon.Hej.trans.DecimalSymbols(Saxon.Api.HostLanguage.XSLT, 31);
                dfs.setInfinity("INF");
                Saxon.Hej.functions.FormatNumber.SubPicture[] pics = getSubPictures("0.0##########################e0", dfs);
                return formatNumber(value, pics, dfs);
            } catch (Saxon.Hej.trans.XPathException e) {
                return value.getStringValue();
            }
        }
        public Saxon.Hej.functions.SystemFunction copy() {
            Saxon.Hej.functions.FormatNumber copy = (Saxon.Hej.functions.FormatNumber)(Saxon.Hej.functions.SystemFunction.makeFunction(getFunctionName().getLocalPart(), getRetainedStaticContext(), getArity()));
            copy.decimalFormatName = decimalFormatName;
            copy.picture = picture;
            copy.decimalSymbols = decimalSymbols;
            copy.subPictures = subPictures;
            return copy;
        }
        public static System.Func<System.Double,string> getFormatter(string picture) {
            Saxon.Hej.trans.DecimalSymbols symbols = new Saxon.Hej.trans.DecimalSymbols(Saxon.Api.HostLanguage.XSLT, 30);
            Saxon.Hej.functions.FormatNumber.SubPicture[] subPictures = getSubPictures(picture, symbols);
            return dbl => formatNumber(new Saxon.Hej.value.DoubleValue(dbl), subPictures, symbols);
        }
    
    }
}