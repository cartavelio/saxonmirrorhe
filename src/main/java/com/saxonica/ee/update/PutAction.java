package com.saxonica.ee.update;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.MutableNodeInfo;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.query.QueryResult;
import net.sf.saxon.trans.XPathException;

import javax.xml.transform.Result;
import javax.xml.transform.stream.StreamResult;
import java.util.Properties;
import java.util.Set;

/**
 * A saved put() call on the pending update list.
 */
public class PutAction extends PendingUpdateAction {

    private final NodeInfo node;
    private final String outputUri;


    public PutAction(NodeInfo node, String outputUri) {
        this.node = node;
        this.outputUri = outputUri;
    }

    /**
     * Apply the pending update action to the affected nodes
     *
     * @param context         the XPath evaluation context
     * @param affectedRootSet the set of root nodes of trees that have been modified, which
     *                        this action should add to as necessary
     * @throws net.sf.saxon.trans.XPathException
     *          if any error occurs applying the update
     */

    @Override
    public void apply(XPathContext context, Set<MutableNodeInfo> affectedRootSet) throws XPathException {
        Result result = new StreamResult(outputUri);
        Properties props = context.getController().getExecutable().getPrimarySerializationProperties().getProperties();
        QueryResult.serialize(node, result, props);
    }

    /**
     * Get the target node of the update action
     *
     * @return the target node, the node to which this update action applies. Returns null in the
     *         case of a delete action, which affects multiple nodes.
     */

    @Override
    public NodeInfo getTargetNode() {
        return node;
    }

    /**
     * Get the phase of operation in which this pending update action is applied. See 3.2.2 upd:applyUpdates,
     * Semantics, rule 2.
     *
     * @return the phase of operation, an integer in the range 1 to 5 representing the stages labelled a-e
     *         in the specification, or 6 for a PutAction.
     */

    @Override
    public int getApplyPhase() {
        return PHASE;
    }

    public final static int PHASE = 6;
}

// Copyright (c) 2008-2022 Saxonica Limited
