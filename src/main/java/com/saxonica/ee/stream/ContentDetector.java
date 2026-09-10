package com.saxonica.ee.stream;

import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.Event;
import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NamespaceMap;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;

/**
 * A ContentDetector is a filter that can be added to a Receiver pipeline to note whether the current
 * element node has children. It delays sending the startElement() event to the next Receiver in the pipeline
 * until it is known whether the start tag is followed by children, or is followed immediately by an end tag.
 * It then passes this information to subsequent filters in the pipeline by use of property flags.
 */
public class ContentDetector extends ProxyReceiver {

    private Event.StartElement pendingEvent = null;

    public ContentDetector(WatchManager nextReceiver) {
        super(nextReceiver);
    }

    /**
     * Notify the start of an element
     */

    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties) throws XPathException {
        flush(true);
        pendingEvent = new Event.StartElement(elemName, type, attributes, namespaces,location, properties);
    }

    /**
     * End of element
     */

    @Override
    public void endElement() throws XPathException {
        flush(false);
        nextReceiver.endElement();
    }

    /**
     * Character data
     */

    @Override
    public void characters(UnicodeString chars, Location locationId, int properties) throws XPathException {
        flush(true);
        nextReceiver.characters(chars, locationId, properties);
    }


    /**
     * Processing Instruction
     */

    @Override
    public void processingInstruction(String target, UnicodeString data, Location locationId, int properties) throws XPathException {
        flush(true);
        nextReceiver.processingInstruction(target, data, locationId, properties);
    }

    /**
     * Output a comment
     */

    @Override
    public void comment(UnicodeString chars, Location locationId, int properties) throws XPathException {
        flush(true);
        nextReceiver.comment(chars, locationId, properties);
    }

    /**
     * Output a delayed startElement() event
     * @param nonEmpty false if the content was empty, true if children were found
     * @throws XPathException if the next receiver in the pipeline reports an error
     */

    private void flush(boolean nonEmpty) throws XPathException {
        if (pendingEvent != null) {
            int properties = pendingEvent.getProperties();
            if (nonEmpty) {
                properties |= ReceiverOption.HAS_CHILDREN;
            }
            pendingEvent.replay(nextReceiver, properties);
            pendingEvent = null;
        }
    }


}

// Copyright (c) 2018-2023 Saxonica Limited