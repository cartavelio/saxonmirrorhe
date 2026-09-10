package com.saxonica.ee.update;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.MutableNodeInfo;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.type.Type;

import java.util.Set;

/**
 * A pending update action representing the effect of a rename expression
 */
public class RenameAction extends PendingUpdateAction {

    private NodeInfo node;
    private NodeName newName;

    /**
     * Create a RenameAction
     *
     * @param node     the node to be renamed
     * @param newName the new name
     */

    public RenameAction(NodeInfo node, NodeName newName) {
        this.node = node;
        this.newName = fixNodeName(newName);
    }

    /**
     * Apply the pending update action to the affected nodes
     *
     * @param context         the XPath evaluation context
     * @param affectedRootSet the set of roots of trees that have been modified, which
     *                        this action should add to as necessary
     */

    public void apply(XPathContext context, Set affectedRootSet) {
        affectedRootSet.add(node.getRoot());
        ((MutableNodeInfo) node).rename(newName);
        int kind = node.getNodeKind();
        if (kind == Type.ELEMENT || kind == Type.ATTRIBUTE) {
            ((MutableNodeInfo) node).removeTypeAnnotation();
        }
    }

    /**
     * Get the target node of the update action
     *
     * @return the target node, the node to which this update action applies. Returns null in the
     *         case of a delete action, which affects multiple nodes.
     */

    public NodeInfo getTargetNode() {
        return node;
    }

    /**
     * Get the new name as an integer namecode
     *
     * @return the new name as an integer namecode
     */

    public NodeName getNewName() {
        return newName;
    }

    /**
     * Get the phase of operation in which this pending update action is applied. See 3.2.2 upd:applyUpdates,
     * Semantics, rule 2. However, the spec does rename, and insert as a single phase; we split it so
     * that rename is phase 0, while insert is phase 1. This ensures that old attributes are renamed
     * before new attributes are inserted, avoiding spurious conflicts.
     *
     * @return the phase of operation, an integer in the range 1 to 5 representing the stages labelled a-e
     *         in the specification
     */

    public int getApplyPhase() {
        return PHASE;
    }

    public final static int PHASE = 0;
}

// Copyright (c) 2008 Saxonica Limited.

