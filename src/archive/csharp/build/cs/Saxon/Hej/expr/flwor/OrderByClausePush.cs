using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class OrderByClausePush : Saxon.Hej.expr.flwor.TuplePush {
        private readonly Saxon.Hej.expr.flwor.TuplePush destination;
        private readonly Saxon.Hej.expr.flwor.OrderByClause orderByClause;
        private readonly Saxon.Hej.expr.flwor.TupleExpression tupleExpr;
        private readonly Saxon.Hej.expr.sort.AtomicComparer[] comparers;
        private readonly Saxon.Hej.expr.XPathContext context;
        private int position = 0;
        private readonly System.Collections.Generic.List<Saxon.Hej.expr.sort.ItemToBeSorted> tupleArray = new System.Collections.Generic.List<Saxon.Hej.expr.sort.ItemToBeSorted>(100);
        public OrderByClausePush(Saxon.Hej.@event.Outputter outputter, Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.expr.flwor.TupleExpression tupleExpr, Saxon.Hej.expr.flwor.OrderByClause orderBy, Saxon.Hej.expr.XPathContext context) : base (outputter) {
            this.destination = destination;
            this.tupleExpr = tupleExpr;
            this.orderByClause = orderBy;
            this.context = context;
            Saxon.Hej.expr.sort.AtomicComparer[] suppliedComparers = orderBy.getAtomicComparers();
            comparers = new Saxon.Hej.expr.sort.AtomicComparer[suppliedComparers.Length];
            for (int n = 0; (n<comparers.Length); n++) {
                this.comparers[n] = suppliedComparers[n].provideContext(context);
            }
        }
        public override void processTuple(Saxon.Hej.expr.XPathContext context) {
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
        public override void close() {
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
            foreach (Saxon.Hej.expr.sort.ItemToBeSorted itbs in tupleArray) {
                tupleExpr.setCurrentTuple(context, (Saxon.Hej.expr.flwor.Tuple)(itbs.value));
                destination.processTuple(context);
            }
            destination.close();
        }
    
    }
}