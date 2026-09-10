package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.SequenceReceiver;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.AttributeCollectionImpl;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.AtomicValue;

import java.util.ArrayList;
import java.util.List;


/**
 * Generate streaming code for an xsl:where-populated instruction.
 */
public class WherePopulatedAdjunct extends TransmissionAdjunct {

    public Feed makeItemFeed(WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
        return new WherePopulatedFeed(out, context);
    }

    private static class WherePopulatedFeed extends ItemFeed {

        private WherePopulatedReceiver receiver;

        public WherePopulatedFeed(Feed out, XPathContext context) {
            super(out, context);
        }

        /**
         * Supply one item towards the streamed input of the expression
         *
         * @param item the item to be supplied
         */
        @Override
        public void processItem(Item<?> item) throws XPathException {
            if (!isEmpty(item)) {
                if (receiver != null) {
                    receiver.releaseStartTag();
                }
                getResult().processItem(item);
            }
        }

        private boolean isEmpty(Item item) {
            if (item instanceof NodeInfo) {
                switch (((NodeInfo)item).getNodeKind()) {
                    case Type.DOCUMENT:
                    case Type.ELEMENT:
                        return !((NodeInfo)item).hasChildNodes();
                    default:
                        return item.getStringValueCS().length() == 0;
                }
            } else if (item instanceof AtomicValue) {
                return item.getStringValueCS().length() == 0;
            } else if (item instanceof MapItem) {
                return ((MapItem) item).isEmpty();
            } else if (item instanceof ArrayItem) {
                return ((ArrayItem) item).isEmpty();
            } else {
                return false;
            }
        }

        /**
         * Get a receiver to which start and end events for documents and elements can be
         * written. By default this will be the same as the receiver for the result feed,
         * because very few XSLT constructs (specifically, variables and functions) allow
         * the results of document and element constructors to be captured for further
         * processing. A construct that does need to process these events will intercept
         * them by providing its own receiver.
         */
        @Override
        public SequenceReceiver getReceiver() {
            if (receiver == null) {
                receiver = new WherePopulatedReceiver(getResult().getReceiver());
            }
            return receiver;
        }
    }


    private static class WherePopulatedReceiver extends ProxyReceiver {

        private int level = 0;
        private boolean pendingStartTag = false;
        private NodeName pendingElemName;
        private SchemaType pendingSchemaType;
        private Location pendingLocationId;
        private int pendingProperties;
        private AttributeCollectionImpl pendingAttributes;
        private List<NamespaceBindingSet> pendingNamespaceSets;

        public WherePopulatedReceiver(Receiver next) {
            super(next);
        }

        /**
         * Start of a document node.
         *
         * @param properties
         */
        @Override
        public void startDocument(int properties) throws XPathException {
            if (level++ == 0) {
                pendingStartTag = true;
                pendingElemName = null;
                pendingProperties = properties;
            } else {
                super.startDocument(properties);
            }
        }

        /**
         * Notify the start of an element
         * @param elemName   integer code identifying the name of the element within the name pool.
         * @param typeCode   integer code identifying the element's type within the name pool.
         * @param location   location of the start tag
         * @param properties properties of the element node
         */
        @Override
        public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties) throws XPathException {
            releaseStartTag();
            if (level++ == 0) {
                pendingStartTag = true;
                pendingElemName = elemName;
                pendingSchemaType = typeCode;
                pendingLocationId = location.saveLocation();
                pendingProperties = properties;
                if (pendingAttributes == null) {
                    pendingAttributes = new AttributeCollectionImpl(getPipelineConfiguration().getConfiguration());
                } else {
                    pendingAttributes.clear();
                }
                if (pendingNamespaceSets == null) {
                    pendingNamespaceSets = new ArrayList<NamespaceBindingSet>(4);
                } else {
                    pendingNamespaceSets.clear();
                }

            } else {
                super.startElement(elemName, typeCode, location, properties);
            }
        }

        @Override
        public void attribute(NodeName nameCode, SimpleType typeCode, CharSequence value, Location locationId, int properties) throws XPathException {
            if (level == 1 && pendingStartTag) {
                pendingAttributes.addAttribute(nameCode, typeCode, value.toString(), locationId, properties);
            } else {
                super.attribute(nameCode, typeCode, value, locationId, properties);
            }
        }

        @Override
        public void namespace(NamespaceBindingSet namespaceBindings, int properties) throws XPathException {
            if (level == 1 && pendingStartTag) {
                pendingNamespaceSets.add(namespaceBindings);
            } else {
                super.namespace(namespaceBindings, properties);
            }
        }

        @Override
        public void startContent() throws XPathException {
            if (level == 1 && pendingStartTag) {
                // no action
            } else {
                super.startContent();
            }
        }

        /**
         * Notify the end of a document node
         */
        @Override
        public void endDocument() throws XPathException {
            if (--level == 0) {
                if (!pendingStartTag) {
                    super.endDocument();
                }
            } else {
                super.endDocument();
            }
            pendingStartTag = false;
        }

        /**
         * End of element
         */
        @Override
        public void endElement() throws XPathException {
            if (--level == 0) {
                if (!pendingStartTag) {
                    super.endElement();
                }
            } else {
                super.endElement();
            }
            pendingStartTag = false;
        }

        public void releaseStartTag() throws XPathException {
            if (level==1 && pendingStartTag) {
                if (pendingElemName == null) {
                    getNextReceiver().startDocument(pendingProperties);
                } else {
                    getNextReceiver().startElement(pendingElemName, pendingSchemaType, pendingLocationId, pendingProperties);
                    for (NamespaceBindingSet ns : pendingNamespaceSets) {
                        getNextReceiver().namespace(ns, 0);
                    }
                    for (int i=0; i<pendingAttributes.getLength(); i++) {
                        getNextReceiver().attribute(
                            pendingAttributes.getNodeName(i),
                            pendingAttributes.getTypeAnnotation(i),
                            pendingAttributes.getValue(i),
                            pendingAttributes.getLocation(i),
                            pendingAttributes.getProperties(i));
                    }
                    getNextReceiver().startContent();
                }
                pendingStartTag = false;
            }
        }

        // Discard zero-length text nodes if level >= 1

        @Override
        public void characters(CharSequence chars, Location locationId, int properties) throws XPathException {
            if (level == 0 || chars.length() > 0) {
                releaseStartTag();
                super.characters(chars, locationId, properties);
            }
        }


        @Override
        public void append(Item item, Location locationId, int copyNamespaces) throws XPathException {
            if (level == 1 && pendingStartTag) {
                if (item instanceof NodeInfo) {
                    NodeInfo node = (NodeInfo)item;
                    if (node.getNodeKind() == Type.TEXT && node.getStringValueCS().length() == 0) {
                        return; // ignore empty text nodes
                    } else if (node.getNodeKind() == Type.ATTRIBUTE) {
                        pendingAttributes.addAttribute(NameOfNode.makeName(node), (SimpleType)node.getSchemaType(), node.getStringValue(), locationId, 0);
                        return;
                    } else if (node.getNodeKind() == Type.NAMESPACE) {
                        pendingNamespaceSets.add(new NamespaceBinding(node.getLocalPart(), node.getStringValue()));
                        return;
                    }
                }

                releaseStartTag();
            }
            super.append(item, locationId, copyNamespaces);
        }
    }


}

// Copyright (c) 2014-2015 Saxonica Limited.