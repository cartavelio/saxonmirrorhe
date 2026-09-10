////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.value;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.XPathComparable;
import net.sf.saxon.functions.AccessorFn;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.str.TwineBuilder;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.NoDynamicContextException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.util.*;

/**
 * A value of type xs:time
 */

public final class TimeValue extends CalendarValue implements XPathComparable {

    private final int hour;
    private final int minute;
    private final int second;
    private final int nanosecond;


    /**
     * Construct a time value given the hour, minute, second, and microsecond components.
     * This constructor performs no validation.
     *
     * @param hour        the hour value, 0-23
     * @param minute      the minutes value, 0-59
     * @param second      the seconds value, 0-59
     * @param microsecond the number of microseconds, 0-999999
     * @param tzMinutes   the timezone displacement in minutes from UTC. Supply the value
     *                    {@link CalendarValue#NO_TIMEZONE} if there is no timezone component.
     * @deprecated since 10.0: use the constructor {@link #TimeValue(int, int, int, int, int, AtomicType)}
     * that accepts nanosecond precision
     */

    @Deprecated
    public TimeValue(byte hour, byte minute, byte second, int microsecond, int tzMinutes) {
        super(BuiltInAtomicType.TIME, tzMinutes);
        this.hour = hour;
        this.minute = minute;
        this.second = second;
        this.nanosecond = microsecond * 1000;
    }

    /**
     * Private constructor to construct a time value given the hour, minute, second, and nanosecond components.
     * This constructor performs no validation.
     *
     * @param hour        the hour value, 0-23
     * @param minute      the minutes value, 0-59
     * @param second      the seconds value, 0-59
     * @param nanosecond  the number of microseconds, 0-999999
     * @param tzMinutes   the timezone displacement in minutes from UTC. Supply the value
     *                    {@link CalendarValue#NO_TIMEZONE} if there is no timezone component.
     * @param typeLabel   the type annotation (must be a subtype of xs:time)
     */

    public TimeValue(int hour, int minute, int second, int nanosecond, int tzMinutes, AtomicType typeLabel) {
        super(typeLabel, tzMinutes);
        this.hour = hour;
        this.minute = minute;
        this.second = second;
        this.nanosecond = nanosecond;
    }

    /**
     * Factory method to construct a time value given the hour, minute, second, and nanosecond components.
     * This constructor performs no validation.
     *
     * @param hour        the hour value, 0-23
     * @param minute      the minutes value, 0-59
     * @param second      the seconds value, 0-59
     * @param nanosecond  the number of nanoseconds, 0-999_999_999
     * @param tz          the timezone displacement in minutes from UTC. Supply the value
     *                    {@link CalendarValue#NO_TIMEZONE} if there is no timezone component.
     * @return the constructed time value
     */

    public TimeValue makeTimeValue(byte hour, byte minute, byte second, int nanosecond, int tz) {
        return new TimeValue(hour, minute, second, nanosecond, tz, BuiltInAtomicType.TIME);
    }

    private TimeValue(DateTimeValue.MutableDateTimeValue m) {
        super(m.typeLabel, m.tzMinutes);
        this.hour = m.hour;
        this.minute = m.minute;
        this.second = m.second;
        this.nanosecond = m.nanosecond;
    }

    //#if CSHARP==false
    /**
     * Constructor: create a time value given a Java calendar object
     *
     * @param calendar holds the date and time
     * @param tz       the timezone offset in minutes, or NO_TIMEZONE indicating that there is no timezone
     */

    public TimeValue(/*@NotNull*/ GregorianCalendar calendar, int tz) {
        super(BuiltInAtomicType.TIME, tz);
        hour = (byte) calendar.get(Calendar.HOUR_OF_DAY);
        minute = (byte) calendar.get(Calendar.MINUTE);
        second = (byte) calendar.get(Calendar.SECOND);
        nanosecond = calendar.get(Calendar.MILLISECOND) * 1_000_000;
    }
    //#endif

    /**
     * Static factory method: create a time value from a supplied string, in
     * ISO 8601 format
     *
     * @param s the time in the lexical format hh:mm:ss[.ffffff] followed optionally by
     *          timezone in the form [+-]hh:mm or Z
     * @return either a TimeValue corresponding to the xs:time, or a ValidationFailure
     *         if the supplied value was invalid
     */

