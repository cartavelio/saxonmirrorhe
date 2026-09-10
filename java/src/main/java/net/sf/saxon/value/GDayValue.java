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
import net.sf.saxon.str.*;
import net.sf.saxon.trans.NoDynamicContextException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;

import java.util.EnumSet;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * Implementation of the xs:gDay data type
 */

public class GDayValue extends GDateValue {

    private static final Pattern regex =
            Pattern.compile("---([0-9][0-9])(Z|[+-][0-9][0-9]:[0-9][0-9])?");

    private GDayValue(MutableGDateValue m) {
        super(m);
    }

    private GDayValue(DateTimeValue.MutableDateTimeValue m) {
        super(m);
    }

    protected EnumSet<AccessorFn.Component> getDefinedComponents() {
        return EnumSet.of(AccessorFn.Component.DAY);
    }

    public static ConversionResult makeGDayValue(UnicodeString value) {
        final UnicodeString trimmed = Whitespace.trim(value);
        Matcher m = regex.matcher(trimmed.toString());
        if (!m.matches()) {
            return new ValidationFailure("Cannot convert '" + value + "' to a gDay");
        }
        MutableGDateValue g = new MutableGDateValue();
        String base = m.group(1);
        String tz = m.group(2);
        String date = "2000-01-" + base + (tz == null ? "" : tz);
        g.typeLabel = BuiltInAtomicType.G_DAY;
        setLexicalValue(g, BMPString.of(date), true);
        if (g.error != null) {
            return g.error;
        }
        return new GDayValue(g);
    }

    /**
     * Creates an instance of GDayValue.  Includes validation
     * checks.  If a validation error is detected, an instance of
     * ValidationFailure will be returned instead.
     *
     * @param day - number of a day within an arbitrary month
     * @param timezoneInMinutes - number of minutes to adjust by for the timezone
     * @return - an instance of GDayValue
     */
    public static ConversionResult makeGDayValue(int day, int timezoneInMinutes) {
        DateTimeValue.MutableDateTimeValue g = new DateTimeValue.MutableDateTimeValue();
        g.typeLabel = BuiltInAtomicType.G_DAY;
        if (g.tzMinutes != timezoneInMinutes) {
            GDayValue.setValues(g, day, timezoneInMinutes);
        } else {
            GDayValue.setValues(g, day);
        }
        if (g.error != null) return g.error;
        return new GDayValue(g);
    }

    public static ConversionResult makeGDayValue(int day) {
        return GDayValue.makeGDayValue(day, CalendarValue.NO_TIMEZONE);
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
     * @param day - number of a day within an arbitrary month
     * @param timezoneInMinutes - number of minutes to adjust by for the timezone
     * @return the input instance of MutableDGateValue, with the day attribute set to the input day number
     */
    protected static void setValues(DateTimeValue.MutableDateTimeValue m, int day, int timezoneInMinutes) {
        GDayValue.setValues(m, day);
        if (m.error != null) return;
        DateTimeValue.setTimezoneInMinutes(m, timezoneInMinutes);
    }

    protected static void setValues(DateTimeValue.MutableDateTimeValue m, int day) {
        if (day < 1 || day > 31) {
            m.error = badDateComponent("Day must be between 1 and 31 (inclusive).", day, "FODT0006");
            return;
        }
        m.day = (byte) day;
    }

    /**
     * Constructs an instance of GDayValue without any validation
     * of the input values.
     *
     * @param day - number of a day within an arbitrary month
     * @param tz - number of minutes to adjust by for the timezone
     * @return - an instance of GDayValue
     */
    public GDayValue(int day, int tz) {
        this(day, tz, BuiltInAtomicType.G_DAY);
    }

    public GDayValue(int day, int tz, AtomicType type) {
        this(new MutableGDateValue(2000, 1, day, false, tz, type));
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
        return new GDayValue(m);
    }


    @Override
    public XPathComparable getXPathComparable(StringCollator collator, int implicitTimezone, int specVersion) throws NoDynamicContextException {
        return specVersion >= 40 ? new DateValue(1972, (byte)12, day)
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
        return BuiltInAtomicType.G_DAY;
    }

    /*@NotNull*/
    @Override
    public UnicodeString getPrimitiveStringValue() {

        TwineBuilder tb = TwineBuilder.make(16);

        tb = tb.append('-').append('-').append('-');
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
        throw new XPathException("Cannot add a duration to an xs:gDay", "XPTY0004").asTypeError();
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
        return new GDayValue(dt.getDay(), dt.getTimezoneInMinutes());
    }
}

