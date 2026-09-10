package com.saxonica.ee.stream;

import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.StartTagBuffer;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;

/**
 * A ContentDetector is a filter that can be added to a Receiver pipeline to note whether the current
 * element node has children. It delays sending the startContent() event to the next Receiver in the pipeline
 * until it is known whether the start tag is followed by children, or is followed immediately by an end tag.
 * It then passes this information to subsequent filters in the pipeline by use of property flags.
 */
public class ContentDetector extends ProxyReceiver {

    private boolean pending = false;

    public ContentDetector(StartTagBuffer nextReceiver) {
        super(nextReceiver);
    }

    /**
     * Notify the start of an element
     *  @param elemName   integer code identifying the name of the element within the name pool.
     * @param typeCode   integer code identifying the element's type within the name pool.
     * @param location
     * @param properties properties of the element node
     */

    public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties) throws XPathException {
        flush(true);
        pending = false;
        nextReceiver.startElement(elemName, typeCode, location, properties);
    }

    /**
     * Notify the start of the content, that is, the completion of all attributes and namespaces.
     * Note that the initial receiver of output from XSLT instructions will not receive this event,
     * it has to detect it itself. Note that this event is reported for every element even if it has
     * no attributes, no namespaces, and no content.
     */


    public void startContent() throws XPathException {
        pending = true;
    }

    /**
     * End of element
     */

    public void endElement() throws XPathException {
        flush(false);
        nextReceiver.endElement();
    }

    /**
     * Character data
     */

    public void characters(CharSequence chars, Location locationId, int properties) throws XPathException {
        flush(true);
        nextReceiver.characters(chars, locationId, properties);
    }


    /**
     * Processing Instruction
     */

    public void processingInstruction(String target, CharSequence data, Location locationId, int properties) throws XPathException {
        flush(true);
        nextReceiver.processingInstruction(target, data, locationId, properties);
    }

    /**
     * Output a comment
     */

    public void comment(CharSequence chars, Location locationId, int properties) throws XPathException {
        flush(true);
        nextReceiver.comment(chars, locationId, properties);
    }

    /**
     * Output a delayed startContent() event
     * @param nonEmpty false if the content was empty, true if children were found
     * @throws XPathException if the next receiver in the pipeline reports an error
     */

    private void flush(boolean nonEmpty) throws XPathException {
        if (pending) {
            ((StartTagBuffer)nextReceiver).setHasChildren(nonEmpty);
            pending = false;
            nextReceiver.startContent();
        }
    }


}

// Copyright (c) 2018 Saxonica Limited