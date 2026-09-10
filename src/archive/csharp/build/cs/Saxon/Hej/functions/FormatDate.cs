using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class FormatDate : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        static readonly string[] knownCalendars = {"AD", "AH", "AME", "AM", "AP", "AS", "BE", "CB", "CE", "CL", "CS", "EE", "FE", "ISO", "JE", "KE", "KY", "ME", "MS", "NS", "OS", "RS", "SE", "SH", "SS", "TE", "VE", "VS"};
        private readonly static Saxon.Hej.str.UnicodeString STR_0 = Saxon.Hej.str.BMPString.of("0");
        private readonly static Saxon.Hej.str.UnicodeString STR_01 = Saxon.Hej.str.BMPString.of("01");
        private readonly static Saxon.Hej.str.UnicodeString STR_1 = Saxon.Hej.str.BMPString.of("1");
        private readonly static Saxon.Hej.str.UnicodeString STR_f = Saxon.Hej.str.BMPString.of("f");
        private readonly static Saxon.Hej.str.UnicodeString STR_F = Saxon.Hej.str.BMPString.of("F");
        private readonly static Saxon.Hej.str.UnicodeString STR_i = Saxon.Hej.str.BMPString.of("i");
        private readonly static Saxon.Hej.str.UnicodeString STR_I = Saxon.Hej.str.BMPString.of("I");
        private readonly static Saxon.Hej.str.UnicodeString STR_J = Saxon.Hej.str.BMPString.of("J");
        private readonly static Saxon.Hej.str.UnicodeString STR_M = Saxon.Hej.str.BMPString.of("M");
        private readonly static Saxon.Hej.str.UnicodeString STR_N = Saxon.Hej.str.BMPString.of("N");
        private readonly static Saxon.Hej.str.UnicodeString STR_Nn = Saxon.Hej.str.BMPString.of("Nn");
        private readonly static Saxon.Hej.str.UnicodeString STR_n = Saxon.Hej.str.BMPString.of("n");
        private readonly static Saxon.Hej.str.UnicodeString STR_P = Saxon.Hej.str.BMPString.of("P");
        private readonly static Saxon.Hej.str.UnicodeString STR_s = Saxon.Hej.str.BMPString.of("s");
        private readonly static Saxon.Hej.str.UnicodeString STR_Y = Saxon.Hej.str.BMPString.of("Y");
        private readonly static Saxon.Hej.str.UnicodeString STR_Z = Saxon.Hej.str.BMPString.of("Z");
        private string adjustCalendar(string calendarVal, string result, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.StructuredQName cal;
            try {
                cal = Saxon.Hej.om.StructuredQName.fromLexicalQName((calendarVal), false, true, getRetainedStaticContext());
            } catch (Saxon.Hej.trans.XPathException e) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Invalid calendar name. "+e.Message));
                err.setErrorCode("FOFD1340");
                err.setXPathContext(context);
                throw err;
            }
            if (cal.hasURI("")) {
                string calLocal = cal.getLocalPart();
                if ((calLocal.Equals("AD") || calLocal.Equals("ISO"))) {
                } else {
                    if ((Saxon.Ejava.util.Arrays.binarySearch(knownCalendars, calLocal)>=0)) {
                        result = ("[Calendar: AD]"+result);
                    } else {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Unknown no-namespace calendar: "+calLocal));
                        err.setErrorCode("FOFD1340");
                        err.setXPathContext(context);
                        throw err;
                    }
                }
            } else {
                result = ("[Calendar: AD]"+result);
            }
            return result;
        }
        private static string formatDate(Saxon.Hej.value.CalendarValue value, string format, string language, string place, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Configuration config = context.getConfiguration();
            bool languageDefaulted = (language == null);
            if ((language == null)) {
                language = config.getDefaultLanguage();
            }
            if ((place == null)) {
                place = config.getDefaultCountry();
            }
            if ((value.hasTimezone() && place.Contains("/"))) {
                Saxon.Ejava.util.TimeZone tz = Saxon.Ejava.util.TimeZone.getTimeZone(place);
                if ((tz != null)) {
                    Singulink.Numerics.BigDecimal seconds = value.toDateTime().secondsSinceEpoch();
                    int milliOffset = tz.getOffset((((long)seconds)*1000));
                    value = value.adjustTimezone((milliOffset/60000));
                }
            }
            Saxon.Hej.lib.Numberer numberer = config.makeNumberer(language, place);
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(64);
            if (((!(languageDefaulted) && (numberer.GetType() == typeof(Saxon.Hej.expr.number.Numberer_en))) && !(language.StartsWith("en")))) {
                sb.append("[Language: en]");
            }
            if ((numberer.defaultedLocale() != null)) {
                sb.append((("[Language: "+numberer.defaultedLocale().Name)+"]"));
            }
            int i = 0;
            while (true) {
                while (((i<format.Length) && (format[i] != '['))) {
                    sb.append(format[i]);
                    if ((format[i] == ']')) {
                        i++;
                        if (((i == format.Length) || (format[i] != ']'))) {
                            Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("Closing ']' in date picture must be written as ']]'");
                            e.setErrorCode("FOFD1340");
                            e.setXPathContext(context);
                            throw e;
                        }
                    }
                    i++;
                }
                if ((i == format.Length)) {
                    break ;
                }
                i++;
                if (((i<format.Length) && (format[i] == '['))) {
                    sb.append('[');
                    i++;
                } else {
                    int close = ((i<format.Length) ? format.IndexOf("]", i) : -1);
                    if ((close == -1)) {
                        Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("Date format contains a '[' with no matching ']'");
                        e.setErrorCode("FOFD1340");
                        e.setXPathContext(context);
                        throw e;
                    }
                    string componentFormat = format.Substring(i, close - i);
                    sb.append(formatComponent(value, Saxon.Hej.value.Whitespace.removeAllWhitespace(componentFormat), numberer, place, context));
                    i = (close+1);
                }
            }
            return sb.ToString();
        }
        private static readonly Saxon.Hej.regex.ARegularExpression componentPattern = Saxon.Hej.regex.ARegularExpression.compile("([YMDdWwFHhmsfZzPCE])\\s*(.*)", "");
        private static Saxon.Hej.str.UnicodeString formatComponent(Saxon.Hej.value.CalendarValue value, string specifier, Saxon.Hej.lib.Numberer numberer, string country, Saxon.Hej.expr.XPathContext context) {
            bool ignoreDate = (value is Saxon.Hej.value.TimeValue);
            bool ignoreTime = (value is Saxon.Hej.value.DateValue);
            Saxon.Hej.value.DateTimeValue dtvalue = value.toDateTime();
            Saxon.Hej.str.UnicodeString uSpecifier = Saxon.Hej.str.StringView.of(specifier).tidy();
            Saxon.Hej.regex.ARegexIterator matcher = (Saxon.Hej.regex.ARegexIterator)(componentPattern.analyze(uSpecifier));
            Saxon.Hej.om.Item firstMatch = ((/*1430*/Saxon.Hej.value.StringValue)matcher.next());
            if ((((firstMatch == null) || (firstMatch.getUnicodeStringValue().length32() != uSpecifier.length32())) || !(matcher.isMatching()))) {
                Saxon.Hej.trans.XPathException error = new Saxon.Hej.trans.XPathException((("Unrecognized date/time component ["+specifier)+']'));
                error.setErrorCode("FOFD1340");
                error.setXPathContext(context);
                throw error;
            }
            Saxon.Hej.str.UnicodeString component = matcher.getRegexGroup(1);
            Saxon.Hej.str.UnicodeString format = matcher.getRegexGroup(2);
            bool defaultFormat = false;
            if ((format.isEmpty() || (format.codePointAt(0) == ','))) {
                defaultFormat = true;
                switch (component.codePointAt(0)) {
                  case 'F':
                    format = STR_Nn.concat(format);
                    break ;
                  case 'P':
                    format = STR_n.concat(format);
                    break ;
                  case 'C':
                  case 'E':
                    format = STR_N.concat(format);
                    break ;
                  case 'm':
                  case 's':
                    format = STR_01.concat(format);
                    break ;
                  case 'z':
                  case 'Z':
                    break ;
                  default:
                    format = STR_1.concat(format);
                    break ;
                }
            }
            switch (component.codePointAt(0)) {
              case 'Y':
                if (ignoreDate) {
                    Saxon.Hej.trans.XPathException error = new Saxon.Hej.trans.XPathException("In format-time(): an xs:time value does not contain a year component");
                    error.setErrorCode("FOFD1350");
                    error.setXPathContext(context);
                    throw error;
                } else {
                    int year = dtvalue.getYear();
                    if ((year<0)) {
                        year = -(year);
                    }
                    return formatNumber(component, year, format, defaultFormat, numberer, context);
                }
              case 'M':
                if (ignoreDate) {
                    Saxon.Hej.trans.XPathException error = new Saxon.Hej.trans.XPathException("In format-time(): an xs:time value does not contain a month component");
                    error.setErrorCode("FOFD1350");
                    error.setXPathContext(context);
                    throw error;
                } else {
                    int month = dtvalue.getMonth();
                    return formatNumber(component, month, format, defaultFormat, numberer, context);
                }
              case 'D':
                if (ignoreDate) {
                    Saxon.Hej.trans.XPathException error = new Saxon.Hej.trans.XPathException("In format-time(): an xs:time value does not contain a day component");
                    error.setErrorCode("FOFD1350");
                    error.setXPathContext(context);
                    throw error;
                } else {
                    int day = dtvalue.getDay();
                    return formatNumber(component, day, format, defaultFormat, numberer, context);
                }
              case 'd':
                if (ignoreDate) {
                    Saxon.Hej.trans.XPathException error = new Saxon.Hej.trans.XPathException("In format-time(): an xs:time value does not contain a day component");
                    error.setErrorCode("FOFD1350");
                    error.setXPathContext(context);
                    throw error;
                } else {
                    int day = Saxon.Hej.value.DateValue.getDayWithinYear(dtvalue.getYear(), dtvalue.getMonth(), dtvalue.getDay());
                    return formatNumber(component, day, format, defaultFormat, numberer, context);
                }
              case 'W':
                if (ignoreDate) {
                    Saxon.Hej.trans.XPathException error = new Saxon.Hej.trans.XPathException("In format-time(): cannot obtain the week number from an xs:time value");
                    error.setErrorCode("FOFD1350");
                    error.setXPathContext(context);
                    throw error;
                } else {
                    int week = Saxon.Hej.value.DateValue.getWeekNumber(dtvalue.getYear(), dtvalue.getMonth(), dtvalue.getDay());
                    return formatNumber(component, week, format, defaultFormat, numberer, context);
                }
              case 'w':
                if (ignoreDate) {
                    Saxon.Hej.trans.XPathException error = new Saxon.Hej.trans.XPathException("In format-time(): cannot obtain the week number from an xs:time value");
                    error.setErrorCode("FOFD1350");
                    error.setXPathContext(context);
                    throw error;
                } else {
                    int week = Saxon.Hej.value.DateValue.getWeekNumberWithinMonth(dtvalue.getYear(), dtvalue.getMonth(), dtvalue.getDay());
                    return formatNumber(component, week, format, defaultFormat, numberer, context);
                }
              case 'H':
                if (ignoreTime) {
                    Saxon.Hej.trans.XPathException error = new Saxon.Hej.trans.XPathException("In format-date(): an xs:date value does not contain an hour component");
                    error.setErrorCode("FOFD1350");
                    error.setXPathContext(context);
                    throw error;
                } else {
                    Saxon.Hej.value.Int64Value hour = (Saxon.Hej.value.Int64Value)(value.getComponent(Saxon.Hej.functions.AccessorFn.Component.HOURS));
                    System.Diagnostics.Debug.Assert((hour != null));
                    return formatNumber(component, (int)(hour.longValue()), format, defaultFormat, numberer, context);
                }
              case 'h':
                if (ignoreTime) {
                    Saxon.Hej.trans.XPathException error = new Saxon.Hej.trans.XPathException("In format-date(): an xs:date value does not contain an hour component");
                    error.setErrorCode("FOFD1350");
                    error.setXPathContext(context);
                    throw error;
                } else {
                    Saxon.Hej.value.Int64Value hour = (Saxon.Hej.value.Int64Value)(value.getComponent(Saxon.Hej.functions.AccessorFn.Component.HOURS));
                    System.Diagnostics.Debug.Assert((hour != null));
                    int hr = (int)(hour.longValue());
                    if ((hr>12)) {
                        hr = (hr-12);
                    }
                    if ((hr == 0)) {
                        hr = 12;
                    }
                    return formatNumber(component, hr, format, defaultFormat, numberer, context);
                }
              case 'm':
                if (ignoreTime) {
                    Saxon.Hej.trans.XPathException error = new Saxon.Hej.trans.XPathException("In format-date(): an xs:date value does not contain a minutes component");
                    error.setErrorCode("FOFD1350");
                    error.setXPathContext(context);
                    throw error;
                } else {
                    Saxon.Hej.value.Int64Value minutes = (Saxon.Hej.value.Int64Value)(value.getComponent(Saxon.Hej.functions.AccessorFn.Component.MINUTES));
                    System.Diagnostics.Debug.Assert((minutes != null));
                    return formatNumber(component, (int)(minutes.longValue()), format, defaultFormat, numberer, context);
                }
              case 's':
                if (ignoreTime) {
                    Saxon.Hej.trans.XPathException error = new Saxon.Hej.trans.XPathException("In format-date(): an xs:date value does not contain a seconds component");
                    error.setErrorCode("FOFD1350");
                    error.setXPathContext(context);
                    throw error;
                } else {
                    Saxon.Hej.value.IntegerValue seconds = (Saxon.Hej.value.IntegerValue)(value.getComponent(Saxon.Hej.functions.AccessorFn.Component.WHOLE_SECONDS));
                    System.Diagnostics.Debug.Assert((seconds != null));
                    return formatNumber(component, (int)(seconds.longValue()), format, defaultFormat, numberer, context);
                }
              case 'f':
                if (ignoreTime) {
                    Saxon.Hej.trans.XPathException error = new Saxon.Hej.trans.XPathException("In format-date(): an xs:date value does not contain a fractional seconds component");
                    error.setErrorCode("FOFD1350");
                    error.setXPathContext(context);
                    throw error;
                } else {
                    Saxon.Hej.value.Int64Value micros = (Saxon.Hej.value.Int64Value)(value.getComponent(Saxon.Hej.functions.AccessorFn.Component.MICROSECONDS));
                    System.Diagnostics.Debug.Assert((micros != null));
                    return formatNumber(component, (int)(micros.longValue()), format, defaultFormat, numberer, context);
                }
              case 'z':
              case 'Z':
                Saxon.Hej.value.DateTimeValue dtv;
                if ((value is Saxon.Hej.value.TimeValue)) {
                    Saxon.Hej.value.DateTimeValue now = Saxon.Hej.value.DateTimeValue.getCurrentDateTime(context);
                    int year = now.getYear();
                    int tzoffset = value.getTimezoneInMinutes();
                    Saxon.Hej.value.DateTimeValue baseDate = new Saxon.Hej.value.DateTimeValue(year, (byte)(1), (byte)(1), (byte)(0), (byte)(0), (byte)(0), 0, tzoffset, false);
                    Saxon.Ejava.util.Optional<bool> b = Saxon.Hej.expr.number.NamedTimeZone.inSummerTime(baseDate, country);
                    if ((b.isPresent() && b.get())) {
                        baseDate = new Saxon.Hej.value.DateTimeValue(year, (byte)(7), (byte)(1), (byte)(0), (byte)(0), (byte)(0), 0, tzoffset, false);
                    }
                    dtv = Saxon.Hej.value.DateTimeValue.makeDateTimeValue(baseDate.toDateValue(), (Saxon.Hej.value.TimeValue)(value));
                } else {
                    dtv = value.toDateTime();
                }
                return formatTimeZone(dtv, (char)(component.codePointAt(0)), format, country);
              case 'F':
                if (ignoreDate) {
                    Saxon.Hej.trans.XPathException error = new Saxon.Hej.trans.XPathException("In format-time(): an xs:time value does not contain day-of-week component");
                    error.setErrorCode("FOFD1350");
                    error.setXPathContext(context);
                    throw error;
                } else {
                    int day = Saxon.Hej.value.DateValue.getDayOfWeek(dtvalue.getYear(), dtvalue.getMonth(), dtvalue.getDay());
                    return formatNumber(component, day, format, defaultFormat, numberer, context);
                }
              case 'P':
                if (ignoreTime) {
                    Saxon.Hej.trans.XPathException error = new Saxon.Hej.trans.XPathException("In format-date(): an xs:date value does not contain an am/pm component");
                    error.setErrorCode("FOFD1350");
                    error.setXPathContext(context);
                    throw error;
                } else {
                    int minuteOfDay = ((dtvalue.getHour()*60)+dtvalue.getMinute());
                    return formatNumber(component, minuteOfDay, format, defaultFormat, numberer, context);
                }
              case 'C':
                return Saxon.Hej.str.StringView.of(numberer.getCalendarName("AD")).tidy();
              case 'E':
                if (ignoreDate) {
                    Saxon.Hej.trans.XPathException error = new Saxon.Hej.trans.XPathException("In format-time(): an xs:time value does not contain an AD/BC component");
                    error.setErrorCode("FOFD1350");
                    error.setXPathContext(context);
                    throw error;
                } else {
                    int year = dtvalue.getYear();
                    return Saxon.Hej.str.StringView.of(numberer.getEraName(year)).tidy();
                }
              default:
                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException((("Unknown format-date/time component specifier '"+format.substring(0, 1))+'\''));
                e.setErrorCode("FOFD1340");
                e.setXPathContext(context);
                throw e;
            }
        }
        private static readonly Saxon.Hej.regex.ARegularExpression widthPattern = Saxon.Hej.regex.ARegularExpression.compile(",(\\*|[0-9]+)(\\-(\\*|[0-9]+))?", "");
        private static readonly Saxon.Hej.regex.ARegularExpression digitsPattern = Saxon.Hej.regex.ARegularExpression.compile("\\p{Nd}+", "");
        private static readonly Saxon.Hej.regex.ARegularExpression digitsOrOptionalDigitsPattern = Saxon.Hej.regex.ARegularExpression.compile("[#\\p{Nd}]+", "");
        private static readonly Saxon.Hej.regex.ARegularExpression fractionalDigitsPattern = Saxon.Hej.regex.ARegularExpression.compile("\\p{Nd}+#*", "");
        private static Saxon.Hej.str.UnicodeString formatNumber(Saxon.Hej.str.UnicodeString component, int value, Saxon.Hej.str.UnicodeString format, bool defaultFormat, Saxon.Hej.lib.Numberer numberer, Saxon.Hej.expr.XPathContext context) {
            int comma = (int)(Saxon.Hej.str.StringTool.lastIndexOf(format, ','));
            Saxon.Hej.str.UnicodeString widths = Saxon.Hej.str.EmptyUnicodeString.getInstance();
            if ((comma>=0)) {
                widths = format.substring(comma);
                format = format.prefix(comma);
            }
            Saxon.Hej.str.UnicodeString primary = format;
            string letterValue = null;
            string ordinal = null;
            int lastCP = Saxon.Hej.str.StringTool.lastCodePoint(primary);
            if ((lastCP == 't')) {
                primary = primary.prefix((primary.length()-1));
                letterValue = "traditional";
            } else {
                if ((lastCP == 'o')) {
                    primary = primary.prefix((primary.length()-1));
                    ordinal = numberer.getOrdinalSuffixForDateTime(component.ToString());
                }
            }
            int min = 1;
            int max = System.Int32.MaxValue;
            if (digitsPattern.matches(primary)) {
                int primaryLen = primary.length32();
                if ((primaryLen>1)) {
                    min = primaryLen;
                    max = primaryLen;
                }
            }
            if (STR_Y.Equals(component)) {
                min = max = 0;
                if (!(widths.isEmpty())) {
                    max = getWidths(widths)[1];
                } else {
                    if (digitsPattern.containsMatch(primary)) {
                        Saxon.Hej.z.IntIterator primaryIter = primary.codePoints();
                        while (primaryIter.hasNext()) {
                            int c = primaryIter.next();
                            if ((c == '#')) {
                                max++;
                            } else {
                                if (((((c>='0') && (c<='9'))) || Saxon.Hej.regex.charclass.Categories.ESCAPE_d.test(c))) {
                                    min++;
                                    max++;
                                }
                            }
                        }
                    }
                }
                if ((max<=1)) {
                    max = System.Int32.MaxValue;
                }
                if (((max<4) || (((max<System.Int32.MaxValue) && (value>9999))))) {
                    value = (value%(int)(System.Math.Pow(10, max)));
                }
            }
            if ((primary.Equals(STR_I) || primary.Equals(STR_i))) {
                int[] range = getWidths(widths);
                min = range[0];
                string roman = numberer.format(value, primary, null, letterValue, ordinal);
                Saxon.Hej.str.UnicodeBuilder s = new Saxon.Hej.str.UnicodeBuilder();
                s.append(roman);
                int len = Saxon.Hej.str.StringTool.getStringLength(roman);
                while ((len<min)) {
                    s.append(' ');
                    len++;
                }
                return s.toUnicodeString();
            } else {
                if (!(widths.isEmpty())) {
                    int[] range = getWidths(widths);
                    min = System.Math.Max(min, range[0]);
                    if ((max == System.Int32.MaxValue)) {
                        max = range[1];
                    } else {
                        max = System.Math.Max(max, range[1]);
                    }
                    if (defaultFormat) {
                        if (((Saxon.Hej.str.StringTool.lastCodePoint(primary) == '1') && (min != primary.length()))) {
                            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder((min+1));
                            for (int i = 1; (i<min); i++) {
                                sb.append('0');
                            }
                            sb.append('1');
                            primary = sb.toUnicodeString();
                        }
                    }
                }
            }
            if (STR_P.Equals(component)) {
                if (!((((STR_N.Equals(primary) || STR_n.Equals(primary)) || STR_Nn.Equals(primary))))) {
                    primary = STR_n;
                }
                if ((max == System.Int32.MaxValue)) {
                    max = 4;
                }
            } else {
                if (STR_Y.Equals(component)) {
                    if ((max<System.Int32.MaxValue)) {
                        value = (value%(int)(System.Math.Pow(10, max)));
                    }
                } else {
                    if (STR_f.Equals(component)) {
                        if (!(digitsPattern.containsMatch(primary))) {
                            return formatNumber(component, value, STR_1, defaultFormat, numberer, context);
                        }
                        if (!(digitsOrOptionalDigitsPattern.matches(primary))) {
                            Saxon.Hej.str.UnicodeString reverseFormat = reverse(format);
                            Saxon.Hej.str.UnicodeString reverseValue = reverse(Saxon.Hej.str.BMPString.of((""+value)));
                            Saxon.Hej.str.UnicodeString reverseResult = formatNumber(STR_s, Saxon.Impl.Helpers.Utils.ParseInt(reverseValue.ToString()), reverseFormat, false, numberer, context);
                            Saxon.Hej.str.UnicodeString correctedResult = reverse(reverseResult);
                            if ((correctedResult.length()>max)) {
                                correctedResult = correctedResult.prefix(max);
                            }
                            return correctedResult;
                        }
                        if (!(fractionalDigitsPattern.matches(primary))) {
                            throw new Saxon.Hej.trans.XPathException(("Invalid picture for fractional seconds: "+primary), "FOFD1340");
                        }
                        Saxon.Hej.str.UnicodeString str;
                        if ((value == 0)) {
                            str = STR_0;
                        } else {
                            str = Saxon.Hej.str.BMPString.of(((((1000000+value))+"")).Substring(1));
                            if ((str.length()>max)) {
                                str = str.prefix(max);
                            }
                        }
                        while ((str.length()<min)) {
                            str = str.concat(STR_0);
                        }
                        if ((str.length()>min)) {
                            while (((str.length()>min) && (str.codePointAt((str.length()-1)) == '0'))) {
                                str = str.prefix((str.length()-1));
                            }
                        }
                        int zeroDigit = Saxon.Hej.expr.number.Alphanumeric.getDigitFamily(format.codePointAt(0));
                        if (((zeroDigit>=0) && (zeroDigit != '0'))) {
                            int[] digits = new int[10];
                            for (int z = 0; (z<=9); z++) {
                                digits[z] = (zeroDigit+z);
                            }
                            long n = System.Int64.Parse(str.ToString());
                            int requiredLength = str.length32();
                            str = Saxon.Hej.str.StringView.tidy(Saxon.Hej.expr.number.AbstractNumberer.convertDigitSystem(n, digits, requiredLength));
                        }
                        return str;
                    }
                }
            }
            if (((STR_N.Equals(primary) || STR_n.Equals(primary)) || STR_Nn.Equals(primary))) {
                string s = "";
                if (STR_M.Equals(component)) {
                    s = numberer.monthName(value, min, max);
                } else {
                    if (STR_F.Equals(component)) {
                        s = numberer.dayName(value, min, max);
                    } else {
                        if (STR_P.Equals(component)) {
                            s = numberer.halfDayName(value, min, max);
                        } else {
                            primary = STR_1;
                        }
                    }
                }
                if (STR_N.Equals(primary)) {
                    return Saxon.Hej.str.StringView.tidy(s.ToUpper());
                } else {
                    if (STR_n.Equals(primary)) {
                        return Saxon.Hej.str.StringView.tidy(s.ToLower());
                    } else {
                        return Saxon.Hej.str.StringView.tidy(s);
                    }
                }
            }
            Saxon.Hej.expr.number.NumericGroupFormatter picGroupFormat;
            try {
                picGroupFormat = Saxon.Hej.functions.FormatInteger.getPicSeparators(primary);
            } catch (Saxon.Hej.trans.XPathException e) {
                if ("FODF1310".Equals(e.getErrorCodeLocalPart())) {
                    e.setErrorCode("FOFD1340");
                }
                throw;
            }
            Saxon.Hej.str.UnicodeString adjustedPicture = picGroupFormat.getAdjustedPicture();
            string formattedStr = numberer.format(value, adjustedPicture, picGroupFormat, letterValue, ordinal);
            int formattedLen = Saxon.Hej.str.StringTool.getStringLength(formattedStr);
            int digitZero;
            if ((formattedLen<min)) {
                digitZero = Saxon.Hej.expr.number.Alphanumeric.getDigitFamily(adjustedPicture.codePointAt(0));
                Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(formattedStr);
                while ((formattedLen<min)) {
                    Saxon.Hej.str.StringTool.prependWideChar(fsb, digitZero);
                    formattedLen = (formattedLen+1);
                }
                formattedStr = fsb.ToString();
            }
            return Saxon.Hej.str.StringView.tidy(formattedStr);
        }
        private static Saxon.Hej.str.UnicodeString reverse(Saxon.Hej.str.UnicodeString @in) {
            @in = @in.tidy();
            Saxon.Hej.str.UnicodeBuilder builder = new Saxon.Hej.str.UnicodeBuilder();
            for (long i = (@in.length()-1); (i>=0); i--) {
                builder.append(@in.codePointAt(i));
            }
            return builder.toUnicodeString();
        }
        private static int[] getWidths(Saxon.Hej.str.UnicodeString widths) {
            try {
                int min = -1;
                int max = -1;
                if (!(widths.isEmpty())) {
                    Saxon.Hej.regex.RegexIterator widthIter = widthPattern.analyze(widths);
                    Saxon.Hej.value.StringValue firstMatch = ((/*1430*/Saxon.Hej.value.StringValue)widthIter.next());
                    if ((((firstMatch != null) && (firstMatch.length() == widths.length())) && widthIter.isMatching())) {
                        Saxon.Hej.str.UnicodeString smin = widthIter.getRegexGroup(1);
                        if ((((smin == null) || smin.isEmpty()) || Saxon.Hej.str.StringConstants.ASTERISK.Equals(smin))) {
                            min = 1;
                        } else {
                            min = Saxon.Impl.Helpers.Utils.ParseInt(smin.ToString());
                        }
                        Saxon.Hej.str.UnicodeString smax = widthIter.getRegexGroup(3);
                        if ((((smax == null) || smax.isEmpty()) || Saxon.Hej.str.StringConstants.ASTERISK.Equals(smax))) {
                            max = System.Int32.MaxValue;
                        } else {
                            max = Saxon.Impl.Helpers.Utils.ParseInt(smax.ToString());
                        }
                        if ((min<1)) {
                            throw new Saxon.Hej.trans.XPathException(("Invalid min value in format picture "+Saxon.Hej.trans.Err.wrap(widths, Saxon.Hej.trans.Err.VALUE)), "FOFD1340");
                        }
                        if (((max<1) || (max<min))) {
                            throw new Saxon.Hej.trans.XPathException(("Invalid max value in format picture "+Saxon.Hej.trans.Err.wrap(widths, Saxon.Hej.trans.Err.VALUE)), "FOFD1340");
                        }
                    } else {
                        throw new Saxon.Hej.trans.XPathException(("Unrecognized width specifier in format picture "+Saxon.Hej.trans.Err.wrap(widths, Saxon.Hej.trans.Err.VALUE)), "FOFD1340");
                    }
                }
                int[] result = new int[2];
                result[0] = min;
                result[1] = max;
                return result;
            } catch (Saxon.Ejava.lang.NumberFormatException err) {
                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("Invalid integer used as width in date/time picture");
                e.setErrorCode("FOFD1340");
                throw e;
            }
        }
        private static Saxon.Hej.str.UnicodeString formatTimeZone(Saxon.Hej.value.DateTimeValue value, char component, Saxon.Hej.str.UnicodeString format, string country) {
            int comma = (int)(Saxon.Hej.str.StringTool.lastIndexOf(format, ','));
            Saxon.Hej.str.UnicodeString widthModifier = Saxon.Hej.str.EmptyUnicodeString.getInstance();
            if ((comma>=0)) {
                widthModifier = format.substring(comma);
                format = format.prefix(comma);
            }
            if (!(value.hasTimezone())) {
                if (format.Equals(STR_Z)) {
                    return STR_J;
                } else {
                    return Saxon.Hej.str.EmptyUnicodeString.getInstance();
                }
            }
            if ((format.isEmpty() && !(widthModifier.isEmpty()))) {
                int[] widths = getWidths(widthModifier);
                int min = widths[0];
                int max = widths[1];
                if ((min<=1)) {
                    format = Saxon.Hej.str.BMPString.of(((max>=4) ? "0:00" : "0"));
                } else {
                    if ((min<=4)) {
                        format = Saxon.Hej.str.BMPString.of(((max>=5) ? "00:00" : "00"));
                    } else {
                        format = Saxon.Hej.str.BMPString.of("00:00");
                    }
                }
            }
            if (format.isEmpty()) {
                format = Saxon.Hej.str.BMPString.of("00:00");
            }
            int tz = value.getTimezoneInMinutes();
            bool useZforZero = (Saxon.Hej.str.StringTool.lastCodePoint(format) == 't');
            if ((useZforZero && (tz == 0))) {
                return STR_Z;
            }
            if (useZforZero) {
                format = format.prefix((format.length()-1));
            }
            int digits = 0;
            int separators = 0;
            int separatorChar = ':';
            int zeroDigit = -1;
            int[] expandedFormat = Saxon.Hej.str.StringTool.expand(format);
            foreach (int ch in expandedFormat) {
                if ((System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch) == System.Globalization.UnicodeCategory.DecimalDigitNumber)) {
                    digits++;
                    if ((zeroDigit<0)) {
                        zeroDigit = Saxon.Hej.expr.number.Alphanumeric.getDigitFamily(ch);
                    }
                } else {
                    separators++;
                    separatorChar = ch;
                }
            }
            int[] buffer = new int[10];
            int used = 0;
            if ((digits>0)) {
                if ((component == 'z')) {
                    buffer[0] = 'G';
                    buffer[1] = 'M';
                    buffer[2] = 'T';
                    used = 3;
                }
                bool negative = (tz<0);
                tz = System.Math.Abs(tz);
                buffer[used++] = (negative ? '-' : '+');
                int hour = (tz/60);
                int minute = (tz%60);
                bool includeMinutes = (((minute != 0) || (digits>=3)) || (separators>0));
                bool includeSep = ((((minute != 0) && (digits<=2))) || (((separators>0) && (((minute != 0) || (digits>=3))))));
                int hourDigits = ((digits<=2) ? digits : (digits-2));
                if (((hour>9) || (hourDigits>=2))) {
                    buffer[used++] = (zeroDigit+(hour/10));
                }
                buffer[used++] = (((hour%10))+zeroDigit);
                if (includeSep) {
                    buffer[used++] = separatorChar;
                }
                if (includeMinutes) {
                    buffer[used++] = ((minute/10)+zeroDigit);
                    buffer[used++] = ((minute%10)+zeroDigit);
                }
                return Saxon.Hej.str.StringTool.fromCodePoints(buffer, used);
            } else {
                if (format.Equals(Saxon.Hej.str.BMPString.of("Z"))) {
                    int hour = (tz/60);
                    int minute = (tz%60);
                    if ((((hour<-12) || (hour>12)) || (minute != 0))) {
                        return formatTimeZone(value, 'Z', Saxon.Hej.str.BMPString.of("00:00"), country);
                    } else {
                        return Saxon.Hej.str.BMPString.of((""+"YXWVUTSRQPONZABCDEFGHIKLM"[(hour+12)]));
                    }
                } else {
                    if (((format.codePointAt(0) == 'N') || (format.codePointAt(0) == 'n'))) {
                        return Saxon.Hej.str.StringView.of(getNamedTimeZone(value, country, format)).tidy();
                    } else {
                        return formatTimeZone(value, 'Z', Saxon.Hej.str.BMPString.of("00:00"), country);
                    }
                }
            }
        }
        private static string getNamedTimeZone(Saxon.Hej.value.DateTimeValue value, string country, Saxon.Hej.str.UnicodeString format) {
            int min = 1;
            int comma = (int)(format.indexOf(','));
            if ((comma>0)) {
                Saxon.Hej.str.UnicodeString widths = format.substring(comma);
                int[] range = getWidths(widths);
                min = range[0];
            }
            if (((format.codePointAt(0) == 'N') || (format.codePointAt(0) == 'n'))) {
                if ((min<=5)) {
                    string tzname = Saxon.Hej.expr.number.NamedTimeZone.getTimeZoneNameForDate(value, country);
                    if ((tzname == null)) {
                        return formatTimeZone(value, 'Z', Saxon.Hej.str.BMPString.of("Z00:00t"), country).ToString();
                    }
                    if ((format.codePointAt(0) == 'n')) {
                        tzname = tzname.ToLower();
                    }
                    return tzname;
                } else {
                    return Saxon.Hej.expr.number.NamedTimeZone.getOlsonTimeZoneName(value, country);
                }
            }
            Saxon.Hej.str.UnicodeBuilder sbz = new Saxon.Hej.str.UnicodeBuilder();
            value.appendTimezone(sbz);
            return sbz.ToString();
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.CalendarValue value = (Saxon.Hej.value.CalendarValue)(arguments[0].head());
            if ((value == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            string format = arguments[1].head().getStringValue();
            Saxon.Hej.value.StringValue calendarVal = null;
            Saxon.Hej.value.StringValue countryVal = null;
            Saxon.Hej.value.StringValue languageVal = null;
            if ((getArity()>2)) {
                languageVal = (Saxon.Hej.value.StringValue)(arguments[2].head());
                calendarVal = (Saxon.Hej.value.StringValue)(arguments[3].head());
                countryVal = (Saxon.Hej.value.StringValue)(arguments[4].head());
            }
            string calendar = ((calendarVal == null) ? null : calendarVal.getStringValue());
            string language = ((languageVal == null) ? null : languageVal.getStringValue());
            string place = ((countryVal == null) ? null : countryVal.getStringValue());
            if ((place != null)) {
                value = adjustTimezoneToPlace(value, place);
            }
            string result = formatDate(value, format, language, place, context);
            if ((calendarVal != null)) {
                result = adjustCalendar(calendar, result, context);
            }
            return new Saxon.Hej.value.StringValue(result);
        }
        private Saxon.Hej.value.CalendarValue adjustTimezoneToPlace(Saxon.Hej.value.CalendarValue value, string place) {
            return value;
        }
    
    }
}