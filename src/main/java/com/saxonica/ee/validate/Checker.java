package com.saxonica.ee.validate;

import net.sf.saxon.Configuration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.NamespaceBindingSet;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;

/**
 * A Checker is a Receiver used to check the content of elements and attributes, especially those
 * that participate in uniqueness, key, and keyref constraints
 */

public abstract class Checker implements Receiver {

    private PipelineConfiguration pipe;
    private String systemId;

    public void attribute(NodeName nameCode, SimpleType typeCode, CharSequence value, Location locationId, int properties) throws XPathException {
        //
    }

    public void characters(CharSequence chars, Location locationId, int properties) throws XPathException {
        //
    }

    public void close() throws XPathException {
        //
    }

    public void comment(CharSequence content, Location locationId, int properties) throws XPathException {
        //
    }

    public void endDocument() throws XPathException {
        //
    }

    public void endElement() throws XPathException {
        //
    }

    public Configuration getConfiguration() {
        return pipe.getConfiguration();
    }

    /*@NotNull*/
    public PipelineConfiguration getPipelineConfiguration() {
        return pipe;
    }

    public void namespace(NamespaceBindingSet namespaceBindings, int properties) throws XPathException {
        //
    }

    public void open() throws XPathException {
        //
    }

    public void processingInstruction(String name, CharSequence data, Location locationId, int properties) throws XPathException {
        //
    }

    public void setPipelineConfiguration(/*@NotNull*/ PipelineConfiguration config) {
        pipe = config;
    }

    public void setSystemId(String systemId) {
        this.systemId = systemId;
    }

    public void setUnparsedEntity(String name, String systemID, String publicID) throws XPathException {
        //
    }

    public void startContent() throws XPathException {
        //
    }

    public void startDocument(int properties) throws XPathException {
        //
    }

    public void startElement(NodeName nameCode, SchemaType typeCode, Location location, int properties) throws XPathException {
        //
    }

    public String getSystemId() {
        return systemId;
    }

    public boolean usesTypeAnnotations() {
        return false;
    }
}

// Copyright (c) 2005-2009 Saxonica Limited.
