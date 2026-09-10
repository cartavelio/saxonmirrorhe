////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package internaltests;


import net.sf.saxon.Configuration;
import net.sf.saxon.expr.AscendingRangeIterator;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.expr.sort.NumericComparer;
import net.sf.saxon.functions.DeepEqual;
import net.sf.saxon.ma.zeno.ZenoSequence;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.IntegerRange;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.NumericValue;
import org.junit.jupiter.api.Test;

import java.util.ArrayList;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

public class ZenoSequenceTest {

    @Test
    public void testAppend() {
        try {
            List<GroundedValue> initial = new ArrayList<>();
            initial.add(new IntegerRange(1, 1, 20));
            initial.add(new IntegerRange(21, 1, 40));
            initial.add(new IntegerRange(41, 1, 60));
            ZenoSequence chain = ZenoSequence.join(initial);
            for (int i=61; i<=100; i++) {
                chain = chain.append(new Int64Value(i));
            }
            assertEquals(100, chain.getLength());
            assertEquals(1, ((IntegerValue) chain.itemAt(0)).longValue());
            assertEquals(51, ((IntegerValue)chain.itemAt(50)).longValue());
            assertEquals(82, ((IntegerValue) chain.itemAt(81)).longValue());
            assertEquals(100, ((IntegerValue) chain.itemAt(99)).longValue());
        } catch (XPathException e) {
            fail();
        }
    }

    @Test
    public void testIterate() {
        try {
            List<GroundedValue> initial = new ArrayList<>();
            initial.add(new IntegerRange(1, 1, 20));
            initial.add(new IntegerRange(21, 1, 40));
            initial.add(new IntegerRange(41, 1, 60));
            ZenoSequence chain = ZenoSequence.join(initial);
            for (int i=61; i<=100; i++) {
                chain = chain.append(new Int64Value(i));
            }
            AtomicComparer comparer = NumericComparer.getInstance();
            Configuration config = new Configuration();
            XPathContext context = config.getConversionContext();
            assertTrue(DeepEqual.deepEqual(chain.iterate(), new AscendingRangeIterator(1, 1, 100), comparer, context, 0));
            assertEquals(51, ((IntegerValue)chain.itemAt(50)).longValue());
        } catch (XPathException e) {
            fail();
        }
    }

    @Test
    public void testHead() {
        try {
            List<GroundedValue> initial = new ArrayList<>();
            initial.add(new IntegerRange(1, 1, 20));
            initial.add(new IntegerRange(21, 1, 40));
            initial.add(new IntegerRange(41, 1, 60));
            ZenoSequence chain = ZenoSequence.join(initial);
            Item head = chain.head();
            assertEquals(1L, ((NumericValue) head).longValue());
        } catch (XPathException e) {
            fail();
        }
    }

    @Test
    public void testItemAt() {
        try {
            List<GroundedValue> initial = new ArrayList<>();
            initial.add(new IntegerRange(1, 1, 20));
            initial.add(new IntegerRange(21, 1, 40));
            initial.add(new IntegerRange(41, 1, 60));
            ZenoSequence chain = ZenoSequence.join(initial);
            Item i25 = chain.itemAt(25);
            assertEquals(26L, ((NumericValue) i25).longValue());
        } catch (XPathException e) {
            fail();
        }
    }

    @Test
    public void testRecursive() {
        try {
            List<GroundedValue> initial = new ArrayList<>();
            initial.add(new IntegerRange(1, 1, 20));
            initial.add(new IntegerRange(21, 1, 40));
            initial.add(new IntegerRange(41, 1, 60));
            ZenoSequence chain = ZenoSequence.join(initial);

            List<GroundedValue> l2 = new ArrayList<>();
            for (int i=61; i<=100; i++) {
                l2.add(new Int64Value(i));
            }
            ZenoSequence chain2 = ZenoSequence.join(l2);
            List<GroundedValue> l3 = new ArrayList<>();
            l3.add(chain);
            l3.add(chain2);
            ZenoSequence chain3 = ZenoSequence.join(l3);
            AtomicComparer comparer = NumericComparer.getInstance();
            Configuration config = new Configuration();
            XPathContext context = config.getConversionContext();
            assertTrue(DeepEqual.deepEqual(chain3.iterate(), new AscendingRangeIterator(1, 1, 100), comparer, context, 0));
            assertEquals(51, ((IntegerValue)chain3.itemAt(50)).longValue());
        } catch (XPathException e) {
            fail();
        }
    }

    @Test
    public void testSubsequence() {
        try {
            List<GroundedValue> initial = new ArrayList<>();
            initial.add(new IntegerRange(1, 1, 20));
            initial.add(new IntegerRange(21, 1, 40));
            initial.add(new IntegerRange(41, 1, 60));
            ZenoSequence chain = ZenoSequence.join(initial);

            List<GroundedValue> l2 = new ArrayList<>();
            for (int i=61; i<=100; i++) {
                l2.add(new Int64Value(i));
            }
            ZenoSequence chain2 = ZenoSequence.join(l2);
            List<GroundedValue> l3 = new ArrayList<>();
            l3.add(chain);
            l3.add(chain2);
            ZenoSequence chain3 = ZenoSequence.join(l3);

            GroundedValue sub = chain3.subsequence(40, 20);
            AtomicComparer comparer = NumericComparer.getInstance();
            Configuration config = new Configuration();
            XPathContext context = config.getConversionContext();
            assertTrue(DeepEqual.deepEqual(sub.iterate(), new AscendingRangeIterator(41, 1, 60), comparer, context, 0));
        } catch (XPathException e) {
            fail();
        }
    }
}

