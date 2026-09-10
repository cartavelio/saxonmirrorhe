////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.value;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.str.TwineBuilder;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.NoDynamicContextException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;

import javax.xml.datatype.XMLGregorianCalendar;
import java.math.BigDecimal;
import java.util.GregorianCalendar;


/**
 * Abstract superclass for Date, Time, and DateTime.
 */

public abstract class CalendarValue extends AtomicValue implements AtomicMatchKey {

    // This is a reimplementation that makes no use of the Java Calendar/Date types except for computations.

    private final int tzMinutes;  // timezone offset in minutes: or the special value NO_TIMEZONE

    /**
     * The value NO_TIMEZONE is used in a value that has no timezone, and it can be passed as an argument
     * to a comparison operation if a value with no timezone is to be matched.
     */
    public static final int NO_TIMEZONE = Integer.MIN_VALUE;

    /**
     * The value MISSING_TIMEZONE is returned as the value of implicit-timezone() if the context has no
     * known value for the implicit timezone, which typically arises when early (compile-time) evaluation
     * is attempted. This makes it impossible to compare values with a timezone against values with
     * no timezone
     */

    public static final int MISSING_TIMEZONE = Integer.MAX_VALUE;

    public static final BigDecimal SIXTY = BigDecimal.valueOf(60);

    /**
     * Parse a string to create a CalendarValue whose actual type will depend on the format of the string
     *
     * @param s     a string in the lexical space of one of the date/time types (date, time, dateTime,
     *              gYearMonth, gYear, gMonth, gMonthDay, or gDay
     * @param rules conversion rules to apply (affects handling of year 0)
     * @return either a value of the appropriate type, or a ValidationFailure if the format is invalid
     */

    public static ConversionResult makeCalendarValue(UnicodeString s, ConversionRules rules) {
        ConversionResult cr = DateTimeValue.makeDateTimeValue(s, rules);
        ConversionResult firstError = cr;
        if (cr instanceof ValidationFailure) {
            cr = DateValue.makeDateValue(s, rules);
        }
        if (cr instanceof ValidationFailure) {
            cr = TimeValue.makeTimeValue(s);
        }
        if (cr instanceof ValidationFailure) {
            cr = GYearValue.makeGYearValue(s, rules);
        }
        if (cr instanceof ValidationFailure) {
            cr = GYearMonthValue.makeGYearMonthValue(s, rules);
        }
        if (cr instanceof ValidationFailure) {
            cr = GMonthValue.makeGMonthValue(s);
        }
        if (cr instanceof ValidationFailure) {
            cr = GMonthDayValue.makeGMonthDayValue(s);
        }
        if (cr instanceof ValidationFailure) {
            cr = GDayValue.makeGDayValue(s);
        }
        if (cr instanceof ValidationFailure) {
            return firstError;
        }
        return cr;
    }

    public CalendarValue(AtomicType typeLabel) {
        super(typeLabel);
        this.tzMinutes = NO_TIMEZONE;
    }

    public CalendarValue(AtomicMetadata typeLabel, int tzMinutes) {
        super(typeLabel);
        this.tzMinutes = tzMinutes;
    }

    /**
     * Determine whether this value includes a timezone
     *
     * @return true if there is a timezone in the value, false if not
     */

    public final boolean hasTimezone() {
        return tzMinutes != NO_TIMEZONE;
    }

    /**
     * Convert the value to a DateTime, retaining all the components that are actually present, and
     * substituting conventional values for components that are missing
     *
     * @return the equivalent DateTimeValue
     */

    public abstract DateTimeValue toDateTime();

    /**
     * Get the timezone value held in this object.
     *
     * @return The timezone offset from GMT in minutes, positive or negative; or the special
     *         value NO_TIMEZONE indicating that the value is not in a timezone
     */

    public final int getTimezoneInMinutes() {
        return tzMinutes;
    }

    //#if CSHARP==false
    /**
     * Get a Java Calendar object that represents this date/time value. The Calendar
     * object will be newly created for the purpose. This will respect the timezone
     * if there is one (provided the timezone is within the range supported by the {@code GregorianCalendar}
     * class, which in practice means that it is not -14:00). If there is no timezone or if
     * the timezone is out of range, the result will be in GMT.
     *
     * @return A Calendar object representing the date and time. Note that Java can only
     *         represent the time to millisecond precision.
     */

    public abstract GregorianCalendar getCalendar();
    //#endif

    //#if CSHARP==false
    /**
     * Get an XMLGregorianCalendar object that represents this date/time value The object
     * will be newly created for the purpose
     * @return an XMLGregorianCalendar object representing the data and time; specifically,
     * the components of the date and time that are actually present in this value
     */

