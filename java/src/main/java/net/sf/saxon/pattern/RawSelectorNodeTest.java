////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.pattern;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.ma.jnode.JNode;
import net.sf.saxon.ma.jnode.JNodeType;
import net.sf.saxon.om.GNode;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.AtomicValue;

import java.util.HashSet;
import java.util.Set;

/**
 * A RawSelectorNodeTest represents a nodetest written using the syntax get(Expression)
 *
 * <p>TEMP: only a single literal is supported</p>
 */

public class RawSelectorNodeTest extends RawNodeTest {

    private final AtomicMatchKey key;
    private final int defaultNodeKind;

    public RawSelectorNodeTest(Expression expression, int defaultNodeKind) {
        this.defaultNodeKind = defaultNodeKind;
        if (!(expression instanceof Literal)) {
            // Temporary restriction
            throw new UnsupportedOperationException();
        }
        AtomicValue value = (AtomicValue) ((Literal)expression).getGroundedValue();
        key = value.asMapKey();
    }

    /**
     * Convert this RawNodeTest to an executable node test using type information
     * about the origin node of the axis step.
     *
     * @param origin the type of the origin node
     * @return a refined NodeTest.
     */
    @Override
    public NodeTest cook(ItemType origin) {
        if (origin instanceof JNodeType) {
            Set<AtomicMatchKey> keys = new HashSet<>();
            keys.add(key);
            return new JNodeSelectorTest(keys);
        }
        if (origin instanceof XNodeTest) {
            throw new UnsupportedOperationException();
        }
        return this;
    }

    /**
     * Convert this RawNodeTest to an executable node test knowing the
     * actual origin node. This method can take advantage of the actual
     * tree model used, e.g. if the origin is a node in a TinyTree.
     *
     * @param origin the origin node
     * @return a refined NodeTest.
     */
    @Override
    public NodePredicate cook(GNode origin) {
        if (origin instanceof JNode) {
            return node -> ((JNode)origin).getSelector().asMapKey().equals(key) ;
        }
        if (origin instanceof NodeInfo) {
            switch (defaultNodeKind) {
                case Type.ATTRIBUTE:
                    return NodeKindTest.ATTRIBUTE;
                case Type.NAMESPACE:
                    return NodeKindTest.NAMESPACE;
                default:
                    return NodeKindTest.ELEMENT;
            }
        }
        return this;
    }

    @Override
    public String toString() {
        return "get(" + key + ")";
    }
}

