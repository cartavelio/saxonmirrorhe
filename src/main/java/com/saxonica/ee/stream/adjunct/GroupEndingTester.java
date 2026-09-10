package com.saxonica.ee.stream.adjunct;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ForEachGroup;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;


/**
 * This class implements GroupingFeed when the group-ending-with option is used.
 * It contains the logic to decide whether an item starts a new group
 */


public class GroupEndingTester extends GroupBoundaryTester {

    private final Pattern pattern;
    private boolean foundEndingItem = true;

    public GroupEndingTester(ForEachGroup expression, XPathContext context) {
        super(expression, context);
        pattern = (Pattern)expression.getGroupingKey();
    }

    /**
     * Signal an item that matches the selection
     * has been found.
     *
     * @return true if this item is the start of a new group
     * @throws net.sf.saxon.trans.XPathException
     *          May be raised if a dynamic error occurs
     */
    @Override
    public boolean notifyItem() throws XPathException {
        boolean startNew = foundEndingItem;
        foundEndingItem = pattern.matchesItem(getLocalContext().getContextItem(), getLocalContext());
        return startNew;
    }

}

// Copyright (c) 2018-2023 Saxonica Limited