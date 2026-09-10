////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.pattern.nodetest;

import net.sf.saxon.om.GNode;
import net.sf.saxon.om.Item;
import net.sf.saxon.pattern.qname.NoQNameTest;
import net.sf.saxon.pattern.qname.QNameTest;
import net.sf.saxon.type.ErrorType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.type.UType;
import net.sf.saxon.type.gnode.GNodeType;

import java.util.Optional;

/**
 * A {@code NodeRejector} is a {@code NodeTest} that unconditionally fails to match anything.
 */

public class NodeRejector extends NodeTest {

    public static NodeRejector INSTANCE = new NodeRejector();

    public static NodeRejector getInstance() {
        return INSTANCE;
    }

    private NodeRejector() {}

    /**
     * Determine the default priority to use if this node-test appears as a match pattern
     * for a template with no explicit priority attribute.
     *
     * @return the default priority for the pattern
     */

    public double getDefaultPriority() {
        return 0;
    }

    /**
     * Extract a QNameTest (the strongest one possible) that must be satisfied by a node
     * if it is to satisfy this NodeTest
     * @return the strongest possible QNameTest
     */

    public QNameTest getQNameTest() {
        return NoQNameTest.getInstance();
    }

    public GNodeType getItemType() {
        return ErrorType.getInstance();
    }

    /**
     * Get a UType that all matching nodes must satisfy
     * @return a UType
     */

    public UType getUType() {
        return UType.VOID;
    }

    /**
     * Test whether this node test is satisfied by a given node. This alternative
     * method is used in the case of nodes where calculating the fingerprint is expensive,
     * for example DOM or JDOM nodes.
     * @param node the node to be matched
     * @return true if the node test is satisfied by the supplied node, false otherwise
     */
    
    public boolean test(GNode node) {
        return false;
    }


    /**
     * Copy a NodeTest.
     * Since they are never written to except in their constructors, returns the same.
     *
     * @return the original nodeTest
     */

    /*@NotNull*/
    public NodeRejector copy() {
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
        return Optional.of("Nothing matches this node test");
    }

}

