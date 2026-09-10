package internaltests;

import net.sf.saxon.ma.trie.ImmutableIntSet;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;


/**
 *  Tests of the ImmutableIntSet
 */

public class ImmutableIntSetTest {

    @Test
    public void testAddition() {
        ImmutableIntSet s = new ImmutableIntSet();
        int size = 0;
        for (int i=0; i<10000; i+=100) {
            s = s.add(i);
            size++;
            assertEquals(size, s.size());
        }
        for (int i = 0; i < 10000; i += 50) {
            boolean b = s.contains(i);
            assertEquals(i % 100 == 0, b);
        }

    }

}
