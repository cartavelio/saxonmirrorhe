package com.saxonica.ee.stream.adjunct;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ForEachGroup;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.expr.sort.CompositeAtomicKey;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.AtomicArray;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;

import java.util.ArrayList;
import java.util.List;


/**
 * This class implements GroupingFeed when the group-adjacent option is used.
 * It contains the logic to decide whether an item starts a new group, based
 * on computing the grouping key and comparing it with that of the previous item
 */


public class GroupAdjacentTester extends GroupBoundaryTester {

    private CompositeAtomicKey currentGroupingComparisonKey;
    private final StringCollator collator;
    private AtomicSequence currentGroupingKey;
    private final int implicitTimezone;

    public GroupAdjacentTester(ForEachGroup expression, XPathContext context) {
        super(expression, context);
        collator = expression.getCollation();
        implicitTimezone = context.getImplicitTimezone();
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
        Expression groupingKey = getControllingExpression().getGroupingKey();
        List<AtomicMatchKey> comparisonKeyList = new ArrayList<>();
        SequenceIterator iter = groupingKey.iterate(getLocalContext());
        AtomicArray key = new AtomicArray(iter);
        for (AtomicValue val : key) {
            comparisonKeyList.add(val.getXPathMatchKey(collator, implicitTimezone));
        }
        CompositeAtomicKey cak = new CompositeAtomicKey(comparisonKeyList);
        boolean isNewGroup = currentGroupingComparisonKey == null || !currentGroupingComparisonKey.equals(cak);
        currentGroupingComparisonKey = cak;
        if (isNewGroup) {
            currentGroupingKey = key;
        }
        return isNewGroup;
    }

    public AtomicSequence getCurrentGroupingKey() {
        return currentGroupingKey;
    }

}

// Copyright (c) 2018-2023 Saxonica Limited