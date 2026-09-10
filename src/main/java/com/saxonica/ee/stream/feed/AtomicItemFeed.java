package com.saxonica.ee.stream.feed;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;

/**
 * An AtomicItemFeed is an ItemFeed in which the only allowed event is {@link #append(Item)}
 * passing an atomic value. All other events result in an {@link IllegalStateException}. This
 * is appropriate for feeds implementing expressions that atomize their operands, since the
 * compiler should ensure that only atomic values are delivered to such expressions.
 */

public abstract class AtomicItemFeed extends ItemFeed {

    public AtomicItemFeed(ItemFeed result, XPathContext context) {
        super(result, context);
    }

    public AtomicItemFeed(Expression exp, ItemFeed result, XPathContext context) {
        super(exp, result, context);
    }

    private void notAllowed() {
        throw new IllegalStateException("Nodes cannot be passed to an instance of " + getClass().getName());
    }

    @Override
    public void startDocument(int properties) throws XPathException {
        notAllowed();
    }

    @Override
    public void endDocument() throws XPathException {
        notAllowed();
    }

    @Override
    public void setUnparsedEntity(String name, String systemID, String publicID) throws XPathException {
        notAllowed();
    }

    @Override
    public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties) throws XPathException {
        notAllowed();
    }

    @Override
    public void namespace(String prefix, NamespaceUri namespaceUri, int properties) throws XPathException {
        notAllowed();
    }

    @Override
    public void attribute(NodeName attName, SimpleType typeCode, String value, Location location, int properties) throws XPathException {
        notAllowed();
    }

    @Override
    public void startContent() throws XPathException {
        notAllowed();
    }

    @Override
    public void endElement() throws XPathException {
        notAllowed();
    }

    @Override
    public void characters(UnicodeString chars, Location location, int properties) throws XPathException {
        notAllowed();
    }

    @Override
    public void processingInstruction(String name, UnicodeString data, Location location, int properties) throws XPathException {
        notAllowed();
    }

    @Override
    public void comment(UnicodeString content, Location location, int properties) throws XPathException {
        notAllowed();
    }

    @Override
    public void startElement(NodeName elemName, SchemaType type, AttributeMap attributes, NamespaceMap namespaces, Location location, int properties) throws XPathException {
        notAllowed();
    }

}

// Copyright (c) 2018-2023 Saxonica Limited
