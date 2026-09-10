package net.sf.saxon.pattern.nodetest;

import net.sf.saxon.ma.jnode.AnyJNodeType;
import net.sf.saxon.om.GNode;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.pattern.qname.AnyQNameTest;
import net.sf.saxon.pattern.qname.QNameTest;
import net.sf.saxon.pattern.qname.SpecificQNameTest;
import net.sf.saxon.tree.tiny.NodeVectorTree;
import net.sf.saxon.type.*;
import net.sf.saxon.type.gnode.*;
import net.sf.saxon.z.IntPredicateLambda;
import net.sf.saxon.z.IntPredicateProxy;

/**
 * A NodeTypeTest is a NodeTest that simply tests whether a node matches
 * a given node item type
 */

public class NodeTypeTest extends NodeTest {

    private final GNodeType nodeType;

    public static final NodeTypeTest GNODE = new NodeTypeTest(AnyGNodeType.getInstance());
    public static final NodeTypeTest JNODE = new NodeTypeTest(AnyJNodeType.getInstance());
    public static final NodeTypeTest XNODE = new NodeTypeTest(AnyXNodeType.getInstance());
    public static final NodeTypeTest ELEMENT = new NodeTypeTest(NodeKindType.ELEMENT);
    public static final NodeTypeTest ATTRIBUTE = new NodeTypeTest(NodeKindType.ATTRIBUTE);
    public static final NodeTypeTest TEXT = new NodeTypeTest(NodeKindType.TEXT);
    public static final NodeTypeTest DOCUMENT = new NodeTypeTest(NodeKindType.DOCUMENT);
    public static final NodeTypeTest COMMENT = new NodeTypeTest(NodeKindType.COMMENT);
    public static final NodeTypeTest PROCESSING_INSTRUCTION = new NodeTypeTest(NodeKindType.PROCESSING_INSTRUCTION);
    public static final NodeTypeTest NAMESPACE = new NodeTypeTest(NodeKindType.NAMESPACE);

    public NodeTypeTest(GNodeType nodeType) {
        this.nodeType = nodeType;
    }

    /**
     * Determine the default priority to use if this node-test appears as a match pattern
     * for a template with no explicit priority attribute.
     *
     * @return the default priority for the pattern
     */
    @Override
    public double getDefaultPriority() {
        return nodeType.getDefaultPriority();
    }

    /**
     * Extract a QNameTest (the strongest one possible) that must be satisfied by a node
     * if it is to satisfy this NodeTest
     *
     * @return the strongest possible QNameTest
     */
    @Override
    public QNameTest getQNameTest() {
        if (nodeType instanceof XNodeType) {
            return ((XNodeType)nodeType).getAllowedNodeNames();
        }
        return AnyQNameTest.getInstance();
    }

    /**
     * Get a matching function that can be used to test whether numbered nodes in a TinyTree
     * or DominoTree satisfy the node test. (Calling this matcher must give the same result
     * as calling <code>matchesNode(tree.getNode(nodeNr))</code>, but it may well be faster).
     *
     * @param tree the tree against which the returned function will operate
     * @return an IntPredicate; the matches() method of this predicate takes a node number
     * as input, and returns true if and only if the node identified by this node number
     * matches the node test.
     */

    public IntPredicateProxy getMatcher(final NodeVectorTree tree) {

        byte[] nodeKindArray = tree.getNodeKindArray();
        
        if (nodeType == AnyGNodeType.getInstance() || nodeType == AnyXNodeType.getInstance()) {
            return n -> nodeKindArray[n] != Type.PARENT_POINTER;
        }

        int nodeKind = nodeType.getPrimitiveType();

        if (nodeType instanceof NodeKindType) {
            if (nodeKind == Type.TEXT) {
                return IntPredicateLambda.of(nodeNr -> {
                    int k = nodeKindArray[nodeNr];
                    return k == Type.TEXT || k == Type.WHITESPACE_TEXT;
                });
            } else {
                return IntPredicateLambda.of(
                        nodeNr -> (nodeKindArray[nodeNr] & 0x0f) == nodeKind);
            }
        }

        if (nodeType instanceof NamedXNodeType) {
            QNameTest qNameTest = getQNameTest();
            if (qNameTest instanceof SpecificQNameTest) {
                int fingerprint = ((SpecificQNameTest) qNameTest).getFingerprint();
                SchemaType contentType = nodeType.getContentType();
                if (contentType == AnyType.getInstance() ||
                        (nodeKind == Type.ATTRIBUTE && contentType == BuiltInAtomicType.ANY_ATOMIC)) {
                    return IntPredicateLambda.of(
                            nodeNr -> (nodeKindArray[nodeNr] & 0x0f) == nodeKind &&
                                    tree.getFingerprint(nodeNr) == fingerprint);
                }
            }
        }
        // In all other cases, use the general-purpose implementation
        return super.getMatcher(tree);
    }

