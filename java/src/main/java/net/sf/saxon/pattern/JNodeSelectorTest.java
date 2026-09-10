////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.pattern;

import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.ma.jnode.JNode;
import net.sf.saxon.om.GNode;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.pattern.nodetest.NodeTest;
import net.sf.saxon.pattern.qname.QNameTest;
import net.sf.saxon.type.Type;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.type.UType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.z.IntSet;

import java.util.Optional;
import java.util.Set;

/**
 * A JNodeSelectorTest matches JNodes whose selector property is one of a defined set of atomic
 * values.
 */

public class JNodeSelectorTest extends NodeTest {

    private final Set<AtomicMatchKey> keys;

    public JNodeSelectorTest(Set<AtomicMatchKey> keys) {
        this.keys = keys;
    }

    /**
     * Get the node kind that this name test matches
     *
     * @return the matching node kind
     */

    public int getNodeKind() {
        return Type.JNODE;
    }

    /**
     * Get the corresponding {@link UType}. A UType is a union of primitive item
     * types.
     *
     * @return the smallest UType that subsumes this item type
     */
    @Override
    public UType getUType() {
        return UType.JNODE;
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
//     * @param annotation The actual content type of the node
//     */
//    @Override
//    public boolean matches(int nodeKind, NodeName name, SchemaType annotation) {
//        return false;
//    }
//
//    @Override
//    public IntPredicateProxy getMatcher(final NodeVectorTree tree) {
//        return null;
//    }

    /**
     * Test whether this node test is satisfied by a given node. This alternative
     * method is used in the case of nodes where calculating the fingerprint is expensive,
     * for example DOM or JDOM nodes.
     *
     * @param gnode the node to be matched
     */

    @Override
    public boolean test(GNode gnode) {

        if (gnode instanceof JNode) {
            AtomicValue actual = ((JNode)gnode).getSelector();
            if (actual == null) {
                return false;
            }
            AtomicMatchKey actualKey = actual.asMapKey();
            return keys.contains(actualKey);
        }

        return false;
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
        return null;
    }


    /**
     * Get the fingerprint required
     */

    @Override
    public int getFingerprint() {
        return -1;
    }

    /**
     * Get the name of the nodes matched by this nodetest, if it matches a specific name.
     * Return null if the node test matches nodes of more than one name
     */
    @Override
    public StructuredQName getMatchingNodeName() {
        return null;
    }

    /**
     * Determine the types of nodes to which this pattern applies. Used for optimisation.
     * For patterns that match nodes of several types, return Type.NODE
     *
     * @return the type of node matched by this pattern. e.g. Type.ELEMENT or Type.TEXT
     */

    //@Override
    public int getPrimitiveType() {
        return Type.JNODE;
    }

    /**
     * Get the set of node names allowed by this NodeTest. This is returned as a set of Integer fingerprints.
     * A null value indicates that all names are permitted (i.e. that there are no constraints on the node name.
     * The default implementation returns null.
     */

    /*@NotNull*/
    @Override
    public Optional<IntSet> getRequiredNodeNames() {
        return null;
    }

    public String toString() {
        return "get(" + keys + ")";
    }

    /**
     * Returns a hash code value for the object.
     */

    public int hashCode() {
        return keys.hashCode();
    }

    /**
     * Determines whether two NameTests are equal
     */

    public boolean equals(Object other) {
        return other instanceof JNodeSelectorTest &&
                ((JNodeSelectorTest) other).keys.equals(keys);
    }
//
//    @Override
//    public String getFullAlphaCode() {
//        return getBasicAlphaCode() + " n" + getMatchingNodeName().getEQName();
//    }

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
        return toString();
    }

}

