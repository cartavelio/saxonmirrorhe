package net.sf.saxon.sapling.dsl;

@FunctionalInterface
public interface ElementDsl {
    void buildWith(ElementBuilder builder);
}
