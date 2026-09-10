package net.sf.saxon.value;

import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Disabled;
import org.junit.jupiter.api.Test;

import java.math.BigDecimal;
import java.math.BigInteger;

import static org.junit.jupiter.api.Assertions.*;

class CalendarValueTest {

    @Test
    void isValidTime() throws XPathException {
        assertTrue(CalendarValue.isValidTime(12, 34, 56, 7809));
    }

    @Test
    void isValidTime_midnight00() throws XPathException {
        assertTrue(CalendarValue.isValidTime(0, 0, 0, 0));
    }

    @Test
    void isValidTime_midnight24() throws XPathException {
        assertTrue(CalendarValue.isValidTime(24, 0, 0, 0));
    }

    @Test
    void isValidTime_almostMidnight() throws XPathException {
        assertTrue(CalendarValue.isValidTime(23, 59, 59, 9999));
    }

    @Test
    void isValidTime_justPastMidnight00() throws XPathException {
        assertTrue(CalendarValue.isValidTime(0, 0, 0,1));
    }

    @Test
    void isValidTime_minsPastMidnight24() throws XPathException {
        assertThrows(XPathException.class, () -> {
            CalendarValue.isValidTime(24, 10, 0, 0);
        });
    }

    @Test
    void isValidTime_secsPastMidnight24() throws XPathException {
        assertThrows(XPathException.class, () -> {
            CalendarValue.isValidTime(24, 0, 20, 0);
        });
    }

    @Test
    void isValidTime_fractionalSecsPastMidnight24() throws XPathException {
        assertThrows(XPathException.class, () -> {
            CalendarValue.isValidTime(24, 0, 0, 1);
        });
    }

    @Test
    void isValidTime_negativeHours() {
        IntegerValue hours = IntegerValue.makeIntegerValue(new BigInteger("-1"));
        IntegerValue minutes = IntegerValue.makeIntegerValue(new BigInteger("0"));
        DecimalValue seconds = (DecimalValue) DecimalValue.parseNumber("0");

        assertThrows(XPathException.class, () -> {
            CalendarValue.isValidTime(-1, 0, 0, 0);
        });
    }

    @Test
    void isValidTime_tooManyHours() {
        assertThrows(XPathException.class, () -> {
            CalendarValue.isValidTime(25, 0, 0, 0);
        });
    }

    @Test
    void isValidTime_negativeMinutes() {
        assertThrows(XPathException.class, () -> {
            CalendarValue.isValidTime(0, -1, 0, 0);
        });
    }

    @Test
    void isValidTime_tooManyMinutes() {
       assertThrows(XPathException.class, () -> {
            CalendarValue.isValidTime(0, 60, 0, 0);
        });
    }

    @Test
    void isValidTime_negativeSeconds() {
        assertThrows(XPathException.class, () -> {
            CalendarValue.isValidTime(0, 0, -1, 0);
        });
    }

    @Test
    void isValidTime_tooManySeconds() {
        assertThrows(XPathException.class, () -> {
            CalendarValue.isValidTime(0, 0, 60, 0);
        });
    }

    @Disabled("Only relevant when input is a string.")
    @Test
    void isValidTime_fractionalSecondsEndsInZero() {
      assertThrows(XPathException.class, () -> {
            CalendarValue.isValidTime(0, 0, 0, 0);
        });
    }
}