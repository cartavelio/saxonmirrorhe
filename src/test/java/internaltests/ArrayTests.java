package internaltests;

import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.str.StringView;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ValidationException;
import net.sf.saxon.value.DateTimeValue;
import net.sf.saxon.value.DateValue;
import net.sf.saxon.value.DayTimeDurationValue;
import net.sf.saxon.value.DurationValue;
import org.junit.jupiter.api.Test;

import java.time.LocalDateTime;
import java.time.ZonedDateTime;
import java.time.temporal.ChronoField;

import static java.time.temporal.ChronoField.*;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

public class DateTimeTests {

    /**
     * Test that DateTimeValue implements TemporalAccessor correctly
     */
    private void tryTemporalAccessor(String in) {
        DateTimeValue d1 = DateTimeValue.parse(StringView.tidy(in));
        ZonedDateTime d2 = ZonedDateTime.parse(in);
        for (ChronoField cf : new ChronoField[]{
                NANO_OF_SECOND, NANO_OF_DAY, MICRO_OF_SECOND, MICRO_OF_DAY, MILLI_OF_SECOND,
                MILLI_OF_DAY, SECOND_OF_MINUTE, SECOND_OF_DAY, MINUTE_OF_HOUR, MINUTE_OF_DAY,
                HOUR_OF_AMPM, CLOCK_HOUR_OF_AMPM, HOUR_OF_DAY, CLOCK_HOUR_OF_DAY, AMPM_OF_DAY,
                DAY_OF_WEEK, ALIGNED_DAY_OF_WEEK_IN_MONTH, ALIGNED_DAY_OF_WEEK_IN_YEAR,
                DAY_OF_MONTH, DAY_OF_YEAR, EPOCH_DAY, ALIGNED_WEEK_OF_MONTH,
                ALIGNED_WEEK_OF_YEAR, MONTH_OF_YEAR, PROLEPTIC_MONTH, YEAR_OF_ERA,
                YEAR, ERA, INSTANT_SECONDS, OFFSET_SECONDS}) {

            long expected = d2.getLong(cf);
            long actual = d1.getLong(cf);
            assertEquals(actual, expected, cf + " of " + in);
        }
    }

    @Test
    public void test01() {
        tryTemporalAccessor("1966-01-01T09:20:36.123Z");
    }

    @Test
    public void test02() {
        tryTemporalAccessor("1970-01-01T00:00:00Z");
    }

    @Test
    public void test03() {
        tryTemporalAccessor("1972-02-29T12:00:00Z");
    }

    @Test
    public void test04() {
        tryTemporalAccessor("2012-02-29T12:00:00-05:00");
    }

    @Test
    public void test05() {
        tryTemporalAccessor("2012-02-29T14:00:00-05:00");
    }

    @Test
    public void test06() {
        tryTemporalAccessor("2012-02-29T14:00:00+10:15");
    }

    @Test
    public void test07() {
        tryTemporalAccessor("1066-10-14T13:00:00Z");
    }

    @Test
    public void test08() {
        tryTemporalAccessor("-0055-03-14T13:00:00Z");
    }

    @Test
    public void testJulianDayNumber() {
        // test Julian day number across the year zero divide
        try {
            DateValue start = (DateValue)DateValue.makeDateValue(StringView.wrap("0002-01-01"), ConversionRules.DEFAULT).asAtomic();
            int j = start.getJulianDayNumber();
            for (int i=j; i>j-1000; i--) {
                DateValue d = DateValue.dateFromJulianDayNumber(i);
                int k = d.getJulianDayNumber();
                assertEquals(i, k, d.getStringValue());
            }
        } catch (ValidationException e) {
            fail(e.getMessage());
        }
    }

