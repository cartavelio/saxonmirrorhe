package com.saxonica.ee.stream.feed;

import net.sf.saxon.event.Builder;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.tiny.TinyBuilder;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.Type;

/**
 * An GroundedItemFeed is an ItemFeed in which decomposed document and element nodes
 * (that is, start|end Document|Element events) are not accepted, and parentless leaf nodes supplied
 * using methods such as {@link #characters} or {@link Outputter#attribute(NodeName, SimpleType, String, Location, int)}
 * are converted into calls on {@link #append(Item)} supplying an {@link net.sf.saxon.tree.util.Orphan}
 * item.
 */

public abstract class GroundedItemFeed extends ItemFeed {

    private int level;
    private Builder builder;
    private Outputter buildingOutputter;

    public GroundedItemFeed(ItemFeed result, XPathContext context) {
        super(result, context);
    }

    public GroundedItemFeed(Expression exp, ItemFeed result, XPathContext context) {
        super(exp, result, context);
    }
    

    private void makeBuilder() throws XPathException {
        builder = new TinyBuilder(getPipelineConfiguration());
        buildingOutputter = new ComplexContentOutputter(builder);
        buildingOutputter.open();
    }

    @Override
    public void startDocument(int properties) throws XPathException {
        if (buildingOutputter == null) {
            level = 0;
            makeBuilder();
        } else {
            level++;
        }
        buildingOutputter.startDocument(0);
    }

    @Override
    public void endDocument() throws XPathException {
        level--;
        buildingOutputter.endDocument();
        if (level == 0) {
            buildingOutputter.close();
            NodeInfo node = builder.getCurrentRoot();
            append(node);
            builder = null;
            buildingOutputter = null;
        }
    }

    @Override
    public void setUnparsedEntity(String name, String systemID, String publicID) throws XPathException {
        if (buildingOutputter != null) {
            buildingOutputter.setUnparsedEntity(name, systemID, publicID);
        }
    }

    @Override
    public void startElement(NodeName elemName, SchemaType type, AttributeMap attributes, NamespaceMap namespaces, Location location, int properties) throws XPathException {
        if (builder == null) {
            level = 0;
            makeBuilder();
        }
        level++;
        buildingOutputter.startElement(elemName, type, attributes, namespaces, location, properties);
    }

    @Override
    public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties) throws XPathException {
        if (builder == null) {
            level = 0;
            makeBuilder();
        }
        level++;
        buildingOutputter.startElement(elemName, typeCode, location, properties);
    }

    @Override
    public void namespace(String prefix, NamespaceUri namespaceUri, int properties) throws XPathException {
        if (buildingOutputter == null) {
            Orphan orphan = new Orphan(getConfiguration());
            orphan.setNodeKind(Type.NAMESPACE);
            orphan.setNodeName(new NoNamespaceName(prefix));
            orphan.setStringValue(namespaceUri.toUnicodeString());
            append(orphan);
        } else {
            buildingOutputter.namespace(prefix, namespaceUri, properties);
        }
    }

    @Override
    public void attribute(NodeName attName, SimpleType typeCode, String value, Location location, int properties) throws XPathException {
        if (buildingOutputter == null) {
            Orphan orphan = new Orphan(getConfiguration());
            orphan.setNodeKind(Type.ATTRIBUTE);
            orphan.setTypeAnnotation(typeCode);
            orphan.setNodeName(attName);
            orphan.setStringValue(StringView.tidy(value));
            append(orphan);
        } else {
            buildingOutputter.attribute(attName, typeCode, value, location, properties);
        }
    }

    @Override
    public void startContent() throws XPathException {
        if (buildingOutputter != null) {
            buildingOutputter.startContent();
        }
    }

    @Override
    public void endElement() throws XPathException {
        level--;
        if (buildingOutputter != null) {
            buildingOutputter.endElement();
            if (level == 0) {
                buildingOutputter.close();
                NodeInfo node = builder.getCurrentRoot();
                append(node);
                builder = null;
                buildingOutputter = null;
            }
        }

    }

    @Override
    public void characters(UnicodeString chars, Location location, int properties) throws XPathException {
        if (buildingOutputter == null) {
            Orphan orphan = new Orphan(getConfiguration());
            orphan.setNodeKind(Type.TEXT);
            orphan.setStringValue(chars.tidy());
            append(orphan);
        } else {
            buildingOutputter.characters(chars, location, properties);
        }
    }

    @Override
    public void processingInstruction(String name, UnicodeString data, Location location, int properties) throws XPathException {
        if (buildingOutputter == null) {
            Orphan orphan = new Orphan(getConfiguration());
            orphan.setNodeKind(Type.PROCESSING_INSTRUCTION);
            orphan.setNodeName(new NoNamespaceName(name));
            orphan.setStringValue(data.tidy());
            append(orphan);
        } else {
            buildingOutputter.processingInstruction(name, data, location, properties);
        }
    }

    @Override
    public void comment(UnicodeString content, Location location, int properties) throws XPathException {
        if (buildingOutputter == null) {
            Orphan orphan = new Orphan(getConfiguration());
            orphan.setNodeKind(Type.COMMENT);
            orphan.setStringValue(content.tidy());
            append(orphan);
        } else {
            buildingOutputter.comment(content, location, properties);
        }
    }



}

// Copyright (c) 2018-2022 Saxonica Limited
