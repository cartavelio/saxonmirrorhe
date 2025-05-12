package net.sf.saxon.test.xdmfixtures;

@FunctionalInterface
public interface DslLambda<T> {
    abstract void buildWith(T definition);
}