    /*@NotNull*/
    public static ConversionResult makeTimeValue(UnicodeString s) {
        // input must have format hh:mm:ss[.fff*][([+|-]hh:mm | Z)]
        StringTokenizer tok = new StringTokenizer(Whitespace.trim(s).toString(), "-:.+Z", true);
        if (!tok.hasMoreTokens()) {
            return badTime("too short", s);
        }
        String part = tok.nextToken();

        if (part.length() != 2) {
            return badTime("hour must be two digits", s);
        }
        long value = DurationValue.simpleInteger(part);
        if (value < 0) {
            return badTime("Non-numeric hour component", s);
        }
        if (value > 24) {
            return badTime("hour is out of range", s);
        }
        byte hour = (byte) value;
        if (!tok.hasMoreTokens()) {
            return badTime("too short", s);
        }
        if (!":".equals(tok.nextToken())) {
            return badTime("wrong delimiter after hour", s);
        }

        if (!tok.hasMoreTokens()) {
            return badTime("too short", s);
        }
        part = tok.nextToken();
        if (part.length() != 2) {
            return badTime("minute must be two digits", s);
        }
        value = DurationValue.simpleInteger(part);
        if (value < 0) {
            return badTime("Non-numeric minute component", s);
        }
        if (value > 59) {
            return badTime("minute is out of range", s);
        }
        byte minute = (byte) value;
        if (hour == 24 && minute != 0) {
            return badTime("If hour is 24, minute must be 00", s);
        }
        if (!tok.hasMoreTokens()) {
            return badTime("too short", s);
        }
        if (!":".equals(tok.nextToken())) {
            return badTime("wrong delimiter after minute", s);
        }

        if (!tok.hasMoreTokens()) {
            return badTime("too short", s);
        }
        part = tok.nextToken();
        if (part.length() != 2) {
            return badTime("second must be two digits", s);
        }
        value = DurationValue.simpleInteger(part);
        if (value < 0) {
            return badTime("Non-numeric second component", s);
        }
        if (value > 59) {
            return badTime("second is out of range", s);
        }
        byte second = (byte) value;
        if (hour == 24 && second != 0) {
            return badTime("If hour is 24, second must be 00", s);
        }

        int tz = NO_TIMEZONE;
        boolean negativeTz = false;
        int state = 0;
        int nanosecond = 0;
        while (tok.hasMoreTokens()) {
            if (state == 9) {
                return badTime("characters after the end", s);
            }
            String delim = tok.nextToken();
            if (".".equals(delim)) {
                if (state != 0) {
                    return badTime("decimal separator occurs twice", s);
                }
                if (!tok.hasMoreTokens()) {
                    return badTime("decimal point must be followed by digits", s);
                }
                part = tok.nextToken();
                if (part.length() > 9 && part.matches("^[0-9]+$")) {
                    part = part.substring(0, 9);
                }
                value = DurationValue.simpleInteger(part);
                if (value < 0) {
                    return badTime("Non-numeric fractional seconds component", s);
                }
                double fractionalSeconds = Double.parseDouble('.' + part);
                nanosecond = (int) Math.round(fractionalSeconds * 1_000_000_000);
                if (hour == 24 && nanosecond != 0) {
                    return badTime("If hour is 24, fractional seconds must be 0", s);
                }
                state = 1;
            } else if ("Z".equals(delim)) {
                if (state > 1) {
                    return badTime("Z cannot occur here", s);
                }
                tz = 0;
                state = 9;  // we've finished
            } else if ("+".equals(delim) || "-".equals(delim)) {
                if (state > 1) {
                    return badTime(delim + " cannot occur here", s);
                }
                state = 2;
                if (!tok.hasMoreTokens()) {
                    return badTime("missing timezone", s);
                }
                part = tok.nextToken();
                if (part.length() != 2) {
                    return badTime("timezone hour must be two digits", s);
                }
                value = DurationValue.simpleInteger(part);
                if (value < 0) {
                    return badTime("Non-numeric timezone hour component", s);
                }
                if (value > 14) {
                    return badTime("timezone hour is out of range", s);
                }
                tz = (int)value * 60;
                if ("-".equals(delim)) {
                    negativeTz = true;
                }
            } else if (":".equals(delim)) {
                if (state != 2) {
                    return badTime("colon cannot occur here", s);
                }
                state = 9;
                part = tok.nextToken();
                value = DurationValue.simpleInteger(part);
                if (value < 0) {
                    return badTime("Non-numeric timezone minute component", s);
                }

                if (part.length() != 2) {
                    return badTime("timezone minute must be two digits", s);
                }
                if (value > 59) {
                    return badTime("timezone minute is out of range", s);
                }
                int tzminute = (int)value;
                tz += tzminute;
                if (negativeTz) {
                    tz = -tz;
                }
            } else {
                return badTime("timezone format is incorrect", s);
            }
        }

        if (state == 2 || state == 3) {
            return badTime("timezone incomplete", s);
        }

        if (hour == 24) {
            hour = 0;
        }

        return new TimeValue(hour, minute, second, nanosecond, tz, BuiltInAtomicType.TIME);
    }

