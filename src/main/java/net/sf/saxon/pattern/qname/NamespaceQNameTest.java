package net.sf.saxon.pattern.qname;

import net.sf.saxon.om.NamePool;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StructuredQName;

public class NamespaceQNameTest implements QNameTest {

    private final NamespaceUri namespace;

    public NamespaceQNameTest(NamespaceUri namespace) {
        this.namespace = namespace;
    }

    @Override
    public boolean matches(StructuredQName qname) {
        return qname.hasURI(namespace);
    }

    @Override
    public boolean matchesFingerprint(NamePool namePool, int fp) {
        return namePool.getURI(fp) == namespace;
    }

    @Override
    public String exportQNameTest() {
        return "Q{" + namespace + "}*";
    }

    public NamespaceUri getNamespace() {
        return namespace;
    }
}

