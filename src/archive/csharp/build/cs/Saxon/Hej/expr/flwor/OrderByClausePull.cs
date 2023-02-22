using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class OrderByClausePull : Saxon.Hej.expr.flwor.TuplePull {
        private readonly Saxon.Hej.expr.flwor.TuplePull @base;
        private readonly Saxon.Hej.expr.flwor.OrderByClause orderByClause;
        private readonly Saxon.Hej.expr.flwor.TupleExpression tupleExpr;
        private int currentPosition = -1;
        private readonly Saxon.Hej.expr.sort.AtomicComparer[] comparers;
        private readonly System.Collections.Generic.List<Saxon.Hej.expr.sort.ItemToBeSorted> tupleArray = new System.Collections.Generic.List<Saxon.Hej.expr.sort.ItemToBeSorted>(100);
        public OrderByClausePull(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.flwor.TupleExpression tupleExpr, Saxon.Hej.expr.flwor.OrderByClause orderBy, Saxon.Hej.expr.XPathContext context)  {
            this.@base = @base;
            this.tupleExpr = tupleExpr;
            this.orderByClause = orderBy;
            Saxon.Hej.expr.sort.AtomicComparer[] suppliedComparers = orderBy.getAtomicComparers();
            comparers = new Saxon.Hej.expr.sort.AtomicComparer[suppliedComparers.Length];
            for (int n = 0; (n<comparers.Length); n++) {
                this.comparers[n] = suppliedComparers[n].provideContext(context);
            }
        }
        public override bool nextTuple(Saxon.Hej.expr.XPathContext context) {
            if ((currentPosition<0)) {
                currentPosition = 0;
                int position = 0;
                while (@base.nextTuple(context)) {
                    Saxon.Hej.expr.flwor.Tuple tuple = tupleExpr.evaluateItem(context);
                    Saxon.Hej.expr.sort.SortKeyDefinitionList sortKeyDefinitions = orderByClause.getSortKeyDefinitions();
                    Saxon.Hej.expr.sort.ItemToBeSorted itbs = new Saxon.Hej.expr.sort.ItemToBeSorted(sortKeyDefinitions.size());
                    itbs.value = tuple;
                    for (int i = 0; (i<sortKeyDefinitions.size()); i++) {
                        itbs.sortKeyValues[i] = orderByClause.evaluateSortKey(i, context);
                    }
                    itbs.originalPosition = ++position;
                    tupleArray.Add(itbs);
                }
                try {
                    tupleArray.Sort((a, b) => {
    try {
        for (int i = 0; (i<comparers.Length); i++) {
            int comp = comparers[i].compareAtomicValues(a.sortKeyValues[i], b.sortKeyValues[i]);
            if ((comp != 0)) {
                return comp;
            }
        }
    } catch (Saxon.Hej.trans.NoDynamicContextException e) {
        throw new Saxon.Ejava.lang.AssertionError(("Sorting without dynamic context: "+e.Message));
    }
    return (a.originalPosition-b.originalPosition);
} );
                } catch (System.InvalidCastException e) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Non-comparable types found while sorting: "+e.Message));
                    err.setErrorCode("XPTY0004");
                    throw err;
                }
            }
            if ((currentPosition<tupleArray.Count)) {
                tupleExpr.setCurrentTuple(context, (Saxon.Hej.expr.flwor.Tuple)(tupleArray[currentPosition++].value));
                return true;
            } else {
                return false;
            }
        }
        public override void close() {
            @base.close();
        }
    
    }
}