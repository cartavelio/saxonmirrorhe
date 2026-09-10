////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.value;

import net.sf.saxon.expr.sort.XPathComparable;
import net.sf.saxon.functions.AccessorFn;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.str.*;
import net.sf.saxon.trans.NoDynamicContextException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;

import java.util.EnumSet;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * Implementation of the xs:gYear data type
 */

public class GYearValue extends GDateValue {

    private static final Pattern regex =
            Pattern.compile("(-?[0-9]+)(Z|[+-][0-9][0-9]:[0-9][0-9])?");

    private GYearValue(MutableGDateValue m) {
        super(m);
    }

    private GYearValue(DateTimeValue.MutableDateTimeValue m) {
        super(m);
    }

    public static ConversionResult makeGYearValue(UnicodeString value, ConversionRules rules) {
        MutableGDateValue g = new MutableGDateValue();
        final UnicodeString trimmed = Whitespace.trim(value);
        Matcher m = regex.matcher(trimmed.toString());
        if (!m.matches()) {
            return new ValidationFailure("Cannot convert '" + value + "' to a gYear");
        }
        String base = m.group(1);
        String tz = m.group(2);
        String date = base + "-01-01" + (tz == null ? "" : tz);
        g.typeLabel = BuiltInAtomicType.G_YEAR;
        setLexicalValue(g, BMPString.of(date), rules.isAllowYearZero());
        if (g.error != null) {
            return g.error;
        }
        return new GYearValue(g);
    }

    /**
     * Creates an instance of GYearValue.  Includes validation
     * checks.  If a validation error is detected, an instance of
     * ValidationFailure will be returned instead.
     *
     * @param year - number of a year in the Gregorian calendar
     * @param timezoneInMinutes - number of minutes to adjust by for the timezone
     * @param allowYearZero - true if the year '0000' is allowed (per XSD 1.1), false if it is not (per XSD 1.0)
     * @return an instance of GYearValue or ValidationFailure
     */
    public static ConversionResult makeGYearValue(int year, int timezoneInMinutes, boolean allowYearZero) {
        DateTimeValue.MutableDateTimeValue g = new DateTimeValue.MutableDateTimeValue();
        g.typeLabel = BuiltInAtomicType.G_YEAR;
        if (g.tzMinutes != timezoneInMinutes) {
            GYearValue.setValues(g, year, timezoneInMinutes, allowYearZero);
        } else {
            GYearValue.setValues(g, year, allowYearZero);
        }
        if (g.error != null) return g.error;
        return new GYearValue(g);
    }

    public static ConversionResult makeGYearValue(int year, boolean allowYearZero) {
        return GYearValue.makeGYearValue(year, CalendarValue.NO_TIMEZONE, allowYearZero);
    }

    /**
     * Checks whether the component parts of a gYear value
     * are valid per the rules specified in XPath and XQuery Functions
     * and Operators 4.0.
     * If the input values are valid, they will be assigned to their
     * corresponding attributes in the input instance of MutableGDateValue.
     * If an error is found, the details will be assigned to the
     * error attribute of the input instance of MutableGDateValue.
     *
     * @param m - a mutable placeholder for storing the instance values during validation checks
     * @param year - number of a year in the Gregorian calendar
     * @param timezoneInMinutes - number of minutes to adjust by for the timezone
     * @param allowYearZero - true if the year '0000' is allowed (per XSD 1.1), false if it is not (per XSD 1.0)
     */
    protected static void setValues(DateTimeValue.MutableDateTimeValue m, int year, int timezoneInMinutes, boolean allowYearZero) {
        GYearValue.setValues(m, year, allowYearZero);
        if (m.error != null) return;
        DateTimeValue.setTimezoneInMinutes(m, timezoneInMinutes);
    }

    protected static void setValues(DateTimeValue.MutableDateTimeValue m, int year, boolean allowYearZero) {
        m.hasNoYearZero = !allowYearZero;

        // "0000" is an illegal year value per XSD 1.0 (https://www.w3.org/TR/xmlschema-2/#truncatedformats)
        // but legal per XSD 1.1.
        if (year == 0 && !allowYearZero ) {
            m.error = badDateComponent("Year zero is not allowed", year, "FODT0006");
            return;
        }

        // Internally, dates are stored per XSD 1.1 and adjusted to match XSD 1.0 on request.
        m.year = year;
    }


    /**
     * Constructs an instance of GYearValue without any validation
     * of the input values.
     *
     * @param year - number of a year in the Gregorian calendar
     * @param tz - number of minutes to adjust by for the timezone
     * @param xsd10 - false if the year '0000' is allowed (per XSD 1.1), true if it is not (per XSD 1.0)
     */
    public GYearValue(int year, int tz, boolean xsd10) {
        this(new MutableGDateValue(year, 1, 1, xsd10,tz, BuiltInAtomicType.G_YEAR));
    }

    public GYearValue(int year, int tz, AtomicType type) {
        this(new MutableGDateValue(year, 1, 1, false, tz, type));
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
        return new GYearValue(m);
    }

    @Override
    public XPathComparable getXPathComparable(StringCollator collator, int implicitTimezone, int specVersion) throws NoDynamicContextException {
        return specVersion >= 40 ? new DateValue(year, (byte)1, (byte)1)
                .adjustTimezone(hasTimezone() ? getTimezoneInMinutes() : implicitTimezone) : null;
    }

    protected EnumSet<AccessorFn.Component> getDefinedComponents() {
        return EnumSet.of(AccessorFn.Component.YEAR);
    }

    /**
     * Determine the primitive type of the value. This delivers the same answer as
     * getItemType().getPrimitiveItemType(). The primitive types are
     * the 19 primitive types of XML Schema, plus xs:integer, xs:dayTimeDuration and xs:yearMonthDuration,
     * and xs:untypedAtomic. For external objects, the result is AnyAtomicType.
     */

    @Override
    public BuiltInAtomicType getPrimitiveType() {
        return BuiltInAtomicType.G_YEAR;
    }

    /*@NotNull*/
    @Override
    public UnicodeString getPrimitiveStringValue() {

        TwineBuilder tb = TwineBuilder.make(16);
        int yr = year;
        if (year <= 0) {
            yr = -yr + (hasNoYearZero ? 1 : 0);           // no year zero in lexical space for XSD 1.0
            if (yr != 0) {
                tb = tb.append('-');
            }
        }
        tb = appendString(tb, yr, (yr > 9999 ? (yr + "").length() : 4));

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
        throw new XPathException("Cannot add a duration to an xs:gYear", "XPTY0004").asTypeError();
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
        return new GYearValue(dt.getYear(), dt.getTimezoneInMinutes(), hasNoYearZero);
    }
}