    public XMLGregorianCalendar getXMLGregorianCalendar() {
        return new SaxonXMLGregorianCalendar(this);
    }
    //#endif
    
    /**
     * Add a duration to this date/time value
     *
     * @param duration the duration to be added (which might be negative)
     * @return a new date/time value representing the result of adding the duration. The original
     *         object is not modified.
     * @throws XPathException if an error is detected
     */

    public abstract CalendarValue add(DurationValue duration) throws XPathException;

    /**
     * Determine the difference between two points in time, as a duration
     *
     * @param other   the other point in time
     * @param context the dynamic context, used to obtain timezone information. May be set to null
     *                only if both values contain an explicit timezone, or if neither does so.
     * @return the duration as an xs:dayTimeDuration
     * @throws net.sf.saxon.trans.XPathException
     *          for example if one value is a date and the other is a time
     */

    public DayTimeDurationValue subtract(/*@NotNull*/ CalendarValue other, /*@Nullable*/ XPathContext context) throws XPathException {
        DateTimeValue dt1 = toDateTime();
        DateTimeValue dt2 = other.toDateTime();
        if (dt1.getTimezoneInMinutes() != dt2.getTimezoneInMinutes()) {
            int tz = CalendarValue.NO_TIMEZONE;
            if (context == null || (tz = context.getImplicitTimezone()) == CalendarValue.MISSING_TIMEZONE) {
                throw new NoDynamicContextException("Implicit timezone required");
            }
            dt1 = dt1.adjustToUTC(tz);
            dt2 = dt2.adjustToUTC(tz);
        }
        BigDecimal d1 = dt1.toJulianInstant();
        BigDecimal d2 = dt2.toJulianInstant();
        BigDecimal difference = d1.subtract(d2);
        return new DayTimeDurationValue(difference, BuiltInAtomicType.DAY_TIME_DURATION);
    }

    /**
     * Return a date, time, or dateTime with the same localized value, but
     * without the timezone component
     *
     * @return the result of removing the timezone
     */

    /*@NotNull*/
    public final CalendarValue removeTimezone() {
        return adjustTimezone(NO_TIMEZONE);
    }

    /**
     * Return a new date, time, or dateTime with the same normalized value, but
     * in a different timezone
     *
     * @param tz the new timezone offset from UTC, in minutes; the value {@link #NO_TIMEZONE} indicates
     *           that any existing timezone should be removed
     * @return the date/time in the new timezone
     */

    public abstract CalendarValue adjustTimezone(int tz);

    /**
     * Return a new date, time, or dateTime with the same normalized value, but
     * in a different timezone, specified as a dayTimeDuration
     *
     * @param tz the new timezone, in minutes
     * @return the date/time in the new timezone
     * @throws XPathException if an error is detected
     */

    public final CalendarValue adjustTimezone(DayTimeDurationValue tz) throws XPathException {
        BigDecimal seconds = tz.getTotalSeconds();
        BigDecimal[] divRem = seconds.divideAndRemainder(SIXTY);
        if (divRem[1].signum() != 0) {
            throw new XPathException("Timezone is not an integral number of minutes", "FODT0003");
        }
        long tzMinutes;
        try {
            tzMinutes = divRem[0].longValueExact();
        } catch (ArithmeticException e) {
            tzMinutes = Integer.MAX_VALUE;
        }
        if (Math.abs(tzMinutes) > 14 * 60) {
            throw new XPathException("Timezone out of range (-14:00 to +14:00)", "FODT0003");
        }
        return adjustTimezone((int)tzMinutes);
    }


    /**
     * Get an object value that implements the XPath equality and ordering comparison semantics for this value.
     * If the ordered parameter is set to true, the result will be a Comparable and will support a compareTo()
     * method with the semantics of the XPath lt/gt operator, provided that the other operand is also obtained
     * using the getXPathComparable() method. In all cases the result will support equals() and hashCode() methods
     * that support the semantics of the XPath eq operator, again provided that the other operand is also obtained
     * using the getXPathComparable() method. A context argument is supplied for use in cases where the comparison
     * semantics are context-sensitive, for example where they depend on the implicit timezone or the default
     * collation.
     *
     * @param collator         collation used for strings
     * @param implicitTimezone the XPath dynamic evaluation context, used in cases where the comparison is context
     * @param specVersion
     */

