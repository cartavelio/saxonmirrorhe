package performance;


import jaxptest.ConfigTest;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.fail;

public class PerformanceTestCase {
    protected final ConfigTest configTest = new ConfigTest();

    public final static double FAILURE_THRESHOLD = 2.0e0;

    public void measure(Measurable test) {
        int expected = test.expectedTimeMicroseconds();
        if (expected == 0) {
            expected = 1;
        }
        int runs = Integer.min(10000000 / expected, 100);
        for (int i=0; i<runs; i++) {
            test.run();
        }
        long start = System.nanoTime();
        for (int i = 0; i < runs; i++) {
            test.run();
        }
        long actualNano = (System.nanoTime() - start) / runs;
        long actualMicro = actualNano / 1000;
        System.err.println(test.name() + ": Actual time " + actualMicro + " microsecs - expected " + expected);
        if (actualMicro > expected * 1000 * FAILURE_THRESHOLD) {
            fail("Performance regression: factor " + ((double)actualMicro / ((double)expected)));
        }
    }

    @Test
    @Tag("performance")
    public void testExample() {
        measure(new Measurable() {
            @Override
            public void run() {
                int j = 0;
                for (int i=0; i<100000; i++) {
                    j++;
                }
                //System.err.println("j = " + j);
            }

            @Override
            public int expectedTimeMicroseconds() {
                return 30;
            }

            @Override
            public String name() {
                return "example";
            }
        });
    }
}

