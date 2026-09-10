package com.saxonica.ee.stream.adjunct;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ForEachGroup;
import net.sf.saxon.trans.XPathException;


/**
 * This feed supports xsl:for-each-group in streaming mode. It is supplied with all the nodes selected by the
 * xsl:for-each-group instruction (in composed form); it decides whether a node is the first in a new group,
 * and if so, triggers the start/end-of-group actions. It also feeds every node into a push pipeline that
 * reflects the processing of the "current-group" variable.
 * <p>The items received by this Feed will generally be supplied by a CopyOfWatch. There is scope for future
 * optimization to prevent the selected nodes being unnecessarily composed in memory.</p>
 * <p>There are thus two destinations into which this class feeds. The first, identified by the usual result
 * variable, receives every item in the group, and is notified of the start and end of a group by calling
 * its open() and close() methods. This destination processes the instructions within the for-each-group
 * instruction before and after the reference to the grouping variable. The second destination, identified
 * by the variable outerFeed, has its open() method called at the beginning of processing the population,
 * and its close() method called at the end of processing the population; its append() method is called
 * with the items that result from evaluation of the for-each-group body.</p>
 */


public abstract class GroupBoundaryTester {

    // localContext contains the iterator over the nodes selected by the for-each-group instruction,
    // and is the context in which the inner inverted expression is evaluated

    private final XPathContext localContext;
    private ForEachGroup controllingExpression;

    public GroupBoundaryTester(ForEachGroup expression, XPathContext context) {
        this.controllingExpression = expression;
        this.localContext = context;
    }

    protected XPathContext getLocalContext() {
        return localContext;
    }

    public void setControllingExpression(ForEachGroup controllingExpression) {
        this.controllingExpression = controllingExpression;
    }

    public ForEachGroup getControllingExpression() {
        return controllingExpression;
    }

    /**
     * Signal that a new item that matches the selection
     * has been found. The new item will be the context item
     * (and its position will be the context position) in the current
     * iterator of the supplied context object
     * @return true if this item is the start of a new group
     * @throws net.sf.saxon.trans.XPathException
     *          May be raised if a dynamic error occurs
     */
    public abstract boolean notifyItem() throws XPathException;


}

// Copyright (c) 2018-2022 Saxonica Limited