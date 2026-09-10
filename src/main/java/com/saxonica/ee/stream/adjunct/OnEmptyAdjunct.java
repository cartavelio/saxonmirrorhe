package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;


/**
 * Generate streaming code for a consuming xsl:on-empty instruction
 */
public class OnEmptyAdjunct extends TransmissionAdjunct implements FeedMaker {

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new OnEmptyFeed(getExpression(), (ConditionalBlockAdjunct.ConditionalBlockFeed)out, context);
    }

    private static class OnEmptyFeed extends ItemFeed {

        boolean isEmpty;

        OnEmptyFeed(Expression expr, ConditionalBlockAdjunct.ConditionalBlockFeed next, XPathContext context) {
            super(expr, next, context);
        }

        /**
         * Start evaluating the expression. The default implementation does nothing.
         *
         * @param terminator used to achieve early exit
         * @throws XPathException if a dynamic error occurs
         */
        @Override
        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            isEmpty = ((ConditionalBlockAdjunct.ConditionalBlockFeed)getNextOutputter()).isEmpty();
        }

        @Override
        public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
            if (isEmpty) {
                return super.startSelectedParentNode(node, locationId);
            } else {
                return null;
            }
        }

        @Override
        public void append(Item item) throws XPathException {
            if (isEmpty) {
                getNextOutputter().append(item);
            }
        }

        @Override
        public void endSelectedParentNode(Location locationId) throws XPathException {
            if (isEmpty) {
                super.endSelectedParentNode(locationId);
            }
        }

        @Override
        public void close() throws XPathException {
            super.close();
        }

        @Override
        public void startDocument(int properties) throws XPathException {
            if (isEmpty) {
                super.startDocument(properties);
            }
        }

        @Override
        public void endDocument() throws XPathException {
            if (isEmpty) {
                super.endDocument();
            }
        }

        @Override
        public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties) throws XPathException {
            if (isEmpty) {
                super.startElement(elemName, typeCode, location, properties);
            }
        }

        @Override
        public void startElement(NodeName elemName, SchemaType type, AttributeMap attributes, NamespaceMap namespaces, Location location, int properties) throws XPathException {
            if (isEmpty) {
                super.startElement(elemName, type, attributes, namespaces, location, properties);
            }
        }

        @Override
        public void namespace(String prefix, String namespaceUri, int properties) throws XPathException {
            if (isEmpty) {
                super.namespace(prefix, namespaceUri, properties);
            }
        }

        @Override
        public void attribute(NodeName attName, SimpleType typeCode, String value, Location location, int properties) throws XPathException {
            if (isEmpty) {
                super.attribute(attName, typeCode, value, location, properties);
            }
        }

        @Override
        public void startContent() throws XPathException {
            if (isEmpty) {
                super.startContent();
            }
        }

        @Override
        public void endElement() throws XPathException {
            if (isEmpty) {
                super.endElement();
            }
        }

        @Override
        public void characters(UnicodeString chars, Location location, int properties) throws XPathException {
            if (isEmpty) {
                super.characters(chars, location, properties);
            }
        }

        @Override
        public void processingInstruction(String name, UnicodeString data, Location location, int properties) throws XPathException {
            if (isEmpty) {
                super.processingInstruction(name, data, location, properties);
            }
        }

        @Override
        public void comment(UnicodeString content, Location location, int properties) throws XPathException {
            if (isEmpty) {
                super.comment(content, location, properties);
            }
        }
    }

}

// Copyright (c) 2018-2022 Saxonica Limited