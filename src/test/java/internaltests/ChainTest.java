////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package internaltests;


import junit.framework.TestCase;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.RangeIterator;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.expr.sort.NumericComparer;
import net.sf.saxon.functions.DeepEqual;
import net.sf.saxon.om.Chain;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.IntegerRange;
import net.sf.saxon.value.IntegerValue;

import java.util.ArrayList;
import java.util.List;

public class ChainTest extends TestCase {

    public void testAppend() {
        try {
            List<GroundedValue<?>> initial = new ArrayList<>();
            initial.add(new IntegerRange(1, 20));
            initial.add(new IntegerRange(21, 40));
            initial.add(new IntegerRange(41, 60));
            Chain chain = new Chain(initial);
            for (int i=61; i<=100; i++) {
                chain.append(new Int64Value(i));
            }
            assertEquals(chain.getLength(), 100);
            assertEquals(((IntegerValue)chain.itemAt(50)).longValue(), 51);
        } catch (XPathException e) {
            fail();
        }
    }

    public void testIterate() {
        try {
            List<GroundedValue<?>> initial = new ArrayList<>();
            initial.add(new IntegerRange(1, 20));
            initial.add(new IntegerRange(21, 40));
            initial.add(new IntegerRange(41, 60));
            Chain chain = new Chain(initial);
            for (int i=61; i<=100; i++) {
                chain.append(new Int64Value(i));
            }
            AtomicComparer comparer = NumericComparer.getInstance();
            Configuration config = new Configuration();
            XPathContext context = config.getConversionContext();
            assertTrue(DeepEqual.deepEqual(chain.iterate(), new RangeIterator(1, 100), comparer, context, 0));
            assertEquals(((IntegerValue)chain.itemAt(50)).longValue(), 51);
        } catch (XPathException e) {
            fail();
        }
    }

    public void testRecursive() {
        try {
            List<GroundedValue<?>> initial = new ArrayList<>();
            initial.add(new IntegerRange(1, 20));
            initial.add(new IntegerRange(21, 40));
            initial.add(new IntegerRange(41, 60));
            Chain chain = new Chain(initial);

            List<GroundedValue<?>> l2 = new ArrayList<>();
            for (int i=61; i<=100; i++) {
                l2.add(new Int64Value(i));
            }
            Chain chain2 = new Chain(l2);
            List<GroundedValue<?>> l3 = new ArrayList<>();
            l3.add(chain);
            l3.add(chain2);
            Chain chain3 = new Chain(l3);
            AtomicComparer comparer = NumericComparer.getInstance();
            Configuration config = new Configuration();
            XPathContext context = config.getConversionContext();
            assertTrue(DeepEqual.deepEqual(chain3.iterate(), new RangeIterator(1, 100), comparer, context, 0));
            assertEquals(((IntegerValue)chain3.itemAt(50)).longValue(), 51);
        } catch (XPathException e) {
            fail();
        }
    }

    public void testSubsequence() {
        try {
            List<GroundedValue<?>> initial = new ArrayList<>();
            initial.add(new IntegerRange(1, 20));
            initial.add(new IntegerRange(21, 40));
            initial.add(new IntegerRange(41, 60));
            Chain chain = new Chain(initial);

            List<GroundedValue<?>> l2 = new ArrayList<>();
            for (int i=61; i<=100; i++) {
                l2.add(new Int64Value(i));
            }
            Chain chain2 = new Chain(l2);
            List<GroundedValue<?>> l3 = new ArrayList<>();
            l3.add(chain);
            l3.add(chain2);
            Chain chain3 = new Chain(l3);

            GroundedValue sub = chain3.subsequence(40, 20);
            AtomicComparer comparer = NumericComparer.getInstance();
            Configuration config = new Configuration();
            XPathContext context = config.getConversionContext();
            assertTrue(DeepEqual.deepEqual(sub.iterate(), new RangeIterator(41, 60), comparer, context, 0));
        } catch (XPathException e) {
            fail();
        }
    }
}

