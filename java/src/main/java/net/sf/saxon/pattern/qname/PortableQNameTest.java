package net.sf.saxon.pattern.qname;

import net.sf.saxon.om.NamePool;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.pattern.PortableNameTest;
import net.sf.saxon.pattern.UnionQNameTest;
import net.sf.saxon.type.Affinity;

/**
 * An alternative to {@link SpecificQNameTest} that is not bound to a specific name pool.
 */

public class PortableQNameTest implements QNameTest {

    private final StructuredQName qName;


    public PortableQNameTest(StructuredQName qName) {
        this.qName = qName;
    }

    public StructuredQName getStructuredQName() {
        return qName;
    }

    @Override
    public boolean matches(StructuredQName qname) {
        return qname.equals(this.qName);
    }

    @Override
    public boolean matchesFingerprint(NamePool namePool, int fp) {
        return namePool.allocateFingerprint(qName.getNamespaceUri(), qName.getLocalPart()) == fp;
    }

    @Override
    public String exportQNameTest() {
        return qName.getEQName();
    }

    @Override
    public Affinity relationship(QNameTest other) {
        if (other instanceof AnyQNameTest) {
            return Affinity.SUBSUMED_BY;
        } else if (other instanceof SpecificQNameTest) {
            return this.qName.equals(((SpecificQNameTest) other).getStructuredQName()) ? Affinity.SAME_TYPE : Affinity.DISJOINT;
        } else if (other instanceof PortableQNameTest) {
            return this.qName.equals(((PortableQNameTest)other).qName) ? Affinity.SAME_TYPE : Affinity.DISJOINT;
        } else if (other instanceof NamespaceQNameTest) {
            return qName.hasURI(((NamespaceQNameTest)other).getNamespace()) ? Affinity.SUBSUMED_BY : Affinity.DISJOINT;
        } else if (other instanceof LocalQNameTest) {
            return qName.getLocalPart().equals(((LocalQNameTest)other).getLocalName()) ? Affinity.SUBSUMED_BY : Affinity.DISJOINT;
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
        return new PortableNameTest(nodeKind, qName.getNamespaceUri(), qName.getLocalPart());
    }

    @Override
    public String toString() {
        //return qName.getEQName();
        return qName.getLocalPart();
    }

    @Override
    public int hashCode() {
        return qName.hashCode();
    }

    @Override
    public boolean equals(Object obj) {
        return obj instanceof PortableQNameTest && ((PortableQNameTest)obj).qName.equals(qName);
    }
}

