////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.pattern;

import net.sf.saxon.om.GNode;
import net.sf.saxon.om.Item;
import net.sf.saxon.pattern.nodetest.NodePredicate;
import net.sf.saxon.pattern.nodetest.NodeTest;
import net.sf.saxon.pattern.qname.AnyQNameTest;
import net.sf.saxon.pattern.qname.QNameTest;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.type.UType;
import net.sf.saxon.type.gnode.AnyGNodeType;

import java.util.Optional;

/**
 * A NodeTest that wraps a general Predicate applied to nodes.
 *
 * <p>This is needed for C#, where it is not possible for a method
 * to accept either a NodeTest object or a Predicate supplied as a lambda expression. To supply a predicate, it is
 * therefore necessary to wrap it in a {@code NodeSelector}.</p>
 */

public class NodePredicateTest implements NodeTest {

    private final NodePredicate predicate;

    private NodePredicateTest(NodePredicate predicate) {
        this.predicate = predicate;
    }

    /**
     * Create a NodeTest based on a supplied predicate. The NodeTest selects a node if the
     * predicate returns true.
     * @param predicate the supplied predicate (a boolean function of a node)
     * @return a NodeTest matching the selected nodes
     */
    public static NodeTest of(NodePredicate predicate) {
        return new NodePredicateTest(predicate);
    }

    @Override
    public boolean test(GNode node) {
        return predicate.test(node);
    }

    /**
     * Determine the default priority to use if this node-test appears as a match pattern
     * for a template with no explicit priority attribute.
     *
     * @return the default priority for the pattern
     */
    @Override
    public double getDefaultPriority() {
        return -1;
    }

    /**
     * Extract a QNameTest (the strongest one possible) that must be satisfied by a node
     * if it is to satisfy this NodeTest
     *
     * @return the strongest possible QNameTest
     */
    @Override
    public QNameTest getQNameTest() {
        return AnyQNameTest.getInstance();
    }

    /**
     * Get an item type that all matching nodes must satisfy
     *
     * @return an item type
     */
    @Override
    public ItemType getItemType() {
        return AnyGNodeType.getInstance();
    }

    /**
     * Get a UType that all matching nodes must satisfy
     *
     * @return a UType
     */
    @Override
    public UType getUType() {
        return UType.GNODE;
    }

    /**
     * Determine whether the content type (if present) is nillable
     *
     * @return true if the content test (when present) can match nodes that are nilled
     */
    @Override
    public boolean isNillable() {
        return true;
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
        return Optional.empty();
    }

    /**
     * Get a concise string representation of this node test for use in diagnostics
     *
     * @return a suitably abbreviated respresention of the node test
     */
    @Override
    public String toShortString() {
        return toString();
    }
}

