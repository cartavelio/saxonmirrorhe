package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.GroundedItemFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.InstanceOfExpression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.Cardinality;

/**
 * Generate streaming code for an "instance of" expression
 */
public class InstanceOfAdjunct extends InspectionAdjunct implements FeedMaker {


    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new InstanceOfFeed(watchManager, getExpression(), out, context);
    }

    private static class InstanceOfFeed extends GroundedItemFeed {

        WatchManager watchManager;
        ItemType itemType;
        int level;
        int count;
        int cardinality;
        boolean matchesSoFar;
        boolean closed;


        InstanceOfFeed(WatchManager watchManager, Expression exp, ItemFeed result, XPathContext context) {
            super(exp, result, context);
            this.watchManager = watchManager;
            itemType = ((InstanceOfExpression)exp).getRequiredItemType();
            cardinality = ((InstanceOfExpression)exp).getRequiredCardinality();
            matchesSoFar = true;
        }

        @Override
        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            count = 0;
            level = 0;
            matchesSoFar = true;
        }

        @Override
        public void append(Item item) throws XPathException {
            if (level == 0 && matchesSoFar) {
                if (!itemType.matches(item, getContext().getConfiguration().getTypeHierarchy())) {
                    matchesSoFar = false;
                }
                count++;
                if (count == 2 && !Cardinality.allowsMany(cardinality)) {
                    matchesSoFar = false;
                }
                if (!matchesSoFar) {
                    getNextOutputter().append(BooleanValue.FALSE);
                    if (watchManager.allowsEarlyExit()) {
                        closed = true;
                        getNextOutputter().close();
                        getTerminator().terminate();
                    }
                }
            }
        }

        @Override
        public void close() throws XPathException {
            if (matchesSoFar) {
                if (count == 0 && !Cardinality.allowsZero(cardinality)) {
                    matchesSoFar = false;
                }
                getNextOutputter().append(BooleanValue.get(matchesSoFar));
            }
            if (!closed) {
                getNextOutputter().close();
            }
        }

        @Override
        public void startDocument(int properties) throws XPathException {
            if (level == 0) {
                Orphan orphan = new Orphan(getConfiguration());
                orphan.setNodeKind(Type.DOCUMENT);
                append(orphan);
            }
            level++;
        }

        @Override
        public void endDocument() throws XPathException {
            level--;
        }

        @Override
        public void startElement(NodeName elemName, SchemaType type, Location location, int properties) throws XPathException {
            if (level == 0 && matchesSoFar) {
                Orphan orphan = new Orphan(getConfiguration());
                orphan.setNodeKind(Type.ELEMENT);
                orphan.setNodeName(elemName);
                orphan.setTypeAnnotation(type);
                orphan.setStringValue(EmptyUnicodeString.getInstance());
                append(orphan);
            }
            level++;
        }

        @Override
        public void namespace(String prefix, String namespaceUri, int properties) throws XPathException {
            if (level == 0) {
                super.namespace(prefix, namespaceUri, properties);
            }
        }

        @Override
        public void attribute(NodeName attName, SimpleType typeCode, String value, Location location, int properties) throws XPathException {
            if (level == 0) {
                super.attribute(attName, typeCode, value, location, properties);
            }
        }

        @Override
        public void endElement() throws XPathException {
            level--;
        }

        @Override
        public void characters(UnicodeString chars, Location location, int properties) throws XPathException {
            if (level == 0) {
                super.characters(chars, location, properties);
            }
        }

        @Override
        public void processingInstruction(String name, UnicodeString data, Location location, int properties) throws XPathException {
            if (level == 0) {
                super.processingInstruction(name, data, location, properties);
            }
        }

        @Override
        public void comment(UnicodeString content, Location location, int properties) throws XPathException {
            if (level == 0) {
                super.comment(content, location, properties);
            }
        }

        @Override
        public void startElement(NodeName elemName, SchemaType type, AttributeMap attributes, NamespaceMap namespaces, Location location, int properties) throws XPathException {
            startElement(elemName, type, location, properties);
        }
    }
}

// Copyright (c) 2018-2022 Saxonica Limited