    /**
     * Creates an instance of TimeValue.  Includes validation
     * checks.  If a validation error is detected, an instance of
     * ValidationFailure will be returned instead.
     *
     * @param hour - hour number within an arbitrary day
     * @param minute - minute within the hour specified
     * @param second - second within the minute specified
     * @param nanosecond - nanosecond within the second specified
     * @param timezoneInMinutes - number of minutes to adjust by for the timezone
     * @return - an instance of TimeValue or ValidationFailure
     */
    public static ConversionResult makeTimeValue(int hour, int minute, int second, int nanosecond, int timezoneInMinutes) {
        DateTimeValue.MutableDateTimeValue g = new DateTimeValue.MutableDateTimeValue();
        g.typeLabel = BuiltInAtomicType.TIME;
        if (g.tzMinutes != timezoneInMinutes) {
            TimeValue.setValues(g, hour, minute, second, nanosecond, timezoneInMinutes);
        } else {
            TimeValue.setValues(g, hour, minute, second, nanosecond);
        }
        if (g.error != null) return g.error;
        return new TimeValue(g);
    }

    public static ConversionResult makeTimeValue(int hour, int minute, int second, int nanosecond) {
        return TimeValue.makeTimeValue(hour, minute, second, nanosecond, CalendarValue.NO_TIMEZONE);
    }

    /**
     * Checks whether a gDay value in an arbitrary month and year
     * is valid per the rules specified in XPath and XQuery Functions
     * and Operators 4.0.
     * If the input values are valid, they will be assigned to their
     * corresponding attributes in the input instance of MutableGDateValue.
     * If an error is found, the details will be assigned to the
     * error attribute of the input instance of MutableGDateValue.
     *
     * @param m - a mutable placeholder for storing the instance values during validation checks
     * @param hour - hour number within an arbitrary day
     * @param minute - minute within the hour specified
     * @param second - second within the minute specified
     * @param nanosecond - nanosecond within the second specified
     * @param timezoneInMinutes - number of minutes to adjust by for the timezone
     * @return the input instance of MutableTimeValue, with the attributes set to their corresponding input values
     */
    protected static void setValues(DateTimeValue.MutableDateTimeValue m, int hour, int minute, int second, int nanosecond, int timezoneInMinutes) {
        TimeValue.setValues(m, hour, minute, second, nanosecond);
        if (m.error != null) return;
        DateTimeValue.setTimezoneInMinutes(m, timezoneInMinutes);
    }

    protected static void setValues(DateTimeValue.MutableDateTimeValue m, int hour, int minute, int second, int nanosecond) {
        // XSD 1.0 stated that "24" was an invalid hour
        // but XSD 1.1 equates "24" to "00",
        // so the hour must be less than 25
        if (hour < 0 || hour > 24) {
            m.error = badTimeComponent("Hour must be between 0 and 24 (inclusive).", hour, "FODT0006");
            return;
        }
        m.hour = (byte) hour;
        if (minute < 0 || minute > 59) {
            m.error = badTimeComponent("Minute must be between 0 and 59 (inclusive).", minute, "FODT0006");
            return;
        }
        m.minute = (byte) minute;
        if (second < 0 || second > 59) {
            m.error = badTimeComponent("Second must be between 0 and 59 (inclusive).", second, "FODT0006");
            return;
        }
        m.second = (byte) second;
        if (nanosecond < 0 || nanosecond > 999999999) {
            m.error = badTimeComponent("Nanosecond must be between 0 and 999999999 (inclusive).", nanosecond, "FODT0006");
            return;
        }
        m.nanosecond = nanosecond;
    }

