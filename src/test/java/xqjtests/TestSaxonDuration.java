package xqjtests;

import com.saxonica.xqj.SaxonXQDataSource;
import net.sf.saxon.Configuration;
import net.sf.saxon.type.ValidationException;
import net.sf.saxon.value.DurationValue;
import net.sf.saxon.value.SaxonDuration;
import org.junit.jupiter.api.Test;

import javax.xml.datatype.Duration;
import javax.xml.xquery.*;
import java.util.Date;
import java.util.GregorianCalendar;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Some tests for the SaxonDuration class, used only to support XQJ
 */
public class TestSaxonDuration {

    private javax.xml.datatype.Duration getDayTimeDuration() {
        try {
            Configuration configuration = new Configuration();
            XQDataSource xqDataSource = new SaxonXQDataSource(configuration);
            XQConnection xqConn = xqDataSource.getConnection();
            XQPreparedExpression xqExpr = xqConn.prepareExpression("xs:dayTimeDuration('P1DT2H3M4.5S')");
            XQResultSequence xqResult = xqExpr.executeQuery();
            //noinspection LoopStatementThatDoesntLoop
            while (xqResult.next()) {
                return (SaxonDuration)xqResult.getObject();
            }
        } catch (XQException e) {
            e.printStackTrace();
            fail();
        }
        return null;
    }

    private javax.xml.datatype.Duration getYearMonthDuration() {
        try {
            Configuration configuration = new Configuration();
            XQDataSource xqDataSource = new SaxonXQDataSource(configuration);
            XQConnection xqConn = xqDataSource.getConnection();
            XQPreparedExpression xqExpr = xqConn.prepareExpression("xs:yearMonthDuration('P8Y9M')");
            XQResultSequence xqResult = xqExpr.executeQuery();
            //noinspection LoopStatementThatDoesntLoop
            while (xqResult.next()) {
                return (SaxonDuration) xqResult.getObject();
            }
        } catch (XQException e) {
            e.printStackTrace();
            fail();
        }
        return null;
    }

    @Test
    public void testComponents() {
        Duration dayTimeDuration = getDayTimeDuration();
        assertEquals(1, dayTimeDuration.getSign());
        assertEquals(1, dayTimeDuration.getDays());
        assertEquals(2, dayTimeDuration.getHours());
        assertEquals(3, dayTimeDuration.getMinutes());
        assertEquals(4, dayTimeDuration.getSeconds());
        Duration yearMonthDuration = getYearMonthDuration();
        assertEquals(1, yearMonthDuration.getSign());
        assertEquals(8, yearMonthDuration.getYears());
        assertEquals(9, yearMonthDuration.getMonths());
    }

    @Test
    public void testEquals() {
        assertEquals(getDayTimeDuration(), getDayTimeDuration());
        assertEquals(getYearMonthDuration(), getYearMonthDuration());
    }

    @Test
    public void testAddition() {
        try {
            Duration dayTimeDuration = getDayTimeDuration();
            Duration yearMonthDuration = getYearMonthDuration();
            assertEquals(new SaxonDuration((DurationValue)DurationValue.makeDuration("P2DT4H6M9S").asAtomic()),
                         dayTimeDuration.add(dayTimeDuration));
            assertEquals(new SaxonDuration((DurationValue) DurationValue.makeDuration("P17Y6M").asAtomic()),
                         yearMonthDuration.add(yearMonthDuration));
        } catch (ValidationException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testAdditionToCalendar() {
        Duration yearMonthDuration = getYearMonthDuration();
        GregorianCalendar calendar = new GregorianCalendar(2001, 1, 1);
        yearMonthDuration.addTo(calendar);
        assertEquals(new GregorianCalendar(2009, 10, 1), calendar);
    }

    @Test
    public void testAdditionToDate() {
        Duration yearMonthDuration = getYearMonthDuration();
        Date date = new GregorianCalendar(2001, 1, 1).getTime();
        yearMonthDuration.addTo(date);
        assertEquals(new GregorianCalendar(2009, 10, 1).getTime(), date);
    }


}





