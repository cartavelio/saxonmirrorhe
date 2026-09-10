////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.value;

import net.sf.saxon.expr.sort.XPathComparable;
import net.sf.saxon.functions.AccessorFn;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.str.BMPString;
import net.sf.saxon.str.TwineBuilder;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.NoDynamicContextException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;

import java.util.EnumSet;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * Implementation of the xs:gYear data type
 */

public class GMonthDayValue extends GDateValue {

    private static final Pattern regex =
            Pattern.compile("--([0-9][0-9]-[0-9][0-9])(Z|[+-][0-9][0-9]:[0-9][0-9])?");

    private GMonthDayValue(MutableGDateValue m) {
        super(m);
    }

    private GMonthDayValue(DateTimeValue.MutableDateTimeValue m) {
        super(m);
    }

    protected EnumSet<AccessorFn.Component> getDefinedComponents() {
        return EnumSet.of(AccessorFn.Component.MONTH, AccessorFn.Component.DAY);
    }

    public static ConversionResult makeGMonthDayValue(UnicodeString value) {
        final UnicodeString trimmed = Whitespace.trim(value);
        Matcher m = regex.matcher(trimmed.toString());
        if (!m.matches()) {
            return new ValidationFailure("Cannot convert '" + value + "' to a gMonthDay");
        }
        MutableGDateValue g = new MutableGDateValue();
        String base = m.group(1);
        String tz = m.group(2);
        String date = "2000-" + base + (tz == null ? "" : tz);
        g.typeLabel = BuiltInAtomicType.G_MONTH_DAY;
        setLexicalValue(g, BMPString.of(date), true);
        if (g.error != null) {
            return g.error;
        }
        return new GMonthDayValue(g);
    }

    /**
     * Creates an instance of GMonthDayValue.  Includes validation
     * checks.  If a validation error is detected, an instance of
     * ValidationFailure will be returned instead.
     *
     * @param month - number of a month within an arbitrary year
     * @param day - number of a day within the specified month
     * @param timezoneInMinutes - number of minutes to adjust by for the timezone
     * @return - an instance of GMonthDayValue or ValidationFailure
     */
    public static ConversionResult makeGMonthDayValue(int month, int day, int timezoneInMinutes) {
        DateTimeValue.MutableDateTimeValue g = new DateTimeValue.MutableDateTimeValue();
        g.typeLabel = BuiltInAtomicType.G_MONTH_DAY;
        if (g.tzMinutes != timezoneInMinutes) {
            GMonthDayValue.setValues(g, month, day, timezoneInMinutes);
        } else {
            GMonthDayValue.setValues(g, month, day);
        }
        if (g.error != null) return g.error;
        return new GMonthDayValue(g);
    }

    public static ConversionResult makeGMonthDayValue(int month, int day) {
        return GMonthDayValue.makeGMonthDayValue(month, day, CalendarValue.NO_TIMEZONE);
    }

    /**
     * Checks whether the component parts of a gMonthDay value, in an
     * arbitrary year, are valid per the rules specified in XPath and
     * XQuery Functions and Operators 4.0.
     * If the input values are valid, they will be assigned to their
     * corresponding attributes in the input instance of MutableGDateValue.
     * If an error is found, the details will be assigned to the
     * error attribute of the input instance of MutableGDateValue.
     *
     * @param m - a mutable placeholder for storing the instance values during validation checks
     * @param month - number of a month within an arbitrary year
     * @param day - number of a day within the specified month
     * @param timezoneInMinutes - number of minutes to adjust by for the timezone
     * @return the input instance of MutableGDateValue, updated with the values supplied
     */
    protected static void setValues(DateTimeValue.MutableDateTimeValue m, int month, int day, int timezoneInMinutes) {
        GMonthDayValue.setValues(m, month, day);
        if (m.error != null) return;
        DateTimeValue.setTimezoneInMinutes(m, timezoneInMinutes);
    }

