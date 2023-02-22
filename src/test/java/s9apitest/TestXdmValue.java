package s9apitest;

import net.sf.saxon.s9api.XdmAtomicValue;
import net.sf.saxon.s9api.XdmEmptySequence;
import net.sf.saxon.s9api.XdmValue;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;

/**
 * Test XdmArray class, representing an XPath map item
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


}




