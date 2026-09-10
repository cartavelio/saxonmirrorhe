package com.saxonica.ee.stream.om;

import net.sf.saxon.Configuration;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodePredicate;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.Type;

/**
 * A FleetingNode is a node that exists transiently in the course of streaming processing. The node has
 * a parent (unless it is the root), and the case of an element node it also has attributes and namespaces.
 * If it is a "remembered ancestor" then it may have descendants, but these cannot be found using the descendant
 * axis. It has no following or preceding nodes.
 */

public class FleetingNode implements NodeInfo {

    private FleetingNode parent;
    protected FleetingDocumentNode root;
    private int nodeKind;
    private NodeName nodeName;
    private SchemaType typeAnnotation;
    private UnicodeString stringValue;
    private int nodeNumber;

    public FleetingNode() {
    }

    /**
     * To implement {@link Sequence}, this method returns the item itself
     *
     * @return this item
     */

    @Override
    public NodeInfo head() {
        return this;
    }

    /**
     * Get information about the tree to which this NodeInfo belongs
     *
     * @return the TreeInfo
     * @since 9.7
     */
    @Override
    public TreeInfo getTreeInfo() {
        return root;
    }

    /**
     * Get the depth of this node in the tree.
     *
     * @return 0 for the document node, 1 for the outermost element, etc
     */

    public int getDepth() {
        return parent == null ? 0 : parent.getDepth() + 1;
    }

    /**
     * Get the serial number of this node within the containing document
     * @return the serial number of this node: this reflects document order.
     */

    public int getNodeNumber() {
        return nodeNumber;
    }

    public void setParent(FleetingNode parent) {
        this.parent = parent;
        this.root = parent.root;
        this.nodeNumber = root.allocateNodeNumber();
    }

    public void setNodeName(NodeName name) {
        this.nodeName = name;
    }

    public void setNodeKind(int nodeKind) {
        this.nodeKind = nodeKind;
    }

    public void setStringValue(UnicodeString stringValue) {
        this.stringValue = stringValue;
    }

    public void setTypeAnnotation(SchemaType typeAnnotation) {
        this.typeAnnotation = typeAnnotation;
    }

    /**
     * Get the kind of node. This will be a value such as {@link net.sf.saxon.type.Type#ELEMENT}
     * or {@link net.sf.saxon.type.Type#ATTRIBUTE}. There are seven kinds of node: documents, elements, attributes,
     * text, comments, processing-instructions, and namespaces.
     *
     * @return an integer identifying the kind of node. These integer values are the
     *         same as those used in the DOM
     * @see net.sf.saxon.type.Type
     * @since 8.4
     */

    @Override
    public final int getNodeKind() {
        return nodeKind;
    }

    /**
     * Determine whether this is the same node as another node.
     * <p>Note that two different NodeInfo instances can represent the same conceptual node.
     * Therefore the "==" operator should not be used to test node identity. The equals()
     * method should give the same result as isSameNodeInfo(), but since this rule was introduced
     * late it might not apply to all implementations.</p>
     * <p>Note: a.isSameNodeInfo(b) if and only if generateId(a)==generateId(b).</p>
     * <p>This method has the same semantics as isSameNode() in DOM Level 3, but
     * works on Saxon NodeInfo objects rather than DOM Node objects.</p>
     *
     * @param other the node to be compared with this node
     * @return true if this NodeInfo object and the supplied NodeInfo object represent
     *         the same node in the tree.
     */

    public boolean equals(Object other) {
        if (this == other) {
            return true;
        } else if (other instanceof FleetingNode) {
            FleetingNode fn = (FleetingNode) other;
            switch (getNodeKind()) {
                case Type.ATTRIBUTE:
                    return getNodeKind() == fn.getNodeKind() &&
                            getParent() == fn.getParent() &&
                            getNodeName().equals(fn.getNodeName());
                case Type.NAMESPACE:
                    return getNodeKind() == fn.getNodeKind() &&
                            getParent() == fn.getParent() &&
                            getLocalPart().equals(fn.getLocalPart());
                default:
                    return false;
            }
        } else {
            return false;
        }
    }

