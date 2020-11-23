package internaltests;

import junit.framework.TestCase;
import net.sf.saxon.z.IntHashSet;

import java.util.Random;


/**
 *  Tests of the NodeInfo interface as implemented by the tinytree
 */
public class IntSetTest extends TestCase {

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
                    assertTrue("Size after adding", set.size() == size + (added ? 1 : 0));
                    size = set.size();
                } else {
                    boolean removed = set.remove(r);
                    assertTrue("Size after removing", set.size() == size - (removed ? 1 : 0));
                    size = set.size();
                }
            }
            for (int i=0; i<1000000; i++) {
                boolean b = rand.nextBoolean();
                int r = rand.nextInt(10);
                if (b) {
                    boolean added = set.add(r);
                    assertTrue("Size after adding", set.size() == size + (added ? 1 : 0));
                    size = set.size();
                } else {
                    boolean removed = set.remove(r);
                    assertTrue("Size after removing", set.size() == size - (removed ? 1 : 0));
                    size = set.size();
                }
            }
        } catch (Exception err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }
}