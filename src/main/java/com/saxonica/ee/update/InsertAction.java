package com.saxonica.ee.update;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.om.MutableNodeInfo;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Type;

import java.util.Set;

/**
 * A pending update action representing the effect of an insert expression
 */
public class InsertAction extends PendingUpdateAction {

    private final NodeInfo[] content;
    private final MutableNodeInfo targetNode;
    private final int position;
    private final boolean inheritNamespaces;

    /**
     * Create an InsertAction
     *
     * @param content    the content sequence to be inserted (a value which must consist entirely of nodes. The
     *                   caller is responsible for copying the nodes into the same object model representation as the tree into
     *                   which they will be inserted; also for merging adjacent text nodes, extracting the children of a document
     *                   node, and so on.)
     * @param targetNode the node that defines where the new nodes will be inserted
     * @param position   defines where the nodes will be inserted: before or after the target node, or as the first
     *                   or last child of the target node.
     */

    public InsertAction(NodeInfo[] content, MutableNodeInfo targetNode, int position, boolean inherit) {
        this.content = content;
        this.targetNode = targetNode;
        this.position = position;
        this.inheritNamespaces = inherit;
    }

    /**
     * Apply the pending update action to the affected nodes
     *  @param context         the XPath evaluation context
     * @param affectedRootSet the set of roots of trees that have been modified, which
     */

    @Override
    public void apply(XPathContext context, Set<MutableNodeInfo> affectedRootSet) throws XPathException {
        if (!targetNode.getTreeInfo().isMutable()) {
            throw new XPathException("Attempting to update a non-updatable element", SaxonErrorCode.SXUP0081);
        }
        boolean affectsType = false;
        for (NodeInfo node : content) {
            int kind = node.getNodeKind();
            if (kind == Type.ELEMENT || kind == Type.TEXT) {
                affectsType = true;
                break;
            }
        }
        switch (position) {
            case Token.FIRST_INTO:
                targetNode.insertChildren(content, true, inheritNamespaces);
                if (affectsType) {
                    targetNode.removeTypeAnnotation();
                }
                break;
            case Token.LAST_INTO:
            case Token.INTO:
                targetNode.insertChildren(content, false, inheritNamespaces);
                if (affectsType) {
                    targetNode.removeTypeAnnotation();
                }
                break;
            case Token.BEFORE:
                targetNode.insertSiblings(content, true, inheritNamespaces);
                if (affectsType) {
                    ((MutableNodeInfo) targetNode.getParent()).removeTypeAnnotation();
                }
                break;
            case Token.AFTER:
                targetNode.insertSiblings(content, false, inheritNamespaces);
                if (affectsType) {
                    ((MutableNodeInfo) targetNode.getParent()).removeTypeAnnotation();
                }
                break;
            default:
                throw new UnsupportedOperationException("Unknown insert position " + position);
        }
        affectedRootSet.add((MutableNodeInfo)targetNode.getRoot());
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
        return (position == Token.INTO ? 1 : 2);
    }
}

// Copyright (c) 2008-2022 Saxonica Limited
