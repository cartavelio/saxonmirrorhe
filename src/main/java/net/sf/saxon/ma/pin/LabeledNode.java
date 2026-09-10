package net.sf.saxon.ma.pin;

import net.sf.saxon.ma.map.StringMap;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodePredicate;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.type.Type;

import java.util.HashMap;

/**                                                   n
 * A <code>LabeledMapItem</code> represents an XDM map together with a label, which is a map:
 * the map typically contains retained information about how the map was selected within a JSON-like tree.
 */

public class LabeledNode implements NodeInfo, LabeledItem {

    private final NodeInfo subjectNode;
    private final StringMap label;


    /**
     * Create a LabeledNode with a supplied label
     * @param subject the subject map item
     * @param label the label
     */

    public LabeledNode(NodeInfo subject, StringMap label) {
        this.subjectNode = subject;
        this.label = label;
    }


    /**
     * Get the subject item without its label
     *
     * @return the subject item
     */
    @Override
    public Item getSubject() {
        return subjectNode;
    }

    /**
     * Get the label of this value, typically, information about
     * the containing maps and arrays in the containing tree
     *
     * @return the associated {@code Pedigree}.
     */
    @Override
    public StringMap getLabel() {
        return label;
    }

    /**
     * Get the value of the item as a Unicode string. For nodes, this is the string value of the
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
        return subjectNode.getUnicodeStringValue();
    }

    /**
     * Get information about the tree to which this NodeInfo belongs
     *
     * @return the TreeInfo
     * @since 9.7
     */
    @Override
    public TreeInfo getTreeInfo() {
        return subjectNode.getTreeInfo();
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
        return subjectNode.getNodeKind();
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
        return subjectNode.getSystemId();
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
        return subjectNode.getBaseURI();
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
        return subjectNode.compareOrder(other);
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
        return subjectNode.hasFingerprint();
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
        return subjectNode.getFingerprint();
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
        return subjectNode.getLocalPart();
    }

    /**
     * Get the URI part of the name of this node. This is the URI corresponding to the
     * prefix, or the URI of the default namespace if appropriate.
     *
     * @return The URI of the namespace of this node. For an unnamed node,
     * or for an element or attribute that is not in a namespace, or for a processing
     * instruction, returns {@link NamespaceUri#NULL}.
     * @since 12.0; replaces getURI() which returned a string.
     */
    @Override
    public NamespaceUri getNamespaceUri() {
        return subjectNode.getNamespaceUri();
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
        return subjectNode.getDisplayName();
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
        return subjectNode.getPrefix();
    }

    /**
     * Get the typed value. This will either be a single AtomicValue or a value whose items are
     * atomic values.
     *
     * @return the typed value of the node
     * @throws XPathException if the node has no typed value, for example if
     *                        it is an element node with element-only content
     * @since 8.5. Changed in 9.5 to return the new type AtomicSequence.
     */
    @Override
    public AtomicSequence atomize() throws XPathException {
        return subjectNode.atomize();
    }

    /**
     * Get the NodeInfo object representing the parent of this node
     *
     * @return the parent of this node; null if this node has no parent
     * @since 8.4
     */
    @Override
    public NodeInfo getParent() {
        return subjectNode.getParent();
    }

    /**
     * Return an iteration over all the nodes reached by the given axis from this node
     * that match a given NodeTest
     *
     * @param axisNumber an integer identifying the axis; one of the constants
     *                   defined in class {@link AxisInfo}
     * @param predicate  A condition to be satisfied by the returned nodes; nodes
     *                   that do not satisfy this condition are not included in the result
     * @return an AxisIterator that delivers the nodes reached by the axis in
     * turn.  The nodes are returned in axis order (document order for a forwards
     * axis, reverse document order for a reverse axis).
     * @throws UnsupportedOperationException if the namespace axis is
     *                                       requested and this axis is not supported for this implementation.
     * @see AxisInfo
     * @since 8.4. Changed in 10.0 to accept any {@code Predicate<NodeInfo>} as the second argument. It is still
     * possible to supply a {@link NodeTest}, because {@code NodeTest} implements {@code Predicate<NodeInfo>}.
     */
    @Override
    public AxisIterator iterateAxis(int axisNumber, NodePredicate predicate) {
        return subjectNode.iterateAxis(axisNumber, predicate);
    }

