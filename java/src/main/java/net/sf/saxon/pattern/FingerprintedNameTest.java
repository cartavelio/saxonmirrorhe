////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.pattern;

import net.sf.saxon.om.GNode;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.pattern.qname.AnyQNameTest;
import net.sf.saxon.pattern.qname.QNameTest;
import net.sf.saxon.tree.tiny.NodeVectorTree;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.Type;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.type.UType;
import net.sf.saxon.z.IntPredicateLambda;
import net.sf.saxon.z.IntPredicateProxy;
import net.sf.saxon.z.IntSet;
import net.sf.saxon.z.IntSingletonSet;

import java.util.Optional;

/**
 * A FingerprintedNameTest matches nodes by node kind and fingerprint. It is used
 * only when searching a tree whose nodes are fingerprinted, such as a TinyTree.
 */

public class FingerprintedNameTest extends NodeTest {

    private final int nodeKind;
    private final int fingerprint;
    private final UType uType;
//    private final NamePool namePool;
//    /*@Nullable*/ private NamespaceUri uri = null;  // the URI corresponding to the fingerprint - computed lazily
//    /*@Nullable*/ private String localName = null; //the local name corresponding to the fingerprint - computed lazily

    /**
     * Create a NameTest to match nodes by fingerprint
     *
     * @param nodeKind  the kind of node, for example {@link Type#ELEMENT}
     * @param fingerprint  the fingerprint of the required nodes.
     * @since 13.0
     */

    public FingerprintedNameTest(int nodeKind, int fingerprint) {
        this.nodeKind = nodeKind;
        this.fingerprint = fingerprint;
        this.uType = UType.fromTypeCode(nodeKind);
    }

    /**
     * Get the node kind that this name test matches
     *
     * @return the matching node kind
     */

    public int getNodeKind() {
        return nodeKind;
    }

    /**
     * Get the corresponding {@link UType}. A UType is a union of primitive item
     * types.
     *
     * @return the smallest UType that subsumes this item type
     */
    @Override
    public UType getUType() {
        return uType;
    }

    /**
     * Test whether this node test is satisfied by a given node. This method is only
     * fully supported for a subset of NodeTests, because it doesn't provide all the information
     * needed to evaluate all node tests. In particular (a) it can't be used to evaluate a node
     * test of the form element(N,T) or schema-element(E) where it is necessary to know whether the
     * node is nilled, and (b) it can't be used to evaluate a node test of the form
     * document-node(element(X)). This in practice means that it is used (a) to evaluate the
     * simple node tests found in the XPath 1.0 subset used in XML Schema, and (b) to evaluate
     * node tests where the node kind is known to be an attribute.
     *
     * @param nodeKind   The kind of node to be matched
     * @param name       identifies the expanded name of the node to be matched.
     *                   The value should be null for a node with no name.
     * @param annotation The actual content type of the node
     */
    @Override
    public boolean matches(int nodeKind, NodeName name, SchemaType annotation) {
        if (nodeKind != this.nodeKind) {
            return false;
        }
        if (name.hasFingerprint()) {
            return name.getFingerprint() == this.fingerprint;
        } else {
            throw new UnsupportedOperationException();
        }
    }

    @Override
    public IntPredicateProxy getMatcher(final NodeVectorTree tree) {
        final byte[] nodeKindArray = tree.getNodeKindArray();
        final int[] nameCodeArray = tree.getNameCodeArray();
        return IntPredicateLambda.of(nodeNr -> (nameCodeArray[nodeNr] & 0xfffff) == fingerprint &&
                (nodeKindArray[nodeNr] & 0x0f) == nodeKind);
    }

    /**
     * Test whether this node test is satisfied by a given node. This alternative
     * method is used in the case of nodes where calculating the fingerprint is expensive,
     * for example DOM or JDOM nodes.
     *
     * @param node the node to be matched
     */

    @Override
    public boolean test(GNode node) {
        return ((NodeInfo)node).getFingerprint() == fingerprint;
    }

    /**
     * Determine the default priority of this node test when used on its own as a Pattern
     */

    @Override
    public final double getDefaultPriority() {
        return 0.0;
    }

    /**
     * Extract a QNameTest (the strongest one possible) that must be satisfied by a node
     * if it is to satisfy this NodeTest
     *
     * @return the strongest possible QNameTest
     */

    public QNameTest getQNameTest() {
        return AnyQNameTest.getInstance();
    }


    /**
     * Get the fingerprint required
     */

    @Override
    public int getFingerprint() {
        return fingerprint;
    }

    /**
     * Determine the types of nodes to which this pattern applies. Used for optimisation.
     * For patterns that match nodes of several types, return Type.NODE
     *
     * @return the type of node matched by this pattern. e.g. Type.ELEMENT or Type.TEXT
     */

    @Override
    public int getPrimitiveType() {
        return nodeKind;
    }

    /**
     * Get the set of node names allowed by this NodeTest. This is returned as a set of Integer fingerprints.
     * A null value indicates that all names are permitted (i.e. that there are no constraints on the node name.
     * The default implementation returns null.
     */

    /*@NotNull*/
    @Override
    public Optional<IntSet> getRequiredNodeNames() {
        return Optional.of(new IntSingletonSet(fingerprint));
    }


    public String toString() {
        switch (nodeKind) {
            case Type.ELEMENT:
                return "element(" + fingerprint + ")";
            case Type.ATTRIBUTE:
                return "attribute(" + fingerprint + ")";
            case Type.PROCESSING_INSTRUCTION:
                return "processing-instruction(" + fingerprint + ')';
            case Type.NAMESPACE:
                return "namespace-node(" + fingerprint + ')';
            default:
                return "node(" + fingerprint + ')';
        }
    }

    /**
     * Returns a hash code value for the object.
     */

    public int hashCode() {
        return nodeKind << 20 ^ fingerprint;
    }

    /**
     * Determines whether two NameTests are equal
     */

    public boolean equals(Object other) {
        return other instanceof FingerprintedNameTest &&
                ((FingerprintedNameTest) other).nodeKind == nodeKind &&
                ((FingerprintedNameTest) other).fingerprint == fingerprint;
    }

    @Override
    public String getFullAlphaCode() {
        return getBasicAlphaCode() + " n" + getMatchingNodeName().getEQName();
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
    @Override
    public Optional<String> explainMismatch(Item item, TypeHierarchy th) {
        Optional<String> explanation = super.explainMismatch(item, th);
        if (explanation.isPresent()) {
            return explanation;
        }
        return Optional.of("The node has the wrong name");
    }

}

