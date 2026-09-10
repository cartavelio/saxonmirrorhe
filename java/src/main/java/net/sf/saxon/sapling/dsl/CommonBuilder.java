package net.sf.saxon.sapling.dsl;

import net.sf.saxon.s9api.QName;
import net.sf.saxon.sapling.*;

public interface CommonBuilder<T> {
    public void append(SaplingNode node);

    public T builder();

    public T defaultNs(String uri);

    public String getDefaultNs();

    public default SaplingElement emptyElement(String name) {
        SaplingElement element;
        if (getDefaultNs() != null) {
            QName qName = new QName(getDefaultNs(), name);
            element = Saplings.elem(qName);
        } else {
            element = Saplings.elem(name);
        }
        return element;
    }

    public default SaplingElement emptyElement(QName qName) {
        return Saplings.elem(qName);
    }

    public default T buildWithElement(SaplingElement element, ElementDsl definition) {
        ElementBuilder childBuilder = new ElementBuilder(element);
        childBuilder.defaultNs(getDefaultNs());
        definition.buildWith(childBuilder);
        this.append(childBuilder.getResult());
        return this.builder();
    }

    public default T elem(String name) {
        this.append(emptyElement(name));
        return this.builder();
    }

    public default T elem(QName qName) {
        this.append(this.emptyElement(qName));
        return this.builder();
    }

    public default T elem(String name, String content) {
        SaplingElement element = this.emptyElement(name).withText(content);
        this.append(element);
        return this.builder();
    }

    public default T elem(QName qName, String content) {
        SaplingElement element = this.emptyElement(qName).withText(content);
        this.append(element);
        return this.builder();
    }

    public default T elem(String name, ElementDsl definition) {
        SaplingElement element = this.emptyElement(name);
        return buildWithElement(element, definition);
    }

    public default T elem(QName qName, ElementDsl definition) {
        SaplingElement element = this.emptyElement(qName);
        return buildWithElement(element, definition);
    }

    public default T text(String value) {
        SaplingText text = Saplings.text(value);
        this.append(text);
        return this.builder();
    }

    public default T comment(String value) {
        SaplingComment comment = Saplings.comment(value);
        this.append(comment);
        return this.builder();
    }

    public default T pi(String target, String value) {
        SaplingProcessingInstruction pi = Saplings.pi(target, value);
        this.append(pi);
        return this.builder();
    }
}