    /*@Nullable*/
    @Override
    public AtomicMatchKey getXPathMatchKey(StringCollator collator, int implicitTimezone, int specVersion)
    throws NoDynamicContextException {
        if (hasTimezone()) {
            return this;
        }
        if (implicitTimezone == MISSING_TIMEZONE) {
            throw new NoDynamicContextException("Unknown implicit timezone");
        }
        return adjustTimezone(implicitTimezone);
    }

    /**
     * Get a value whose equals() method follows the "same key" rules for comparing the keys of a map.
     *
     * @return a value with the property that the equals() and hashCode() methods follow the rules for comparing
     * keys in maps.
     */
    @Override
    public AtomicMatchKey asMapKey(int specVersion) {
        return new CalendarValueMapKey(this);
    }

    /**
     * Compare this value to another value of the same type, using the supplied implicit timezone if required.
     *
     * @param other   the other value to be compared
     * @param implicitTimezone the implicit timezone as an offset in minutes
     * @return the comparison result
     * @throws NoDynamicContextException if the result depends on the implicit timezone and
     * the supplied timezone is {@link CalendarValue#MISSING_TIMEZONE}
     */

    public abstract int compareTo(CalendarValue other, int implicitTimezone) throws NoDynamicContextException;

    @Override
    public boolean isIdentical(/*@NotNull*/ AtomicValue v) {
        return super.isIdentical(v) && tzMinutes == ((CalendarValue) v).tzMinutes;
    }

    /**
     * Get a hashCode that offers the guarantee that if A.isIdentical(B), then A.identityHashCode() == B.identityHashCode()
     *
     * @return a hashCode suitable for use when testing for identity.
     */
    @Override
    public int identityHashCode() {
        return hashCode() ^ tzMinutes;
    }

    /**
     * Add a string representation of the timezone, typically
     * formatted as "Z" or "+03:00" or "-10:00", to a supplied
     * string buffer
     *
     * @param tb The TwineBuilder containing the string to be updated
     * @return the TwineBuilder to be used for subsequent calls
     */

    public final TwineBuilder appendTimezone(TwineBuilder tb) {
        if (hasTimezone()) {
            tb = appendTimezone(getTimezoneInMinutes(), tb);
        }
        return tb;
    }

    /**
     * Format a timezone and append it to a buffer
     *
     * @param tz the timezone
     * @param tb The TwineBuilder containing the string to be updated
     * @return the TwineBuilder to be used for subsequent calls
     */

    public static TwineBuilder appendTimezone(int tz, TwineBuilder tb) {
        if (tz == 0) {
            tb = tb.append('Z');
        } else {
            tb = tb.append(tz > 0 ? "+" : "-");
            tz = Math.abs(tz);
            tb = appendTwoDigits(tb, tz / 60);
            tb = tb.append(':');
            tb = appendTwoDigits(tb, tz % 60);
        }
        return tb;
    }

    /**
     * Append an integer, formatted with leading zeros to a fixed size, to a string buffer
     *
     * @param tb The TwineBuilder containing the string to be updated
     * @param value the integer to be formatted
     * @param size  the number of digits required (max 9)
     * @return the TwineBuilder to be used for subsequent calls
     */

    protected static TwineBuilder appendString(TwineBuilder tb, int value, int size) {
        String s = "000000000" + value;
        return tb.append(s.substring(s.length() - size));
    }

    /**
     * Append an integer, formatted as two digits, to a string buffer
     *
     * @param tb The TwineBuilder containing the string to be updated
     * @param value the integer to be formatted (must be in the range 0..99
     * @return the TwineBuilder to be used for subsequent calls
     */

    protected static TwineBuilder appendTwoDigits(TwineBuilder tb, int value) {
        return tb.append((char) (value / 10 + '0')).append((char) (value % 10 + '0'));
    }

    private static class CalendarValueMapKey implements AtomicMatchKey {

        private final CalendarValue value;
        public CalendarValueMapKey(CalendarValue value) {
            this.value = value;
        }
        /**
         * Get an atomic value that encapsulates this match key. Needed to support the collation-key() function.
         *
         * @return an atomic value that encapsulates this match key
         */
        @Override
        public AtomicValue asAtomic() {
            return value;
        }

        @Override
        public boolean equals(Object obj) {
            if (obj instanceof CalendarValueMapKey) {
                CalendarValue a = value;
                CalendarValue b = ((CalendarValueMapKey)obj).value;
                if (a.hasTimezone() == b.hasTimezone()) {
                    if (a.hasTimezone()) {
                        return a.adjustTimezone(b.tzMinutes).isIdentical(b);
                    } else {
                        return a.isIdentical(b);
                    }
                } else {
                    return false;
                }
            } else {
                return false;
            }
        }

