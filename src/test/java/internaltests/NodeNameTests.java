package internaltests;

import net.sf.saxon.om.CodedName;
import net.sf.saxon.om.FingerprintedQName;
import net.sf.saxon.om.NamePool;
import net.sf.saxon.om.NoNamespaceName;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

/**
 * Test implementations of NodeName
 */
public class NodeNameTests {

    @Test
    public void testCodedName() {
        FingerprintedQName n = new FingerprintedQName("foo", "http://ns1/", "test");
        NamePool pool = new NamePool();
        int code = pool.allocateFingerprint("http://ns1/", "test");
        CodedName cd = new CodedName(code, "", pool);
        boolean b = cd.equals(n);
        assertEquals(n.hashCode(), cd.hashCode());
        assertTrue(b);
        assertEquals(n, cd);
    }

    @Test
    public void testNoNamespaceName() {
        FingerprintedQName n = new FingerprintedQName("", "", "test");
        NoNamespaceName cd = new NoNamespaceName("test");
        boolean b = cd.equals(n);
        assertEquals(n.hashCode(), cd.hashCode());
        assertTrue(b);
        assertEquals(n, cd);
    }
}


