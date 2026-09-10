package com.saxonica.ee.update;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.MutableNodeInfo;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Type;

import java.util.Set;

/**
 * A pending update action representing the effect of a delete expression
 */
public class DeleteAction extends PendingUpdateAction {

    private final NodeInfo targetNode;

    /**
     * Create a DeleteAction
     *
     * @param node the node to be deleted
     */

    public DeleteAction(NodeInfo node) {
        targetNode = node;
        if (!(node instanceof MutableNodeInfo)) {
            throw new IllegalArgumentException("Attempting to delete a non-updateable node");
        }
    }

    /**
     * Apply the pending update action to the affected node
     *
     * @param context         the XPath evaluation context
     * @param affectedRootSet the set of roots of trees that have been modified, which
     *                        this action should add to as necessary
     * @throws net.sf.saxon.trans.XPathException
     *          if any error occurs applying the update
     */

    @Override
    public void apply(XPathContext context, Set<MutableNodeInfo> affectedRootSet) throws XPathException {

        if (!((MutableNodeInfo) targetNode).isDeleted()) {
            // This check means we do nothing if the node has already been deleted or detached from its tree
            int kind = targetNode.getNodeKind();
            MutableNodeInfo parent = (MutableNodeInfo) targetNode.getParent();
            affectedRootSet.add((MutableNodeInfo)targetNode.getRoot());
            ((MutableNodeInfo) targetNode).delete();
            if (kind == Type.ELEMENT || kind == Type.ATTRIBUTE || kind == Type.TEXT) {
                parent.removeTypeAnnotation();
            }
        }
    }

    /**
     * Get the target node of the update action
     *
     * @return the target node, the node to which this update action applies.
     */

    @Override
    public NodeInfo getTargetNode() {
        return targetNode;
    }


    /**
     * Get the phase of operation in which this pending update action is applied. See 3.2.2 upd:applyUpdates,
     * Semantics, rule 2.
     *
     * @return the phase of operation, an integer in the range 1 to 5 representing the stages labelled a-e
     *         in the specification
     */

    @Override
    public int getApplyPhase() {
        return PHASE;
    }

    public final static int PHASE = 5;
}

// Copyright (c) 2008-2023 Saxonica Limited

