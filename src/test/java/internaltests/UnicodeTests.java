package internaltests;

import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.value.*;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertTrue;

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
        assertTrue(next == 65536);
        next = ((Int64Value)it.next()).longValue();
        assertTrue(next == 65537);
        next = ((Int64Value)it.next()).longValue();
        assertTrue(next == 65538);
        assertTrue(it.next() == null);
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


}

