package net.sf.saxon.pattern.qname;

import net.sf.saxon.om.NamePool;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.type.Affinity;

public class AnyQNameTest implements QNameTest {

    private final static AnyQNameTest INSTANCE = new AnyQNameTest();

    public static AnyQNameTest getInstance() {
        return INSTANCE;
    }

    private AnyQNameTest(){};

    @Override
    public boolean matches(StructuredQName qname) {
        return true;
    }

    @Override
    public boolean matchesFingerprint(NamePool namePool, int fp) {
        return true;
    }

    @Override
    public String exportQNameTest() {
        return "*";
    }

    @Override
    public Affinity relationship(QNameTest other) {
        return other instanceof AnyQNameTest ? Affinity.SAME_TYPE : Affinity.SUBSUMES;
    }
}

