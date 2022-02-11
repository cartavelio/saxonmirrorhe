package performance;

import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ListIterator;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.IntegerValue;

import java.util.ArrayList;
import java.util.List;

public class ArrayPerformance extends PerformanceTestCase {

    public void measurePut(final int n) {
        try {
            List<AtomicValue> numbers = new ArrayList<>(n);
            for (int i=0; i<n; i++) {
                numbers.add(new Int64Value(i));
            }
            final SimpleArrayItem array = SimpleArrayItem.makeSimpleArrayItem(new ListIterator<>(numbers));
            measure(new Measurable() {

                ArrayItem a = array;
                @Override
                public void run() {
                    try {
                        ArrayItem a = array;
                        for (int i=0; i<n; i++) {
                            a = a.put(i, ((IntegerValue)a.get(i)).plus(Int64Value.PLUS_ONE));
                        }
                    } catch (XPathException e) {
                        e.printStackTrace();
                        fail(e.getMessage());
                    }
                }

                @Override
                public int expectedTimeMilliseconds() {
                    return n/1000;
                }

                @Override
                public String name() {
                    return "testPut" + n;
                }
            });
        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testPut1K() {
        measurePut(1000);
    }

    public void testPut10K() {
        measurePut(10000);
    }

    public void testPut100K() {
        measurePut(100000);
    }

//    public void testPut1M() {
//        measurePut(1000000);
//    }
}