    /*@NotNull*/
    private static ValidationFailure badTime(String msg, UnicodeString value) {
        ValidationFailure err = new ValidationFailure(
                "Invalid time " + Err.wrap(value, Err.VALUE) + " (" + msg + ")");
        err.setErrorCode("FORG0001");
        return err;
    }

    protected static ValidationFailure badTimeComponent(String msg, int value, String errorCode) {
        ValidationFailure err = new ValidationFailure(
                "Invalid time component: " + Err.wrap(value + "", Err.VALUE) + " (" + msg + ")");
        err.setErrorCode(errorCode);
        return err;
    }

    /**
     * Determine the primitive type of the value. This delivers the same answer as
     * getItemType().getPrimitiveItemType(). The primitive types are
     * the 19 primitive types of XML Schema, plus xs:integer, xs:dayTimeDuration and xs:yearMonthDuration,
     * and xs:untypedAtomic. For external objects, the result is AnyAtomicType.
     */

    /*@NotNull*/
    @Override
    public BuiltInAtomicType getPrimitiveType() {
        return BuiltInAtomicType.TIME;
    }

    /**
     * Get the hour component, 0-23
     *
     * @return the hour
     */

    public int getHour() {
        return hour;
    }

    /**
     * Get the minute component, 0-59
     *
     * @return the minute
     */

    public int getMinute() {
        return minute;
    }

    /**
     * Get the second component, 0-59
     *
     * @return the second
     */

    public int getSecond() {
        return second;
    }

    /**
     * Get the nanosecond component, 0-999_999
     *
     * @return the nanoseconds
     */

    public int getNanosecond() {
        return nanosecond;
    }


    /**
     * Convert to string
     *
     * @return ISO 8601 representation, in the localized timezone
     *         (the timezone held within the value).
     */

    /*@NotNull*/
    @Override
    public UnicodeString getPrimitiveStringValue() {

        TwineBuilder tb = TwineBuilder.make(16);;

        tb = appendTwoDigits(tb, hour).append(':');
        tb = appendTwoDigits(tb, minute).append(':');
        tb = appendTwoDigits(tb, second);
        if (nanosecond != 0) {
            tb = tb.append('.');
            int ms = nanosecond;
            int div = 100_000_000;
            while (ms > 0) {
                int d = ms / div;
                tb = tb.append((char) (d + '0'));
                ms = ms % div;
                div /= 10;
            }
        }

        if (hasTimezone()) {
            tb = appendTimezone(tb);
        }

        return tb.toUnicodeString();

    }


    /**
     * Get the canonical lexical representation as defined in XML Schema. This is not always the same
     * as the result of casting to a string according to the XPath rules. For an xs:time it is the
     * time adjusted to UTC
     *
     * @return the canonical lexical representation if defined in XML Schema
     */

    @Override
    public UnicodeString getCanonicalLexicalRepresentation() {
        if (hasTimezone() && getTimezoneInMinutes() != 0) {
            return adjustTimezone(0).getUnicodeStringValue();
        } else {
            return this.getUnicodeStringValue();
        }
    }


    /**
     * Convert to a DateTime value. The date components represent a reference date, as defined
     * in the spec for comparing times.
     */

    /*@NotNull*/
    @Override
    public DateTimeValue toDateTime() {
        return new DateTimeValue(1972, (byte) 12, (byte) 31, hour, minute, second, nanosecond, getTimezoneInMinutes());
    }

    //#if CSHARP==false
    /**
     * Get a Java Calendar object corresponding to this time, on a reference date
     */

    /*@NotNull*/
    @Override
    public GregorianCalendar getCalendar() {
        // create a calendar using the specified timezone
        int tz = hasTimezone() ? getTimezoneInMinutes()*60000 : 0;
        TimeZone zone = new SimpleTimeZone(tz, "LLL");
        GregorianCalendar calendar = new GregorianCalendar(zone);
        calendar.setLenient(false);
        if (tz < calendar.getMinimum(Calendar.ZONE_OFFSET) || tz > calendar.getMaximum(Calendar.ZONE_OFFSET)) {
            return adjustTimezone(0).getCalendar();
        }

        // use a reference date of 1972-12-31
        calendar.set(1972, Calendar.DECEMBER, 31, hour, minute, second);
        calendar.set(Calendar.MILLISECOND, nanosecond / 1_000_000);
        calendar.set(Calendar.ZONE_OFFSET, tz);
        calendar.set(Calendar.DST_OFFSET, 0);

        calendar.getTime();
        return calendar;
    }
    //#endif