    @Override
    public int hashCode() {
        switch (getNodeKind()) {
            case Type.ATTRIBUTE:
            case Type.NAMESPACE:
                return getParent().hashCode() ^ getLocalPart().hashCode();
            default:
                return super.hashCode();
        }
    }

    /**
     * Get the System ID for the node. Note this is not the
     * same as the base URI: the base URI can be modified by xml:base, but
     * the system ID cannot. The base URI is used primarily for resolving
     * relative URIs within the content of the document. The system ID is
     * used primarily in conjunction with a line number, for identifying the
     * location of elements within the source XML, in particular when errors
     * are found. For a document node, the System ID represents the value of
     * the document-uri property as defined in the XDM data model.
     *
     * @return the System Identifier of the entity in the source document
     *         containing the node, or null if not known or not applicable.
     * @since 8.4
     */

    @Override
    public String getSystemId() {
        return root.getSystemId();
    }

    /**
     * Get the Base URI for the node, that is, the URI used for resolving a relative URI contained
     * in the node. This will be the same as the System ID unless xml:base has been used. Where the
     * node does not have a base URI of its own, the base URI of its parent node is returned.
     *
     * @return the base URI of the node. This may be null if the base URI is unknown.
     * @since 8.4
     */

    @Override
    public String getBaseURI() {
        return Navigator.getBaseURI(this);
    }

    /**
     * Get an immutable copy of this Location object. By default Location objects may be mutable, so they
     * should not be saved for later use. The result of this operation holds the same location information,
     * but in an immutable form.
     */
    @Override
    public Location saveLocation() {
        return this;
    }

    /**
     * Determine the relative position of this node and another node, in document order.
     * <p>The other node must always be in the same tree; the effect of calling this method
     * when the two nodes are in different trees is undefined. To obtain a global ordering
     * of nodes, the application should first compare the result of getDocumentNumber(),
     * and only if the document number is the same should compareOrder() be called.</p>
     *
     * @param other The other node, whose position is to be compared with this
     *              node
     * @return -1 if this node precedes the other node, +1 if it follows the
     *         other node, or 0 if they are the same node. (In this case,
     *         isSameNode() will always return true, and the two nodes will
     *         produce the same result for generateId())
     * @since 8.4
     */

    @Override
    public final int compareOrder(NodeInfo other) {
        if (equals(other)) {
            return 0;
        }
        switch (getNodeKind()) {
            case Type.DOCUMENT:
                return -1;
            case Type.ELEMENT:
                if (other.getNodeKind() == Type.ATTRIBUTE || other.getNodeKind() == Type.NAMESPACE) {
                    if (equals(other.getParent())) {
                        return -1;
                    } else {
                        return compareOrder(other.getParent());
                    }
                }
                return Integer.compare(getNodeNumber(), ((FleetingNode)other).getNodeNumber());
            case Type.COMMENT:
            case Type.PROCESSING_INSTRUCTION:
            case Type.TEXT:
                if (other.getNodeKind() == Type.ATTRIBUTE || other.getNodeKind() == Type.NAMESPACE) {
                    return compareOrder(other.getParent());
                }
                return Integer.compare(getNodeNumber(), ((FleetingNode) other).getNodeNumber());
            case Type.ATTRIBUTE:
                if (other.getNodeKind() == Type.ATTRIBUTE) {
                    if (getParent().equals(other.getParent())) {
                        // the relative position of attributes is determined by their position in the
                        // attribute collection of the containing element
                        return ((FleetingElementNode)getParent()).compareAttributeOrder(NameOfNode.makeName(this), NameOfNode.makeName(other));
                    } else {
                        return getParent().compareOrder(other.getParent());
                    }
                } else if (other.getNodeKind() == Type.NAMESPACE) {
                    if (getParent().equals(other.getParent())) {
                        return +1;
                    } else {
                        return getParent().compareOrder(other.getParent());
                    }
                } else if (other.equals(getParent())) {
                    return +1;
                } else {
                    return getParent().compareOrder(other);
                }
            case Type.NAMESPACE:
                if (other.getNodeKind() == Type.NAMESPACE) {
                    if (getParent().equals(other.getParent())) {
                        // the relative position of namespace nodes is determined by their position in the
                        // attribute collection of the containing element
                        int i0 = ((FleetingElementNode) getParent()).getNamespaceNodeIndex(getLocalPart());
                        int i1 = ((FleetingElementNode) getParent()).getNamespaceNodeIndex(other.getLocalPart());
                        return i0 < i1 ? -1 : +1;
                    } else {
                        return getParent().compareOrder(other.getParent());
                    }
                } else if (other.getNodeKind() == Type.ATTRIBUTE) {
                    if (getParent().equals(other.getParent())) {
                        return -1;
                    } else {
                        return getParent().compareOrder(other.getParent());
                    }
                } else if (other.equals(getParent())) {
                    return +1;
                } else {
                    return getParent().compareOrder(other);
                }

        }
        throw new IllegalStateException();
    }

