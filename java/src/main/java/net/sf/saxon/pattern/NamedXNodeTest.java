////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.pattern;

import net.sf.saxon.om.*;
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
 * A NamedXNodeTest is a predicate that matches an XNode node with a specific node kind
 * and a specific QName.
 */

public class NamedXNodeTest extends NodeTest {

    private final int nodeKind;
    private final int fingerprint;
    private final UType uType;
    private final NamespaceUri uri;
    private final String localName;

    /**
     * Create a NamedXNodeTest to match nodes by name
     *
     * @param nodeKind  the kind of node, for example {@link Type#ELEMENT}
     * @param qName     the QName of the required node. The fingerprint mut be present.
     * @since 13.0
     */

    public NamedXNodeTest(int nodeKind, FingerprintedQName qName) {
        this.uri = qName.getNamespaceUri();
        this.localName = qName.getLocalPart();
        this.nodeKind = nodeKind;
        this.fingerprint = qName.getFingerprint();
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
        return nodeKind == this.nodeKind
                && name.hasURI(uri)
                && name.getLocalPart().equals(localName);
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
     * @param gnode the node to be matched
     */

    @Override
    public boolean test(GNode gnode) {

        if (gnode.getNodeKind() != nodeKind) {
            return false;
        }

        NodeInfo node = (NodeInfo) gnode;

        // Two different algorithms are used for name matching. If the fingerprint of the node is readily
        // available, we use it to do an integer comparison. Otherwise, we do string comparisons on the URI
        // and local name. In practice, Saxon's native node implementations use fingerprint matching, while
        // DOM and JDOM nodes use string comparison of names

        if (node.hasFingerprint()) {
            return node.getFingerprint() == fingerprint;
        } else {
            return localName.equals(node.getLocalPart()) && uri.equals(node.getNamespaceUri());
        }
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
        // TODO: when is this used?
        throw new UnsupportedOperationException();
        //return new SpecificQNameTest(new StructuredQName("", uri, localName), namePool, fingerprint);
    }


    /**
     * Get the fingerprint required
     */

    @Override
    public int getFingerprint() {
        return fingerprint;
    }

    /**
     * Get the name of the nodes matched by this nodetest, if it matches a specific name.
     * Return null if the node test matches nodes of more than one name
     */
    @Override
    public StructuredQName getMatchingNodeName() {
        return new StructuredQName("", uri, localName);
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

    /**
     * Get the namespace URI matched by this nametest
     *
     * @return the namespace URI (using "" for the "null namepace")
     */

    public NamespaceUri getNamespaceURI() {
        return uri;
    }

    /**
     * Get the local name matched by this nametest
     *
     * @return the local name
     */

    public String getLocalPart() {
        return localName;
    }

    public String toString() {
        switch (nodeKind) {
            case Type.ELEMENT:
                return "element(" + getMatchingNodeName().getEQName() + ")";
            case Type.ATTRIBUTE:
                return "attribute(" + getMatchingNodeName().getEQName() + ")";
            case Type.PROCESSING_INSTRUCTION:
                return "processing-instruction(" + localName + ')';
            case Type.NAMESPACE:
                return "namespace-node(" + localName + ')';
        }
        return getMatchingNodeName().getEQName();
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
        return other instanceof NamedXNodeTest &&
                ((NamedXNodeTest) other).nodeKind == nodeKind &&
                ((NamedXNodeTest) other).fingerprint == fingerprint;
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

    @Override
    public String toShortString() {
        switch (nodeKind) {
            case Type.ELEMENT:
                return getNamespaceURI().isEmpty() ? localName : toString();
            case Type.ATTRIBUTE:
                return "@" + (getNamespaceURI().isEmpty() ? localName : toString());
            default:
                return toString();
        }
    }

}

