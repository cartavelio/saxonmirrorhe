////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr.flwor;

import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.expr.sort.SortKeyDefinitionList;
import net.sf.saxon.expr.sort.ThingToBeSorted;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.NoDynamicContextException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;

import java.util.ArrayList;

/**
 * Represents the tuple stream delivered by an "order by" clause. This sorts the tuple stream supplied
 * as its input, and outputs the same tuples but in sorted order.
 */
public class OrderByClausePush extends TuplePush {

    private final TuplePush destination;
    private final OrderByClause orderByClause;
    private final TupleExpression tupleExpr;
    private final AtomicComparer[] comparers;
    private final XPathContext context;
    private int position = 0;
    private final ArrayList<ThingToBeSorted> tupleArray = new ArrayList<>(100);

    public OrderByClausePush(Outputter outputter, TuplePush destination, TupleExpression tupleExpr, OrderByClause orderBy, XPathContext context) {
        super(outputter);
        this.destination = destination;
        this.tupleExpr = tupleExpr;
        this.orderByClause = orderBy;
        this.context = context;

        AtomicComparer[] suppliedComparers = orderBy.getAtomicComparers();
        comparers = new AtomicComparer[suppliedComparers.length];
        for (int n = 0; n < comparers.length; n++) {
            this.comparers[n] = suppliedComparers[n].provideContext(context);
        }
    }

    /**
     * Process the next tuple.
     */
    @Override
    public void processTuple(XPathContext context) throws XPathException {
        FlworTuple<Sequence> tuple = tupleExpr.evaluateItem(context);
        SortKeyDefinitionList sortKeyDefinitions = orderByClause.getSortKeyDefinitions();
        ThingToBeSorted itbs = new ThingToBeSorted(tuple, new GroundedValue[sortKeyDefinitions.size()]);
        for (int i = 0; i < sortKeyDefinitions.size(); i++) {
            itbs.sortkeys()[i] = orderByClause.evaluateSortKey(i, context);
        }
        tupleArray.add(itbs);

    }

    /**
     * Close the tuple stream, indicating that no more tuples will be delivered
     */
    @Override
    public void close() throws XPathException {
        try {
            tupleArray.sort((ThingToBeSorted a, ThingToBeSorted b) -> {
                try {
                    for (int i = 0; i < comparers.length; i++) {
                        int comp = comparers[i].compareAtomicValues(
                                (AtomicValue)a.sortkeys()[i], (AtomicValue)b.sortkeys()[i]);
                        if (comp != 0) {
                            // we have found a difference, so we can return
                            return comp;
                        }
                    }
                } catch (NoDynamicContextException e) {
                    throw new AssertionError("Sorting without dynamic context: " + e.getMessage());
                }
                return 0;
            });
        } catch (ClassCastException e) {
            throw new XPathException("Non-comparable types found while sorting: " + e.getMessage(), "XPTY0004");
        }

        for (ThingToBeSorted itbs : tupleArray) {
            tupleExpr.setCurrentTuple(context, (FlworTuple<Sequence>) itbs.thing());
            destination.processTuple(context);
        }
        destination.close();
    }
}