    /**
     * Make a copy of this time value,
     * but with a different type label
     *
     * @param metadata the new type label. This must be a subtype of xs:time.
     */

    /*@NotNull*/
    @Override
    public AtomicValue withMetadata(AtomicMetadata metadata) {
        return new TimeValue(hour, minute, second, nanosecond, getTimezoneInMinutes(), metadata.getType());
    }

    /**
     * Return a new time with the same normalized value, but
     * in a different timezone. This is called only for a TimeValue that has an explicit timezone
     *
     * @param timezone the new timezone offset, in minutes
     * @return the time in the new timezone. This will be a new TimeValue unless no change
     *         was required to the original value
     */

    /*@NotNull*/
    @Override
    public TimeValue adjustTimezone(int timezone) {
        DateTimeValue dt = toDateTime().adjustTimezone(timezone);
        return new TimeValue(dt.getHour(), dt.getMinute(), dt.getSecond(),
                dt.getNanosecond(), dt.getTimezoneInMinutes(), BuiltInAtomicType.TIME);
    }

    /**
     * Get a component of the value. Returns null if the requested component is not present.
     * @param component the required component
     */

    /*@Nullable*/
    @Override
    public AtomicValue getComponent(AccessorFn.Component component) throws XPathException {
        switch (component) {
            case HOURS:
                return Int64Value.makeIntegerValue(hour);
            case MINUTES:
                return Int64Value.makeIntegerValue(minute);
            case SECONDS:
                BigDecimal d = BigDecimal.valueOf(nanosecond);
                d = d.divide(BigDecimalValue.ONE_BILLION, 9, RoundingMode.DOWN);
                d = d.add(BigDecimal.valueOf(second));
                return new BigDecimalValue(d);
            case WHOLE_SECONDS: //(internal use only)
                return Int64Value.makeIntegerValue(second);
            case MICROSECONDS:
                return new Int64Value(nanosecond / 1000);
            case NANOSECONDS:
                return new Int64Value(nanosecond);
            case TIMEZONE:
                if (hasTimezone()) {
                    return DayTimeDurationValue.fromMilliseconds(60000L * getTimezoneInMinutes());
                } else {
                    return null;
                }
            case YEAR:
            case MONTH:
            case DAY:
                return null;
            default:
                throw new IllegalArgumentException("Unknown component for time: " + component);
        }
    }

    @Override
    public XPathComparable getXPathComparable(StringCollator collator, int implicitTimezone, int specVersion) throws NoDynamicContextException {
        if (hasTimezone()) {
            return this;
        } else if (implicitTimezone == MISSING_TIMEZONE) {
            throw new NoDynamicContextException("Unknown implicit timezone");
        } else {
            return adjustTimezone(implicitTimezone);
        }
    }

    /**
     * Compare the value to another dateTime value
     *
     * @param other The other dateTime value
     * @return negative value if this one is the earler, 0 if they are chronologically equal,
     *         positive value if this one is the later. For this purpose, dateTime values with an unknown
     *         timezone are considered to be UTC values (the Comparable interface requires
     *         a total ordering).
     * @throws ClassCastException if the other value is not a TimeValue (the parameter
     *                            is declared as Object to satisfy the Comparable interface)
     */

    public int compareTo(XPathComparable other) {
        if (other instanceof TimeValue) {
            TimeValue otherTime = (TimeValue) other;
            if (getTimezoneInMinutes() == otherTime.getTimezoneInMinutes()) {
                if (hour != otherTime.hour) {
                    return IntegerValue.signum(hour - otherTime.hour);
                } else if (minute != otherTime.minute) {
                    return IntegerValue.signum(minute - otherTime.minute);
                } else if (second != otherTime.second) {
                    return IntegerValue.signum(second - otherTime.second);
                } else if (nanosecond != otherTime.nanosecond) {
                    return IntegerValue.signum(nanosecond - otherTime.nanosecond);
                } else {
                    return 0;
                }
            } else {
                return toDateTime().compareTo(otherTime.toDateTime());
            }
        } else {
            throw new ClassCastException("Cannot compare xs:time to " + other);
        }
    }

