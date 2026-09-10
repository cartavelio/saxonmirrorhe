using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class StringToDouble : Saxon.Hej.type.StringConverter {
        private static readonly Saxon.Hej.type.StringToDouble THE_INSTANCE = new Saxon.Hej.type.StringToDouble();
        public static Saxon.Hej.type.StringToDouble getInstance() {
            return THE_INSTANCE;
        }
        protected internal StringToDouble()  {
        }
        public double stringToNumber(Saxon.Hej.str.UnicodeString s) {
            int len = s.length32();
            bool containsDisallowedChars = false;
            bool containsWhitespace = false;
            if ((len<9)) {
                bool useJava = false;
                long num = 0;
                int dot = -1;
                int lastDigit = -1;
                bool onlySpaceAllowed = false;
                bool breakLoop = false;
                for (int i = 0; (i<len); i++) {
                    int c = s.codePointAt(i);
                    switch (c) {
                      case ' ':
                      case '\n':
                      case '\t':
                      case '\r':
                        containsWhitespace = true;
                        if ((lastDigit != -1)) {
                            onlySpaceAllowed = true;
                        }
                        break ;
                      case '0':
                      case '1':
                      case '2':
                      case '3':
                      case '4':
                      case '5':
                      case '6':
                      case '7':
                      case '8':
                      case '9':
                        if (onlySpaceAllowed) {
                            throw new Saxon.Ejava.lang.NumberFormatException("Numeric value contains embedded whitespace");
                        }
                        lastDigit = i;
                        num = ((num*10)+((c-'0')));
                        break ;
                      case '.':
                        if (onlySpaceAllowed) {
                            throw new Saxon.Ejava.lang.NumberFormatException("Numeric value contains embedded whitespace");
                        }
                        if ((dot != -1)) {
                            throw new Saxon.Ejava.lang.NumberFormatException("Only one decimal point allowed");
                        }
                        dot = i;
                        break ;
                      case 'x':
                      case 'X':
                      case 'f':
                      case 'F':
                      case 'd':
                      case 'D':
                      case 'n':
                      case 'N':
                        containsDisallowedChars = true;
                        useJava = true;
                        breakLoop = true;
                        break ;
                      default:
                        useJava = true;
                        break ;
                    }
                    if (breakLoop) {
                        break ;
                    }
                }
                if (!(useJava)) {
                    if ((lastDigit == -1)) {
                        throw new Saxon.Ejava.lang.NumberFormatException("String to double conversion: no digits found");
                    } else {
                        if (((dot == -1) || (dot>lastDigit))) {
                            return (double)(num);
                        } else {
                            int afterPoint = (lastDigit-dot);
                            return ((double)(num)/powers[afterPoint]);
                        }
                    }
                }
            } else {
                bool breakLoop2 = false;
                for (int i = 0; (i<len); i++) {
                    int c = s.codePointAt(i);
                    switch (c) {
                      case ' ':
                      case '\n':
                      case '\t':
                      case '\r':
                        containsWhitespace = true;
                        break ;
                      case '0':
                      case '1':
                      case '2':
                      case '3':
                      case '4':
                      case '5':
                      case '6':
                      case '7':
                      case '8':
                      case '9':
                      case '.':
                      case 'e':
                      case 'E':
                      case '+':
                      case '-':
                        break ;
                      default:
                        containsDisallowedChars = true;
                        breakLoop2 = true;
                        break ;
                    }
                    if (breakLoop2) {
                        break ;
                    }
                }
            }
            string n = (containsWhitespace ? Saxon.Hej.value.Whitespace.trim(s).ToString() : s.ToString());
            if ("INF".Equals(n)) {
                return System.Double.PositiveInfinity;
            } else {
                if ("+INF".Equals(n)) {
                    return signedPositiveInfinity();
                } else {
                    if ("-INF".Equals(n)) {
                        return System.Double.NegativeInfinity;
                    } else {
                        if ("NaN".Equals(n)) {
                            return System.Double.NaN;
                        } else {
                            if (containsDisallowedChars) {
                                throw new Saxon.Ejava.lang.NumberFormatException(("invalid floating point value: "+s));
                            }
                            try {
                                return System.Double.Parse(n);
                            } catch (Saxon.Ejava.lang.NumberFormatException nfe) {
                                throw;
                            } catch (System.Exception err) {
                                throw new Saxon.Ejava.lang.NumberFormatException(err.Message);
                            }
                        }
                    }
                }
            }
        }
        protected internal virtual double signedPositiveInfinity() {
            throw new Saxon.Ejava.lang.NumberFormatException("the float/double value '+INF' is not allowed under XSD 1.0");
        }
        private static readonly double[] powers = new double[]{1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000};
        public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
            try {
                double d = stringToNumber(input);
                return new Saxon.Hej.value.DoubleValue(d);
            } catch (Saxon.Ejava.lang.NumberFormatException e) {
                return new Saxon.Hej.type.ValidationFailure((("Cannot convert string "+Saxon.Hej.trans.Err.wrap(input, Saxon.Hej.trans.Err.VALUE))+" to double"));
            }
        }
    
    }
}