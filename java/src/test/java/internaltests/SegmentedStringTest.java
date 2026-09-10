package internaltests;

import net.sf.saxon.str.SegmentedString;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.z.IntIterator;
import net.sf.saxon.z.IntRangeSet;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;

/**
 * Test aspects of Saxon Unicode handling, especially surrogate pairs
 */
public class SegmentedStringTest {

    @Test
    public void testReduced() {
        IntRangeSet input = new IntRangeSet(new int[]{'0', 'A', 'a'}, new int[]{'9', 'Z', 'z'});
        IntIterator iter = input.iterator();
        UnicodeString first = new SegmentedString(iter);
        UnicodeString str = first;
        assertEquals(62, str.length());
        for (int i = 0; i < 100; i++) {
            str = str.concat(first);
        }
        assertEquals(62 * 101, str.length());
        assertEquals('0', str.codePointAt(62 * 10));
        assertEquals('A', str.codePointAt(62 * 10 + 10));
    }

    @Test
    public void testBasic() {
        IntRangeSet input = new IntRangeSet(new int[]{'0', 'A', 'a', 5000}, new int[]{'9', 'Z', 'z', 50000});
        IntIterator iter = input.iterator();
        UnicodeString first = new SegmentedString(iter);
        UnicodeString str = first;
        assertEquals(45063, str.length());
        for (int i=0; i<100; i++) {
            str = str.concat(first);
        }
        assertEquals(45063*101, str.length());
        assertEquals('0', str.codePointAt(45063 * 10));
        assertEquals('A', str.codePointAt(45063 * 10 + 10));
    }




}

