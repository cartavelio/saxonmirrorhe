package net.sf.saxon.sapling.dsl;

import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.s9api.QName;
import net.sf.saxon.sapling.*;

public class ElementBuilder implements CommonBuilder<ElementBuilder> {
    private String name;
    private QName qName;
    private String defaultNs;
    private SaplingElement element;

    public ElementBuilder(SaplingElement element) {
        this.element = element;
        this.defaultNs = null;
    }

    @Override
    public void append(SaplingNode node) {
        this.element = element.withChild(node);
    }

    @Override
    public ElementBuilder builder() {
        return this;
    }

    public ElementBuilder attr(String name, String value) {
        this.element = element.withAttr(name, value);
        return this;
    }

    public ElementBuilder attr(QName name, String value) {
        this.element = element.withAttr(name, value);
        return this;
    }

    public ElementBuilder ns(String prefix, String uri) {
        this.element = element.withNamespace(prefix, uri);
        return this;
    }

    @Override
    public ElementBuilder defaultNs(String uri) {
        this.defaultNs = uri;
        return this;
    }

    @Override
    public String getDefaultNs() {
        return defaultNs;
    }

    public SaplingElement getResult() {
        return element;
    }
}
