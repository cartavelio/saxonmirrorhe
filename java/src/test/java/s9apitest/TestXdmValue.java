package s9apitest;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;

/**
 * Test XdmValue class, representing an arbitrary value
 */
public class TestXdmValue {

    @Test
    public void testSubsequence() {
        XdmValue v = XdmEmptySequence.getInstance();
        v = v.append(new XdmAtomicValue(10));
        v = v.append(new XdmAtomicValue(11));
        v = v.append(new XdmAtomicValue(12));
        v = v.append(new XdmAtomicValue(13));
        v = v.append(new XdmAtomicValue(14));
        XdmValue v1 = v.subsequence(1, 4);
        assertEquals(v1.size(), 4);
        assertEquals(v1.itemAt(0).getStringValue(), "11");
        XdmValue v2 = v.subsequence(0, 4);
        assertEquals(v2.size(), 4);
        assertEquals(v2.itemAt(0).getStringValue(), "10");
        XdmValue v3 = v.subsequence(-1, 3);
        assertEquals(v3.size(), 3);
        assertEquals(v3.itemAt(0).getStringValue(), "10");
    }

    @Test
    public void testFilterByPredicate() {
        XdmValue v = XdmEmptySequence.getInstance();
        v = v.append(new XdmAtomicValue(10));
        v = v.append(new XdmAtomicValue(11));
        v = v.append(new XdmAtomicValue(12));
        v = v.append(new XdmAtomicValue(13));
        v = v.append(new XdmAtomicValue(14));
        XdmValue w = v.where(it -> {
            try {
                return ((XdmAtomicValue)it).getLongValue() > 12 ;
            } catch (SaxonApiException e) {
                return false;
            }
        });
        assertEquals(w.size(), 2);
        assertEquals(w.itemAt(0).getStringValue(), "13");
    }

    @Test
    public void testFilterByType() {
        XdmValue v = XdmEmptySequence.getInstance();
        v = v.append(new XdmAtomicValue(10));
        v = v.append(new XdmAtomicValue(11.1));
        v = v.append(new XdmAtomicValue(12));
        v = v.append(new XdmAtomicValue(13.1));
        v = v.append(new XdmAtomicValue(14));
        XdmValue w = v.where(ItemType.INTEGER);
        assertEquals(3, w.size());
        assertEquals("10\n12\n14", w.toString());
        assertEquals("10", w.itemAt(0).getStringValue(), "10");
    }


}




