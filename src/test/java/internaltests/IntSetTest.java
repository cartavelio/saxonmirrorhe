package internaltests;

import net.sf.saxon.z.IntHashMap;
import net.sf.saxon.z.IntHashSet;
import net.sf.saxon.z.IntIterator;
import org.junit.jupiter.api.Test;

import java.util.*;

import static org.junit.jupiter.api.Assertions.*;


/**
 *  Tests of the integer set classes
 */
@SuppressWarnings("ConstantConditions")
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
                    assertEquals(size + (added ? 1 : 0), set.size(), "Size after adding");
                    size = set.size();
                } else {
                    boolean removed = set.remove(r);
                    assertEquals(size - (removed ? 1 : 0), set.size(), "Size after removing");
                    size = set.size();
                }
            }
            for (int i=0; i<1000000; i++) {
                boolean b = rand.nextBoolean();
                int r = rand.nextInt(10);
                if (b) {
                    boolean added = set.add(r);
                    assertEquals(size + (added ? 1 : 0), set.size(), "Size after adding");
                    size = set.size();
                } else {
                    boolean removed = set.remove(r);
                    assertEquals(size - (removed ? 1 : 0), set.size(), "Size after removing");
                    size = set.size();
                }
            }
        } catch (Exception err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    @Test
    public void testIntHashMapValueIterator1() {
        // empty iterator.
        // Note (see bug 4907) the only thing we can safely say about an empty IntIterator
        // is that a call on hasNext() returns false. The effect of further calls on hasNext()
        // or next() is undefined.
        IntHashMap<String> map = new IntHashMap<>();
        Iterator<String> iter = map.valueIterator();
        assertFalse(iter.hasNext());
    }

    @Test
    public void testIntHashMapValueIterator2() {
        // Note (see bug 4907) we're testing the only case that is well-defined, namely
        // an alternating sequence of hasNext()/next() calls.
        IntHashMap<String> map = new IntHashMap<>();
        map.put(1, "a");
        map.put(2, "b");
        map.put(42, "alice");
        Set<String> contents = new HashSet<>();
        Iterator<String> iter = map.valueIterator();
        assertTrue(iter.hasNext());
        contents.add(iter.next());
        assertTrue(iter.hasNext());
        contents.add(iter.next());
        assertTrue(iter.hasNext());
        contents.add(iter.next());
        assertEquals(contents.size(), 3);
        assertFalse(iter.hasNext());
    }


    @Test
    public void testIntHashMapKeyIterator1() {
        // empty iterator.
        // Note (see bug 4907) the only thing we can safely say about an empty IntIterator
        // is that a call on hasNext() returns false. The effect of further calls on hasNext()
        // or next() is undefined.
        IntHashMap<String> map = new IntHashMap<>();
        IntIterator iter = map.keyIterator();
        assertFalse(iter.hasNext());
    }

    @Test
    public void testIntHashMapKeyIterator2() {
        // Note (see bug 4907) we're testing the only case that is well-defined, namely
        // an alternating sequence of hasNext()/next() calls.
        IntHashMap<String> map = new IntHashMap<>();
        map.put(1, "a");
        map.put(2, "b");
        map.put(42, "alice");
        IntHashSet contents = new IntHashSet();
        IntIterator iter = map.keyIterator();
        assertTrue(iter.hasNext());
        contents.add(iter.next());
        assertTrue(iter.hasNext());
        contents.add(iter.next());
        assertTrue(iter.hasNext());
        contents.add(iter.next());
        assertEquals(contents.size(), 3);
        assertFalse(iter.hasNext());
    }


    @Test
    public void testIntHashSetValueIterator1() {
        // empty iterator
        IntHashSet map = new IntHashSet();
        IntIterator iter = map.iterator();
        assertFalse(iter.hasNext());
    }

    @Test
    public void testIntHashSetValueIterator2() {
        // Bug 4907
        IntHashSet map = new IntHashSet();
        map.add(1);
        map.add(2);
        map.add(42);
        IntHashSet contents = new IntHashSet();
        IntIterator iter = map.iterator();
        assertTrue(iter.hasNext());
        contents.add(iter.next());
        assertTrue(iter.hasNext());
        contents.add(iter.next());
        assertTrue(iter.hasNext());
        contents.add(iter.next());
        assertEquals(contents.size(), 3);
        assertFalse(iter.hasNext());
    }


}
