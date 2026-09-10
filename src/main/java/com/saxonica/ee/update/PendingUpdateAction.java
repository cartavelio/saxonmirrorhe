package com.saxonica.ee.update;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;

import java.util.Set;

/**
 * A pending update action, such as is found on a pending update list
 */
public abstract class PendingUpdateAction {

    Expression originator;   // Not currently used.

    /**
     * Set the originating update expression. This is used for diagnostics.
     *
     * @param originator the originating expression
     */

    public void setOriginator(Expression originator) {
        this.originator = originator;
    }

    /**
     * Get the originating update expression. This is used for diagnostics.
     *
     * @return the originating expression
     */

    public Expression getOriginator() {
        return originator;
    }

    /**
     * Apply the pending update action to the affected nodes
     *
     * @param context         the XPath evaluation context
     * @param affectedRootSet the set of root nodes of trees that have been modified, which
     *                        this action should add to as necessary
     * @throws XPathException if any error occurs applying the update
     */

    public abstract void apply(XPathContext context, Set<MutableNodeInfo> affectedRootSet) throws XPathException;

    /**
     * Get the target node of the update action
     *
     * @return the target node, the node to which this update action applies. Returns null in the
     *         case of a delete action, which affects multiple nodes.
     */

    public abstract NodeInfo getTargetNode();

    /**
     * Get the phase of operation in which this pending update action is applied. See 3.2.2 upd:applyUpdates,
     * Semantics, rule 2.
     *
     * @return the phase of operation, an integer in the range 1 to 5 representing the stages labelled a-e
     *         in the specification, or 6 for a PutAction
     */

    public abstract int getApplyPhase();

    /**
     * Fix a supplied node name for retention in the pending update list. Names held here must not
     * contain references to mutable nodes
     *
     * @param name the supplied node name
     * @return an immutable copy of the supplied node name
     */

    protected NodeName fixNodeName(NodeName name) {
        if (name instanceof NameOfNode) {
            if (name.hasFingerprint()) {
                return new FingerprintedQName(name.getPrefix(), name.getURI(), name.getLocalPart(), name.getFingerprint());
            } else {
                return new FingerprintedQName(name.getPrefix(), name.getURI(), name.getLocalPart());
            }
        } else {
            return name;
        }
    }
}

// Copyright (c) 2008-2022 Saxonica Limited
