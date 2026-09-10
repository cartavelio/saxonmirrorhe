package net.sf.saxon.pattern.qname;

import net.sf.saxon.om.NamePool;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.pattern.UnionQNameTest;
import net.sf.saxon.type.Affinity;

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
        return toString();
    }

    public NamespaceUri getNamespace() {
        return namespace;
    }

    public Affinity relationship(QNameTest other) {
        if (other instanceof AnyQNameTest) {
            return Affinity.SUBSUMED_BY;
        } else if (other instanceof SpecificQNameTest) {
            return ((SpecificQNameTest) other).getStructuredQName().hasURI(namespace) ? Affinity.SUBSUMES : Affinity.DISJOINT;
        } else if (other instanceof NamespaceQNameTest) {
            return ((NamespaceQNameTest) other).namespace == namespace ? Affinity.SAME_TYPE : Affinity.DISJOINT;
        } else if (other instanceof LocalQNameTest) {
            return Affinity.OVERLAPS;
        } else if (other instanceof NoQNameTest) {
            return Affinity.SUBSUMES;
        } else if (other instanceof UnionQNameTest) {
            return new UnionQNameTest(this).relationship(other);
        } else {
            throw new IllegalArgumentException();
        }
    }

    @Override
    public String toString() {
        return "Q{" + namespace + "}*";
    }

    @Override
    public int hashCode() {
        return namespace.hashCode();
    }

    @Override
    public boolean equals(Object obj) {
        return obj instanceof NamespaceQNameTest && namespace == ((NamespaceQNameTest)obj).namespace;
    }
}

