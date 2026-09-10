package net.sf.saxon.pattern.qname;

import net.sf.saxon.om.NamePool;
import net.sf.saxon.om.StructuredQName;

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
}

