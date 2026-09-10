package com.saxonica.xqj.pull;

import net.sf.saxon.expr.parser.ExplicitLocation;
import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.om.NamespaceBinding;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.pull.NamespaceContextImpl;
import net.sf.saxon.pull.PullProvider;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.SingleNodeIterator;
import net.sf.saxon.tree.util.FastStringBuffer;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.Whitespace;

import javax.xml.namespace.NamespaceContext;
import javax.xml.namespace.QName;
import javax.xml.stream.Location;
import javax.xml.stream.XMLStreamConstants;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;

/**
 * This class bridges PullProvider events to XMLStreamReader (Stax) events. That is, it acts
 * as an XMLStreamReader, fetching the underlying data from a PullProvider.
 * <p>A PullProvider may provide access to any XDM sequence, whereas an XMLStreamReader always
 * reads a document. The conversion of a sequence to a document follows the rules for
 * "normalizing" a sequence in the Serialization specification: for example, atomic values are
 * converted into text nodes, with adjacent atomic values being space-separated.</p>
 */
public class PullToStax implements XMLStreamReader {

    private PullNamespaceReducer provider;
    private boolean previousAtomic;
    private FastStringBuffer currentTextNode = new FastStringBuffer(FastStringBuffer.C64);
    private int currentStaxEvent = XMLStreamConstants.START_DOCUMENT;
    private XPathException pendingException = null;

    /**
     * Create an XMLStreamReader that reads the contents of a tree rooted at a document or element
     * node
     * @param node the node to be read. This must be a document or element node
     * @throws IllegalArgumentException if the node is not a document or element node
     */

    public static XMLStreamReader getXMLStreamReader(NodeInfo node) {
        int kind = node.getNodeKind();
        if (!(kind == Type.DOCUMENT || kind == Type.ELEMENT)) {
            throw new IllegalArgumentException("Node must be a document or element node");
        }
        PullFromIterator pull = new PullFromIterator(SingleNodeIterator.makeIterator(node));
        pull.setPipelineConfiguration(node.getConfiguration().makePipelineConfiguration());
        return new PullToStax(pull);
    }

    /**
     * Create a PullToStax instance, which wraps a Saxon PullProvider as a Stax XMLStreamReader
     *
     * @param provider the Saxon PullProvider from which the events will be read
     */

    public PullToStax(PullProvider provider) {
        if (provider instanceof PullNamespaceReducer) {
            this.provider = (PullNamespaceReducer) provider;
        } else {
            this.provider = new PullNamespaceReducer(provider);
        }
    }

    public int getAttributeCount() {
        return provider.getAttributes().getLength();
    }

    public boolean isAttributeSpecified(int i) {
        return true;
    }

    public QName getAttributeName(int i) {
        AttributeCollection atts = provider.getAttributes();
        return new QName(atts.getURI(i), atts.getLocalName(i), atts.getPrefix(i));
    }

    public String getAttributeLocalName(int i) {
        return provider.getAttributes().getLocalName(i);
    }

    public String getAttributeNamespace(int i) {
        return provider.getAttributes().getURI(i);
    }

    public String getAttributePrefix(int i) {
        return provider.getAttributes().getPrefix(i);
    }

    public String getAttributeType(int i) {
        AttributeCollection ac = provider.getAttributes();
        if (ac.isId(i)) {
            return "ID";
        } else if (ac.isIdref(i)) {
            return "IDREFS";
        } else {
            return "CDATA";
        }
    }

    public String getAttributeValue(int i) {
        return provider.getAttributes().getValue(i);
    }

    public String getAttributeValue(String uri, String local) {
        return provider.getAttributes().getValue(uri, local);
    }

    public int getEventType() {
        return currentStaxEvent;
    }

    public int getNamespaceCount() {
        NamespaceBinding[] bindings = provider.getNamespaceDeclarations();
        int count = bindings.length;
        for (int i=0; i<count; i++) {
            if (bindings[i] == null) {
                return i;
            }
        }
        return count;
    }

    public String getText() {
        if (previousAtomic) {
            return currentTextNode.toString();
        } else {
            try {
                return provider.getStringValue().toString();
            } catch (XPathException e) {
                pendingException = e;
                return "";
            }
        }
    }

    public int getTextLength() {
        if (previousAtomic) {
            return currentTextNode.length();
        } else {
            try {
                return provider.getStringValue().length();
            } catch (XPathException e) {
                pendingException = e;
                return 0;
            }
        }
    }

    public int getTextStart() {
        return 0;
    }

    public char[] getTextCharacters() {
        if (previousAtomic) {
            return currentTextNode.toCharArray();
        } else {
            try {
                String stringValue = provider.getStringValue().toString();
                char[] chars = new char[stringValue.length()];
                stringValue.getChars(0, chars.length, chars, 0);
                return chars;
            } catch (XPathException e) {
                pendingException = e;
                return new char[0];
            }
        }
    }


