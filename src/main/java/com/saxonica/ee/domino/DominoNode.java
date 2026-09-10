package com.saxonica.ee.domino;

import net.sf.saxon.dom.DOMNodeWrapper;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.pattern.NameTest;
import net.sf.saxon.pattern.NodePredicate;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.NamespaceNode;
import net.sf.saxon.tree.iter.*;
import net.sf.saxon.tree.tiny.AncestorIterator;
import net.sf.saxon.tree.tiny.TinyNodeImpl;

import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.tree.wrapper.VirtualNode;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.StringValue;
import org.w3c.dom.Attr;
import org.w3c.dom.NamedNodeMap;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import java.util.ArrayList;
import java.util.List;

/**
 * A node in the Domino tree (other than an attribute or namespace node)
 */
public class DominoNode implements NodeInfo, VirtualNode {

    private final DominoTree tree;
    private final int nodeNr;
    private DominoNode parent;

    public DominoNode(DominoTree tree, int nodeNr) {
        this.tree = tree;
        this.nodeNr = nodeNr;
    }

    public DominoNode(DominoTree tree, int nodeNr, DominoNode parent) {
        this.tree = tree;
        this.nodeNr = nodeNr;
        this.parent = parent;
    }

    public final int getNodeNr() {
        return nodeNr;
    }

    /**
     * Set the parent of this node. Providing this information is useful,
     * if it is known, because otherwise getParent() has to search backwards
     * through the document.
     *
     * @param parent the parent of this node
     */

