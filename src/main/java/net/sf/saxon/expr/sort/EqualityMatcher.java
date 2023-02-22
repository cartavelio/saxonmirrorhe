package net.sf.saxon.expr.sort;

public interface EqualityMatcher<T> {

    boolean equal(T a, T b);

    int hash(T a);
}