    public int getTextCharacters(int sourceStart, char[] target, int targetStart, int length) throws XMLStreamException {
        if (pendingException != null) {
            throw new XMLStreamException(pendingException);
        }
        if (previousAtomic) {
            int end = sourceStart + length;
            if (end > currentTextNode.length()) {
                end = currentTextNode.length();
            }
            currentTextNode.getChars(sourceStart, end, target, targetStart);
            return end - sourceStart;
        } else {
            try {
                String stringValue = provider.getStringValue().subSequence(sourceStart, sourceStart + length).toString();
                stringValue.getChars(0, length, target, targetStart);
                return length;
            } catch (XPathException e) {
                pendingException = e;
                return 0;
            }
        }
    }

    public int next() throws XMLStreamException {
        if (pendingException != null) {
            throw new XMLStreamException(pendingException);
        }
        if (currentStaxEvent == END_DOCUMENT) {
            throw new IllegalStateException("next() called after END_DOCUMENT");
            // Javadoc is unclear. See Saxon bug 3806, resulting in JDK bug JDK-8204329
        }
        int p;
        try {
            p = provider.next();
        } catch (XPathException e) {
            throw new XMLStreamException(e);
        }
        switch (p) {
            case PullProvider.START_OF_INPUT:
                return next();
            case PullProvider.ATOMIC_VALUE:
                currentTextNode.setLength(0);
                if (previousAtomic) {
                    currentTextNode.append(' ');
                }
                currentTextNode.append(provider.getAtomicValue().getStringValue());
                currentStaxEvent = XMLStreamConstants.CHARACTERS;
                break;
            case PullProvider.START_DOCUMENT:
                // STAX doesn't actually report START_DOCUMENT: it's the initial state before reading any events
                currentStaxEvent = XMLStreamConstants.START_DOCUMENT;
                return next();
            case PullProvider.END_DOCUMENT:
                currentStaxEvent = XMLStreamConstants.END_DOCUMENT;
                break;
            case PullProvider.START_ELEMENT:
                currentStaxEvent = XMLStreamConstants.START_ELEMENT;
                break;
            case PullProvider.END_ELEMENT:
                currentStaxEvent = XMLStreamConstants.END_ELEMENT;
                break;
            case PullProvider.TEXT:
                currentStaxEvent = XMLStreamConstants.CHARACTERS;
                break;
            case PullProvider.COMMENT:
                currentStaxEvent = XMLStreamConstants.COMMENT;
                break;
            case PullProvider.PROCESSING_INSTRUCTION:
                currentStaxEvent = XMLStreamConstants.PROCESSING_INSTRUCTION;
                break;
            case PullProvider.END_OF_INPUT:
                currentStaxEvent = XMLStreamConstants.END_DOCUMENT;
                break;
            case PullProvider.ATTRIBUTE:
                throw new XMLStreamException("Free-standing attributes cannot be serialized");
            case PullProvider.NAMESPACE:
                throw new XMLStreamException("Free-standing namespace nodes cannot be serialized");
            default:
                throw new IllegalStateException("Unknown Stax event " + p);

        }
        previousAtomic = p == PullProvider.ATOMIC_VALUE;
        return currentStaxEvent;
    }

    public int nextTag() throws XMLStreamException {
        if (pendingException != null) {
            throw new XMLStreamException(pendingException);
        }
        int eventType = next();
        while ((eventType == XMLStreamConstants.CHARACTERS && isWhiteSpace()) // skip whitespace
                || (eventType == XMLStreamConstants.CDATA && isWhiteSpace())
                // skip whitespace
                || eventType == XMLStreamConstants.SPACE
                || eventType == XMLStreamConstants.PROCESSING_INSTRUCTION
                || eventType == XMLStreamConstants.COMMENT
                ) {
            eventType = next();
        }
        if (eventType != XMLStreamConstants.START_ELEMENT && eventType != XMLStreamConstants.END_ELEMENT) {
            throw new XMLStreamException("expected start or end tag", getLocation());
        }
        return eventType;
    }

    public void close() throws XMLStreamException {
        if (pendingException != null) {
            throw new XMLStreamException(pendingException);
        }
        provider.close();
    }

    public boolean hasName() {
        return currentStaxEvent == XMLStreamConstants.START_ELEMENT ||
                currentStaxEvent == XMLStreamConstants.END_ELEMENT;
    }

    public boolean hasNext() throws XMLStreamException {
        if (pendingException != null) {
            throw new XMLStreamException(pendingException);
        }
        return currentStaxEvent != XMLStreamConstants.END_DOCUMENT;
    }

    public boolean hasText() {
        return currentStaxEvent == XMLStreamConstants.CHARACTERS || currentStaxEvent == XMLStreamConstants.COMMENT;
    }

    public boolean isCharacters() {
        return currentStaxEvent == XMLStreamConstants.CHARACTERS;
    }

    public boolean isEndElement() {
        return currentStaxEvent == XMLStreamConstants.END_ELEMENT;
    }

    public boolean isStandalone() {
        return false;
    }

    public boolean isStartElement() {
        return currentStaxEvent == XMLStreamConstants.START_ELEMENT;
    }

    public boolean isWhiteSpace() {
        try {
            return currentStaxEvent == XMLStreamConstants.CHARACTERS && Whitespace.isWhite(provider.getStringValue());
        } catch (XPathException e) {
            pendingException = e;
            return false;
        }
    }