    protected void setParentNode(DominoNode parent) {
        this.parent = parent;
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

    public Node getDomNode() {
        return tree.getDomNode(nodeNr);
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
        if (other instanceof DominoNode) {
            int c = nodeNr - ((DominoNode)other).nodeNr;
            return Integer.compare(c, 0);
        } else if (other instanceof DominoAttr) {
            return -other.compareOrder(this);
        } else if (other instanceof NamespaceNode) {
            return -other.compareOrder(this);
        } else {
            throw new AssertionError(other.getClass());
        }
    }

    /**
     * Test if this node is an ancestor-or-self of another
     *
     * @param d the putative descendant-or-self node
     * @return true if this node is an ancestor-or-self of d
     */

    public boolean isAncestorOrSelf(/*@NotNull*/ NodeInfo d) {
        // If d is an attribute, then either "this" must be the same attribute, or "this" must
        // be an ancestor-or-self of the parent of d.
        if (d instanceof DominoAttr) {
            return isAncestorOrSelf(d.getParent());
        }

        if (d instanceof DominoNode) {
            // If it's a different tree, return false
            if (tree != ((DominoNode)d).tree) {
                return false;
            }
            int dn = ((DominoNode)d).nodeNr;


            // From now on, we know that both "this" and "dn" are nodes in the primary array

            // If this node is later in document order, return false
            if (nodeNr > dn) {
                return false;
            }

            // If it's the same node, return true
            if (nodeNr == dn) {
                return true;
            }

            // We've dealt with the "self" case: to be an ancestor, it must be an element or document node
            if (!this.hasChildNodes()) {
                return false;
            }

            // If this node is deeper than the target node then it can't be an ancestor
            if (tree.depth[nodeNr] >= tree.depth[dn]) {
                return false;
            }

            // The following code will exit as soon as we find an ancestor that has a following-sibling:
            // when that happens, we know it's an ancestor iff its following-sibling is beyond the node we're
            // looking for. If the ancestor has no following sibling, we go up a level.

            // The algorithm depends on the following assertion: if A is before D in document order, then
            // either A is an ancestor of D, or some ancestor-or-self of A has a following-sibling that
            // is before-or-equal to D in document order.

            int n = nodeNr;
            while (true) {
                int nextSib = tree.next[n];
                if (nextSib < 0 || nextSib > dn) {
                    return true;
                } else if (tree.depth[nextSib] == 0) {
                    return true;
                } else if (nextSib < n) {
                    n = nextSib;
                    // continue
                } else {
                    return false;
                }
            }
        }
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
        buffer.append("d");
        buffer.append(Long.toString(tree.getDocumentNumber()));
        buffer.append(TinyNodeImpl.NODE_LETTER[getNodeKind()]);
        buffer.append(Integer.toString(nodeNr));
    }

    /**
     * Get the string value of a given attribute of this node
     *
     * @param uri   the namespace URI of the attribute name. Supply the empty string for an attribute
     *              that is in no namespace
     * @param local the local part of the attribute name.
     * @return the attribute value if it exists, or null if it does not exist. Always returns null
     * if this node is not an element.
     */
    @Override
    public String getAttributeValue(String uri, String local) {
        if (hasAttributes()) {
            uri = uri.isEmpty() ? null : uri;
            Node att = getDomNode().getAttributes().getNamedItemNS(uri, local);
            return att==null ? null : att.getNodeValue();
        } else {
            return null;
        }
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
        return tree.getNamespaceMap(nodeNr);
    }

    /**
     * Get the display name of this node, in the form of a lexical QName.
     * For elements and attributes this is [prefix:]localname.
     * For unnamed nodes, it is an empty string.
     *
     * @return The display name of this node. For a node with no name, returns
     * an empty string.
     */
    @Override
    public String getDisplayName() {
        if (getNodeKind() == Type.ELEMENT) {
            return getDomNode().getNodeName();
        } else if (getNodeKind() == Type.PROCESSING_INSTRUCTION) {
                return getLocalPart();
        } else {
            return "";
        }
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
        return tree.getFingerprint(nodeNr);
    }

    /**
     * Get line number. Line numbers are not maintained by default, except for
     * stylesheets and schema documents. Line numbering can be requested using the
     * -l option on the command line, or by setting options on the TransformerFactory
     * or the Configuration before the source document is built.
     * <p>The granularity of line numbering is normally the element level: for other nodes
     * such as text nodes and attributes, the line number of the parent element will normally be returned.</p>
     * <p>In the case of a tree constructed by taking input from a SAX parser, the line number will reflect the
     * SAX rules: that is, the line number of an element is the line number where the start tag ends. This
     * may be a little confusing where elements have many attributes spread over multiple lines, or where
     * single attributes (as can easily happen with XSLT 2.0 stylesheets) occupy several lines.</p>
     * <p>In the case of a tree constructed by a stylesheet or query, the line number may reflect the line in
     * the stylesheet or query that caused the node to be constructed.</p>
     * <p>The line number can be read from within an XPath expression using the Saxon extension function
     * saxon:line-number()</p>
     *
     * @return the line number of the node in its original source document; or
     * -1 if not available
     * @since 8.4
     */
    @Override
    public int getLineNumber() {
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
        try {
            int f = getFingerprint();
            return f == -1 ? "" : tree.getNamePool().getLocalName(f);
        } catch (NullPointerException e) {
            e.printStackTrace();
            throw e;
        }
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
        return tree.getNodeKind(nodeNr);
    }

    /**
     * Get the NodeInfo object representing the parent of this node
     *
     * @return the parent of this node; null if this node has no parent
     * @since 8.4
     */
    @Override
    public DominoNode getParent() {
        if (parent != null) {
            return parent;
        }
        int p = tree.getParentNodeNr(nodeNr);
        if (p == -1) {
            parent = null;
        } else {
            parent = tree.getNode(p);
        }
        return parent;
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
        if (getNodeKind() == Type.ELEMENT) {
            // Using the DOM method getPrefix() is not safe
            String name = getDomNode().getNodeName();
            int colon = name.indexOf(':');
            if (colon < 0) {
                return "";
            } else {
                return name.substring(0, colon);
            }
        } else {
            return "";
        }
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
    public DominoNode getRoot() {
        return tree.getRootNode();
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
     */
    @Override
    public String getSystemId() {
        return tree.getSystemId();
    }

    /**
     * Get information about the tree to which this NodeInfo belongs
     *
     * @return the TreeInfo
     */
    @Override
    public TreeInfo getTreeInfo() {
        return tree;
    }

    /**
     * Get the URI part of the name of this node. This is the URI corresponding to the
     * prefix, or the URI of the default namespace if appropriate.
     *
     * @return The URI of the namespace of this node. For an unnamed node,
     * or for an element or attribute that is not in a namespace, or for a processing
     * instruction, returns an empty string.
     */
    @Override
    public String getURI() {
        int fp = getFingerprint();
        return fp == -1 ? "" : tree.getNamePool().getURI(fp);
    }

    /**
     * Determine whether the node has any children.
     * <p>Note: the result is equivalent to
     * <code>iterateAxis(Axis.CHILD).next() != null</code></p>
     *
     * @return True if the node has one or more children
     */
    @Override
    public boolean hasChildNodes() {
        int kind = getNodeKind();
        return (kind == Type.ELEMENT || kind == Type.DOCUMENT) && tree.depth[nodeNr+1] > tree.depth[nodeNr];
    }

    /**
     * Ask whether the node has any attributes
     * @return true if this is an element node and it has one or more attributes (not counting namespace declarations)
     */

    public boolean hasAttributes() {
        return getNodeKind() == Type.ELEMENT && ((tree.nameCode[nodeNr] & DominoTree.HAS_ATTRIBUTES) != 0);
    }

    /**
     * Ask whether the node has any locally-declared namespaces
     *
     * @return true if this is an element node and it has one or more locally-declared namespaces
     */

    public boolean hasNamespaces() {
        return getNodeKind() == Type.ELEMENT && ((tree.nameCode[nodeNr] & DominoTree.HAS_NAMESPACES) != 0);
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
     */
    @Override
    public boolean hasFingerprint() {
        return true;
    }

    /**
     * Determine whether this is the same node as another node.
     * <p>Note that two different NodeInfo instances can represent the same conceptual node.
     * Therefore the "==" operator should not be used to test node identity. The equals()
     * method should give the same result as isSameNodeInfo(), but since this rule was introduced
     * late it might not apply to all implementations.</p>
     * <p> Note: a.equals(b) if and only if generateId(a)==generateId(b).</p>
     * <p>This method has the same semantics as isSameNode() in DOM Level 3, but
     * works on Saxon NodeInfo objects rather than DOM Node objects.</p>
     *
     * @param other the node to be compared with this node
     * @return true if this NodeInfo object and the supplied NodeInfo object represent
     * the same node in the tree.
     */
    @Override
    public boolean equals(Object other) {
        return other instanceof DominoNode && ((DominoNode)other).tree == tree && ((DominoNode)other).nodeNr == nodeNr;
    }

    /**
     * Return an iteration over all the nodes reached by the given axis from this node
     * that match a given NodeTest
     *
     * @param axisNumber an integer identifying the axis; one of the constants
     *                   defined in class {@link AxisInfo}
     * @param predicate   A condition to be satisfied by the returned nodes; nodes
     *                   that do not satisfy this condition are not included in the result
     * @return an AxisIterator that delivers the nodes reached by the axis in
     * turn.  The nodes are returned in axis order (document order for a forwards
     * axis, reverse document order for a reverse axis).
     * @throws UnsupportedOperationException if the namespace axis is
     *                                       requested and this axis is not supported for this implementation.
     * @see AxisInfo
     */
    @Override
    public AxisIterator iterateAxis(int axisNumber, NodePredicate predicate) {
        if (predicate instanceof NodeTest) {
            NodeTest nodeTest = (NodeTest)predicate;
            int type = getNodeKind();
            switch (axisNumber) {
                case AxisInfo.ANCESTOR:
                    return new AncestorIterator(this, nodeTest);

                case AxisInfo.ANCESTOR_OR_SELF:
                    AxisIterator ancestors = new AncestorIterator(this, nodeTest);
                    if (nodeTest.test(this)) {
                        return new PrependAxisIterator(this, ancestors);
                    } else {
                        return ancestors;
                    }

                case AxisInfo.ATTRIBUTE:
                    if (!hasAttributes()) {
                        return EmptyIterator.ofNodes();
                    }
                    Node element = getDomNode();
                    if (nodeTest instanceof NameTest) {
                        String uri = ((NameTest) nodeTest).getNamespaceURI();
                        if (uri.isEmpty()) {
                            uri = null;
                        }
                        String local = ((NameTest) nodeTest).getLocalPart();
                        synchronized(getDomNode()) {
                            Attr attr = (Attr) element.getAttributes().getNamedItemNS(uri, local);
                            if (attr == null) {
                                return EmptyIterator.ofNodes();
                            } else {
                                DominoAttr wrappedAttr = new DominoAttr(tree, this, attr);
                                return SingleNodeIterator.makeIterator(wrappedAttr);
                            }
                        }
                    } else {
                        synchronized(element) {
                            List<NodeInfo> list = new ArrayList<>();
                            NamedNodeMap atts = element.getAttributes();
                            if (atts != null) {
                                final int attsLen = atts.getLength();
                                for (int i = 0; i < attsLen; i++) {
                                    String name = atts.item(i).getNodeName();
                                    if (!(name.startsWith("xmlns") &&
                                                  (name.length() == 5 || name.charAt(5) == ':'))) {
                                        list.add(new DominoAttr(tree, this, (Attr) atts.item(i)));
                                    }
                                }
                            }
                            return new Navigator.AxisFilter(new NodeListIterator(list), nodeTest);
                        }
                    }

                case AxisInfo.CHILD:
                    if (hasChildNodes()) {
                        return new SiblingIterator(tree, this, nodeTest, true);
                    } else {
                        return EmptyIterator.ofNodes();
                    }

                case AxisInfo.DESCENDANT:
                    if (type == Type.DOCUMENT &&
                            nodeTest instanceof NameTest &&
                            nodeTest.getPrimitiveType() == Type.ELEMENT) {
                        return tree.getAllElements(nodeTest.getFingerprint());
                    } else if (hasChildNodes()) {
                        return new DescendantIterator(tree, this, nodeTest);
                    } else {
                        return EmptyIterator.ofNodes();
                    }

                case AxisInfo.DESCENDANT_OR_SELF:
                    if (hasChildNodes()) {
                        AxisIterator descendants = new DescendantIterator(tree, this, nodeTest);
                        if (nodeTest.test(this)) {
                            return new PrependAxisIterator(this, descendants);
                        } else {
                            return descendants;
                        }
                    } else {
                        return Navigator.filteredSingleton(this, nodeTest);
                    }

                case AxisInfo.FOLLOWING:
                    if (type == Type.ATTRIBUTE || type == Type.NAMESPACE) {
                        return new FollowingIterator(tree, getParent(), nodeTest, true);
                    } else if (tree.depth[nodeNr] == 0) {
                        return EmptyIterator.ofNodes();
                    } else {
                        return new FollowingIterator(tree, this, nodeTest, false);
                    }

                case AxisInfo.FOLLOWING_SIBLING:
                    if (type == Type.ATTRIBUTE || type == Type.NAMESPACE || tree.depth[nodeNr] == 0) {
                        return EmptyIterator.ofNodes();
                    } else {
                        return new SiblingIterator(tree, this, nodeTest, false);
                    }

                case AxisInfo.NAMESPACE:
                    if (type != Type.ELEMENT) {
                        return EmptyIterator.ofNodes();
                    }
                    AxisIterator unfiltered;
                    if (!hasNamespaces()) {
                        DominoNode parent = getParent();
                        if (parent.getNodeKind() == Type.ELEMENT) {
                            unfiltered = parent.iterateAxis(axisNumber, nodeTest);
                        } else {
                            NamespaceBinding xml = NamespaceBinding.XML;
                            unfiltered = SingleNodeIterator.makeIterator(new NamespaceNode(this, xml, 0));
                        }
                    } else {
                        unfiltered = NamespaceNode.makeIterator(this, nodeTest);
                    }
                    return new Navigator.AxisFilter(unfiltered, nodeTest);

                case AxisInfo.PARENT:
                    NodeInfo parent = getParent();
                    return Navigator.filteredSingleton(parent, nodeTest);

                case AxisInfo.PRECEDING:
                    if (type == Type.ATTRIBUTE || type == Type.NAMESPACE) {
                        return new PrecedingIterator(tree, getParent(), nodeTest, false);
                    } else if (tree.depth[nodeNr] == 0) {
                        return EmptyIterator.ofNodes();
                    } else {
                        return new PrecedingIterator(tree, this, nodeTest, false);
                    }

                case AxisInfo.PRECEDING_SIBLING:
                    if (type == Type.ATTRIBUTE || type == Type.NAMESPACE || tree.depth[nodeNr] == 0) {
                        return EmptyIterator.ofNodes();
                    } else {
                        return new PrecedingSiblingIterator(tree, this, nodeTest);
                    }

                case AxisInfo.SELF:
                    return Navigator.filteredSingleton(this, nodeTest);

                case AxisInfo.PRECEDING_OR_ANCESTOR:
                    if (type == Type.DOCUMENT) {
                        return EmptyIterator.ofNodes();
                        //                } else if (type == Type.ATTRIBUTE || type == Type.NAMESPACE) {
                        //                    // See test numb32.
                        //                    TinyNodeImpl el = getParent();
                        //                    return new PrependIterator(el, new PrecedingEnumeration(tree, el, nodeTest, true));
                    } else {
                        return new PrecedingIterator(tree, this, nodeTest, true);
                    }

                default:
                    throw new IllegalArgumentException("Unknown axis number " + axisNumber);
            }
        } else {
            return new Navigator.AxisFilter(
                    iterateAxis(axisNumber, AnyNodeTest.getInstance()), predicate);
        }
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
        switch (getNodeKind()) {
            case Type.COMMENT:
            case Type.PROCESSING_INSTRUCTION:
                return new StringValue(getUnicodeStringValue());
            default:
                return StringValue.makeUntypedAtomic(getUnicodeStringValue());
        }
    }


    /**
     * Get the string value of the item as a {@link UnicodeString}. This is in some cases more efficient than
     * the version of the method that returns a String. The method satisfies the rule that
     * <code>X.getStringValueCS().toString()</code> returns a string that is equal to
     * <code>X.getStringValue()</code>.
     * <p>If the calling code can handle a UnicodeString, this method should
     * be used. If the caller requires a string, the {@link #getStringValue()} method is preferred.</p>
     *
     * @return the string value of the item
     * @throws UnsupportedOperationException if the item is a function item (an unchecked exception
     *                                       is used here to avoid introducing exception handling to a large number of paths where it is not
     *                                       needed)
     * @since 8.4
     */
    @Override
    public UnicodeString getUnicodeStringValue() {
        switch (getNodeKind()) {
            case Type.DOCUMENT:
            case Type.ELEMENT:
                if (hasChildNodes()) {
                    NodeList children1 = getDomNode().getChildNodes();
                    UnicodeBuilder sb1 = new UnicodeBuilder();
                    DOMNodeWrapper.expandStringValue(children1, sb1);
                    return sb1.toUnicodeString();
                } else {
                    return EmptyUnicodeString.getInstance();
                }

            case Type.ATTRIBUTE:
                return emptyIfNull(((Attr) getDomNode()).getValue());

            case Type.TEXT:
                UnicodeString value = emptyIfNull(getDomNode().getNodeValue());
                if ((tree.nameCode[nodeNr] & DominoTree.HAS_CONTINUATION) != 0) {
                    UnicodeBuilder buffer = new UnicodeBuilder();
                    buffer.accept(value);
                    Node textNode = getDomNode();
                    while ((textNode = textNode.getNextSibling()) != null) {
                        buffer.accept(emptyIfNull(textNode.getNodeValue()));
                    }
                    return buffer.toUnicodeString();
                } else {
                    return value;
                }

            case Type.COMMENT:
            case Type.PROCESSING_INSTRUCTION:
                return emptyIfNull(getDomNode().getNodeValue());

            default:
                return EmptyUnicodeString.getInstance();
        }
    }

    private static UnicodeString emptyIfNull(String s) {
        return s == null ? EmptyUnicodeString.getInstance() : StringView.of(s).tidy();
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

    /**
     * Get the node underlying this virtual node. If this is a VirtualNode the method
     * will automatically drill down through several layers of wrapping.
     * <p>In some cases a single VirtualNode may represent an XPath text node that maps to a sequence
     * of adjacent nodes (for example text nodes and CDATA nodes) in the underlying tree. In this case
     * the first node in this sequence is returned.</p>
     *
     * @return The underlying DOM node.
     */
    @Override
    public Node getRealNode() {
        return getDomNode();
    }

    /**
     * Get the node underlying this virtual node. Note that this may itself be
     * a VirtualNode; you may have to drill down through several layers of
     * wrapping.
     * <p>In some cases a single VirtualNode may represent an XPath text node that maps to a sequence
     * of adjacent nodes (for example text nodes and CDATA nodes) in the underlying tree. In this case
     * the first node in this sequence is returned.</p>
     *
     * @return The underlying DOM node.
     */
    @Override
    public Node getUnderlyingNode() {
        return getDomNode();
    }
}

// Copyright (c) 2017-2022 Saxonica Limited

