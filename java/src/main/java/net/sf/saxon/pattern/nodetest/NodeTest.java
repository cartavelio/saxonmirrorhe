////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.pattern.nodetest;

import net.sf.saxon.om.*;
import net.sf.saxon.pattern.qname.QNameTest;
import net.sf.saxon.trans.Err;
import net.sf.saxon.tree.tiny.NodeVectorTree;
import net.sf.saxon.type.*;
import net.sf.saxon.type.gnode.AnyGNodeType;
import net.sf.saxon.z.IntPredicateLambda;
import net.sf.saxon.z.IntPredicateProxy;
import net.sf.saxon.z.IntSet;
import net.sf.saxon.z.IntUniversalSet;

import java.util.Optional;

/**
 * A NodeTest is a simple kind of pattern that enables a context-free test of whether
 * a node matches a given node kind and name. There are several kinds of node test: a full name test, a prefix test, and an
 * "any node of a given type" test, an "any node of any type" test, a "no nodes"
 * test (used, e.g. for "@comment()").
 * <p>As well as being used to support XSLT pattern matching, NodeTests act as predicates in
 * axis steps.</p>
 * <p>Node tests used in axis step overlap with node types used in ItemType syntax. In previous
 * Saxon versions, the {@code NodeTest} class served both purposes: it represented both a node
 * test used in an axis step, and a node type used in (say) an {@code as} attribute. From Saxon 13
 * these two concept are separated. In cases where the node test in an axis step is simply
 * an item type (such as {@code node()} or {@code element(e)}, the NodeTest will typically
 * be an {@code NodeTypeTest} that references the required item type. The {@code NodeTest}
 * class itself no longer implements {@code ItemType}</p>
// * <p>For use in user-written application calling the iterateAXIS methods in {@link NodeInfo},
// * it is possible to write a user-defined subclass of <code>NodeTest</code> that implements
// * a single method, {@link #matches(int, NodeName, SchemaType)}</p>
 */

public abstract class NodeTest implements NodePredicate {

    /**
     * Determine the default priority to use if this node-test appears as a match pattern
     * for a template with no explicit priority attribute.
     *
     * @return the default priority for the pattern
     */

    public abstract double getDefaultPriority();

    /**
     * Extract a QNameTest (the strongest one possible) that must be satisfied by a node
     * if it is to satisfy this NodeTest
     * @return the strongest possible QNameTest
     */

    public abstract QNameTest getQNameTest();

    /**
     * For an {@code XNodeType} that can only match one kind of node
     * and one node name, return that node name, as an integer
     * fingerprint. In other cases, return -1.
     *
     * @param nodeKind the kind of node required. If the {@code XNodeType}
     *                 does not match this node kind, return -1.
     * @return an integer fingerprint in the case of a node type that
     * only matches one node kind and one qualified name. In other
     * cases return -1.
     */

    public int getFingerprint() {
        return -1;
    }

    /**
     * Get an item type that all matching nodes must satisfy
     * @return an item type
     */

    public ItemType getItemType() {
        return AnyGNodeType.getInstance();
    }

    /**
     * Get a UType that all matching nodes must satisfy
     * @return a UType
     */

    public UType getUType() {
        return getItemType().getUType();
    }

    /**
     * Get the name of the nodes matched by this nodetest, if it matches a specific name.
     * Return null if the node test matches nodes of more than one name
     */

    public StructuredQName getMatchingNodeName() {
        return null;
    }

    /**
     * Get a matching function that can be used to test whether numbered nodes in a TinyTree
     * or DominoTree satisfy the node test. (Calling this matcher must give the same result
     * as calling <code>matchesNode(tree.getNode(nodeNr))</code>, but it may well be faster).
     * @param tree the tree against which the returned function will operate
     * @return an IntPredicate; the matches() method of this predicate takes a node number
     * as input, and returns true if and only if the node identified by this node number
     * matches the node test.
     */

