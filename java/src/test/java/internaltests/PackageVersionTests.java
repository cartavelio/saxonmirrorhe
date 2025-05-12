package internaltests;

import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.StringValue;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertNull;

/**
 * Test aspects of Saxon Unicode handling, especially surrogate pairs
 */
public class UnicodeTests {

    @Test
    public void testCharacterIterator() {
        int[] ch = new int[]{65536, 65537, 65538};
        String s = new String(ch, 0, ch.length);
        StringValue sv = new StringValue(s);
        SequenceIterator it = sv.iterateCharacters();
        long next = ((Int64Value)it.next()).longValue();
        assertEquals(65536, next);
        next = ((Int64Value)it.next()).longValue();
        assertEquals(65537, next);
        next = ((Int64Value)it.next()).longValue();
        assertEquals(65538, next);
        assertNull(it.next());
    }


    @Test
    public void testEncoding() {
        /*
        146.00  =>   4046401A36E2EB1D
36.30   =>   4042266666666666
76.22   =>   40530E147AE147AE
3865.20 =>   40DA06683E8C7FD4
0.200   =>   3FC999999999999A
         */

        System.err.println(Double.longBitsToDouble(Long.parseLong("4046401A36E2EB1D", 16)));
        System.err.println(Double.longBitsToDouble(Long.parseLong("4042266666666666", 16)));
        System.err.println(Double.longBitsToDouble(Long.parseLong("40530E147AE147AE", 16)));
        System.err.println(Double.longBitsToDouble(Long.parseLong("40DA06683E8C7FD4", 16)));
        System.err.println(Double.longBitsToDouble(Long.parseLong("3FC999999999999A", 16)));
    }

    @Test
    public void testUnicodeBuilder() {
        UnicodeBuilder ub = new UnicodeBuilder();
        for (int i=0; i<50000; i++) {
            ub = ub.append("XYZ");
        }
        ub = ub.append(100000);
        for (int i = 0; i < 50000; i++) {
            ub = ub.append("ABC");
        }
        assertEquals(300001, ub.length());
        assertFalse(ub.isEmpty());
        UnicodeString str = ub.toUnicodeString();
        assertEquals(300001, str.length());
        assertFalse(str.isEmpty());
        assertEquals(str.getWidth(), 24);
        assertEquals(150000, str.indexOf(100000));
        assertEquals(150001, str.indexOf('A'));
    }


}

