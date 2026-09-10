package performance;

import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ListIterator;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.IntegerValue;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;

import java.util.ArrayList;
import java.util.List;

import static org.junit.jupiter.api.Assertions.fail;

public class ArrayPerformance extends PerformanceTestCase {

    public void measurePut(final int n) {
        try {
            List<AtomicValue> numbers = new ArrayList<>(n);
            for (int i=0; i<n; i++) {
                numbers.add(new Int64Value(i));
            }
            final SimpleArrayItem array = SimpleArrayItem.makeSimpleArrayItem(new ListIterator.Of<>(numbers));
            measure(new Measurable() {

                final ArrayItem a = array;
                @Override
                public void run() {
                    ArrayItem a = array;
                    for (int i=0; i<n; i++) {
                        a = a.put(i, ((IntegerValue)a.get(i)).plus(Int64Value.PLUS_ONE));
                    }
                }

                @Override
                public int expectedTimeMicroseconds() {
                    return n;
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

    @Test
    @Tag("performance")
    public void testPut1K() {
        measurePut(500);
    }

    @Test
    @Tag("performance")
    public void testPut10K() {
        measurePut(5000);
    }

    @Test
    @Tag("performance")
    public void testPut100K() {
        measurePut(50000);
    }

//    public void testPut1M() {
//        measurePut(1000000);
//    }
}

