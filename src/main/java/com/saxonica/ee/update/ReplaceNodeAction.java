package com.saxonica.ee.update;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.MutableNodeInfo;
import net.sf.saxon.om.NodeInfo;

import java.util.Set;

/**
 * A pending update action representing the effect of a "replace node" expression
 */
public class ReplaceNodeAction extends PendingUpdateAction {

    private final NodeInfo[] content;
    private final MutableNodeInfo targetNode;
    private final boolean inherit;

    /**
     * Create a ReplaceNodeAction
     *
     * @param content    the content sequence to be inserted (a value which must consist entirely of nodes. The
     *                   caller is responsible for copying the nodes into the same object model representation as the tree into
     *                   which they will be inserted; also for merging adjacent text nodes, extracting the children of a document
     *                   node, and so on.)
     * @param targetNode the node that defines where the new nodes will be inserted
     */

    ReplaceNodeAction(NodeInfo[] content, MutableNodeInfo targetNode, boolean inherit) {
        this.content = content;
        this.targetNode = targetNode;
        this.inherit = inherit;
    }

    /**
     * Apply the pending update action to the affected nodes
     *  @param context         the XPath evaluation context
     * @param affectedRootSet the set of roots of trees that have been modified, which
     */

    @Override
    public void apply(XPathContext context, Set<MutableNodeInfo> affectedRootSet) {
        NodeInfo root = targetNode.getRoot();
        MutableNodeInfo parent = (MutableNodeInfo) targetNode.getParent();
        if (!parent.isDeleted() && !targetNode.isDeleted()) {
            targetNode.replace(content, inherit);
            affectedRootSet.add((MutableNodeInfo)root);
            parent.removeTypeAnnotation();
        }
    }

    /**
     * Get the target node of the update action
     *
     * @return the target node, the node to which this update action applies. Returns null in the
     *         case of a delete action, which affects multiple nodes.
     */

    @Override
    public NodeInfo getTargetNode() {
        return targetNode;
    }

    /**
     * Get the list of nodes acting as the replacement nodes
     *
     * @return the replacement nodes
     */

    public NodeInfo[] getNewContent() {
        return content;
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
        return 3;
    }
}

// Copyright (c) 2008-2022 Saxonica Limited
