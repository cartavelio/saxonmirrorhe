package net.sf.saxon.pattern.qname;

import net.sf.saxon.om.NamePool;
import net.sf.saxon.om.StructuredQName;

public class LocalQNameTest implements QNameTest {

    private final String localName;

    public LocalQNameTest(String local) {
        this.localName = local;
    }

    @Override
    public boolean matches(StructuredQName qname) {
        return qname.getLocalPart().equals(localName);
    }

    @Override
    public boolean matchesFingerprint(NamePool namePool, int fp) {
        return namePool.getLocalName(fp).equals(localName);
    }

    @Override
    public String exportQNameTest() {
        return "*:" + localName;
    }
}