        @Override
        public int hashCode() {
            return asAtomic().hashCode();
        }
    }


    public static void validateDate(int year, int month, int day) throws XPathException {
        validateGYear(year);
        validateGMonthDay(month, day);

        if (month == 2 && day > 28 && !(CalendarValue.isLeapYear(year))) {
            throw new XPathException("There are only 28 days in February " + year + ", as it is not a leap year.", "FODT0006");
        }
    }

    public static void validateDate(IntegerValue year, IntegerValue month, IntegerValue day) throws XPathException {
        validateDate((int) year.longValue(), (int) month.longValue(), (int) day.longValue());
    }

    /**
     * Checks whether a gYear value is valid per the rules specified
     * in XPath and XQuery Functions and Operators 4.0.
     *
     * @param year - year number
     * @return true if the value in abides by the rules, otherwise throw an XPathException
     * @throws XPathException
     */
    public static void validateGYear(int year) throws XPathException {
        // "0000" is an illegal year value per XSD 1.0 (https://www.w3.org/TR/xmlschema-2/#truncatedformats)
        // but legal per XSD 1.1.
        // Internally, dates are stored per XSD 1.1 and adjusted to match XSD 1.0 on request.

        // Nothing extra to check.
    }

    public static void validateGYear(IntegerValue year) throws XPathException {
        validateGYear((int) year.longValue());
    }

    /**
     * Checks whether a gYearMonth value is valid per the rules specified
     * in XPath and XQuery Functions and Operators 4.0.
     *
     * @param year - a Gregorian year number
     * @param month - month number within the year
     * @return true if the value in abides by the rules, otherwise throw an XPathException
     * @throws XPathException
     */
    public static void validateGYearMonth(int year, int month) throws XPathException {
        validateGYear(year);
        validateGMonth(month);
    }

    /**
     * Checks whether a gMonth value is valid per the rules specified
     * in XPath and XQuery Functions and Operators 4.0.
     *
     * @param month - number of the month within an arbitrary year
     * @return true if the value in abides by the rules, otherwise throw an XPathException
     * @throws XPathException
     */
    public static void validateGMonth(int month) throws XPathException {
        if (month < 1 || month > 12) {
            throw new XPathException("The month must be between 1 and 12 (inclusive).", "FODT0006");
        }
    }

    public static void validateGMonth(IntegerValue month) throws XPathException {
        validateGMonth((int) month.longValue());
    }

    /**
     * Checks whether a gMonthDay value is valid per the rules specified
     * in XPath and XQuery Functions and Operators 4.0.
     *
     * @param month - month within an arbitrary year
     * @param day - day number within the month
     * @return true if the value in abides by the rules, otherwise throw an XPathException
     * @throws XPathException
     */
    public static void validateGMonthDay(int month, int day) throws XPathException {

        validateGDay(day);
        validateGMonth(month);

        switch (month) {
            case 2: // Feb, the day must be no more than 29
                if (day > 29) {
                    throw new XPathException("The day must be no more than 29 when the month is 2.", "FODT0006");
                }
                break;
            case 4: // Apr
            case 6: // Jun
            case 9: // Sep
            case 11: // Nov
                if (day > 30) {
                    throw new XPathException("The day must be no more than 30 when the month is " + month + ".", "FODT0006");
                }
        }
    }

    public static void validateGMonthDay(IntegerValue month, IntegerValue day) throws XPathException {
        validateGMonthDay((int) month.longValue(), (int) day.longValue());
    }

    /**
     * Checks whether a gDay value is valid per the rules specified
     * in XPath and XQuery Functions and Operators 4.0.
     *
     * @param day - number of a day within an arbitrary month
     * @return true if the value in abides by the rules, otherwise throw an XPathException
     * @throws XPathException
     */
    public static void validateGDay(int day) throws XPathException {
        if (day < 1 || day > 31) {
            throw new XPathException("The day must be between 1 and 31 (inclusive).", "FODT0006");
        }
    }

    public static void validateGDay(IntegerValue day) throws XPathException {
        validateGDay((int) day.longValue());
    }

    /**
     * Checks whether a time value is valid per the rules for time specified
     * in XPath and XQuery Functions and Operators 4.0.
     *
     * @param hour - number of hour
     * @param minute - number of minute
     * @param second - whole number value of second
     * @param nanosecond - fractional value of second, in nanoseconds
     * @throws XPathException
     */
    public static void validateTime(int hour, int minute, int second, int nanosecond) throws XPathException {

       validateHour(hour);
       validateMinute(minute);
       validateSecond(second);
       validateNanosecond(nanosecond);

        // If hour is 24, minute and second may only be 0
        if (hour == 24 && (minute > 0 || second > 0 || nanosecond > 0)) {
            throw new XPathException("The time may not be greater than '24:00:00'.");
        }
    }