    /**
     * Get a fingerprint that can be used to match required nodes.
     *
     * @param nodeKind the kind of nodes required.
     * @return if all nodes selected by the node test are of the required nodeKind, and
     * have the same fingerprint, then return that fingerprint; otherwise return -1. Note
     * that it is acceptable for the node test to impose further constraints, for example
     * on the type annotation.
     */
    @Override
    public int getRequiredFingerprint(int nodeKind) {
        ItemType type = getItemType();
        if (type instanceof NamedXNodeType) {
            if (((NamedXNodeType)type).getNodeKind() != nodeKind) {
                return -1;
            }
            QNameTest qNameTest = ((NamedXNodeType) type).getAllowedNodeNames();
            if (qNameTest instanceof SpecificQNameTest) {
                return ((SpecificQNameTest)qNameTest).getFingerprint();
            }
            return -1;
        }
        return -1;
    }

    /**
     * Get the name of the nodes matched by this nodetest, if it matches a specific name.
     * Return null if the node test matches nodes of more than one name
     */
    @Override
    public StructuredQName getMatchingNodeName() {
        ItemType type = getItemType();
        if (type instanceof NamedXNodeType) {
            QNameTest qNameTest = ((NamedXNodeType) type).getAllowedNodeNames();
            if (qNameTest instanceof SpecificQNameTest) {
                return ((SpecificQNameTest) qNameTest).getStructuredQName();
            }
        }
        return null;
    }

    /**
     * Get an item type that all matching nodes must satisfy
     *
     * @return an item type
     */
    @Override
    public GNodeType getItemType() {
        return nodeType;
    }

    /**
     * Test whether this node test is satisfied by a given node.
     *
     * @param node the node to be matched
     * @return true if the node test is satisfied by the supplied node, false otherwise
     */
    @Override
    public boolean test(GNode node) {
        return nodeType.matches(node);
    }

//    /**
//     * Test whether this node test is satisfied by a given node. This method is only
//     * fully supported for a subset of NodeTests, because it doesn't provide all the information
//     * needed to evaluate all node tests. In particular (a) it can't be used to evaluate a node
//     * test of the form element(N,T) or schema-element(E) where it is necessary to know whether the
//     * node is nilled, and (b) it can't be used to evaluate a node test of the form
//     * document-node(element(X)). This in practice means that it is used (a) to evaluate the
//     * simple node tests found in the XPath 1.0 subset used in XML Schema, and (b) to evaluate
//     * node tests where the node kind is known to be an attribute.
//     *
//     * @param nodeKind   The kind of node to be matched
//     * @param name       identifies the expanded name of the node to be matched.
//     *                   The value should be null for a node with no name.
//     * @param annotation The actual content type of the node. Null means no constraint.
//     * @return true if the node matches this node test
//     */
//    @Override
//    public boolean matches(int nodeKind, NodeName name, SchemaType annotation) {
//        return false;
//    }


    /**
     * Returns a string representation of the object.
     */
    @Override
    public String toString() {
        return getItemType().toString();
    }

    /**
     * Get a concise string representation of this node test for use in diagnostics
     *
     * @return a suitably abbreviated respresention of the node test
     */
    @Override
    public String toShortString() {
        return getItemType().toString();
    }
}


