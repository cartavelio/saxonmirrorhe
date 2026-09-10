package performance;

import net.sf.saxon.functions.Count;
import net.sf.saxon.ma.map.AbstractFixedMap;
import net.sf.saxon.ma.map.EmptyMap;
import net.sf.saxon.ma.map.GeneralMapBuilder;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.DoubleValue;
import net.sf.saxon.value.StringValue;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;

import java.util.Random;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.fail;

public class MapPerformance extends PerformanceTestCase {

    public void measureBuild(final int n) {

            measure(new Measurable() {

                @Override
                public void run() {
                    Random random = new Random();
                    GeneralMapBuilder builder = AbstractFixedMap.getBuilder(40);
                    for (int i = 0; i < n; i++) {
                        builder.put(new StringValue("abc" + i), new DoubleValue(random.nextDouble()));
                    }
                    try {
                        MapItem map = builder.getCompletedMap();
                        assertNotNull(map.get(new StringValue("abc123")));
                        assertEquals(n, map.size());
                    } catch (XPathException e) {
                        fail(e.getMessage());
                    }
                }

                @Override
                public int expectedTimeMicroseconds() {
                    return n/5;
                }

                @Override
                public String name() {
                    return "testMapBuild" + n;
                }
            });
         
    }

    public void measureIncrementalBuild(final int n) {

            measure(new Measurable() {

                @Override
                public void run() {
                    Random random = new Random();
                    MapItem map = EmptyMap.INSTANCE_40;
                    for (int i = 0; i < n; i++) {
                        map = map.put(new StringValue("abc" + i), new DoubleValue(random.nextDouble()));
                    }
                    assertNotNull(map.get(new StringValue("abc123")));
                    assertEquals(n, map.size());
                }

                @Override
                public int expectedTimeMicroseconds() {
                    return n/2;
                }

                @Override
                public String name() {
                    return "testIncrementalBuild" + n;
                }
            });

    }

    public void measureRetrieval(final int n, final int m) {

        measure(new Measurable() {

            @Override
            public void run() {
                Random random = new Random();
                GeneralMapBuilder builder = AbstractFixedMap.getBuilder(40);
                for (int i = 0; i < n; i++) {
                    builder.put(new StringValue("abc" + i), new DoubleValue(random.nextDouble()));
                }
                try {
                    MapItem map = builder.getCompletedMap();
                    assertNotNull(map.get(new StringValue("abc123")));
                    assertEquals(n, map.size());
                    for (int j=0; j<m; j++) {
                        assertNotNull(map.get(new StringValue("abc" + j%n)));
                    }
                } catch (XPathException e) {
                    fail(e.getMessage());
                }
            }

            @Override
            public int expectedTimeMicroseconds() {
                return n/2 + m/30;
            }

            @Override
            public String name() {
                return "testMapRetrieval-" + m + "-of-" + n;
            }
        });

    }


    @Test
    @Tag("performance")
    public void testBuild1000() {
        measureBuild(1000);
    }

    @Test
    @Tag("performance")
    public void testBuild10K() {
        measureBuild(10000);
    }

    @Test
    @Tag("performance")
    public void testBuild100K() {
        measureBuild(100000);
    }

    @Test
    @Tag("performance")
    public void testPut1000() {
        measureIncrementalBuild(1000);
    }

    @Test
    @Tag("performance")
    public void testPut10K() {
        measureIncrementalBuild(10000);
    }

    @Test
    @Tag("performance")
    public void testPut100K() {
        measureIncrementalBuild(100000);
    }


    public void measureScan(final int n) {

        measure(new Measurable() {

            @Override
            public void run() {
                Random random = new Random();
                try {
                    GeneralMapBuilder builder = AbstractFixedMap.getBuilder(40);
                    for (int i = 0; i < n; i++) {
                        builder.put(new StringValue("abc" + i), new DoubleValue(random.nextDouble()));
                    }
                    MapItem map = builder.getCompletedMap();
                    assertNotNull(map.get(new StringValue("abc123")));

                    assertEquals(n, Count.count(map.keys()));
                } catch (XPathException e) {
                    fail();
                }
            }

            @Override
            public int expectedTimeMicroseconds() {
                return n;
            }

            @Override
            public String name() {
                return "testMapScan-" + n/5;
            }
        });

    }



    @Test
    @Tag("performance")
    public void testScan100K() {
        measureScan(100_000);
    }

    @Test
    @Tag("performance")
    public void testRetrieval() {
        measureRetrieval(1000, 1_000_000);
    }


}

