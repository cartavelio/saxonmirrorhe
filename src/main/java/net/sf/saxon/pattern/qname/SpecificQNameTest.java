package net.sf.saxon.pattern.qname;

import net.sf.saxon.om.NamePool;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.pattern.NameTest;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.pattern.PortableNameTest;
import net.sf.saxon.pattern.UnionQNameTest;
import net.sf.saxon.type.Affinity;

public class SpecificQNameTest implements QNameTest {

    private final int fingerprint;
    private final StructuredQName qName;

    private final NamePool namePool;

    public SpecificQNameTest(StructuredQName qName, NamePool pool) {
        this.namePool = pool;
        this.qName = qName;
        this.fingerprint = pool.allocateFingerprint(qName.getNamespaceUri(), qName.getLocalPart());
    }

    public SpecificQNameTest(NamePool pool, int fingerprint) {
        this.namePool = pool;
        this.fingerprint = fingerprint;
        this.qName = namePool.getStructuredQName(fingerprint);
    }

    public SpecificQNameTest(StructuredQName qName, NamePool pool, int fingerprint) {
        this.namePool = pool;
        this.fingerprint = fingerprint;
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
        return fp == fingerprint;
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
            return this.fingerprint == ((SpecificQNameTest)other).fingerprint ? Affinity.SAME_TYPE : Affinity.DISJOINT;
        } else if (other instanceof PortableQNameTest) {
            return this.getStructuredQName().equals(((PortableQNameTest)other).getStructuredQName()) ? Affinity.SAME_TYPE : Affinity.DISJOINT;
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
        return new NameTest(nodeKind, qName.getNamespaceUri(), qName.getLocalPart(), namePool);
    }

    @Override
    public String toString() {
        //return qName.getEQName();
        return qName.getLocalPart();
    }

    @Override
    public int hashCode() {
        return fingerprint;
    }

    @Override
    public boolean equals(Object obj) {
        return obj instanceof SpecificQNameTest && ((SpecificQNameTest)obj).fingerprint == fingerprint;
    }
}