    /**
     * Get the string value of a given attribute of this node
     *
     * @param uri   the namespace URI of the attribute name. Supply {@link NamespaceUri#NULL} for an attribute
     *              that is in no namespace
     * @param local the local part of the attribute name.
     * @return the attribute value if it exists, or null if it does not exist. Always returns null
     * if this node is not an element.
     * @since 12.0
     */
    @Override
    public String getAttributeValue(NamespaceUri uri, String local) {
        return subjectNode.getAttributeValue(uri, local);
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
        return subjectNode.getRoot();
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
        return subjectNode.hasChildNodes();
    }

    /**
     * Construct a character string that uniquely identifies this node.
     * Note: a.isSameNode(b) if and only if generateId(a)==generateId(b)
     *
     * @param buffer a buffer which will be updated to hold a string
     *               that uniquely identifies this node, across all documents.
     * @since 8.7
     * <p>Changed in Saxon 8.7 to generate the ID value in a client-supplied buffer</p>
     */
    @Override
    public void generateId(StringBuilder buffer) {
        subjectNode.generateId(buffer);
    }

    @Override
    public void setSystemId(String systemId) {
        subjectNode.setSystemId(systemId);
    }

    /**
     * Get all namespace declarations and undeclarations defined on this element.
     * <p>This method is intended primarily for internal use. User applications needing
     * information about the namespace context of a node should use <code>iterateAxis(Axis.NAMESPACE)</code>.
     * (However, not all implementations support the namespace axis, whereas all implementations are
     * required to support this method.)</p>
     *
     * <p>In many cases (and in particular with Saxon's TinyTree and LinkedTree implementations),
     * from 10.0 onwards it is more efficient to call {@link #getAllNamespaces()} to get all the in-scope
     * namespaces at once, rather than building up the list be calling {@code getDeclaredNamespaces} on
     * each ancestor node.</p>
     *
     * @param buffer If this is non-null, and the result array fits in this buffer, then the result
     *               may overwrite the contents of this array, to avoid the cost of allocating a new array on the heap.
     * @return An array of namespace binding objects representing the namespace declarations and undeclarations present on
     * this element. For a node other than an element, return null.
     * If the uri part is "", then this is a namespace undeclaration rather than a declaration.
     * The XML namespace is never included in the list. If the supplied array is larger than required,
     * then the first unused entry will be set to null.
     * <p>
     * For a node other than an element, the method returns null.</p>
     */
    @Override
    public NamespaceBinding[] getDeclaredNamespaces(NamespaceBinding[] buffer) {
        return subjectNode.getDeclaredNamespaces(buffer);
    }

    /**
     * Get all the namespace bindings that are in-scope for this element.
     * <p>For an element return all the prefix-to-uri bindings that are in scope. This may include
     * a binding to the default namespace (represented by a prefix of "").  The map does NOT include
     * the implicit binding of the XML namespace. It will never include
     * "undeclarations" - that is, the namespace URI will never be empty; the effect of an undeclaration
     * is to remove a binding from the in-scope namespaces, not to add anything.</p>
     * <p>For a node other than an element, returns null.</p>
     *
     * @return the in-scope namespaces for an element, or null for any other kind of node.
     */
    @Override
    public NamespaceMap getAllNamespaces() {
        return subjectNode.getAllNamespaces();
    }

