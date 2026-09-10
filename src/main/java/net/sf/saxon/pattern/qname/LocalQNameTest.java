package net.sf.saxon.pattern.qname;

import net.sf.saxon.om.NamePool;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.pattern.LocalNameTest;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.pattern.UnionQNameTest;
import net.sf.saxon.type.Affinity;

public class LocalQNameTest implements QNameTest {

    private final String localName;

    public LocalQNameTest(String local) {
        this.localName = local;
    }

    public String getLocalName() {
        return localName;
    }

    @Override
    public boolean matches(StructuredQName qname) {
        return qname.getLocalPart().equals(localName);
    }

    @Override
    public boolean matchesFingerprint(NamePool namePool, int fp) {
        return namePool.getLocalName(fp).equals(localName);
    }

    public Affinity relationship(QNameTest other) {
        if (other instanceof AnyQNameTest) {
            return Affinity.SUBSUMED_BY;
        } else if (other instanceof SpecificQNameTest) {
            return ((SpecificQNameTest) other).getStructuredQName().getLocalPart().equals(localName) ? Affinity.SUBSUMES : Affinity.DISJOINT;
        } else if (other instanceof NamespaceQNameTest) {
            return Affinity.OVERLAPS;
        } else if (other instanceof LocalQNameTest) {
            return ((LocalQNameTest) other).localName.equals(localName) ? Affinity.SAME_TYPE : Affinity.DISJOINT;
        } else if (other instanceof NoQNameTest) {
            return Affinity.SUBSUMES;
        } else if (other instanceof UnionQNameTest) {
            return new UnionQNameTest(this).relationship(other);
        } else {
            throw new IllegalArgumentException();
        }
    }

    @Override
    public NodeTest makeNodeTest(int nodeKind) {
        return new LocalNameTest(nodeKind, localName);
    }

    @Override
    public String exportQNameTest() {
        return toString();
    }

    @Override
    public String toString() {
        return "*:" + localName;
    }

    @Override
    public int hashCode() {
        return localName.hashCode();
    }

    @Override
    public boolean equals(Object obj) {
        return obj instanceof LocalQNameTest && ((LocalQNameTest)obj).localName.equals(localName);
    }
}

