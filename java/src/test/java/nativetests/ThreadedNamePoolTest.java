package nativetests;

import net.sf.saxon.om.NamePool;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StructuredQName;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;

import java.util.Iterator;
import java.util.List;
import java.util.Random;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;

import static org.junit.jupiter.api.Assertions.fail;


/**
 * Scalability test on the NamePool to compare the new (9.7) implementation with the old.
 */
public class ThreadedNamePoolTest {


    private static final String keyXsl =
            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"2.0\">\n" +
            "    <xsl:output method=\"xml\"/>\n" +
            "    <xsl:key name='k' match='LINE' use='string-length(.)'/>\n" +
            "    <xsl:template match=\"/\">\n" +
            "        <out><xsl:value-of select=\"count(key('k', 17))\"/></out>\n" +
            "    </xsl:template>\n" +
            "</xsl:stylesheet>";


    @Test
    @Tag("performance")
    public void testNamePoolThreaded() {
        for (int t=1; t<30; t+=3) {
            doNamePoolThreaded(t);
        }
    }

    private void doNamePoolThreaded(final int threads) {
        try {
            final Random random = new Random();
            //final NamePool pool = new NamePool2();
            final NamePool pool = new NamePool();
            long start = System.nanoTime();
            ExecutorService executor = Executors.newFixedThreadPool(threads);
            for (int i = 0; i < threads; i++) {
                executor.execute(new Runnable() {

                    @Override
                    public void run() {
                        try {
                            int runs = 1000000 / threads;
                            for (int k=0; k<runs; k++) {
                                List<Integer> codes = new java.util.ArrayList<>(10000);
                                String local = "N" + random.nextInt(10000);
                                int code = pool.allocateFingerprint(NamespaceUri.EXSLT_MATH, local);
                                codes.add(code);
                                Iterator<Integer> iterator = codes.iterator();
                                for (int n = 0; n < 20; n++) {
                                    if (iterator.hasNext()) {
                                        StructuredQName name = pool.getStructuredQName(iterator.next());
                                        if (name.getLocalPart().equals("bingo")) {
                                            System.err.println("bingo");
                                        }
                                    }
                                }
                            }
                            //System.out.println("thread-#" + Thread.currentThread().getId() + " OK");
                        } catch (Throwable ex) {
                            System.err.println("thread-# " + Thread.currentThread().getId() + " ERROR");
                            ex.printStackTrace();
                        }
                    }
                });
            }
            executor.shutdown();
            executor.awaitTermination(300, TimeUnit.SECONDS);
            long end = System.nanoTime();
            System.err.println("Threads " + threads + " time " + (end-start)/1000000 + "ms");
        } catch (InterruptedException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }
}

