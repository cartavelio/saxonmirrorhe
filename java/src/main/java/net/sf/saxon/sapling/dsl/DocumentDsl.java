package net.sf.saxon.sapling.dsl;

@FunctionalInterface
public interface DocumentDsl {
    void buildWith(DocumentBuilder builder);
}