    /**
     * Return the string value of the node as defined in the XPath data model.
     * <p>The interpretation of this depends on the type
     * of node. For an element it is the accumulated character content of the element,
     * including descendant elements.</p>
     * <p>This method returns the string value as if the node were untyped. Unlike the string value
     * accessor in the XPath 2.0 data model, it does not report an error if the element has a complex
     * type, instead it returns the concatenation of the descendant text nodes as it would if the element
     * were untyped.</p>
     *
     * @return the string value of the node
     * @since 8.4
     */

    @Override
    public final UnicodeString getUnicodeStringValue() {
        if (stringValue == null) {
            throw new UnsupportedOperationException("The string value of a streamed node is not available");
        }
        return stringValue;
    }

    /**
     * Ask whether this NodeInfo implementation holds a fingerprint identifying the name of the
     * node in the NamePool. If the answer is true, then the {@link #getFingerprint} method must
     * return the fingerprint of the node. If the answer is false, then the {@link #getFingerprint}
     * method should throw an {@code UnsupportedOperationException}. In the case of unnamed nodes
     * such as text nodes, the result can be either true (in which case getFingerprint() should
     * return -1) or false (in which case getFingerprint may throw an exception).
     *
     * @return true if the implementation of this node provides fingerprints.
     * @since 9.8; previously Saxon relied on using <code>FingerprintedNode</code> as a marker interface.
     */
    @Override
    public boolean hasFingerprint() {
        return true;
    }

    /**
     * Get fingerprint. The fingerprint is a coded form of the expanded name
     * of the node: two nodes
     * with the same name code have the same namespace URI and the same local name.
     * The fingerprint contains no information about the namespace prefix. For a name
     * in the null namespace, the fingerprint is the same as the name code.
     *
     * @return an integer fingerprint; two nodes with the same fingerprint have
     *         the same expanded QName. For unnamed nodes (text nodes, comments, document nodes,
     *         and namespace nodes for the default namespace), returns -1.
     * @since 8.4
     */

    @Override
    public final int getFingerprint() {
        if (nodeName == null) {
            return -1;
        } else {
            return nodeName.obtainFingerprint(getNamePool());
        }
    }

    public final NodeName getNodeName() {
        return nodeName;
    }

    /**
     * Get the local part of the name of this node. This is the name after the ":" if any.
     *
     * @return the local part of the name. For an unnamed node, returns "". Unlike the DOM
     *         interface, this returns the full name in the case of a non-namespaced name.
     * @since 8.4
     */

    @Override
    public final String getLocalPart() {
        return nodeName == null ? "" : nodeName.getLocalPart();
    }

    /**
     * Get the URI part of the name of this node. This is the URI corresponding to the
     * prefix, or the URI of the default namespace if appropriate.
     *
     * @return The URI of the namespace of this node. For an unnamed node,
     *         or for an element or attribute that is not in a namespace, or for a processing
     *         instruction, returns an empty string.
     * @since 8.4
     */

    @Override
    public final NamespaceUri getNamespaceUri() {
        return nodeName == null ? NamespaceUri.NULL : nodeName.getURI();
    }

    /**
     * Get the display name of this node, in the form of a lexical QName.
     * For elements and attributes this is [prefix:]localname.
     * For unnamed nodes, it is an empty string.
     *
     * @return The display name of this node. For a node with no name, returns
     *         an empty string.
     * @since 8.4
     */