    /**
     * Compare the value to another dateTime value
     *
     * @param other   The other dateTime value
     * @param implicitTimezone The implicit timezone assumed for a value with no timezone
     * @return negative value if this one is the earler, 0 if they are chronologically equal,
     *         positive value if this one is the later. For this purpose, dateTime values with an unknown
     *         timezone are considered to be UTC values (the Comparable interface requires
     *         a total ordering).
     * @throws ClassCastException        if the other value is not a DateTimeValue (the parameter
     *                                   is declared as Object to satisfy the Comparable interface)
     * @throws NoDynamicContextException if the implicit timezone is required and is not available
     *                                   (because the function is called at compile time)
     */

    @Override
    public int compareTo(/*@NotNull*/ CalendarValue other, /*@NotNull*/ int implicitTimezone) throws NoDynamicContextException {
        if (!(other instanceof TimeValue)) {
            throw new ClassCastException("Time values are not comparable to " + other.getClass());
        }
        TimeValue otherTime = (TimeValue) other;
        if (getTimezoneInMinutes() == otherTime.getTimezoneInMinutes()) {
            // The values have the same time zone, or neither has a timezone
            return compareTo(otherTime);
        } else {
            return toDateTime().compareTo(otherTime.toDateTime(), implicitTimezone);
        }
    }


    /*@NotNull*/
    public TimeComparable getSchemaComparable() {
        return new TimeComparable(this);
    }

    public static class TimeComparable implements Comparable<TimeComparable> {

        private final TimeValue value;
        public TimeComparable(TimeValue value) {
            this.value = value;
        }

        /*@NotNull*/
        public TimeValue asTimeValue() {
            return value;
        }

        @Override
        public int compareTo(TimeComparable o) {
            DateTimeValue dt0 = asTimeValue().toDateTime();
            DateTimeValue dt1 = o.asTimeValue().toDateTime();
            return dt0.getSchemaComparable().compareTo(dt1.getSchemaComparable());
        }

        public boolean equals(/*@NotNull*/ Object o) {
            return o instanceof TimeComparable && compareTo((TimeComparable)o) == 0;
        }

        public int hashCode() {
            return value.toDateTime().getSchemaComparable().hashCode();
        }
    }


    public boolean equals(Object other) {
        return other instanceof TimeValue && compareTo((TimeValue)other) == 0;
    }

    public int hashCode() {
        return DateTimeValue.computeHashCode(
                1951, 10, 11, hour, minute, second, nanosecond, getTimezoneInMinutes());
    }

    /**
     * Add a duration to a dateTime
     *
     * @param duration the duration to be added (may be negative)
     * @return the new date
     * @throws net.sf.saxon.trans.XPathException
     *          if the duration is an xs:duration, as distinct from
     *          a subclass thereof
     */

    /*@NotNull*/
    @Override
    public TimeValue add(/*@NotNull*/ DurationValue duration) throws XPathException {
        if (duration instanceof DayTimeDurationValue) {
            DateTimeValue dt = toDateTime().add(duration);
            return new TimeValue(dt.getHour(), dt.getMinute(), dt.getSecond(),
                    dt.getNanosecond(), getTimezoneInMinutes(), BuiltInAtomicType.TIME);
        } else {
            throw new XPathException("Time+Duration arithmetic is supported only for xs:dayTimeDuration", "XPTY0004")
                    .asTypeError();
        }
    }

    /**
     * Determine the difference between two points in time, as a duration
     *
     * @param other   the other point in time
     * @param context XPath dynamic evaluation context
     * @return the duration as an xs:dayTimeDuration
     * @throws XPathException for example if one value is a date and the other is a time
     */

    @Override
    public DayTimeDurationValue subtract(/*@NotNull*/ CalendarValue other, XPathContext context) throws XPathException {
        if (!(other instanceof TimeValue)) {
            XPathException err = new XPathException("First operand of '-' is a time, but the second is not");
            err.setIsTypeError(true);
            throw err;
        }
        return super.subtract(other, context);
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
     * Extract the fractional second (to the right of the decimal point)
     * from a decimal second value and return its value in nanoseconds.
     *
     * @param second - a decimal number
     * @return the fractional value of a second, in nanoseconds
     */
    public static int nanosecondFromSecond(BigDecimal second) {
        BigDecimal fractionalSecond = second.remainder(BigDecimal.ONE);
        return (fractionalSecond.multiply(BigDecimal.valueOf(1_000_000_000))).intValue();
    }

}

