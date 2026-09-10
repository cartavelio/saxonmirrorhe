package net.sf.saxon.sapling.dsl;

import net.sf.saxon.sapling.*;

public class DocumentBuilder implements CommonBuilder<DocumentBuilder>{
    private SaplingDocument document;
    private String defaultNamespace;

    public DocumentBuilder(SaplingDocument document) {
        this.document = document;
    }

    @Override
    public void append(SaplingNode node) {
        this.document = this.document.withChild(node);
    }

    @Override
    public DocumentBuilder builder() {
        return this;
    }

    public SaplingDocument getResult() {
        return document;
    }

    public DocumentBuilder defaultNs(String uri) {
        this.defaultNamespace = uri;
        return this;
    }

    @Override
    public String getDefaultNs() {
        return defaultNamespace;
    }
}