    @Override
    public final String getDisplayName() {
        return nodeName == null ? "" : nodeName.getDisplayName();
    }

    /**
     * Get the prefix of the name of the node. This is defined only for elements and attributes.
     * If the node has no prefix, or for other kinds of node, returns a zero-length string.
     *
     * @return The prefix of the name of the node.
     * @since 8.4
     */

    @Override
    public final String getPrefix() {
        return nodeName == null ? "" : nodeName.getPrefix();
    }

    /**
     * Get the configuration used to build the tree containing this node.
     *
     * @return the Configuration
     * @since 8.4
     */

    @Override
    public Configuration getConfiguration() {
        return root.getConfiguration();
    }

    /**
     * Get the NamePool that holds the namecode for this node
     *
     * @return the namepool
     * @since 8.4
     */

    public NamePool getNamePool() {
        return root.getNamePool();
    }

    /**
     * Get the type annotation of this node, if any. The type annotation is represented as
     * SchemaType object.
     * <p>Types derived from a DTD are not reflected in the result of this method.</p>
     *
     * @return For element and attribute nodes: the type annotation derived from schema
     *         validation (defaulting to xs:untyped and xs:untypedAtomic in the absence of schema
     *         validation). For comments, text nodes, processing instructions, and namespaces: null.
     *         For document nodes, either xs:untyped if the document has not been validated, or
     *         xs:anyType if it has.
     * @since 9.4
     */
    @Override
    public final SchemaType getSchemaType() {
        return typeAnnotation;
    }

    /**
     * Get the typed value.
     *
     * @return the typed value. This will either be a single AtomicValue or a Value whose items are
     *         atomic values.
     */

    @Override
    public AtomicSequence atomize() throws XPathException {
        return typeAnnotation.atomize(this);
    }

    /**
     * Get the NodeInfo object representing the parent of this node
     *
     * @return the parent of this node; null if this node has no parent
     */

    @Override
    public final NodeInfo getParent() {
        return parent;
    }

    /**
     * Return an iteration over all the nodes reached by the given axis from this node
     * that match a given NodeTest
     *
     * @param axisNumber an integer identifying the axis; one of the constants
     *                   defined in class {@link AxisInfo}
     * @param nodeTest   A condition to be satisfied by the returned nodes; nodes
     *                   that do not satisfy this condition are not included in the result
     * @return an AxisIterator that delivers the nodes reached by the axis in
     *         turn.  The nodes are returned in axis order (document order for a forwards
     *         axis, reverse document order for a reverse axis).
     * @throws UnsupportedOperationException if the namespace axis is
     *                                       requested and this axis is not supported for this implementation.
     */

    @Override
    public AxisIterator iterateAxis(int axisNumber, NodePredicate nodeTest) {
        if (AxisInfo.isAlwaysEmpty(axisNumber, nodeKind)) {
            return EmptyIterator.ofNodes();
        }
        switch (axisNumber) {
            case AxisInfo.ANCESTOR:
                if (nodeKind == Type.DOCUMENT) {
                    return EmptyIterator.ofNodes();
                }
                return new Navigator.AxisFilter(
                        new Navigator.AncestorEnumeration(this, false),
                        nodeTest);

            case AxisInfo.ANCESTOR_OR_SELF:
                if (nodeKind == Type.DOCUMENT) {
                    return Navigator.filteredSingleton(this, nodeTest);
                }
                return new Navigator.AxisFilter(
                        new Navigator.AncestorEnumeration(this, true),
                        nodeTest);

            case AxisInfo.PARENT:
                return Navigator.filteredSingleton(getParent(), nodeTest);

            case AxisInfo.SELF:
                return Navigator.filteredSingleton(this, nodeTest);

            case AxisInfo.ATTRIBUTE:
                return EmptyIterator.ofNodes();

            case AxisInfo.NAMESPACE:
                return EmptyIterator.ofNodes();

            default:
                XPathException err = new XPathException("Navigation using " + AxisInfo.axisName[axisNumber] +
                        " axis is not supported from a streamed input node", SaxonErrorCode.SXST0061);
                //err.printStackTrace();
                throw new UnsupportedOperationException(err);
                // This error should have been detected at compile time.
        }
    }