    protected static void setValues(DateTimeValue.MutableDateTimeValue m, int month, int day) {
        GMonthValue.setValues(m, month); // Includes check that month is between 1-12 (inc)
        if (m.error != null) return;

        GDayValue.setValues(m, day); // Includes check that day is between 1-31 (inc)
        if (m.error != null) return;

        switch (month) {
            case 2: // Feb, the day must be no more than 29
                if (day > 29) {
                    m.error = badDateComponent("Day must be no more than 29 when the month is 2.", day, "FODT0006");
                }
                break;
            case 4: // Apr
            case 6: // Jun
            case 9: // Sep
            case 11: // Nov
                if (day > 30) {
                    m.error = badDateComponent("Day must be no more than 30 when the month is " + month + ".", day, "FODT0006");
                }
                break; // C# needs final case to be clearly terminated
        }
    }

    /**
     * Constructs an instance of GMonthDayValue without any validation
     * of the input values.
     *
     * @param month - number of a month within an arbitrary year
     * @param day - number of a day within the specified month
     * @param tz - number of minutes to adjust by for the timezone
     */
    public GMonthDayValue(int month, int day, int tz) {
        this(month, day, tz, BuiltInAtomicType.G_MONTH_DAY);
    }

    public GMonthDayValue(int month, int day, int tz, AtomicType type) {
        this(new MutableGDateValue(2000, month, day, false, tz, type));
    }

    /**
     * Make a copy of this date, time, or dateTime value
     *
     * @param metadata the type label of the new copy. The caller is responsible for checking that
     *                  the value actually conforms to this type.
     * @return the copied value
     */

    @Override
    public AtomicValue withMetadata(AtomicMetadata metadata) {
        MutableGDateValue m = makeMutableCopy();
        m.typeLabel = metadata.getType();
        return new GMonthDayValue(m);
    }


    @Override
    public XPathComparable getXPathComparable(StringCollator collator, int implicitTimezone, int specVersion) throws NoDynamicContextException {
        return specVersion >= 40 ? new DateValue(1972, month, day)
                .adjustTimezone(hasTimezone() ? getTimezoneInMinutes() : implicitTimezone) : null;
    }

    /**
     * Determine the primitive type of the value. This delivers the same answer as
     * getItemType().getPrimitiveItemType(). The primitive types are
     * the 19 primitive types of XML Schema, plus xs:integer, xs:dayTimeDuration and xs:yearMonthDuration,
     * and xs:untypedAtomic. For external objects, the result is AnyAtomicType.
     */

    @Override
    public BuiltInAtomicType getPrimitiveType() {
        return BuiltInAtomicType.G_MONTH_DAY;
    }

    /*@NotNull*/
    @Override
    public UnicodeString getPrimitiveStringValue() {

        TwineBuilder tb = TwineBuilder.make(16);

        tb = tb.append('-').append('-');
        tb = appendTwoDigits(tb, month).append('-');
        tb = appendTwoDigits(tb, day);

        if (hasTimezone()) {
            tb = appendTimezone(tb);
        }

        return tb.toUnicodeString();

    }

    /**
     * Add a duration to this date/time value
     *
     * @param duration the duration to be added (which might be negative)
     * @return a new date/time value representing the result of adding the duration. The original
     *         object is not modified.
     * @throws net.sf.saxon.trans.XPathException if an error is detected
     *
     */

    @Override
    public CalendarValue add(DurationValue duration) throws XPathException {
        throw new XPathException("Cannot add a duration to an xs:gMonthDay", "XPTY0004").asTypeError();
    }

    /**
     * Return a new date, time, or dateTime with the same normalized value, but
     * in a different timezone
     *
     * @param tz the new timezone, in minutes
     * @return the date/time in the new timezone
     */

    @Override
    public CalendarValue adjustTimezone(int tz) {
        DateTimeValue dt = toDateTime().adjustTimezone(tz);
        return new GMonthDayValue(dt.getMonth(), dt.getDay(), dt.getTimezoneInMinutes());
    }
}

