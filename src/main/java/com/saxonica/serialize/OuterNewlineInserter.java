package com.saxonica.serialize;

import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NamespaceMap;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.serialize.XMLEmitter;
import net.sf.saxon.str.BMPString;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;

/**
 * OuterNewlineInserter: This ProxyReceiver inserts a newline after any comment or PI child of the document
 * node that precedes the first element child, and before any comment or PI child that follows the first
 * element child. This action is currently invoked only when saxon:canonical=yes is requested, though it
 * could in principle be made available as a free-standing option.
 */


public class OuterNewlineInserter extends ProxyReceiver {

    private int level = 0;
    private boolean beforeFirstElement = true;

    private static final UnicodeString NEWLINE = BMPString.of("\n");

    /**
     * Create an XML Indenter
     *
     * @param next the next receiver in the pipeline, always an XMLEmitter
     */

    public OuterNewlineInserter(XMLEmitter next) {
        super(next);
    }

    /**
     * Start of document
     */

    @Override
    public void open() throws XPathException {
        nextReceiver.open();
    }

    /**
     * Output element start tag
     */

    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties) throws XPathException {
        nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
        level++;
        beforeFirstElement = false;
    }


    /**
     * Output element end tag
     */

    @Override
    public void endElement() throws XPathException {
        level--;
        nextReceiver.endElement();
    }

    /**
     * Output a processing instruction
     */

    @Override
    public void processingInstruction(String target, UnicodeString data, Location locationId, int properties) throws XPathException {
        if (level == 0 && !beforeFirstElement) {
            characters(NEWLINE, locationId, ReceiverOption.NONE);
        }
        nextReceiver.processingInstruction(target, data, locationId, properties);
        if (level == 0 && beforeFirstElement) {
            characters(NEWLINE, locationId, ReceiverOption.NONE);
        }
    }

    /**
     * Output a comment
     */

    @Override
    public void comment(UnicodeString chars, Location locationId, int properties) throws XPathException {
        if (level == 0 && !beforeFirstElement) {
            characters(NEWLINE, locationId, ReceiverOption.NONE);
        }
        nextReceiver.comment(chars, locationId, properties);
        if (level == 0 && beforeFirstElement) {
            characters(NEWLINE, locationId, ReceiverOption.NONE);
        }
    }

}

// Copyright (c) 2018-2023 Saxonica Limited