    public IntPredicateProxy getMatcher(final NodeVectorTree tree) {
        // Default implementation materialises the node
        return IntPredicateLambda.of(nodeNr -> tree.getNodeKind(nodeNr) != Type.PARENT_POINTER
                                               && test(tree.getNode(nodeNr)));
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
//
//    public abstract boolean matches(int nodeKind, NodeName name, SchemaType annotation);

    /**
     * Test whether this node test is satisfied by a given node. This alternative
     * method is used in the case of nodes where calculating the fingerprint is expensive,
     * for example DOM or JDOM nodes.
     * @param node the node to be matched
     * @return true if the node test is satisfied by the supplied node, false otherwise
     */
    
    public abstract boolean test(GNode node);

//    /**
//     * Get the content type allowed by this NodeTest (that is, the type annotation of the matched nodes).
//     * Return AnyType if there are no restrictions. The default implementation returns AnyType.
//     *
//     * @return the type annotation that all nodes matching this NodeTest must satisfy
//     */
//
//    @CSharpSuppressWarnings("UnsafeIteratorConversion")
//    public SchemaType getContentType() {
//        Set<PrimitiveUType> m = getUType().decompose();
//        Iterator<PrimitiveUType> it = m.iterator();
//        if (m.size() == 1 && it.hasNext()) {
//            PrimitiveUType p = it.next();
//            switch (p) {
//                case DOCUMENT:
//                    return AnyType.getInstance();
//                case ELEMENT:
//                    return AnyType.getInstance();
//                case ATTRIBUTE:
//                    return AnySimpleType.getInstance();
//                case COMMENT:
//                    return BuiltInAtomicType.STRING;
//                case TEXT:
//                    return BuiltInAtomicType.UNTYPED_ATOMIC;
//                case PI:
//                    return BuiltInAtomicType.STRING;
//                case NAMESPACE:
//                    return BuiltInAtomicType.STRING;
//            }
//        }
//        return AnyType.getInstance();
//    }

    /**
     * Get the set of node names allowed by this NodeTest. This is returned as a set of Integer fingerprints.
     * If all names are permitted (i.e. there are no constraints on the node name), returns IntUniversalSet.getInstance().
     * The default implementation returns the universal set.
     *
     * @return the set of integer fingerprints of the node names that this node test can match; or absent
     * if the set of names cannot be represented (for example, with the name tests *:xxx or xxx:*)
     */

    /*@NotNull*/
    public Optional<IntSet> getRequiredNodeNames() {
        return Optional.of(IntUniversalSet.getInstance());
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

    public int getRequiredFingerprint(int nodeKind) {
        return -1;
    }

    /**
     * Determine whether the content type (if present) is nillable
     *
     * @return true if the content test (when present) can match nodes that are nilled
     */

    public boolean isNillable() {
        return true;
    }

    /**
     * Copy a NodeTest.
     * Since they are never written to except in their constructors, returns the same.
     *
     * @return the original nodeTest
     */

    /*@NotNull*/
    public NodeTest copy() {
        return this;
    }


    /**
     * Get extra diagnostic information about why a supplied item does not conform to this
     * item type, if available. If extra information is returned, it should be in the form of a complete
     * sentence, minus the closing full stop. No information should be returned for obvious cases.
     *
     * @param item the item that doesn't match this type
     * @param th   the type hierarchy cache
     * @return optionally, a message explaining why the item does not match the type
     */

    public Optional<String> explainMismatch(Item item, TypeHierarchy th) {
        if (item instanceof NodeInfo) {
            UType actualKind = UType.getUType(item);
            if (!getUType().overlaps(actualKind)) {
                return Optional.of("The supplied value is " + actualKind.toStringWithIndefiniteArticle());
            }
            return Optional.empty();
        } else {
            return Optional.of("The supplied value is " + Err.describeGenre(item.getGenre()));
        }
    }

    /**
     * Get a concise string representation of this node test for use in diagnostics
     * @return a suitably abbreviated respresention of the node test
     */
    public String toShortString() {
        return toString();
    }


}

