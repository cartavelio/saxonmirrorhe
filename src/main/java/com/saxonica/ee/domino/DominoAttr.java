package com.saxonica.ee.domino;

import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodePredicate;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.iter.PrependAxisIterator;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.StringValue;
import org.w3c.dom.Attr;
import org.w3c.dom.Node;

/**
 * An attribute node in the Domino tree
 */
public class DominoAttr implements NodeInfo {

    private final DominoTree tree;
    private final DominoNode parent;
    private final Attr domAttr;

    public DominoAttr(DominoTree tree, DominoNode parent, Attr domAttr) {
        this.tree = tree;
        this.parent = parent;
        this.domAttr = domAttr;
    }

    /**
     * Get the effective boolean value of this sequence
     *
     * @return the effective boolean value
     * @throws XPathException if the sequence has no effective boolean value (for example a sequence of two integers)
     */
    @Override
    public boolean effectiveBooleanValue() throws XPathException {
        return true;
    }

    /**
     * Get information about the tree to which this NodeInfo belongs
     *
     * @return the TreeInfo
     * @since 9.7
     */
    @Override
    public TreeInfo getTreeInfo() {
        return tree;
    }

    /**
     * Get the kind of node. This will be a value such as {@link Type#ELEMENT}
     * or {@link Type#ATTRIBUTE}. There are seven kinds of node: documents, elements, attributes,
     * text, comments, processing-instructions, and namespaces.
     *
     * @return an integer identifying the kind of node. These integer values are the
     * same as those used in the DOM
     * @see Type
     * @since 8.4
     */
    @Override
    public int getNodeKind() {
        return Type.ATTRIBUTE;
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
     * the same node in the tree.
     */
    @Override
    public boolean equals(Object other) {
        return other instanceof DominoAttr &&
                parent.equals (((DominoAttr)other).parent) &&
                getLocalPart().equals(((DominoAttr) other).getLocalPart()) &&
                getNamespaceUri().equals(((DominoAttr) other).getNamespaceUri());
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
     * containing the node, or null if not known or not applicable.
     * @since 8.4
     */
    @Override
    public String getSystemId() {
        return tree.getSystemId();
    }

    /**
     * Get the Base URI for the node, that is, the URI used for resolving a relative URI contained
     * in the node. This will be the same as the System ID unless xml:base has been used. Where the
     * node does not have a base URI of its own, the base URI of its parent node is returned.
     *
     * @return the base URI of the node. This may be null if the base URI is unknown, including the case
     * where the node has no parent.
     * @since 8.4
     */
    @Override
    public String getBaseURI() {
        return Navigator.getBaseURI(this);
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
     * other node, or 0 if they are the same node. (In this case,
     * isSameNode() will always return true, and the two nodes will
     * produce the same result for generateId())
     * @since 8.4
     */
    @Override
    public int compareOrder(NodeInfo other) {
        Node otherDomNode;
        if (other instanceof DominoAttr) {
            otherDomNode = ((DominoAttr)other).domAttr;
        } else if (other instanceof DominoNode) {
            otherDomNode = ((DominoNode)other).getDomNode();
        } else {
            // it must be a namespace node
            return -other.compareOrder(this);
        }
        // Use the DOM Level-3 compareDocumentPosition() method
        if (equals(other)) {
            return 0;
        }
        synchronized(domAttr) {
            short relationship = domAttr.compareDocumentPosition(otherDomNode);
            if ((relationship &
                    (Node.DOCUMENT_POSITION_PRECEDING | Node.DOCUMENT_POSITION_CONTAINS)) != 0) {
                return +1;
            } else if ((relationship &
                    (Node.DOCUMENT_POSITION_FOLLOWING | Node.DOCUMENT_POSITION_CONTAINED_BY)) != 0) {
                return -1;
            }
        }
        throw new AssertionError();
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
        return false;
    }

    /**
     * Get fingerprint. The fingerprint is a coded form of the expanded name
     * of the node: two nodes
     * with the same name code have the same namespace URI and the same local name.
     * The fingerprint contains no information about the namespace prefix. For a name
     * in the null namespace, the fingerprint is the same as the name code.
     *
     * @return an integer fingerprint; two nodes with the same fingerprint have
     * the same expanded QName. For unnamed nodes (text nodes, comments, document nodes,
     * and namespace nodes for the default namespace), returns -1.
     * @throws UnsupportedOperationException if this kind of node does not hold
     *                                       namepool fingerprints (specifically, if {@link #hasFingerprint()} returns false).
     * @since 8.4 (moved into FingerprintedNode at 9.7; then back into NodeInfo at 9.8).
     */
    @Override
    public int getFingerprint() {
        return -1;
    }

    /**
     * Get the local part of the name of this node. This is the name after the ":" if any.
     *
     * @return the local part of the name. For an unnamed node, returns "". Unlike the DOM
     * interface, this returns the full name in the case of a non-namespaced name.
     * @since 8.4
     */
    @Override
    public String getLocalPart() {
        return domAttr.getLocalName();
    }

    /**
     * Get the URI part of the name of this node. This is the URI corresponding to the
     * prefix, or the URI of the default namespace if appropriate.
     *
     * @return The URI of the namespace of this node. For an unnamed node,
     * or for an element or attribute that is not in a namespace, or for a processing
     * instruction, returns an empty string.
     * @since 8.4
     */
    @Override
    public NamespaceUri getNamespaceUri() {
        return NamespaceUri.of(domAttr.getNamespaceURI());
    }

    /**
     * Get the display name of this node, in the form of a lexical QName.
     * For elements and attributes this is [prefix:]localname.
     * For unnamed nodes, it is an empty string.
     *
     * @return The display name of this node. For a node with no name, returns
     * an empty string.
     * @since 8.4
     */
    @Override
    public String getDisplayName() {
        return domAttr.getNodeName();
    }

    /**
     * Get the prefix of the name of the node. This is defined only for elements and attributes.
     * If the node has no prefix, or for other kinds of node, returns a zero-length string.
     *
     * @return The prefix of the name of the node.
     * @since 8.4
     */
    @Override
    public String getPrefix() {
        String name = domAttr.getNodeName();
        int colon = name.indexOf(':');
        if (colon < 0) {
            return "";
        } else {
            return name.substring(0, colon);
        }
    }

    /**
     * Get the type annotation of this node, if any. The type annotation is represented as
     * SchemaType object.
     * <p>Types derived from a DTD are not reflected in the result of this method.</p>
     *
     * @return For element and attribute nodes: the type annotation derived from schema
     * validation (defaulting to xs:untyped and xs:untypedAtomic in the absence of schema
     * validation). For comments, text nodes, processing instructions, and namespaces: null.
     * For document nodes, either xs:untyped if the document has not been validated, or
     * xs:anyType if it has.
     * @since 9.4
     */
    @Override
    public SchemaType getSchemaType() {
        return BuiltInAtomicType.UNTYPED_ATOMIC;
    }

    /**
     * Get the NodeInfo object representing the parent of this node
     *
     * @return the parent of this node; null if this node has no parent
     * @since 8.4
     */
    @Override
    public NodeInfo getParent() {
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
     * turn.  The nodes are returned in axis order (document order for a forwards
     * axis, reverse document order for a reverse axis).
     * @throws UnsupportedOperationException if the namespace axis is
     *                                       requested and this axis is not supported for this implementation.
     * @see AxisInfo
     * @since 8.4
     */
    @Override
    public AxisIterator iterateAxis(int axisNumber, NodePredicate nodeTest) {
        switch (axisNumber) {
            case AxisInfo.ANCESTOR:
                return parent.iterateAxis(AxisInfo.ANCESTOR_OR_SELF, nodeTest);

            case AxisInfo.ANCESTOR_OR_SELF:
                if (nodeTest.test(this)) {
                    return new PrependAxisIterator(this, parent.iterateAxis(AxisInfo.ANCESTOR_OR_SELF, nodeTest));
                } else {
                    return parent.iterateAxis(AxisInfo.ANCESTOR_OR_SELF, nodeTest);
                }

            case AxisInfo.ATTRIBUTE:
            case AxisInfo.CHILD:
            case AxisInfo.DESCENDANT:
            case AxisInfo.DESCENDANT_OR_SELF:
            case AxisInfo.FOLLOWING_SIBLING:
            case AxisInfo.NAMESPACE:
            case AxisInfo.PRECEDING_SIBLING:
                return EmptyIterator.ofNodes();

            case AxisInfo.FOLLOWING:
                return new Navigator.AxisFilter(
                        new Navigator.FollowingEnumeration(this),
                        nodeTest);

            case AxisInfo.PARENT:
                return Navigator.filteredSingleton(parent, nodeTest);

            case AxisInfo.PRECEDING:
                return new Navigator.AxisFilter(
                        new Navigator.PrecedingEnumeration(this, false),
                        nodeTest);

            case AxisInfo.SELF:
                return Navigator.filteredSingleton(this, nodeTest);

            case AxisInfo.PRECEDING_OR_ANCESTOR:
                return new Navigator.AxisFilter(
                        new Navigator.PrecedingEnumeration(this, true),
                        nodeTest);
            default:
                throw new IllegalArgumentException("Unknown axis number " + axisNumber);
        }
    }

    /**
     * Get the string value of a given attribute of this node
     *
     * @param uri   the namespace URI of the attribute name. Supply the empty string for an attribute
     *              that is in no namespace
     * @param local the local part of the attribute name.
     * @return the attribute value if it exists, or null if it does not exist. Always returns null
     * if this node is not an element.
     * @since 9.4
     */
    @Override
    public String getAttributeValue(NamespaceUri uri, String local) {
        return null;
    }

    /**
     * Get the root node of the tree containing this node
     *
     * @return the NodeInfo representing the top-level ancestor of this node.
     * This will not necessarily be a document node. If this node has no parent,
     * then the method returns this node.
     * @since 8.4
     */
    @Override
    public NodeInfo getRoot() {
        return getParent().getRoot();
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
     */
    @Override
    public void generateId(StringBuilder buffer) {
        getParent().generateId(buffer);
        buffer.append("a");
        buffer.append(Integer.toString(tree.getNamePool().allocateFingerprint(getNamespaceUri(), getLocalPart())));
    }

    /**
     * Get all namespace declarations and undeclarations defined on this element.
     * <p>This method is intended primarily for internal use. User applications needing
     * information about the namespace context of a node should use <code>iterateAxis(Axis.NAMESPACE)</code>.
     * (However, not all implementations support the namespace axis, whereas all implementations are
     * required to support this method.)</p>
     *
     * @param buffer If this is non-null, and the result array fits in this buffer, then the result
     *               may overwrite the contents of this array, to avoid the cost of allocating a new array on the heap.
     * @return An array of integers representing the namespace declarations and undeclarations present on
     * this element. For a node other than an element, return null. Otherwise, the returned array is a
     * sequence of namespace codes, whose meaning may be interpreted by reference to the name pool. The
     * top half word of each namespace code represents the prefix, the bottom half represents the URI.
     * If the bottom half is zero, then this is a namespace undeclaration rather than a declaration.
     * The XML namespace is never included in the list. If the supplied array is larger than required,
     * then the first unused entry will be set to -1.
     * <p>For a node other than an element, the method returns null.</p>
     */
    @Override
    public NamespaceBinding[] getDeclaredNamespaces(NamespaceBinding[] buffer) {
        return null;
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

    /**
     * Get the value of the item as a string. For nodes, this is the string value of the
     * node as defined in the XPath 2.0 data model, except that all nodes are treated as being
     * untyped: it is not an error to get the string value of a node with a complex type.
     * For atomic values, the method returns the result of casting the atomic value to a string.
     *
     * @return the string value of the item
     * @throws UnsupportedOperationException if the item is a function item (an unchecked exception
     *                                       is used here to avoid introducing exception handling to a large number of paths where it is not
     *                                       needed)
     * @since 8.4
     */
    @Override
    public UnicodeString getUnicodeStringValue() {
        return StringView.of(domAttr.getValue()).tidy();
    }


    /**
     * Atomize the item.
     *
     * @return the result of atomization
     * @throws XPathException if atomization is not allowed
     *                        for this kind of item
     */
    @Override
    public AtomicSequence atomize() throws XPathException {
        return StringValue.makeUntypedAtomic(getUnicodeStringValue());
    }

    /**
     * Get an immutable copy of this Location object. By default Location objects may be mutable, so they
     * should not be saved for later use. The result of this operation holds the same location information,
     * but in an immutable form.
     */
    @Override
    public Location saveLocation() {
        return null;
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

    }
}

// Copyright (c) 2017-2022 Saxonica Limited


