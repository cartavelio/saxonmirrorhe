using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class DecimalSymbols {
        public const int DECIMAL_SEPARATOR = 0;
        public const int GROUPING_SEPARATOR = 1;
        public const int DIGIT = 2;
        public const int MINUS_SIGN = 3;
        public const int PERCENT = 4;
        public const int PER_MILLE = 5;
        public const int ZERO_DIGIT = 6;
        public const int EXPONENT_SEPARATOR = 7;
        public const int PATTERN_SEPARATOR = 8;
        public const int INFINITY = 9;
        public const int NAN = 10;
        private const int ERR_NOT_SINGLE_CHAR = 0;
        private const int ERR_NOT_UNICODE_DIGIT = 1;
        private const int ERR_SAME_CHAR_IN_TWO_ROLES = 2;
        private const int ERR_TWO_VALUES_FOR_SAME_PROPERTY = 3;
        private static readonly string[] XSLT_CODES = {"XTSE0020", "XTSE1295", "XTSE1300", "XTSE1290"};
        private static readonly string[] XQUERY_CODES = {"XQST0097", "XQST0097", "XQST0098", "XQST0114"};
        private string[] errorCodes = XSLT_CODES;
        private string infinityValue;
        private string NaNValue;
        public readonly static string[] propertyNames = {"decimal-separator", "grouping-separator", "digit", "minus-sign", "percent", "per-mille", "zero-digit", "exponent-separator", "pattern-separator", "infinity", "NaN"};
        private readonly int[] intValues = new int[(propertyNames.Length-2)];
        private readonly int[] precedences = new int[propertyNames.Length];
        private readonly bool[] inconsistent = new bool[propertyNames.Length];
        public DecimalSymbols(Saxon.Api.HostLanguage language, int languageLevel)  {
            intValues[DECIMAL_SEPARATOR] = '.';
            intValues[GROUPING_SEPARATOR] = ',';
            intValues[DIGIT] = '#';
            intValues[MINUS_SIGN] = '-';
            intValues[PERCENT] = '%';
            intValues[PER_MILLE] = '\u2030';
            intValues[ZERO_DIGIT] = '0';
            intValues[EXPONENT_SEPARATOR] = 'e';
            intValues[PATTERN_SEPARATOR] = ';';
            infinityValue = "Infinity";
            NaNValue = "NaN";
            Saxon.Ejava.util.Arrays.fill(precedences, System.Int32.MinValue);
            setHostLanguage(language, languageLevel);
        }
        public void setHostLanguage(Saxon.Api.HostLanguage language, int languageLevel) {
            if ((language == Saxon.Api.HostLanguage.XQUERY)) {
                errorCodes = XQUERY_CODES;
            } else {
                errorCodes = XSLT_CODES;
            }
        }
        public int getDecimalSeparator() {
            return intValues[DECIMAL_SEPARATOR];
        }
        public int getGroupingSeparator() {
            return intValues[GROUPING_SEPARATOR];
        }
        public int getDigit() {
            return intValues[DIGIT];
        }
        public int getMinusSign() {
            return intValues[MINUS_SIGN];
        }
        public int getPercent() {
            return intValues[PERCENT];
        }
        public int getPerMille() {
            return intValues[PER_MILLE];
        }
        public int getZeroDigit() {
            return intValues[ZERO_DIGIT];
        }
        public int getExponentSeparator() {
            return intValues[EXPONENT_SEPARATOR];
        }
        public int getPatternSeparator() {
            return intValues[PATTERN_SEPARATOR];
        }
        public string getInfinity() {
            return infinityValue;
        }
        public string getNaN() {
            return NaNValue;
        }
        public void setDecimalSeparator(string value) {
            setProperty(DECIMAL_SEPARATOR, value, 0);
        }
        public void setGroupingSeparator(string value) {
            setProperty(GROUPING_SEPARATOR, value, 0);
        }
        public void setDigit(string value) {
            setProperty(DIGIT, value, 0);
        }
        public void setMinusSign(string value) {
            setProperty(MINUS_SIGN, value, 0);
        }
        public void setPercent(string value) {
            setProperty(PERCENT, value, 0);
        }
        public void setPerMille(string value) {
            setProperty(PER_MILLE, value, 0);
        }
        public void setZeroDigit(string value) {
            setProperty(ZERO_DIGIT, value, 0);
        }
        public void setExponentSeparator(string value) {
            setProperty(EXPONENT_SEPARATOR, value, 0);
        }
        public void setPatternSeparator(string value) {
            setProperty(PATTERN_SEPARATOR, value, 0);
        }
        public void setInfinity(string value) {
            setProperty(INFINITY, value, 0);
        }
        public void setNaN(string value) {
            setProperty(NAN, value, 0);
        }
        public void setProperty(int key, string value, int precedence) {
            string name = propertyNames[key];
            if ((key<=PATTERN_SEPARATOR)) {
                int intValue = singleChar(name, value);
                if ((precedence>precedences[key])) {
                    intValues[key] = intValue;
                    precedences[key] = precedence;
                    inconsistent[key] = false;
                } else {
                    if ((precedence == precedences[key])) {
                        if ((intValue != intValues[key])) {
                            inconsistent[key] = true;
                        }
                    } else {
                    }
                }
                if (((key == ZERO_DIGIT) && !(isValidZeroDigit(intValue)))) {
                    throw new Saxon.Hej.trans.XPathException("The value of the zero-digit attribute must be a Unicode digit with value zero", errorCodes[ERR_NOT_UNICODE_DIGIT]);
                }
            } else {
                if ((key == INFINITY)) {
                    if ((precedence>precedences[key])) {
                        infinityValue = value;
                        precedences[key] = precedence;
                        inconsistent[key] = false;
                    } else {
                        if ((precedence == precedences[key])) {
                            if (!(infinityValue.Equals(value))) {
                                inconsistent[key] = true;
                            }
                        }
                    }
                } else {
                    if ((key == NAN)) {
                        if ((precedence>precedences[key])) {
                            NaNValue = value;
                            precedences[key] = precedence;
                            inconsistent[key] = false;
                        } else {
                            if ((precedence == precedences[key])) {
                                if (!(NaNValue.Equals(value))) {
                                    inconsistent[key] = false;
                                }
                            }
                        }
                    } else {
                        throw new Saxon.Ejava.lang.IllegalArgumentException();
                    }
                }
            }
        }
        public void setIntProperty(string name, int value) {
            for (int i = 0; (i<propertyNames.Length); i++) {
                if (propertyNames[i].Equals(name)) {
                    intValues[i] = value;
                }
            }
        }
        public void export(Saxon.Hej.om.StructuredQName name, Saxon.Hej.trace.ExpressionPresenter @out) {
            Saxon.Hej.trans.DecimalSymbols defaultSymbols = new Saxon.Hej.trans.DecimalSymbols(Saxon.Api.HostLanguage.XSLT, 31);
            @out.startElement("decimalFormat");
            if ((name != null)) {
                @out.emitAttribute("name", name);
            }
            for (int i = 0; (i<intValues.Length); i++) {
                int propValue = intValues[i];
                if ((propValue != defaultSymbols.intValues[i])) {
                    @out.emitAttribute(propertyNames[i], (propValue+""));
                }
            }
            if (!("Infinity".Equals(getInfinity()))) {
                @out.emitAttribute("infinity", getInfinity());
            }
            if (!("NaN".Equals(getNaN()))) {
                @out.emitAttribute("NaN", getNaN());
            }
            @out.endElement();
        }
        private int singleChar(string name, string value) {
            Saxon.Hej.str.UnicodeString us = Saxon.Hej.str.StringView.of(value).tidy();
            if ((us.length() != 1)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Attribute "+name)+" should be a single character"), errorCodes[ERR_NOT_SINGLE_CHAR]);
                err.setIsStaticError(true);
                throw err;
            }
            return us.codePointAt(0);
        }
        public void checkConsistency(Saxon.Hej.om.StructuredQName name) {
            for (int i = 0; (i<10); i++) {
                if (inconsistent[i]) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((((("Inconsistency in "+(((name == null) ? "unnamed decimal format. " : (("decimal format "+name.getDisplayName())+". "))))+"There are two inconsistent values for decimal-format property ")+propertyNames[i])+" at the same import precedence"));
                    err.setErrorCode(errorCodes[ERR_TWO_VALUES_FOR_SAME_PROPERTY]);
                    err.setIsStaticError(true);
                    throw err;
                }
            }
            Saxon.Hej.z.IntHashMap<string> map = new Saxon.Hej.z.IntHashMap<string>(20);
            map.put(getDecimalSeparator(), "decimal-separator");
            if ((map.get(getGroupingSeparator()) != null)) {
                duplicate("grouping-separator", map.get(getGroupingSeparator()), name);
            }
            map.put(getGroupingSeparator(), "grouping-separator");
            if ((map.get(getPercent()) != null)) {
                duplicate("percent", map.get(getPercent()), name);
            }
            map.put(getPercent(), "percent");
            if ((map.get(getPerMille()) != null)) {
                duplicate("per-mille", map.get(getPerMille()), name);
            }
            map.put(getPerMille(), "per-mille");
            if ((map.get(getDigit()) != null)) {
                duplicate("digit", map.get(getDigit()), name);
            }
            map.put(getDigit(), "digit");
            if ((map.get(getPatternSeparator()) != null)) {
                duplicate("pattern-separator", map.get(getPatternSeparator()), name);
            }
            map.put(getPatternSeparator(), "pattern-separator");
            if ((map.get(getExponentSeparator()) != null)) {
                duplicate("exponent-separator", map.get(getExponentSeparator()), name);
            }
            map.put(getExponentSeparator(), "exponent-separator");
            int zero = getZeroDigit();
            for (int i = zero; (i<(zero+10)); i++) {
                if ((map.get(i) != null)) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((((("Inconsistent properties in "+(((name == null) ? "unnamed decimal format. " : (("decimal format "+name.getDisplayName())+". "))))+"The same character is used as digit ")+((i-zero)))+" in the chosen digit family, and as the ")+map.get(i)));
                    err.setErrorCode(errorCodes[ERR_SAME_CHAR_IN_TWO_ROLES]);
                    throw err;
                }
            }
        }
        private void duplicate(string role1, string role2, Saxon.Hej.om.StructuredQName name) {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((((("Inconsistent properties in "+(((name == null) ? "unnamed decimal format. " : (("decimal format "+name.getDisplayName())+". "))))+"The same character is used as the ")+role1)+" and as the ")+role2));
            err.setErrorCode(errorCodes[ERR_SAME_CHAR_IN_TWO_ROLES]);
            throw err;
        }
        public static bool isValidZeroDigit(int zeroDigit) {
            return (Saxon.Ejava.util.Arrays.binarySearch(zeroDigits, zeroDigit)>=0);
        }
        static int[] zeroDigits = {0x0030, 0x0660, 0x06f0, 0x0966, 0x09e6, 0x0a66, 0x0ae6, 0x0b66, 0x0be6, 0x0c66, 0x0ce6, 0x0d66, 0x0e50, 0x0ed0, 0x0f20, 0x1040, 0x17e0, 0x1810, 0x1946, 0x19d0, unchecked((int)0xff10), 0x104a0, 0x1d7ce, 0x1d7d8, 0x1d7e2, 0x1d7ec, 0x1d7f6};
        public override bool Equals(System.Object obj) {
            if (!(((obj is Saxon.Hej.trans.DecimalSymbols)))) {
                return false;
            }
            Saxon.Hej.trans.DecimalSymbols o = (Saxon.Hej.trans.DecimalSymbols)(obj);
            return ((((((((((getDecimalSeparator() == o.getDecimalSeparator()) && (getGroupingSeparator() == o.getGroupingSeparator())) && (getDigit() == o.getDigit())) && (getMinusSign() == o.getMinusSign())) && (getPercent() == o.getPercent())) && (getPerMille() == o.getPerMille())) && (getZeroDigit() == o.getZeroDigit())) && (getPatternSeparator() == o.getPatternSeparator())) && getInfinity().Equals(o.getInfinity())) && getNaN().Equals(o.getNaN()));
        }
        public override int GetHashCode() {
            return ((getDecimalSeparator()+((37*getGroupingSeparator())))+((41*getDigit())));
        }
    
    }
}