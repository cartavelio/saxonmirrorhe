package com.saxonica.serialize;

import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.serialize.XMLEmitter;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;

/**
 * OuterNewlineInserter: This ProxyReceiver inserts a newline after any comment or PI child of the document
 * node that precedes the first element child, and before any comment or PI child that follows the first
 * element child. This action is currently invoked only when saxon:canonical=yes is requested, though it
 * could in principle be made available as a free-standing option.
 *
 * @author Michael Kay
 */


public class OuterNewlineInserter extends ProxyReceiver {

    private int level = 0;
    private boolean beforeFirstElement = true;

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

    public void open() throws XPathException {
        nextReceiver.open();
    }

    /**
     * Output element start tag
     */

    public void startElement(NodeName nameCode, SchemaType type, Location location, int properties) throws XPathException {
        nextReceiver.startElement(nameCode, type, location, properties);
        level++;
        beforeFirstElement = false;
    }


    /**
     * Output element end tag
     */

    public void endElement() throws XPathException {
        level--;
        nextReceiver.endElement();
    }

    /**
     * Output a processing instruction
     */

    public void processingInstruction(String target, CharSequence data, Location locationId, int properties) throws XPathException {
        if (level == 0 && !beforeFirstElement) {
            characters("\n", locationId, 0);
        }
        nextReceiver.processingInstruction(target, data, locationId, properties);
        if (level == 0 && beforeFirstElement) {
            characters("\n", locationId, 0);
        }
    }

    /**
     * Output a comment
     */

    public void comment(CharSequence chars, Location locationId, int properties) throws XPathException {
        if (level == 0 && !beforeFirstElement) {
            characters("\n", locationId, 0);
        }
        nextReceiver.comment(chars, locationId, properties);
        if (level == 0 && beforeFirstElement) {
            characters("\n", locationId, 0);
        }
    }

}

// Copyright (c) 2018 Saxonica Ltd