    /**
     * Get an immutable copy of this Location object. By default Location objects may be mutable, so they
     * should not be saved for later use. The result of this operation holds the same location information,
     * but in an immutable form.
     *
     * @return an immutable copy (which may be the original object, if it is already immutable)
     */
    @Override
    public Location saveLocation() {
        return subjectNode.saveLocation();
    }

    /**
     * Returns a hash code value for the object. This method is
     * supported for the benefit of hash tables such as those provided by
     * {@link HashMap}.
     * <p>
     * The general contract of {@code hashCode} is:
     * <ul>
     * <li>Whenever it is invoked on the same object more than once during
     *     an execution of a Java application, the {@code hashCode} method
     *     must consistently return the same integer, provided no information
     *     used in {@code equals} comparisons on the object is modified.
     *     This integer need not remain consistent from one execution of an
     *     application to another execution of the same application.
     * <li>If two objects are equal according to the {@code equals(Object)}
     *     method, then calling the {@code hashCode} method on each of
     *     the two objects must produce the same integer result.
     * <li>It is <em>not</em> required that if two objects are unequal
     *     according to the {@link Object#equals(Object)}
     *     method, then calling the {@code hashCode} method on each of the
     *     two objects must produce distinct integer results.  However, the
     *     programmer should be aware that producing distinct integer results
     *     for unequal objects may improve the performance of hash tables.
     * </ul>
     * <p>
     * As much as is reasonably practical, the hashCode method defined
     * by class {@code Object} does return distinct integers for
     * distinct objects. (The hashCode may or may not be implemented
     * as some function of an object's memory address at some point
     * in time.)
     *
     * @return a hash code value for this object.
     * @see Object#equals(Object)
     * @see System#identityHashCode
     */
    @Override
    public int hashCode() {
        return subjectNode.hashCode();
    }

    /**
     * Indicates whether some other object is "equal to" this one.
     * <p>
     * The {@code equals} method implements an equivalence relation
     * on non-null object references:
     * <ul>
     * <li>It is <i>reflexive</i>: for any non-null reference value
     *     {@code x}, {@code x.equals(x)} should return
     *     {@code true}.
     * <li>It is <i>symmetric</i>: for any non-null reference values
     *     {@code x} and {@code y}, {@code x.equals(y)}
     *     should return {@code true} if and only if
     *     {@code y.equals(x)} returns {@code true}.
     * <li>It is <i>transitive</i>: for any non-null reference values
     *     {@code x}, {@code y}, and {@code z}, if
     *     {@code x.equals(y)} returns {@code true} and
     *     {@code y.equals(z)} returns {@code true}, then
     *     {@code x.equals(z)} should return {@code true}.
     * <li>It is <i>consistent</i>: for any non-null reference values
     *     {@code x} and {@code y}, multiple invocations of
     *     {@code x.equals(y)} consistently return {@code true}
     *     or consistently return {@code false}, provided no
     *     information used in {@code equals} comparisons on the
     *     objects is modified.
     * <li>For any non-null reference value {@code x},
     *     {@code x.equals(null)} should return {@code false}.
     * </ul>
     * <p>
     * The {@code equals} method for class {@code Object} implements
     * the most discriminating possible equivalence relation on objects;
     * that is, for any non-null reference values {@code x} and
     * {@code y}, this method returns {@code true} if and only
     * if {@code x} and {@code y} refer to the same object
     * ({@code x == y} has the value {@code true}).
     * <p>
     * Note that it is generally necessary to override the {@code hashCode}
     * method whenever this method is overridden, so as to maintain the
     * general contract for the {@code hashCode} method, which states
     * that equal objects must have equal hash codes.
     *
     * @param obj the reference object with which to compare.
     * @return {@code true} if this object is the same as the obj
     * argument; {@code false} otherwise.
     * @see #hashCode()
     * @see HashMap
     */
    @Override
    public boolean equals(Object obj) {
        return subjectNode.equals(obj);
    }
}

// Copyright (c) 2024 Saxonica Limited

