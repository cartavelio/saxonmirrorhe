using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class ParseIetfDate : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue stringValue = (Saxon.Hej.value.StringValue)(arguments[0].head());
            if ((stringValue == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            return Saxon.Hej.om.SequenceTool.itemOrEmpty(parse(stringValue.getStringValue(), context));
        }
        private readonly string[] dayNames = new string[]{"Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"};
        private bool isDayName(string str) {
            foreach (string s in dayNames) {
                if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(s, str)) {
                    return true;
                }
            }
            return false;
        }
        private readonly string[] monthNames = new string[]{"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"};
        private bool isMonthName(string str) {
            foreach (string s in monthNames) {
                if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(s, str)) {
                    return true;
                }
            }
            return false;
        }
        private byte getMonthNumber(string str) {
            if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("Jan", str)) {
                return (byte)(1);
            } else {
                if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("Feb", str)) {
                    return (byte)(2);
                } else {
                    if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("Mar", str)) {
                        return (byte)(3);
                    } else {
                        if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("Apr", str)) {
                            return (byte)(4);
                        } else {
                            if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("May", str)) {
                                return (byte)(5);
                            } else {
                                if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("Jun", str)) {
                                    return (byte)(6);
                                } else {
                                    if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("Jul", str)) {
                                        return (byte)(7);
                                    } else {
                                        if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("Aug", str)) {
                                            return (byte)(8);
                                        } else {
                                            if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("Sep", str)) {
                                                return (byte)(9);
                                            } else {
                                                if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("Oct", str)) {
                                                    return (byte)(10);
                                                } else {
                                                    if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("Nov", str)) {
                                                        return (byte)(11);
                                                    } else {
                                                        if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("Dec", str)) {
                                                            return (byte)(12);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return (byte)(0);
        }
        private int requireDSep(System.Collections.Generic.IList<string> tokens, int i, string input) {
            bool found = false;
            if (" ".Equals(tokens[i])) {
                i++;
                found = true;
            }
            if ("-".Equals(tokens[i])) {
                i++;
                found = true;
            }
            if (" ".Equals(tokens[i])) {
                i++;
                found = true;
            }
            if (!(found)) {
                badDate("Date separator missing", input);
            }
            return i;
        }
        private static void badDate(string msg, string value) {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((((("Invalid IETF date value "+value)+" (")+msg)+")"));
            err.setErrorCode("FORG0010");
            throw err;
        }
        private readonly string[] timezoneNames = new string[]{"UT", "UTC", "GMT", "EST", "EDT", "CST", "CDT", "MST", "MDT", "PST", "PDT"};
        private bool isTimezoneName(string str) {
            foreach (string s in timezoneNames) {
                if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(s, str)) {
                    return true;
                }
            }
            return false;
        }
        private int getTimezoneOffsetFromName(string str) {
            if (((Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("UT", str)|Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("UTC", str))|Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("GMT", str))) {
                return 0;
            } else {
                if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("EST", str)) {
                    return (-5*60);
                } else {
                    if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("EDT", str)) {
                        return (-4*60);
                    } else {
                        if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("CST", str)) {
                            return (-6*60);
                        } else {
                            if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("CDT", str)) {
                                return (-5*60);
                            } else {
                                if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("MST", str)) {
                                    return (-7*60);
                                } else {
                                    if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("MDT", str)) {
                                        return (-6*60);
                                    } else {
                                        if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("PST", str)) {
                                            return (-8*60);
                                        } else {
                                            if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("PDT", str)) {
                                                return (-7*60);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return 0;
        }
        public Saxon.Hej.value.DateTimeValue parse(string input, Saxon.Hej.expr.XPathContext context) {
            System.Collections.Generic.IList<string> tokens = tokenize(input);
            int year = 0;
            byte month = 0;
            byte day = 0;
            System.Collections.Generic.IList<Saxon.Hej.value.TimeValue> timeValue = new System.Collections.Generic.List<Saxon.Hej.value.TimeValue>();
            int i = 0;
            string currentToken = tokens[i];
            if ((System.Text.RegularExpressions.Regex.IsMatch(currentToken, "[A-Za-z]+") && isDayName(currentToken))) {
                currentToken = tokens[++i];
                if (",".Equals(currentToken)) {
                    currentToken = tokens[++i];
                }
                if (!(" ".Equals(currentToken))) {
                    badDate("Space missing after day name", input);
                }
                currentToken = tokens[++i];
            }
            if (isMonthName(currentToken)) {
                month = getMonthNumber(currentToken);
                i = requireDSep(tokens, (i+1), input);
                currentToken = tokens[i];
                if (!(System.Text.RegularExpressions.Regex.IsMatch(currentToken, "[0-9]+"))) {
                    badDate("Day number expected after month name", input);
                }
                if ((currentToken.Length>2)) {
                    badDate("Day number exceeds two digits", input);
                }
                day = (byte)(System.Int32.Parse(currentToken));
                currentToken = tokens[++i];
                if (!(" ".Equals(currentToken))) {
                    badDate("Space missing after day number", input);
                }
                i = parseTime(tokens, ++i, timeValue, input);
                currentToken = tokens[++i];
                if (!(" ".Equals(currentToken))) {
                    badDate("Space missing after time string", input);
                }
                currentToken = tokens[++i];
                if (System.Text.RegularExpressions.Regex.IsMatch(currentToken, "[0-9]+")) {
                    year = checkTwoOrFourDigits(input, currentToken);
                } else {
                    badDate("Year number expected after time", input);
                }
            } else {
                if (System.Text.RegularExpressions.Regex.IsMatch(currentToken, "[0-9]+")) {
                    if ((currentToken.Length>2)) {
                        badDate("First number in string expected to be day in two digits", input);
                    }
                    day = (byte)(System.Int32.Parse(currentToken));
                    i = requireDSep(tokens, ++i, input);
                    currentToken = tokens[i];
                    if (!(isMonthName(currentToken))) {
                        badDate("Abbreviated month name expected after day number", input);
                    }
                    month = getMonthNumber(currentToken);
                    i = requireDSep(tokens, ++i, input);
                    currentToken = tokens[i];
                    if (System.Text.RegularExpressions.Regex.IsMatch(currentToken, "[0-9]+")) {
                        year = checkTwoOrFourDigits(input, currentToken);
                    } else {
                        badDate("Year number expected after month name", input);
                    }
                    currentToken = tokens[++i];
                    if (!(" ".Equals(currentToken))) {
                        badDate("Space missing after year number", input);
                    }
                    i = parseTime(tokens, ++i, timeValue, input);
                } else {
                    badDate("String expected to begin with month name or day name (or day number)", input);
                }
            }
            if (!(Saxon.Hej.value.GDateValue.isValidDate(year, month, day))) {
                badDate("Date is not valid", input);
            }
            currentToken = tokens[++i];
            if (!(currentToken.Equals(EOF))) {
                badDate("Extra content found in string after date", input);
            }
            Saxon.Hej.value.DateValue date = new Saxon.Hej.value.DateValue(year, month, day);
            Saxon.Hej.value.TimeValue time = timeValue[0];
            if ((time.getHour() == 24)) {
                date = Saxon.Hej.value.DateValue.tomorrow(date.getYear(), date.getMonth(), date.getDay());
                time = new Saxon.Hej.value.TimeValue((byte)(0), (byte)(0), (byte)(0), 0, time.getTimezoneInMinutes(), "");
            }
            return Saxon.Hej.value.DateTimeValue.makeDateTimeValue(date, time);
        }
        private int checkTwoOrFourDigits(string input, string currentToken) {
            int year;
            if ((currentToken.Length == 4)) {
                year = System.Int32.Parse(currentToken);
            } else {
                if ((currentToken.Length == 2)) {
                    year = (System.Int32.Parse(currentToken)+1900);
                } else {
                    badDate("Year number must be two or four digits", input);
                    year = 0;
                }
            }
            return year;
        }
        public int parseTime(System.Collections.Generic.IList<string> tokens, int currentPosition, System.Collections.Generic.IList<Saxon.Hej.value.TimeValue> result, string input) {
            byte hour;
            byte minute;
            byte second = 0;
            int microsecond = 0;
            int tz = 0;
            int i = currentPosition;
            int n = currentPosition;
            Saxon.Ejava.lang.StringBuilder currentToken = new Saxon.Ejava.lang.StringBuilder(tokens[i]);
            if (!(System.Text.RegularExpressions.Regex.IsMatch(currentToken.ToString(), "[0-9]+"))) {
                badDate("Hour number expected", input);
            }
            if ((currentToken.length()>2)) {
                badDate("Hour number exceeds two digits", input);
            }
            hour = (byte)(System.Int32.Parse(currentToken.ToString()));
            currentToken = new Saxon.Ejava.lang.StringBuilder(tokens[++i]);
            if (!(":".Equals(currentToken.ToString()))) {
                badDate("Separator ':' missing after hour", input);
            }
            currentToken = new Saxon.Ejava.lang.StringBuilder(tokens[++i]);
            if (!(System.Text.RegularExpressions.Regex.IsMatch(currentToken.ToString(), "[0-9]+"))) {
                badDate("Minutes expected after hour", input);
            }
            if ((currentToken.length() != 2)) {
                badDate("Minutes must be exactly two digits", input);
            }
            minute = (byte)(System.Int32.Parse(currentToken.ToString()));
            currentToken = new Saxon.Ejava.lang.StringBuilder(tokens[++i]);
            bool finished = false;
            if (currentToken.ToString().Equals(EOF)) {
                n = (i-1);
                finished = true;
            } else {
                if (":".Equals(currentToken.ToString())) {
                    currentToken = new Saxon.Ejava.lang.StringBuilder(tokens[++i]);
                    if (!(System.Text.RegularExpressions.Regex.IsMatch(currentToken.ToString(), "[0-9]+"))) {
                        badDate("Seconds expected after ':' separator after minutes", input);
                    }
                    if ((currentToken.length() != 2)) {
                        badDate("Seconds number must have exactly two digits (before decimal point)", input);
                    }
                    second = (byte)(System.Int32.Parse(currentToken.ToString()));
                    currentToken = new Saxon.Ejava.lang.StringBuilder(tokens[++i]);
                    if (currentToken.ToString().Equals(EOF)) {
                        n = (i-1);
                        finished = true;
                    } else {
                        if (".".Equals(currentToken.ToString())) {
                            currentToken = new Saxon.Ejava.lang.StringBuilder(tokens[++i]);
                            if (!(System.Text.RegularExpressions.Regex.IsMatch(currentToken.ToString(), "[0-9]+"))) {
                                badDate("Fractional part of seconds expected after decimal point", input);
                            }
                            int len = System.Math.Min(6, currentToken.length());
                            currentToken = new Saxon.Ejava.lang.StringBuilder(currentToken.substring(0, len));
                            while ((currentToken.length()<6)) {
                                currentToken.append("0");
                            }
                            microsecond = System.Int32.Parse(currentToken.ToString());
                            if ((i<(tokens.Count-1))) {
                                currentToken = new Saxon.Ejava.lang.StringBuilder(tokens[++i]);
                            }
                        }
                    }
                }
            }
            if (!(finished)) {
                if (" ".Equals(currentToken.ToString())) {
                    currentToken = new Saxon.Ejava.lang.StringBuilder(tokens[++i]);
                    if (System.Text.RegularExpressions.Regex.IsMatch(currentToken.ToString(), "[0-9]+")) {
                        n = (i-2);
                        finished = true;
                    }
                }
                if (!(finished)) {
                    if (System.Text.RegularExpressions.Regex.IsMatch(currentToken.ToString(), "[A-Za-z]+")) {
                        if (!(isTimezoneName(currentToken.ToString()))) {
                            badDate("Timezone name not recognised", input);
                        }
                        tz = getTimezoneOffsetFromName(currentToken.ToString());
                        n = i;
                        finished = true;
                    } else {
                        if (("+".Equals(currentToken.ToString())|"-".Equals(currentToken.ToString()))) {
                            string sign = currentToken.ToString();
                            int tzOffsetHours = 0;
                            int tzOffsetMinutes = 0;
                            currentToken = new Saxon.Ejava.lang.StringBuilder(tokens[++i]);
                            if (!(System.Text.RegularExpressions.Regex.IsMatch(currentToken.ToString(), "[0-9]+"))) {
                                badDate((("Parsing timezone offset, number expected after '"+sign)+"'"), input);
                            }
                            int tLength = currentToken.length();
                            if ((tLength>4)) {
                                badDate("Timezone offset does not have the correct number of digits", input);
                            } else {
                                if ((tLength>=3)) {
                                    tzOffsetHours = System.Int32.Parse(currentToken.substring(0, (tLength-2)));
                                    tzOffsetMinutes = System.Int32.Parse(currentToken.substring((tLength-2), tLength));
                                    currentToken = new Saxon.Ejava.lang.StringBuilder(tokens[++i]);
                                } else {
                                    tzOffsetHours = System.Int32.Parse(currentToken.ToString());
                                    currentToken = new Saxon.Ejava.lang.StringBuilder(tokens[++i]);
                                    if (":".Equals(currentToken.ToString())) {
                                        currentToken = new Saxon.Ejava.lang.StringBuilder(tokens[++i]);
                                        if (System.Text.RegularExpressions.Regex.IsMatch(currentToken.ToString(), "[0-9]+")) {
                                            if ((currentToken.length() != 2)) {
                                                badDate("Parsing timezone offset, minutes must be two digits", input);
                                            } else {
                                                tzOffsetMinutes = System.Int32.Parse(currentToken.ToString());
                                            }
                                            currentToken = new Saxon.Ejava.lang.StringBuilder(tokens[++i]);
                                        }
                                    }
                                }
                            }
                            if ((tzOffsetMinutes>59)) {
                                badDate("Timezone offset minutes out of range", input);
                            }
                            tz = ((tzOffsetHours*60)+tzOffsetMinutes);
                            if (sign.Equals("-")) {
                                tz = -(tz);
                            }
                            if (currentToken.ToString().Equals(EOF)) {
                                n = (i-1);
                                finished = true;
                            } else {
                                if (" ".Equals(currentToken.ToString())) {
                                    currentToken = new Saxon.Ejava.lang.StringBuilder(tokens[++i]);
                                    if (System.Text.RegularExpressions.Regex.IsMatch(currentToken.ToString(), "[0-9]+")) {
                                        n = (i-2);
                                        finished = true;
                                    }
                                }
                            }
                            if ((!(finished) && "(".Equals(currentToken.ToString()))) {
                                currentToken = new Saxon.Ejava.lang.StringBuilder(tokens[++i]);
                                if (" ".Equals(currentToken.ToString())) {
                                    currentToken = new Saxon.Ejava.lang.StringBuilder(tokens[++i]);
                                }
                                if (!(System.Text.RegularExpressions.Regex.IsMatch(currentToken.ToString(), "[A-Za-z]+"))) {
                                    badDate("Timezone name expected after '('", input);
                                } else {
                                    if (System.Text.RegularExpressions.Regex.IsMatch(currentToken.ToString(), "[A-Za-z]+")) {
                                        if (!(isTimezoneName(currentToken.ToString()))) {
                                            badDate("Timezone name not recognised", input);
                                        }
                                        currentToken = new Saxon.Ejava.lang.StringBuilder(tokens[++i]);
                                    }
                                }
                                if (" ".Equals(currentToken.ToString())) {
                                    currentToken = new Saxon.Ejava.lang.StringBuilder(tokens[++i]);
                                }
                                if (!(")".Equals(currentToken.ToString()))) {
                                    badDate("Expected ')' after timezone name", input);
                                }
                                n = i;
                                finished = true;
                            } else {
                                if (!(finished)) {
                                    badDate("Unexpected content after timezone offset", input);
                                }
                            }
                        } else {
                            badDate("Unexpected content in time (after minutes)", input);
                        }
                    }
                }
            }
            if (!(finished)) {
                throw new Saxon.Ejava.lang.AssertionError("Should have finished");
            }
            if (!(isValidTime(hour, minute, second, microsecond, tz))) {
                badDate("Time/timezone is not valid", input);
            }
            Saxon.Hej.value.TimeValue timeValue = new Saxon.Hej.value.TimeValue(hour, minute, second, (microsecond*1000), tz, "");
            result.Add(timeValue);
            return n;
        }
        public static bool isValidTime(int hour, int minute, int second, int microsecond, int tz) {
            return ((((((((((((hour>=0) && (hour<=23)) && (minute>=0)) && (minute<60)) && (second>=0)) && (second<60)) && (microsecond>=0)) && (microsecond<1000000)) || ((((hour == 24) && (minute == 0)) && (second == 0)) && (microsecond == 0)))) && (tz>=(-14*60))) && (tz<=(14*60)));
        }
        private System.Collections.Generic.IList<string> tokenize(string input) {
            System.Collections.Generic.IList<string> tokens = new System.Collections.Generic.List<string>();
            input = input.Trim();
            if (input.Length==0) {
                badDate("Input is empty", input);
                return tokens;
            }
            int i = 0;
            input = (input+(char)(0));
            while (true) {
                char c = input[i];
                if ((c == 0)) {
                    tokens.Add(EOF);
                    return tokens;
                }
                if (Saxon.Hej.value.Whitespace.isWhite(c)) {
                    int j = i;
                    while (Saxon.Hej.value.Whitespace.isWhite(input[j++])) {
                    }
                    tokens.Add(" ");
                    i = (j-1);
                } else {
                    if (System.Char.IsLetter(c)) {
                        int j = i;
                        while (System.Char.IsLetter(input[j++])) {
                        }
                        tokens.Add(input.Substring(i, (j-1) - i));
                        i = (j-1);
                    } else {
                        if (System.Char.IsDigit(c)) {
                            int j = i;
                            while (System.Char.IsDigit(input[j++])) {
                            }
                            tokens.Add(input.Substring(i, (j-1) - i));
                            i = (j-1);
                        } else {
                            tokens.Add(input.Substring(i, (i+1) - i));
                            i++;
                        }
                    }
                }
            }
        }
        private const string EOF = "";
    
    }
}