package com.saxonica.ee.schema.sdoc;

import net.sf.saxon.om.AxisInfo;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.TreeInfo;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.type.Type;

/**
 * An XML document containing a module of an XML Schema. This object is a simple wrapper around the document
 * node of the tree representing the parsed XML schema document, and is used to provide type-safety and a level
 * of abstraction for the interfaces that use it.
 */
public class SchemaDocument {

    private final TreeInfo document;

    /**
     * Create a SchemaDocument from a given document. This document must have been built using
     * the SchemaNodeFactory
     *
     * @param doc the document containing the xs:schema element
     */

    public SchemaDocument(TreeInfo doc) {
        document = doc;
    }

    /**
     * Get the document wrapped by this SchemaDocument
     *
     * @return the document
     */

    public TreeInfo getDocument() {
        return document;
    }

    /**
     * Ask if this is a source schema document
     * @return true if this schema document contains source XSD declarations
     */

    public boolean isSourceSchemaDocument() {
        return getXSDSchema() != null;
    }

    /**
     * Ask if this is a compiled (SCM) schema document
     *
     * @return true if this schema document contains compiled SCM declarations
     */

    public boolean isCompiledSchemaDocument() {
        AxisIterator iter = document.getRootNode().iterateAxis(AxisInfo.CHILD, NodeKindTest.makeNodeKindTest(Type.ELEMENT));
        NodeInfo node = iter.next();
        return node.getNamespaceUri().equals(NamespaceUri.SCM);
    }

    /**
     * Get the xs:schema element within the schema document
     *
     * @return the XSDSchema object representing the xs:schema element, or null if the SchemaDocument
     * does not contain an xs:schema element (for example if it contains a compiled schema)
     */

    public XSDSchema getXSDSchema() {
        AxisIterator iter = document.getRootNode().iterateAxis(AxisInfo.CHILD, NodeKindTest.makeNodeKindTest(Type.ELEMENT));
        NodeInfo node = iter.next();
        return node instanceof XSDSchema ? (XSDSchema)node : null;
    }

    /**
     * Get the target namespace of the schema document
     *
     * @return the target namespace in the xs:schema element of the schema document
     */

    public NamespaceUri getTargetNamespace() {
        XSDSchema root = getXSDSchema();
        return root == null ? null : root.getTargetNamespace();
    }

}

// Copyright (c) 2004-2023 Saxonica Limited