    public void testZonedDateTime() {
        try {
            DateTimeValue val = DateTimeValue.parse(StringView.wrap("2012-02-29T14:00:00+10:15"));
            ZonedDateTime zdt = val.toZonedDateTime();
            DateTimeValue val2 = DateTimeValue.fromZonedDateTime(zdt);
            assertEquals(val, val2, "round-trip");
            DateTimeValue val3 = DateTimeValue.parse(StringView.wrap("2012-02-29T14:00:00"));
            ZonedDateTime zdt2 = val3.toZonedDateTime();
            DateTimeValue val4 = DateTimeValue.fromZonedDateTime(zdt2);
            assertEquals(val4, DateTimeValue.parse(StringView.wrap("2012-02-29T14:00:00Z")), "round-trip2");
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testLocalDateTime() {
        try {
            DateTimeValue val = DateTimeValue.parse(StringView.wrap("2012-02-29T14:00:00"));
            LocalDateTime zdt = val.toLocalDateTime();
            DateTimeValue val2 = DateTimeValue.fromLocalDateTime(zdt);
            assertEquals(val, val2, "round-trip");
            DateTimeValue val3 = DateTimeValue.parse(StringView.wrap("2012-02-29T14:00:00-05:00"));
            LocalDateTime zdt2 = val.toLocalDateTime();
            DateTimeValue val4 = DateTimeValue.fromLocalDateTime(zdt2);
            assertEquals(val4, DateTimeValue.parse(StringView.wrap("2012-02-29T14:00:00")), "round-trip2");
        } catch (Exception e) {
            fail();
            e.printStackTrace();
        }
    }

    public void testNanosecondPrecision() {
        try {
            DateTimeValue val = DateTimeValue.parse(StringView.wrap("2012-02-29T14:00:00.123456789"));
            DateTimeValue val2 = val.add((DurationValue)DayTimeDurationValue.makeDayTimeDurationValue(StringView.wrap("PT0.000000001S")).asAtomic());
            assertEquals(DateTimeValue.parse(StringView.wrap("2012-02-29T14:00:00.12345679")), val2, "val2");
            DateTimeValue val3 = val2.add(DayTimeDurationValue.fromNanoseconds(-1));
            assertEquals("val3", "2012-02-29T14:00:00.123456789", val3.getStringValue());
            assertEquals(DayTimeDurationValue.fromNanoseconds(1_000_000_000),
                         DayTimeDurationValue.makeDayTimeDurationValue(StringView.wrap("PT1S")),
                         "dtd");
        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testWeekNumber() {

        assertEquals(1, DateValue.getWeekNumber(2018, 12, 31));

        assertEquals(52, DateValue.getWeekNumber(1997, 12, 28));
        assertEquals(1, DateValue.getWeekNumber(1997, 12, 29));
        assertEquals(1, DateValue.getWeekNumber(1997, 12, 30));
        assertEquals(1, DateValue.getWeekNumber(1997, 12, 31));
        assertEquals(1, DateValue.getWeekNumber(1998, 1, 1));
        assertEquals(1, DateValue.getWeekNumber(1998, 1, 2));
        assertEquals(1, DateValue.getWeekNumber(1998, 1, 3));
        assertEquals(1, DateValue.getWeekNumber(1998, 1, 4));
        assertEquals(2, DateValue.getWeekNumber(1998, 1, 5));

        assertEquals(52, DateValue.getWeekNumber(1998, 12, 27));
        assertEquals(53, DateValue.getWeekNumber(1998, 12, 28));
        assertEquals(53, DateValue.getWeekNumber(1998, 12, 29));
        assertEquals(53, DateValue.getWeekNumber(1998, 12, 30));
        assertEquals(53, DateValue.getWeekNumber(1998, 12, 31));
        assertEquals(53, DateValue.getWeekNumber(1999, 1, 1));
        assertEquals(53, DateValue.getWeekNumber(1999, 1, 2));
        assertEquals(53, DateValue.getWeekNumber(1999, 1, 3));
        assertEquals(1, DateValue.getWeekNumber(1999, 1, 4));
        assertEquals(1, DateValue.getWeekNumber(1999, 1, 5));

        assertEquals(51, DateValue.getWeekNumber(1999, 12, 26));
        assertEquals(52, DateValue.getWeekNumber(1999, 12, 27));
        assertEquals(52, DateValue.getWeekNumber(1999, 12, 28));
        assertEquals(52, DateValue.getWeekNumber(1999, 12, 29));
        assertEquals(52, DateValue.getWeekNumber(1999, 12, 30));
        assertEquals(52, DateValue.getWeekNumber(1999, 12, 31));
        assertEquals(52, DateValue.getWeekNumber(2000, 1, 1));
        assertEquals(52, DateValue.getWeekNumber(2000, 1, 2));
        assertEquals(1, DateValue.getWeekNumber(2000, 1, 3));
        assertEquals(1, DateValue.getWeekNumber(2000, 1, 4));
        assertEquals(1, DateValue.getWeekNumber(2000, 1, 5));

    }

}