    public boolean standaloneSet() {
        return false;
    }


    public String getCharacterEncodingScheme() {
        return null;
    }

    public String getElementText() throws XMLStreamException {
        if (pendingException != null) {
            throw new XMLStreamException(pendingException);
        }
        if (getEventType() != XMLStreamConstants.START_ELEMENT) {
            throw new XMLStreamException("parser must be on START_ELEMENT to read next text", getLocation());
        }
        int eventType = next();
        StringBuilder content = new StringBuilder();
        while (eventType != XMLStreamConstants.END_ELEMENT) {
            if (eventType == XMLStreamConstants.CHARACTERS
                    || eventType == XMLStreamConstants.CDATA
                    || eventType == XMLStreamConstants.SPACE
                    || eventType == XMLStreamConstants.ENTITY_REFERENCE) {
                content.append(getText());
            } else if (eventType == XMLStreamConstants.PROCESSING_INSTRUCTION
                    || eventType == XMLStreamConstants.COMMENT) {
                // skipping
            } else if (eventType == XMLStreamConstants.END_DOCUMENT) {
                throw new XMLStreamException("unexpected end of document when reading element text content", getLocation());
            } else if (eventType == XMLStreamConstants.START_ELEMENT) {
                throw new XMLStreamException("element text content may not contain START_ELEMENT", getLocation());
            } else {
                throw new XMLStreamException("Unexpected event type " + eventType, getLocation());
            }
            eventType = next();
        }
        return content.toString();
    }

    public String getEncoding() {
        return null;
    }

    public String getLocalName() {
        return provider.getNodeName().getLocalPart();
    }

    public String getNamespaceURI() {
        return provider.getNodeName().getURI();
    }

    public String getPIData() {
        if (currentStaxEvent != XMLStreamConstants.PROCESSING_INSTRUCTION) {
            throw new IllegalStateException("Not positioned at a processing instruction");
        }
        try {
            return provider.getStringValue().toString();
        } catch (XPathException e) {
            pendingException = e;
            return "";
        }
    }

    public String getPITarget() {
        if (currentStaxEvent != XMLStreamConstants.PROCESSING_INSTRUCTION) {
            throw new IllegalStateException("Not positioned at a processing instruction");
        }
        return provider.getNodeName().getLocalPart();
    }

    public String getPrefix() {
        return provider.getNodeName().getPrefix();
    }


    public String getVersion() {
        return "1.0";
    }


    public String getNamespacePrefix(int i) {
        return provider.getNamespaceDeclarations()[i].getPrefix();

    }

    public String getNamespaceURI(int i) {
        return provider.getNamespaceDeclarations()[i].getURI();
    }

    public NamespaceContext getNamespaceContext() {
        return new NamespaceContextImpl(provider);
    }

    public QName getName() {
        NodeName nn = provider.getNodeName();
        return new QName(nn.getURI(), nn.getLocalPart(), nn.getPrefix());
    }

    public Location getLocation() {
        net.sf.saxon.expr.parser.Location sourceLocator = provider.getSourceLocator();
        if (sourceLocator == null) {
            sourceLocator = ExplicitLocation.UNKNOWN_LOCATION;
        }
        return new SourceStreamLocation(sourceLocator);
    }

    public Object getProperty(String s) throws IllegalArgumentException {
        throw new IllegalArgumentException("Unknown property " + s);
    }

    public void require(int event, String uri, String local) throws XMLStreamException {
        if (pendingException != null) {
            throw new XMLStreamException(pendingException);
        }
        if (currentStaxEvent != event) {
            throw new XMLStreamException("Required event type is " + event + ", actual event is " + currentStaxEvent);
        }
        if (uri != null && !uri.equals(getNamespaceURI())) {
            throw new XMLStreamException("Required namespace is " + uri + ", actual is " + getNamespaceURI());
        }
        if (local != null && !local.equals(getLocalName())) {
            throw new XMLStreamException("Required local name is " + local + ", actual is " + getLocalName());
        }
    }

    /*@Nullable*/
    public String getNamespaceURI(String s) {
        return provider.getURIForPrefix(s, true);
    }

    /**
     * Bridge a SAX SourceLocator to a javax.xml.stream.Location
     */

    public static class SourceStreamLocation implements javax.xml.stream.Location {

        private net.sf.saxon.expr.parser.Location locator;

        /**
         * Create a StAX SourceStreamLocation object based on a given SAX SourceLocator
         *
         * @param locator the SAX SourceLocator
         */
        public SourceStreamLocation(net.sf.saxon.expr.parser.Location locator) {
            this.locator = locator;
        }

        public int getCharacterOffset() {
            return -1;
        }

        public int getColumnNumber() {
            return locator.getColumnNumber();
        }

        public int getLineNumber() {
            return locator.getLineNumber();
        }

        public String getPublicId() {
            return locator.getPublicId();
        }

        public String getSystemId() {
            return locator.getSystemId();
        }
    }

}

// Copyright (c) 2009-2018 Saxonica Limited.
