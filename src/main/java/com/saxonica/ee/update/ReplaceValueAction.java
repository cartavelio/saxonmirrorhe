package com.saxonica.ee.update;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.MutableNodeInfo;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.Type;

import java.util.Set;

/**
 * A pending update action representing the effect of a "replace value" expression. This corresponds broadly
 * to the upd:replaceValue operation defined in the W3C specification, except that it is capable of replacing
 * the string value of an element node (that is, the upd:replaceElementContent operation) as well as a
 * text, attribute, comment, or processing-instruction node.
 */
public class ReplaceValueAction extends PendingUpdateAction {

    private final UnicodeString content;
    private final MutableNodeInfo targetNode;

    /**
     * Create an InsertAction
     *
     * @param content    the content sequence to be inserted (a value which must consist entirely of nodes. The
     *                   caller is responsible for copying the nodes into the same object model representation as the tree into
     *                   which they will be inserted; also for merging adjacent text nodes, extracting the children of a document
     *                   node, and so on.)
     * @param targetNode the node that defines where the new nodes will be inserted
     */

    ReplaceValueAction(UnicodeString content, MutableNodeInfo targetNode) {
        this.content = content;
        this.targetNode = targetNode;
    }

    /**
     * Apply the pending update action to the affected nodes
     *  @param context         the XPath evaluation context
     * @param affectedRootSet the set of roots of trees that have been modified, which
     */

    @Override
    public void apply(XPathContext context, Set<MutableNodeInfo> affectedRootSet) {
        NodeInfo root = targetNode.getRoot();
        targetNode.replaceStringValue(content);
        int kind = targetNode.getNodeKind();
        if (kind == Type.ELEMENT || kind == Type.TEXT || kind == Type.ATTRIBUTE) {
            targetNode.removeTypeAnnotation();
        }
        affectedRootSet.add((MutableNodeInfo)root);
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
     * Get the phase of operation in which this pending update action is applied. See 3.2.2 upd:applyUpdates,
     * Semantics, rule 2.
     *
     * @return the phase of operation, an integer in the range 1 to 5 representing the stages labelled a-e
     *         in the specification
     */

    @Override
    public int getApplyPhase() {
        return (targetNode.getNodeKind() == Type.ELEMENT ? 4 : 1);
    }
}

// Copyright (c) 2008-2022 Saxonica Limited