    public static void validateTime(IntegerValue hour, IntegerValue minute, DecimalValue second) throws XPathException {
        validateTime((int) hour.longValue(), (int) minute.longValue(), (BigDecimal) second.getDecimalValue());
    }

    public static void validateTime(int hour, int minute, BigDecimal second) throws XPathException {
        validateTime(hour, minute, second.intValue(), nanosecondFromSecond(second));
    }

    /**
     * Check whether the hour component of a time falls within
     * the range specified in XSD 1.1 and XPath F&O 4.0.
     *
     * @param hour - the hour component of an arbitrary time value
     * @throws XPathException
     */
    public static void validateHour(int hour) throws XPathException {
        // XSD 1.0 stated that "24" was an invalid hour
        // but XSD 1.1 equates "24" to "00",
        // so the hour must be less than 25
        if (hour < 0 || hour > 24) {
            throw new XPathException("The hour component of a time must be between 0 and 24 (inclusive).", "FODT0006");
        }
    }

    public static void validateHour(IntegerValue hour) throws XPathException {
        validateHour((int) hour.longValue());
    }

    /**
     * Check whether the minute component of a time falls within
     * the range specified in XSD 1.1 and XPath F&O 4.0.
     *
     * @param minute - the minute component of an arbitrary time value
     * @throws XPathException
     */
    public static void validateMinute(int minute) throws XPathException {
        if (minute < 0 || minute > 59) {
            throw new XPathException("The minute component of a time must be between 0 and 59 (inclusive).", "FODT0006");
        }
    }

    public static void validateMinute(IntegerValue minute) throws XPathException {
        validateMinute((int) minute.longValue());
    }

    /**
     * Check whether the minute component of a time falls within
     * the range specified in XSD 1.1 and XPath F&O 4.0.
     *
     * @param second - the minute component of an arbitrary time value
     * @throws XPathException
     */
    public static void validateSecond(int second) throws XPathException {
        if (second < 0 || second > 59) {
            throw new XPathException("The second must be between 0 and 59 (inclusive).", "FODT0006");
        }
    }

    public static void validateSecond(DecimalValue second) throws XPathException {
        int[] partsOfSecond = splitSecond(second);
        validateSecond(partsOfSecond[0]);
        validateNanosecond(partsOfSecond[1]);
    }

    /**
     * Split a decimal second value into its component parts.
     *
     * @param second
     * @return the whole number; and the fractional value in nanoseconds
     */
    public static int[] splitSecond(DecimalValue second) {
        BigDecimal dSecond = (BigDecimal) second.getDecimalValue();
        return new int[]{dSecond.intValue(), nanosecondFromSecond(dSecond)};
    }

    /**
     * Check whether the nanosecond component of a time falls within
     * the range specified in XSD 1.1 and XPath F&O 4.0.
     *
     * @param nanosecond - the fractional value of an arbitrary second, in nanoseconds
     * @throws XPathException
     */
    public static void validateNanosecond(int nanosecond) throws XPathException {
        if (nanosecond < 0 || nanosecond > 999999999) {
            throw new XPathException("The nanosecond must be between 0 and 999999999 (inclusive)", "FODT0006");
        }
    }

    /**
     * Extract the fractional second (to the right of the decimal point)
     * from a decimal second value and return their value in nanosecond.
     *
     * @param second - a decimal number
     * @return the fractional value of a second, in nanosecond
     */
    public static int nanosecondFromSecond(BigDecimal second) {
        BigDecimal fractionalsecond = second.remainder(BigDecimal.ONE);
        return (fractionalsecond.multiply(BigDecimal.valueOf(1_000_000_000))).intValue();
    }

    /**
     * Checks whether a year value represents a leap year
     * in the Gregorian calendar, per XSD 1.1.
     *
     * @param year - the number of the year to check
     * @return true if year is a leap year, else false
     */
    public static boolean isLeapYear(int year) {
        if ((year % 4) == 0) {
            // Divisible by 4
            if ((year % 100) == 0) {
                // Divisible by 4 and 100
                if ((year % 400) == 0) {
                    // Divisible by 4, 100 and 400
                    return true;
                } else {
                    // Divisible by 4 and 100 but not 400
                    return false;
                }
            } else {
                // Divisible by 4 but not 100
                return true;
            }
        }
        return false;
    }
}

