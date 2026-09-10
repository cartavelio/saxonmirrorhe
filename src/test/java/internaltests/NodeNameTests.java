package internaltests;

import junit.framework.TestCase;
import net.sf.saxon.om.CodedName;
import net.sf.saxon.om.FingerprintedQName;
import net.sf.saxon.om.NamePool;
import net.sf.saxon.om.NoNamespaceName;

/**
 * Test implementations of NodeName
 */
public class NodeNameTests extends TestCase {

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

    public void testNoNamespaceName() {
        FingerprintedQName n = new FingerprintedQName("", "", "test");
        NoNamespaceName cd = new NoNamespaceName("test");
        boolean b = cd.equals(n);
        assertEquals(n.hashCode(), cd.hashCode());
        assertTrue(b);
        assertEquals(n, cd);
    }
}


