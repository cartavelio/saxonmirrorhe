package com.saxonica.ee.trans;

import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Sink;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.serialize.JSONEmitter;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.value.StringValue;

/**
 * This class handles export of packages for Saxon-JS 2.0 by converting the XML representation
 * of SEF files to a JSON representation, at the level of emitting events.
 */

public class ExpressionPresenterToJson extends ProxyReceiver {

    private final JSONEmitter out;
    private boolean pending = false;
    private int level = 0;

    public ExpressionPresenterToJson(JSONEmitter out, PipelineConfiguration pipe) {
        super(new Sink(pipe));
        this.out = out;
    }

    /**
     * Notify the start of an element
     */
    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties) throws XPathException {
        if (pending) {
            out.writeKey("C");
            out.startArray(false);
        }
        out.startMap(false);
        out.writeKey("N");
        out.writeAtomicValue(new StringValue(elemName.getLocalPart()));
        for (AttributeInfo att : attributes) {
            out.writeKey(att.getNodeName().getLocalPart());
            out.writeAtomicValue(new StringValue(att.getValue()));
        }
        pending = true;
        level++;
    }

    /**
     * End of element
     */
    @Override
    public void endElement() throws XPathException {
        if (!pending) {
            out.endArray();
        }
        if (--level > 0) {
            // at outermost level, leave room for the checksum
            out.endMap();
        }
        pending = false;
    }

    @Override
    public void processingInstruction(String target, UnicodeString data, Location locationId, int properties) throws XPathException {
        // Used to output the checksum
        out.writeKey(target);
        out.writeAtomicValue(new StringValue(data.tidy()));
    }

    /**
     * Notify the end of a document node
     */
    @Override
    public void endDocument() throws XPathException {
        out.endMap();
    }

    /**
     * End of output. Note that closing this receiver also closes the rest of the
     * pipeline.
     */
    @Override
    public void close() throws XPathException {
        out.close();
    }
}

// Copyright (c) 2018-2022 Saxonica Limited

