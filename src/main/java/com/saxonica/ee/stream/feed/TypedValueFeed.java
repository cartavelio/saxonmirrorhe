package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;

import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.str.UnicodeBuilder;

/**
 * This class is used when a streamed template requests evaluation of the typed value
 * of the nodes selected by a path expression.
 */


public class TypedValueFeed extends AbsorptionFeed {


    public TypedValueFeed(ItemFeed result, XPathContext context) {
        super(result, context);
    }

    @Override
    protected PendingItem makePendingItem(int sequenceNr, FleetingParentNode node) {
        return new PendingTypedValue(sequenceNr, node);
    }

    @Override
    protected Sequence processGroundedItem(Item item) throws XPathException {
        if (item instanceof AtomicValue) {
            return item;
        } else if (item instanceof NodeInfo) {
            return item.atomize();
        } else if (item instanceof ArrayItem) {
            return item.atomize();
        } else {
            dynamicError(new XPathException("Cannot atomize item of type " + Type.displayTypeName(item), "FOTY0013"));
            return null;
        }
    }


    // Following methods are to handle document and element nodes constructed by a consuming expression,
    // and then atomized: see for example sx-treat-as-108. Ideally we would integrate this case with the
    // handling of events coming directly from the source document.

    private int depth = 0;
    private StringBuilder buffer;


    @Override
    public void startDocument(int properties) throws XPathException {
        if (depth == 0) {
            buffer = new StringBuilder(256);
        }
        depth++;
    }

    @Override
    public void endDocument() throws XPathException {
        depth--;
        if (depth == 0) {
            append(new StringValue(buffer.toString()));
        }
    }

    @Override
    public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties) throws XPathException {
        if (depth == 0) {
            buffer = new StringBuilder(256);
        }
        depth++;
    }

    @Override
    public void startElement(NodeName elemName, SchemaType type, AttributeMap attributes, NamespaceMap namespaces, Location location, int properties) throws XPathException {
        startElement(elemName, type, location, properties);
    }

    @Override
    public void namespace(String prefix, String namespaceUri, int properties) throws XPathException {
        if (depth == 0) {
            characters(StringView.of(namespaceUri), Loc.NONE, properties);
        }
    }

    @Override
    public void attribute(NodeName attName, SimpleType typeCode, String value, Location location, int properties) throws XPathException {
        if (depth == 0) {
            characters(StringView.of(value), location, properties);
        }
    }

    @Override
    public void startContent()  {
        // no action
    }

    @Override
    public void endElement() throws XPathException {
        depth--;
        if (depth == 0) {
            append(new StringValue(buffer.toString()));
        }
    }

    @Override
    public void characters(UnicodeString chars, Location location, int properties) throws XPathException {
        if (depth == 0) {
            append(new StringValue(chars.tidy()));
        } else {
            buffer.append(chars);
        }
    }

    @Override
    public void processingInstruction(String name, UnicodeString data, Location location, int properties) throws XPathException {
        if (depth == 0) {
            characters(data, location, properties);
        }
    }

    @Override
    public void comment(UnicodeString content, Location location, int properties) throws XPathException {
        if (depth == 0) {
            characters(content, Loc.NONE, properties);
        }
    }



    /**
     * A pending item represents a tree under construction.
     * The sequence number represents its position relative to other trees under construction.
     * The builder is the builder being used to construct the tree.
     */

    private static class PendingTypedValue extends PendingItem {
        public UnicodeBuilder buffer;
        public FleetingParentNode node;

        public PendingTypedValue(int sequence, FleetingParentNode node) {
            super(sequence);
            buffer = new UnicodeBuilder();
            this.node = node;
        }

        @Override
        public Receiver getGatherer() {
            return new StringValueGatherer(buffer);
        }

        @Override
        public Sequence deliver() throws XPathException {
            node.setStringValue(buffer.toUnicodeString());
            return node.atomize();
        }

        @Override
        public void rollback() {
            buffer.clear();
        }
    }
}

// Copyright (c) 2009-2022 Saxonica Limited