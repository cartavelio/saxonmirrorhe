package s9apitest;

import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XdmAtomicValue;
import org.junit.jupiter.api.Test;

import java.math.BigDecimal;
import java.time.Instant;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.time.ZonedDateTime;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * test XdmAtomicValue class especially conversions
 */
public class TestAtomicValues {

    @Test
    public void testInteger() {
        try {
            assertEquals(23, new XdmAtomicValue(23).getLongValue());
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testDouble() {
        try {
            assertEquals(23e3, new XdmAtomicValue(23e3).getDoubleValue());
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testDecimal() {
        try {
            assertEquals(BigDecimal.valueOf(500), new XdmAtomicValue(500).getDecimalValue());
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testInstant() {
        Instant now = Instant.now();
        assertEquals(now, new XdmAtomicValue(now).getInstant());
    }

    @Test
    public void testZonedDateTime() {
        // Note that information about the civil date time is lost on round-tripping
        ZonedDateTime now = ZonedDateTime.now();
        assertEquals(now.toOffsetDateTime(), new XdmAtomicValue(now).getZonedDateTime().toOffsetDateTime());
    }

    @Test
    public void testLocalDateTime() {
        LocalDateTime now = LocalDateTime.now();
        assertEquals(now, new XdmAtomicValue(now).getLocalDateTime());
    }

    @Test
    public void testLocalDate() {
        LocalDate now = LocalDate.now();
        assertEquals(now, new XdmAtomicValue(now).getLocalDate());
    }

}

