package com.saxonica.ee.validate;

import net.sf.saxon.Configuration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.om.NamespaceMap;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;

/**
 * An abstract implementation of {@link Receiver} in which all events are (by default) ignored
 */

public abstract class AbstractReceiver implements Receiver {

    private PipelineConfiguration pipe;
    private String systemId;

    @Override
    public void characters(UnicodeString chars, Location locationId, int properties) throws XPathException {
        //
    }

    @Override
    public void close() throws XPathException {
        //
    }

    @Override
    public void comment(UnicodeString content, Location locationId, int properties) throws XPathException {
        //
    }

    @Override
    public void endDocument() throws XPathException {
        //
    }

    @Override
    public void endElement() throws XPathException {
        //
    }

    public Configuration getConfiguration() {
        return pipe.getConfiguration();
    }

    /*@NotNull*/
    @Override
    public PipelineConfiguration getPipelineConfiguration() {
        return pipe;
    }

    @Override
    public void open() throws XPathException {
        //
    }

    @Override
    public void processingInstruction(String name, UnicodeString data, Location locationId, int properties) throws XPathException {
        //
    }

    @Override
    public void setPipelineConfiguration(/*@NotNull*/ PipelineConfiguration config) {
        pipe = config;
    }

    @Override
    public void setSystemId(String systemId) {
        this.systemId = systemId;
    }

    @Override
    public void setUnparsedEntity(String name, String systemID, String publicID) throws XPathException {
        //
    }

    @Override
    public void startDocument(int properties) throws XPathException {
        //
    }

    @Override
    public void startElement(NodeName nameCode, SchemaType typeCode,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties) throws XPathException {
        //
    }

    @Override
    public String getSystemId() {
        return systemId;
    }

    @Override
    public boolean usesTypeAnnotations() {
        return false;
    }
}

// Copyright (c) 2018-2022 Saxonica Limited
