////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package performance;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Performance test of recursive functions that append or prepend to a sequence. This should have linear performance
 */

public class TestSharedAppend {

    @Test
    @Tag("performance")
    public void testAppend() {
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            String query =
                    "declare variable $n as xs:integer external;" +
                    "declare function local:f($seq as xs:integer*, $next as xs:integer, $max as xs:integer) " +
                            "{if ($next gt $max) then $seq else local:f(($seq, $next), $next+1, $max)};" +
                    "count(local:f((), 1, $n))";
            XQueryExecutable exec = c.compile(query);
            XQueryEvaluator qe = exec.load();
            QName param = new QName("n");
            int[] counts = {1000, 10000, 100000, 200000, 300000, 1000000};
            long last = 0;
            for (int n : counts) {
                System.err.println("count = " + n);
                int totaltime = 0;
                int runs = n<10000 ? 50 : 1;
                for (int i=0; i<runs; i++) {
                    qe.setExternalVariable(param, new XdmAtomicValue(n));
                    long start = System.nanoTime();
                    XdmItem result = qe.evaluateSingle();
                    assertEquals((long)n, ((XdmAtomicValue)result).getLongValue(), "result");
                    long end = System.nanoTime();
                    totaltime += (end - start) / 1000000e0;
                }
                last = totaltime / runs;
                System.err.println("Append: " + n + " time: " + (totaltime / runs) + " ms");
            }
            assertTrue(last < 1200, "performance");
            // Saxon 9.6 with new Chain implementation takes 968ms for n=1000000
            // Saxon 9.7 - 520ms
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testPrepend() {
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            String query =
                    "declare variable $n as xs:integer external;" +
                    "declare function local:f($seq as xs:integer*, $next as xs:integer, $max as xs:integer) " +
                            "{if ($next gt $max) then $seq else local:f(($next, $seq), $next+1, $max)};" +
                    "count(local:f((), 1, $n))";
            XQueryExecutable exec = c.compile(query);
            XQueryEvaluator qe = exec.load();
            QName param = new QName("n");
            int[] counts = {1000, 10000, 100000, 200000, 300000, 1000000};
            long last = 0;
            for (int n : counts) {
                int totaltime = 0;
                int runs = (n<10000 ? 50 : 1);
                for (int i=0; i<runs; i++) {
                    qe.setExternalVariable(param, new XdmAtomicValue(n));
                    long start = System.nanoTime();
                    XdmItem result = qe.evaluateSingle();
                    assertEquals((long)n, ((XdmAtomicValue)result).getLongValue(), "result");
                    long end = System.nanoTime();
                    totaltime += (end - start) / 1000000e0;
                }
                last = totaltime / runs;
                System.err.println("Prepend: " + n + " time: " + (totaltime / runs) + " ms");
            }
            assertTrue(last < 1500, "performance");
            // Saxon 9.6 with new Chain implementation takes 968ms for n=1000000
            // Saxon 9.7: 699ms
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testChainSubscript() {
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            String query =
                    "declare variable $n as xs:integer external;" +
                    "declare function local:f($seq as xs:integer*, $next as xs:integer, $max as xs:integer) " +
                            "{if ($next gt $max) then $seq else local:f(($seq, $next), $next+1, $max)};" +
                    "declare variable $s := (local:f((), 1, 100000));" +
                    "(for $i in 1 to 20 return $s[$n+$i])[6]";
            XQueryExecutable exec = c.compile(query);
            XQueryEvaluator qe = exec.load();
            QName param = new QName("n");
            int[] counts = {1000, 10000, 20000, 30000, 80000};
            long last = 0;
            for (int n : counts) {
                int totaltime = 0;
                int runs = 100;
                for (int i=0; i<runs; i++) {
                    qe.setExternalVariable(param, new XdmAtomicValue(n));
                    long start = System.nanoTime();
                    XdmItem result = qe.evaluateSingle();
                    assertEquals((long)n+6, ((XdmAtomicValue)result).getLongValue(), "result");
                    long end = System.nanoTime();
                    totaltime += (end - start) / 1000000e0;
                }
                last = totaltime / runs;
                System.err.println("Subscript: " + n + " time: " + (totaltime / runs) + " ms");
            }
            assertTrue(last < 100, "performance");
            // Saxon 9.6 with new Chain implementation takes 76ms regardless of the subscript
            // Saxon 9.7: 52-56ms
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


    @Test
    public void testMapMerge() {  // See bug 4865. Takes about 1.5s in 10.5
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            String query = "declare namespace map =\n"
                    + "    \"http://www.w3.org/2005/xpath-functions/map\";\n"
                    + "let $seq := (1, 1 to 500000, 1, 2)\n"
                    + "let $counts := fold-left($seq, map { }, function($sofar, $this) {\n"
                    + "  map:put($sofar, $this, head(($sofar($this) + 1, 1)))\n"
                    + "}) return\n"
                    + "  map:for-each($counts, function($key, $count) {\n"
                    + "    $key[$count > 1]\n"
                    + "  })";
            XQueryExecutable exec = c.compile(query);
            XQueryEvaluator qe = exec.load();
            XdmValue result = qe.evaluate();
            assertEquals(2, result.size());
        } catch (SaxonApiException e) {
            fail();
        }
    }

}

