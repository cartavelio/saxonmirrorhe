package net.sf.saxon.z;

public interface IntPredicateProxy
//#if CSHARP==false
        extends java.util.function.IntPredicate
//#endif
{
    boolean test(int value);

    default IntPredicateProxy union(IntPredicateProxy other) {
        return IntPredicateLambda.of(val -> test(val) || other.test(val));
    }

}

