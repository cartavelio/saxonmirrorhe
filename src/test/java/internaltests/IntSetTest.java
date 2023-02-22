package internaltests;

import net.sf.saxon.z.IntHashSet;
import org.junit.jupiter.api.Test;

import java.util.Random;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


/**
 *  Tests of the integer set classes
 */
public class IntSetTest {

    @Test
    public void testInsertRemove() {
        try {
            IntHashSet set = new IntHashSet(10);
            Random rand = new Random();
            int size = 0;
            for (int i=0; i<1000000; i++) {
                boolean b = rand.nextBoolean();
                int r = rand.nextInt(10000);
                if (b) {
                    boolean added = set.add(r);
                    assertTrue(set.size() == size + (added ? 1 : 0), "Size after adding");
                    size = set.size();
                } else {
                    boolean removed = set.remove(r);
                    assertTrue(set.size() == size - (removed ? 1 : 0), "Size after removing");
                    size = set.size();
                }
            }
            for (int i=0; i<1000000; i++) {
                boolean b = rand.nextBoolean();
                int r = rand.nextInt(10);
                if (b) {
                    boolean added = set.add(r);
                    assertTrue(set.size() == size + (added ? 1 : 0), "Size after adding");
                    size = set.size();
                } else {
                    boolean removed = set.remove(r);
                    assertTrue(set.size() == size - (removed ? 1 : 0), "Size after removing");
                    size = set.size();
                }
            }
        } catch (Exception err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }
}