    /**
     * Get the string value of a given attribute of this node
     *
     * @param uri   the namespace URI of the attribute name. Supply the empty string for an attribute
     *              that is in no namespace
     * @param local the local part of the attribute name.
     * @return the attribute value if it exists, or null if it does not exist. Always returns null
     *         if this node is not an element.
     * @since 9.4
     */
    @Override
    public String getAttributeValue(/*@NotNull*/ NamespaceUri uri, /*@NotNull*/ String local) {
        return null;
    }

    /**
     * Get the root node of the tree containing this node
     *
     * @return the NodeInfo representing the top-level ancestor of this node.
     *         This will not necessarily be a document node. If this node has no parent,
     *         then the method returns this node.
     * @since 8.4
     */

    @Override
    public NodeInfo getRoot() {
        return root;
    }

    /**
     * Determine whether the node has any children.
     * <p>Note: the result is equivalent to
     * <code>iterateAxis(Axis.CHILD).next() != null</code></p>
     *
     * @return True if the node has one or more children
     * @since 8.4
     */

    @Override
    public boolean hasChildNodes() {
        return false;
    }

    /**
     * Construct a character string that uniquely identifies this node.
     * Note: a.isSameNode(b) if and only if generateId(a)==generateId(b)
     *
     * @param buffer a buffer which will be updated to hold a string
     *               that uniquely identifies this node, across all documents.
     * @since 8.7
     *        <p>Changed in Saxon 8.7 to generate the ID value in a client-supplied buffer</p>
     */

    @Override
    public void generateId(StringBuilder buffer) {
        switch (getNodeKind()) {
            case Type.ATTRIBUTE:
                getParent().generateId(buffer);
                buffer.append('a').append(((FleetingElementNode) getParent()).getAttributeIndex(getNamespaceUri(), getLocalPart()));
                break;
            case Type.NAMESPACE:
                getParent().generateId(buffer);
                buffer.append('n').append(((FleetingElementNode) getParent()).getNamespaceNodeIndex(getLocalPart()));
                break;
            case Type.DOCUMENT:
                buffer.append("s").append(Math.abs(getTreeInfo().getDocumentNumber()));
                break;
            default:
                buffer.append("s").append(Math.abs(getTreeInfo().getDocumentNumber())).append("f").append(nodeNumber);
                break;
        }
    }

    /**
     * Get all in-scope namespaces defined on this element.
     * @return a {@code NamespaceMap} containing all the in-scope namespaces for an element (not just
     * those declared locally on the element itself). Returns null if the node is
     * not an element.
     */

    /*@Nullable*/
    public NamespaceMap getInScopeNamespaces() {
        return null;
    }

    /**
     * Ask whether this is a node in a streamed document
     *
     * @return true if the node is in a document being processed using streaming
     */
    @Override
    public boolean isStreamed() {
        return true;
    }

    /**
     * Set the system identifier for this Source.
     * <p>The system identifier is optional if the source does not
     * get its data from a URL, but it may still be useful to provide one.
     * The application can use a system identifier, for example, to resolve
     * relative URIs and to include in error messages and warnings.</p>
     *
     * @param systemId The system identifier as a URL string.
     */

    @Override
    public void setSystemId(String systemId) {
        // no action
    }

    @Override
    public NamespaceBinding[] getDeclaredNamespaces(NamespaceBinding[] buffer) {
        throw new UnsupportedOperationException();
    }

    /**
     * Get all the namespace bindings that are in-scope for this element.
     * <p>For an element return all the prefix-to-uri bindings that are in scope. This may include
     * a binding to the default namespace (represented by a prefix of ""). It will never include
     * "undeclarations" - that is, the namespace URI will never be empty; the effect of an undeclaration
     * is to remove a binding from the in-scope namespaces, not to add anything.</p>
     * <p>For a node other than an element, returns null.</p>
     *
     * @return the in-scope namespaces for an element, or null for any other kind of node.
     */
    @Override
    public NamespaceMap getAllNamespaces() {
        return null;
    }
}

// Copyright (c) 2009-2022 Saxonica Limited