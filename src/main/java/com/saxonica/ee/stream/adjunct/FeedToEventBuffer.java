package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.ItemFeed;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.OutputterEvent;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;

import java.util.ArrayList;
import java.util.List;

/**
 * An ItemFeed that captures all written events in a buffer for subsequent replay.
 */

public class FeedToEventBuffer extends ItemFeed {

    private List<OutputterEvent> buffer = new ArrayList<>();
    private final ItemFeed agglomerator;

    public FeedToEventBuffer(Outputter next, ItemFeed agglomerator, XPathContext context) {
        super(next, context, true);
        this.agglomerator = agglomerator;
    }

    public void setBuffer(List<OutputterEvent> buffer) {
        this.buffer = buffer;
    }

    @Override
    public void startDocument(int properties) throws XPathException {
        buffer.add(new OutputterEvent.StartDocument(properties));
    }

    @Override
    public void endDocument() throws XPathException {
        buffer.add(new OutputterEvent.EndDocument());
    }

    @Override
    public void startElement(NodeName elemName, SchemaType typeCode,
                             Location location, int properties) {
        buffer.add(new OutputterEvent.StartElement(elemName, typeCode, location, properties));
    }

    @Override
    public void startElement(NodeName elemName, SchemaType type, AttributeMap attributes, NamespaceMap namespaces, Location location, int properties) throws XPathException {
        buffer.add(new OutputterEvent.StartElement(elemName, type, location, properties));
        for (AttributeInfo att : attributes) {
            buffer.add(new OutputterEvent.Attribute(att.getNodeName(), att.getType(), att.getValue(), att.getLocation(), att.getProperties()));
        }
        for (NamespaceBinding binding : namespaces) {
            buffer.add(new OutputterEvent.Namespace(binding.getPrefix(), binding.getNamespaceUri(), properties));
        }
        buffer.add(new OutputterEvent.StartContent());
    }

    @Override
    public void attribute(NodeName name, SimpleType type, String value,
                          Location location, int properties) {
        buffer.add(new OutputterEvent.Attribute(name, type, value.toString(), location, properties));
    }

    @Override
    public void namespace(String prefix, NamespaceUri uri, int properties) {
        buffer.add(new OutputterEvent.Namespace(prefix, uri, properties));
    }

    @Override
    public void startContent() {
        buffer.add(new OutputterEvent.StartContent());
    }

    @Override
    public void endElement() throws XPathException {
        buffer.add(new OutputterEvent.EndElement());
    }

    @Override
    public void characters(UnicodeString chars, Location location, int properties) {
        buffer.add(new OutputterEvent.Text(chars, location, properties));
    }

    @Override
    public void processingInstruction(String name, UnicodeString data, Location location, int properties) {
        buffer.add(new OutputterEvent.ProcessingInstruction(name, data, location, properties));
    }

    @Override
    public void comment(UnicodeString content, Location location, int properties) {
        buffer.add(new OutputterEvent.Comment(content, location, properties));
    }

    /**
     * Supply one item towards the streamed input of the expression
     *
     * @param item the item to be supplied
     */
    @Override
    public void append(Item item) throws XPathException {
        buffer.add(new OutputterEvent.Append(item, Loc.NONE, ReceiverOption.NONE));
    }

    @Override
    public void close() throws XPathException {
        // no action
    }

    /**
     * Replay the captured events to a supplied destination
     *
     * @param out the destination {@code Receiver} to receive the events
     * @throws XPathException if any error occurs
     */

    public void replay(Outputter out) throws XPathException {
        if (!hasFailed()) {
            for (OutputterEvent event : buffer) {
                event.replay(out);
            }
        }
    }

    /**
     * Mark this feed as having failed (that is, as having detected and reported
     * a dynamic error). Once the feed is marked as having failed, it should not
     * attempt to process any further input.
     */
    @Override
    public void setHasFailed() {
        super.setHasFailed();
        agglomerator.setHasFailed();
    }
}

// Copyright (c) 2018-2023 Saxonica Limited