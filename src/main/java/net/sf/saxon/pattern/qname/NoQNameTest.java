package net.sf.saxon.pattern.qname;

import net.sf.saxon.om.NamePool;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.type.Affinity;

public class NoQNameTest implements QNameTest {

    private final static NoQNameTest INSTANCE = new NoQNameTest();

    public static NoQNameTest getInstance() {
        return INSTANCE;
    }

    private NoQNameTest(){};

    @Override
    public boolean matches(StructuredQName qname) {
        return false;
    }

    @Override
    public boolean matchesFingerprint(NamePool namePool, int fp) {
        return false;
    }

    public Affinity relationship(QNameTest other) {
        if (other instanceof NoQNameTest) {
            return Affinity.SAME_TYPE;
        } else {
            return Affinity.SUBSUMED_BY;
        }
    }


    @Override
    public String exportQNameTest() {
        return "xs:error";
    }
}

