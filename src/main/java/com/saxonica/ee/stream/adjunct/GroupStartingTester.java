package com.saxonica.ee.stream.adjunct;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ForEachGroup;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;


/**
 * This class implements GroupingFeed when the group-starting-with option is used.
 * It contains the logic to decide whether an item starts a new group
 */


public class GroupStartingTester extends GroupBoundaryTester {

    private int pos = 0;
    private final Pattern pattern;

    public GroupStartingTester(ForEachGroup expression, XPathContext context) {
        super(expression, context);
        pattern = (Pattern) expression.getGroupingKey();
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
        return pos++ == 0 || pattern.matches(getLocalContext().getContextItem(), getLocalContext());
    }

}

// Copyright (c) 2018-2022 Saxonica Limited