package nativetests;

import junit.framework.TestCase;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.NamePool;
import net.sf.saxon.om.StructuredQName;

import java.util.Iterator;
import java.util.List;
import java.util.Random;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;


/**
 * Scalability test on the NamePool to compare the new (9.7) implementation with the old.
 */
public class ThreadedNamePoolTest extends TestCase {


    private static final String keyXsl =
            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"2.0\">\n" +
            "    <xsl:output method=\"xml\"/>\n" +
            "    <xsl:key name='k' match='LINE' use='string-length(.)'/>\n" +
            "    <xsl:template match=\"/\">\n" +
            "        <out><xsl:value-of select=\"count(key('k', 17))\"/></out>\n" +
            "    </xsl:template>\n" +
            "</xsl:stylesheet>";


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

                    public void run() {
                        try {
                            int runs = 1000000 / threads;
                            for (int k=0; k<runs; k++) {
                                List<Integer> codes = new java.util.ArrayList<Integer>(10000);
                                String local = "N" + random.nextInt(10000);
                                int code = pool.allocateFingerprint(NamespaceConstant.EXSLT_MATH, local);
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

//
// The contents of this file are subject to the Mozilla Public License Version
// 1.0 (the "License");
// you may not use this file except in compliance with the License. You may
// obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations
// under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Michael Kay,
//
// Portions created by (your name) are Copyright (C) (your legal entity). All
// Rights Reserved.
//
// Contributor(s): none.
//
