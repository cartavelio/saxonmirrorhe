package performance;

import junit.framework.TestCase;

public class PerformanceTestCase extends TestCase {

    public final static double FAILURE_THRESHOLD = 2.0e0;

    public void measure(Measurable test) {
        int expected = test.expectedTimeMilliseconds();
        if (expected == 0) {
            expected = 1;
        }
        int runs = Integer.min(10000 / expected, 100);
        for (int i=0; i<runs; i++) {
            test.run();
        }
        long start = System.nanoTime();
        for (int i = 0; i < runs; i++) {
            test.run();
        }
        long actualNano = (System.nanoTime() - start) / runs;
        long actualMicro = actualNano / 1000;
        System.err.println(test.name() + ": Actual time " + actualMicro + "microsecs - expected " + (expected*1000));
        if (actualMicro > expected * 1000 * FAILURE_THRESHOLD) {
            fail("Performance regression: factor " + ((double)actualMicro / ((double)expected*1000)));
        }
    }

    public void testDummy() {
        assertTrue(true);
    }
}

