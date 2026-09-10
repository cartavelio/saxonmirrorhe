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
 * ItemFeed implementation supporting the {@code fn:exists()}, {@code fn:empty()},
 * and {@code fn:count()} function
 */

public abstract class ExistenceFeed extends ItemFeed {

    private int level = 0;

    protected ExistenceFeed(Expression exp, ItemFeed result, XPathContext context) {
        super(exp, result, context);
    }

    public abstract void action() throws XPathException;


    @Override
    public void startDocument(int properties) throws XPathException {
        if (level == 0) {
            action();
        }
        level++;
    }

    @Override
    public void endDocument() throws XPathException {
        level--;
    }

    @Override
    public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties) throws XPathException {
        if (level == 0) {
            action();
        }
        level++;
    }

    @Override
    public void startElement(NodeName elemName, SchemaType type, AttributeMap attributes, NamespaceMap namespaces, Location location, int properties) throws XPathException {
        if (level == 0) {
            action();
        }
        level++;
    }

    @Override
    public void namespace(String prefix, NamespaceUri namespaceUri, int properties) throws XPathException {
        if (level == 0) {
            action();
        }
    }

    @Override
    public void endElement() throws XPathException {
        level--;
    }

    @Override
    public void characters(UnicodeString chars, Location location, int properties) throws XPathException {
        if (level == 0) {
            action();
        }
    }

    @Override
    public void comment(UnicodeString content, Location location, int properties) throws XPathException {
        if (level == 0) {
            action();
        }
    }

    @Override
    public void attribute(NodeName attName, SimpleType typeCode, String value, Location location, int properties) throws XPathException {
        if (level == 0) {
            action();
        }
    }

    @Override
    public void append(Item item) throws XPathException {
        if (level == 0) {
            action();
        }
    }

    @Override
    public abstract void close() throws XPathException;
}

// Copyright (c) 2018-2022 Saxonica Limited