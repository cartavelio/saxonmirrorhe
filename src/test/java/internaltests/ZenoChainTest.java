package internaltests;


import net.sf.saxon.ma.zeno.ZenoChain;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 *  Tests of the ZenoChain structure
 */

public class ZenoChainTest {

    @Test
    public void testIteration() {
        ZenoChain<Integer> chain = new ZenoChain<>();
        for (int i = 0; i < 2000; i++) {
            chain = chain.add(i);
            assertEquals(i+1, chain.size());
        }
        assertEquals("(1024,512,256,128,64,16)", chain.showMetrics());
        int k=0;
        for (int z : chain) {
            assertEquals(k++, z);
        }
    }

    @Test
    public void testAddAll() {
        ZenoChain<Integer> chain = new ZenoChain<>();
        for (int i=0; i<10; i++) {
            ZenoChain<Integer> subChain = new ZenoChain<>();
            for (int j=0; j<10; j++) {
                subChain = subChain.add(i*10 + j);
            }
            chain = chain.addAll(subChain);
        }
        assertEquals(100, chain.size());
        assertEquals("(64,32,4)", chain.showMetrics());
        int k = 0;
        for (int z : chain) {
            assertEquals(k++, z);
        }
    }

    @Test
    public void testConcat() {
        ZenoChain<Integer> chain = new ZenoChain<>();
        for (int i = 0; i < 10; i++) {
            ZenoChain<Integer> subChain = new ZenoChain<>();
            for (int j = 0; j < 10; j++) {
                subChain = subChain.add(i * 10 + j);
            }
            ZenoChain<Integer> chain2 = chain.concat(subChain);
            assertEquals(chain.size() + 10, chain2.size());
            chain = chain2;
        }
        assertEquals(100, chain.size());
        assertEquals("(90,10)", chain.showMetrics());
        int k = 0;
        for (int z : chain) {
            assertEquals(k++, z);
        }
    }

    @Test
    public void testPrepend() {
        ZenoChain<Integer> chain = new ZenoChain<>();
        for (int i = 0; i < 2000; i++) {
            chain = chain.prepend(i);
            assertEquals(i + 1, chain.size());
        }
        assertEquals("(16,64,128,256,512,1024)", chain.showMetrics());
        int k = 0;
        for (int z : chain) {
            assertEquals(1999 - (k++), z);
        }
    }

    @Test
    public void testTail() {
        ZenoChain<Integer> chain = new ZenoChain<>();
        for (int i = 0; i < 2000; i++) {
            chain = chain.add(i);
            assertEquals(i + 1, chain.size());
        }
        assertEquals("(1024,512,256,128,64,16)", chain.showMetrics());
        chain = chain.subList(1, chain.size());
        assertEquals(1999, chain.size());
        assertEquals("(64,959,512,256,128,64,16)", chain.showMetrics());
        chain = chain.subList(1, chain.size());
        assertEquals(1998, chain.size());
        assertEquals("(63,959,512,256,128,64,16)", chain.showMetrics());
        int k = 2;
        for (int z : chain) {
            assertEquals(k++, z);
        }
    }

    @Test
    public void testSublist() {
        ZenoChain<Integer> chain = new ZenoChain<>();
        for (int i = 0; i < 2000; i++) {
            chain = chain.add(i);
            chain = chain.prepend(i);
        }
        assertEquals(4000, chain.size());
        // This is the current distribution in 11.1. It doesn't have to be this way, so long as it's something reasonable
        assertEquals("(1,1391,512,128,256,1024,688)", chain.showMetrics());
        ZenoChain<Integer> slice = chain.subList(0, 100);
        assertEquals(100, slice.size());
        assertEquals("(1,99)", slice.showMetrics());
        // Check the original is unharmed
        assertEquals(4000, chain.size());
        assertEquals("(1,1391,512,128,256,1024,688)", chain.showMetrics());
        // and another slice
        slice = chain.subList(38, 538);
        assertEquals(500, slice.size());
        assertEquals("(500)", slice.showMetrics());
        assertEquals(4000, chain.size());
        // and another
        slice = chain.subList(2000, 3000);
        assertEquals(1000, slice.size());
        assertEquals("(32,256,712)", slice.showMetrics());
        assertEquals(4000, chain.size());
        // and another
        slice = chain.subList(3000, 4000);
        assertEquals(1000, slice.size());
        assertEquals("(312,688)", slice.showMetrics());
        assertEquals(4000, chain.size());
    }

    @Test
    public void testSublistNegativeStart() {
        ZenoChain<Integer> chain = new ZenoChain<>();
        for (int i = 0; i < 2000; i++) {
            chain = chain.add(i);
        }
        try {
            ZenoChain<Integer> slice = chain.subList(-20, 100);
            fail("Exception not caught");
        } catch (IndexOutOfBoundsException e) {
            // OK
        }
    }

    @Test
    public void testSublistStartTooBig() {
        ZenoChain<Integer> chain = new ZenoChain<>();
        for (int i = 0; i < 200; i++) {
            chain = chain.add(i);
        }
        try {
            ZenoChain<Integer> slice = chain.subList(3000, 4000);
            fail("Exception not caught");
        } catch (IndexOutOfBoundsException e) {
            // OK
        }
    }

    @Test
    public void testSublistNegativeEnd() {
        ZenoChain<Integer> chain = new ZenoChain<>();
        for (int i = 0; i < 200; i++) {
            chain = chain.add(i);
        }
        try {
            ZenoChain<Integer> slice = chain.subList(0, -20);
            fail("Exception not caught");
        } catch (IndexOutOfBoundsException e) {
            // OK
        }
    }

    @Test
    public void testSublistEndBeyondMax() {
        ZenoChain<Integer> chain = new ZenoChain<>();
        for (int i = 0; i < 200; i++) {
            chain = chain.add(i);
        }
        try {
            ZenoChain<Integer> slice = chain.subList(0, 500);
            fail("Exception not caught");
        } catch (IndexOutOfBoundsException e) {
            // OK
        }
    }


}
